using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    public class RecordsResults
    {
        
        public string? PersonID { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public int StepRuleToApply { get; set; }
        public int? LastStepCompleted { get; set; }
        public bool MoveToNext { get; set; }
        public bool? MarkedAsFraud { get; set; }
        public string? Status { get; set; }
        public bool Completed { get; set; }
        public bool? Step1HospitalMatch { get; set; }
        public bool? Step2GiactMatch { get; set; }
        public bool? Step3Part1_InboundCallAuthNo { get; set; }
        public bool? Step3Part2_InboundCallAuthYes { get; set; }
        public bool? Step3Part3_ComplexCheck { get; set; }
        public bool Step3aPassedOTPPhoneGiact { get; set; }
        
    }
}
