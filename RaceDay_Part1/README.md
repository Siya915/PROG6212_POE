# RaceDay System — Part 1

## 1. System Description

RaceDay is a REST-based event management system for organising and participating in running events. The system stores users, events, categories, event enrolments and race results in a SQL Server relational database.

Part 1 was completed before API implementation and contains the system plan, ERD, endpoint plan, database script and CI/CD validation workflow.

## 2. User Roles

### Participant
A Participant can:
- Register and log in.
- View and update their profile.
- Browse events and categories.
- Enrol in an event.
- Cancel their own enrolment.
- View their results.

### Organizer
An Organizer can:
- Register and log in.
- View and update their profile.
- Create, update and delete events.
- Create, update and delete categories.
- Assign categories to events.
- View event enrolments.
- Record and view race results.

## 3. Database Design

The database contains six entities:

1. **Users** — stores account/profile information and the user's role.
2. **Events** — stores race event information and the organizer responsible for it.
3. **Categories** — stores event categories.
4. **EventCategories** — junction table resolving the many-to-many Event/Category relationship.
5. **Enrolments** — links Participants to Events.
6. **Results** — stores the result of an enrolment.

### Main relationships

- One User (Organizer) can create many Events.
- One Event can have many Categories, and one Category can belong to many Events.
- One User (Participant) can have many Enrolments.
- One Event can have many Enrolments.
- One Enrolment can have one Result.

See `docs/raceday_erd.png`.

## 4. API Planning

The complete endpoint specification is in `docs/endpoint_plan.md`.

Authentication, user profile, events, categories, event enrolments and results are covered as required. Role restrictions are included for every protected endpoint.

## 5. SQL Database

The complete SQL Server script is:

`docs/raceday_database.sql`

It creates `RaceDayDB`, creates all six tables with primary keys, foreign keys, NOT NULL, UNIQUE, DEFAULT and CHECK constraints, and inserts realistic sample data.

The seed data includes:
- 2 Organizers
- 2 Participants
- 3 Events
- Categories assigned to every event
- 4 sample enrolments
- 2 sample results

The script also includes verification SELECT statements.

## 6. CI/CD

GitHub Actions validates that the required `/docs` folder and planning files exist.

Workflow:

`.github/workflows/validate-docs.yml`

After pushing the repository to GitHub, the workflow should produce a green check when all required files are present.

**CI/CD screenshot:**  
`docs/ci-cd-screenshot.png` should be replaced with the actual screenshot of the successful GitHub Actions green build after the repository is pushed.

## 7. Video

Record an **unlisted YouTube video** explaining:
1. The RaceDay system and two roles.
2. The ERD entities and relationships.
3. Why the EventCategories junction table is needed.
4. The API endpoint plan and role restrictions.
5. The SQL database script.
6. A live execution of the SQL script in SSMS.

**YouTube video link:** Replace this line with the final unlisted YouTube URL after recording.

## 8. Suggested Meaningful Git Commits

The assignment requires at least 20 meaningful commits. Use your own GitHub account and make the commits while building the project. Suggested sequence:

1. `Initial RaceDay repository setup`
2. `Add docs folder`
3. `Add initial system description`
4. `Add user role definitions`
5. `Add initial database entities`
6. `Add ERD user and event relationships`
7. `Add category relationship to ERD`
8. `Add enrolment relationship to ERD`
9. `Add result relationship to ERD`
10. `Finalize RaceDay ERD`
11. `Add authentication endpoint plan`
12. `Add profile endpoint plan`
13. `Add event endpoint plan`
14. `Add category endpoint plan`
15. `Add enrolment endpoint plan`
16. `Add result endpoint plan`
17. `Finalize endpoint plan`
18. `Add SQL Users and Events tables`
19. `Add SQL Categories and EventCategories tables`
20. `Add SQL Enrolments and Results tables`
21. `Add realistic seed data`
22. `Add SQL verification queries`
23. `Add GitHub Actions documentation workflow`
24. `Update README with CI/CD and video instructions`

Do not create empty commits just to reach 20; make each commit represent a real change.

## 9. Folder Structure

```text
RaceDay/
├── .github/
│   └── workflows/
│       └── validate-docs.yml
├── docs/
│   ├── raceday_erd.png
│   ├── raceday_erd.pdf
│   ├── endpoint_plan.md
│   ├── endpoint_plan.pdf
│   └── raceday_database.sql
└── README.md
```
