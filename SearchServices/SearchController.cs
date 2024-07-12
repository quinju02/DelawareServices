using DelawareSimulator.DbConnections;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace DelawareSimulator.SearchServices
{
    [Route("SearchService")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SearchRequest()
        {

            if (Request.Headers["SoapAction"].ToString().Equals("Delaware.Ecorp.Web/PublicXMLService/SignOn"))
            {
                string uuid = Guid.NewGuid().ToString("D").ToLower();
                string boundary = $"uuid:{uuid}";
                string start = "<http://tempuri.org/0>";
                string startInfo = "text/xml";
                string contentType = $"multipart/related; boundary=\"{boundary}\"; type=\"application/xop+xml\"; start=\"{start}\"; start-info=\"{startInfo}\"";

                string xmlContent = ResponseXMLSearchService.ToXml(uuid);
                string multipartContent = CreateMultipartContent(boundary, start, xmlContent);

                return new ContentResult
                {
                    Content = multipartContent,
                    ContentType = contentType,
                    StatusCode = 200,
                };
            }
            else
            {
                using (StreamReader reader = new StreamReader(Request.Body))
                {
                    string requestBody = await reader.ReadToEndAsync();

                    // Parse the XML
                    XDocument xmlDoc = XDocument.Parse(requestBody);

                    // Define the namespaces used in the XML
                    XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
                    XNamespace ns1 = "Delaware.Ecorp.Web";
                    XNamespace ns2 = "http://schemas.datacontract.org/2004/07/Delaware.ICIS.XmlFiling.Types";

                    // Extract the header values
                    var clientAccountNumElement = xmlDoc
                        .Element(soapenv + "Envelope")
                        ?.Element(soapenv + "Header")
                        ?.Element(ns1 + "ClientAccountNum");

                    var packetNumElement = xmlDoc
                        .Element(soapenv + "Envelope")
                        ?.Element(soapenv + "Header")
                        ?.Element(ns1 + "PacketNum");

                    var agentPONumberElement = xmlDoc
                        .Element(soapenv + "Envelope")
                        ?.Element(soapenv + "Header")
                        ?.Element(ns1 + "agentPONumber");

                    var secureTokenElement = xmlDoc
                        .Element(soapenv + "Envelope")
                        ?.Element(soapenv + "Header")
                        ?.Element(ns1 + "secureToken");

                    // Extract the body value
                    var searchNameElement = xmlDoc
                        .Element(soapenv + "Envelope")
                        ?.Element(soapenv + "Body")
                        ?.Element(ns1 + "corporationSearchRequest")
                        ?.Element(ns2 + "searchName");

                    // Get the values as strings
                    string clientAccountNum = clientAccountNumElement?.Value;
                    string packetNum = packetNumElement?.Value;
                    string agentPONumber = agentPONumberElement?.Value;
                    string secureToken = secureTokenElement?.Value;
                    string searchName = searchNameElement?.Value;

                    if (!String.IsNullOrEmpty(searchName))
                    {
                        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                        QuerySearchService querySearchService = new QuerySearchService(configuration);
                        List<CorporationSearchResponse> searchList = querySearchService.ExecuteQuery(searchName);

                        if (searchList.Count > 0)
                        {
                            string boundary = $"uuid:{secureToken}";
                            string start = "<http://tempuri.org/0>";
                            string startInfo = "text/xml";
                            string contentType = $"multipart/related; boundary=\"{boundary}\"; type=\"application/xop+xml\"; start=\"{start}\"; start-info=\"{startInfo}\"";

                            string xmlContent = ResponseXMLSearchService.ToXml(clientAccountNum, packetNum, agentPONumber, secureToken, searchList);
                            string multipartContent = CreateMultipartContent(boundary, start, xmlContent);

                            return new ContentResult
                            {
                                Content = multipartContent,
                                ContentType = contentType,
                                StatusCode = 200,
                            };
                        }
                        else
                        {
                            return NoContentResponse(secureToken, clientAccountNum, packetNum, agentPONumber);
                        }

                    }
                    else
                    {
                        return NoContentResponse(secureToken, clientAccountNum, packetNum, agentPONumber);
                    }
                }
            }
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

        private ContentResult NoContentResponse(string secureToken, string clientAccountNum, string packetNum, string agentPONumber)
        {
            string boundary = $"uuid:{secureToken}";
            string start = "<http://tempuri.org/0>";
            string startInfo = "text/xml";
            string contentType = $"multipart/related; boundary=\"{boundary}\"; type=\"application/xop+xml\"; start=\"{start}\"; start-info=\"{startInfo}\"";

            string xmlContent = ResponseXMLSearchService.ToXmlNoContent(clientAccountNum, packetNum, agentPONumber, secureToken);
            string multipartContent = CreateMultipartContent(boundary, start, xmlContent);

            return new ContentResult
            {
                Content = multipartContent,
                ContentType = contentType,
                StatusCode = 200,
            };
        }
    }

    public static class ResponseXMLSearchService
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

        public static string ToXml(string clientAccountNum, string packetNum, string agentPONumber, string secureToken, List<CorporationSearchResponse> searchList)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace h = "Delaware.Ecorp.Web";
            XNamespace i = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace a = "http://schemas.datacontract.org/2004/07/Delaware.ICIS.XmlFiling.Types";

            var xml = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "s", soapenv),
                new XElement(soapenv + "Header",
                    new XElement(h + "agentNumber", new XAttribute(XNamespace.Xmlns + "h", h), clientAccountNum),
                    new XElement(h + "agentPONumber", agentPONumber),
                    new XElement(h + "attentionLine", new XAttribute(XNamespace.Xmlns + "i", i), new XAttribute(i + "nil", "true")),
                    new XElement(h + "fileDateTime", new XAttribute(XNamespace.Xmlns + "i", i), new XAttribute(i + "nil", "true")),
                    new XElement(h + "packetNumber", packetNum),
                    new XElement(h + "receivedDateTime", DateTime.Now.ToString("yyyyMMdd")),
                    new XElement(h + "successful", "true")
                ),
                new XElement(soapenv + "Body",
                    new XElement(h + "corporationSearchResponse",
                        new XAttribute(XNamespace.Xmlns + "a", a),
                        new XAttribute(XNamespace.Xmlns + "i", i),
                        from search in searchList
                        select new XElement(a + "corporationSearch",
                            new XElement(a + "fileNumber", search.FileNumber),
                            new XElement(a + "name", search.Name),
                            new XElement(a + "entityKind", search.Entity)
                        )
                    )
                )
            );

            return xml.ToString(SaveOptions.DisableFormatting);
        }

        public static string ToXmlNoContent(string clientAccountNum, string packetNum, string agentPONumber, string secureToken)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace h = "Delaware.Ecorp.Web";
            XNamespace i = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace a = "http://schemas.datacontract.org/2004/07/Delaware.ICIS.XmlFiling.Types";

            var xml = new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "s", soapenv),
                new XElement(soapenv + "Header",
                    new XElement(h + "agentNumber", new XAttribute(XNamespace.Xmlns + "h", h), clientAccountNum),
                    new XElement(h + "agentPONumber", agentPONumber),
                    new XElement(h + "attentionLine", new XAttribute(XNamespace.Xmlns + "i", i), new XAttribute(i + "nil", "true")),
                    new XElement(h + "fileDateTime", new XAttribute(XNamespace.Xmlns + "i", i), new XAttribute(i + "nil", "true")),
                    new XElement(h + "packetNumber", packetNum),
                    new XElement(h + "receivedDateTime", DateTime.Now.ToString("yyyyMMdd")),
                    new XElement(h + "successful", "true")
                ),
                new XElement(soapenv + "Body")
            );

            return xml.ToString(SaveOptions.DisableFormatting);
        }
    }
}
