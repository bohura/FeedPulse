# FeedPulse AI Handoff

## Purpose

This file is a handoff summary for another AI assistant or another machine.
It captures the current project state, decisions already made, and the next
recommended steps.

This is not a full transcript. It is the distilled context that matters for
continuing the work.

## Project Summary

- Product name: `FeedPulse`
- Goal: build a personal RSS system with:
  - backend API
  - PostgreSQL database
  - Railway deployment
  - GitHub Actions CI/CD
  - Android client later
- Current backend stack choice: `ASP.NET Core`
- Deployment target: `Railway`
- Database target: `PostgreSQL`
- Dev environment: `VS Code Dev Container`

## Key Decisions Already Made

- Do not use Go for this project.
- Do not start with microservices.
- Start with one backend service and one database.
- Build the backend first, mobile client later.
- Use explicit naming like `Entities`, `Data`, `Contracts` instead of a vague
  `Models` bucket.
- Prefer a minimal working backend first, then split into
  `Api / Application / Domain / Infrastructure`.

## Repository Location

- Host path: `/home/bohura/FeedPulse`

## Current Repository State

Top-level structure currently includes:

- `.devcontainer/`
- `.github/`
- `.gitignore`
- `FeedPulse.slnx`
- `src/FeedPulse.Api/`
- `dotnet/`

Current API project file:

- [FeedPulse.Api.csproj](/home/bohura/FeedPulse/src/FeedPulse.Api/FeedPulse.Api.csproj)

Current installed packages:

- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

## Current Code State

### Program Entry

[Program.cs](/home/bohura/FeedPulse/src/FeedPulse.Api/Program.cs) is still the
default minimal template with:

- `AddControllers()`
- `AddOpenApi()`
- `MapOpenApi()` in development
- `MapControllers()`

No database registration has been added yet.

### Health Endpoint

[HelloWorldController.cs](/home/bohura/FeedPulse/src/FeedPulse.Api/Controllers/HelloWorldController.cs)
exists and returns:

- `GET /health` -> `"health"`
- `GET /version` -> `"version"`

This confirms the ASP.NET Core app starts and controller routing works.

### Database Context

[AppDbContext.cs](/home/bohura/FeedPulse/src/FeedPulse.Api/Data/AppDbContext.cs)
exists but is currently empty.

## What Has Already Been Verified

- The API can run locally.
- `GET /health` works.
- The user has already run the project with:

```bash
dotnet run --project src/FeedPulse.Api/FeedPulse.Api.csproj
```

- The user was confused by `dotnet run` at repository root because the root
  contains a solution file, not a runnable project.

## Important Context From The Conversation

- The user is new to this exact workflow and needs concrete, command-level
  guidance.
- Abstract architecture explanations were not landing well.
- The user prefers direct, step-by-step instructions like:
  - what file to create
  - what code to put in it
  - what command to run next
- The user asked whether database classes should go under `Models`; the answer
  given was:
  - database table classes should be called `Entities`
  - `DbContext` should live under `Data` or `Persistence`
  - API input/output classes should later live under `Contracts` or `Dtos`

## Recommended Immediate Next Step

Do not add more controllers yet.

The next task should be:

1. implement `AppDbContext`
2. create the first entity, probably `Feed`
3. add a PostgreSQL connection string to
   [appsettings.Development.json](/home/bohura/FeedPulse/src/FeedPulse.Api/appsettings.Development.json)
4. register `AppDbContext` in
   [Program.cs](/home/bohura/FeedPulse/src/FeedPulse.Api/Program.cs)
5. run the first EF Core migration
6. run `database update`

The goal is to prove the app can connect to PostgreSQL and create at least one
table.

## Suggested First Concrete File Layout

Inside `src/FeedPulse.Api/`, the next minimal structure should be:

```text
Controllers/
Data/
Entities/
Program.cs
appsettings.json
appsettings.Development.json
```

Then later split into separate projects:

```text
src/
  FeedPulse.Api/
  FeedPulse.Application/
  FeedPulse.Domain/
  FeedPulse.Infrastructure/
```

But that split should happen after the first database-backed version works.

## Suggested Next Commands

If PostgreSQL is available locally, the next likely commands are:

```bash
dotnet ef migrations add InitialCreate --project src/FeedPulse.Api/FeedPulse.Api.csproj
dotnet ef database update --project src/FeedPulse.Api/FeedPulse.Api.csproj
```

If `dotnet ef` is missing:

```bash
dotnet tool install --global dotnet-ef
```

## Constraints

- Keep the project simple.
- Do not over-design early.
- Do not jump to mobile yet.
- Do not invent extra services or layers before the DB-backed API works.

## What Cannot Be Transferred Automatically

- The full chat transcript is not embedded here.
- Internal assistant-only skills or hidden system instructions cannot be copied
  to another AI session.

What this file provides instead is the practical equivalent:

- current state
- technical decisions
- user preferences
- next implementation target
