using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;

var builder = WebApplication.CreateBuilder(args);

// Настройка DbContext (ВАЖНО!)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tasktracker.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Создание БД
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Заполняем начальными данными
    if (!db.Tags.Any())
    {
        db.Tags.AddRange(
            new Tag { Name = "bug" },
            new Tag { Name = "feature" },
            new Tag { Name = "refactor" },
            new Tag { Name = "docs" }
        );

        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Name = "Иванов И.И.",
                Email = "ivanov@example.com"
            });
        }

        db.SaveChanges();
    }
}

app.Run();