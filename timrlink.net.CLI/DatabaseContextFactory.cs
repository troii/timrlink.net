using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace timrlink.net.CLI
{
    /// <summary>
    /// This class is needed so that we can run all sorts of dotnet ef commands. In My case it was useful to drop
    /// database for example to start from scratch: dotnet ef database drop ....
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer(args[0])
                .Options;
            return new DatabaseContext(options);
        }
    }
}
