# FeedPulse AI Handoff

## Purpose
This file captures the current project state so another AI can continue without rereading the full chat.

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
- Dev environment: `VS Code Dev Container` and `Visual Studio`
- User preference: guide step-by-step, do not dump finished code unless explicitly asked

## Key Decisions Already Made
- Do not use Go for this project.
- Do not start with microservices.
- Start with one backend service and one database.
- Build the backend first, mobile client later.
- Use explicit naming like `Entities`, `Data`, and `Contracts` instead of a vague `Models` bucket.
- Prefer a minimal working backend first, then split into `Api / Application / Domain / Infrastructure` later.

## Repository Location
- Repo path: `C:/Users/Bohura/source/repos/bohura/FeedPulse`
- API project: `C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/FeedPulse.Api.csproj`

## Current Repository State
Top-level structure currently includes:

- `.devcontainer/`
- `.github/`
- `.gitignore`
- `FeedPulse.slnx`
- `docs/`
- `src/FeedPulse.Api/`
- `dotnet/`

## Current Code State

### Program Entry
[`Program.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Program.cs) currently:

- registers controllers
- registers Swagger/OpenAPI
- registers `IFeedSyncService` -> `FeedSyncService`
- registers `AppDbContext`
- uses PostgreSQL via `UseNpgsql`

### Configuration
[`appsettings.json`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/appsettings.json) currently contains the PostgreSQL connection string under `ConnectionStrings:WebDatabase`.

### Entities
[`Feed.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Entities/Feed.cs) exists with:

- `Id`
- `Title`
- `Url`
- `CreatedAt`
- `IsActive`
- `FeedItems` navigation collection

[`FeedItem.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Entities/FeedItem.cs) exists with:

- `Id`
- `FeedId`
- `Feed` navigation
- `ExternalId` nullable
- `Title`
- `Link`
- `Summary` nullable
- `PublishedAt` nullable
- `CreatedAt`

### Database Context
[`AppDbContext.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Data/AppDbContext.cs) exists and exposes:

- `DbSet<Feed> Feeds`
- `DbSet<FeedItem> FeedItems`

### API Controllers
[`FeedsController.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Controllers/FeedsController.cs) currently implements CRUD for feeds:

- `GET /feeds`
- `GET /feeds/{id}`
- `POST /feeds`
- `PUT /feeds/{id}`
- `DELETE /feeds/{id}`

It uses request DTOs under `Contracts/Feeds`.

[`FeedItemsController.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Controllers/FeedItemsController.cs) now provides read-only item browsing by feed:

- `GET /feeds/{feedId}/items`
- `GET /feeds/{feedId}/items/{id}`

It filters by `feedId`, orders items by `PublishedAt ?? CreatedAt` descending, and uses `AsNoTracking()` for read queries.

### Sync Service
[`FeedSyncService.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Services/FeedSyncService.cs) and [`IFeedSyncService.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Services/IFeedSyncService.cs) are now in place.

Current sync behavior:

- `POST /feeds/{id}/sync` is wired through `FeedsController`
- downloads the feed XML from the stored feed URL
- parses RSS `<rss>/<channel>/<item>` entries
- de-duplicates items before insert using `FeedId + ExternalId`
- stores new `FeedItem` rows in PostgreSQL
- converts `PublishedAt` to UTC before saving, to satisfy PostgreSQL `timestamp with time zone`
- returns a `FeedSyncResult` with `FeedId`, `FeedTitle`, `FetchedCount`, `AddedCount`, `Success`, and `ErrorMessage`

Atom parsing is present in the service, but the Atom branch is still only counting entries and has not been wired to full persistence yet.

### Contracts
[`CreateFeedRequest.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Contracts/Feeds/CreateFeedRequest.cs)
and [`UpdateFeedRequest.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Contracts/Feeds/UpdateFeedRequest.cs)
exist and are used by the feeds controller.

### Migrations
EF Core migrations already exist:

- `InitialCreate`
- `AddFeedItems`

The `FeedItems` table, primary key, foreign key to `Feeds`, and index on `FeedId` have already been generated.

### Template / Stray File Note
There is also a root-level [`AppDbContext.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/AppDbContext.cs) at the repo root containing a template `Class1`.
It is not the real DbContext and should not be confused with [`src/FeedPulse.Api/Data/AppDbContext.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Data/AppDbContext.cs).

### Request File
[`FeedPulse.Api.http`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/FeedPulse.Api.http) is still the default template file and has not been updated yet.

## What Has Already Been Verified
- The PostgreSQL container workflow works for this project.
- EF migrations can create the tables successfully.
- The database now contains `Feeds` and `FeedItems`.
- The user has already been able to run migration/update workflows successfully from the API project.
- `POST /feeds/{id}/sync` successfully downloads an RSS feed and inserts `FeedItem` rows into the database.
- Example successful sync result observed in Swagger: `fetchedCount = 20`, `addedCount = 20`, `success = true`.

## Important Context From The Conversation
- The user is new to this exact workflow and needs concrete, step-by-step guidance.
- Abstract architecture explanations were not landing well.
- The user prefers direct instructions like:
  - which file to touch
  - what concept the file is for
  - what command to run next
- The user explicitly asked to be guided rather than handed a fully finished implementation.

## Recommended Next Step
Do not jump into background jobs yet.

Best next feature:

1. finish Atom persistence so RSS and Atom behave consistently
2. verify repeat sync is idempotent and does not duplicate rows
3. later move the sync logic into a background worker

## Guidance Style Going Forward
When continuing this project:

- explain one step at a time
- tell the user which file to touch and what to change
- do not hand over giant finished code blocks unless the user explicitly asks
- keep the project simple and incremental

## Constraints
- Keep the project simple.
- Do not over-design early.
- Do not jump to mobile yet.
- Do not invent extra services or layers before the DB-backed API works.

## What Cannot Be Transferred Automatically
- The full chat transcript is not embedded here.
- Internal assistant-only skills or hidden system instructions cannot be copied to another AI session.

What this file provides instead is the practical equivalent:

- current state
- technical decisions
- user preferences
- next implementation target

## Recent Milestone
As of 2026-06-21, the backend has reached a working RSS sync milestone:

- CRUD for feeds is in place
- item browsing by feed is in place
- RSS sync can fetch, parse, de-duplicate, and persist items
- sync responses now report how many items were fetched and inserted
