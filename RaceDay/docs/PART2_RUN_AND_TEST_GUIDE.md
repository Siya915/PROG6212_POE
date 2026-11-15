# Combined Parts 1–3 run and test guide

Use the main `README.md` as the full setup guide. This file is a quick sequence for the practical demonstration.

1. Open `RaceDay.sln` in Visual Studio with the .NET 8 SDK installed.
2. Ensure SQL Server LocalDB or SQL Server Express is running.
3. Run `docs/raceday_database.sql` in SSMS, or let the API create the database with `Database:CreateOnStartup=true`.
4. Configure the API's connection string in `RaceDay.Api/appsettings.json`.
5. Configure Visual Studio to start `RaceDay.Api` and `RaceDay.Mvc` together.
6. Start the solution with F5. Check Swagger and the MVC website.
7. Test both roles using the development accounts listed in README.md.
8. Run the tests from Test Explorer or with `dotnet test RaceDay.sln`.
9. For Docker, copy `.env.example` to `.env`, update secrets and run `docker compose up --build`.
10. Before submission, run the GitHub Actions workflow on your own repository and capture a real successful run.

Do not submit until you have completed these checks on your own computer and fixed any errors reported by Visual Studio, SQL Server or Docker.
