using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

public class ReportConnectionProvider
{
    private readonly string _reportConnString;

    public ReportConnectionProvider(IConfiguration config)
    {
        _reportConnString = config.GetConnectionString("ReportsConnection")!;
    }

    public IDbConnection GetReportConnection()
    {
        var conn = new NpgsqlConnection(_reportConnString);
        conn.Open();
        return conn;
    }
}