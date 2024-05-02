using ConsoleApp_ParseData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Helpers
{
    static class Globals
    {
        public static List<HospitalShelterRecords>? hospitalRecords;
        public static List<SiebelRecords>? siebelRecords;
        public static List<EppicRecords>? eppicRecords;
        public static List<GiactRecords>? giactRecords;
        public static IEnumerable<EppicRecords>? inputEppicRecordsInHospitalDB;
        public static IEnumerable<EppicRecords>? inputEppicRecordsNotInHospitalDB;
    }

    public static class DataHelper
    {
        public static void Step1_BuildEppicListWithMatchesInHospital()
        {
            // Builds a List of Eppic records that have a match in Hospital DB
            Console.WriteLine($"Total Eppic records: {Globals.eppicRecords?.Count}");
            if (Globals.hospitalRecords != null)
            {
                Globals.inputEppicRecordsInHospitalDB =
                    from e in Globals.eppicRecords
                    join a in Globals.hospitalRecords
                        on new { e.AddressLine1, e.AddressLine2, e.City, e.State }
                        equals new { a?.AddressLine1, a?.AddressLine2, a?.City, a?.State }
                    select e;
                Console.WriteLine($"Recs that match in the address DB: {Globals.inputEppicRecordsInHospitalDB.Count()}");
            }
            else
            {
                // Handle the case when Globals.hospitalRecords is null
                // For example, log a warning or provide a default value
            }
        }

        public static void Step1_BuildEppicListWithoutInAgainstHospital()
        {
            Console.WriteLine($"Total Eppic records: {Globals.eppicRecords?.Count}");

            // Check if Globals.hospitalRecords is not null
            if (Globals.hospitalRecords != null && Globals.eppicRecords != null)
            {
                // Find Eppic records that do not match in the hospitalRecords
                Globals.inputEppicRecordsNotInHospitalDB =
                    Globals.eppicRecords.Except(
                        from e in Globals.eppicRecords
                        join a in Globals.hospitalRecords
                            on new { e.AddressLine1, e.AddressLine2, e.City, e.State }
                            equals new { a?.AddressLine1, a?.AddressLine2, a?.City, a?.State }
                        select e);

                Console.WriteLine($"Recs not matching in the address DB: {Globals.inputEppicRecordsNotInHospitalDB.Count()}");
            }
            else
            {
                // Handle the case when Globals.hospitalRecords is null
                // For example, log a warning or provide a default value
            }
        }
    }
}
