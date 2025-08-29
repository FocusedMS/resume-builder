# API Documentation

This document describes the RESTful HTTP API exposed by the AI‑Powered Resume
Builder backend. All endpoints live under the `/api` root. Authentication is
handled via JSON Web Tokens (JWT) issued by the `/api/auth/login` endpoint.

## Authentication

### Register a new account

```
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "P@ssw0rd",
  "fullName": "Jane Doe"
}
```

Registers a new user and assigns them the `RegisteredUser` role. Returns
`200 OK` with a simple success message or `400 Bad Request` if the email
already exists or validation fails.

### Log in

```
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "P@ssw0rd"
}
```

Authenticates an existing user. On success returns `200 OK` and a JSON
payload containing a JWT token, the user’s role, email and name. On failure
returns `401 Unauthorized`.

Sample response:

```
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9…",
  "role": "RegisteredUser",
  "email": "user@example.com",
  "name": "Jane Doe"
}
```

Clients should include the JWT in the `Authorization` header for all
protected endpoints:

```
Authorization: Bearer <token>
```

## Resumes

All resume endpoints require authentication. Users with the `Admin` role can
access any resume; users with the `RegisteredUser` role may only access
resumes they own.

### List resumes

```
GET /api/resumes
GET /api/resumes?all=1  # admin only
```

Returns an array of resume objects. Without the `all` query parameter, the
endpoint lists resumes belonging to the authenticated user. Admins may
retrieve all resumes by specifying `?all=1`.

### Get a resume

```
GET /api/resumes/{id}
```

Retrieves a single resume by identifier. Returns `404` if not found or `403`
if the caller is not authorised.

### Create a resume

```
POST /api/resumes
Content-Type: application/json

{
  "title": "My Resume",
  "personalInfo": "…",
  "education": "…",
  "experience": "…",
  "skills": "Java, C#, SQL",
  "templateStyle": "classic"
}
```

Creates a new resume belonging to the authenticated user. Performs
server‑side validation on lengths and sanitises content. Returns the created
resume on success.

### Update a resume

```
PUT /api/resumes/{id}
Content-Type: application/json

{ …same fields as POST… }
```

Updates a resume. Only the owner or an admin can update. Validation and
sanitisation rules mirror those in creation.

### Delete a resume

```
DELETE /api/resumes/{id}
```

Deletes a resume. Returns `204 No Content` on success.

### Download a resume as PDF

```
GET /api/resumes/download/{id}
```

Generates a PDF for the specified resume using the stored `templateStyle` and
returns it with `application/pdf` content type and a `Content‑Disposition`
attachment filename of `resume-{id}.pdf`.

### Generate AI suggestions

```
POST /api/resumes/{id}/ai-suggestions
```

Generates structured suggestions based on the current content of the resume.
The suggestions are persisted as JSON in the `AiSuggestionsJson` column and
returned to the caller in the following shape:

```
{
  "suggestions": [
    {
      "section": "PersonalInfo",
      "priority": "high",
      "message": "Write a 2–3 sentence impact summary.",
      "applyTemplate": "Full‑stack developer with X years…"
    },
    …
  ]
}
```

## Admin endpoints

The following endpoints require the caller to have the `Admin` role.

### List users

```
GET /api/admin/users
```

Returns an array of user objects with their assigned roles. Each object
contains `id`, `email`, `fullName` and `roles` (an array of strings).

### System metrics

```
GET /api/admin/metrics
```

Returns counts of total users, total resumes and resumes created in the last
24 hours.

### Change a user’s role

```
POST /api/admin/users/{id}/roles
Content-Type: application/json

{
  "role": "Admin"
}
```

Removes all current roles from the user and assigns the specified role. If the
role does not exist it is created. Responds with `200 OK` when successful.

### Lock or unlock a user

```
POST /api/admin/users/{id}/lock
Content-Type: application/json

{
  "locked": true
}
```

Soft locks a user account by setting an infinite lockout end date. Pass
`false` to unlock. Responds with `200 OK`.