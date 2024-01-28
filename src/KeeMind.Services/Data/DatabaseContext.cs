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
    private static string? _connectionString;

    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Attachment> Attachments => Set<Attachment>();


    public DatabaseContext()
    { }

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString ?? new SqliteConnectionStringBuilder { DataSource = "dummy.db" }.ToString());

#if DEBUG
        optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
#endif
    }

}

