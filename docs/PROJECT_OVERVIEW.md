# DriveEase — Project Overview

DriveEase is a **self-drive vehicle rental** web application (cars and bikes) built with **ASP.NET Core 9.0 MVC**. Customers can browse and search vehicles, get recommendations, book for hourly or daily rentals, pay, and leave reviews. Administrators manage the fleet and view dashboard statistics.

> This document explains what exists in the codebase today, how the layers fit together, and where the project currently stands. Nothing in the code was modified to produce it.

---

## 1. Tech Stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 9.0 MVC (`net9.0`) |
| Language | C# with nullable reference types + implicit usings enabled |
| Root namespace | `wad_project` |
| ORM | Entity Framework Core 9.0.2 |
| Database | PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4 (hosted on **Supabase**) |
| Config / secrets | `dotenv.net` 4.2.0 (loads a `.env` file) |
| Front-end | Razor views, Bootstrap, jQuery, jQuery Validation |

Project file: `wad-project.csproj`. Note the assembly/root namespace is `wad_project` even though the folder is `DriveEase`.

---

## 2. High-Level Architecture

The code is organized in clean, layered folders:

```
Program.cs            → app startup, DI registration, request pipeline, DB seeding
Controllers/          → MVC controllers (currently only HomeController)
Views/                → Razor views (currently only Home + Shared layout)
Models/               → EF Core entities + enums (the domain)
ViewModels/           → request/response shapes for the UI + validation attributes
Services/             → business logic (interface + implementation per feature)
Data/                 → data access: EF DbContext, seeder, and an in-memory store
Migrations/           → EF Core migration for the initial Supabase schema
wwwroot/              → static assets (css, js, bootstrap, jquery)
```

The intended flow of a request is the classic MVC + service pattern:

```
Browser → Controller → Service (business rules) → Data store → Model → ViewModel → View → Browser
```

### Two data layers coexist

An important detail: the project currently has **two parallel data mechanisms**, reflecting a phased build.

1. **`IDataStore` / `InMemoryDataStore`** — a thread-safe, in-memory collection of lists (Users, Vehicles, Bookings, etc.) seeded on construction. Registered as a **singleton**. All the business services (`UserService`, `VehicleService`, `BookingService`, `PaymentService`, `ReviewService`, `AdminService`) are written against this in-memory store. Comments in the code call this "Phase 1."
2. **`ApplicationDbContext` (EF Core + Supabase Postgres)** — the real database layer. Registered as a scoped `DbContext`, with an initial migration and a `DbSeeder` that populates Supabase on startup if empty. Comments describe this as the "Phase 2" target that will "seamlessly swap" the in-memory store.

Right now both are wired up in `Program.cs`: the services use the in-memory store, while EF Core is initialized and seeded separately. They are **not yet unified** — the services do not read/write through EF Core.

---

## 3. Startup & Configuration (`Program.cs`)

Startup does the following, in order:

1. `DotEnv.Load()` reads a `.env` file into environment variables.
2. Builds the connection string:
   - First tries `ConnectionStrings:SupabaseConnection` from configuration.
   - If empty, assembles one from `SUPABASE_HOST`, `SUPABASE_PORT`, `SUPABASE_DATABASE`, `SUPABASE_USER`, `SUPABASE_PASSWORD`, `SUPABASE_SSL_MODE`, `SUPABASE_TRUST_SERVER_CERT`.
3. Registers `ApplicationDbContext` with Npgsql using that connection string.
4. Registers `InMemoryDataStore` as a **singleton** (`IDataStore`).
5. Registers all business services as **scoped**: `IUserService`, `IVehicleService`, `IBookingService`, `IPaymentService`, `IReviewService`, `IAdminService`.
6. Standard pipeline: exception handler + HSTS (non-dev), HTTPS redirect, routing, authorization, static assets.
7. Default route: `{controller=Home}/{action=Index}/{id?}`.
8. On startup, creates a scope and calls `DbSeeder.SeedAsync(dbContext)` to seed Supabase if it's empty.

Configuration files:
- `appsettings.json` — has an empty `SupabaseConnection` placeholder (the real value comes from `.env`).
- `.env.example` — template showing the Supabase variables to set. Copy to `.env` and fill in real credentials.

---

## 4. Domain Model (`Models/`)

The core entities and their relationships:

