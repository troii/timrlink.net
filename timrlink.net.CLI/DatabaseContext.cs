using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace timrlink.net.CLI
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ProjectTime> ProjectTimes { get; set; }
        public DbSet<Metadata> Metadata { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Metadata>();
            modelBuilder.Entity<ProjectTime>();
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
        public string? UserExternalId { get; set; }
        public string? UserEmployeeNr { get; set; }
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

    internal class Group
    {
        [Key] 
        public long Id { get; set; }

        public string ExternalId { get; set; }

        public string ParentalExternalId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
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

        public static async Task AddOrUpdate<T>(DbSet<T> dbSet, T entity) where T : class
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
}
