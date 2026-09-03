# RaceDay REST API Endpoint Plan

**System:** RaceDay Event Management System  
**Version:** 1.0  
**Database:** RaceDayDB  
**Base URL:** `/api`

## Roles

- **Participant:** Can register/login, manage their profile, browse events and categories, enrol in events, cancel their own enrolment, and view their own/event results.
- **Organizer:** Can register/login, manage their profile, create/update/delete events and categories, view event enrolments, manage event categories, and record results.

> Authentication is assumed to use a bearer token after login. `Public` means no token is required.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Creates a new RaceDay account. | Public | `{ firstName, lastName, email, password, phone, role }` | **201 Created** user created; **400** validation error; **409** email exists |
| POST | `/api/auth/login` | Authenticates a user and returns an access token. | Public | `{ email, password }` | **200 OK** token + user; **401** invalid credentials |
| GET | `/api/users/me` | Returns the logged-in user's profile. | Any logged-in user | None | **200 OK** profile; **401** unauthenticated |
| PUT | `/api/users/me` | Updates the logged-in user's profile. | Any logged-in user | `{ firstName, lastName, phone }` | **200 OK** updated profile; **400** invalid data; **401** unauthenticated |
| GET | `/api/events` | Lists available RaceDay events, optionally filtered by date/category. | Public | None | **200 OK** event list |
| POST | `/api/events` | Creates a new event. | Organizer | `{ eventName, eventDate, startTime, venue, description, capacity, categoryIds }` | **201 Created** event; **400** validation error; **403** forbidden |
| GET | `/api/events/{eventId}` | Gets details for one event, including categories. | Public | None | **200 OK** event; **404** event not found |
| PUT | `/api/events/{eventId}` | Updates an event owned by the organizer. | Organizer | `{ eventName, eventDate, startTime, venue, description, capacity, status, categoryIds }` | **200 OK** updated event; **403** forbidden; **404** not found |
| DELETE | `/api/events/{eventId}` | Removes an event. | Organizer | None | **204 No Content**; **403** forbidden; **404** not found |
| GET | `/api/categories` | Lists all event categories. | Public | None | **200 OK** category list |
| POST | `/api/categories` | Creates a category. | Organizer | `{ categoryName, description }` | **201 Created**; **400** validation error; **409** duplicate |
| PUT | `/api/categories/{categoryId}` | Updates a category. | Organizer | `{ categoryName, description }` | **200 OK**; **404** not found; **409** duplicate |
| DELETE | `/api/categories/{categoryId}` | Deletes a category not in use. | Organizer | None | **204 No Content**; **404** not found; **409** category in use |
| POST | `/api/events/{eventId}/categories` | Adds a category to an event. | Organizer | `{ categoryId }` | **201 Created** link; **404** event/category not found; **409** already linked |
| DELETE | `/api/events/{eventId}/categories/{categoryId}` | Removes a category from an event. | Organizer | None | **204 No Content**; **404** link not found |
| POST | `/api/events/{eventId}/enrolments` | Enrols the logged-in participant in an event. | Participant | None or `{}` | **201 Created** enrolment; **404** event not found; **409** already enrolled/full |
| GET | `/api/enrolments/me` | Lists the logged-in participant's enrolments. | Participant | None | **200 OK** enrolment list; **401** unauthenticated |
| DELETE | `/api/events/{eventId}/enrolments/me` | Cancels the participant's own enrolment. | Participant | None | **204 No Content**; **404** enrolment not found |
| GET | `/api/events/{eventId}/enrolments` | Views all participants enrolled in an event. | Organizer | None | **200 OK** enrolment list; **403** forbidden; **404** event not found |
| POST | `/api/events/{eventId}/results` | Records a result for an enrolled participant. | Organizer | `{ enrolmentId, finishTime, position, points }` | **201 Created** result; **400** invalid data; **404** enrolment not found; **409** result exists |
| GET | `/api/events/{eventId}/results` | Returns results for an event. | Participant / Organizer | None | **200 OK** results; **404** event not found |
| GET | `/api/results/me` | Returns results belonging to the logged-in participant. | Participant | None | **200 OK** result list; **401** unauthenticated |

## REST and Security Decisions

1. Routes use nouns such as `events`, `categories`, `enrolments`, and `results`.
2. `GET` reads data, `POST` creates data, `PUT` updates data, and `DELETE` removes data.
3. Participant-only actions are protected by the `Participant` role.
4. Event/category/result management is protected by the `Organizer` role.
5. The API should validate all IDs and request fields and return appropriate HTTP status codes.
6. Passwords must be hashed by the application before storage; the database seed data uses SHA-256 hashes only to provide safe demonstration records.
