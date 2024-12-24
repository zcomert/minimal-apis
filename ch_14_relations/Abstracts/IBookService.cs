using Entities;
using Entities.DTOs;

namespace Abstracts;

public interface IBookService
{
    int Count { get; }
    List<BookDto> GetBooks();
    BookDto? GetBookById(int id);
    Book AddBook(BookDtoForInsertion item);
    Book UpdateBook(int id, BookDtoForUpdate item);
    void DeleteBook(int id);
}
