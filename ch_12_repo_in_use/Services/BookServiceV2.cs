using System.Net.Mime;
using Abstracts;
using Repositories;

namespace Services;

public class BookServiceV2 : IBookService
{
    private readonly RepositoryContext _context;

    public BookServiceV2(RepositoryContext context)
    {
        _context = context;
    }

    public int Count => _context.Books.ToList().Count;

    public void AddBook(Book item)
    {
        _context.Books.Add(item);
        _context.SaveChanges();
    }

    public void DeleteBook(int id)
    {
        var book = _context.Books.FirstOrDefault(b => b.Id.Equals(id));
        if (book is not null)
        {
            _context.Books.Remove(book);
            _context.SaveChanges();
        }
        else
        {
            throw new BookNotFoundException(id);
        }
    }

    public Book? GetBookById(int id) =>
        _context.Books.FirstOrDefault(b => b.Id.Equals(id));

    public List<Book> GetBooks() =>
        _context.Books.ToList();


    public Book UpdateBook(int id, Book item)
    {
        var book = _context.Books.FirstOrDefault(b => b.Id.Equals(id));
        if (book is null)
        {
            throw new BookNotFoundException(id);
        }

        book.Title = item.Title;
        book.Price = item.Price;
        _context.SaveChanges();
        return book;
    }
}
