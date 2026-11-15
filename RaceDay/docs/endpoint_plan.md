# RaceDay REST API Endpoint Plan

Base route: `/api`. All protected endpoints use the server-side `.RaceDay.Api.Session` cookie. Public event and category reads do not require login. The API stores passwords as BCrypt hashes. Public registration defaults to Participant; Organiser registration requires a private invitation code configured outside source control.

| HTTP method | Route | Purpose | Role required | Request body | Expected response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Create an account | Public; Organiser requires invite code | `{ firstName, lastName, email, password, phone, role, organizerInviteCode }` | 201 account created; 400 invalid input; 409 duplicate email; 403 invalid organiser code |
| POST | `/api/auth/login` | Verify credentials and start session | Public | `{ email, password }` | 200 user and session cookie; 401 invalid credentials |
| POST | `/api/auth/logout` | End current session | Logged in | None | 200 logged out |
| GET | `/api/auth/me` | Return current account | Logged in | None | 200 safe user profile; 401 not logged in |
| GET | `/api/users/profile` | View own profile | Logged in | None | 200 profile; 401/404 |
| PUT | `/api/users/profile` | Update own profile | Logged in | `{ firstName, lastName, phone }` | 200 updated; 400 invalid data |
| GET | `/api/events` | List and search events | Public | Query: `search`, `status`, `eventType` | 200 event list with categories and enrolment count |
| GET | `/api/events/{id}` | View one event | Public | None | 200 event; 404 not found |
| GET | `/api/events/mine` | List events owned by current Organiser | Organiser | None | 200 own events; 401/403 |
| POST | `/api/events` | Create event | Organiser | `{ eventName, eventType, eventDate, startTime, venue, description, capacity }` | 201 event created; 400/401/403 |
| PUT | `/api/events/{id}` | Edit own event | Owning Organiser | Same event fields | 200 updated; 403 not owner; 404 missing |
| DELETE | `/api/events/{id}` | Delete event without enrolments | Owning Organiser | None | 204 deleted; 409 if enrolments exist |
| GET | `/api/categories` | List distance/age categories | Public | None | 200 category list |
| GET | `/api/categories/{id}` | View one category | Public | None | 200 category; 404 missing |
| POST | `/api/categories` | Add a category | Organiser | `{ categoryName, description }` | 201 created; 409 duplicate |
| PUT | `/api/categories/{id}` | Edit category | Organiser | `{ categoryName, description }` | 200 updated; 404 missing |
| DELETE | `/api/categories/{id}` | Delete unused category | Organiser | None | 204 deleted; 409 if in use |
| POST | `/api/events/{eventId}/categories/{categoryId}` | Assign category to event | Owning Organiser | None | 200 assigned; 409 already assigned |
| DELETE | `/api/events/{eventId}/categories/{categoryId}` | Remove category from event | Owning Organiser | None | 204 removed; 409 if confirmed enrolments use it |
| GET | `/api/enrolments` | List own enrolments or enrolments for own events | Participant / Organiser | None | 200 role-filtered list; 401/403 |
| POST | `/api/enrolments` | Enrol and select category | Participant | `{ eventID, categoryID }` | 201 enrolment; 400 invalid category; 409 closed/full/duplicate event |
| DELETE | `/api/enrolments/{id}` | Cancel own enrolment or enrolment in own event | Participant / owning Organiser | None | 204 cancelled; 403 not owner |
| GET | `/api/results` | View personal results or results for own events | Participant / Organiser | None | 200 role-filtered result list |
| POST | `/api/results/{enrolmentId}` | Record finish time, position, total finishers and points | Owning Organiser | `{ finishTime, position, totalFinishers, points }` | 201 result; 400 invalid position/finisher count; 409 duplicate/cancelled enrolment |
| POST | `/api/files/profile-picture` | Upload own profile image | Logged in | Multipart form field `file` | 200 image URL; 400 invalid file; 401 |
| POST | `/api/files/events/{eventId}/banner` | Upload event banner | Owning Organiser | Multipart form field `file` | 200 image URL; 400 invalid file; 403 not owner |

## Status code rules

- `200 OK`: read or update succeeded.
- `201 Created`: a new record was created.
- `204 No Content`: a delete/cancel succeeded.
- `400 Bad Request`: invalid request values.
- `401 Unauthorized`: no valid session.
- `403 Forbidden`: signed-in user has the wrong role or does not own the record.
- `404 Not Found`: requested record does not exist.
- `409 Conflict`: duplicate enrolment, full/closed event, or record cannot be safely deleted.

## Role design decision

The public registration screen lets the user select Participant or Organiser. The API only accepts an Organiser registration when a server-side invitation code matches `Security:OrganizerInviteCode`. The invitation code must be set as an environment variable or user secret and must not be committed to GitHub. This prevents a visitor from granting themselves organiser privileges by editing the request body.
