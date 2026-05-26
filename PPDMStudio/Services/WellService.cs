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
        Task UpdateWellHeaderAsync(string connectionString, WellHeader header);
        Task<IEnumerable<string>> GetReferenceValuesAsync(string connectionString, string tableName, string columnName);
        Task AddReferenceValueAsync(string connectionString, string table, string column, string value);
        Task<IEnumerable<AreaItem>> SearchStatesAsync(string connectionString, string search);
        Task<AreaItem?> GetAreaByIdAsync(string connectionString, string areaId, string areaType);
        Task AddAreaAsync(string connectionString, string areaType, string areaId, string preferredName);
        Task<IEnumerable<AreaItem>> SearchCountiesAsync(string connectionString, string stateAreaId, string search);
        Task AddCountyAsync(string connectionString, string areaId, string preferredName, string parentStateAreaId);
        Task<bool> InsertWellAsync(string connectionString, WellHeader header);
    }

    public class WellService : IWellService
    {
        private static string AuditUser => Environment.UserName;

        public async Task<WellHeader?> GetWellHeaderAsync(string connectionString, string uwi)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<WellHeader>(
                "SELECT W.UWI, W.WELL_NAME, W.ASSIGNED_FIELD, W.OPERATOR, W.CURRENT_STATUS, " +
                "W.PROFILE_TYPE, W.DEPTH_DATUM, " +
                "W.KB_ELEV, W.GROUND_ELEV, " +
                "W.FINAL_TD, W.LOG_TD, W.DRILL_TD, W.MAX_TVD, " +
                "W.SPUD_DATE, W.FINAL_DRILL_DATE, W.COMPLETION_DATE, W.ABANDONMENT_DATE, " +
                "W.SURFACE_LATITUDE, W.SURFACE_LONGITUDE, " +
                "W.BOTTOM_HOLE_LATITUDE, W.BOTTOM_HOLE_LONGITUDE, " +
                "W.REMARK, " +
                "MAX(CASE WHEN WA.AREA_TYPE = 'STATE' THEN WA.AREA_ID END) AS STATE, " +
                "MAX(CASE WHEN WA.AREA_TYPE = 'COUNTY' THEN WA.AREA_ID END) AS COUNTY " +
                "FROM WELL W " +
                "LEFT JOIN WELL_AREA WA ON W.UWI = WA.UWI " +
                "WHERE W.UWI = @UWI " +
                "GROUP BY W.UWI, W.WELL_NAME, W.ASSIGNED_FIELD, W.OPERATOR, W.CURRENT_STATUS, " +
                "W.PROFILE_TYPE, W.DEPTH_DATUM, " +
                "W.KB_ELEV, W.GROUND_ELEV, " +
                "W.FINAL_TD, W.LOG_TD, W.DRILL_TD, W.MAX_TVD, " +
                "W.SPUD_DATE, W.FINAL_DRILL_DATE, W.COMPLETION_DATE, W.ABANDONMENT_DATE, " +
                "W.SURFACE_LATITUDE, W.SURFACE_LONGITUDE, " +
                "W.BOTTOM_HOLE_LATITUDE, W.BOTTOM_HOLE_LONGITUDE, W.REMARK",
                new { UWI = uwi });
        }

        public async Task<IEnumerable<AreaItem>> SearchStatesAsync(string connectionString, string search)
        {
            using var conn = new SqlConnection(connectionString);
            return await conn.QueryAsync<AreaItem>("""
                SELECT AREA_ID AS AreaId, PREFERRED_NAME AS PreferredName
                FROM AREA
                WHERE AREA_TYPE = 'STATE'
                  AND PREFERRED_NAME LIKE @search
                ORDER BY PREFERRED_NAME
                """,
                new { search = $"%{search}%" });
        }

        public async Task<IEnumerable<AreaItem>> SearchCountiesAsync(
            string connectionString, string stateAreaId, string search)
        {
            using var conn = new SqlConnection(connectionString);
            return await conn.QueryAsync<AreaItem>(
                """
                SELECT a.AREA_ID AS AreaId, a.PREFERRED_NAME AS PreferredName
                FROM AREA a
                JOIN AREA_CONTAIN ac ON a.AREA_ID = ac.CONTAINED_AREA_ID
                                     AND a.AREA_TYPE = ac.CONTAINED_AREA_TYPE
                WHERE a.AREA_TYPE = 'COUNTY'
                  AND ac.CONTAINING_AREA_ID = @stateAreaId
                  AND ac.CONTAINING_AREA_TYPE = 'STATE'
                  AND a.PREFERRED_NAME LIKE @search
                ORDER BY a.PREFERRED_NAME
                """,
                new { stateAreaId, search = $"%{search}%" });
        }

        public async Task<AreaItem?> GetAreaByIdAsync(string connectionString, string areaId, string areaType)
        {
            using var conn = new SqlConnection(connectionString);
            return await conn.QuerySingleOrDefaultAsync<AreaItem>(
                """
                SELECT AREA_ID AS AreaId, PREFERRED_NAME AS PreferredName
                FROM AREA
                WHERE AREA_ID = @areaId
                  AND AREA_TYPE = @areaType
                """,
                new { areaId, areaType });
        }

        public async Task AddAreaAsync(
            string connectionString, string areaType, string areaId, string preferredName)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                await conn.ExecuteAsync(
                    """
                    IF NOT EXISTS (SELECT 1 FROM R_AREA_TYPE WHERE AREA_TYPE = @areaType)
                        INSERT INTO R_AREA_TYPE (AREA_TYPE, ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
                        VALUES (@areaType, @auditUser, @auditDate, @auditUser, @auditDate)
                    """,
                    new { areaType, auditUser = AuditUser, auditDate = DateTime.UtcNow }, tx);

                await conn.ExecuteAsync(
                    """
                    INSERT INTO AREA (AREA_ID, AREA_TYPE, PREFERRED_NAME, ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
                    VALUES (@areaId, @areaType, @preferredName, @auditUser, @auditDate, @auditUser, @auditDate)
                    """,
                    new { areaId, areaType, preferredName, auditUser = AuditUser, auditDate = DateTime.UtcNow }, tx);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task UpdateWellHeaderAsync(string connectionString, WellHeader header)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync("""
                    UPDATE WELL SET
                        WELL_NAME             = @WELL_NAME,
                        ASSIGNED_FIELD        = @ASSIGNED_FIELD,
                        OPERATOR              = @OPERATOR,
                        CURRENT_STATUS        = @CURRENT_STATUS,
                        PROFILE_TYPE          = @PROFILE_TYPE,
                        DEPTH_DATUM           = @DEPTH_DATUM,
                        KB_ELEV               = @KB_ELEV,
                        GROUND_ELEV           = @GROUND_ELEV,
                        FINAL_TD              = @FINAL_TD,
                        LOG_TD                = @LOG_TD,
                        DRILL_TD              = @DRILL_TD,
                        MAX_TVD               = @MAX_TVD,
                        SPUD_DATE             = @SPUD_DATE,
                        FINAL_DRILL_DATE      = @FINAL_DRILL_DATE,
                        COMPLETION_DATE       = @COMPLETION_DATE,
                        ABANDONMENT_DATE      = @ABANDONMENT_DATE,
                        SURFACE_LATITUDE      = @SURFACE_LATITUDE,
                        SURFACE_LONGITUDE     = @SURFACE_LONGITUDE,
                        BOTTOM_HOLE_LATITUDE  = @BOTTOM_HOLE_LATITUDE,
                        BOTTOM_HOLE_LONGITUDE = @BOTTOM_HOLE_LONGITUDE,
                        REMARK                = @REMARK
                    WHERE UWI = @UWI
                    """, header, transaction);

                await UpsertWellArea(connection, transaction, header.UWI, "STATE", header.STATE);
                await UpsertWellArea(connection, transaction, header.UWI, "COUNTY", header.COUNTY);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static async Task UpsertWellArea(
            SqlConnection connection,
            SqlTransaction transaction,
            string uwi,
            string areaType,
            string? areaId)
        {
            if (string.IsNullOrWhiteSpace(areaId))
            {
                await connection.ExecuteAsync(
                    "DELETE FROM WELL_AREA WHERE UWI = @UWI AND AREA_TYPE = @AREA_TYPE",
                    new { UWI = uwi, AREA_TYPE = areaType }, transaction);
            }
            else
            {
                await connection.ExecuteAsync(
                    """
                    MERGE WELL_AREA AS target
                    USING (SELECT @UWI AS UWI, @AREA_TYPE AS AREA_TYPE, @AREA_ID AS AREA_ID) AS source
                        ON target.UWI = source.UWI AND target.AREA_TYPE = source.AREA_TYPE
                    WHEN MATCHED THEN
                        UPDATE SET
                            AREA_ID          = source.AREA_ID,
                            ROW_CHANGED_BY   = @auditUser,
                            ROW_CHANGED_DATE = @auditDate
                    WHEN NOT MATCHED THEN
                        INSERT (UWI, AREA_TYPE, AREA_ID, SOURCE, ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
                        VALUES (source.UWI, source.AREA_TYPE, source.AREA_ID, @source, @auditUser, @auditDate, @auditUser, @auditDate);
                    """,
                    new
                    {
                        UWI = uwi,
                        AREA_TYPE = areaType,
                        AREA_ID = areaId,
                        source = AuditUser,
                        auditUser = AuditUser,
                        auditDate = DateTime.UtcNow
                    }, transaction);
            }
        }

        public async Task<IEnumerable<Well>> GetWellsAsync(
            string connectionString,
            WellFilter filter,
            IEnumerable<string>? wellListUwis = null)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

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
            return await connection.QueryAsync<Well>(sql, new
            {
                filter.UWI,
                filter.WellName,
                filter.Operator,
                filter.AssignedField,
                filter.County,
                filter.State
            });
        }

        public async Task<int> GetWellCountAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WELL");
        }

        public async Task<IEnumerable<string>> GetReferenceValuesAsync(string connectionString, string tableName, string columnName)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<string>(
                $"SELECT DISTINCT {columnName} FROM {tableName} WHERE {columnName} IS NOT NULL ORDER BY {columnName}");
        }

        public async Task AddReferenceValueAsync(string connectionString, string table, string column, string value)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.ExecuteAsync(
                $"INSERT INTO {table} ({column}) VALUES (@value)",
                new
                {
                    value,
                    auditUser = AuditUser,
                    auditDate = DateTime.UtcNow
                });
        }

        public async Task AddCountyAsync(
            string connectionString, string areaId, string preferredName, string parentStateAreaId)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                await conn.ExecuteAsync(
                    """
                    IF NOT EXISTS (SELECT 1 FROM R_AREA_TYPE WHERE AREA_TYPE = 'COUNTY')
                        INSERT INTO R_AREA_TYPE (AREA_TYPE, ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
                        VALUES ('COUNTY', @auditUser, @auditDate, @auditUser, @auditDate)
                    """,
                    new { auditUser = AuditUser, auditDate = DateTime.UtcNow }, tx);

                await conn.ExecuteAsync(
                    """
                    INSERT INTO AREA (AREA_ID, AREA_TYPE, PREFERRED_NAME, ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
                    VALUES (@areaId, 'COUNTY', @preferredName, @auditUser, @auditDate, @auditUser, @auditDate)
                    """,
                    new { areaId, preferredName, auditUser = AuditUser, auditDate = DateTime.UtcNow }, tx);

                await conn.ExecuteAsync(
                    """
                    INSERT INTO AREA_CONTAIN (
                        CONTAINING_AREA_ID, CONTAINING_AREA_TYPE,
                        CONTAINED_AREA_ID,  CONTAINED_AREA_TYPE,
                        SOURCE,
                        ROW_CREATED_BY, ROW_CREATED_DATE,
                        ROW_CHANGED_BY, ROW_CHANGED_DATE)
                    VALUES (
                        @parentStateAreaId, 'STATE',
                        @areaId,            'COUNTY',
                        @source,
                        @auditUser, @auditDate,
                        @auditUser, @auditDate)
                    """,
                    new
                    {
                        parentStateAreaId,
                        areaId,
                        source = AuditUser,
                        auditUser = AuditUser,
                        auditDate = DateTime.UtcNow
                    }, tx);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> InsertWellAsync(string connectionString, WellHeader header)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var inserted = await connection.ExecuteScalarAsync<int>("""
            IF NOT EXISTS (SELECT 1 FROM WELL WHERE UWI = @UWI)
            BEGIN
                INSERT INTO WELL (
                    UWI,
                    WELL_NAME,
                    OPERATOR,
                    ASSIGNED_FIELD,
                    CURRENT_STATUS,
                    PROFILE_TYPE,
                    DEPTH_DATUM,
                    KB_ELEV,
                    GROUND_ELEV,
                    FINAL_TD,
                    LOG_TD,
                    DRILL_TD,
                    MAX_TVD,
                    SPUD_DATE,
                    FINAL_DRILL_DATE,
                    COMPLETION_DATE,
                    ABANDONMENT_DATE,
                    SURFACE_LATITUDE,
                    SURFACE_LONGITUDE,
                    BOTTOM_HOLE_LATITUDE,
                    BOTTOM_HOLE_LONGITUDE,
                    REMARK,
                    ROW_CREATED_BY,
                    ROW_CREATED_DATE,
                    ROW_CHANGED_BY,
                    ROW_CHANGED_DATE
                ) VALUES (
                    @UWI,
                    @WELL_NAME,
                    @OPERATOR,
                    @ASSIGNED_FIELD,
                    @CURRENT_STATUS,
                    @PROFILE_TYPE,
                    @DEPTH_DATUM,
                    @KB_ELEV,
                    @GROUND_ELEV,
                    @FINAL_TD,
                    @LOG_TD,
                    @DRILL_TD,
                    @MAX_TVD,
                    @SPUD_DATE,
                    @FINAL_DRILL_DATE,
                    @COMPLETION_DATE,
                    @ABANDONMENT_DATE,
                    @SURFACE_LATITUDE,
                    @SURFACE_LONGITUDE,
                    @BOTTOM_HOLE_LATITUDE,
                    @BOTTOM_HOLE_LONGITUDE,
                    @REMARK,
                    @AuditUser,
                    @AuditDate,
                    @AuditUser,
                    @AuditDate
                )
                SELECT 1
            END
            ELSE
                SELECT 0
            """,
                    new
                    {
                        header.UWI,
                        header.WELL_NAME,
                        header.OPERATOR,
                        header.ASSIGNED_FIELD,
                        header.CURRENT_STATUS,
                        header.PROFILE_TYPE,
                        header.DEPTH_DATUM,
                        header.KB_ELEV,
                        header.GROUND_ELEV,
                        header.FINAL_TD,
                        header.LOG_TD,
                        header.DRILL_TD,
                        header.MAX_TVD,
                        header.SPUD_DATE,
                        header.FINAL_DRILL_DATE,
                        header.COMPLETION_DATE,
                        header.ABANDONMENT_DATE,
                        header.SURFACE_LATITUDE,
                        header.SURFACE_LONGITUDE,
                        header.BOTTOM_HOLE_LATITUDE,
                        header.BOTTOM_HOLE_LONGITUDE,
                        header.REMARK,
                        AuditUser,
                        AuditDate = DateTime.UtcNow
                    },
                    transaction);

                if (inserted == 1)
                {
                    await UpsertWellArea(connection, transaction, header.UWI, "STATE", header.STATE);
                    await UpsertWellArea(connection, transaction, header.UWI, "COUNTY", header.COUNTY);
                }

                transaction.Commit();
                return inserted == 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private string BuildQuery(WellFilter filter, bool useWellList = false)
        {
            var where = new List<string>();
            var having = new List<string>();

            if (useWellList)
                where.Add("W.UWI IN (SELECT UWI FROM #WellList)");

            if (!string.IsNullOrWhiteSpace(filter.UWI))
                where.Add("W.UWI = @UWI");

            if (!string.IsNullOrWhiteSpace(filter.WellName))
                where.Add("W.WELL_NAME LIKE @WellName");

            if (!string.IsNullOrWhiteSpace(filter.Operator))
                where.Add("W.OPERATOR LIKE @Operator");

            if (!string.IsNullOrWhiteSpace(filter.AssignedField))
                where.Add("W.ASSIGNED_FIELD LIKE @AssignedField");

            if (!string.IsNullOrWhiteSpace(filter.County))
                having.Add("MAX(CASE WHEN WA.AREA_TYPE = 'COUNTY' THEN WA.AREA_ID END) LIKE @County");

            if (!string.IsNullOrWhiteSpace(filter.State))
                having.Add("MAX(CASE WHEN WA.AREA_TYPE = 'STATE' THEN WA.AREA_ID END) LIKE @State");

            var sql = "SELECT W.UWI, W.WELL_NAME, W.OPERATOR, " +
                      "W.ASSIGNED_FIELD, W.SPUD_DATE, " +
                      "W.SURFACE_LATITUDE, W.SURFACE_LONGITUDE, " +
                      "MAX(CASE WHEN WA.AREA_TYPE = 'STATE'  THEN WA.AREA_ID END) AS STATE, " +
                      "MAX(CASE WHEN WA.AREA_TYPE = 'COUNTY' THEN WA.AREA_ID END) AS COUNTY, " +
                      "MAX(CASE WHEN WA.AREA_TYPE = 'COUNTY' THEN A.PREFERRED_NAME END) AS COUNTY_NAME " +
                      "FROM WELL W " +
                      "LEFT JOIN WELL_AREA WA ON W.UWI = WA.UWI " +
                      "LEFT JOIN AREA A ON A.AREA_ID = WA.AREA_ID AND A.AREA_TYPE = WA.AREA_TYPE ";

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
