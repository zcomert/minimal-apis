using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Book API",
        Description = "ASP.NET Core Web API projesidir.",
        Version = "v1",
        License = new() { },
        TermsOfService = new Uri("https://wwww.google.com"),
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
    options.AddPolicy("All", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
    options.AddPolicy("Special", builder => 
    {
        builder.WithOrigins("http://localhost:3000",
                            "https://www.samsun.edu.tr")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("All");
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

app.MapGet("/api/books", () =>
{
    return Book.List.Any()
    ? Results.Ok(Book.List)
    : Results.NoContent();
})
.Produces<IEnumerable<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CRUD", "GETs");

app.MapGet("/api/books/{id:int}", (int id) =>
{
    if (!(id > 0 && id <= 1000))
    {
        throw new ArgumentOutOfRangeException("Id must be in range 1 - 1000.");
    }
    // Kitap var mı?
    var book = Book
        .List
        .FirstOrDefault(b => b.Id.Equals(id));

    return book is not null
        ? Results.Ok(book)      // 200
                                // : Results.NotFound();   // 404
        : throw new BookNotFoundException(id);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("GETs");

app.MapPost("/api/books", (Book newBook) =>
{
    newBook.Id = Book.List.Max(b => b.Id) + 1;    // otomatik
    Book.List.Add(newBook);
    return Results.Created($"/api/books/{newBook.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");

app.MapPut("/api/books/{id:int}", (int id, Book updateBook) =>
{
    // id kontrolü
    if (!(id > 0 && id < 1000))
    {
        throw new ArgumentOutOfRangeException("1-1000");
    }

    var book = Book
                .List
                .FirstOrDefault(b => b.Id.Equals(id));

    if (book is null)
    {
        throw new BookNotFoundException(id);  // 404 : Not found!
    }
    book.Title = updateBook.Title;
    book.Price = updateBook.Price;

    return Results.Ok(book);    // 200 
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");

app.MapDelete("/api/books/{id:int}", (int id) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    var book = Book
        .List
        .FirstOrDefault(b => b.Id.Equals(id));

    if (book is null)
        throw new BookNotFoundException(id);

    Book.List.Remove(book);
    return Results.NoContent();     // 204
})
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CRUD");

app.MapGet("/api/books/search", (string? title) =>
{
    if (!(title?.Length >= 3))
        throw new ArgumentException("The length of search term must be greater 2.");

    var books = string.IsNullOrEmpty(title)
        ? Book.List
        : Book
            .List
            .Where(b => b.Title != null &&
                   b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();

    return books.Any()
        ? Results.Ok(books)     // 200
        : Results.NoContent();  // 204
})
.Produces<IEnumerable<Book>>(StatusCodes.Status200OK)
.Produces<IEnumerable<Book>>(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
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

class Book
{
    public int Id { get; set; }
    public String? Title { get; set; }
    public Decimal Price { get; set; }

    // Seed data
    private static List<Book> _bookList = new List<Book>()
    {
        new Book { Id = 1, Title = "İnce Memed", Price = 20.00M },
        new Book { Id = 2, Title = "Kuyucaklı Yusuf", Price = 15.50M },
        new Book { Id = 3, Title = "Çalıkuşu", Price = 18.75M }
    };

    public static List<Book> List => _bookList;
}