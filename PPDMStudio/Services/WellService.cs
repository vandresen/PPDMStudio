using Dapper;
using Microsoft.Data.SqlClient;
using PPDMStudio.Models;

namespace PPDMStudio.Services
{
    public interface IWellService
    {
        Task<IEnumerable<Well>> GetWellsAsync(string connectionString, WellFilter filter, IEnumerable<string>? wellListUwis = null);
        Task<WellHeader?> GetWellHeaderAsync(string connectionString, string uwi);
        Task<int> GetWellCountAsync(string connectionString);
    }

    public class WellService : IWellService
    {
        public async Task<WellHeader?> GetWellHeaderAsync(string connectionString, string uwi)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<WellHeader>(
                "SELECT W.UWI, W.WELL_NAME, W.OPERATOR, W.CURRENT_STATUS, " +
                "W.PROFILE_TYPE, W.ENVIRONMENT_TYPE, W.DEPTH_DATUM, " +
                "W.KB_ELEV, W.GROUND_ELEV, " +
                "W.FINAL_TD, W.LOG_TD, W.DRILL_TD, W.MAX_TVD, " +
                "W.SPUD_DATE, W.FINAL_DRILL_DATE, W.COMPLETION_DATE, W.ABANDONMENT_DATE, " +
                "W.SURFACE_LATITUDE, W.SURFACE_LONGITUDE, " +
                "W.BOTTOM_HOLE_LATITUDE, W.BOTTOM_HOLE_LONGITUDE, " +
                "W.REMARK, " +
                "MAX(CASE WHEN WA.AREA_TYPE = 'COUNTRY' THEN WA.AREA_ID END) AS COUNTRY, " +
                "MAX(CASE WHEN WA.AREA_TYPE = 'STATE' THEN WA.AREA_ID END) AS STATE, " +
                "MAX(CASE WHEN WA.AREA_TYPE = 'COUNTY' THEN WA.AREA_ID END) AS COUNTY " +
                "FROM WELL W " +
                "LEFT JOIN WELL_AREA WA ON W.UWI = WA.UWI " +
                "WHERE W.UWI = @UWI " +
                "GROUP BY W.UWI, W.WELL_NAME, W.OPERATOR, W.CURRENT_STATUS, " +
                "W.PROFILE_TYPE, W.ENVIRONMENT_TYPE, W.DEPTH_DATUM, " +
                "W.KB_ELEV, W.GROUND_ELEV, " +
                "W.FINAL_TD, W.LOG_TD, W.DRILL_TD, W.MAX_TVD, " +
                "W.SPUD_DATE, W.FINAL_DRILL_DATE, W.COMPLETION_DATE, W.ABANDONMENT_DATE, " +
                "W.SURFACE_LATITUDE, W.SURFACE_LONGITUDE, " +
                "W.BOTTOM_HOLE_LATITUDE, W.BOTTOM_HOLE_LONGITUDE, W.REMARK",
                new { UWI = uwi });
        }

        public async Task<IEnumerable<Well>> GetWellsAsync(
            string connectionString,
            WellFilter filter,
            IEnumerable<string>? wellListUwis = null)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // If well list provided, bulk insert to temp table
            if (wellListUwis != null && wellListUwis.Any())
            {
                await connection.ExecuteAsync(
                    "CREATE TABLE #WellList (UWI NVARCHAR(40) PRIMARY KEY)");

                var batches = wellListUwis
                    .Select((uwi, index) => new { uwi, index })
                    .GroupBy(x => x.index / 1000)
                    .Select(g => g.Select(x => x.uwi));

                foreach (var batch in batches)
                {
                    var insertSql = "INSERT INTO #WellList (UWI) VALUES " +
                        string.Join(",", batch.Select(u => $"('{u}')"));
                    await connection.ExecuteAsync(insertSql);
                }
            }

            var sql = BuildQuery(filter, wellListUwis != null && wellListUwis.Any());
            return await connection.QueryAsync<Well>(sql);
        }

        public async Task<int> GetWellCountAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WELL");
        }

        private string BuildQuery(WellFilter filter, bool useWellList = false)
        {
            var where = new List<string>();
            var having = new List<string>();

            if (useWellList)
                where.Add("W.UWI IN (SELECT UWI FROM #WellList)");

            if (!string.IsNullOrWhiteSpace(filter.UWI))
                where.Add($"W.UWI = '{filter.UWI}'");

            if (!string.IsNullOrWhiteSpace(filter.WellName))
                where.Add($"W.WELL_NAME LIKE '{filter.WellName}'");

            if (!string.IsNullOrWhiteSpace(filter.Operator))
                where.Add($"W.OPERATOR LIKE '{filter.Operator}'");

            if (!string.IsNullOrWhiteSpace(filter.AssignedField))
                where.Add($"W.ASSIGNED_FIELD LIKE '{filter.AssignedField}'");

            if (!string.IsNullOrWhiteSpace(filter.County))
                having.Add($"MAX(CASE WHEN WA.AREA_TYPE = 'COUNTY' THEN WA.AREA_ID END) LIKE '{filter.County}'");

            if (!string.IsNullOrWhiteSpace(filter.State))
                having.Add($"MAX(CASE WHEN WA.AREA_TYPE = 'STATE' THEN WA.AREA_ID END) LIKE '{filter.State}'");

            if (!string.IsNullOrWhiteSpace(filter.Country))
                having.Add($"MAX(CASE WHEN WA.AREA_TYPE = 'COUNTRY' THEN WA.AREA_ID END) LIKE '{filter.Country}'");

            var sql = "SELECT W.UWI, W.WELL_NAME, W.OPERATOR, " +
                      "W.ASSIGNED_FIELD, W.SPUD_DATE, " +
                      "W.SURFACE_LATITUDE, W.SURFACE_LONGITUDE, " +
                      "MAX(CASE WHEN WA.AREA_TYPE = 'COUNTRY' THEN WA.AREA_ID END) AS COUNTRY, " +
                      "MAX(CASE WHEN WA.AREA_TYPE = 'STATE' THEN WA.AREA_ID END) AS STATE, " +
                      "MAX(CASE WHEN WA.AREA_TYPE = 'COUNTY' THEN WA.AREA_ID END) AS COUNTY " +
                      "FROM WELL W " +
                      "LEFT JOIN WELL_AREA WA ON W.UWI = WA.UWI ";

            if (where.Any())
                sql += "WHERE " + string.Join(" AND ", where) + " ";

            sql += "GROUP BY W.UWI, W.WELL_NAME, W.OPERATOR, " +
                   "W.ASSIGNED_FIELD, W.SPUD_DATE, " +
                   "W.SURFACE_LATITUDE, W.SURFACE_LONGITUDE";

            if (having.Any())
                sql += " HAVING " + string.Join(" AND ", having);

            return sql;
        }
    }
}
