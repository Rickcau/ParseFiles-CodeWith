
using ConsoleApp_ParseData.Helpers;
using ConsoleApp_ParseData.Models;
using FileHelpers;
using System.Configuration;
using Microsoft.SemanticKernel;
using ConsoleApp_ParseData.Util;
using System.Text.Json;

Console.WriteLine("Parase Data Examples...");
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.

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
DataHelper_old dataHelper = new DataHelper_old(uploadedFilesRequest, _blobConnection, true);
var result = await dataHelper.Intialize();
var siebeldataRecords = dataHelper.SiebelDataRecords;
#endregion

#region New Logic Per Nitin Examples
// New Models are: FormOfAuthConclusion, IvrFraudConclusion, KBAorOTPFraudConclusion
// New AI functions abstracted into CallLogsCheckerV2 just so you have the new code isolated, you can move this into your CallLogsChecker code base
// I just extracted out so you could look only at it without the distraction of the existing code in CallLogsChecker
// Step 1 Logic find all records in Hospital Mark as Not Fraud do on go to step two
//
//
//

// Step 1 - We have all the data needed in Globals.inputEppicRecordsInHospitalDB and Globals.inputEppicRecordsNotInHospitalDB for step 1
// All items in the Globals.inputEppicRecordsInHospitalDB added to RecordsResults as Passed Step 1 Not Fraud

// Step 2 - We have all the data needed in Step2_BuildEppicListWithMatchesInGiactNotInHospitalsDB() and Step2_BuildEppicListWithMatchesNotInGiactNorHospitals();
// All items in Step2_BuildEppicListWithMatchesInGiactNotInHospitalsDB() added to RecordsResults as Passed Step 2 not fraud
// All items in Step2_BuildEppicListWithMatchesNotInGiactNorHospitals() added to RecordsResults as Failed Step 2 move to step3 

// Now, we have all the data in RecordsResults that need to be processed in Step 3
// Loop Through all records in RecordsResult to process where LastStepCompleted = 2 and StepRuleToApply= 3
// run the rules agains each record set the fields as needed.

// 

//
//Example of that RecordsToProcess would look like
// "1234", "7048605555", "AddressLine1","AddressLine2", "Charlotte", "NC", "28808", 1, 0,null,"",true,null,null, null, null, null, null
// 1. Add all items in Globals.inputEppicRecordsInHospitalDB to RecordsToProcess
// set MarkAsFraud to False, StepRuleToApply = 1, LastStepCompleted = 1, Status = "Found in Hospital - Not Fraud"
// MoveToNext to false
// 
// 1.a Add all items in Globals.inputEppicRecordsNotInHospitalDB to RecordsToProcess and 
// set MarkAsFraud to False, StepRuleToApply = 2, LastStepCompleted = 1, Status = "Not found in Hospital"
// Run Step 2

