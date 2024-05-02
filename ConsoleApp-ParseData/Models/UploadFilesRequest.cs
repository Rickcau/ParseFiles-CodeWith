using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_ParseData.Models
{
    public class UploadedFilesRequest
    {
        public string? GIACT_FILENAME { get; set; }
        public string? SIEBEL_FILENAME { get; set; }
        public string? EPPIC_FILENAME { get; set; }
        public string? ADDRESS_FILENAME { get; set; }
    }
}
