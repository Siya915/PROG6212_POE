# RaceDay — Parts 2 and 3 Visual Studio Solution

RaceDay is a running-event management system for Organisers and Participants. This package contains only the RESTful ASP.NET Core Web API (Part 2) and the ASP.NET Core MVC website that consumes the API (Part 3). Part 1 planning deliverables are intentionally excluded.

> **Build and environment note:** This package targets **.NET 8** and is structured as a Visual Studio solution. The generation environment used to prepare this package did not have the .NET SDK, SQL Server or Docker installed, so a local `dotnet build`, SQL execution and container run could not be performed here. Run the verification steps below on your Visual Studio computer before submitting. Do not describe the project as tested until those checks pass on your machine.

## 1. Solution contents

```text
RaceDay/
├── RaceDay.sln
├── RaceDay.Api/                  # ASP.NET Core Web API + EF Core + SQL Server
│   ├── Controllers/              # Auth, events, categories, enrolments, results, files
│   ├── Data/                     # Entity Framework Core DbContext
│   ├── Dtos/                     # API request models and validation
│   ├── Models/                   # Six database entities
│   ├── Services/                 # Azure Blob Storage / local image storage
│   ├── wwwroot/uploads/          # Local-development image fallback
│   └── appsettings.json
├── RaceDay.Mvc/                  # ASP.NET Core MVC website (API client only)
│   ├── Controllers/              # Auth, public events, Organiser, Participant
│   ├── Models/                   # MVC view models and API response models
│   ├── Services/ApiClient.cs      # Sends requests to Part 2 API
│   ├── Views/                    # Razor pages for both roles
│   └── wwwroot/css/site.css
├── RaceDay.Tests/                # xUnit unit tests
├── docs/
│   ├── endpoint_plan.md          # Part 2 API endpoint specification
│   ├── endpoint_plan.pdf         # Part 2 API endpoint specification PDF
│   ├── PART2_RUN_AND_TEST_GUIDE.md
│   └── video_walkthrough_script.md
├── .github/workflows/dotnet.yml  # CI validation, build and test
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

## 2. Technology and prerequisites

- Visual Studio 2022 (17.8 or later) or Visual Studio 2026 with the **ASP.NET and web development** workload.
- .NET 8 SDK.
- SQL Server LocalDB, SQL Server Express or SQL Server Developer Edition.
- SQL Server Management Studio (SSMS), recommended for inspecting the Code First database.
- Docker Desktop (optional, for Part 3 containerisation).
- Internet access on the first build so NuGet can restore packages and the website can load Bootstrap from its CDN.

NuGet package versions are pinned in the project files. The API uses Entity Framework Core SQL Server 8.0.11, BCrypt.Net-Next 4.0.3, Azure.Storage.Blobs 12.22.2 and Swashbuckle 6.6.2. The test project uses xUnit 2.9.2 and Microsoft.NET.Test.Sdk 17.12.0.

## 3. Run the solution in Visual Studio

### Step 1 — Open the solution

1. Extract the ZIP to a normal folder, for example `C:\Projects\RaceDay`.
2. Open `RaceDay.sln` in Visual Studio.
3. Wait for NuGet package restore to finish.
4. Confirm the **.NET 8 SDK** is installed.

### Step 2 — Prepare SQL Server

The default API connection string in `RaceDay.Api/appsettings.json` uses LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

If you use LocalDB, check that `(localdb)\MSSQLLocalDB` is available. If you use SQL Server Express or another SQL Server instance, replace the connection string with your server name and authentication details.

**Option A — Let the API create the database:** keep `Database:CreateOnStartup` set to `true`. On a clean SQL Server database, Entity Framework Core `EnsureCreated()` creates the tables from the C# models and the API adds development sample data.

**Database setup:** the API creates the database from its Entity Framework Core Code First models when `Database:CreateOnStartup` is enabled. Development sample data is added by the API. The database tables are implementation dependencies for Parts 2 and 3, not a separate Part 1 submission deliverable.

### Step 3 — Configure organiser invitations

Public registration can create a Participant without an invitation. Organiser registration requires a private invitation code. Set it outside source control, for example with Visual Studio User Secrets from the `RaceDay.Api` project:

```bash
dotnet user-secrets init --project RaceDay.Api
dotnet user-secrets set "Security:OrganizerInviteCode" "UseYourOwnLongPrivateCode" --project RaceDay.Api
```

Alternatively set the environment variable `Security__OrganizerInviteCode`. Do not commit a real invitation code to GitHub. The sample Organiser accounts are already seeded for local coursework testing.

### Step 4 — Start both web projects

1. In Solution Explorer, right-click the solution and select **Configure Startup Projects**.
2. Select **Multiple startup projects**.
3. Set `RaceDay.Api` to **Start**.
4. Set `RaceDay.Mvc` to **Start**.
5. Leave `RaceDay.Tests` as a test project (do not start it as a web app).
6. Press **F5**.

Local URLs:

- MVC website: `https://localhost:7200` (or `http://localhost:5200`).
- API Swagger: `https://localhost:7041/swagger` (or `http://localhost:5041/swagger`).

