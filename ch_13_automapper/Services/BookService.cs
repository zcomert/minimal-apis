using Abstracts;
using Entities;
using Entities.Exceptions;

namespace Services;

public class BookService : IBookService
{
    private readonly List<Book> _bookList;
    public BookService()
    {
        // seed data
        _bookList = new List<Book>()
        {
            new Book { Id = 1, Title = "Devlet", Price = 20.00M },
            new Book { Id = 2, Title = "Ateşten Gömlek", Price = 15.50M },
            new Book { Id = 3, Title = "Huzur", Price = 18.75M }
        };
    }

    public List<Book> GetBooks() => _bookList;

    public int Count => _bookList.Count;

    public Book? GetBookById(int id) =>
        _bookList.FirstOrDefault(b => b.Id.Equals(id));

    public void AddBook(Book newBook)
    {
        newBook.Id = _bookList.Max(b => b.Id) + 1;
        _bookList.Add(newBook);
    }

    public Book UpdateBook(int id, Book updateBook)
    {
        var book = _bookList.FirstOrDefault(b => b.Id.Equals(id));
        if (book is null)
        {
            throw new BookNotFoundException(id);
        }

        book.Title = updateBook.Title;
        book.Price = updateBook.Price;

        return book;
    }

    public void DeleteBook(int id)
    {
        var book = _bookList.FirstOrDefault(b => b.Id.Equals(id));
        if (book is not null)
        {
            _bookList.Remove(book);
        }
        else
        {
            throw new BookNotFoundException(id);
        }
    }
}