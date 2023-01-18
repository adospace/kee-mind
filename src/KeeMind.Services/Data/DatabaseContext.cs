using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Services.Data;

public class DatabaseContext : DbContext
{
    private readonly string? _connectionString;

    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Tag> Tags => Set<Tag>();

    //private SqliteConnection? _connection;

    public DatabaseContext()
    { }

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    //internal DatabaseContext(string databaseFile)
    //{
    //    if (!string.IsNullOrEmpty(databaseFile)) _dbFile = databaseFile;
    //}

    //internal DatabaseContext(SqliteConnection sqliteConnection)
    //{
    //    if (!string.IsNullOrEmpty(sqliteConnection?.DataSource)) _dbFile = sqliteConnection.DataSource;
    //    _connection = sqliteConnection;
    //}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //_connection ??= new SqliteConnection(_connectionString ?? new SqliteConnectionStringBuilder { DataSource = "dummy.db" }.ToString());
        optionsBuilder.UseSqlite(_connectionString ?? new SqliteConnectionStringBuilder { DataSource = "dummy.db" }.ToString());

#if DEBUG
        optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
#endif
    }

}

