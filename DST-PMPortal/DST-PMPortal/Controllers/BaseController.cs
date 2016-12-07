using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DST_PMPortal.Controllers
{
    public class BaseController : ApiController
    {
        private HttpClient CreateClient()
        {
            var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
            string authInfo = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", @"DST-DOM\msuttie", "")));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic");
            return client;
        }
    }
}
