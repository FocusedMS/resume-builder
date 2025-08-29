# Roles and Permissions

The AI‑Powered Resume Builder defines three roles to control access to
functionality throughout the application. Each role grants a different level
of capability within the system.

| Role           | Description                                                     | Permissions |
| -------------- | --------------------------------------------------------------- | ----------- |
| **Guest**      | Unauthenticated visitors browsing the application.              | • View the public home page<br>• Explore features overview |
| **RegisteredUser** | Users who have created an account and authenticated.           | • All Guest permissions<br>• Create, view, update and delete their own resumes<br>• Generate AI suggestions for their own resumes<br>• Download their resumes as PDF |
| **Admin**      | Administrators responsible for user and system management.      | • All RegisteredUser permissions<br>• View and manage any resume<br>• View all users and change their roles<br>• Lock or unlock user accounts<br>• View system metrics |

## Notes

* New accounts created via the sign‑up form are automatically assigned the
  `RegisteredUser` role.
* Admin privileges should be granted manually via the `POST /api/admin/users/{id}/roles`
  endpoint.
* The `Guest` role is implicit. If no JWT is provided, the user is treated
  as a guest and denied access to protected endpoints.