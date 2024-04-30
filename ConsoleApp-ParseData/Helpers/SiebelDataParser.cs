using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ConsoleApp_ParseData.Models;
using FileHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp_ParseData.Helpers
{
    internal class SiebelDataParser
    {
        private List<SiebelCallNotes>? _siebelCallNotes;
        private int _countofCallNotes = 0;

        public int CountOfCallNotes
        {
            get
            {
                return _countofCallNotes;
            }
        }
        public List<SiebelCallNotes> CallNotes
        {
            get
            {
                if (_siebelCallNotes == null)
                {
                    _siebelCallNotes = new List<SiebelCallNotes>();
                }

                return _siebelCallNotes;
            }
        }
        public List<SiebelRecords> ParseCsv(string filePath)
        {
            // Example: filePath = "C:\\temp\\Data\\Siebel\\NOV7_activity_v2.csv"
            var engineSiebel = new FileHelperEngine<SiebelRecords>();
            var recordsSiebel = engineSiebel.ReadFile(filePath);
            List<SiebelRecords> siebelRecordsList = recordsSiebel.ToList();

            // Now let's filter all the records that actually have ActivityDescriptions and copy those into it's own list for use later
            _siebelCallNotes = siebelRecordsList
                 .Where(record => record.ActivityDescription != "")
                 .Select(record => new SiebelCallNotes { PersonID = record.PersonID, CallNotes = record.ActivityDescription })
                 .ToList();
            // store the count of items that actually have CallNotes
            _countofCallNotes = _siebelCallNotes.Count();
            return siebelRecordsList;
        }
        public void PrintSiebelRecords(List<SiebelRecords> recordsSiebel)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordSiebel in recordsSiebel)
            {
                count++;
                Console.WriteLine($@"Record# {count} ProgramName: {recordSiebel.ProgramName} PersonID: {recordSiebel.PersonID} ContactFirstName: {recordSiebel.ContactFirstName} ContactLastName: {recordSiebel.ContactLastName} ActivityCreatedDate: {recordSiebel.ActivityCreatedDate} ActivityType: {recordSiebel.ActivityType} ActivityCreatedBy: {recordSiebel.ActivityCreatedBy} ActivityDescription: {recordSiebel.ActivityDescription}");
                if (recordSiebel.ActivityDescription != "")
                {
                    Console.WriteLine("\n\n\n");
                    Console.WriteLine("Activity Description has data!!!");
                    Console.WriteLine("***************************");
                    Console.WriteLine(recordSiebel.ActivityDescription);
                    Console.WriteLine("***************************\n\n");
                }
            }
        }

        public void PrintSiebelCallNoteRecords(List<SiebelCallNotes> recordsSiebel)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordSiebel in recordsSiebel)
            {
                count++;
                Console.WriteLine($@"Record# {count} PersonID: {recordSiebel.PersonID} CallNotes: {recordSiebel.CallNotes}");
            }
        }

        public SiebelCallNotes? FindSiebelCallNoteByPersonID(string personID)  // returns only one callnote note sure if this is valid as it appears a personID can have multuple callnotes
        {
            // this is one we need clarification on.
            if (_siebelCallNotes == null)
                return null; // If _siebelCallNotes is null, return null

            return _siebelCallNotes.FirstOrDefault(note => note.PersonID == personID);
        }

        public List<SiebelCallNotes> FindAllSiebelCallNotesByPersonID(string personID)
        {
            if (_siebelCallNotes == null)
                return new List<SiebelCallNotes>(); // If _siebelCallNotes is null, return an empty list

            return _siebelCallNotes.Where(note => note.PersonID == personID).ToList();
        }
    }
}

