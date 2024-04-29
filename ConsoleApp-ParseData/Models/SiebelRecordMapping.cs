using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class SiebelRecords
    {
        public string? ProgramName;
        public string? PersonID;
        public string? ContactFirstName;
        public string? ContactLastName;

        public string? ActivityCreatedDate;
        public string? ActivityType;
        public string? ActivityCreatedBy;

        [FieldOptional]
        [FieldQuoted(MultilineMode.AllowForBoth)]
        public string? ActivityDescription;
    }
}



