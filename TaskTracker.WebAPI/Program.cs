using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Interfaces;
using TaskTracker.Infrastructure.Data;
using TaskTracker.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tasktracker.db"));

// Register Repository
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Register Services
builder.Services.AddScoped<ITaskService, TaskService>();

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
            new TaskTracker.Domain.Entities.Tag { Name = "bug" },
            new TaskTracker.Domain.Entities.Tag { Name = "feature" },
            new TaskTracker.Domain.Entities.Tag { Name = "refactor" },
            new TaskTracker.Domain.Entities.Tag { Name = "docs" }
        );

        if (!db.Users.Any())
        {
            db.Users.Add(new TaskTracker.Domain.Entities.User
            {
                Name = "Иванов И.И.",
                Email = "ivanov@example.com"
            });
        }

        db.SaveChanges();
    }
}

app.Run();