# Task Tracker

Task Tracker --- это веб-приложение для управления задачами с поддержкой
фильтрации, отчётов, статусов и аналитики.

Проект реализован по архитектуре Clean Architecture с разделением на
слои:

-   Domain
-   Application
-   Infrastructure
-   WebAPI (REST + Frontend)

------------------------------------------------------------------------

# Архитектура проекта

## Структура слоёв

TaskTracker.Domain - Entities - Enums - Interfaces

TaskTracker.Application - DTOs - Services - Interfaces - Validators

TaskTracker.Infrastructure - Data - Repositories - Migrations

TaskTracker.WebAPI - Controllers - Middleware - wwwroot (Frontend)

## Диаграмма зависимостей

WebAPI\
↓\
Application\
↓\
Domain\
↑\
Infrastructure

-   Domain --- бизнес-сущности\
-   Application --- бизнес-логика\
-   Infrastructure --- EF Core + БД\
-   WebAPI --- REST API + Frontend

------------------------------------------------------------------------

# Как запустить проект

## 1. Открыть решение

Открыть файл:

TaskTracker.sln

в Visual Studio.

## 2. Выбрать Startup Project

TaskTracker.WebAPI

## 3. Применить миграции

В Package Manager Console:

Update-Database

## 4. Запустить

Нажать F5.

После запуска открыть:

https://localhost:xxxx/index.html

Swagger:

https://localhost:xxxx/swagger

------------------------------------------------------------------------

# Frontend

Frontend реализован на:

-   HTML5 (семантическая разметка)
-   CSS3 (Flexbox, Grid, Media Queries)
-   Vanilla JavaScript (Fetch API)

## Структура

TaskTracker.WebAPI/wwwroot

-   index.html
-   reports.html
-   edit.html

css/ - styles.css

js/ - api.js - tasks-page.js - reports-page.js - edit-page.js

## Возможности

-   Создание задачи
-   Редактирование (модальное окно)
-   Удаление
-   Фильтрация
-   Подсветка Overdue
-   Отчёты с bar chart
-   Адаптивный дизайн (mobile \<768px)

------------------------------------------------------------------------

# API Endpoints

## Tasks

GET /api/tasks --- Получить список задач\
GET /api/tasks/{id} --- Получить задачу\
POST /api/tasks --- Создать задачу\
PUT /api/tasks/{id} --- Обновить задачу\
DELETE /api/tasks/{id} --- Удалить задачу

### Query параметры

/api/tasks?status=New\
/api/tasks?assigneeId=1\
/api/tasks?dueAfter=2025-01-01\
/api/tasks?dueBefore=2025-12-31\
/api/tasks?tags=1,2

Поддерживаемые статусы:

-   New
-   InProgress
-   Done
-   Overdue

------------------------------------------------------------------------

## Users

GET /api/users

------------------------------------------------------------------------

## Tags

GET /api/tags

------------------------------------------------------------------------

## Reports

GET /api/reports/status-summary\
GET /api/reports/overdue-by-assignee\
GET /api/reports/avg-completion-time

------------------------------------------------------------------------

# E2E тестирование (ручное)

Сценарий проверки:

1.  Создать задачу
2.  Проверить отображение
3.  Отредактировать
4.  Изменить статус
5.  Проверить Overdue
6.  Применить фильтры
7.  Перейти в отчёты
8.  Удалить задачу

Проверено в:

-   Chrome

------------------------------------------------------------------------

# Known Issues

1.  Нет авторизации (все пользователи имеют полный доступ).
2.  Нет сортировки по колонкам.

------------------------------------------------------------------------

# Технологии

-   .NET 8
-   ASP.NET Core Web API
-   Entity Framework Core
-   SQL Server
-   HTML5 / CSS3 / JavaScript

------------------------------------------------------------------------