
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

#region Semantic Kernel Example Parsing of CallNotes
// Lets get the callnotes for PersonID
Console.WriteLine("Checking for Fraud Conclusion using AI NOT using JSON mode!!");
var siebeldataParser = new SiebelDataParser();
CallLogChecker callLogChecker = new CallLogChecker();
siebeldataParser.ParseCsv("C:\\temp\\Data\\Siebel\\Siebel.20231107.CSV");
var recordswithCallNotes = siebeldataParser.FindAllSiebelCallNotesByPersonID("5094334");
var result = await callLogChecker.CheckFraudIntent2Async(kernel, "5094334", recordswithCallNotes.First().CallNotes ?? "");
Console.WriteLine(result);
Console.WriteLine("\n Let's now call CheckFraudIntenAsync which is using JSON Mode");
var result2 = await callLogChecker.CheckFraudIntentAsync(kernel, "5094334", recordswithCallNotes.First().CallNotes ?? "");
Console.WriteLine(result2);
Console.WriteLine("\n");
// Let's check the ActionConclusion for the CallNotes
Console.WriteLine("\n Let's now call CheckActionConclusionAsync which is using JSON Mode");
var result3 = await callLogChecker.CheckActionConclusionAsync(kernel, "5094334", recordswithCallNotes.First().CallNotes ?? "");
Console.WriteLine(result3);
Console.WriteLine("\n");

// siebeldataParser.PrintSiebelCallNoteRecords(recordswithCallNotes);

#endregion
#region Parse Epic File Example
// Parse Epic example

//Console.WriteLine("Let's read the Epicc File\n");

//var engineEpicc = new FileHelperEngine<EpiccRecords>();
//var recordsEpicc = engineEpicc.ReadFile("C:\\temp\\Data\\Epicc\\EPPIC.20231107.CSV");
//var count1 = 0;
//foreach (var recordEpicc in recordsEpicc)
//{
//    count1++;
//    Console.WriteLine($@"Record# {count1} UniqueID: {recordEpicc.UniqueID} AddressLine1: {recordEpicc.AddressLine1} AddressLine2: {recordEpicc.AddressLine2} City: {recordEpicc.City} State: {recordEpicc.State} ZipCode: {recordEpicc.ZipCode}");
//}
//Console.WriteLine("\n\n");
#endregion

#region Parse Siebel Example using SiebelDataParser Class
// Let's get the callnotes for PersonID 5094334
var siebeldataParser2 = new SiebelDataParser();
siebeldataParser2.ParseCsv("C:\\temp\\Data\\Siebel\\Siebel.20231107.CSV");
var recordswithCallNotes2 = siebeldataParser.FindAllSiebelCallNotesByPersonID("5094334");
Console.WriteLine(result);
#endregion


//Console.WriteLine("Let's read the Siebel File\n");

//var engineSiebel = new FileHelperEngine<SiebelRecords>();
//var recordsSiebel = engineSiebel.ReadFile("C:\\temp\\Data\\Siebel\\NOV7_activity_v2.csv");
//var count2 = 0;
//foreach (var recordSiebel in recordsSiebel)
//{
//    count2++;
//    Console.WriteLine($@"Record# {count2} ProgramName: {recordSiebel.ProgramName} PersonID: {recordSiebel.PersonID} ContactFirstName: {recordSiebel.ContactFirstName} ContactLastName: {recordSiebel.ContactLastName} ActivityCreatedDate: {recordSiebel.ActivityCreatedDate} ActivityType: {recordSiebel.ActivityType} ActivityCreatedBy: {recordSiebel.ActivityCreatedBy} ActivityDescription: {recordSiebel.ActivityDescription}");
//    if (recordSiebel.ActivityDescription != "")
//    {
//        Console.WriteLine("\n\n\n");
//        Console.WriteLine("Activity Description has data!!!");
//        Console.WriteLine("***************************");
//        Console.WriteLine(recordSiebel.ActivityDescription);
//        Console.WriteLine("***************************\n\n");
//    }
//}

#region Parse GIACT file example
//Console.WriteLine("\n\n");
//Console.WriteLine("Let's read the Giact File\n");

//var engineGiact = new FileHelperEngine<GiactRecords>();
//var recordsGiact = engineGiact.ReadFile("C:\\temp\\Data\\Giact\\GIACT202131107.CSV");

//foreach (var recordGiact in recordsGiact)
//{
//    Console.WriteLine($@"UniqueID: { recordGiact.UniqueID} City: {recordGiact.City} State: {recordGiact.City} ZipCode: {recordGiact.ZipCode} AddressLine1: {recordGiact.AddressLine1}");
//}
#endregion
