
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
var _blobConnection = ConfigurationManager.AppSettings.Get("AzureOpenAIModel");
BlobHelper blobHelper = new BlobHelper();
blobHelper = new BlobHelper()
{
    Container = "eppic",
    ConnectionString = _blobConnection
};
var siebeldataParser = new SiebelDataParser();
// Use below to load from file local disk
// Load Siebel data
using (StreamReader stream = File.OpenText(@"C:\temp\Data\Siebel\Siebel.20231107.CSV"))
{
    siebeldataParser.LoadData(stream);
}
// Load GIACT data
var giactdataParser = new GiactDataParser();
using (StreamReader stream = File.OpenText(@"C:\temp\Data\Giact\GIACT202131107.CSV"))
{
    giactdataParser.LoadData(stream);
}
// Load EPPIC data
var eppicdataParser = new EppicDataParser();
using (StreamReader stream = File.OpenText(@"C:\temp\Data\Eppic\EPPIC.20231107.CSV"))
{
    eppicdataParser.LoadData(stream);
}
// Load Hospital data
var hospitaldataParser = new HospitalShelterDataParser();
using (StreamReader stream = File.OpenText(@"C:\temp\Data\Hospitals\Hospital-Shelters.20231107.csv"))
{
    hospitaldataParser.LoadData(stream);
}

// Use below to load from Azure Blob Storage note how this uses the Blob Helper
//using (StreamReader stream = await blobHelper.GetStreamReaderFromBlob("scrubbedSampleSiebel.csv"))
//{
//    parser.LoadData(stream);
//}

//var output = parser.ParseCsv();
//parser.PrintSiebelRecords(output);
// Parse and set records to Globals 
var hospitaldataRecords = hospitaldataParser.ParseCsv();
var eppicdataRecords = eppicdataParser.ParseCsv();
var siebeldataRecords = siebeldataParser.ParseCsv();
var giactdataRecords = giactdataParser.ParseCsv();
Globals.hospitalRecords = hospitaldataRecords;
Globals.eppicRecords = eppicdataRecords;
Globals.siebelRecords = siebeldataRecords;
Globals.giactRecords = giactdataRecords;
DataHelper.Step1_BuildEppicListWithMatchesInHospital();  // Build the list of Eppic Records that have an Address Match in Hospital DB
DataHelper.Step1_BuildEppicListWithoutInAgainstHospital(); // Build the list of Eppic Records that do not have an Address Match in Hospital DB, this need to be processed by Step 2.
// The records can be accessed for processing as such:
var test = Globals.inputEppicRecordsInHospitalDB?.Count();
var test2 = Globals.inputEppicRecordsNotInHospitalDB?.Count();
#endregion

#region Print Hospital Data, FindHospitalByFullAddress
Console.WriteLine("\n\n Let's play with the Hospital Shelter File using HospitalShelterData Parser!");
var recordswithFullAddress = hospitaldataParser.FindHospitalByFullAddress("799 47dH bd", "", "SAN DIEGO", "CA", hospitaldataRecords);
Console.WriteLine(recordswithFullAddress?.AddressLine1);
hospitaldataParser.PrintHospitalRecords(hospitaldataRecords);
#endregion

#region Print Epic Data, FindEppicPersonID 
Console.WriteLine("\n\n Let's play with the Eppic Data using Eppic Data Parser!");
var recordswithPersonID = eppicdataParser.FindEppicPersonID("5094334",eppicdataRecords);
Console.WriteLine(recordswithPersonID?.AddressLine1);
eppicdataParser.PrintEppicRecords(eppicdataRecords);
#endregion

#region Print Siebel Data using SiebelDataParser Class
Console.WriteLine("\n\n Let's play with the Siebel data using SiebelDataParser");
var recordswithCallNotes = siebeldataParser.FindAllSiebelCallNotesByPersonID("5094334");
Console.WriteLine(recordswithCallNotes?.FirstOrDefault()?.CallNotes);
siebeldataParser.PrintSiebelRecords(siebeldataRecords);
#endregion

#region Print GIACT Data example using Giact Data Parser
Console.WriteLine("\n\n Let's play with the Giact file using GiactDataParser");
var recordswithUniqueID = giactdataParser.FindGiactUniqueID("5094334",giactdataRecords);
Console.WriteLine(recordswithUniqueID?.AddressLine1);
giactdataParser.PrintGiactRecords(giactdataRecords);
#endregion

#region Semantic Kernel Example Parsing of All CallNotes
// Lets Run the AI code on all the siebel records just to check how to dealt with the ActivityDescription (Call Notes)
Console.WriteLine("Checking for Fraud Conclusion using AI ");
CallLogChecker callLogChecker = new CallLogChecker();

var fraudIntentResult = "";
foreach (var recordSiebel in siebeldataRecords)
{
    fraudIntentResult = await callLogChecker.CheckFraudIntentAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
    Console.WriteLine($@"# Fraud Conslusion Result # PersonID: {recordSiebel.PersonID}");
    Console.WriteLine($@"{fraudIntentResult}\n");
}

Console.WriteLine("Checking all recrods for Action Conclusion using AI ");
var actionConclusionResult = "";
foreach (var recordSiebel in siebeldataRecords)
{
    actionConclusionResult = await callLogChecker.CheckActionConclusionAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
    Console.WriteLine($@"# Fraud Conslusion Result # PersonID: {recordSiebel.PersonID}");
    Console.WriteLine($@"{actionConclusionResult}\n");
}
#endregion
