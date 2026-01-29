using Dapper;

public class ReportRepository
{
    private readonly ReportConnectionProvider _reportProvider;

    public ReportRepository(ReportConnectionProvider reportProvider)
    {
        _reportProvider = reportProvider;
    }

    public async Task<int> GetTotalTracksCountAsync()
    {
        using var conn = _reportProvider.GetReportConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Tracks\"");
    }
}