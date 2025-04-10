using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Environment2D> Environment2Ds { get; set; }
        public DbSet<Object2D> Object2Ds { get; set; }
    }
}
