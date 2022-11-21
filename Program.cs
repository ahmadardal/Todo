using Microsoft.EntityFrameworkCore;
using MinimalApi;
using MinimalApi.Model;
;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Våra apier lägger vi här

// Hämtar alla Todo's i våran tabell
app.MapGet("/todoItems", async (DataContext db) =>
{
    return await db.Todos.ToListAsync();
});

app.MapGet("/todoItems/complete", async (DataContext db) => 
{

    return await db.Todos.Where(todo => todo.isComplete).ToListAsync();

});

app.MapGet("/todoItems/{id}", async (int id, DataContext db) =>
{

    var todo = await db.Todos.FindAsync(id);

    if (todo == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(todo);

});

// Lägger upp en Todo i våran tabell
app.MapPost("/todoItems", async (Todo todo, DataContext db) =>
{

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoItems/{todo.id}", todo);

});

app.MapPut("/todoItems/{id}", async (int id, Todo updatedTodo, DataContext db) =>
{

    var todo = await db.Todos.FindAsync(id);

    if (todo == null)
    {
        return Results.NotFound();
    }

    todo.Name = updatedTodo.Name;
    todo.isComplete = updatedTodo.isComplete;

    await db.SaveChangesAsync();
    return Results.Ok("Updated2 successfully!");

});


app.MapDelete("/todoItems/{id}", async (int id, DataContext db) =>
{

    var todo = await db.Todos.FindAsync(id);

    if (todo == null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.Ok($"{todo.Name} is removed successfully!");

});


var bookings = new List<string>();

app.MapGet("/getBookings", () =>
{
    return bookings;
});

app.MapPost("/book/{name}", (string name) =>
{

    bookings.Add(name);

    return Results.Created("Booked sucessfully!", null);

});



app.Run();
