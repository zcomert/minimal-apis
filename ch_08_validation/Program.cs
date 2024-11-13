using System.ComponentModel.DataAnnotations;
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

app.MapGet("/api/books", () =>
{
    return Book.List.Count > 0
        ? Results.Ok(Book.List) // 200
        : Results.NoContent();  // 204
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.WithTags("CRUD", "GETs");

app.MapGet("/api/books/{id:int}", (int id) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    // Kitap var mÄ±?
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

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(newBook);

    bool isValid = Validator.TryValidateObject(newBook,
        context,
        validationResults,
        true);

    if (!isValid)
    {
        return Results.UnprocessableEntity(validationResults); // 422
    }

    newBook.Id = Book.List.Max(b => b.Id) + 1;    // otomatik
    Book.List.Add(newBook);
    return Results.Created($"/api/books/{newBook.Id}", newBook);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces<List<ValidationResult>>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");

app.MapPut("/api/books/{id:int}", (int id, Book updateBook) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(updateBook);
    var isValid = Validator.TryValidateObject(updateBook,
            context,
            validationResults,
            true);

    if (!isValid)
    {
        // LINQ - Stream API
        string errors = string.Join(".", 
            validationResults.Select(vr => vr.ErrorMessage));
        
        //foreach (var vr in validationResults)
        //{
        //    errors = errors + vr.ErrorMessage;
        //}
        throw new ValidationException(errors);
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
.Produces<ErrorDetails>(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");

app.MapDelete("/api/books/{id:int}", (int id) =>
{
    if (!(id > 0 && id <= 1000))
        throw new ArgumentOutOfRangeException("1-1000");

    var book = Book
        .List
        .FirstOrDefault(b => b.Id.Equals(id));

    if (book is null)
    {
        throw new BookNotFoundException(id); // 404
    }
    Book.List.Remove(book);
    return Results.NoContent();     // 204
})
.Produces(StatusCodes.Status204NoContent)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("CRUD");

app.MapGet("/api/books/search", (string? title) =>
{
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
    [Required]
    public int Id { get; set; }

    [MinLength(2, ErrorMessage = "Min len. must be 2")]
    [MaxLength(25, ErrorMessage = "Max len. must be 25")]
    public String? Title { get; set; }

    [Range(10, 100)]
    public Decimal Price { get; set; }

    // Seed data
    private static List<Book> _bookList = new List<Book>()
    {
        new Book { Id = 1, Title = "Ä°nce Memed", Price = 20.00M },
        new Book { Id = 2, Title = "KuyucaklÄ± Yusuf", Price = 15.50M },
        new Book { Id = 3, Title = "Ã‡alÄ±kuÅŸu", Price = 18.75M }
    };

    public static List<Book> List => _bookList;
}