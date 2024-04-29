using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ConsoleApp_ParseData.Models;

namespace ConsoleApp_ParseData.Helpers
{
    internal class SiebelDataParser
    {
        public List<SiebelData> ParseCsv(string filePath)
        {
            List<SiebelData> siebelDataList = new List<SiebelData>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                // Skip header
                reader.ReadLine();

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');

                    if (fields.Length >= 8)
                    {
                        SiebelData siebelData = new SiebelData();
                        siebelData.ProgramName = fields[0];
                        siebelData.PersonID = int.Parse(fields[1]);
                        siebelData.ContactFirstName = fields[2];
                        siebelData.ContactLastName = fields[3];
                        siebelData.ActivityCreatedDate = DateTime.ParseExact(fields[4], "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        siebelData.ActivityType = fields[5];
                        siebelData.ActivityCreatedBy = fields[6];
                        siebelData.ActivityDescription = fields[7];

                        siebelDataList.Add(siebelData);
                    }
                    else
                    {
                        Console.WriteLine("Invalid data format");
                    }
                }
            }

            return siebelDataList;
        }
    }
}

