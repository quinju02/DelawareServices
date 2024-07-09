using DelawareSimulator.SearchServices;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;


namespace DelawareSimulator.DbConnections
{
    public class QuerySearchService
    {
        private readonly string _connectionString;

        public QuerySearchService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        public List<CorporationSearchResponse> ExecuteQuery(string? searchName)
        {
            List<CorporationSearchResponse> searchCompaniesList = new List<CorporationSearchResponse>();
            CorporationSearchResponse corporationSearchResponse = new CorporationSearchResponse();

            using (var conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    StringBuilder selectSearchCompaniesQuery = new StringBuilder();

                    selectSearchCompaniesQuery.Append("SELECT * ");
                    selectSearchCompaniesQuery.Append("FROM prct_test.delawaredata ");
                    selectSearchCompaniesQuery.Append("WHERE companyName LIKE '%");
                    selectSearchCompaniesQuery.Append(searchName);
                    selectSearchCompaniesQuery.Append("%' ;");

                    MySqlCommand selectSearchCompaniesCommand = new MySqlCommand(selectSearchCompaniesQuery.ToString(), conn);
                    MySqlDataAdapter selectSearcCompaniesDa = new MySqlDataAdapter();
                    selectSearcCompaniesDa.SelectCommand = selectSearchCompaniesCommand;

                    DataSet dsSearchCompanies = new DataSet();
                    selectSearcCompaniesDa.Fill(dsSearchCompanies, "prct_test.delawaredata");

                    if (dsSearchCompanies.Tables["prct_test.delawaredata"].Rows.Count > 0)
                    {
                        foreach (DataRow selectSearchCompaniesRow in dsSearchCompanies.Tables["prct_test.delawaredata"].Rows)
                        {
                            corporationSearchResponse = new CorporationSearchResponse
                            {

                                FileNumber = Convert.ToInt32(selectSearchCompaniesRow["fileNumber"]),
                                Name = Convert.ToString(selectSearchCompaniesRow["companyName"]),
                                Entity = Convert.ToString(selectSearchCompaniesRow["agentName"]),
                            };

                            searchCompaniesList.Add(corporationSearchResponse);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return searchCompaniesList;
        }
    }
}
