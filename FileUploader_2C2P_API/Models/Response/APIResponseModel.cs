using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace FileUploader_2C2P_API.Models.Response
{
    public class APIResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }

        public dynamic Response { get; set; }
    }
}