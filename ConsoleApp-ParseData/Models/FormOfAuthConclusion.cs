using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{ 
    public class FormOfAuthConclusion
    {
        public string? PersonID { get; set; }
        public string? ActivityRelatedTo { get; set; }
        public string? WasCallerAuthenticated { get; set; }
        public string? FormOfAuthentication { get; set; }
    }
}
