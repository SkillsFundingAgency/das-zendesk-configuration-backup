
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ZendeskConfigBackup.Services;
/// <summary>
/// Responsible for communication with the Zendesk API, including authentication and data retrieval.
/// </summary>
public class ZendeskClient
{
    // HttpClient instance for making HTTP requests to the Zendesk API
    private readonly HttpClient _client;
    // Base URL for the Zendesk API, e.g., "https://yourdomain.zendesk.com"
    private readonly string _baseUrl;
    // Client ID for OAuth authentication
    private readonly string _clientId;
    // Client Secret for OAuth authentication
    private readonly string _clientSecret;

    
    public ZendeskClient(
        HttpClient client,
        string baseUrl,
        string clientId,
        string clientSecret)
    {
        _client = client;
        _baseUrl = baseUrl.TrimEnd('/');
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    // Authenticate with Zendesk
    // Performs OAuth authentication using client credentials and sets the access token in the HttpClient's authorization header.
    public async Task AuthenticateAsync()
    {
        Console.WriteLine("Authenticating with Zendesk...");
        // Prepare the token request payload creating an anonymous type
        var tokenRequest = new
        {
            grant_type = "client_credentials",
            client_id = _clientId,
            client_secret = _clientSecret,
            scope = "read"
        };
        // Send the token request to the Zendesk OAuth endpoint
        using HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"{_baseUrl}/oauth/tokens",
                tokenRequest);
        //Check for successful response and throw an exception if not successful
        response.EnsureSuccessStatusCode();
        // Parse string into structured JSON document
        using JsonDocument document =
            JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());
        // Extract the access token from the JSON response
        string? accessToken =
            document.RootElement
                .GetProperty("access_token")
                .GetString();
        // Throw an exception if the access token is null or empty
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new Exception(
                "Zendesk did not return an access token.");
        }
        // Set the access token in the HttpClient's to include authorization header for subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
        // Clear any existing Accept headers
        _client.DefaultRequestHeaders.Accept.Clear();

        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        Console.WriteLine("Authenticated successfully.");
    }

    // Retrieve every page of an API collection
    // This method handles pagination for API endpoints that return data in multiple pages. It retrieves all records from the specified endpoint and collection name, returning a list of JSON elements.
    public async Task<List<JsonElement>> GetAllAsync(string endpoint, string collectionName)
    {
        //Creates empty list to store retrieved records
        var records = new List<JsonElement>();
        // Constructs the initial URL for the API request
        string? nextUrl = $"{_baseUrl}{endpoint}";

        int pageNumber = 1;
        // Loop to retrieve all pages of the collection until nextUrl is null or empty, indicating no more pages to retrieve
        while (!string.IsNullOrWhiteSpace(nextUrl))
        {
            // Send a GET request to the current nextUrl
            using HttpResponseMessage response =
                await _client.GetAsync(nextUrl);

            response.EnsureSuccessStatusCode();
            // Parse the response content into a JSON
            using JsonDocument document =
                JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());
            // Extract the root element of the JSON
            JsonElement root = document.RootElement;
            //Extracts the correct collection of items from the JSON response based on the provided collection name e.g., "macros", "views", etc.
            JsonElement items =
                root.GetProperty(collectionName);
            // Loop through and add each item in the collection to the records list
            foreach (JsonElement item in items.EnumerateArray())
            {
                records.Add(item.Clone());
            }
            // Log the number of records retrieved for the current page
            Console.WriteLine(
                $"{collectionName} page {pageNumber}: " +
                $"{items.GetArrayLength()} records");

            // Cursor pagination
            // This block checks for cursor-based pagination by looking for a "meta" property with a "has_more" field. If more pages are available, it retrieves the "next" link from the "links" property to continue fetching data.
            if (root.TryGetProperty("meta", out JsonElement meta) &&
                meta.TryGetProperty("has_more", out JsonElement hasMore) &&
                hasMore.ValueKind == JsonValueKind.True)
            {
                // If there are more pages, retrieve the next URL from the "links" property
                if (!root.TryGetProperty("links", out JsonElement links) ||
                    !links.TryGetProperty("next", out JsonElement next) ||
                    next.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(next.GetString()))
                {
                    throw new Exception(
                        $"Pagination error for {collectionName}.");
                }

                nextUrl = next.GetString();
            }
            // Offset pagination, e.g. SLA policies
            // Checks for offset-based pagination by looking for a "next_page" property. If it exists and is a string, it sets the nextUrl to that value; otherwise, it sets nextUrl to null, indicating no more pages.
            else if (root.TryGetProperty(
                "next_page", out JsonElement nextPage))
            {
                // If next_page is present, set nextUrl to its value; otherwise, set it to null
                nextUrl = nextPage.ValueKind == JsonValueKind.String
                    ? nextPage.GetString()
                    : null;
            }
            else
            {
                nextUrl = null;
            }

            pageNumber++;
        }

        Console.WriteLine($"Total {collectionName} retrieved: {records.Count}");
        // Return the complete list of records retrieved from all pages
        return records;
    }
}
