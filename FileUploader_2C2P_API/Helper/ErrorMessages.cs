using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUploader_2C2P_API.Helper
{
    public static class ErrorMessages
    {
        public static string InValidFileType = "Invalid File Type. Allowed file type are .CSV and .XML";

        public static string NotFoundFiles = "File Not Found";

        public static string BadRecordsDesc = "{0} is Invalid due to {}";

        public static string InvalidTrasaction = "The length of the record is more than 50. Maximum allowed char is 50";

        public static string NotFoundTransaction = "Transaction Identificator is not found";

        public static string BadInput = "You have supplied bad Input";

        public static string Success = "File Processed Successfully";

        public static string InternalServerError = "Techincal difficulties. Please conctact Administrator";

        public static string InvalidStatusDesc = "Invalid Satus description.Possibles Values are Approved, Rejected and Done";

        public static string NotFoundStatusDesc = "Status description is not found";

        public static string InValidDateFormat = "Invalid Date fomrat. Possible Date format is yyyy-MM-ddThh:mm:ss e.g. 2019-01-23T13:45:10";

        public static string NotFoundDateTime = "Date Time is empty";

        public static string InvalidCurrencyCode = "Invalid Currency code. sample currency code for American dollor is USD";

        public static string InvalidAmount = "Invalid amount. The Possible value is decimal e.g. 100.00";

        public static string NotFoundAmount = "Invalid amount. Value not supplied";

        public static string NotFoundCurrency = "Invalid Currency code. Value not supplied";

        public static string NoContent = "No Data Found";

        public static string InvalidXMLFile = "File contains invalid data format. Please check the XML tags";
    }
}