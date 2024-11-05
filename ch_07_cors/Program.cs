using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Book Api",
        Version = "v1",
        Description = "Virtual Campus",
        License = new(),
        TermsOfService = new("https://www.youtube.com/@virtual.campus"),
        Contact = new()
        {
            Name = "Zafer CÖMERT",
            Email = "zcomert@samsun.edu.tr",
            Url = new("http://www.samsun.edu.tr")
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
    options.AddPolicy("Private", builder => 
    {
        builder.WithOrigins("https://www.samsun.edu.tr","https://localhost:3000")
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
    return Book.List.Count > 0
     ? Results.Ok(Book.List)
     : Results.NotFound();
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithTags("CRUD", "GETs");

app.MapGet("/api/books/{id:int}", (int id) =>
{
    if (!(id >= 1 && id <= 1000))
        throw new ArgumentOutOfRangeException($"{id}"); // 400 BadRequest

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
    if (!(id >= 1 && id <= 1000))
        throw new ArgumentOutOfRangeException($"{id}"); // 400 BadRequest

    var book = Book
                .List
                .FirstOrDefault(b => b.Id.Equals(id));

    if (book is null)
    {
        return Results.NotFound();  // 404 : Not found!
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
    if (!(id >= 1 && id <= 1000))
        throw new ArgumentOutOfRangeException($"{id}"); // 400 BadRequest

    var book = Book
        .List
        .FirstOrDefault(b => b.Id.Equals(id));

    if (book is null)
    {
        return Results.NotFound();
    }
    Book.List.Remove(book);
    return Results.NoContent();     // 204
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.WithTags("CRUD");

app.MapGet("/api/books/search", (string? title) =>
{
    if (title?.Length <= 2)
        throw new ArgumentException("The length of search term must be at least 3 characters.");

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