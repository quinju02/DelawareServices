using DelawareSimulator.ReportServices;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;


namespace DelawareSimulator.DbConnections
{
    public class QueryReportService
    {
        private readonly string _connectionString;

        public QueryReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        public CorporationDetailsResponse ExecuteQuery(int? fileNumber)
        {
            CorporationDetailsResponse corpDetailsResponse = null;

            using (var conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    StringBuilder selectFileNumberQuery = new StringBuilder();

                    selectFileNumberQuery.Append("SELECT * ");
                    selectFileNumberQuery.Append("FROM prct_test.delawaredata ");
                    selectFileNumberQuery.Append("WHERE fileNumber = '");
                    selectFileNumberQuery.Append(fileNumber);
                    selectFileNumberQuery.Append("' ;");

                    MySqlCommand selectFileNumberCommand = new MySqlCommand(selectFileNumberQuery.ToString(), conn);
                    MySqlDataAdapter selectFileNumberDa = new MySqlDataAdapter();
                    selectFileNumberDa.SelectCommand = selectFileNumberCommand;

                    DataSet dsFileNumber = new DataSet();
                    selectFileNumberDa.Fill(dsFileNumber, "prct_test.delawaredata");

                    if (dsFileNumber.Tables["prct_test.delawaredata"].Rows.Count > 0)
                    {
                        foreach (DataRow selectFileNumberRow in dsFileNumber.Tables["prct_test.delawaredata"].Rows)
                        {
                            corpDetailsResponse = new CorporationDetailsResponse
                            {
                                FileNumber = fileNumber,
                                Name = Convert.ToString(selectFileNumberRow["companyName"]),
                                EntityStatus = Convert.ToString(selectFileNumberRow["entityStatus"]) + " " + DateTime.Now.ToString(),
                                AgentNumber = Convert.ToInt32(selectFileNumberRow["agentNumber"]),
                                AgentName = Convert.ToString(selectFileNumberRow["agentName"]),
                                AgentAddress = new AgentAddress
                                {
                                    MailingAddress1 = Convert.ToString(selectFileNumberRow["agentMailingAddress1"]),
                                    MailingAddress2 = Convert.ToString(selectFileNumberRow["agentMailingAddress2"]),
                                    MailingAddress3 = Convert.ToString(selectFileNumberRow["agentMailingAddress3"]),
                                    City = Convert.ToString(selectFileNumberRow["agentCity"]),
                                    County = Convert.ToString(selectFileNumberRow["agentCounty"]),
                                    State = Convert.ToString(selectFileNumberRow["agentState"]),
                                    Country = Convert.ToString(selectFileNumberRow["agentCountry"]),
                                    PostalCode = Convert.ToString(selectFileNumberRow["agentPostalCode"])
                                },
                                AgentCounty = Convert.ToString(selectFileNumberRow["agentCounty"]),
                                Kind = Convert.ToString(selectFileNumberRow["kind"]),
                                Type = Convert.ToString(selectFileNumberRow["corpType"]),
                                TaxType = Convert.ToString(selectFileNumberRow["taxType"]),
                                Residency = Convert.ToString(selectFileNumberRow["residency"]),
                                IncorporationDate = Convert.ToString(selectFileNumberRow["incorporationDate"]),
                                IncorporationState = Convert.ToString(selectFileNumberRow["incorporationState"]),
                                ForeignIncorporationDate = Convert.ToString(selectFileNumberRow["foreignIncorporationDate"]),
                                BankruptcyStatus = Convert.ToString(selectFileNumberRow["bankruptcyStatus"]),
                                StockCompany = Convert.ToInt16(selectFileNumberRow["stockCompany"]) == 1 ? true : false,
                                RenewalDate = Convert.ToString(selectFileNumberRow["renewalDate"]),
                                ExpirationDate = Convert.ToString(selectFileNumberRow["expirationDate"]),
                                StatusDate = Convert.ToString(selectFileNumberRow["statusDate"]),
                                BankruptcyDate = Convert.ToString(selectFileNumberRow["bankruptcyDate"]),
                                BankruptcyCaseNo = Convert.ToString(selectFileNumberRow["bankruptcyCaseNo"]),
                                OriginalState = Convert.ToString(selectFileNumberRow["originalState"]),
                                OriginalForeignName = Convert.ToString(selectFileNumberRow["originalForeignName"]),
                                OriginalForeignKind = Convert.ToString(selectFileNumberRow["originalForeignKind"]),
                                MergedTo = Convert.ToInt32(selectFileNumberRow["mergedTo"]),
                                LastAnnualReport = Convert.ToInt32(selectFileNumberRow["lastAnnualReport"]),
                                QuarterlyFlag = Convert.ToInt16(selectFileNumberRow["quarterlyFlag"]) == 1 ? true : false,
                                StockAmendmentNumber = Convert.ToString(selectFileNumberRow["stockAmendmentNumber"]),
                                StockBeginDate = Convert.ToString(selectFileNumberRow["stockBeginDate"]),
                                StockEndDate = Convert.ToString(selectFileNumberRow["stockEndDate"]),
                                TotalAuthorizedShares = Convert.ToInt32(selectFileNumberRow["totalAuthorizedShares"]),
                                NoParShares = Convert.ToInt32(selectFileNumberRow["noParShares"]),
                                FilingHistories = new FilingHistories
                                {
                                    ServiceRequestNumber = Convert.ToString(selectFileNumberRow["serviceRequestNumber"]),
                                    Sequence = Convert.ToString(selectFileNumberRow["sequence"]),
                                    FilingYear = Convert.ToString(selectFileNumberRow["filingYear"]),
                                    DocumentCode = Convert.ToString(selectFileNumberRow["documentCode"]),
                                    DocPages = Convert.ToString(selectFileNumberRow["docPages"]),
                                    DomesticationPages = Convert.ToString(selectFileNumberRow["domesticationPages"]),
                                    PreviousName = Convert.ToString(selectFileNumberRow["previousName"]),
                                    FilingDateTime = Convert.ToString(selectFileNumberRow["filingDateTime"]),
                                    EffectiveDate = Convert.ToString(selectFileNumberRow["effectiveDate"]),
                                    FilingStatus = Convert.ToString(selectFileNumberRow["filingStatus"]),
                                    MergerType = Convert.ToString(selectFileNumberRow["mergerType"]),

                                },
                                StockDetails = new StockDetails
                                {
                                    Sequence = Convert.ToString(selectFileNumberRow["sequence"]),
                                    Description = Convert.ToString(selectFileNumberRow["stockDescription"]),
                                    Series = Convert.ToString(selectFileNumberRow["series"]),
                                    StockClass = Convert.ToString(selectFileNumberRow["class"]),
                                    AuthorizedStock = Convert.ToString(selectFileNumberRow["authorizedStock"]),
                                    ParValue = Convert.ToString(selectFileNumberRow["parValue"]),
                                    DesignatedShares = Convert.ToString(selectFileNumberRow["designatedShares"]),
                                },
                                TaxBalance = Convert.ToString(selectFileNumberRow["taxBalance"]),
                                TaxInfo = new TaxInfo
                                {
                                    TaxYear = Convert.ToString(selectFileNumberRow["taxYear"]),
                                    FilingFee = Convert.ToString(selectFileNumberRow["filingFee"]),
                                    TotalTaxes = Convert.ToString(selectFileNumberRow["totalTaxes"]),
                                    Penalty = Convert.ToString(selectFileNumberRow["penalty"]),
                                    Interest = Convert.ToString(selectFileNumberRow["interest"]),
                                    Other = Convert.ToString(selectFileNumberRow["other"]),
                                    Paid = Convert.ToString(selectFileNumberRow["paid"]),
                                    CrOrPrePaid = Convert.ToString(selectFileNumberRow["crOrPrePaid"]),
                                    Balance = Convert.ToString(selectFileNumberRow["balance"]),
                                },

                            };

                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return corpDetailsResponse;
        }
    }
}
