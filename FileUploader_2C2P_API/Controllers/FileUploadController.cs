using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FileUploader_2C2P.Helper;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using FileUploader_2C2P_API.Helper;

namespace FileUploader_2C2P_API.Controllers
{
    public class FileUploadController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [HttpGet]
        [Route("api/FileUpload/GetCurrencySymbol")]
        public HttpResponseMessage CurrencySymbolWithLocation()
        {
            FileImportHelper.LoadCurrency();
            return Request.CreateResponse(HttpStatusCode.OK, FileImportHelper.LoadCurrency());
        }

        [HttpPost]
        [Route("api/FileUpload/Upload")]
        public async Task<HttpResponseMessage> Upload()
        {
            string modifiedFileName = string.Empty;

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    if (postedFile.FileName.Contains("csv"))
                    {
                        string filePath;
                        string directoryPath = HttpContext.Current.Server.MapPath("~/upload/csv");
                        log.InfoFormat("Uploaded file Name is {0} and filePath is {1}", postedFile.FileName, directoryPath);
                        if (FileImportHelper.DirectoryCheck(directoryPath))
                        {
                            modifiedFileName = Path.GetFileNameWithoutExtension(postedFile.FileName);
                            modifiedFileName = modifiedFileName + "_" + DateTime.Now.ToString("yyyy-MM-ddHH-mm-ss").Replace("-", string.Empty) + ".csv";

                            filePath = directoryPath + "/" + postedFile.FileName;
                            postedFile.SaveAs(filePath);

                            File.Move(filePath,directoryPath + "/" + modifiedFileName);
                            docfiles.Add(directoryPath + "/" + modifiedFileName);
                            dynamic response = FileImportHelper.ImportCSVFile(docfiles);
                        }
                    }
                    if (postedFile.FileName.Contains("xml"))
                    {
                        string filePath = HttpContext.Current.Server.MapPath("~/upload/xml");

                        log.InfoFormat("Uploaded file Name is {0} and filePath is {1}", postedFile.FileName, filePath);
                        if (FileImportHelper.DirectoryCheck(filePath))
                        {
                            postedFile.SaveAs(filePath + @"\" + postedFile.FileName);
                            docfiles.Add(filePath);

                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.Created, docfiles);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ErrorMessages.NotFoundFiles);
            }
        }
    }
}