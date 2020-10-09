using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUploader_2C2P_API.Models.Response
{
    public class TransactionErrorModel
    {
        public string TransactionIdentificator { get; set; }
        public string FailureReason { get; set; }
    }
}