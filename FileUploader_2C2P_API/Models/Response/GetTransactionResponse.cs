using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUploader_2C2P_API.Models.Response
{
    public class GetTransactionResponse
    {
        public string Id { get; set; }
        public string Payment { get; set; }
        public char Status { get; set; }
    }
}