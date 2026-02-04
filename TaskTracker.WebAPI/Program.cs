using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tasktracker.db"));

// Register application services
builder.Services.AddScoped<ITaskService, TaskService>();
// Добавим позже: builder.Services.AddScoped<IUserService, UserService>();
// Добавим позже: builder.Services.AddScoped<IReportService, ReportService>();

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

// Create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

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