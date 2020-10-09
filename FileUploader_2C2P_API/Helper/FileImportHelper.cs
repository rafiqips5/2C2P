using FileUploader_2C2P.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;
using FileUploader_2C2P_API.Helper;
using FileUploader_2C2P_API.Models.Response;
using System.Data;
using System.Reflection;
using System.Net;

namespace FileUploader_2C2P.Helper
{
    public class FileImportHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static dynamic ImportCSVFile(List<string> filePaths)
        {
            
            log.Info("Inside Method - File Process is started");
            List<TransactionFileModel> transactionFileRecordList = new List<TransactionFileModel>();
            List<TransactionErrorModel> transactionErrorModelList = new List<TransactionErrorModel>(); 

            TransactionFileModel transactionFileModel = null;
            TransactionErrorModel transactionErrorModel = null;

            string errorReason = string.Empty;
            foreach (string path in filePaths)
            {
                transactionFileRecordList = new List<TransactionFileModel>();
                string[] lines = File.ReadAllLines(path);
                log.InfoFormat("Total records available for process is {0} ", lines.Count());
                foreach (string line in lines)
                {
                    string[] columns = line.Split(',');

                    log.InfoFormat("Start processing record for {0}", columns[0]);
                    errorReason = ValidateFileInputs(columns[0], columns[1], columns[2], columns[3], columns[4]);
                    if(string.IsNullOrEmpty(errorReason))
                    {
                        transactionFileModel = new TransactionFileModel();
                        // assinging value to model from file
                        transactionFileModel.TransactionIdentificator = columns[0];
                        transactionFileModel.Amount = columns[1];
                        transactionFileModel.CurrencyCode = columns[2];
                        transactionFileModel.TransactionDate = columns[3];
                        transactionFileModel.Status = columns[4];
                        // adding good record to  insert into list
                        transactionFileRecordList.Add(transactionFileModel);

                    }
                    else
                    {
                        transactionErrorModel = new TransactionErrorModel();
                        transactionErrorModel.TransactionIdentificator = columns[0];
                        transactionErrorModel.FailureReason = errorReason;
                        // adding bad record to show error reason
                        transactionErrorModelList.Add(transactionErrorModel);
                    }
                }
            }

            if(transactionErrorModelList.Count> 0)
            {
                log.InfoFormat("Wrong data in the file. No need to upload the data in the database {0}",transactionErrorModelList.Count);
                return transactionErrorModelList;
            }
            else
            {             
                if(transactionFileRecordList.Count>0)
                {
                    DataTable dataTable = ToDataTable(transactionFileRecordList);
                   
                }
                return HttpStatusCode.OK;
            }
        }
        public static List<Currency> LoadCurrency()
        {
            log.Info("Inside Method : Load Currnecy");
            List<Currency> currencyList = null;
            try
            {

                var currency = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(ci => ci.LCID).Distinct().Select(id => new RegionInfo(id))
                 .GroupBy(r => r.ISOCurrencySymbol)
                 .Select(g => g.First())
                 .Select(r => new
                 {
                     r.ISOCurrencySymbol,
                     r.CurrencyEnglishName,
                     r.CurrencySymbol
                 }).ToList();

                log.InfoFormat("Currency 3 digits symbol count : {0}", currency.Count);
                if (currency.Count > 0)
                {
                    currencyList = new List<Currency>();
                    foreach (var data in currency)
                    {
                        Currency _currency = new Currency();
                        _currency.CurrencyEnglishName = data.CurrencyEnglishName;
                        _currency.CurrencySymbol = data.CurrencySymbol;
                        _currency.ISOCurrencySymbol = data.ISOCurrencySymbol;
                        currencyList.Add(_currency);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exeception occured while fetching the currency symbol :{0}", ex);
            }
            return currencyList;
        }
        
        public static string ValidateFileInputs(string transaction, string amount, string currencyCode, string dateTime, string status)
        {
            string failureReason = string.Empty;
            string[] statusDesc = ConfigurationManager.AppSettings["Status"].Split(',');

           
            List<Currency> currencyLists = null;

            try
            {
                if (string.IsNullOrEmpty(transaction))
                {
                    failureReason = ErrorMessages.NotFoundTransaction;
                }
                if (transaction.Length > 50)
                {
                    failureReason = ErrorMessages.InvalidTrasaction;
                }

                if (string.IsNullOrEmpty(amount))
                {
                    failureReason = failureReason + "," + ErrorMessages.NotFoundAmount;
                }
                if (!Decimal.TryParse(amount, out decimal value))
                {
                    failureReason = failureReason + "," + ErrorMessages.InvalidAmount;
                }

                if (string.IsNullOrEmpty(currencyCode))
                {
                    failureReason = failureReason + "," + ErrorMessages.NotFoundCurrency;
                }

                currencyLists = LoadCurrency();
                var currencyData = currencyLists.Where(symbol => symbol.ISOCurrencySymbol == currencyCode).ToList();

                if (currencyCode.Count() == 0)
                {
                    failureReason = failureReason + "," + ErrorMessages.InvalidCurrencyCode;
                }

                if (string.IsNullOrEmpty(dateTime))
                {
                    failureReason = failureReason + "," + ErrorMessages.NotFoundDateTime;
                }

                if (!DateTime.TryParseExact(dateTime, "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
                {
                    failureReason = failureReason + "," + ErrorMessages.InValidDateFormat;
                }

                if (string.IsNullOrEmpty(status))
                {
                    failureReason = failureReason + "," + ErrorMessages.NotFoundStatusDesc;
                }

                if (!statusDesc.Contains(status.ToLower()))
                {
                    failureReason = failureReason + "," + ErrorMessages.InvalidStatusDesc;
                }
            }
            catch (Exception)
            {

                throw;
            }

            return failureReason;
        }

        public static bool DirectoryCheck(string filePath)
        {
            try
            {
                log.Info("Checking directory");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                    log.InfoFormat("Directory : {0} Created successfully", filePath);
                    return true;
                }
                else
                {
                    log.InfoFormat("Directory path : {0} is available", filePath);
                    return true;
                }
            }
            catch (IOException ex)
            {
                log.ErrorFormat("Exception occured : {0}", ex);
                return false;
            }

        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}