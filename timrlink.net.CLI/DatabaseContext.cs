using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace timrlink.net.CLI
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ProjectTime> ProjectTimes { get; set; }
        public DbSet<Metadata> Metadata { get; set; }

        public DbSet<Group> Group { get; set; }
        public DbSet<GroupUsers> GroupUsers { get; set; }
        
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Metadata>();

            modelBuilder.Entity<GroupUsers>()
                .HasKey(gu => new {gu.GroupId, gu.UserUUID });

            modelBuilder.Entity<GroupUsers>()
                .HasOne<Group>()
                .WithMany(g => g.GroupUsers)
                .HasForeignKey(gu => gu.GroupId);
        }

        public async Task<Metadata> GetMetadata(string key) => await Metadata.SingleOrDefaultAsync(m => m.Key == key);

        public async Task SetMetadata(Metadata metadata)
        {
            var existing = await GetMetadata(metadata.Key);
            if (existing != null)
            {
                existing.Value = metadata.Value;
                Metadata.Update(existing);
            }
            else
            {
                await Metadata.AddAsync(metadata);
            }

            await SaveChangesAsync();
        }
    }

    public class Metadata
    {
        public const string KEY_LAST_PROJECTTIME_IMPORT = "lastprojecttimeimport";

        public Metadata(string key, string value)
        {
            Key = key;
            Value = value;
        }

        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }

    public class ProjectTime
    {
        [Key]
        public Guid UUID { get; set; }
        public string User { get; set; }
        public Guid? UserUUID { get; set; }
        public string UserExternalId { get; set; }
        public string UserEmployeeNr { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public long Duration { get; set; }
        public int BreakTime { get; set; }
        public bool Changed { get; set; }
        public bool Closed { get; set; }
        public string StartPosition { get; set; }
        public string EndPosition { get; set; }
        public DateTimeOffset LastModifiedTime { get; set; }
        public string Task { get; set; }
        public string Description { get; set; }
        public bool Billable { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public Guid? TaskUUID { get; set; }
        public string? TaskExternalId { get; set; }
    }

    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string ExternalId { get; set; }
        public string ParentalExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<GroupUsers> GroupUsers { get; set; }
    }

    [Keyless]
    public class GroupUsers
    {
        public long GroupId { get; set; }
        public string UserUUID { get; set; }
    }

    internal static class DbSetExtensions
    {
        public static async Task AddOrUpdateRange<T>(this DbSet<T> dbSet, IEnumerable<T> dbEntities)
            where T : class
        {
            var asNoTracking = dbSet.AsNoTracking();
            foreach (var entity in dbEntities)
            {
                await AddOrUpdateInternal(dbSet, asNoTracking, entity);
            }
        }

        public static async Task AddOrUpdate<T>(this DbSet<T> dbSet, T entity) where T : class
        {
            await AddOrUpdateInternal(dbSet, dbSet.AsNoTracking(), entity);
        }

        private static async Task AddOrUpdateInternal<T>(DbSet<T> dbSet, IQueryable<T> noTracking, T entity) where T : class
        {
            if (await noTracking.ContainsAsync(entity))
            {
                dbSet.Update(entity);
            }
            else
            {
                await dbSet.AddAsync(entity);
            }
        }
    }

    internal static class DbContextExtensions
    {
        public static async Task InitializeDatabase(this DatabaseContext context, ILogger logger)
        {
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Any())
            {
                const string efMigrationsHistoryTable = "__EFMigrationsHistory";

                bool migrationExists;
                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '{efMigrationsHistoryTable}';";
                    await context.Database.OpenConnectionAsync();

                    var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                    migrationExists = result > 0;
                }

                bool tablesExist;
                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND (TABLE_NAME = 'ProjectTimes' OR TABLE_NAME = 'Metadata');";
                    await context.Database.OpenConnectionAsync();

                    var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                    tablesExist = result > 0;
                }

                if (tablesExist && !migrationExists)
                {
                    logger.LogInformation("Database already has existing tables before running migrations, marking initial migration as already done");

                    // When tables already exist but no migration exists we know that database was created without 
                    // migrations. So we insert the first migration 20221020122606_InitialMigration manually
                    // Then we can switch to migrations managed by Entity Framework
                    var firstMigrationName = pendingMigrations.First();
                    var version = Assembly.GetAssembly(typeof(DbContext)).GetName().Version;
                    if (firstMigrationName != null && version != null)
                    {
                        var versionString = $"{version.Major}.{version.Minor}.{version.Build}";

                        await context.Database.ExecuteSqlRawAsync($"CREATE TABLE {efMigrationsHistoryTable}(MigrationId nvarchar(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY, ProductVersion nvarchar(32) NOT NULL);");
                        await context.Database.ExecuteSqlRawAsync(
                            $"INSERT INTO {efMigrationsHistoryTable} (MigrationId, ProductVersion) VALUES ('{firstMigrationName}', '{versionString}')");
                    }
                }

                // Run the remaining migrations
                pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Running Database Migration... ({pendingMigrations})", string.Join(", ", pendingMigrations));
                    await context.Database.MigrateAsync();
                }
            }
        }
    }
}
