using Microsoft.EntityFrameworkCore.Design;

namespace timrlink.net.CLI
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            return new DatabaseContext(args[0]);
        }
    }
}