using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPDMStudio.Services
{
    using Dapper;
    using Microsoft.Data.Sqlite;
    using System.Data;
    using System.IO;

    namespace PPDMStudio.Services
    {
        public interface IDatabaseService
        {
            IDbConnection CreateConnection();
        }

        public class DatabaseService : IDatabaseService
        {
            private readonly string _dbPath;
            private readonly string _connectionString;

            public DatabaseService()
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PPDMStudio");

                Directory.CreateDirectory(folder);

                _dbPath = Path.Combine(folder, "ppdmstudio.db");
                _connectionString = $"Data Source={_dbPath}";

                InitializeDatabase();
            }

            public IDbConnection CreateConnection()
                => new SqliteConnection(_connectionString);

            private void InitializeDatabase()
            {
                using var db = CreateConnection();

                db.Execute("""
                CREATE TABLE IF NOT EXISTS WellLists (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name        TEXT NOT NULL,
                    Description TEXT,
                    ProjectId   TEXT NOT NULL,
                    CreatedDate TEXT NOT NULL,
                    ModifiedDate TEXT NOT NULL
                );
                """);

                db.Execute("""
                CREATE TABLE IF NOT EXISTS WellListItems (
                    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    WellListId INTEGER NOT NULL,
                    UWI        TEXT NOT NULL,
                    FOREIGN KEY (WellListId) REFERENCES WellLists(Id)
                );
                """);

                db.Execute("""
                CREATE TABLE IF NOT EXISTS FilterPresets (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name        TEXT NOT NULL,
                    ProjectId   TEXT NOT NULL,
                    FilterJson  TEXT NOT NULL,
                    CreatedDate TEXT NOT NULL
                );
                """);
            }
        }
    }
}