| Entity | Purpose | Key relationships |
|---|---|---|
| `User` | Customer or Administrator account | 1-to-many `Bookings`, `Reviews`; unique `Email` |
| `Vehicle` | A rentable car or bike | 1-to-many `Bookings`, `Reviews`; unique `LicensePlate`; holds `HourlyRate`/`DailyRate`, `Status`, cached `AverageRating`/`ReviewCount` |
| `Booking` | A rental reservation | belongs to `User` + `Vehicle`; optional `PromoCode`; 1-to-1 `Payment` and `Review`; unique `BookingReference` |
| `Payment` | Payment for a booking | 1-to-1 with `Booking` (cascade delete) |
| `Review` | Rating + comment for a completed booking | 1-to-1 with `Booking`; links `User` + `Vehicle` |
| `PromoCode` | Discount code (percentage or flat) | referenced by bookings; unique `Code` |
| `PricingRule` | Dynamic pricing (e.g. weekend surge multiplier) | optional `VehicleType` / `DayOfWeek` filters |
| `Notification` | SMS-style notification record | belongs to `User` |

### Enums (`Models/Enums.cs`)
`UserRole` (Customer, Administrator), `RentalType` (Hourly, Daily), `VehicleType` (Bike, Hatchback, Sedan, SUV, Luxury), `FuelType` (Petrol, Diesel, Electric), `TransmissionType` (Manual, Automatic), `VehicleStatus` (Available, Booked, UnderMaintenance), `BookingStatus` (Pending, Confirmed, Completed, Cancelled), `PaymentStatus` (Pending, Successful, Failed), `PaymentMethod` (Card, UPI, NetBanking, Wallet), `DiscountType` (Percentage, FlatAmount), `NotificationType` (BookingConfirmation, BookingCancelled, PaymentAlert).