The MVC application calls the API using `ApiClient`. It does not connect directly to SQL Server and does not duplicate database business logic. The API session cookie is forwarded server-to-server and the MVC application keeps its own session for navigation and role-aware views.

### Step 5 — Sample development accounts

| Role | Email | Password |
|---|---|---|
| Organiser | `amanda@raceday.co.za` | `Organizer123!` |
| Organiser | `brian@raceday.co.za` | `Organizer456!` |
| Participant | `siya@example.com` | `Participant123!` |
| Participant | `lerato@example.com` | `Participant456!` |

These are coursework demonstration accounts. Change or remove them before any public deployment. Passwords are hashed with BCrypt; plain-text passwords are not stored in the database.

## 4. Role functionality

### Organiser

- Dashboard summary of own events, confirmed enrolments and upcoming dates.
- Create, edit and delete own events (events with enrolments cannot be deleted).
- Upload an event banner image.
- Create and remove unused event categories; assign categories to own events.
- View enrolments for own events, including participant, category and status.
- Cancel enrolments for own events.
- Record finish time, finishing position, total finishers and points for confirmed enrolments. Results are entered after the event date and display positions such as “47th out of 312 finishers”.
- View published results for own events.

### Participant

- Browse and search public events with event-type and status filters.
- View event details and available categories.
- Enrol in an open event and select one of its assigned categories.
- View and cancel own enrolments.
- View personal race history and results, including event date, category, finish time and position out of the total finishers.
- Update personal details and upload a profile picture.

The API verifies both role and record ownership. A Participant cannot call Organiser-only endpoints, and one Organiser cannot modify another Organiser's event.

## 5. Part 2 API and authentication

The API uses ASP.NET Core Web API, Entity Framework Core Code First and SQL Server. Login creates a server-side ASP.NET Core session. The API session cookie is HTTP-only. BCrypt hashes passwords. API controllers check the signed-in user's role and, where relevant, ownership of the requested event or enrolment.

See `docs/endpoint_plan.md` for the complete HTTP method, route, description, role, request body and response specification.

### Main API routes

- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`
- `GET /api/users/profile`, `PUT /api/users/profile`
- `GET /api/events`, `GET /api/events/{id}`, `GET /api/events/mine`, event POST/PUT/DELETE
- `GET /api/categories`, category POST/PUT/DELETE
- `POST/DELETE /api/events/{eventId}/categories/{categoryId}`
- `GET/POST /api/enrolments`, `DELETE /api/enrolments/{id}`
- `GET /api/results`, `POST /api/results/{enrolmentId}`
- `POST /api/files/profile-picture`, `POST /api/files/events/{eventId}/banner`

## 6. Part 3 Azure Blob Storage

The API handles all image uploads. The MVC website does not connect to Azure directly. Local fallback image URLs are returned using the API public base URL; Docker sets this to `http://localhost:8081` so the browser can load files from the API container.

- If `AzureStorage:ConnectionString` is configured, the API uploads the image to the configured Azure Blob Storage container (default name `raceday-images`).
- If the Azure connection string is empty, the API uses a local development fallback in `RaceDay.Api/wwwroot/uploads`.
- Accepted types: JPEG, PNG and WEBP.
- Maximum upload size: 5 MB.

