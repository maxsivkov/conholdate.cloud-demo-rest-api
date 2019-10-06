using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Aspose.Pdf.Cloud.Sdk.Api;
using Microsoft.AspNetCore.Hosting;
using System.IO;

using Aspose.BarCode.Cloud.Sdk;
using Aspose.BarCode.Cloud.Sdk.Model.Requests;
using Aspose.Pdf.Cloud.Sdk.Api;
using Aspose.Pdf.Cloud.Sdk.Model;
using Aspose.Html.Cloud.Sdk.Api;
using System.Text;
using System.Text.RegularExpressions;

namespace conholdate.cloud_demo_rest_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BarCodeApi _barCodeApi;
        private readonly PdfApi _pdfApi;
        private readonly HtmlApi _htmlApi;
        //private readonly string basePath = "https://api-qa.aspose.cloud/v3.0/";
        private readonly string basePath = "https://api.aspose.cloud/v3.0/";

        public SampleController(IHostingEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;

            string token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string bearerToken = string.Empty;
            Regex regexp = new Regex(@"Bearer\s+(?<token>\S+)", RegexOptions.IgnoreCase);
            Match match = regexp.Match(token);
            if (match.Success)
                bearerToken = match.Groups["token"].Value;
            _barCodeApi = new BarCodeApi(token);
            _pdfApi = new PdfApi(bearerToken);
            _htmlApi = new HtmlApi(new Aspose.Html.Cloud.Sdk.Client.Authentication.JwtToken
            {
                Token = bearerToken,
                IssuedOn = DateTime.Today,
                ExpiresInSeconds = 86400

            }, basePath);
            //System.IdentityModel.Tokens.Jwt WTF??
        }

        [HttpGet("testhtml")]
        public ActionResult TestHtml()
        {
            string fileName = Guid.NewGuid().ToString();
            string pdfFileName = fileName + ".pdf";

            string pdfFileNamePage2 = fileName + "_Page2.pdf";
            using (var file = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, "second_page.html")))
            {
                _htmlApi.PostConvertDocumentToPdf(file, pdfFileNamePage2);
            }
            string bearerToken = string.Empty;
            string token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            Regex regexp = new Regex(@"Bearer\s+(?<token>\S+)", RegexOptions.IgnoreCase);
            Match match = regexp.Match(token);
            if (match.Success)
                bearerToken = match.Groups["token"].Value;
            StorageApi sa = new StorageApi(new Aspose.Html.Cloud.Sdk.Client.Authentication.JwtToken
            {
                Token = bearerToken,
                IssuedOn = DateTime.Today,
                ExpiresInSeconds = 86400

            }, basePath);
            Aspose.Html.Cloud.Sdk.Api.Model.StreamResponse sr = sa.DownloadFile(pdfFileNamePage2);
            return new FileStreamResult(sr.ContentStream, "application/pdf");
        }

        [HttpGet("ticket")]
        public ActionResult GenerateTicket(string TicketNo, string FlightNo, string Name, DateTime FlightDate,
            string From, string To, string Class, string Seat, string Age, string Phone, string Gender)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(TicketNo).AppendLine(FlightNo).AppendLine(Name).AppendLine(FlightDate.ToString())
                    .AppendLine(From).AppendLine(To).AppendLine(Class).AppendLine(Seat).AppendLine(Age).AppendLine(Phone).AppendLine(Gender);
                string fileName = Guid.NewGuid().ToString();
                string pdfFileName = fileName + ".pdf";
                using (var file = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, "ticket_template.pdf")))
                {
                    var response = _pdfApi.UploadFile(pdfFileName, file);
                }
                using (Stream barcodeResponse = _barCodeApi.BarCodeGetBarCodeGenerate(new BarCodeGetBarCodeGenerateRequest(Crc32.ComputeChecksum(Encoding.UTF8.GetBytes(sb.ToString())).ToString(), "Code128", "png")))
                {
                    _pdfApi.PostInsertImage(pdfFileName, 1, 350, 600, 450, 700, image: barcodeResponse);
                }
                Fields fields = new Fields(List : new List<Field>() {
                    new Field(Name : "TicketNumber", Values : new List<string> { TicketNo } ),
                    new Field(Name : "FlightNumber", Values : new List<string> { FlightNo } ),
                    new Field(Name : "Date", Values : new List<string> { FlightDate.ToShortDateString() } ),
                    new Field(Name : "Time", Values : new List<string> { FlightDate.ToShortTimeString() } ),
                    new Field(Name : "From", Values : new List<string> { From } ),
                    new Field(Name : "To", Values : new List<string> { To } ),
                    new Field(Name : "Class", Values : new List<string> { Class } ),
                    new Field(Name : "Seat", Values : new List<string> { Seat } ),
                    new Field(Name : "Name", Values : new List<string> { Name } ),
                    new Field(Name : "Age", Values : new List<string> { Age } ),
                    new Field(Name : "Phone", Values : new List<string> { Phone } ),
                    new Field(Name : "Gender", Values : new List<string> { Gender } ),
                });
                _pdfApi.PutUpdateFields(pdfFileName, fields);
                /*
                string pdfFileNamePage2 = fileName + "_Page2.pdf";
                using (var file = System.IO.File.OpenRead(Path.Combine(_hostingEnvironment.WebRootPath, "second_page.html")))
                {
                    _htmlApi.PostConvertDocumentToPdf(file, pdfFileNamePage2);
                }
                _pdfApi.PostAppendDocument(pdfFileName, pdfFileNamePage2);
                */
                MemoryStream ms = new MemoryStream();
                using (Stream response = _pdfApi.DownloadFile(pdfFileName))
                {
                    response.CopyTo(ms);
                    ms.Position = 0;
                    return new FileStreamResult(ms, "application/pdf");
                }
            }
            catch (ApiException ex)
            {
                return new StatusCodeResult(ex.ErrorCode);
            }
        }

    }
}