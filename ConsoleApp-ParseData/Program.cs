// See https://aka.ms/new-console-template for more information
using ConsoleApp_ParseData.Helpers;
using ConsoleApp_ParseData.Models;
using FileHelpers;

Console.WriteLine("Hello, World!");

//string filePath = "C:\\temp\\Data\\Siebel\\NOV7_activity_v2.csv"; // Replace with your actual file path

//SiebelDataParser siebelDataParser = new SiebelDataParser();

//List<SiebelData> siebelDataList = siebelDataParser.ParseCsv(filePath);

//// Display the parsed data
//foreach (var siebelData in siebelDataList)
//{
//    Console.WriteLine($"Program Name: {siebelData.ProgramName}, Person ID: {siebelData.PersonID}, Contact First Name: {siebelData.ContactFirstName}, Contact Last Name: {siebelData.ContactLastName}, Activity Created Date: {siebelData.ActivityCreatedDate}, Activity Type: {siebelData.ActivityType}, Activity Created By: {siebelData.ActivityCreatedBy}, Activity Description: {siebelData.ActivityDescription}");
//}

var engine = new FileHelperEngine<SiebelRecords>();
var records = engine.ReadFile("C:\\temp\\Data\\Siebel\\NOV7_activity_v2.csv");


foreach (var record in records)
{
    Console.WriteLine(record.CustomerID);
    Console.WriteLine(record.ProgramName);
    Console.WriteLine(record.ActivityCreatedBy);
    Console.WriteLine(record.ActivityDescription);
   // Console.WriteLine(record.OrderDate.ToString("dd/MM/yyyy"));
    
}
