using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MinimalApi;
using MinimalApi.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {

    options.TokenValidationParameters = new TokenValidationParameters() 
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Våra apier lägger vi här


app.MapPost("/register", async (User user, DataContext db) => {

    // Kolla ifall alla obligatoriska fält är ifyllda.

    // Validera mejlen

    // Dubbelkolla att det inte existerar en användare med samma mejl.

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("User registered!");

});

app.MapPost("/login", async (UserLogin userLogin, DataContext db) => {

    User? user = await db.Users.FirstOrDefaultAsync(u => u.Email.Equals(userLogin.Email) && u.Password.Equals(userLogin.Password));

    if (user == null) {
        return Results.NotFound();
    }

    var secretkey = builder.Configuration["Jwt:Key"];

    if (secretkey == null) {
        return Results.StatusCode(500);
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.GivenName, user.Name),
        new Claim(ClaimTypes.Surname, user.Name),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken
    (
        issuer: builder.Configuration["Jwt:Issuer"],
        audience: builder.Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(5),
        notBefore: DateTime.UtcNow,
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey)),
        SecurityAlgorithms.HmacSha256)
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(tokenString);

});


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
app.MapPost("/todoItems", [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")] async (Todo todo, DataContext db) =>
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
