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
using FileUploader_2C2P_API.Models.Response;
using System.Web.Http.Cors;

namespace FileUploader_2C2P_API.Controllers
{
    [AllowAnonymous]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FileUploadController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        APIResponseModel apiResponseModel = null;

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
            log.Info("File Upload Started");
            var httpRequest = HttpContext.Current.Request;
            log.InfoFormat("Count of the Files : {0}", httpRequest.Files.Count);
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    if (postedFile.FileName.Contains("csv"))
                    {
                        apiResponseModel = FileImportHelper.ImportCSVFile(FileCopyProcess(postedFile));
                        return Request.CreateResponse(HttpStatusCode.PartialContent,apiResponseModel);
                    }
                    if (postedFile.FileName.Contains("xml"))
                    {
                        apiResponseModel = FileImportHelper.ImportXMFiles(FileCopyProcess(postedFile));
                        return Request.CreateResponse(HttpStatusCode.PartialContent,apiResponseModel);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.Created, docfiles);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ErrorMessages.NotFoundFiles);
            }
        }

        public List<string> FileCopyProcess(HttpPostedFile postedFile)
        {
            string filePath;
            string modifiedFileName = string.Empty;
            List<string> docFiles = null;
            string directoryPath = string.Empty;
            try
            {
                if (postedFile.FileName.Contains(".csv"))
                {
                    directoryPath = HttpContext.Current.Server.MapPath("~/upload/CSV");
                    log.InfoFormat("Uploaded file Name is {0} and filePath is {1}", postedFile.FileName, directoryPath);
                    modifiedFileName = Path.GetFileNameWithoutExtension(postedFile.FileName);
                    modifiedFileName = modifiedFileName + "_" + DateTime.Now.ToString("yyyy-MM-ddHH-mm-ss").Replace("-", string.Empty) + ".csv";
                }
                if (postedFile.FileName.Contains(".xml"))
                {
                    directoryPath = HttpContext.Current.Server.MapPath("~/upload/XML");
                    log.InfoFormat("Uploaded file Name is {0} and filePath is {1}", postedFile.FileName, directoryPath);
                    modifiedFileName = Path.GetFileNameWithoutExtension(postedFile.FileName);
                    modifiedFileName = modifiedFileName + "_" + DateTime.Now.ToString("yyyy-MM-ddHH-mm-ss").Replace("-", string.Empty) + ".xml";
                }
                if (FileImportHelper.DirectoryCheck(directoryPath))
                {
                    filePath = directoryPath + "/" + postedFile.FileName;
                    postedFile.SaveAs(filePath);

                    File.Move(filePath, directoryPath + "/" + modifiedFileName);

                    docFiles = new List<string>();
                    docFiles.Add(directoryPath + "/" + modifiedFileName);
                    log.InfoFormat("Imported file has been renamed and path is {0}", directoryPath + "/" + modifiedFileName);
                }
            }
            catch (IOException ex)
            {
                log.ErrorFormat("Exception occured while taking the file back up in temp folder {0}", ex);
                return docFiles;
            }
            return docFiles;
        }
    }
}