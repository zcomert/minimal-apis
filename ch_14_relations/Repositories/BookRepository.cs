using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class BookRepository : RepositoryBase<Book>
{
    public BookRepository(RepositoryContext context) : base(context)
    {
        
    }

    public override Book? Get(int id) =>
         _context
            .Books
            .Include(b => b.Category) // eager loading
            .FirstOrDefault(b => b.Id.Equals(id));

    public override List<Book> GetAll() =>
        _context
            .Books
            .Include(b => b.Category) // eager loading
            .ToList();
    
    
}
