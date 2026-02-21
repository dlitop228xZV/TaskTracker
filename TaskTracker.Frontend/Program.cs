var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку статических файлов
builder.Services.AddControllers();

var app = builder.Build();

// Настраиваем поддержку статических файлов из папки wwwroot
app.UseDefaultFiles(); // index.html будет открываться по умолчанию
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Добавляем простой API для проверки (необязательно)
app.MapGet("/api/status", () => new { status = "Frontend is running" });

app.Run();