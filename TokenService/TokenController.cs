using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace DelawareSimulator.TokenService
{
    [Route("TokenService")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        [HttpPost]
        public IActionResult TokenRequest()
        {
            string uuid = Guid.NewGuid().ToString("D").ToLower();
            string boundary = $"uuid:{uuid}";
            string start = "<http://tempuri.org/0>";
            string startInfo = "text/xml";
            string contentType = $"multipart/related; boundary=\"{boundary}\"; type=\"application/xop+xml\"; start=\"{start}\"; start-info=\"{startInfo}\"";

            string xmlContent = ResponseXMLTokenService.ToXml(uuid);
            string multipartContent = CreateMultipartContent(boundary, start, xmlContent);

            return new ContentResult
            {
                Content = multipartContent,
                ContentType = contentType,
                StatusCode = 200,
            };
        }

        private string CreateMultipartContent(string boundary, string start, string xmlContent)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--" + boundary);
            sb.AppendLine($"Content-Type: application/xop+xml; charset=UTF-8; type=\"text/xml\"");
            sb.AppendLine($"Content-Transfer-Encoding: 8bit");
            sb.AppendLine($"Content-ID: {start}");
            sb.AppendLine();
            sb.AppendLine(xmlContent);
            sb.AppendLine("--" + boundary + "--");

            return sb.ToString();
        }
    }

    public static class ResponseXMLTokenService
    {
        public static string ToXml(string uuid)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace tem = "Delaware.Ecorp.Web";
            XNamespace i = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace a = "http://schemas.datacontract.org/2004/07/Delaware.ICIS.XmlFiling.Types";
            XNamespace h = "Delaware.Ecorp.Web";

            var xml = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "s", soapenv),
                new XElement(soapenv + "Header",
                    new XElement(h + "successful", new XAttribute(XNamespace.Xmlns + "h", h), "true")
                ),
                new XElement(soapenv + "Body",
                    new XElement(tem + "signOnResponse",
                        new XElement(tem + "errors",
                            new XAttribute(XNamespace.Xmlns + "a", a),
                            new XAttribute(XNamespace.Xmlns + "i", i),
                            new XAttribute(i + "nil", "true")
                        ),
                        new XElement(tem + "secureToken", uuid),
                        new XElement(tem + "signOnTime", DateTime.Now.ToString("HH:mm:ss.ffff"))
                    )
                )
            );

            return xml.ToString(SaveOptions.DisableFormatting);
        }
    }
}