if (Constants.RunNewAILogicExamples)
{
    List<RecordsResults> results = new List<RecordsResults>();
    RecordsResults recordresults;
    // Step 1 loop through items Globals.inputEppicRecordsInHospitalDB mark as not fraud
    if (Globals.inputEppicRecordsInHospitalDB != null)
    {
        foreach (var item in Globals.inputEppicRecordsInHospitalDB)
        {
            recordresults = new RecordsResults();
            recordresults.PersonID = item.PersonID;
            recordresults.PhoneNumber = item.Phone_Number;
            recordresults.AddressLine1 = item.AddressLine1;
            recordresults.AddressLine2 = item.AddressLine2;
            recordresults.City = item.City;
            recordresults.State = item.State;
            recordresults.Zip = item.ZipCode;
            recordresults.StepRuleToApply = 1;
            recordresults.LastStepCompleted = 1;
            recordresults.MarkedAsFraud = false;
            recordresults.Status = "Step 1 Found Address in Hospital - Not Fraud!";
            recordresults.Completed = true;
            recordresults.Step1HospitalMatch = true;
            results.Add(recordresults);
        }
    }
    if (Globals.inputEppicRecordsNotInHospitalDB != null)
    { 
        foreach (var item in Globals.inputEppicRecordsNotInHospitalDB)
        {
            recordresults = new RecordsResults();
            recordresults.PersonID = item.PersonID;
            recordresults.PhoneNumber = item.Phone_Number;
            recordresults.AddressLine1 = item.AddressLine1;
            recordresults.AddressLine2 = item.AddressLine2;
            recordresults.City = item.City;
            recordresults.State = item.State;
            recordresults.Zip = item.ZipCode;
            recordresults.StepRuleToApply = 2;
            recordresults.LastStepCompleted = 1;
            recordresults.MarkedAsFraud = false;
            recordresults.Status = "Step 2 Not in Hospital - Move to Step 2";
            recordresults.Completed = false;
            recordresults.Step1HospitalMatch = false;
            results.Add(recordresults);
        }
    }

    // Step 2 
    if (Globals.inputEppicRecordsInGiactDBNotInHospitalsDB != null)
    {
        foreach (var item in Globals.inputEppicRecordsInGiactDBNotInHospitalsDB)
        {
            // Find the record by PersonID
            var foundRecord = results.FirstOrDefault(record => record.PersonID == item.PersonID);
            if (foundRecord != null)
            {
                foundRecord.MarkedAsFraud = false;
                foundRecord.StepRuleToApply = 2;
                foundRecord.LastStepCompleted = 2;
                foundRecord.Status = "Step 2 Found Address in Giact - Not Fraud!";
                foundRecord.Completed = true;
                foundRecord.Step1HospitalMatch = false;
                foundRecord.Step2GiactMatch = true;
            }
            else {  // record not found so lets add a new record
                recordresults = new RecordsResults();
                recordresults.PersonID = item.PersonID;
                recordresults.PhoneNumber = item.Phone_Number;
                recordresults.AddressLine1 = item.AddressLine1;
                recordresults.AddressLine2 = item.AddressLine2;
                recordresults.City = item.City;
                recordresults.State = item.State;
                recordresults.Zip = item.ZipCode;
                recordresults.StepRuleToApply = 2;
                recordresults.LastStepCompleted = 2;
                recordresults.MarkedAsFraud = false;
                recordresults.Status = "Step 2 Found Address in Giact - Not Fraud!";
                recordresults.Completed = true;
                recordresults.Step1HospitalMatch = false;
                recordresults.Step2GiactMatch = true;
                results.Add(recordresults);
        }
        } // all these records have been processed as not fraud and stopped at step 2 flagged as completed
    }

    if (Globals.inputEppicRecordsNotInGiactDBNorHospitalsDB != null) // All these records need to be go to step 3
    {
        foreach (var item in Globals.inputEppicRecordsNotInGiactDBNorHospitalsDB)
        {
            // Find the record by PersonID
            var foundRecord = results.FirstOrDefault(record => record.PersonID == item.PersonID);
            if (foundRecord != null)
            {
                foundRecord.MarkedAsFraud = false;
                foundRecord.StepRuleToApply = 3;
                foundRecord.LastStepCompleted = 2;
                foundRecord.Status = "Step 2 Address not found in Giact go to Step 3";
                foundRecord.Completed = false;
                foundRecord.Step1HospitalMatch = false;
                foundRecord.Step2GiactMatch = false;
            }
            else
            {   // Not found we need to add a new record
                recordresults = new RecordsResults();
                recordresults.PersonID = item.PersonID;
                recordresults.PhoneNumber = item.Phone_Number;
                recordresults.AddressLine1 = item.AddressLine1;
                recordresults.AddressLine2 = item.AddressLine2;
                recordresults.City = item.City;
                recordresults.State = item.State;
                recordresults.Zip = item.ZipCode;
                recordresults.StepRuleToApply = 3;
                recordresults.LastStepCompleted = 2;
                recordresults.MarkedAsFraud = false;
                recordresults.Status = "Step 2 No Address in Giat or Hospital - Move to Step 3";
                recordresults.Completed = false;
                recordresults.Step1HospitalMatch = false;
                recordresults.Step2GiactMatch = false;
                results.Add(recordresults);
            }
        }
    }
    // Next we simple need to execute Step 3 Logic which means we need to loop through all recordresults that have completed = false and StepRuleToApply = 3
    if (results!= null)
    {
        CallLogCheckerV2 callLogCheckerV2Step3 = new CallLogCheckerV2();
        var recordsToProcessForStep3 =
            (from e in results
             where e.Completed == false
             where e.StepRuleToApply == 3
             select e).Distinct();
        foreach (var item in recordsToProcessForStep3)
        {
            // Find Siebel Record and extract call notes for processing
            var recordswithCallNotes = dataHelper.SiebelDataParser.FindAllSiebelCallNotesByPersonIDLastFirst(item.PersonID ?? "");
            if (recordswithCallNotes.Count() == 0)  // this has to be an error in the data files as we should have record in SieBel!
            {
                item.LastStepCompleted = 3;
                item.MarkedAsFraud = true;
                item.Status = "Step 3 Part 1 - Data Error - This record needs to be parsed using Step Logic, but there is no record in SIEBEL";
                item.Completed = true;
                item.LastStepCompleted = 3;
            }
            else if(recordswithCallNotes.Count() > 0) // we have records to process
            {
                var formOfAuthConclusionJson = await callLogCheckerV2Step3.CheckFormOfAuthenticationAsync(kernel, item.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");
                FormOfAuthConclusion? formOfAuthResult = JsonSerializer.Deserialize<FormOfAuthConclusion>(formOfAuthConclusionJson);
                if (formOfAuthResult?.ActivityRelatedTo == "Inbound Call" && formOfAuthResult.WasCallerAuthenticated == "No")
                {
                    item.LastStepCompleted = 3;
                    item.MarkedAsFraud = true;
                    item.Status = "Step 3 Part 1 Inbound Call but Caller not Authenticated Mark as Fraud stop processing";
                    item.Completed = true;
                    item.Step3Part1_InboundCallAuthNo = true;
                    item.LastStepCompleted = 3;
                    break;
                }
                else if (formOfAuthResult?.ActivityRelatedTo == "Inbound Call" && formOfAuthResult.WasCallerAuthenticated == "Yes")
                {
                    item.LastStepCompleted = 3;
                    item.MarkedAsFraud = false;
                    item.Status = "Step 3 Part 1 - Inbound Call Caller Authenticated";
                    item.Completed = false;
                    item.Step3Part1_InboundCallAuthNo = false;
                    item.Step3Part2_InboundCallAuthYes = true;
                    // Now we need to check other items
                    switch (formOfAuthResult?.FormOfAuthentication)
                    {
                        case "ID Verification":
                            item.LastStepCompleted = 3;
                            item.MarkedAsFraud = false;
                            item.Status = "Step 3 Part 3 - Inbound Call ID Verification, Caller Authenticated";
                            item.Completed = false;
                            item.Step3Part1_InboundCallAuthNo = false;
                            item.Step3Part2_InboundCallAuthYes = true;
                            item.Step3Part3_ComplexCheck = true;
                            break;
                        case "IVR Autentication":
                            var ivrFraudConclusionJson = await callLogCheckerV2Step3.CheckIvrFraudConclusionAsync(kernel, item.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");
                            IvrFraudConclusion? ivrFraudConclusionResult = JsonSerializer.Deserialize<IvrFraudConclusion>(ivrFraudConclusionJson);
                            if (ivrFraudConclusionResult?.BannerColor == "Green") // Not Fraud
                            {
                                item.LastStepCompleted = 3;
                                item.MarkedAsFraud = false;
                                item.Status = "Step 3 Part 3 IVR Banner Color Green found!";
                                item.Completed = true;
                                item.Step1HospitalMatch = false;
                                item.Step2GiactMatch = false;
                                item.Step3Part1_InboundCallAuthNo = false;
                                item.Step3Part2_InboundCallAuthYes = true;
                                item.Step3Part3_ComplexCheck = true;
                            }
                            else  // banner color not Green mark as fraud
                            {
                                item.LastStepCompleted = 3;
                                item.MarkedAsFraud = true;
                                item.Status = "Step 3 Part 3 IVR Banner Color Green NOT found! - Fraud";
                                item.Completed = true;
                                item.Step1HospitalMatch = false;
                                item.Step2GiactMatch = false;
                                item.Step3Part1_InboundCallAuthNo = false;
                                item.Step3Part2_InboundCallAuthYes = true;
                                item.Step3Part3_ComplexCheck = true;
                            }
                            break;
                        case "KBA":
                            var kbaFraudConclusionJson = await callLogCheckerV2Step3.CheckKBAorOTPFraudConclusionAsync(kernel, item.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");
                            KBAorOTPFraudConclusion? kbaFraudConclusionResult = JsonSerializer.Deserialize<KBAorOTPFraudConclusion>(kbaFraudConclusionJson);
                            if (kbaFraudConclusionResult?.ReferenceIdFound == "Yes" && kbaFraudConclusionResult?.WasPassedFound == "Yes") // Not Fraud
                            {
                                item.LastStepCompleted = 3;
                                item.MarkedAsFraud = false;
                                item.Status = "Step 3 Part 3 KBA RefID and Passed found not Fraud!";
                                item.Completed = true;
                                item.Step1HospitalMatch = false;
                                item.Step2GiactMatch = false;
                                item.Step3Part1_InboundCallAuthNo = false;
                                item.Step3Part2_InboundCallAuthYes = true;
                                item.Step3Part3_ComplexCheck = true;
                            }
                            else  // banner color not Green mark as fraud
                            {
                                item.LastStepCompleted = 3;
                                item.MarkedAsFraud = true;
                                item.Status = "Step 3 Part 3 KBA RefID or Passed not found - Mark as Fraud!";
                                item.Completed = true;
                                item.Step1HospitalMatch = false;
                                item.Step2GiactMatch = false;
                                item.Step3Part1_InboundCallAuthNo = false;
                                item.Step3Part2_InboundCallAuthYes = true;
                                item.Step3Part3_ComplexCheck = true;
                            }
                            break;
                        case "One Time Passcode":
                            var otpFraudConclusionJson = await callLogCheckerV2Step3.CheckKBAorOTPFraudConclusionAsync(kernel, item.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");
                            KBAorOTPFraudConclusion? otpFraudConclusionResult = JsonSerializer.Deserialize<KBAorOTPFraudConclusion>(otpFraudConclusionJson);
                            if (otpFraudConclusionResult?.ReferenceIdFound == "Yes" && otpFraudConclusionResult?.WasPassedFound == "Yes") // Not Fraud
                            {
                                item.LastStepCompleted = 3;
                                item.MarkedAsFraud = false;
                                item.Status = "Step 3 Part 3 OTP RefID and Passed found not Fraud!";
                                item.Completed = true;
                                item.Step1HospitalMatch = false;
                                item.Step2GiactMatch = false;
                                item.Step3Part1_InboundCallAuthNo = false;
                                item.Step3Part2_InboundCallAuthYes = true;
                                item.Step3Part3_ComplexCheck = true;
                            }
                            else  // RefID and Pass not found Mark as Fraud
                            {
                                item.LastStepCompleted = 3;
                                item.MarkedAsFraud = true;
                                item.Status = "Step 3 Part 3 KBA RefID or Passed not found - Mark as Fraud!";
                                item.Completed = true;
                                item.Step1HospitalMatch = false;
                                item.Step2GiactMatch = false;
                                item.Step3Part1_InboundCallAuthNo = false;
                                item.Step3Part2_InboundCallAuthYes = true;
                                item.Step3Part3_ComplexCheck = true;
                            }
                            break;
                        default:
                            Console.WriteLine("Encounted a form of authentication we dont need to process!");
                            break;
                    }
                }
            }
        }
    }
    //  Let's print all the results out now
    if (results != null)
    {
        foreach (var item in results)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine($@"PersonID: {item.PersonID} MarkedAsFraud: {item.MarkedAsFraud} Completed: {item.Completed} LastStepCompleted: {item.LastStepCompleted} Status: {item.Status}");
        }

    }

    //Console.WriteLine("Let's simply loop through all the SIEBEL Call Notes calling all the not AI functions as examples");

    //CallLogCheckerV2 callLogCheckerV2 = new CallLogCheckerV2();
    //Console.WriteLine();
    //Console.WriteLine("Iterate over all the Call Notes using New AI Logic. Press Enter!");
    //Console.ReadLine();
    //var recordsProcessed = 1;
    //if (siebeldataRecords != null)
    //{
    //    foreach (var recordSiebel in siebeldataRecords)
    //    {
            
    //        var formOfAuthConclusionJson = await callLogCheckerV2.CheckFormOfAuthenticationAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
    //        FormOfAuthConclusion? formOfAuthResult = JsonSerializer.Deserialize<FormOfAuthConclusion>(formOfAuthConclusionJson);
    //        Console.WriteLine($@"Form Of Authentication Conclusion:{recordsProcessed}");
    //        Console.WriteLine($@"PersonID: {recordSiebel.PersonID}");
    //        Console.WriteLine(formOfAuthConclusionJson);
    //        Console.WriteLine(" ----------------------------");
    //        switch (formOfAuthResult?.FormOfAuthentication)
    //        {
    //            case "ID Verification":
    //                Console.WriteLine("Have not really added logic for this, should be simple enought for you to handle");
    //                break;
    //            case "IVR Autentication":
    //                Console.WriteLine("IVR Authentication Record needs to be processed");
    //                var ivrFraudConclusionJson = await callLogCheckerV2.CheckIvrFraudConclusionAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
    //                Console.WriteLine($@"IVR Fraud Conclusion:{recordsProcessed}");
    //                Console.WriteLine($@"PersonID: {recordSiebel.PersonID}");
    //                Console.WriteLine(ivrFraudConclusionJson);
    //                Console.WriteLine(" ----------------------------");
    //                break;
    //            case "KBA":
    //                Console.WriteLine("KBA Authentication Record needs to be processed");
    //                var kbaFraudConclusionJson = await callLogCheckerV2.CheckKBAorOTPFraudConclusionAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
    //                Console.WriteLine(" ----------------------------");
    //                Console.WriteLine($@"KBA Fraud Conclusion:{recordsProcessed}");
    //                Console.WriteLine($@"PersonID: {recordSiebel.PersonID}");
    //                Console.WriteLine(kbaFraudConclusionJson);
    //                Console.WriteLine(" ----------------------------");
    //                break;
    //            case "One Time Passcode":
    //                Console.WriteLine("OTP Authentication Record needs to be processed");
    //                var otpFraudConclusionJson = await callLogCheckerV2.CheckKBAorOTPFraudConclusionAsync(kernel, recordSiebel.PersonID ?? "", recordSiebel.ActivityDescription ?? "");
    //                Console.WriteLine(" ----------------------------");
    //                Console.WriteLine($@"OTP Fraud Conclusion:{recordsProcessed}");
    //                Console.WriteLine($@"PersonID: {recordSiebel.PersonID}");
    //                Console.WriteLine(otpFraudConclusionJson);
    //                Console.WriteLine(" ----------------------------");
    //                break;
    //            default:
    //                Console.WriteLine("Encounted a form of authentication we dont need to process!");
    //                break;
    //        }
    //        Console.WriteLine();
    //        recordsProcessed++;
    //    }
    //    Console.WriteLine($@"Count if SiebelRecords: {siebeldataRecords.Count}, Count of Records Process: {recordsProcessed}");
    //}
}

#endregion


#region Hospital Examples
// Hospital Examples
if (Constants.RunHospitalExamples)
{
    var hospitaldataRecords = dataHelper.HospitalDataRecords;
    Console.WriteLine("Ready to Go, let's search for a HospitalByFullAddress. Press Enter!");
    Console.ReadLine();
    var recordswithFullAddress = dataHelper.HospitalShelterDataParser.FindHospitalByFullAddress("799 47dH bd", "", "SAN DIEGO", "CA", hospitaldataRecords ?? new List<HospitalShelterRecords>());
    Console.WriteLine($@"Hospital Found: {recordswithFullAddress?.AddressLine1}");
    Console.WriteLine("Let's print out all the Hospital Records, press  Enter!");
    Console.ReadLine();
    dataHelper.HospitalShelterDataParser.PrintHospitalRecords(hospitaldataRecords ?? new List<HospitalShelterRecords>());
    Console.WriteLine();
}
#endregion

#region Eppic Examples
// Eppic Examples
if (Constants.RunEppicExamples)
{
    var eppicdataRecords = dataHelper.EppicDataRecords;
    Console.WriteLine("Ready to Go, let's search for a PersonID in the Eppic Data. Press Enter!");
    Console.ReadLine();
    var recordswithPersonID = dataHelper.EppicDataParser.FindEppicPersonID("5094334", eppicdataRecords ?? new List<EppicRecords>());
    Console.WriteLine($@"Eppic Record Found: {recordswithPersonID?.AddressLine1}");
    Console.WriteLine("Let's print out all the Eppic Records, press  Enter!");
    Console.ReadLine();
    dataHelper.EppicDataParser.PrintEppicRecords(eppicdataRecords ?? new List<EppicRecords>());
    Console.WriteLine();
}
#endregion

#region Siebel Examples
// Siebel  Examples
if (Constants.RunSiebelExamples)
{
    // var siebeldataRecords = dataHelper.SiebelDataRecords;
    Console.WriteLine("Ready to Go, let's find Siebel CallNotes by PersonID. Press Enter!");
    Console.ReadLine();
    var recordswithCallNotes = dataHelper.SiebelDataParser.FindAllSiebelCallNotesByPersonID("5094334");
    Console.WriteLine($@"Siebel Record Found: {recordswithCallNotes?.FirstOrDefault()?.CallNotes}");
    Console.WriteLine("Let's print out all the Siebel Records. Press  Enter!");
    Console.ReadLine();
    dataHelper.SiebelDataParser.PrintSiebelRecords(siebeldataRecords ?? new List<SiebelRecords>());
    Console.WriteLine();
}
#endregion

#region Giact Examples
// Giact  Examples
if (Constants.RunGiactlExamples)
{
    var giactdataRecords = dataHelper.GiactDataRecords;
    Console.WriteLine("Ready to Go, let's find Giact Record by UniqueID. Press Enter!");
    Console.ReadLine();
    var recordswithUniqueID = dataHelper.GiactDataParser.FindGiactUniqueID("5094334", giactdataRecords ?? new List<GiactRecords>()); ;
    Console.WriteLine($@"Giact Record Found: {recordswithUniqueID?.AddressLine1}");
    Console.WriteLine("Let's print out all the Giact Records. Press  Enter!");
    Console.ReadLine();
    dataHelper.GiactDataParser.PrintGiactRecords(giactdataRecords ?? new List<GiactRecords>());
    Console.WriteLine();
}
#endregion

#region Semantic Kernel Examples
// Semantic Kernel Examples Parsing of Call Notes with AI
if (Constants.RunSemanticKernelExamples)
{
    Console.WriteLine("Checking for Fraud Conclusion using AI. Press Enter! ");
    Console.ReadLine();
    //var siebeldataRecords = dataHelper.SiebelDataRecords;
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
}
#endregion

