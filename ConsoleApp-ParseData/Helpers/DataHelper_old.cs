using ConsoleApp_ParseData.Models;
using ConsoleApp_ParseData.Util;
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
        public static IEnumerable<EppicRecords>? inputEppicRecordsInGiactDBNotInHospitalsDB;
        public static IEnumerable<EppicRecords>? inputEppicRecordsNotInGiactDBNorHospitalsDB;
    }

    internal class DataHelper_old
    {
        private BlobHelper _blobHelper;
        private int _countofRecords = 0;
        private bool _usingLocalFiles;
        private UploadedFilesRequest _uploadedFilesRequest;
        private SiebelDataParser _siebelDataParser;
        private EppicDataParser _eppicDataParser;
        private GiactDataParser _giactDataParser;
        private HospitalShelterDataParser _hospitalShelterDataParser;
        private List<HospitalShelterRecords>? _hospitaldataRecords;
        private List<EppicRecords>? _eppicdataRecords;
        private List<SiebelRecords>? _siebeldataRecords;
        private List<GiactRecords>? _giactdataRecords;

        public SiebelDataParser SiebelDataParser        {
            get { return _siebelDataParser; }
        }
        public EppicDataParser EppicDataParser
        {
            get { return _eppicDataParser; }
        }
        public GiactDataParser GiactDataParser
        {
            get { return _giactDataParser; }
        }
        public HospitalShelterDataParser HospitalShelterDataParser
        {
            get { return _hospitalShelterDataParser; }
        }

        public List<HospitalShelterRecords>? HospitalDataRecords
        {
            get { return _hospitaldataRecords; }
        }
        public List<EppicRecords>? EppicDataRecords
        {
            get { return _eppicdataRecords; }
        }
        public List<SiebelRecords>? SiebelDataRecords
        {
            get { return _siebeldataRecords; }
        }
        public List<GiactRecords>? GiactDataRecords
        {
            get { return _giactdataRecords; }
        }

        public DataHelper_old(UploadedFilesRequest uploadedFilesRequest,string blobconnectionstring, bool usingLocalFiles) 
        {
            _usingLocalFiles = usingLocalFiles; 
            _uploadedFilesRequest = uploadedFilesRequest;
            _blobHelper = new BlobHelper()
            {
                Container = "eppic",
                ConnectionString = blobconnectionstring
            };
            _siebelDataParser = new SiebelDataParser();
            _eppicDataParser = new EppicDataParser();
            _giactDataParser = new GiactDataParser();
            _hospitalShelterDataParser = new HospitalShelterDataParser();
        }


        public async Task<string> Intialize()
        {
            var result = "Blank";
            if (_usingLocalFiles)
            {
                try
                {
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Siebel\{_uploadedFilesRequest.SIEBEL_FILENAME}"))
                    {
                        _siebelDataParser.LoadData(stream);
                    }
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Eppic\{_uploadedFilesRequest.EPPIC_FILENAME}"))
                    {
                        _eppicDataParser.LoadData(stream);
                    }
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Giact\{_uploadedFilesRequest.GIACT_FILENAME}"))
                    {
                        _giactDataParser.LoadData(stream);
                    }
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Hospitals\{_uploadedFilesRequest.ADDRESS_FILENAME}"))
                    {
                        _hospitalShelterDataParser.LoadData(stream);
                    }
                    _hospitaldataRecords = _hospitalShelterDataParser.ParseCsv();
                    _eppicdataRecords = _eppicDataParser.ParseCsv();
                    _siebeldataRecords = _siebelDataParser.ParseCsv();
                    _giactdataRecords = _giactDataParser.ParseCsv();
                    Globals.hospitalRecords = _hospitaldataRecords;
                    Globals.eppicRecords = _eppicdataRecords;
                    Globals.siebelRecords = _siebeldataRecords;
                    Globals.giactRecords = _giactdataRecords;
                    Step1_BuildEppicListWithMatchesInHospital();
                    Step1_BuildEppicListWithoutInAgainstHospital();
                    Step2_BuildEppicListWithMatchesInGiactNotInHospitalsDB();
                    Step2_BuildEppicListWithMatchesNotInGiactNorHospitals();
                    result = "Success";
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e.ToString());
                    return result = "Failed to load stream";
                }

            }
            else // Using Azure Storage
            {
                try
                {
                    // Use below to load from Azure Blob Storage note how this uses the Blob Helper
                    _blobHelper.Container = "hospitals";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.ADDRESS_FILENAME ?? String.Empty))
                    {
                        _hospitalShelterDataParser.LoadData(stream);
                    }
                    _blobHelper.Container = "siebel";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.SIEBEL_FILENAME ?? String.Empty))
                    {
                        _siebelDataParser.LoadData(stream);
                    }
                    _blobHelper.Container = "eppic";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.EPPIC_FILENAME ?? String.Empty))
                    {
                        _eppicDataParser.LoadData(stream);
                    }
                    _blobHelper.Container = "giact";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.GIACT_FILENAME ?? String.Empty))
                    {
                        _giactDataParser.LoadData(stream);
                    }
                    _hospitaldataRecords = _hospitalShelterDataParser.ParseCsv();
                    _eppicdataRecords = _eppicDataParser.ParseCsv();
                    _siebeldataRecords = _siebelDataParser.ParseCsv();
                    _giactdataRecords = _giactDataParser.ParseCsv();
                    Globals.hospitalRecords = _hospitaldataRecords;
                    Globals.eppicRecords = _eppicdataRecords;
                    Globals.siebelRecords = _siebeldataRecords;
                    Globals.giactRecords = _giactdataRecords;
                    Step1_BuildEppicListWithMatchesInHospital();
                    Step1_BuildEppicListWithoutInAgainstHospital();
                    Step2_BuildEppicListWithMatchesInGiactNotInHospitalsDB();
                    Step2_BuildEppicListWithMatchesNotInGiactNorHospitals();
                    result = "Success";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return result = "Failed to load stream";
                }
            }
            return result;
        }

        public void Step1_BuildEppicListWithMatchesInHospital()
        {
            // Builds a List of Eppic records that have a match in Hospital DB
            Console.WriteLine($"Total eppicRecords: {Globals.eppicRecords?.Count}");
            if (Globals.hospitalRecords != null)
            {
                Globals.inputEppicRecordsInHospitalDB =
                    from e in Globals.eppicRecords
                    from a in Globals.hospitalRecords
                    where string.Compare(e?.AddressLine1?.Trim(), a?.AddressLine1?.Trim(), ignoreCase: true) == 0
                    where string.Compare(e?.AddressLine2?.Trim(), a?.AddressLine2?.Trim(), ignoreCase: true) == 0
                    where string.Compare(e?.City?.Trim(), a?.City?.Trim(), ignoreCase: true) == 0
                    where string.Compare(e?.State?.Trim(), a?.State?.Trim(), ignoreCase: true) == 0
                    select e;
                Console.WriteLine($"Total inputEppicRecordsInHospitalDB: {Globals.inputEppicRecordsInHospitalDB.Count()}");
            }
            else
            {
                throw new NullReferenceException();
            }


        }

        public void Step1_BuildEppicListWithoutInAgainstHospital()
        {
            Console.WriteLine($"Total Eppic records: {Globals.eppicRecords?.Count}");

            Console.WriteLine($"Total eppicRecords: {Globals.eppicRecords?.Count}");

            // Check if Globals.hospitalRecords is not null
            if (Globals.inputEppicRecordsInHospitalDB != null && Globals.eppicRecords != null)
            {
                // Find Eppic records that do not match in the hospitalRecords
                Globals.inputEppicRecordsNotInHospitalDB =
                    Globals.eppicRecords.Except(Globals.inputEppicRecordsInHospitalDB);

                Console.WriteLine($"Total inputEppicRecordsNotInHospitalDB: {Globals.inputEppicRecordsNotInHospitalDB.Count()}");
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public void Step2_BuildEppicListWithMatchesInGiactNotInHospitalsDB()
        {
            Console.WriteLine($"Total inputEppicRecordsNotInHospitalDB: {Globals.inputEppicRecordsNotInHospitalDB?.Count()}");
            if (Globals.giactRecords != null && Globals.inputEppicRecordsNotInHospitalDB != null)
            {

                Globals.inputEppicRecordsInGiactDBNotInHospitalsDB =
                    (from e in Globals.inputEppicRecordsNotInHospitalDB
                     join a in Globals.giactRecords on e.PersonID equals a.UniqueID
                     where string.Compare(e.AddressLine1?.Trim(), a.AddressCurrentPast?.Trim(), ignoreCase: true) == 0
                     where string.Compare(e.City?.Trim(), a.CityCurrentPast?.Trim(), ignoreCase: true) == 0
                     where string.Compare(e.State?.Trim(), a.StateCurrentPast?.Trim(), ignoreCase: true) == 0
                     where string.Compare(e.ZipCode?.Trim(), a.ZipCodeCurrentPast?.Trim(), ignoreCase: true) == 0
                     select e).Distinct();

                Console.WriteLine($"Total inputEppicRecordsInGiactDBNotInHospitalsDB: {Globals.inputEppicRecordsInGiactDBNotInHospitalsDB.Count()}");
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public void Step2_BuildEppicListWithMatchesNotInGiactNorHospitals()
        {
            Console.WriteLine($"Total inputEppicRecordsNotInHospitalDB: {Globals.inputEppicRecordsNotInHospitalDB?.Count()}");
            if (Globals.inputEppicRecordsInGiactDBNotInHospitalsDB != null && Globals.inputEppicRecordsNotInHospitalDB != null)
            {
                Globals.inputEppicRecordsNotInGiactDBNorHospitalsDB =
                   Globals.inputEppicRecordsNotInHospitalDB.Except(Globals.inputEppicRecordsInGiactDBNotInHospitalsDB);

                Console.WriteLine($"Total inputEppicRecordsNotInGiactDBNorHospitalsDB: {Globals.inputEppicRecordsNotInGiactDBNorHospitalsDB.Count()}");
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public static void LoadDataFiles()
        {

        }
    }
}
