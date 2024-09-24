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

app.MapGet("/hello", () => "Hello World.");
app.MapPost("/hello", () => "Hello World.");
app.MapPut("/hello", () => "Hello World.");
app.MapDelete("/hello", () => "Hello World.");

app.Run();

