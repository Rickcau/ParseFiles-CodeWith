using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    public class KBAorOTPFraudConclusion
    {
        public string? PersonID { get; set; }
        public string? WasCallerAuthenticated { get; set; }
        public string? FormOfAuthentication { get; set; }
        public string? ReferenceIdFound { get; set; }
        public string? WasPassedFound { get; set; }
        public string? MarkAsFraud { get; set; }
    }
}
