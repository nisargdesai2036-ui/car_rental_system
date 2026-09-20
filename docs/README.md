# DriveEase Documentation

This folder holds the project documentation for **DriveEase**, a self-drive vehicle rental web app built with ASP.NET Core 9.0 MVC, Entity Framework Core, and PostgreSQL (Supabase).

## Contents

| Document | What it covers |
|---|---|
| `PROJECT_OVERVIEW.md` | High-level overview: tech stack, architecture, layers, current state, and how to run. Start here. |
| `DriveEase_Data_Models.pdf` | All 8 entity models — attributes, data types, sample data, primary/foreign keys, enums, and relationships. |
| `DriveEase_Controllers.pdf` | The Controllers layer — routing, `HomeController`, and the controllers a full build would add. |
| `DriveEase_Data.pdf` | The Data layer — `ApplicationDbContext`, `DbSeeder`, `IDataStore`, and `InMemoryDataStore`. |
| `DriveEase_Views.pdf` | The Views layer — Razor config views, the shared layout, and content views. |
| `DriveEase_ViewModels.pdf` | The ViewModels layer — all 10 view models with validation rules and purpose. |
| `DriveEase_Services.pdf` | The Services layer — the 6 business services, their methods, and business rules. |

## Suggested reading order

1. `PROJECT_OVERVIEW.md` — the big picture
2. `DriveEase_Data_Models.pdf` — the domain (what the data looks like)
3. `DriveEase_Services.pdf` — the business logic that acts on the data
4. `DriveEase_ViewModels.pdf` → `DriveEase_Controllers.pdf` → `DriveEase_Views.pdf` — the presentation flow
5. `DriveEase_Data.pdf` — how data is stored and seeded

> Note: `WAD_Project_Requirements.pdf` remains in the project root as it is the original assignment brief, not generated documentation.
