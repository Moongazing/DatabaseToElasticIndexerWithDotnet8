using Microsoft.EntityFrameworkCore;
using Moongazing.DbToElasticIndexer.Entities;

namespace Moongazing.DbToElasticIndexer.Data;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<Product> Product { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>();
    }
}



