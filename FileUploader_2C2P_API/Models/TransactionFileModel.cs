using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUploader_2C2P.Models
{
    public class TransactionFileModel
    {
        public string TransactionIdentificator { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransactionDate { get; set; }
        public string Status { get; set; }
    }
}