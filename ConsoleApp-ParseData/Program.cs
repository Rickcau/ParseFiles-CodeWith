
using ConsoleApp_ParseData.Helpers;
using ConsoleApp_ParseData.Models;
using FileHelpers;
using System.Configuration;
using Microsoft.SemanticKernel;
using ConsoleApp_ParseData.Util;

Console.WriteLine("Parase Data Examples...");

#region Semantic Kernel Setup
var builder = Kernel.CreateBuilder();

var openAiDeployment = ConfigurationManager.AppSettings.Get("AzureOpenAIModel");
var openAiUri = ConfigurationManager.AppSettings.Get("AzureOpenAIEndpoint");
var openAiApiKey = ConfigurationManager.AppSettings.Get("AzureOpenAIKey");

if (openAiDeployment != null && openAiUri != null && openAiApiKey != null)
{
    builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: openAiDeployment,
    endpoint: openAiUri,
    apiKey: openAiApiKey);

}
// Not using Plugins but jkust incase we decide to: builder.Plugins.AddFromType<UniswapV3SubgraphPlugin>();

var kernel = builder.Build();

#endregion

#region Load Data Files
// Simulate the passing of the JSON request body
UploadedFilesRequest uploadedFilesRequest = new UploadedFilesRequest { ADDRESS_FILENAME= "Hospital-Shelters.20231107.csv", EPPIC_FILENAME= "EPPIC.20231107.CSV", GIACT_FILENAME= "GIACT202131107.CSV", SIEBEL_FILENAME= "Siebel.20231107.CSV" };
var _blobConnection = ConfigurationManager.AppSettings.Get("AzureOpenAIModel") ?? String.Empty;
// Load the Data 
DataHelper dataHelper = new DataHelper(uploadedFilesRequest, _blobConnection, true);
var result = await dataHelper.Intialize();
#endregion

#region Hospital Examples
// Hospital Examples
var hospitaldataRecords = dataHelper.HospitalDataRecords;
Console.WriteLine("Ready to Go, let's search for a HospitalByFullAddress. Press Enter!");
Console.ReadLine();
var recordswithFullAddress = dataHelper.HospitalShelterDataParser.FindHospitalByFullAddress("799 47dH bd", "", "SAN DIEGO", "CA", hospitaldataRecords ?? new List<HospitalShelterRecords>());
Console.WriteLine($@"Hospital Found: {recordswithFullAddress?.AddressLine1}");
Console.WriteLine("Let's print out all the Hospital Records, press  Enter!");
Console.ReadLine();
dataHelper.HospitalShelterDataParser.PrintHospitalRecords(hospitaldataRecords ?? new List<HospitalShelterRecords>());
Console.WriteLine();
#endregion

#region Eppic Examples
// Eppic Examples
var eppicdataRecords = dataHelper.EppicDataRecords;
Console.WriteLine("Ready to Go, let's search for a PersonID in the Eppic Data. Press Enter!");
Console.ReadLine();
var recordswithPersonID = dataHelper.EppicDataParser.FindEppicPersonID("5094334", eppicdataRecords ?? new List<EppicRecords>());
Console.WriteLine($@"Eppic Record Found: {recordswithPersonID?.AddressLine1}");
Console.WriteLine("Let's print out all the Eppic Records, press  Enter!");
Console.ReadLine();
dataHelper.EppicDataParser.PrintEppicRecords(eppicdataRecords ?? new List<EppicRecords>());
Console.WriteLine();
#endregion

#region Siebel Examples
// Siebel  Examples
var siebeldataRecords = dataHelper.SiebelDataRecords;
Console.WriteLine("Ready to Go, let's find Siebel CallNotes by PersonID. Press Enter!");
Console.ReadLine();
var recordswithCallNotes = dataHelper.SiebelDataParser.FindAllSiebelCallNotesByPersonID("5094334");
Console.WriteLine($@"Siebel Record Found: {recordswithCallNotes?.FirstOrDefault()?.CallNotes}");
Console.WriteLine("Let's print out all the Siebel Records. Press  Enter!");
Console.ReadLine();
dataHelper.SiebelDataParser.PrintSiebelRecords(siebeldataRecords ?? new List<SiebelRecords>());
Console.WriteLine();
#endregion

#region Giact Examples
// Giact  Examples
var giactdataRecords = dataHelper.GiactDataRecords;
Console.WriteLine("Ready to Go, let's find Giact Record by UniqueID. Press Enter!");
Console.ReadLine();
var recordswithUniqueID = dataHelper.GiactDataParser.FindGiactUniqueID("5094334", giactdataRecords ?? new List<GiactRecords>()); ;
Console.WriteLine($@"Giact Record Found: {recordswithUniqueID?.AddressLine1}");
Console.WriteLine("Let's print out all the Giact Records. Press  Enter!");
Console.ReadLine();
dataHelper.GiactDataParser.PrintGiactRecords(giactdataRecords ?? new List<GiactRecords>());
Console.WriteLine();
#endregion

#region Semantic Kernel Examples
// Semantic Kernel Examples Parsing of Call Notes with AI
Console.WriteLine("Checking for Fraud Conclusion using AI. Press Enter! ");
Console.ReadLine();
CallLogChecker callLogChecker = new CallLogChecker();
var fraudIntentResult = "";
if (siebeldataRecords != null)
{
    foreach (var recordSiebel in siebeldataRecords)
    {
        fraudIntentResult = await callLogChecker.CheckFraudIntentAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
        Console.WriteLine($@"# Fraud Conslusion Result # PersonID: {recordSiebel.PersonID}");
        Console.WriteLine($@"{fraudIntentResult}\n");
    }
}
Console.WriteLine();
Console.WriteLine("Checking all recrods for Action Conclusion using AI. Press Enter!");
Console.ReadLine();
var actionConclusionResult = "";
if (siebeldataRecords != null)
{
    foreach (var recordSiebel in siebeldataRecords)
    {
        actionConclusionResult = await callLogChecker.CheckActionConclusionAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
        Console.WriteLine($@"# Fraud Conslusion Result # PersonID: {recordSiebel.PersonID}");
        Console.WriteLine($@"{actionConclusionResult}\n");
    }
}
#endregion

