using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    public class SiebelCallNotes
    {
        public string? PersonID { get; set; }
        public string? CallNotes { get; set; }
        public DateTime? ActivityCreatedDate { get; set; }
    }

}
