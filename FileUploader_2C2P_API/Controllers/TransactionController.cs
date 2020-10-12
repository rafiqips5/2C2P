using FileUploader_2C2P.Helper;
using FileUploader_2C2P_API.Helper;
using FileUploader_2C2P_API.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using FileUploader_2C2P_API.Models;

namespace FileUploader_2C2P_API.Controllers
{
    public class TransactionController : ApiController
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        DatabaseHelper databaseHelper = null;
        // GET api/<controller>
        [Route("api/Transaction/TransactionByCurrency/{currency}")]
        [HttpGet]
        public HttpResponseMessage GetTransactionByCurrency(string currency)
        {
            databaseHelper = new DatabaseHelper();
            if (string.IsNullOrEmpty(currency))
            {
                log.InfoFormat("currnecy is empty");
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ErrorMessages.NotFoundCurrency);
            }
            if (currency.Length > 3)
            {
                log.InfoFormat("Bad input {0}", currency);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.BadInput);
            }
            List<GetTransactionResponse> getTransactionResponse = databaseHelper.GetTransaactionByCurrencyType(currency);
            return Request.CreateResponse(getTransactionResponse);
        }
        [HttpGet]
        [Route("api/Transaction/TransactionByStatus/{status}")]
        public HttpResponseMessage GetTransactionByStatus(string status)
        {
            databaseHelper = new DatabaseHelper();
            string[] statusArray = new string[3];
            string[] statusXMLArray = new string[3];
            List<GetTransactionResponse> getTransactionResponse = null;

            try
            {
                if (string.IsNullOrEmpty(status))
                {
                    log.InfoFormat("status is empty");
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ErrorMessages.NotFoundStatusDesc);
                }
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CSVStatus"]) && (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["XMLStatus"])))
                {
                    statusArray = ConfigurationManager.AppSettings["CSVStatus"].Split(',');
                    statusXMLArray = ConfigurationManager.AppSettings["XMLStatus"].Split(',');
                    if (statusArray.Contains(status.ToLower()) || statusXMLArray.Contains(status.ToLower()))
                    {
                        log.InfoFormat("Status : {0} is valid", status);
                        getTransactionResponse = databaseHelper.GetTransaactionBySatus(status);
                        return Request.CreateResponse(getTransactionResponse);
                    }
                    else
                    {
                        log.InfoFormat("Invalid status Input {0}", status);
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.InvalidStatusDesc);
                    }
                }
                else
                {
                    log.InfoFormat("Invalid Configuration. Status in missing in appSeetings section");
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ErrorMessages.InternalServerError);
                }
            }
            catch (WebException ex)
            {
                log.ErrorFormat("Web exception occured {0} ", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ErrorMessages.InternalServerError);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unhandled exception occured {0} ", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ErrorMessages.InternalServerError);
            }

        }
        [HttpPost]
        [Route("api/Transaction/TransactionByDate")]
        public HttpResponseMessage GetTransactionByDate([FromBody] GetTransactionByDatesRequest getTransactionByDatesRequest)
        {
            databaseHelper = new DatabaseHelper();
            string[] statusArray = new string[3];
            List<GetTransactionResponse> getTransactionResponse = null;

            // DateTime stDate = DateTime.ParseExact(getTransactionByDatesRequest.startDate, "yyyy-mm-dd HH:mm:ss.tt", null);
            //DateTime toDate = DateTime.ParseExact(getTransactionByDatesRequest.toDateTime, "yyyy-mm-dd HH:mm:ss.tt", null);
            try
            {
                if (string.IsNullOrEmpty(getTransactionByDatesRequest.StartDate.ToString()) && string.IsNullOrEmpty(getTransactionByDatesRequest.EndDate.ToString()))
                {
                    log.InfoFormat("start date and end date is empty");
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ErrorMessages.NotFoundDateTime);
                }
                if (getTransactionByDatesRequest.StartDate <= getTransactionByDatesRequest.StartDate)
                {
                    log.InfoFormat("start date : {0} and endDate {1} is valid", getTransactionByDatesRequest.StartDate, getTransactionByDatesRequest.EndDate);
                    getTransactionResponse = databaseHelper.GetTransaactionByDate(getTransactionByDatesRequest.StartDate, getTransactionByDatesRequest.EndDate);
                    return Request.CreateResponse(getTransactionResponse);
                }
                else
                {
                    log.InfoFormat("start date : {0} and endDate {1} is valid", getTransactionByDatesRequest.StartDate, getTransactionByDatesRequest.EndDate);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.InvalidDateRange);
                }
            }
            catch (WebException ex)
            {
                log.ErrorFormat("Web exception occured {0} ", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ErrorMessages.InternalServerError);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unhandled exception occured {0} ", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ErrorMessages.InternalServerError);
            }
        }
    }
}