using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FileUploader_2C2P_API.Models.Response;

namespace FileUploader_2C2P.Helper
{
    public class DatabaseHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        readonly string sqlCon = string.Empty;
        readonly string destinationTableName = string.Empty;
        public DatabaseHelper()
        {
            try
            {
                sqlCon = ConfigurationManager.AppSettings["SqlCon"];
                destinationTableName = ConfigurationManager.AppSettings["DestinationTableName"];
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error occured while fetching data from configuraiton {0}", ex);
            }
        }

        public bool TransactionBulkCopyToDataBase(DataTable dataTable)
        {
            log.InfoFormat("Strart Inserting data into Database , Record Count is {0}", dataTable.Rows.Count);
            try
            {
                if (ValidateDatabaseConfiguration())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(sqlCon))
                    {
                        SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlConnection);
                        sqlBulkCopy.DestinationTableName = destinationTableName;

                        // columns mappings
                        sqlBulkCopy.ColumnMappings.Add("TransactionIdentificator", "TransactionIdentificator");
                        sqlBulkCopy.ColumnMappings.Add("Amount", "Amount");
                        sqlBulkCopy.ColumnMappings.Add("CurrencyCode", "CurrencyCode");
                        sqlBulkCopy.ColumnMappings.Add("TransactionDate", "TransactionDate");
                        sqlBulkCopy.ColumnMappings.Add("Status", "Status");

                        sqlConnection.Open();
                        sqlBulkCopy.WriteToServer(dataTable);
                        log.InfoFormat("Data Processed successfully. Reocrd count is {0}", dataTable.Rows.Count);
                    }
                }
            }
            catch (SqlException ex)
            {
                log.ErrorFormat("Sql exception occured {0}", ex);
                return false;
            }
            catch (IndexOutOfRangeException ex)
            {
                log.ErrorFormat("Input error occured {0}", ex);
                return false;

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Un Handled error occured {0}", ex);
                return false;
            }
            return true;
        }

        public List<GetTransactionResponse> GetTransaactionByCurrencyType(string currencyType)
        {
            log.Info("Get Transaction from database started");
            List<GetTransactionResponse> getTransactionResponsesList = null;
            string[] statusDesc = ConfigurationManager.AppSettings["XMLStatus"].Split(',');
            try
            {
                if (ValidateDatabaseConfiguration())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(sqlCon))
                    {
                        using (SqlCommand sqlCommand = new SqlCommand("PROC_GET_TRANSACTION_BY_CURRENCY", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.Parameters.AddWithValue("@i_CurrencyCode", currencyType);
                            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();


                            getTransactionResponsesList = new List<GetTransactionResponse>();
                            while (sqlDataReader.Read())
                            {
                                GetTransactionResponse getTransactionResponse = new GetTransactionResponse();
                                getTransactionResponse.Id = sqlDataReader["TransactionIdentificator"].ToString();
                                getTransactionResponse.Payment = string.Format("{0} {1}", sqlDataReader["Amount"], sqlDataReader["CurrencyCode"]);
                                string status = sqlDataReader["Status"].ToString();
                                getTransactionResponse.Status = GetStatusMappingsForDescription(status);
                                getTransactionResponsesList.Add(getTransactionResponse);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Un handled exception occured : {0}", ex);
                return getTransactionResponsesList;
            }
            return getTransactionResponsesList;
        }

        public List<GetTransactionResponse> GetTransaactionBySatus(string status)
        {
            log.Info("Get Transaction from database started");
            List<GetTransactionResponse> getTransactionResponsesList = null;
            try
            {
                if (ValidateDatabaseConfiguration())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(sqlCon))
                    {
                        using (SqlCommand sqlCommand = new SqlCommand("PROC_GET_TRANSACTION_BY_STATUS", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.Parameters.AddWithValue("@i_STATUS", status);
                            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();


                            getTransactionResponsesList = new List<GetTransactionResponse>();
                            while (sqlDataReader.Read())
                            {
                                GetTransactionResponse getTransactionResponse = new GetTransactionResponse();
                                getTransactionResponse.Id = sqlDataReader["TransactionIdentificator"].ToString();
                                getTransactionResponse.Payment = string.Format("{0} {1}", sqlDataReader["Amount"], sqlDataReader["CurrencyCode"]);

                                string statusDesc = sqlDataReader["Status"].ToString();
                                getTransactionResponse.Status = GetStatusMappingsForDescription(statusDesc);
                                getTransactionResponsesList.Add(getTransactionResponse);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Un handled exception occured : {0}", ex);
                return getTransactionResponsesList;
            }
            return getTransactionResponsesList;
        }

        public List<GetTransactionResponse> GetTransaactionByDate(DateTime startDate, DateTime endDate)
        {
            log.Info("Get Transaction from database started");
            List<GetTransactionResponse> getTransactionResponsesList = null;
            try
            {
                if (ValidateDatabaseConfiguration())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(sqlCon))
                    {
                        using (SqlCommand sqlCommand = new SqlCommand("PROC_GET_TRANSACTION_BY_DATE", sqlConnection))
                        {
                            sqlConnection.Open();
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.Parameters.AddWithValue("@i_StartDate", startDate);
                            sqlCommand.Parameters.AddWithValue("@i_EndDate", startDate);
                            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();


                            getTransactionResponsesList = new List<GetTransactionResponse>();
                            while (sqlDataReader.Read())
                            {
                                GetTransactionResponse getTransactionResponse = new GetTransactionResponse();
                                getTransactionResponse.Id = sqlDataReader["TransactionIdentificator"].ToString();
                                getTransactionResponse.Payment = string.Format("{0} {1}", sqlDataReader["Amount"], sqlDataReader["CurrencyCode"]);
                                string status = sqlDataReader["Status"].ToString();
                                getTransactionResponse.Status = GetStatusMappingsForDescription(status);
                                getTransactionResponsesList.Add(getTransactionResponse);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Un handled exception occured : {0}", ex);
                return getTransactionResponsesList;
            }
            return getTransactionResponsesList;
        }

        public bool ValidateDatabaseConfiguration()
        {
            log.Info("Validating Configuration");
            try
            {
                if (!string.IsNullOrEmpty(sqlCon) && !string.IsNullOrEmpty(destinationTableName))
                {
                    log.InfoFormat("Validation Configuration successfully");
                    return true;
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Exception Occured while fetching the data from configuraiton {0}", ex);
                return false;
            }
            return false;
        }

        public char GetStatusMappingsForDescription(string status)
        {
            char charStatus;
            if (status.ToLower() == "failed")
            {
                charStatus = 'R';
            }
            else if (status.ToLower() == "fineshed")
            {
                charStatus = 'D';
            }
            else
            {
                charStatus = Convert.ToChar(status.ToString().Substring(0, 1).ToUpper());
            }

            return charStatus;
        }
    }
}