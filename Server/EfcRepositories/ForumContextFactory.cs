using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfcRepositories;

public class ForumContextFactory : IDesignTimeDbContextFactory<ForumContext>
{
    public ForumContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ForumContext> builder = new();
        builder.UseSqlite("Data Source=app.db");
        return new ForumContext(builder.Options);
    }
}