Money fields use `decimal(18,2)`; ratings use `decimal(3,2)`; multipliers `decimal(5,2)`. Validation uses DataAnnotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Phone]`).

### EF relationships (`Data/ApplicationDbContext.cs`)
- Unique indexes on `User.Email`, `Vehicle.LicensePlate`, `Booking.BookingReference`, `PromoCode.Code`.
- `Booking → User` and `Booking → Vehicle`: `Restrict` on delete (can't delete a user/vehicle with bookings).
- `Booking → PromoCode`: `SetNull` on delete.
- `Payment` and `Review`: 1-to-1 with `Booking`, `Cascade` delete.

---

## 5. Business Logic (`Services/`)

Each service has an interface (`I*Service.cs`) and implementation, and operates on the singleton `IDataStore` using `lock` blocks for thread safety. Methods are async-signatured but run synchronously over in-memory lists (`Task.FromResult`).

### `UserService`
- `RegisterAsync` — rejects duplicate email or mobile; creates a Customer. **Password is stored in plain text** (see Notes).
- `LoginAsync` — finds user by email *or* mobile, checks `IsActive`, compares password.
- `GetUserByIdAsync`, `GetAllUsersAsync`, `ToggleUserStatusAsync` (activate/deactivate).

### `VehicleService`
- `GetAllVehiclesAsync` — lists vehicles that are not under maintenance.
- `SearchAndFilterAsync` — filters by location, type, price range, fuel, transmission; sorts by price/rating.
- `GetVehicleByIdAsync`.
- `GetRecommendationsAsync` — a **rule-based scoring engine** (out of 100) that ranks available vehicles by seating fit (30 pts), budget fit (30 pts), preferred type (25 pts), and rating (up to 15 pts), returning a human-readable reason string.
- `AddVehicleAsync` (rejects duplicate license plate), `UpdateStatusAsync`.

### `BookingService`
- `IsVehicleAvailableAsync` — overlap check using the interval rule `pickup < existing.Return && return > existing.Pickup`, ignoring cancelled bookings and maintenance vehicles.
- `CalculatePriceAsync` — validates dates, computes duration (ceiling of hours/days), applies a **weekend surge** `PricingRule` multiplier, then applies a promo code (validates active/date window/min amount; caps discount at base price). Returns a `PriceBreakdownViewModel`.
- `CreateBookingAsync` — availability + price, then creates a booking in **Pending** status with a generated `BookingReference` (`DE-yyyyMM-####`).
- `GetBookingByIdAsync`, `GetUserBookingsAsync`, `CancelBookingAsync` (owner-only, can't cancel completed/cancelled).

### `PaymentService`
- `ProcessPaymentAsync` — requires the booking to be Pending and the amount to equal `TotalAmount` (full payment only). On success, creates a `Payment`, links it, and flips the booking to **Confirmed**.
- `GetPaymentByBookingIdAsync`.

### `ReviewService`
- `AddReviewAsync` — enforces: reviewer owns the booking, booking is Completed, no duplicate review per booking. Then recomputes the vehicle's `AverageRating` and `ReviewCount`.
- `GetVehicleReviewsAsync`.

### `AdminService`
- `GetDashboardStatsAsync` — aggregates totals (customers, vehicles by status, bookings by status, total revenue from successful payments) plus the 5 most recent bookings into `AdminDashboardViewModel`.

---

## 6. ViewModels (`ViewModels/`)

These decouple the UI from the entities and carry validation:
- **Auth**: `RegisterViewModel` (with `Compare` password confirmation), `LoginViewModel`.
- **Vehicle**: `VehicleSearchFilterViewModel`, `RecommendationRequestViewModel`, `RecommendedVehicleViewModel`.
- **Booking**: `BookingRequestViewModel`, `PriceBreakdownViewModel`.
- **Payment**: `MakePaymentViewModel`.
- **Review**: `AddReviewViewModel`.
- **Admin**: `AdminDashboardViewModel`.

---

## 7. Data Seeding

Two seeders create the same realistic demo data (users, vehicles, promos, a pricing rule, historical + future bookings, payments, a review, a notification):
- `Data/DbSeeder.cs` — seeds the **Supabase Postgres** database via EF Core on startup (skips if users already exist).
- `Data/InMemoryDataStore.SeedData()` — seeds the **in-memory** store used by the services.

Seeded accounts (demo credentials):
- Admin — `admin@driveease.com` / `Admin@123`
- Customer — `rahul@gmail.com` / `User@123`
- Customer — `priya@gmail.com` / `User@123`

Fleet includes bikes (Royal Enfield Classic 350, Honda Activa), a hatchback (Swift), a sedan (Honda City), SUVs (Creta, Thar, Nexon EV — under maintenance), and a luxury car (BMW 3 Series). Promo codes: `WELCOME20` (20% off), `FLAT500` (₹500 off), `EXPIRED50` (expired, for testing).

---

## 8. Web Layer & Current State

- **Controllers**: only `HomeController` exists (`Index`, `Privacy`, `Error`) — the default template.
- **Views**: only `Home/Index`, `Home/Privacy`, and the shared `_Layout` / error views exist. The layout still shows the default `wad_project` branding and Home/Privacy nav — no DriveEase-specific UI yet.
- **Migrations**: `20260916163448_InitialSupabaseSetup` creates the full schema in Postgres.

### What this means
The **domain, business logic, data access, and seeding are fully built and cohesive**, but the **presentation layer is not connected yet**. There are no `Account`, `Vehicle`, `Booking`, `Payment`, `Review`, or `Admin` controllers/views wiring the services to the browser. So today the app compiles and runs, seeds both stores, and serves the default template pages — but the rental features are only reachable through the service classes, not the UI.

### Natural next steps (not done — described only)
1. Add controllers (`AccountController`, `VehiclesController`, `BookingsController`, `PaymentController`, `ReviewsController`, `AdminController`) that call the existing services.
2. Add Razor views for browse/search, vehicle details, booking flow, payment, reviews, and the admin dashboard.
3. Add authentication/session handling (login state, role checks for admin).
4. Unify the data layer: migrate services from `IDataStore` to `ApplicationDbContext` so reads/writes hit Supabase, then retire the in-memory store.

---

## 9. Notes & Observations

These are factual observations about the current code, not changes:
- **Passwords are stored and compared in plain text** (`UserService`, seed data). This is fine for a class/demo project but not production-safe; hashing (e.g. ASP.NET Core Identity or a password hasher) would be the production approach.
- The in-memory store and EF Core store hold **duplicate seed data** and are not synchronized; services never touch the database.
- Services use `lock` on the shared singleton lists for thread safety, which is appropriate for the in-memory phase but becomes unnecessary once EF Core (scoped `DbContext`) is the source of truth.
- The project/assembly name is `wad_project`; user-facing branding in the layout is still the default template text.

---

## 10. How to Run (reference)

1. Copy `.env.example` to `.env` and fill in real Supabase Postgres credentials.
2. Ensure the .NET 9 SDK is installed.
3. Apply migrations / let the app seed on startup, then run:
   ```
   dotnet run
   ```
4. Browse to the HTTPS URL from `Properties/launchSettings.json`.
