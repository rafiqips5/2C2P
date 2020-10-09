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
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;

namespace FileUploader_2C2P.Helper
{
    public class FileImportHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static dynamic ImportCSVFile(List<string> filePaths)
        {
            APIResponseModel apiResponseModel = null;
            try
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
                        if (string.IsNullOrEmpty(errorReason))
                        {
                            log.InfoFormat("Success Trasanction Id {0}", columns[0]);
                            transactionFileModel = new TransactionFileModel();
                            // assinging value to model from file
                            transactionFileModel.TransactionIdentificator = columns[0];
                            transactionFileModel.Amount = columns[1];
                            transactionFileModel.CurrencyCode = columns[2];
                            transactionFileModel.TransactionDate = Convert.ToDateTime(columns[3]);
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
                    log.Info("Process the records from the file is compeled");
                }
                return PushDataIntoDatabase(transactionFileRecordList, transactionErrorModelList);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception occured while reading the data from the fiel");
                apiResponseModel = new APIResponseModel();
                apiResponseModel.StatusCode = HttpStatusCode.InternalServerError;
                apiResponseModel.Response = ErrorMessages.InternalServerError;
                return apiResponseModel;
            }
        }

        public static dynamic PushDataIntoDatabase(List<TransactionFileModel> transactionFileRecordList, List<TransactionErrorModel> transactionErrorModelList)
        {
            APIResponseModel apiResponseModel = null;
            try
            {
                if (transactionErrorModelList.Count > 0)
                {
                    log.InfoFormat("Wrong data in the file. No need to upload the data in the database {0}", transactionErrorModelList.Count);

                    apiResponseModel = new APIResponseModel();
                    apiResponseModel.StatusCode = HttpStatusCode.BadRequest;
                    apiResponseModel.Response = transactionErrorModelList;
                    return apiResponseModel;
                }
                if (transactionFileRecordList.Count > 0 && transactionErrorModelList.Count == 0)
                {
                    DataTable dataTable = ToDataTable(transactionFileRecordList);
                    if (dataTable.Rows.Count > 0)
                    {
                        log.Info("Opening connection to the database");
                        DatabaseHelper databaseHelper = new DatabaseHelper();
                        if (databaseHelper.TransactionBulkCopyToDataBase(dataTable))
                        {
                            log.Info("Database has been processed successfully");
                            apiResponseModel = new APIResponseModel();
                            apiResponseModel.StatusCode = HttpStatusCode.OK;
                            apiResponseModel.Response = ErrorMessages.Success;
                            return apiResponseModel;
                        }
                        else
                        {
                            apiResponseModel = new APIResponseModel();
                            apiResponseModel.StatusCode = HttpStatusCode.InternalServerError;
                            apiResponseModel.Response = ErrorMessages.InternalServerError;
                        }
                    }
                    else
                    {
                        log.InfoFormat("No data found while converting List to DataTable {0}", dataTable.Rows.Count);
                        apiResponseModel = new APIResponseModel();
                        apiResponseModel.StatusCode = HttpStatusCode.BadRequest;
                        apiResponseModel.Response = ErrorMessages.NoContent;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception occurred while inserting data into database {0}", ex);
                apiResponseModel = new APIResponseModel();
                apiResponseModel.StatusCode = HttpStatusCode.InternalServerError;
                apiResponseModel.Response = ErrorMessages.InternalServerError; ;
            }
            return apiResponseModel;
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

                if (currencyData.Count() == 0)
                {
                    failureReason = failureReason + "," + ErrorMessages.InvalidCurrencyCode;
                }

                if (string.IsNullOrEmpty(dateTime))
                {
                    failureReason = failureReason + "," + ErrorMessages.NotFoundDateTime;
                }

                if (DateTime.TryParseExact(dateTime, "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
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
            try
            {
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
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception occured while converting the data from list to data table :{0}", ex);
                return dataTable;
            }
            return dataTable;
        }

        public static dynamic ImportXMFiles(List<string> filePath)
        {
            log.Info("Start Processing the XML files");
            List<TransactionFileModel> transactionFileModelList = new List<TransactionFileModel>();
            List<TransactionErrorModel> transactionErrorModelList = new List<TransactionErrorModel>();
            APIResponseModel apiResponseModel = null;
            string xsdFileName = ConfigurationManager.AppSettings["TransactionSchema"];

            if (!string.IsNullOrEmpty(xsdFileName))
            {
                foreach (string path in filePath)
                {
                    if (ValidateXML(path, xsdFileName))
                    {
                        log.InfoFormat("XML validated Successfully for the file {0}",path);
                        transactionFileModelList = XMLtoList(path, out transactionErrorModelList);
                        apiResponseModel = PushDataIntoDatabase(transactionFileModelList, transactionErrorModelList);
                    }
                    else
                    {
                        log.InfoFormat("Invalid XML file. Unable to Validate the XML file");
                        apiResponseModel = new APIResponseModel();
                        apiResponseModel.StatusCode = HttpStatusCode.BadRequest;
                        apiResponseModel.Response = ErrorMessages.InvalidXMLFile;
                        
                    }
                }
            }
            else
            {
                log.InfoFormat("XSD file is not available inside the bin\\XSD configuration {0}", xsdFileName);
                apiResponseModel = new APIResponseModel();
                apiResponseModel.StatusCode = HttpStatusCode.BadRequest;
                apiResponseModel.Response = ErrorMessages.InternalServerError;
            }
            return apiResponseModel;
        }

        public static bool ValidateXML(string xmlFilePath,string xsdFileName)
        {
            try
            {
                var localPath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
                XmlSchemaSet schema = new XmlSchemaSet();
                log.InfoFormat("Path of the XSD files {0}", localPath + "/XSD/" + xsdFileName);
                schema.Add("", localPath + "/XSD/" + xsdFileName);
                XmlReader rd = XmlReader.Create(xmlFilePath);
                XDocument doc = XDocument.Load(rd);
                doc.Validate(schema, ValidationEventHandler);
                rd.Close();
            }
            catch(XmlException xe)
            {
                log.ErrorFormat("Xml exception occured :{0}", xe);
                return false;
            }
            catch(FileNotFoundException ex)
            {
                log.ErrorFormat("File not found exception {0}",ex);
                return false;
            }
            catch(Exception ex)
            {
                log.ErrorFormat("Un handled exception occured while validating the XML {0}",ex);
                return false;
            }
            return true;
        }

        public static List<TransactionFileModel> XMLtoList(string xmlFilePath,out List<TransactionErrorModel> transactionErrorModelList)
        {
            List<TransactionFileModel> transactionFileModelList= new List<TransactionFileModel>();
            transactionErrorModelList = new List<TransactionErrorModel>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/Transactions/Transaction");
            foreach (XmlNode node in nodeList)
            {
                string transId = string.Empty;
                string transDate = string.Empty;
                string amount = string.Empty;
                string currencyCode = string.Empty;
                string status = string.Empty;
                string failureReason = string.Empty;

                transId = node.Attributes[0].InnerText;
                transDate = node.ChildNodes[0].InnerText;
                amount = node.ChildNodes[1].ChildNodes[0].InnerText;
                currencyCode = node.ChildNodes[1].ChildNodes[1].InnerText;
                status = node.ChildNodes[2].InnerText;

               failureReason = ValidateFileInputs(transId, amount, currencyCode, transDate, status);

                if (string.IsNullOrEmpty(failureReason))
                {
                    TransactionFileModel transactionFileModel = new TransactionFileModel();
                    transactionFileModel.TransactionIdentificator = transId;
                    transactionFileModel.TransactionDate = Convert.ToDateTime(transDate);
                    transactionFileModel.Amount = amount;
                    transactionFileModel.CurrencyCode = currencyCode;
                    transactionFileModel.Status = status;
                    transactionFileModelList.Add(transactionFileModel);
                }
                else
                {
                    TransactionErrorModel transactionErrorModel = new TransactionErrorModel();
                    transactionErrorModel.TransactionIdentificator = transId;
                    transactionErrorModel.FailureReason = failureReason;
                    transactionErrorModelList.Add(transactionErrorModel);
                }
            }

            return transactionFileModelList;
        }
        public static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {

            XmlSeverityType type = XmlSeverityType.Warning;
            if (Enum.TryParse<XmlSeverityType>("Error", out type))
            {
                if (type == XmlSeverityType.Error) throw new Exception(e.Message);
            }
        }

    }
}