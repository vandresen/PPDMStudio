using Dapper;
using Microsoft.Data.SqlClient;
using PPDMStudio.Models;

namespace PPDMStudio.Services
{
    public interface IMarkerpickService
    {
        Task<IEnumerable<MarkerPick>> GetMarkerPicksAsync(string connectionString, string uwi);
        Task UpdateMarkerPickAsync(string connectionString, MarkerPick pick);
        Task InsertMarkerPickAsync(string connectionString, MarkerPick pick);
        Task DeleteMarkerPickAsync(string connectionString, string uwi, string stratUnitId, string stratNameSetId, string interpId);
    }

    public class MarkerpickService : IMarkerpickService
    {
        private static string AuditUser => Environment.UserName;

        public async Task<IEnumerable<MarkerPick>> GetMarkerPicksAsync(string connectionString, string uwi)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<MarkerPick>("""
                SELECT
                    UWI,
                    STRAT_NAME_SET_ID,
                    STRAT_UNIT_ID,
                    INTERP_ID,
                    PICK_DEPTH,
                    PICK_TVD,
                    PICK_DATE,
                    DOMINANT_LITHOLOGY,
                    REMARK
                FROM STRAT_WELL_SECTION
                WHERE UWI = @uwi
                ORDER BY PICK_DEPTH
                """,
                new { uwi });
        }

        public async Task UpdateMarkerPickAsync(string connectionString, MarkerPick pick)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("""
                UPDATE STRAT_WELL_SECTION SET
                    PICK_DEPTH         = @PICK_DEPTH,
                    PICK_TVD           = @PICK_TVD,
                    PICK_DATE          = @PICK_DATE,
                    DOMINANT_LITHOLOGY = @DOMINANT_LITHOLOGY,
                    REMARK             = @REMARK,
                    ROW_CHANGED_BY     = @auditUser,
                    ROW_CHANGED_DATE   = @auditDate
                WHERE UWI               = @UWI
                  AND STRAT_NAME_SET_ID = @STRAT_NAME_SET_ID
                  AND STRAT_UNIT_ID     = @STRAT_UNIT_ID
                  AND INTERP_ID         = @INTERP_ID
                """,
                new
                {
                    pick.PICK_DEPTH,
                    pick.PICK_TVD,
                    pick.PICK_DATE,
                    pick.DOMINANT_LITHOLOGY,
                    pick.REMARK,
                    pick.UWI,
                    pick.STRAT_NAME_SET_ID,
                    pick.STRAT_UNIT_ID,
                    pick.INTERP_ID,
                    auditUser = AuditUser,
                    auditDate = DateTime.UtcNow
                });
        }

        public async Task InsertMarkerPickAsync(string connectionString, MarkerPick pick)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("""
                INSERT INTO STRAT_WELL_SECTION (
                    UWI,
                    STRAT_NAME_SET_ID,
                    STRAT_UNIT_ID,
                    INTERP_ID,
                    PICK_DEPTH,
                    PICK_TVD,
                    PICK_DATE,
                    DOMINANT_LITHOLOGY,
                    REMARK,
                    ROW_CREATED_BY,
                    ROW_CREATED_DATE,
                    ROW_CHANGED_BY,
                    ROW_CHANGED_DATE
                ) VALUES (
                    @UWI,
                    @STRAT_NAME_SET_ID,
                    @STRAT_UNIT_ID,
                    @INTERP_ID,
                    @PICK_DEPTH,
                    @PICK_TVD,
                    @PICK_DATE,
                    @DOMINANT_LITHOLOGY,
                    @REMARK,
                    @auditUser,
                    @auditDate,
                    @auditUser,
                    @auditDate
                )
                """,
                new
                {
                    pick.UWI,
                    pick.STRAT_NAME_SET_ID,
                    pick.STRAT_UNIT_ID,
                    pick.INTERP_ID,
                    pick.PICK_DEPTH,
                    pick.PICK_TVD,
                    pick.PICK_DATE,
                    pick.DOMINANT_LITHOLOGY,
                    pick.REMARK,
                    auditUser = AuditUser,
                    auditDate = DateTime.UtcNow
                });
        }

        public async Task DeleteMarkerPickAsync(
            string connectionString, string uwi, string stratUnitId, string stratNameSetId, string interpId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("""
                DELETE FROM STRAT_WELL_SECTION
                WHERE UWI               = @uwi
                  AND STRAT_UNIT_ID     = @stratUnitId
                  AND STRAT_NAME_SET_ID = @stratNameSetId
                  AND INTERP_ID         = @interpId
                """,
                new { uwi, stratUnitId, stratNameSetId, interpId });
        }
    }
}
