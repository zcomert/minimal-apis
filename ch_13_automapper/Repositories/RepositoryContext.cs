using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class RepositoryContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public RepositoryContext(DbContextOptions options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "Devlet", Price = 20.00M },
            new Book { Id = 2, Title = "Ateşten Gömlek", Price = 15.50M },
            new Book { Id = 3, Title = "Huzur", Price = 18.75M }
        );
    }
}