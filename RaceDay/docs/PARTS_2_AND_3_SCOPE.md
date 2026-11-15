# RaceDay submission scope: Parts 2 and 3 only

This package intentionally excludes Part 1 deliverables such as the ERD, standalone SQL planning script, and Part 1 planning report.

## Part 2 — RESTful API
- ASP.NET Core Web API targeting .NET 8.
- Entity Framework Core Code First with SQL Server.
- Organiser and Participant registration/login, BCrypt password hashing, server-side session authentication, role checks, and ownership checks.
- API controllers for profiles, events, categories, event-category assignments, enrolments, results, and image uploads.
- xUnit test project.
- Endpoint specification: `endpoint_plan.md` and `endpoint_plan.pdf`.

## Part 3 — MVC frontend
- ASP.NET Core MVC website consuming the API through `RaceDay.Mvc/Services/ApiClient.cs`.
- No direct database access from MVC.
- Separate role-aware Organiser and Participant navigation and dashboards.
- Event/category/enrolment/result/profile workflows and image upload through the API.
- Docker and Docker Compose configuration.
- GitHub Actions build and test workflow.

## Important verification note
The package was assembled and its ZIP integrity and file list were checked. The preparation environment did not provide the .NET SDK, SQL Server, or Docker, so a successful build, test run, database startup, or container launch is not claimed. Follow `README.md` and `PART2_RUN_AND_TEST_GUIDE.md` on a machine with the required tools. Configure Azure Storage using your own credentials; no real credentials are included.
