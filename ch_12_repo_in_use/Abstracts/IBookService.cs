namespace Abstracts;

public interface IBookService
{
    int Count { get; }
    List<Book> GetBooks();
    Book? GetBookById(int id);
    void AddBook(Book item);
    Book UpdateBook(int id, Book item);
    void DeleteBook(int id);
}
