using DelawareSimulator.DbConnections;
using DelawareSimulator.SearchServices;
using DelawareSimulator.TokenService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Xml.Linq;

namespace DelawareSimulator.ReportServices
{
    [Route("ReportService")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> ReportRequest()
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string requestBody = await reader.ReadToEndAsync();
                XDocument xmlDoc = XDocument.Parse(requestBody);
                XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace ns1 = "Delaware.Ecorp.Web";
                XNamespace ns2 = "http://schemas.datacontract.org/2004/07/Delaware.ICIS.XmlFiling.Types";

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

                var fileNameElement = xmlDoc
                    .Element(soapenv + "Envelope")
                    ?.Element(soapenv + "Body")
                    ?.Element(ns1 + "corporationDetailsRequestDocument")
                    ?.Element(ns2 + "fileNumber");

                // Get the values as strings
                string clientAccountNum = clientAccountNumElement?.Value;
                string packetNum = packetNumElement?.Value;
                string agentPONumber = agentPONumberElement?.Value;
                string secureToken = secureTokenElement?.Value;
                string fileNumber = fileNameElement?.Value;

                if (!String.IsNullOrEmpty(fileNumber))
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                    QueryReportService queryReportService = new QueryReportService(configuration);
                    CorporationDetailsResponse corporationDetailsResponse = queryReportService.ExecuteQuery(Int32.Parse(fileNumber));

                    if (corporationDetailsResponse != null)
                    {
                        string boundary = $"uuid:{secureToken}";
                        string start = "<http://tempuri.org/0>";
                        string startInfo = "text/xml";
                        string contentType = $"multipart/related; boundary=\"{boundary}\"; type=\"application/xop+xml\"; start=\"{start}\"; start-info=\"{startInfo}\"";

                        string xmlContent = ResponseXMLReportService.ToXml(clientAccountNum, packetNum, agentPONumber, secureToken, corporationDetailsResponse);
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

            string xmlContent = ResponseXMLReportService.ToXmlNoContent(clientAccountNum, packetNum, agentPONumber, secureToken);
            string multipartContent = CreateMultipartContent(boundary, start, xmlContent);

            return new ContentResult
            {
                Content = multipartContent,
                ContentType = contentType,
                StatusCode = 200,
            };
        }
    }

    public static class ResponseXMLReportService
    {
        public static string ToXml(string clientAccountNum, string packetNum, string agentPONumber, string secureToken, CorporationDetailsResponse corporationDetailsResponse)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace h = "Delaware.Ecorp.Web";
            XNamespace i = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace a = "http://schemas.datacontract.org/2004/07/Delaware.ICIS.XmlFiling.Types";

