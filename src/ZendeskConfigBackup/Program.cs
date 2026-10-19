using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClosedXML.Excel;
using ZendeskConfigBackup.Helpers;
using ZendeskConfigBackup.Services;
using ZendeskConfigBackup.Services;

// --------------------------------------------------
// API Configuration
string baseUrl = "https://esfa1567428279.zendesk.com";
string clientId = "Zendesk_Workbook_Backup";
//Secret currently stored as environment variable
string? clientSecret = Environment.GetEnvironmentVariable("ZENDESK_CLIENT_SECRET");

//Check secret stored
if (string.IsNullOrWhiteSpace(clientSecret))
{
    Console.WriteLine("ZENDESK_CLIENT_SECRET has not been set.");
    return;
}

using HttpClient httpClient = new HttpClient();

var zendesk = new ZendeskClient(
    httpClient,
    baseUrl,
    clientId,
    clientSecret);

await zendesk.AuthenticateAsync();

//Spreadsheet creation using ClosedXML
using var excel = new ExcelExportService();


// --------------------------------------------------
// MACROS
var allMacros = await zendesk.GetAllAsync(
    "/api/v2/macros.json?page[size]=100",
    "macros");
//Create Macros sheet
excel.AddSheet("Macros", allMacros);


//--------------------------------------------------
//Views
var allViews = await zendesk.GetAllAsync(
    "/api/v2/views.json?page[size]=100",
    "views");
//Create Views sheet
excel.AddSheet("Views", allViews);


// --------------------------------------------------
// ROLES
Console.WriteLine("Retrieving roles...");
// Retrieve Roles from Zendesk
var allRoles = await zendesk.GetAllAsync(
    "/api/v2/custom_roles.json",
    "custom_roles");
// Create Roles worksheet
excel.AddSheet("Roles", allRoles);


// --------------------------------------------------
// GROUPS
// Retrieve all Groups from Zendesk
var allGroups = await zendesk.GetAllAsync(
    "/api/v2/groups.json?page[size]=100",
    "groups");
//Create Groups worksheet
excel.AddSheet("Groups", allGroups);


// --------------------------------------------------
// AGENTS / TEAM MEMBERS
// Retrieve all Agents and Admins from Zendesk
var allAgents = await zendesk.GetAllAsync(
    "/api/v2/users.json?role[]=agent&role[]=admin&page[size]=100",
    "users");
//Create Agents worksheet
excel.AddSheet("Agents", allAgents);


// --------------------------------------------------
// TICKET FIELDS
// Retrieve all Ticket Fields from Zendesk
var allTicketFields = await zendesk.GetAllAsync(
    "/api/v2/ticket_fields.json?page[size]=100",
    "ticket_fields");
//Create Ticket Fields worksheet
excel.AddSheet("Ticket Fields", allTicketFields);


// --------------------------------------------------
// BRANDS
// Retrieve all Brands from Zendesk
var allBrands = await zendesk.GetAllAsync(
    "/api/v2/brands.json?page[size]=100",
    "brands");
//Create Brands worksheet
excel.AddSheet("Brands", allBrands);

// --------------------------------------------------
// TICKET FORMS
// Retrieve all Ticket Forms from Zendesk
var allTicketForms = await zendesk.GetAllAsync(
    "/api/v2/ticket_forms.json?page[size]=100",
    "ticket_forms");
//Create Ticket Forms worksheet
excel.AddSheet("Ticket Forms", allTicketForms);


// --------------------------------------------------
// USER FIELDS
// Retrieve all User Fields from Zendesk
var allUserFields = await zendesk.GetAllAsync(
    "/api/v2/user_fields.json?page[size]=100",
    "user_fields");
//Create User Fields worksheet
excel.AddSheet("User Fields", allUserFields);

// --------------------------------------------------
// ORGANISATION FIELDS
// Retrieve all Organisation Fields from Zendesk
var allOrganizationFields = await zendesk.GetAllAsync(
    "/api/v2/organization_fields.json?page[size]=100",
    "organization_fields");
//Create Organisation Fields worksheet
excel.AddSheet("Organisation Fields", allOrganizationFields);

// --------------------------------------------------
// EMAIL / SUPPORT ADDRESSES
// Retrieve all support email addresses from Zendesk
var allEmailAddresses = await zendesk.GetAllAsync(
    "/api/v2/recipient_addresses.json?page[size]=100",
    "recipient_addresses");
//Create Emails worksheet
excel.AddSheet("Emails", allEmailAddresses);


// --------------------------------------------------
// TRIGGERS
// Retrieve all Triggers from Zendesk
var allTriggers = await zendesk.GetAllAsync(
    "/api/v2/triggers.json?page[size]=100",
    "triggers");
//Create Triggers worksheet
excel.AddSheet("Triggers", allTriggers);


// --------------------------------------------------
// AUTOMATIONS
// Retrieve all Automations from Zendesk
var allAutomations = await zendesk.GetAllAsync(
    "/api/v2/automations.json?page[size]=100",
    "automations");
//Create Automations worksheet
excel.AddSheet("Automations", allAutomations);


// --------------------------------------------------
// SLA POLICIES
// Retrieve all SLA policies from Zendesk
var allSlas = await zendesk.GetAllAsync(
    "/api/v2/slas/policies.json",
    "sla_policies");
//Create SLAs worksheet
excel.AddSheet("SLAs", allSlas);

// --------------------------------------------------
// SCHEDULES
// Retrieve Schedules from Zendesk
var allSchedules = await zendesk.GetAllAsync(
    "/api/v2/business_hours/schedules.json",
    "schedules");

//Create Schedules worksheet
excel.AddSheet("Schedules", allSchedules);



// --------------------------------------------------
// SAVE WORKBOOK

string outputPath = Path.Combine(
    Environment.CurrentDirectory,
    "Zendesk_Config_Backup.xlsx");

excel.Save(outputPath);

Console.WriteLine($"Backup saved to: {outputPath}");