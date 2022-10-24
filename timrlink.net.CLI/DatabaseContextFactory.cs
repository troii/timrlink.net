using Microsoft.EntityFrameworkCore.Design;

namespace timrlink.net.CLI
{
    /// <summary>
    /// This class is needed so that we can run all sorts of dotnet ef commands. In My case it was useful to drop
    /// database for example to start from scratch: dotenet ef database drop ....
    /// </summary>
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            return new DatabaseContext(args[0]);
        }
    }
}