            var body = new XElement(h + "corporationDetailsRequestResponse",
                new XElement(h + "corporationDetailsResponse",
                    new XAttribute(XNamespace.Xmlns + "a", a),
                    new XAttribute(XNamespace.Xmlns + "i", i),
                    new XElement(a + "fileNumber", corporationDetailsResponse.FileNumber),
                    new XElement(a + "name", corporationDetailsResponse.Name),
                    new XElement(a + "entityStatus", corporationDetailsResponse.EntityStatus),
                    new XElement(a + "agentNumber", corporationDetailsResponse.AgentNumber),
                    new XElement(a + "agentName", corporationDetailsResponse.AgentName),
                    new XElement(a + "agentAddress",
                        new XElement(a + "mailingAddress1", corporationDetailsResponse.AgentAddress?.MailingAddress1),
                        new XElement(a + "mailingAddress2", corporationDetailsResponse.AgentAddress?.MailingAddress2),
                        new XElement(a + "mailingAddress3", corporationDetailsResponse.AgentAddress?.MailingAddress3, new XAttribute(i + "nil", "true")),
                        new XElement(a + "city", corporationDetailsResponse.AgentAddress?.City),
                        new XElement(a + "county", corporationDetailsResponse.AgentAddress?.County),
                        new XElement(a + "state", corporationDetailsResponse.AgentAddress?.State),
                        new XElement(a + "country", corporationDetailsResponse.AgentAddress?.Country),
                        new XElement(a + "postalCode", corporationDetailsResponse.AgentAddress?.PostalCode)
                    ),
                    new XElement(a + "agentCounty", corporationDetailsResponse.AgentCounty),
                    new XElement(a + "kind", corporationDetailsResponse.Kind),
                    new XElement(a + "type", corporationDetailsResponse.Type),
                    new XElement(a + "taxType", corporationDetailsResponse.TaxType),
                    new XElement(a + "residency", corporationDetailsResponse.Residency),
                    new XElement(a + "incorporationDate", corporationDetailsResponse.IncorporationDate),
                    new XElement(a + "incorporationState", corporationDetailsResponse.IncorporationState),
                    new XElement(a + "foreignIncorporationDate", corporationDetailsResponse.ForeignIncorporationDate),
                    new XElement(a + "bankruptcyStatus", corporationDetailsResponse.BankruptcyStatus, new XAttribute(i + "nil", "true")),
                    new XElement(a + "stockCompany", corporationDetailsResponse.StockCompany.ToString().ToLower()),
                    new XElement(a + "renewalDate", corporationDetailsResponse.RenewalDate),
                    new XElement(a + "expirationDate", corporationDetailsResponse.ExpirationDate),
                    new XElement(a + "statusDate", corporationDetailsResponse.StatusDate),
                    new XElement(a + "bankruptcyDate", corporationDetailsResponse.BankruptcyDate),
                    new XElement(a + "bankruptcyCaseNo", corporationDetailsResponse.BankruptcyCaseNo, new XAttribute(i + "nil", "true")),
                    new XElement(a + "originalState", corporationDetailsResponse.OriginalState),
                    new XElement(a + "originalForeignName", corporationDetailsResponse.OriginalForeignName, new XAttribute(i + "nil", "true")),
                    new XElement(a + "originalForeignKind", corporationDetailsResponse.OriginalForeignKind, new XAttribute(i + "nil", "true")),
                    new XElement(a + "mergedTo", corporationDetailsResponse.MergedTo),
                    new XElement(a + "lastAnnualReport", corporationDetailsResponse.LastAnnualReport),
                    new XElement(a + "quarterlyFlag", corporationDetailsResponse.QuarterlyFlag.ToString().ToLower()),
                    new XElement(a + "stockAmendmentNumber", corporationDetailsResponse.StockAmendmentNumber),
                    new XElement(a + "stockBeginDate", corporationDetailsResponse.StockBeginDate),
                    new XElement(a + "stockEndDate", corporationDetailsResponse.StockEndDate),
                    new XElement(a + "totalAuthorizedShares", corporationDetailsResponse.TotalAuthorizedShares),
                    new XElement(a + "noParShares", corporationDetailsResponse.NoParShares),
                    new XElement(a + "stockDetails",
                        new XElement(a + "sequence", corporationDetailsResponse.StockDetails?.Sequence),
                        new XElement(a + "description", corporationDetailsResponse.StockDetails?.Description),
                        new XElement(a + "series", corporationDetailsResponse.StockDetails?.Series),
                        new XElement(a + "class", corporationDetailsResponse.StockDetails?.StockClass),
                        new XElement(a + "authorizedStock", corporationDetailsResponse.StockDetails?.AuthorizedStock),
                        new XElement(a + "parValue", corporationDetailsResponse.StockDetails?.ParValue),
                        new XElement(a + "designatedShares", corporationDetailsResponse.StockDetails?.DesignatedShares),
                        new XAttribute(i + "nil", "true")),
                    new XElement(a + "filingHistory",
                        new XElement(a + "filingHistory",
                            new XElement(a + "serviceRequestNumber", corporationDetailsResponse.FilingHistories?.ServiceRequestNumber),
                            new XElement(a + "sequence", corporationDetailsResponse.FilingHistories?.Sequence),
                            new XElement(a + "filingYear", corporationDetailsResponse.FilingHistories?.FilingYear),
                            new XElement(a + "documentCode", corporationDetailsResponse.FilingHistories?.DocumentCode),
                            new XElement(a + "docPages", corporationDetailsResponse.FilingHistories?.DocPages),
                            new XElement(a + "domesticationPages", corporationDetailsResponse.FilingHistories?.DomesticationPages),
                            new XElement(a + "previousName", corporationDetailsResponse.FilingHistories?.PreviousName, new XAttribute(i + "nil", "true")),
                            new XElement(a + "filingDateTime", corporationDetailsResponse.FilingHistories?.FilingDateTime),
                            new XElement(a + "effectiveDate", corporationDetailsResponse.FilingHistories?.EffectiveDate),
                            new XElement(a + "filingStatus", corporationDetailsResponse.FilingHistories?.FilingStatus),
                            new XElement(a + "mergerType", corporationDetailsResponse.FilingHistories?.MergerType, new XAttribute(i + "nil", "true"))
                        )
                    ),
                    new XElement(a + "taxBalance", corporationDetailsResponse.TaxBalance),
                    new XElement(a + "taxHistory",
                        new XElement(a + "taxInfo",
                            new XElement(a + "taxYear", corporationDetailsResponse.TaxInfo?.TaxYear),
                            new XElement(a + "filingFee", corporationDetailsResponse.TaxInfo?.FilingFee),
                            new XElement(a + "totalTaxes", corporationDetailsResponse.TaxInfo?.TotalTaxes),
                            new XElement(a + "penalty", corporationDetailsResponse.TaxInfo?.Penalty),
                            new XElement(a + "interest", corporationDetailsResponse.TaxInfo?.Interest),
                            new XElement(a + "other", corporationDetailsResponse.TaxInfo?.Other),
                            new XElement(a + "paid", corporationDetailsResponse.TaxInfo?.Paid),
                            new XElement(a + "crOrPrePaid", corporationDetailsResponse.TaxInfo?.CrOrPrePaid),
                            new XElement(a + "balance", corporationDetailsResponse.TaxInfo?.Balance)
                        )
                    )
                )
            );

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
                new XElement(soapenv + "Body", body)
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
