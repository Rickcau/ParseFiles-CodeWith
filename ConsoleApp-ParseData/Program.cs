// See https://aka.ms/new-console-template for more information
using ConsoleApp_ParseData.Helpers;
using ConsoleApp_ParseData.Models;
using FileHelpers;

Console.WriteLine("Parase Data Examples...");


#region Parse Epic File Example
// Parse Epic example

//Console.WriteLine("Let's read the Epicc File\n");

//var engineEpicc = new FileHelperEngine<EpiccRecords>();
//var recordsEpicc = engineEpicc.ReadFile("C:\\temp\\Data\\Epicc\\TEST-2023-11-07_GIACT_7DAY.csv");
//var count1 = 0;
//foreach (var recordEpicc in recordsEpicc)
//{
//    count1++;
//    Console.WriteLine($@"Record# {count1} UniqueID: {recordEpicc.UniqueID} AddressLine1: {recordEpicc.AddressLine1} AddressLine2: {recordEpicc.AddressLine2} City: {recordEpicc.City} State: {recordEpicc.State} ZipCode: {recordEpicc.ZipCode}");
//}
//Console.WriteLine("\n\n");
#endregion


#region Parse Siebel Example using SiebelDataParser Class
var siebeldataParser = new SiebelDataParser();
siebeldataParser.ParseCsv("C:\\temp\\Data\\Siebel\\NOV7_activity_v2.csv");
var recordswithCallNotes = siebeldataParser.FindAllSiebelCallNotesByPersonID("5094334");
siebeldataParser.PrintSiebelCallNoteRecords(recordswithCallNotes);
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
//var recordsGiact = engineGiact.ReadFile("C:\\temp\\Data\\Giact\\NOV_TEST_GIACT.csv");

//foreach (var recordGiact in recordsGiact)
//{
//    Console.WriteLine($@"UniqueID: { recordGiact.UniqueID} City: {recordGiact.City} State: {recordGiact.City} ZipCode: {recordGiact.ZipCode} AddressLine1: {recordGiact.AddressLine1}");
//}
#endregion
