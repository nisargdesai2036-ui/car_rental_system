# DriveEase Project Documentation

This directory contains technical documentation for **DriveEase**, a self-drive vehicle rental and fleet management web application built with **ASP.NET Core 10.0 (MVC & REST API)**, Entity Framework Core 10, ASP.NET Core Identity, and SQLite.

## Documentation Catalog

| Document | Description |
|---|---|
| [`PROJECT_OVERVIEW.md`](PROJECT_OVERVIEW.md) | **Primary Architecture Reference**: Tech stack (.NET 10), high-level architecture, EF Core entity schema, business logic algorithms, security/RBAC, UI design system, and startup instructions. |
| [`AUTHENTICATION_AND_AUTHORIZATION_TEST_RESULTS.md`](AUTHENTICATION_AND_AUTHORIZATION_TEST_RESULTS.md) | **Security Verification Report**: 30 automated test assertions validating AuthN login/logout flows, identity cookies, and RBAC boundaries for Admin, Owner, and Customer roles. |
| [`controller_endpoints_authorization_rules.txt`](controller_endpoints_authorization_rules.txt) | **API & Controller Authorization Matrix**: Exhaustive list of REST API endpoints across all 8 API controllers, mapping HTTP methods, authorization attributes, allowed roles, and business rules. |
| `DriveEase_Data_Models.pdf` | Detailed specification of entity models, data types, and primary/foreign key relationships. |
| `DriveEase_Controllers.pdf` | Architectural breakdown of controller routing and request handling. |
| `DriveEase_Services.pdf` | Business logic service layer methods, availability overlap algorithms, and pricing rules. |
| `DriveEase_ViewModels.pdf` | Data Transfer Objects (DTOs) and ViewModels with validation rules. |
| `DriveEase_Views.pdf` | Razor presentation views, layout structure, and component styling. |
| `DriveEase_Data.pdf` | Database context (`ApplicationDbContext`), migrations, and seeding strategies. |

## Suggested Reading Order

1. [`PROJECT_OVERVIEW.md`](PROJECT_OVERVIEW.md) — Start here for the complete overview of the current codebase and execution procedures.
2. [`controller_endpoints_authorization_rules.txt`](controller_endpoints_authorization_rules.txt) — Review API endpoints and permission boundaries.
3. [`AUTHENTICATION_AND_AUTHORIZATION_TEST_RESULTS.md`](AUTHENTICATION_AND_AUTHORIZATION_TEST_RESULTS.md) — Review security test coverage and pass rates.
4. Additional PDF specification documents for layer-by-layer details.
