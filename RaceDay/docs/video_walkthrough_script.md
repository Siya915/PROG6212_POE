# RaceDay Parts 1–3 Unlisted Video Walkthrough Script

Use your own screen recording and voice. The script below is a guide; explain the work in your own words and show your actual application running.

## 1. Introduction

“Hello, this video demonstrates my RaceDay running-event management system. I combined Part 1 system planning and database design, Part 2 RESTful API development, and Part 3 MVC frontend and deployment.”

## 2. Part 1 — ERD and database

1. Open `docs/raceday_erd.png`.
2. Explain the six entities: Users, Events, Categories, EventCategories, Enrolments and Results.
3. Point out primary keys and foreign keys.
4. Explain that EventCategories resolves the many-to-many relationship between Events and Categories.
5. Explain that Enrolments stores the selected category for a Participant in a particular Event.
6. Open `docs/raceday_database.sql` in SSMS and run it against a clean SQL Server instance.
7. Show the tables, constraints and sample rows.

## 3. Part 1 — API endpoint plan

Open `docs/endpoint_plan.md`. Explain the HTTP methods, routes, request bodies, expected responses and role requirements. Point out that public registration does not allow a user to promote themselves to Organiser; Organiser registration requires a server-side invitation code.

## 4. Part 2 — API in Visual Studio

1. Open `RaceDay.sln` and show the API project.
2. Explain the models and `RaceDayContext`.
3. Run the API and open Swagger.
4. Register or log in with a Participant and show the session-based login.
5. Show that a Participant can browse events and enrol but cannot create events.
6. Log in as an Organiser and show event, category, enrolment and result endpoints.
7. Explain BCrypt password hashing and role/ownership checks.

## 5. Part 3 — MVC website

1. Run both RaceDay.Api and RaceDay.Mvc as multiple startup projects.
2. Show the public events page and event details.
3. Log in as an Organiser. Show the dashboard, event creation/editing, banner upload, category management, enrolments and result entry.
4. Log out and log in as a Participant. Show enrolment with category selection, My Events, My Results and profile editing/profile picture upload.
5. Explain that the MVC application calls the API using `ApiClient` and does not connect directly to SQL Server.

## 6. Azure Blob Storage and Docker

1. Explain that image uploads go through the API. If an Azure connection string is configured, the API uses Azure Blob Storage; otherwise the local development fallback is used.
2. Show the Dockerfile and docker-compose.yml.
3. Copy `.env.example` to `.env`, set a strong SQL Server password and run `docker compose up --build`.
4. Open the MVC website at `http://localhost:8080` and Swagger at `http://localhost:8081/swagger`.

## 7. Tests and GitHub Actions

1. Run `dotnet test RaceDay.Tests/RaceDay.Tests.csproj` and show the real result.
2. Push the repository to your GitHub account.
3. Open Actions and show the actual green build after it completes.
4. Explain your meaningful commit history.

## 8. Conclusion

“RaceDay now has a planned relational database, a role-aware REST API, an MVC website that consumes the API, image upload support and a Docker deployment setup. Thank you for watching.”

Remember to add your real unlisted YouTube URL to the README. Do not claim a feature worked in the recording unless you have demonstrated it.
