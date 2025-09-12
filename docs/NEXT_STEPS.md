# Next Steps

## Short term
- Add ct to MVC HttpClient calls where appropriate (timeouts/cancellation)
- Admin UI: confirm modal for delete mode with explanatory text
- Fix view warning in `Views/Posts/Details.cshtml` (nullable dereference)
- Add DTO validation (FluentValidation or DataAnnotations) and consistent 400 responses
- Expand Swagger docs: error schemas (ProblemDetails) for 400/401/403/404
- Add audit logging for admin actions (role change, delete mode)

## Medium term
- Client-react alignment (if used): call WebApi with same DTOs + pagination/search/sort
- Enhance search/sort (Posts: by title/author; Users: by role)
- Add transaction-aware domain operations (batch operations UI)
- Add Admin dashboard metrics (counts, recent activity)

## Operational
- Seed configuration examples in README (AdminSeed, Jwt, ConnectionStrings)
- CI: add build/test workflows and code coverage
- Security: tighten CORS and JWT settings per environment

