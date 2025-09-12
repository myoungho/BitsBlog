# Admin Guide

## Overview

- Admin role is determined by JWT claim `role=Admin`.
- MVC (`BitsBlog.Web`) redirects Admin users to `/Admin` after login/registration.
- Admin area provides: Users, Posts, Comments management (read/delete, role change).

## WebApi Endpoints

- `GET /api/users?skip=0&take=100` — list users (Admin only)
- `GET /api/users/{id}` — get user by id (Admin only)
- `PUT /api/users/{id}/role` — body `{ "role": "Admin" | "User" }` (Admin only)
- `DELETE /api/users/{id}` — delete user (Admin only)
- Existing:
  - `GET /api/posts` | `DELETE /api/posts/{id}` (Admin allowed)
  - `GET /api/posts/{postId}/comments` | `DELETE /api/posts/{postId}/comments/{commentId}` (Admin allowed)

## Seeding Admin

Set configuration for seeding an Admin on WebApi startup:

- `AdminSeed:Email`
- `AdminSeed:Password`
- `AdminSeed:DisplayName`

## MVC Admin Area

- Users: `/Admin/Users`
- Posts: `/Admin/Posts`
- Comments: `/Admin/Comments` (query by `postId`)

