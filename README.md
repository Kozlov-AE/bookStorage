# BookStorage

Учебный проект для отработки навыков и экспериментов с современными технологиями. 
Представляет собой веб-приложение для управления личной коллекцией книг с возможностью 
категоризации, поиска и хранения файлов.

Попытка воспроизвести **чистую архитектуру** на бэкенде и **минимально сложную архитектуру** 
на фронтенде - с соблюдением современных практик.

## Tech Stack

| Слой | Технология |
|------|-----------|
| Backend | ASP.NET 10 Minimal API, C# |
| Frontend | React 19, TypeScript 6, Vite 8, Tailwind CSS 4 |
| Database | SQLite (EF Core) |
| API Client | TanStack React Query, Orval (codegen) |
| Infrastructure | Docker, Docker Compose, Nginx |

## Roadmap

- [ ] PostgreSQL в качестве основного хранилища (с возможностью переключения)
- [ ] Redis для кеширования часто запрашиваемых данных
- [ ] IdentityServer / ASP.NET Identity для авторизации и аутентификации
- [ ] Ролевая модель: управление хранилищем книг в зависимости от прав пользователя
- [ ] Полноценный UI-редизайн с переходом на SCSS-модули
- [ ] Доработка недостающего функционала (редактирование, удаление, пагинация, поиск)

## Требования для разработки

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (опционально, для контейнерной разработки)
- IDE: [Rider](https://www.jetbrains.com/rider/) или [Visual Studio 2022+](https://visualstudio.microsoft.com/)

## Быстрый старт (локальная разработка)

### Бэкенд

```bash
dotnet run --project src/BookStorage.Api
# → http://localhost:5189, Swagger по адресу /swagger
```

### Фронтенд

```bash
cd client/bookStorage.Spa
npm install
npm run dev
# → http://localhost:5173, прокси /api → localhost:5189
```

### База данных

SQLite создаётся и мигрируется автоматически при первом запуске. По умолчанию данные 
хранятся в папке приложения:

```
./data/
├── db\booksbookStorage.sqlite   # База данных
├── books\                       # Загруженные файлы книг
└── metadata\                    # Метаданные / обложки
```

При запуске через Docker пути задаются переменными окружения в `docker-compose.yml` 
и могут быть переопределены.

## Docker: окружение для разработки

Горячая перезагрузка кода с поддержкой отладчика:

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build -d
```

| Сервис | URL | Описание |
|--------|-----|----------|
| Frontend | http://localhost:5173 | Vite dev server с HMR |
| API | http://localhost:5189 | ASP.NET под `dotnet run` + `vsdbg` |

Прокси настроен автоматически: Vite перенаправляет `/api/*` в контейнер с бэкендом.

### Отладка

Образ для разработки содержит `vsdbg` — отладчик Microsoft .NET. Подключение из IDE:

**Rider:** Services (`Alt+8`) → Docker → containers → `bookstorage-api-1` → Debug

**Visual Studio:** Debug → Attach to Process → Transport: `Docker (Linux Container)` → 
Выбрать `bookstorage-api-1` → Attach to `dotnet`

После изменений в коде достаточно перезапустить контейнер:

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml restart api
```

## Docker: production-окружение

```bash
docker compose up --build -d
# → http://localhost
```

| Сервис | URL | Описание |
|--------|-----|----------|
| Frontend | http://localhost | Nginx раздаёт SPA и проксирует `/api` |
| API | Внутренний (порт 8080) | Напрямую не выставляется |

Данные сохраняются в `C:\Temp\bookStorage\` (задаётся через `environment` 
в `docker-compose.yml`, переопределяя пути по умолчанию):

```
C:\Temp\bookStorage\
├── db\          # SQLite
├── books\       # Загруженные файлы книг
└── metadata\    # Метаданные / обложки (на будущее)
```

При локальном запуске (без Docker) используется `./data/` внутри папки приложения.

## Структура проекта

```
├── src/
│   ├── BookStorage.Api/          # ASP.NET Minimal API, эндпоинты, DTO
│   ├── BookStorage.Core/         # Доменные сущности, интерфейсы сервисов
│   └── BookStorage.Infrastructure/ # EF Core, репозитории, файловое хранилище
├── client/
│   └── bookStorage.Spa/          # React SPA
│       ├── src/api/generated/    # Orval-генерация API-клиента (не редактировать)
│       └── src/components/       # React-компоненты
├── tests/
│   └── BookStorage.Aplication.UnitTests/ # xUnit-тесты
├── backend.Dockerfile            # Multi-stage: build → debug → runtime
├── frontend.Dockerfile           # Multi-stage: node build → nginx
├── docker-compose.yml            # Production-сборка
└── docker-compose.dev.yml        # Dev-переопределения
```

## Команды

```bash
# Бэкенд
dotnet build              # Собрать решение
dotnet test               # Запустить тесты
dotnet run --project src/BookStorage.Api  # Запустить API

# Фронтенд (внутри client/bookStorage.Spa)
npm run dev               # Vite dev server
npm run build             # Production-сборка
npm run generate          # Перегенерировать API-клиент из OpenAPI-спецификации
```
