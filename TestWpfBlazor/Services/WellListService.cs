using Dapper;
using PPDMStudio.Models;
using PPDMStudio.Services.PPDMStudio.Services;
using System.Data;

namespace PPDMStudio.Services
{
    public interface IWellListService
    {
        Task<IEnumerable<WellList>> GetWellListsAsync(string projectId);
        Task<IEnumerable<string>> GetWellListItemsAsync(int wellListId);
        Task<int> SaveWellListAsync(WellList wellList);
        Task AddWellsToListAsync(int wellListId, IEnumerable<string> uwis);
        Task RemoveWellFromListAsync(int wellListId, string uwi);
        Task DeleteWellListAsync(int wellListId);
        event Action? OnChange;
    }

    public class WellListService : IWellListService
    {
        private readonly IDatabaseService _db;
        public event Action? OnChange;

        public WellListService(IDatabaseService db)
            => _db = db;

        public async Task<IEnumerable<WellList>> GetWellListsAsync(string projectId)
        {
            using var db = _db.CreateConnection();
            return await db.QueryAsync<WellList>(
                "SELECT * FROM WellLists WHERE ProjectId = @ProjectId ORDER BY Name",
                new { ProjectId = projectId });
        }

        public async Task<IEnumerable<string>> GetWellListItemsAsync(int wellListId)
        {
            using var db = _db.CreateConnection();
            return await db.QueryAsync<string>(
                "SELECT UWI FROM WellListItems WHERE WellListId = @WellListId ORDER BY UWI",
                new { WellListId = wellListId });
        }

        public async Task<int> SaveWellListAsync(WellList wellList)
        {
            using var db = _db.CreateConnection();

            if (wellList.Id == 0)
            {
                // Insert new
                var id = await db.ExecuteScalarAsync<int>("""
                    INSERT INTO WellLists (Name, Description, ProjectId, CreatedDate, ModifiedDate)
                    VALUES (@Name, @Description, @ProjectId, @CreatedDate, @ModifiedDate);
                    SELECT last_insert_rowid();
                    """, wellList);

                wellList.Id = id;
            }
            else
            {
                // Update existing
                await db.ExecuteAsync("""
                    UPDATE WellLists 
                    SET Name = @Name, 
                        Description = @Description,
                        ModifiedDate = @ModifiedDate
                    WHERE Id = @Id
                    """, wellList);
            }

            NotifyStateChanged();
            return wellList.Id;
        }

        public async Task AddWellsToListAsync(int wellListId, IEnumerable<string> uwis)
        {
            using var db = _db.CreateConnection();

            foreach (var uwi in uwis)
            {
                // Avoid duplicates
                var exists = await db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM WellListItems WHERE WellListId = @WellListId AND UWI = @UWI",
                    new { WellListId = wellListId, UWI = uwi });

                if (exists == 0)
                    await db.ExecuteAsync(
                        "INSERT INTO WellListItems (WellListId, UWI) VALUES (@WellListId, @UWI)",
                        new { WellListId = wellListId, UWI = uwi });
            }

            NotifyStateChanged();
        }

        public async Task RemoveWellFromListAsync(int wellListId, string uwi)
        {
            using var db = _db.CreateConnection();
            await db.ExecuteAsync(
                "DELETE FROM WellListItems WHERE WellListId = @WellListId AND UWI = @UWI",
                new { WellListId = wellListId, UWI = uwi });

            NotifyStateChanged();
        }

        public async Task DeleteWellListAsync(int wellListId)
        {
            using var db = _db.CreateConnection();

            await db.ExecuteAsync(
                "DELETE FROM WellListItems WHERE WellListId = @WellListId",
                new { WellListId = wellListId });

            await db.ExecuteAsync(
                "DELETE FROM WellLists WHERE Id = @Id",
                new { Id = wellListId });

            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}