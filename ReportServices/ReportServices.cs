using System.Runtime.Serialization;

namespace DelawareSimulator.ReportServices
{
    public class ReportServicesResponseData
    {
        public Header? Header { get; set; }

        public int? RecordCount { get; set; }

        public CorporationDetailsResponse? CorporationDetailsResponse { get; set; }
    }

    public class Header
    {
        public int? Status { get; set; }

        public string? Message { get; set; }

        public string? QueryId { get; set; }

        public string? TransactionId { get; set; }

        public Exceptions? Exceptions { get; set; }

    }
    public class CorporationDetailsResponse
    {
        public int? FileNumber { get; set; }

        public string? Name { get; set; }

        public string? EntityStatus { get; set; }

        public int? AgentNumber { get; set; }

        public string? AgentName { get; set; }

        public AgentAddress? AgentAddress { get; set; }

        public string? AgentCounty { get; set; }

        public string? Kind { get; set; }

        public string? Type { get; set; }

        public string? TaxType { get; set; }

        public string? Residency { get; set; }

        public string? IncorporationDate { get; set; }

        public string? IncorporationState { get; set; }

        public string? ForeignIncorporationDate { get; set; }

        public string? BankruptcyStatus { get; set; }

        public bool? StockCompany { get; set; }

        public string? RenewalDate { get; set; }

        public string? ExpirationDate { get; set; }

        public string? StatusDate { get; set; }

        public string? BankruptcyDate { get; set; }

        public string? BankruptcyCaseNo { get; set; }

        public string? OriginalState { get; set; }

        public string? OriginalForeignName { get; set; }

        public string? OriginalForeignKind { get; set; }

        public int? MergedTo { get; set; }

        public int? LastAnnualReport { get; set; }

        public bool? QuarterlyFlag { get; set; }

        public string? StockAmendmentNumber { get; set; }

        public string? StockBeginDate { get; set; }

        public string? StockEndDate { get; set; }

        public int? TotalAuthorizedShares { get; set; }

        public int? NoParShares { get; set; }

        public StockDetails? StockDetails { get; set; }

        public FilingHistories? FilingHistories { get; set; }

        public string? TaxBalance { get; set; }

        public TaxInfo? TaxInfo { get; set; }
    }

    public class AgentAddress
    {
        public string? MailingAddress1 { get; set; }

        public string? MailingAddress2 { get; set; }

        public string? MailingAddress3 { get; set; }

        public string? City { get; set; }

        public string? County { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        public string? PostalCode { get; set; }

    }

    public class StockDetails
    {
        public string? Sequence { get; set; }

        public string? Description { get; set; }

        public string? Series { get; set; }

        public string? StockClass { get; set; }

        public string? AuthorizedStock { get; set; }

        public string? ParValue { get; set; }

        public string? DesignatedShares { get; set; }

    }

    public class FilingHistories
    {
        public string? ServiceRequestNumber { get; set; }

        public string? Sequence { get; set; }

        public string? FilingYear { get; set; }

        public string? DocumentCode { get; set; }

        public string? DocPages { get; set; }

        public string? DomesticationPages { get; set; }

        public string? PreviousName { get; set; }

        public string? FilingDateTime { get; set; }

        public string? EffectiveDate { get; set; }

        public string? FilingStatus { get; set; }

        public string? MergerType { get; set; }

    }

    public class TaxInfo
    {

        public string? TaxYear { get; set; }

        public string? FilingFee { get; set; }

        public string? TotalTaxes { get; set; }

        public string? Penalty { get; set; }

        public string? Interest { get; set; }

        public string? Other { get; set; }

        public string? Paid { get; set; }

        public string? CrOrPrePaid { get; set; }

        public string? Balance { get; set; }

    }

    public class Exceptions
    {
        public WsException? WsException { get; set; }

        public TransactionCap? TransactionCap { get; set; }
    }

    public class WsException
    {
        public string? Source { get; set; }

        public string? Code { get; set; }

        public string? Location { get; set; }

        public string? Message { get; set; }

    }

    public class TransactionCap
    {
        public int? Maximum { get; set; }

        public int? Count { get; set; }

        public bool? AllowAboveMax { get; set; }
    }
}