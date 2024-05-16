using ConsoleApp_ParseData.Helpers;
using ConsoleApp_ParseData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace ConsoleApp_ParseData.Util
{
    internal static class StepHelper
    {
        static public void RunStep1(List<RecordsResults> results)
        {
            RecordsResults recordresults;
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
            else
            {
                throw new NullReferenceException();
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
            else
            {
                throw new NullReferenceException();
            }
        }

        static public void RunStep2(List<RecordsResults> results)
        {
            RecordsResults recordresults;
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
                    else
                    {  // record not found so lets add a new record
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
            else
            {
                throw new NullReferenceException();
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
            } else {
            
                throw new NullReferenceException();
            }
        }

        static public async Task RunStep3(List<RecordsResults> results, Kernel kernel, DataHelper_old dataHelper)
        {
            // Next we simple need to execute Step 3 Logic which means we need to loop through all recordresults that have completed = false and StepRuleToApply = 3
            if (results != null)
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
                    else if (recordswithCallNotes.Count() > 0) // we have records to process
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
            else
            {
                throw new NullReferenceException();
            }
        }
    }
}
