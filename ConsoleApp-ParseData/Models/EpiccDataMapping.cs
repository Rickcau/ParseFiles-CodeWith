using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class EpiccRecords
    {
        public string? UniqueID;
        public string? AddressLine1;
        public string? AddressLine2;
        public string? City;
        public string? State;
        public string? ZipCode;
    }
}
