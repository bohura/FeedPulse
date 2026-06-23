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
[`AppDbContext.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Data/AppDbContext.cs) currently:

- exposes `DbSet<Feed> Feeds`
- exposes `DbSet<FeedItem> FeedItems`
- configures a unique index on `FeedItem (FeedId, ExternalId)`

### API Controllers
[`FeedsController.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Controllers/FeedsController.cs) currently implements CRUD for feeds plus sync:

- `GET /feeds`
- `GET /feeds/{id}`
- `POST /feeds`
- `PUT /feeds/{id}`
- `DELETE /feeds/{id}`
- `POST /feeds/{id}/sync`

Current details:

- `GET /feeds` now uses `AsNoTracking()`
- `GET /feeds` now orders by `CreatedAt` descending, then `Id` descending
- feed create/update still uses DTOs under `Contracts/Feeds`

[`FeedItemsController.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Controllers/FeedItemsController.cs) provides read-only item browsing by feed:

- `GET /feeds/{feedId}/items`
- `GET /feeds/{feedId}/items/{id}`

Current details:

- filters by `feedId`
- orders items by `PublishedAt ?? CreatedAt` descending
- uses `AsNoTracking()` for read queries
- no longer injects the sync service

### Sync Service
[`FeedSyncService.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Services/FeedSyncService.cs) and [`IFeedSyncService.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Services/IFeedSyncService.cs) are in place.

Current sync behavior:

- `POST /feeds/{id}/sync` is wired through `FeedsController`
- downloads the feed XML from the stored feed URL
- parses RSS `<rss>/<channel>/<item>` entries
- parses Atom `<feed>/<entry>` entries and persists them
- skips entries missing required `title` or `link`
- de-duplicates before insert using `FeedId + ExternalId`
- is now backed by a database-level unique index on `FeedId + ExternalId`
- stores new `FeedItem` rows in PostgreSQL
- converts `PublishedAt` to UTC before saving, to satisfy PostgreSQL `timestamp with time zone`
- returns a `FeedSyncResult` with `FeedId`, `FeedTitle`, `FetchedCount`, `AddedCount`, `Success`, and `ErrorMessage`

### Contracts
[`CreateFeedRequest.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Contracts/Feeds/CreateFeedRequest.cs)
and [`UpdateFeedRequest.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Contracts/Feeds/UpdateFeedRequest.cs)
exist and are used by the feeds controller.

### Migrations
EF Core migrations currently include:

- `InitialCreate`
- `AddFeedItems`
- `AddFeedItemUniqueIndex`

Current migration state:

- `FeedItems` table exists
- foreign key to `Feeds` exists
- unique index on `(FeedId, ExternalId)` has been generated and applied successfully

### Template / Stray File Note
There is also a root-level [`AppDbContext.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/AppDbContext.cs) at the repo root containing a template `Class1`.
It is not the real DbContext and should not be confused with [`src/FeedPulse.Api/Data/AppDbContext.cs`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/Data/AppDbContext.cs).

### Request File
[`FeedPulse.Api.http`](C:/Users/Bohura/source/repos/bohura/FeedPulse/src/FeedPulse.Api/FeedPulse.Api.http) has now been updated away from the default template.
It currently contains practical requests for:

- `GET /feeds`
- `POST /feeds`
- `GET /feeds/{id}`
- `POST /feeds/{id}/sync`
- `GET /feeds/{id}/items`

## What Has Already Been Verified
- The PostgreSQL container workflow works for this project.
- EF migrations can create and update the tables successfully.
- The database contains `Feeds` and `FeedItems`.
- `POST /feeds/{id}/sync` successfully downloads an RSS feed and inserts `FeedItem` rows into the database.
- Atom sync works against `https://github.com/dotnet/runtime/releases.atom`.
- A repeat sync against the same feed was verified to be idempotent.
- After the duplicate-prevention changes, repeat sync no longer adds duplicate rows.
- The `AddFeedItemUniqueIndex` migration applied successfully.
- The updated requests in `FeedPulse.Api.http` were reported working.

## Important Context From The Conversation
- The user is new to this exact workflow and needs concrete, step-by-step guidance.
- Abstract architecture explanations were not landing well.
- The user prefers direct instructions like:
  - which file to touch
  - what concept the file is for
  - what command to run next
- The user explicitly asked to be guided rather than handed a fully finished implementation.

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
As of 2026-06-23, the backend has reached a more stable feed-sync milestone:

- CRUD for feeds is in place
- item browsing by feed is in place
- RSS sync can fetch, parse, de-duplicate, and persist items
- Atom sync can fetch, parse, de-duplicate, and persist items
- duplicate prevention is now enforced both in code and in the database
- repeat sync on the same feed has been verified as idempotent
- the request file now contains practical API calls instead of the default template

## 2026-06-23 Progress Update
Progress completed in this session:

- added an Atom guard so entries without `title` or `link` are skipped instead of creating invalid `FeedItem` values
- added a unique index for `FeedItems (FeedId, ExternalId)` in `AppDbContext`
- generated and applied the `AddFeedItemUniqueIndex` migration
- verified repeat sync does not insert duplicates
- updated `FeedPulse.Api.http` with useful requests for feeds, sync, and items
- updated `GET /feeds` to use `AsNoTracking()` and newest-first ordering
- simplified `FeedItemsController` by removing the sync-service dependency and cleaning variable names

## Recommended Next Step
Keep the next step small.

Recommended follow-up:

1. make `GET /feeds/{id}` use `AsNoTracking()` as well, for consistency with the other read endpoints
2. optionally clean formatting / encoding in `FeedPulse.Api.http` comments if they appear garbled in some editors
3. after that, choose the next product feature instead of more internal cleanup

Practical feature candidates after the cleanup pass:

- improve feed list/detail responses
- add pagination for feed items
- decide how inactive feeds should behave during sync
- only later move sync into a background worker