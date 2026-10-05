# DriveEase — Project Overview & Architecture Documentation

DriveEase is a full-featured **self-drive vehicle rental & fleet management platform** (supporting luxury cars, SUVs, sedans, hatchbacks, and bikes) built with **ASP.NET Core 10.0 MVC & REST Web API**, Entity Framework Core 10, ASP.NET Core Identity, and SQLite database.

Customers can browse and search vehicles with multi-criteria filtering, calculate dynamic tariffs (hourly & daily rates with surge pricing and promo discounts), make reservations, complete simulated payments, and leave reviews. Vehicle owners can list their vehicles and view fleet earnings, while administrators manage approvals, user accounts, promo codes, and platform analytics.

---

## 1. Tech Stack & Dependencies

| Concern | Technology / Library | Version / Detail |
|---|---|---|
| **Framework** | ASP.NET Core 10.0 Web Application | Target Framework: `net10.0` |
| **Language** | C# 13 | Nullable reference types & implicit usings enabled |
| **Root Namespace** | `wad_project` | Assembly name: `wad-project` |
| **Database & ORM** | Entity Framework Core 10 | SQLite (`Microsoft.EntityFrameworkCore.Sqlite` 10.0.0) |
| **Authentication & AuthZ** | ASP.NET Core Identity | Cookie-based session + custom API HTTP 401/403 status handler |
| **API Documentation** | OpenAPI / Swagger UI | `Swashbuckle.AspNetCore` 7.3.1 |
| **Config / Environment** | `dotenv.net` | Version 4.2.0 |
| **Front-End UI** | Razor Views + Bootstrap 5 + Custom Design System | "Midnight Obsidian Velocity" Dark Theme (`driveease.css`) |
| **Design System Fonts** | Inter & Plus Jakarta Sans | Material Symbols Outlined Icons |

---

## 2. High-Level System Architecture

The codebase combines **MVC Razor Views** for user interaction with a robust set of **REST API Endpoints** for client/mobile consumption.

```
DriveEase Root/
├── Program.cs             → Application entry point, DI configuration, Identity middleware, DbSeeder trigger
├── Controllers/           → MVC & REST API Controllers (Auth, Cars, Bookings, Payments, Admin, Reviews, etc.)
├── Models/                → EF Core Entities (User, Vehicle, Booking, Payment, Review, PromoCode, PricingRule, Notification) & Enums
├── ViewModels/            → Data Transfer Objects (DTOs) & UI Form Models with DataAnnotation validation
├── Data/                  → ApplicationDbContext (EF Core) & DbSeeder (Initial seed data for users, cars, bookings)
├── Services/              → Business Logic (BookingService with pricing engine & overlap detection)
├── DTOs/                  → Dedicated API request/response contracts
├── Views/                 → Razor HTML Views (Home Index, Explore, Book, Account Login/Register/Profile, Shared Layout)
├── wwwroot/               → Static assets (Custom driveease.css theme, JavaScript, Bootstrap)
└── docs/                  → System architecture, API matrix, test suite documentation
```

### Request Execution Flow
```
Browser / Client → Controller (MVC / REST API) → Service Layer (Business Rules & Overlap Check) → EF Core DbContext → SQLite Database (DriveEase.db)
```

---

## 3. Data Model & Entity Schema (`Models/`)

The database is built on **EF Core 10** with relational integrity, unique indexes, and soft-delete capabilities:

