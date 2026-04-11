# Система ETL для интеграции данных о персонале

Данный проект реализует систему извлечения, преобразования и загрузки (ETL) данных из двух разрозненных источников в единое хранилище (DWH) с использованием стека .NET 10 и PostgreSQL.


## 🏗 Архитектура системы

### Система состоит из трех независимых реляционных баз данных:

Source_Projects_DB: Информация о сотрудниках, проектах и ролях.

Source_HR_DB: Кадровые данные (резюме) и финансовые показатели (оклады, премии).

Data_Warehouse: Итоговое хранилище с очищенными и консолидированными данными.

### Технологический стек

- Runtime: .NET 10

- ORM: Entity Framework Core (Npgsql)

- БД: PostgreSQL 16+

- BPMN: Camunda Modeler (описание алгоритма)


## 📊 Проектирование БД

### Обоснование выбора СУБД

### Для реализации выбрана PostgreSQL, так как она:

- Обеспечивает высокую скорость работы с индексами типа B-tree для быстрого поиска по ФИО.

- Поддерживает тип данных UUID, что позволяет генерировать идентификаторы на стороне приложения, избегая конфликтов при слиянии данных.

- Имеет отличную поддержку финансовых типов данных (NUMERIC), гарантирующих точность расчетов.

### Типы данных

- Guid (UUID): Первичные ключи в источниках.

- decimal (numeric(18,2)): Для финансовых полей (Salary, Bonus, TotalIncome).

- text: Для длинных описаний (резюме, описание проектов).

- int (identity): Суррогатные ключи в Хранилище для оптимизации производительности запросов.


Диаграмма классов (UML)

Ниже представлена структура сущностей во всех трех базах данных и их связи.

![Изображение диаграммы классов](/docs/class_diagram.png)

## ⚙️ Алгоритм ETL

### Процесс описан в формате BPMN (Base.bpmn) и включает следующие этапы:

![Изображение BPMN](/docs/etl_process.png)

## 🚀 Запуск проекта

### 1. Настройка БД

Создайте три пустые базы данных в PostgreSQL:
```sql
CREATE DATABASE source_projects;
CREATE DATABASE source_hr;
CREATE DATABASE data_warehouse;
```
### 2. Настройка конфигурации

Укажите строки подключения в appsettings.json для проекта Lab2.App.

### 3. Применение миграций

Выполните команды в консоли в корне проекта для создания таблиц:
```
dotnet ef migrations add InitialProjects --context ProjectDbContext --project Data --startup-project App
dotnet ef database update --context ProjectDbContext --project Data --startup-project App

dotnet ef migrations add InitialHr --context HrDbContext --project Data --startup-project App
dotnet ef database update --context HrDbContext --project Data --startup-project App

dotnet ef migrations add InitialWarehouse --context WarehouseDbContext --project Data --startup-project App
dotnet ef database update --context WarehouseDbContext --project Data --startup-project App
```
### 4. Запуск ETL

Запустите консольное приложение. Система автоматически:

- Заполнит источники тестовыми данными (Seeding).

- Выполнит алгоритм сопоставления.

- Выведет отчет о количестве синхронизированных записей.


## 📁 Структура решения

Lab2.Models: POCO-классы сущностей, разделенные по пространствам имен (Source1, Source2, Warehouse).

Lab2.Data: Контексты Entity Framework Core.

Lab2.EtlEngine: Сервисы трансформации и нормализации данных.

Lab2.App: Точка входа и конфигурация DI.
