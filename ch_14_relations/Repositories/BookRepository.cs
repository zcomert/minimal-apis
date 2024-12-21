using Entities;

namespace Repositories;

public class BookRepository
{
    private readonly RepositoryContext _context;

    public BookRepository(RepositoryContext context)
    {
        _context = context;
    }

    public Book? Get(int id) =>
         _context.Books.FirstOrDefault(b => b.Id.Equals(id));

    public List<Book> GetAll() =>
        _context.Books.ToList();

    public void Add(Book item)
    {
        _context.Books.Add(item);
        _context.SaveChanges();
    }
    public void Remove(Book item)
    {
        _context.Remove(item);
        _context.SaveChanges();
    }
    public void Update(Book item)
    {
        _context.Books.Update(item);
        _context.SaveChanges();
    }
}