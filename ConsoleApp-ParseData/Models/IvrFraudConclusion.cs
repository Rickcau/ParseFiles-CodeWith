using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    public class IvrFraudConclusion
    {
        public string? PersonID { get; set; }
        public string? WasCallerAuthenticated { get; set; }
        public string? FormOfAuthentication { get; set; }
        public string? BannerColor { get; set; }
        public string? MarkAsFraud { get; set; }
    }
}
