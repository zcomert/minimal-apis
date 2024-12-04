using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Book API",
        Version = "v1",
        Description = "Virtual Campus",
        License = new(),
        TermsOfService = new("https://www.samsun.edu.tr"),
        Contact = new()
        {
            Email = "zcomert@samsun.edu.tr",
            Name = "Zafer Cömert",
            Url = new Uri("https://www.youtube.com/@virtual.campus")
        }
    });
});

builder.Services.AddCors(options =>
{
    // all
    options.AddPolicy("all", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });

    // special
    options.AddPolicy("special", builder =>
    {
        builder.WithOrigins("https://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// DI Registration
builder.Services
    .AddSingleton<IBookService, BookService>();

builder.Services.AddDbContext<RepositoryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("sqlite")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("all");
app.UseHttpsRedirection();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes
                .Status500InternalServerError;

        context.Response.ContentType = "application/json";

        var contextFeature = context
            .Features
            .Get<IExceptionHandlerFeature>();

        if (contextFeature is not null)
        {
            context.Response.StatusCode = contextFeature.Error switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status422UnprocessableEntity,
                ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError,
            };

            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = contextFeature.Error.Message,
            }.ToString()
            );
        }
    });
});


app.MapGet("/api/error", () =>
{
    throw new Exception("An error has been occured.");
})
.Produces<ErrorDetails>(StatusCodes.Status500InternalServerError)
.ExcludeFromDescription();

app.MapGet("/api/books", (IBookService bookService) =>
{
    return bookService.Count > 0
        ? Results.Ok(bookService.GetBooks()) // 200
        : Results.NoContent();  // 204
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CRUD", "GETs");

app.MapGet("/api/books/{id:int}", (int id, IBookService bookService) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    // Kitap var mi?
    var book = bookService.GetBookById(id);

    return book is not null
        ? Results.Ok(book)      // 200
        : throw new BookNotFoundException(id);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("GETs");

app.MapPost("/api/books", (Book newBook, IBookService bookService) =>
{
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(newBook);
    var isValid = Validator
        .TryValidateObject(newBook, context, validationResults, true);

    if (!isValid)
        return Results.UnprocessableEntity(validationResults);

    bookService.AddBook(newBook);

    return Results.Created($"/api/books/{newBook.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");

app.MapPut("/api/books/{id:int}", (int id, Book updateBook, IBookService bookService) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(updateBook);
    var isValid = Validator
        .TryValidateObject(updateBook, context, validationResults, true);

    if (!isValid)
    {
        var errors = string.Join(" ",
            validationResults.Select(v => v.ErrorMessage));

        throw new ValidationException(errors);
    }


    var book = bookService.UpdateBook(id, updateBook);
    return Results.Ok(book);    // 200 
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.Produces<ErrorDetails>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");

app.MapDelete("/api/books/{id:int}", (int id, IBookService bookService) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    bookService.DeleteBook(id);
    return Results.NoContent();     // 204
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");

app.MapGet("/api/books/search", (string? title, IBookService bookService) =>
{
    var books = string.IsNullOrEmpty(title)
        ? bookService.GetBooks()
        : bookService
            .GetBooks()
            .Where(b => b.Title != null &&
                   b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();

    return books.Any()
        ? Results.Ok(books)     // 200
        : Results.NoContent();  // 204
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("GETs");

app.Run();


public abstract class NotFoundException : Exception
{
    protected NotFoundException(string message) : base(message)
    {

    }
}

public sealed class BookNotFoundException : NotFoundException
{
    public BookNotFoundException(int id) : base($"The book with {id} could not be found!")
    {

    }
}

public class ErrorDetails
{
    public int StatusCode { get; set; }
    public String? Message { get; set; }
    public String? AtOccured => DateTime.Now.ToLongDateString();

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class Book
{
    [Required]
    public int Id { get; set; }

    [MinLength(2, ErrorMessage = "The title should include at least two characters.")]
    [MaxLength(25, ErrorMessage = "The title must be 25 characters or less.")]
    public String? Title { get; set; }

    [Range(1, 100, ErrorMessage = "Price must be between 1 and 100.")]
    public Decimal Price { get; set; }
}

interface IBookService
{
    int Count { get; }
    List<Book> GetBooks();
    Book? GetBookById(int id);
    void AddBook(Book item);
    Book UpdateBook(int id, Book item);
    void DeleteBook(int id);
}

class BookService : IBookService
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