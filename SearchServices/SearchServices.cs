namespace DelawareSimulator.SearchServices
{

    public class SearchServices
    {
        public Header? Header { get; set; }

        public int? RecordCount { get; set; }

        public List<CorporationSearchResponse>? corporationSearchResponse { get; set; }
    }

    public class Header
    {
        public int? Status { get; set; }

        public string? Message { get; set; }

        public string? QueryId { get; set; }

        public string? TransactionId { get; set; }

        public Exceptions? Exceptions { get; set; }

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

    public class CorporationSearchResponse
    {
        public int? FileNumber { get; set; }

        public string? Name { get; set; }

        public string? Entity { get; set; }

    }
}
