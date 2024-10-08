var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/books", () =>
{
    return Results.Ok(Book.List); // 200
});

app.MapGet("/api/books/{id:int}", (int id) =>
{
    // Kitap var mı?
    var book = Book
        .List
        .FirstOrDefault(b => b.Id.Equals(id));

    return book is not null
        ? Results.Ok(book)      // 200
        : Results.NotFound();   // 404
});

app.MapPost("/api/books", (Book newBook) =>
{
    newBook.Id = Book.List.Max(b => b.Id) + 1;    // otomatik
    Book.List.Add(newBook);
    return Results.Created($"/api/books/{newBook.Id}", newBook);
});

app.MapPut("/api/books/{id:int}", (int id, Book updateBook) =>
{
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
});

app.MapDelete("/api/books/{id:int}", (int id) =>
{
    var book = Book
        .List
        .FirstOrDefault(b => b.Id.Equals(id));

    if (book is null)
    {
        return Results.NotFound();
    }
    Book.List.Remove(book);
    return Results.NoContent();     // 204
});

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
});

app.Run();


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