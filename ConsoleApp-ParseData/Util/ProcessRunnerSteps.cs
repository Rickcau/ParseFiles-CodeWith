using ConsoleApp_ParseData.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleApp_ParseData.Util
{
   
    public class StepResult
    {
        public string? StepName { get; set; }
        public string? StatusCode { get; set; }
        public string? Message { get; set; }
    }

    public class ProcessResult
    {
        public StepResult? EppicAddressInHospitalDBResult { get; set; }  // Step 1
        public StepResult? EppicAddressInGiactResult { get; set; }       // Step 2 
        public StepResult? EppicSeibelCallNotesResult { get; set; }      // Step 3
        public StepResult? EppicSeibelOPTCallNotesResult { get; set; }   // Step 3a
        public string? MasterStatusCode { get; set; }                    // Pass or Fail
    }
    public class ProcessRunnerSteps
    {
        private string _address;
        #region Example useage
        // Instantiate the ProcessRunner class
        /*
            var processRunner = new ProcessRunner();

            // Run the steps
            var processResult = processRunner.RunSteps();

            // Convert the result to JSON
            var json = JsonSerializer.Serialize(processResult, new JsonSerializerOptions { WriteIndented = true });

            // Print the JSON result
            Console.WriteLine(json);
            */
        #endregion
        #region Important Notes
        // Like we will need to pass in a pointer to the file that trigger ProcessRunner
        // This data will be used to perform the lookups.  Since we do not know that that data looks like yet 
        // we will need to add that after the fact.
        #endregion
        public ProcessRunnerSteps(string address)
        {
            this._address = address;
        }

        public ProcessResult RunSteps()
        {
            // The loop likely needs to be outside of this logic
            // Each of these steps need to be ran sequentially looping through the EPPIC records one at a time
            // Checking for Pass or Fail then logging the result to either a list or file so it can be reported on
            var processResult = new ProcessResult();
            #region Step 1: Eppic Check Against Hospital DB
            processResult.EppicAddressInHospitalDBResult = new StepResult { StepName = "Step1: Eppic Address Check In Hospitals" };
            try  // Step 1 - Check the Address Info of the EPPIC record to see if there is a Match in the Hospital DB  if true then stop the process for this record log the details and move to next step
            {
                // Need to call the logic to perform the Eppic Check Against Hospital set the status
                // Since you need to 
                processResult.EppicAddressInHospitalDBResult.StatusCode = "PASS";
                processResult.EppicAddressInHospitalDBResult.Message = "Address Not found in Hospital DB go to Step 2";
                if (processResult.EppicAddressInHospitalDBResult.StatusCode == "FAILED Hospital Check")
                {
                    return processResult; // Exit and return the result
                }
            }
            catch (Exception ex)
            {
                // If there is an excepton with the API call set StatusCode to FAIL and exit
                processResult.EppicAddressInHospitalDBResult.StatusCode = "FAIL";
                processResult.EppicAddressInHospitalDBResult.Message = ex.Message;
                processResult.MasterStatusCode = "FAIL";
                return processResult;
            }
            #endregion

            #region Step 2: Check Eppic Address against GIACT only if last step is set to Pass
            // When Last Step is set to Pass that means the previous step requires moving to the next step
            processResult.EppicAddressInGiactResult = new StepResult { StepName = "Step2: Eppic Address Check in GIACT" };
            try
            {
                // Stub out Address Check using GIACT
                // Replace with actual implementation
                // using the _address details that were passed in execute the logic to verify the Address Check using GIACT lookup
                // Set the values to determine PASS or FAIL
                processResult.EppicAddressInGiactResult.StatusCode = "PASS";
                processResult.EppicAddressInGiactResult.Message = "Success";
                if (processResult.EppicAddressInGiactResult.StatusCode == "FAIL")
                {
                    processResult.MasterStatusCode = "FAIL";
                    return processResult; // Exit and return the result
                }
            }
            catch (Exception ex)
            {
                processResult.EppicAddressInGiactResult.StatusCode = "FAIL";
                processResult.EppicAddressInGiactResult.Message = ex.Message;
                processResult.MasterStatusCode = "FAIL";
                return processResult;
            }
            #endregion

            #region Step 3: Check Eppic Record Against Seibel CallNotesResult only if last step is set to pass
            // When Last Step is set to Pass that means the previous step requires moving to the next step
            // This logic requires the use AI
            processResult.EppicSeibelCallNotesResult = new StepResult { StepName = "Step 3: Eppic Record Check in GIACT Call Notes" };
            try
            {
                // Stub out Address Check using GIACT
                // Replace with actual implementation
                // using the _address details that were passed in execute the logic to verify the Address Check using GIACT lookup
                // Set the values to determine PASS or FAIL
                processResult.EppicSeibelCallNotesResult.StatusCode = "PASS";
                processResult.EppicSeibelCallNotesResult.Message = "Success";
                if (processResult.EppicSeibelCallNotesResult.StatusCode == "FAIL")
                {
                    processResult.MasterStatusCode = "FAIL";
                    return processResult; // Exit and return the result
                }
            }
            catch (Exception ex)
            {
                processResult.EppicSeibelCallNotesResult.StatusCode = "FAIL";
                processResult.EppicSeibelCallNotesResult.Message = ex.Message;
                processResult.MasterStatusCode = "FAIL";
                return processResult;
            }
            #endregion

            #region Step 3a: Check Eppic Record Against Seibel CallNotes OTP Check only if last step is set to pass
            // When Last Step is set to Pass that means the previous step requires moving to the next step
            // This logic requires the use AI
            processResult.EppicSeibelOPTCallNotesResult = new StepResult { StepName = "Step 3a: Eppic OTP Check in GIACT Call Notes" };
            try
            {
                processResult.EppicSeibelOPTCallNotesResult.StatusCode = "PASS";
                processResult.EppicSeibelOPTCallNotesResult.Message = "Success";
                if (processResult.EppicSeibelOPTCallNotesResult.StatusCode == "FAIL")
                {
                    processResult.MasterStatusCode = "FAIL";
                    return processResult; // Exit and return the result
                }
            }
            catch (Exception ex)
            {
                processResult.EppicSeibelOPTCallNotesResult.StatusCode = "FAIL";
                processResult.EppicSeibelOPTCallNotesResult.Message = ex.Message;
                processResult.MasterStatusCode = "FAIL";
                return processResult;
            }
            #endregion
            return processResult;
        }

        #region Example JSON response
        /*
            {
                "SiebelLookupResult": {
                    "StepName": "Siebel Lookup",
                    "StatusCode": "PASS",
                    "Message": "Success"
                },
                "AddressCheckResult": {
                    "StepName": "Address Check",
                    "StatusCode": "PASS",
                    "Message": "Success"
                },
                "GoogleSearchResult": {
                    "StepName": "Google Search",
                    "StatusCode": "PASS",
                    "Message": "Success"
                },
                "MasterStatusCode":"PASS"
            }

        */
        #endregion

        #region GoogleSearch Not bing used
        //private bool PerformGoogleSearch(string address)
        //{
        //    // Implement Google Search API call to verify address
        //    // Replace with actual implementation
        //    using (var client = new HttpClient())
        //    {
        //        // var address = "1600 Amphitheatre Parkway, Mountain View, CA"; // Replace with actual address
        //        var apiKey = "YOUR_GOOGLE_API_KEY"; // Replace with actual API key
        //        var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={address}&key={apiKey}";
        //        var response = client.GetAsync(url).Result;
        //        var content = response.Content.ReadAsStringAsync().Result;
        //        var jsonData = JsonDocument.Parse(content);
        //        var results = jsonData.RootElement.GetProperty("results");
        //        foreach (var result in results.EnumerateObject())
        //        {
        //            var types = result.Value.GetProperty("types");
        //            if (types.EnumerateArray().Any(t => t.GetString() == "hospital" || t.GetString() == "health"))
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
        #endregion
    }
}

