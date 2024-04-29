using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    internal class SiebelData
    {
        public string? ProgramName { get; set; }
        public int PersonID { get; set; }
        public string? ContactFirstName { get; set; }
        public string? ContactLastName { get; set; }
        public DateTime ActivityCreatedDate { get; set; }
        public string? ActivityType { get; set; }
        public string? ActivityCreatedBy { get; set; }
        public string? ActivityDescription { get; set; }
    }
}
