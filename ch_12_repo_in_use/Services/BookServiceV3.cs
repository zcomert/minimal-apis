using Abstracts;
using Repositories;

namespace Services;

public class BookServiceV3 : IBookService
{
    private readonly BookRepository _bookRepo;

    public BookServiceV3(BookRepository bookRepo)
    {
        _bookRepo = bookRepo;
    }

    public int Count => _bookRepo.GetAll().Count;

    public void AddBook(Book item)
    {
        _bookRepo.Add(item);
    }

    public void DeleteBook(int id)
    {
        var book = _bookRepo.Get(id);
        if (book is not null)
            _bookRepo.Remove(book);
        else
            throw new BookNotFoundException(id);
    }

    public Book? GetBookById(int id) =>
        _bookRepo.Get(id);


    public List<Book> GetBooks() =>
        _bookRepo.GetAll();


    public Book UpdateBook(int id, Book item)
    {
        var book = _bookRepo.Get(id);
        if (book is null)
        {
            throw new BookNotFoundException(id);
        }

        book.Title = item.Title;
        book.Price = item.Price;
        _bookRepo.Update(book);
        return book;
    }
}
