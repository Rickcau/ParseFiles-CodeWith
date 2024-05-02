using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ConsoleApp_ParseData.Models;
using FileHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp_ParseData.Helpers
{
    internal class GiactDataParser
    {
        private int _countofRecords = 0;
        public List<GiactRecords>? giactRecordsList;

        public void LoadData(StreamReader reader)
        {
            var engineGiact = new FileHelperEngine<GiactRecords>();
            var recordsGiact = engineGiact.ReadStream(reader);
            this.giactRecordsList = recordsGiact.ToList();
        }
        public int CountOfRecords
        {
            get
            {
                return _countofRecords;
            }
        }

        public List<GiactRecords> ParseCsv()
        {
            return giactRecordsList ?? new List<GiactRecords>() ;
        }
        public void PrintGiactRecords(List<GiactRecords> recordsGiact)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordGiact in recordsGiact)
            {
                count++;
                Console.WriteLine($@"Record# {count} UniqueID: {recordGiact.UniqueID} AddressLine1: {recordGiact.AddressLine1} City: {recordGiact.City} State: {recordGiact.State} ZipCode: {recordGiact.ZipCode} AddressCurrentPast: {recordGiact.AddressCurrentPast} CityCurrentPast: {recordGiact.CityCurrentPast} Status: {recordGiact.Status}");
            }
        }

        public GiactRecords? FindGiactUniqueID(string uniqueid, List<GiactRecords> giactrecords)  
        {
            return giactrecords.FirstOrDefault(record => record.UniqueID == uniqueid);
        }

        public GiactRecords? FindGiactByAddressLine1(string address, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.AddressLine1 == address);
        }

        public GiactRecords? FindGiactByAddressCurrentPast(string addresscurrentpast, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.AddressCurrentPast == addresscurrentpast);
        }

    }
}

