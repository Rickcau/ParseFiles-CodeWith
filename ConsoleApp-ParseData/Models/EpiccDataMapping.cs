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
    {   // Per User Story 790 we are only supposed to be using
        // UniqueID, HomePhoneNumber, WorkPhoneNumber
        // But the next step is to perform a lookup in the Hospital Address File
        // If we don't have an Address how do we perform the lookup?
        public string? Person_ID;
        public string? Phone_Number;
        public string? Address_01;
        public string? Address_02;
        public string? City;
        public string? State;
        public string? ZipCode;
    }
}
