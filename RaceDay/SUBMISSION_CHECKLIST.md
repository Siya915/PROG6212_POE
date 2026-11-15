# RaceDay Parts 1–3 submission checklist

## Part 1 — Planning and database
- [ ] Review the six-entity ERD in `docs/raceday_erd.png`.
- [ ] Review the complete endpoint specification in `docs/endpoint_plan.md`.
- [ ] Run `docs/raceday_database.sql` in SSMS on a clean SQL Server database.
- [ ] Verify the sample records and relationships.

## Part 2 — API
- [ ] Build `RaceDay.Api` in Visual Studio.
- [ ] Confirm the API connects to the correct SQL Server instance.
- [ ] Test registration and login.
- [ ] Test Participant and Organiser role restrictions.
- [ ] Test event, category, enrolment and result endpoints in Swagger.
- [ ] Run all xUnit tests.

## Part 3 — MVC, storage and Docker
- [ ] Start API and MVC together.
- [ ] Test Organiser dashboard and event management.
- [ ] Test category assignment and enrolment management.
- [ ] Test result entry and published results.
- [ ] Test Participant event enrolment, profile and results.
- [ ] Test image upload using local fallback or your own Azure Blob Storage.
- [ ] Copy `.env.example` to `.env` and run `docker compose up --build`.

## GitHub and final submission
- [ ] Push the complete solution to your own GitHub repository.
- [ ] Make at least 20 meaningful commits using your own account.
- [ ] Confirm the GitHub Actions build and tests pass.
- [ ] Add a screenshot of your actual green Actions build to README.md.
- [ ] Record and add your actual unlisted YouTube walkthrough URL.
- [ ] Update the Word report with screenshots from your own running application.
- [ ] Submit the GitHub repository URL on ARC.
