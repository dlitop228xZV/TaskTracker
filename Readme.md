* Task Tracker

Task Tracker — учебное веб-приложение для управления задачами.  
Проект реализован с использованием ASP.NET Core Web API и слоистой архитектуры (N-Layer).

Приложение позволяет:

- Создавать, редактировать и удалять задачи
- Назначать исполнителей
- Устанавливать дедлайн и приоритет
- Фильтровать задачи
- Получать отчёты
- Работать через Swagger UI

---

* Технологический стек

- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- xUnit (для unit-тестов)

---

* Требования

* SDK
- .NET 8 SDK (или версия, указанная в проекте)

* IDE (рекомендуется)
Visual Studio 2022+

* Структура
TaskTracker/
	TaskTracker.sln
	TaskTracker.Domain          → Доменные модели и бизнес-логика
	TaskTracker.Application     → DTO, сервисы, валидация
	TaskTracker.Infrastructure  → EF Core, DbContext, репозитории
	TaskTracker.WebAPI          → Контроллеры, Swagger, запуск API

** Инструкция по запуску проекта
1.Клонировать репозиторий
git clone https://github.com/dlitop228xZV/TaskTracker
cd TaskTracker
2.Применить миграции базы данных
dotnet ef database update --project TaskTracker.Infrastructure --startup-project TaskTracker.WebAPI
/* Если EF CLI не установлен:
dotnet tool install --global dotnet-ef */
3.Запустить проект
cd TaskTracker.WebAPI
dotnet run

* Swagger UI
http://localhost:5143/swagger
(не факт что 5143. Ифна с консоли в помощь)





