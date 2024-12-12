
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Abstracts;
using Entities;
using Entities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;

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
    .AddScoped<BookRepository>();

builder.Services
    .AddScoped<IBookService, BookServiceV3>();

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