| Entity Model | Purpose | Key Attributes & Relationships |
|---|---|---|
| **`ApplicationUser` / `User`** | Platform user account | Inherits ASP.NET Identity `IdentityUser`. Attributes: `FullName`, `UserRole`, `VerificationStatus`, `DrivingLicenseNumber`, `IsActive`. Has many `Bookings`, `Vehicles` (if Owner), `Reviews`. |
| **`Vehicle`** | Rentable car or bike | Attributes: `Brand`, `Model`, `Year`, `LicensePlate`, `VehicleType`, `FuelType`, `Transmission`, `SeatingCapacity`, `HourlyRate`, `DailyRate`, `Status`, `IsApproved`, `AverageRating`, `PickupLocation`, `ImageUrl`. Soft-deleted (`IsDeleted`). |
| **`Booking`** | Rental reservation | Attributes: `BookingReference` (e.g. `DE-202610-1001`), `PickupDateTime`, `ReturnDateTime`, `RentalType`, `BasePrice`, `SurgeAmount`, `DiscountAmount`, `TotalAmount`, `Status` (`Pending`, `Confirmed`, `Completed`, `Cancelled`). FK to `User`, `Vehicle`, optional `PromoCode`. |
| **`Payment`** | Payment transaction | Attributes: `TransactionId`, `Amount`, `PaymentMethod`, `PaymentStatus`, `PaymentDate`. 1-to-1 relationship with `Booking`. |
| **`Review`** | Customer rating & feedback | Attributes: `Rating` (1 to 5), `Comment`, `CreatedAt`. Links `User`, `Vehicle`, and `Booking`. Automatically updates vehicle `AverageRating` and `ReviewCount`. |
| **`PromoCode`** | Discount promotional code | Attributes: `Code`, `DiscountType` (`Percentage`, `FlatAmount`), `DiscountValue`, `MinBookingAmount`, `MaxDiscountAmount`, `ValidUntil`, `IsActive`. |
| **`PricingRule`** | Dynamic pricing engine rule | Attributes: `Name`, `SurgeMultiplier` (e.g., 1.25x for weekends), `DayOfWeek`, `IsActive`. |
| **`Notification`** | In-app user notifications | Attributes: `Title`, `Message`, `Type`, `IsRead`, `CreatedAt`. FK to `User`. |

---

## 4. Business Logic & Core Algorithms (`Services/`)

### 1. Vehicle Overlap Detection Algorithm (`BookingService.cs`)
To prevent double-booking, vehicle availability is evaluated using interval collision detection:
$$\text{Overlap} \iff (\text{pickup} < \text{existing.ReturnDateTime}) \land (\text{return} > \text{existing.PickupDateTime})$$
Bookings with status `Cancelled` or vehicles in `UnderMaintenance` / `Booked` status are excluded.

### 2. Pricing & Discount Calculation Engine (`BookingService.cs`)
The price calculation handles:
1. **Base Rate**: Based on duration (hours or days) and vehicle rates.
2. **Weekend Surge**: Applies dynamic multiplier if dates fall on Saturday or Sunday.
3. **Promo Code Discount**: Evaluates minimum booking amount, percentage or flat discount cap, and validity date.
4. **Total Calculation**: $\text{Total} = \text{BasePrice} + \text{SurgeAmount} - \text{DiscountAmount}$.

---

## 5. Security & Authorization Architecture

- **Authentication**: ASP.NET Core Identity with persistent cookie authentication (`.DriveEase.Auth`).
- **Role-Based Access Control (RBAC)**:
  - **`Admin`**: Platform-wide monitoring, owner/customer approvals, vehicle listing moderation, promo code management, user activation/deactivation.
  - **`Owner`**: Vehicle listing submission, vehicle availability & maintenance status toggles, viewing fleet bookings and earnings.
  - **`Customer`**: Vehicle discovery, booking creation, simulated payment execution, trip history, writing reviews.
- **API Status Code Handling**: Standard ASP.NET Core Identity redirects unauthenticated requests to `/Account/Login`. For API requests starting with `/api/`, custom middleware intercepts redirects and returns proper JSON `401 Unauthorized` or `403 Forbidden` status codes.

---

## 6. User Interface & Design System

The application features a modern, luxury automotive design system named **"Midnight Obsidian Velocity"**:
- **Color Palette**: Dark obsidian canvas (`#0A0F1D`), dark slate surface containers (`#1A1F2E`), indigo-to-purple gradient primary controls (`#6366F1` to `#8B5CF6`).
- **Contrast & Text Visibility**: High-contrast text token overrides (`#F8FAFC` primary white, `#94A3B8` light slate muted text, `#CBD5E1` form labels) ensuring full WCAG readability across all pages, forms, modals, tables, and selects.
- **Micro-Animations & Glassmorphism**: Translucent backdrop blur, ambient flares, card hover transformations, interactive booking modals, and responsive navigation pills.

---

## 7. How to Run the Application

1. **Prerequisites**: .NET 10.0 SDK installed (`dotnet --info`).
2. **Database Initialization**: The SQLite database (`DriveEase.db`) is automatically initialized and seeded on startup.
3. **Execution Command**:
   ```powershell
   cd s:\PROJECTS\DriveEase
   dotnet run
   ```
4. **Access Endpoints**:
   - **Web Application**: `http://localhost:5124` or `http://localhost:5000`
   - **Swagger API Docs**: `http://localhost:5124/swagger`

### Default Seeded Test Accounts (Password: `1234`)
- **Admin**: `admin@driveease.com`
- **Customer**: `rahul@gmail.com`
- **Owner**: `owner@driveease.com`
