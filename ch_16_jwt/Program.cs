using Abstracts;
using Configuration;
using Entities;
using Entities.DTOs;
using Entities.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddCustomSwagger();
builder.Services.AddCustomCors();

builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.AddAuthorization();


// DI Registration
builder.Services
    .AddScoped<BookRepository>();

builder.Services
    .AddScoped<IBookService, BookServiceV3>();

builder.Services
    .AddScoped<IAuthService, AuthenticationManager>();

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
app.UseCustomExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();



app.MapGet("/api/error", () =>
{
    throw new Exception("An error has been occured.");
})
.Produces<ErrorDetails>(StatusCodes.Status500InternalServerError)
.ExcludeFromDescription();

app.MapGet("/api/books", [Authorize(Roles ="User")](IBookService bookService) =>
{
    return bookService.Count > 0
        ? Results.Ok(bookService.GetBooks()) // 200
        : Results.NoContent();  // 204
})
.Produces<List<Book>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status204NoContent)
.RequireAuthorization()
.WithTags("CRUD", "GETs");

app.MapGet("/api/books/{id:int}", (int id, IBookService bookService) =>
{
    var book = bookService.GetBookById(id);
    return Results.Ok(book);
})
.Produces<Book>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status404NotFound)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("GETs");

app.MapPost("/api/books", (BookDtoForInsertion newBook, IBookService bookService) =>
{
    var book = bookService.AddBook(newBook);
    return Results.Created($"/api/books/{book.Id}", book.Id);
})
.Produces<Book>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status422UnprocessableEntity)
.WithTags("CRUD");

app.MapPut("/api/books/{id:int}", (int id, BookDtoForUpdate updateBook, IBookService bookService) =>
{
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


app.MapPost("/api/auth", async (UserForRegistrationDto userDto,
    IAuthService authService) =>
{
    var result = await authService
        .RegisterUserAsync(userDto);

    return result.Succeeded
        ? Results.Ok(result)
        : Results.BadRequest(result.Errors);
})
.Produces<IdentityResult>(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status400BadRequest)
.WithTags("Auth");


app.MapPost("/api/login", async (UserForAuthenticationDto userDto,
    IAuthService authService) =>
{
    if (!await authService.ValidateUserAsync(userDto))
        return Results.Unauthorized(); // 401
    return Results.Ok(new
    {
        Token = await authService.CreateTokenAsync()
    });
})
.Produces(StatusCodes.Status200OK)
.Produces<ErrorDetails>(StatusCodes.Status401Unauthorized)
.WithTags("Auth");



app.Run();


