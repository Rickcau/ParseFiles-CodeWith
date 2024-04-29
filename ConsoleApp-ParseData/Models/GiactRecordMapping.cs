﻿using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class GiactRecords
    {
        public string? UniqueID;
        public string? AddressLine1;
        public string? City;
        public string? State;
        public string? ZipCode;
        public string? Country;
        public string? ItemReferenceID;
        public string? CreatedDate;
    }
}