To configure Azure, set the `AzureStorage:ConnectionString` environment variable (configuration key `AzureStorage__ConnectionString`) and optionally `AzureStorage__ContainerName`. The service creates the container with Blob public-read access so the returned image URL can be displayed in this coursework application. This requires the storage account to allow public blob access. For a real production deployment, use private containers and short-lived read SAS URLs or a secured image delivery layer instead. Never commit an Azure connection string to source control.

## 7. Run with Docker

Docker Desktop must be running. From the repository root:

```bash
cp .env.example .env
```

Edit `.env` and replace `SA_PASSWORD` with a strong SQL Server password. Change `ORGANIZER_INVITE_CODE`. Add an Azure connection string only if you have an Azure Storage account; leaving it blank enables local image storage.

Start the solution:

```bash
docker compose up --build
```

Open:

- MVC website: `http://localhost:8080`
- API Swagger: `http://localhost:8081/swagger`

Stop containers:

```bash
docker compose down
```

Stop containers and delete the SQL data volume (this permanently deletes the Docker database):

```bash
docker compose down -v
```

The API retries the SQL Server connection during startup because the SQL Server container can take time to initialise. The MVC container communicates with the API using the internal Docker address `http://api:8080/`.

## 8. Build and test from a terminal

Run these commands from the folder containing `RaceDay.sln`:

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test RaceDay.Tests/RaceDay.Tests.csproj --configuration Release
dotnet run --project RaceDay.Api/RaceDay.Api.csproj
dotnet run --project RaceDay.Mvc/RaceDay.Mvc.csproj
```

To run the two web applications together in Visual Studio, use the multiple-startup-project steps in section 3.

The API currently uses `EnsureCreated()` for a simple clean-database Code First setup. If you later add EF Core migrations, install the tool with `dotnet tool install --global dotnet-ef --version 8.0.11`, create a migration with `dotnet ef migrations add InitialCreate --project RaceDay.Api`, change the startup database call from `EnsureCreated()` to `Migrate()`, and then run `dotnet ef database update --project RaceDay.Api`. Do not mix `EnsureCreated()` with migrations on the same database.

## 9. GitHub Actions and submission requirements

The workflow `.github/workflows/dotnet.yml` checks the Part 2 and Part 3 projects and documentation, restores packages, builds the solution and runs the xUnit tests on pushes and pull requests.

To complete the assignment submission:

1. Create your own GitHub repository.
2. Push this solution to your repository.
3. Make at least **20 meaningful commits** using your own GitHub account. Do not manufacture commit history; commit real stages such as ERD, endpoint plan, SQL schema, API models, authentication, controllers, MVC views, uploads, Docker and tests.
4. Open the **Actions** tab and wait for a real green build. Capture your own screenshot and add it to this README or `docs/`.
5. Record an unlisted YouTube walkthrough. Explain the ERD, endpoint plan, run the SQL script in SSMS, show the API in Swagger, log in using both roles, demonstrate the MVC features and run tests. Add the real unlisted YouTube URL below.
6. Submit your GitHub repository URL on ARC.

**GitHub Actions screenshot:** Add your actual successful build screenshot here after running the workflow.

**Unlisted YouTube walkthrough:** Add your actual video URL here after recording it.

## 11. Important coursework and production notes

- This is a coursework starter solution. Verify it on your computer before submitting.
- The current API uses in-memory session storage. For multiple production instances, replace it with a shared distributed cache such as SQL Server or Redis.
- Add production-grade CSRF protection for direct API browser clients, rate limiting, monitoring, logging, secure secrets, database backups, privacy controls and security review before public deployment.
- The public Organiser registration route requires a server-side invitation code. Do not expose that code to Participants.
- The generated project does not include real Azure credentials, a real GitHub repository, a successful GitHub Actions screenshot or a YouTube recording. These must be supplied by the student.
- Use the detailed Word report in `docs/RaceDay_Parts_1_2_3_Final_Project_Report.docx` as the combined project explanation and update it with your own screenshots and implementation evidence.
