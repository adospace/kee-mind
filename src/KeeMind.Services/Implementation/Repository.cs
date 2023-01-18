using KeeMind.Services.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KeeMind.Services.Implementation;

internal class Repository : IRepository
{
    private readonly ISettingsStorage _settings;
    private string? _currentDatabaseConnectionString;
    private string _defaultDatabasePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeeMind", "KeeMind.db");

    public Repository(ISettingsStorage settings)
    {
        _settings = settings;
    }

    private string GetDbPath()
    {
        var dbName = _settings.Get("DB_NAME", null);
        return dbName == null ? _defaultDatabasePath : Path.Combine(Path.GetDirectoryName(_defaultDatabasePath).ThrowIfNull(), dbName + ".db");
    }

    public bool ArchiveExists()
        => File.Exists(_defaultDatabasePath);

    public void CloseArchive()
    {
        _currentDatabaseConnectionString = null;
    }

    public async Task CreateArchive(string PIN)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_defaultDatabasePath).ThrowIfNull());

        var dbPath = GetDbPath();

        if (File.Exists(dbPath))
        {
            throw new InvalidOperationException($"Database file already existing at: {dbPath}");
        }

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Password = PIN.ThrowIfNull()// PRAGMA key is being sent from EF Core directly after opening the connection
        }.ToString();

        using var db = new DatabaseContext(connectionString);

        await db.Database.MigrateAsync();

        _currentDatabaseConnectionString = connectionString;
    }

    public void DeleteArchive()
    {
        //if (_currentDatabaseConnectionString != null)
        {
            SqliteConnection.ClearAllPools();
            var dbPath = GetDbPath();
            File.Delete(dbPath);
        }
    }

    public DatabaseContext OpenArchive()
    {
        return new DatabaseContext(_currentDatabaseConnectionString.ThrowIfNull());
    }

    public async Task<DatabaseContext?> TryOpenArchive(string PIN)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_defaultDatabasePath).ThrowIfNull());

        var dbPath = GetDbPath();

        if (!File.Exists(dbPath))
        {
            throw new InvalidOperationException();
        }

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Password = PIN.ThrowIfNull()// PRAGMA key is being sent from EF Core directly after opening the connection
        }.ToString();

        try
        {
            var db = new DatabaseContext(connectionString);

            await db.Database.MigrateAsync();

            _currentDatabaseConnectionString = connectionString;

            return db;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
        }
    }
}
