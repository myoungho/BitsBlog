# Changelog

## 2025-09-12

- Admin area (MVC) added with Bootstrap styling
  - Areas/Admin: Dashboard, Users, Posts, Comments
  - Admin link in navbar (JWT role=Admin), login redirect to /Admin
- WebApi: Users controller (Admin-only)
  - GET /api/users (paging, search, sort + X-Total-Count)
  - GET /api/users/{id}
  - PUT /api/users/{id}/role
  - DELETE /api/users/{id}?mode=anonymize|cascade
- Profile features (MVC + WebApi)
  - DisplayName edit, Password change (modal)
  - Stop reading profile from JWT; fetch via auth/me
  - After profile update, issue new JWT
- Pagination refactor
  - Shared models: PagedResult<T>, PaginationModel; PagingUtils
  - Shared partial: Views/Shared/_Pagination.cshtml (page size, first/last)
  - Applied to user Posts, Admin Users/Posts/Comments with search/sort
- DTO standardization
  - Added input DTOs for Posts/Comments/Users/Auth
  - Services accept DTOs instead of tuples/primitive parameters
  - Auth and other controllers bind DTOs
- Swagger/OpenAPI improvements
  - XML docs enabled (WebApi + Application) and included in Swagger
  - Summaries and Produces/Consumes/Response annotations for endpoints
- Repository redesign
  - New APIs: AsQueryable(bool), List/Count/Any, GetByKeyAsync, range ops
  - Transactions: Begin/Commit/Rollback + ExecuteInTransaction helpers
  - SaveChangesAsync standardized; removed SaveDbContextChangesAsync
  - Removed GetAllAsync; tests updated
- Apply transactions in services
  - Wrap inserts/updates/deletes using ExecuteInTransactionAsync
- Admin user delete modes
  - anonymize: mask author fields, keep content
  - cascade: delete user’s posts/comments
  - Admin UI select per-row
- CancellationToken propagation
  - Services accept ct; EF and repository calls pass ct
  - WebApi passes HttpContext.RequestAborted to services

