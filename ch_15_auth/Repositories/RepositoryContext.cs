using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class RepositoryContext : IdentityDbContext<User>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }

    public RepositoryContext(DbContextOptions options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed the Category entities first
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                CategoryId = 1,
                CategoryName = "Felsefe"
            },
            new Category
            {
                CategoryId = 2,
                CategoryName = "Roman"
            },
            new Category
            {
                CategoryId = 3,
                CategoryName = "Deneme"
            }
        );

        // Seed the Book entities and reference the categories using CategoryId
        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "Devlet",
                Price = 20.00M,
                URL = "/images/1.jpg",
                CategoryId = 1
            },
            new Book
            {
                Id = 2,
                Title = "Ateşten Gömlek",
                Price = 15.50M,
                URL = "/images/1.jpg",
                CategoryId = 2
            },
            new Book
            {
                Id = 3,
                Title = "Huzur",
                Price = 18.75M,
                URL = "/images/1.jpg",
                CategoryId = 3
            }
        );
    
        // Seed the Role data
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole
            {
                Name = "User",
                NormalizedName = "USER"
            }
        );
    }
}