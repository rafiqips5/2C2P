using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUploader_2C2P_API.Models
{
    public class GetTransactionByDatesRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}