using Azure;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace ConsoleApp_ParseData.Util
{
    internal class CallLogCheckerV2
    {
#pragma warning disable SKEXP0010
        private string _promptKBAorOTPFraudConclusion = @"'<query>'
        PersonID: {{$personid}}
        {{$query}} 
        '<end query>'
        Return and populate the [JSON] below based on extracting the details from the '<query>' (ignore case when making decisions).
        Perform the following actions in order.
        1. Search for the words 'Reference ID:' and if found set 'ReferenceIdFound' to 'Yes'
        2. If you find the words 'Reference ID:' and the word Passed or Pass is found, set 'WasPassedFound' to 'Yes' and set 'MarkAsFraud' to 'Not Fraud'
        3. If you do not find the words 'Reference ID:' and the word Passed or Pass, then set 'MarkAsFraud' to 'Fraud'

       [JSON]
             {
                'PersonID': '12345',
                'WasCallerAuthenticated' : 'was caller authenticated>',
                'FormOfAuthentication' : '<form of authentication>',
                'ReferenceIdFound' : '<was Reference ID: found?>',
                'WasPassedFound' : '<was Passed found?>'
                'MarkAsFraud' : <'is this fraud'>
             }
       [JSON END]

       [Examples for JSON Output]
            {
               'PersonID': '12345',
               'WasCallerAuthenticated' : 'Yes',
               'FormOfAuthentication' : 'KBA',
               'ReferenceIdFound' : 'No',
               'WasPassedFound' : 'No'
               'MarkAsFraud' : 'Fraud'
            }
            {
               'PersonID': '12345',
               'WasCallerAuthenticated' : 'Yes',
               'FormOfAuthentication' : 'One Time Passcode',
               'ReferenceIdFound' : 'No',
               'WasPassedFound' : 'No'
               'MarkAsFraud' : 'Fraud'
            }
            {
               'PersonID': '12345',
               'WasCallerAuthenticated' : 'Yes',
               'FormOfAuthentication' : 'One Time Passcode',
               'ReferenceIdFound' : 'Yes',
               'WasPassedFound' : 'Yes'
               'MarkAsFraud' : 'Not Fraud'
            }

       Per user query what is the KBA or OTP Fraud Conclusion?";
        private string _promptIvrFraudConclusion = @"'<query>'
        PersonID: {{$personid}}
        {{$query}}
        '<end query>'
        Return and populate the [JSON] below based on extracting the details from the '<query>' (ignore case when making decisions).
        Perform the following actions in order.
        1. Search for the words 'Banner Color is' and set the 'BannerColor' to the color found in the '<query>' 
        2. If the Banner Color is Green set the 'MarkAsFraud' to 'Not Fraud' otherwise set it to 'Fraud'

       [JSON]
             {
                'PersonID': '12345',
                'WasCallerAuthenticated' : '<was caller authenticated>',
                'FormOfAuthentication' : '<form of authentication>',
                'BannerColor' : '<what is the banner color>'
                'MarkAsFraud' : <'is this fraud'>
             }
       [JSON END]

       [Examples for JSON Output]
            {
               'PersonID': '12345',
               'WasCallerAuthenticated' : 'Yes',
               'FormOfAuthentication' : 'IVR Authenticated',
               'BannerColor' : 'Green',
               'MarkAsFraud' : 'Not Fraud'
            }

            {
               'PersonID': '12345',
               'WasCallerAuthenticated' : 'Yes',
               'FormOfAuthentication' : 'IVR Authenticated',
               'BannerColor' : 'Red',
               'MarkAsFraud' : 'Fraud'
            }
            {
               'PersonID': '12345',
               'WasCallerAuthenticated' : 'Yes',
               'FormOfAuthentication' : 'IVR Authenticated',
               'BannerColor' : 'Yellow',
               'MarkAsFraud' : 'Fraud'
            }
       Per user query what is the IVR Fraud Conclusion?";

        private string _promptFormOfAuthConclusion = @"'<query>'
        PersonID: {{$personid}}
        {{$query}}
        '<end query>'
        Return and populate the [JSON] below based on extracting the details from the '<query>' (ignore case when making decisions).
        Perform the following actions in order.
        1. Look for the string 'Activities Related To:' and extract the value and set the 'ActivityRelatedTo' to this value
        2. Look for the string 'Was Caller Authenticated' the values should be 'Yes or No' and set the 'WasCallerAuthenticated' to this value.
        3. Look for the string 'Form of Authentication:' and extract the value and the 'FormOfAuthentication' to this value.  

       [JSON]
             {
                'PersonID': '12345',
                'ActivityRelatedTo' : '<activity related to>',
                'WasCallerAuthenticated' : 'was caller authenticated>',
                'FormOfAuthentication' : '<form of authentication>'
             }
       [JSON END]

       [Examples for JSON Output]
            {
               'PersonID': '12345',
               'ActivityRelatedTo' : 'Inbound Call',
               'WasCallerAuthenticated 'Yes',
               'FormOfAuthentication' : 'KBA'
            }

            { 
               'PersonID': '12345',
               'ActivityRelatedT' : 'Inbound Call',
               'WasCallerAuthenticated 'Yes',
               'FormOfAuthentication' : 'ID Verification'
            }

            { 
               'PersonID': '12345',
               'ActivityRelatedTo' : 'Inbound Call',
               'WasCallerAuthenticated 'No',
               'FormOfAuthentication' : 'Low Risk'
            }
            { 
               'PersonID': '12345',
               'ActivityRelatedTo' : 'Inbound Call',
               'WasCallerAuthenticated 'Yes',
               'FormOfAuthentication' : 'One Time Passcode'
            }
            {
               'PersonID': '12345',
               'ActivityRelatedTo' :  'Inbound Call',
               'WasCallerAuthenticated 'Yes',
               'FormOfAuthentication' : 'IVR Authenticated,
            }
       Per user query what is the Form of Auth Conclusion?";


        public async Task<string> CheckFormOfAuthenticationAsync(Kernel? kernel, string? personid, string? query)
        {   // This function is used for verifying step 3.  
            // Activities related to: Inbound call (has to happen) && (Form of Auth: Id Verification OR Form of Auth: KBA)
           #pragma warning disable SKEXP0010

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object", // setting JSON output mode
            };

            KernelArguments arguments = new(executionSettings) { { "query", query }, { "personid", personid } };
            string result = "";
            try
            {
                // KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }) { { "query", query } };
                // Commenting out all the Console.WriteLine as it created confusion against the output that is being logging in the program.cs
                // Console.WriteLine("SK ,- FormOfAuthentication");
                var response = await kernel.InvokePromptAsync(_promptFormOfAuthConclusion, arguments);
                var metadata = response.Metadata;
                //Console.WriteLine($@"FormOfAuthentication:{personid}");
                //Console.WriteLine(response);
                //Console.WriteLine("----------------------");
                //if (metadata != null && metadata.ContainsKey("Usage"))
                //{
                //    var usage = (CompletionsUsage?)metadata["Usage"];
                //    Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                //}
                result = response.GetValue<string>() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }

        public async Task<string> CheckIvrFraudConclusionAsync(Kernel kernel, string personid, string query)
        { //_promptIvrFraudConclusion

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object",
            };

            KernelArguments arguments = new(executionSettings) { { "query", query }, { "personid", personid } };
            string result = "";
            try
            {
                // Commenting out all the Console.WriteLine as it created confusion against the output that is being logging in the program.cs
                //Console.WriteLine("SK ,- CheckActionConclusionIntent");
                var response = await kernel.InvokePromptAsync(_promptIvrFraudConclusion, arguments);
                var metadata = response.Metadata;

                //Console.WriteLine($@"CheckIvrFraudConclusion:{personid}");
                //Console.WriteLine(response);
                //Console.WriteLine("----------------------");
                //if (metadata != null && metadata.ContainsKey("Usage"))
                //{
                //    var usage = (CompletionsUsage?)metadata["Usage"];
                //    Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                //}
                result = response.ToString() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }
     
        public async Task<string> CheckKBAorOTPFraudConclusionAsync(Kernel kernel, string personid, string query)
        { 
#pragma warning disable SKEXP0010

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object",
            };

            KernelArguments arguments = new(executionSettings) { { "query", query }, { "personid", personid } };
            string result = "";
            try
            {
                // Commenting out all the Console.WriteLine as it created confusion against the output that is being logging in the program.cs
                //Console.WriteLine("SK ,- CheckKBAorOTPFraudConclusion");
                var response = await kernel.InvokePromptAsync(_promptKBAorOTPFraudConclusion, arguments);
                var metadata = response.Metadata;
                //Console.WriteLine($@"CheckKBAorOTPFraudConclusion:{personid}");
                //Console.WriteLine(response);
                //Console.WriteLine("----------------------");
                //if (metadata != null && metadata.ContainsKey("Usage"))
                //{
                //    var usage = (CompletionsUsage?)metadata["Usage"];
                //    Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                //}
                result = response.ToString() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }
    }
}

