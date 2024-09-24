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

var variableLambda = () => "[Variable] Hello World.";

app.MapGet("/hello", () => "Hello World.");         // inline
app.MapPost("/hello", variableLambda);              // lambda variable
app.MapPut("/hello", Hello);                        // local function
app.MapDelete("/hello", new HelloHandler().Hello);  // instance member


String Hello()
{
    return "[Local Function] Hello World.";
}


app.Run();

class HelloHandler
{
    public String? Hello()
    {
        return "[Instance member] Hello World";
    }
}
