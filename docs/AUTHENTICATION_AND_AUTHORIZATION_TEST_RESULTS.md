# DriveEase Authentication & Authorization Test Results

## Overview

This document records the verification and testing results for the **Authentication (AuthN)** and **Role-Based Access Control (AuthZ / RBAC)** systems implemented in **DriveEase Car Rental System**.

Testing was performed using a single, unified, automated verification test suite: [`run_auth_authz_tests.ps1`](file:///d:/wad-project/run_auth_authz_tests.ps1).

---

## Executive Summary

| Attribute | Value |
| :--- | :--- |
| **Target Host** | `http://localhost:5124` |
| **Frameworks** | ASP.NET Core 9.0 Identity, Cookie Authentication, EF Core, PostgreSQL |
| **Total Test Assertions** | **30** |
| **Passed Assertions** | **30** |
| **Failed Assertions** | **0** |
| **Overall Pass Rate** | **100%** |
| **Status** | **VERIFIED & SECURE** |

---

## Roles & Permission Structure

The system enforces three primary user roles with strictly segregated authorization boundaries:

1. **Admin**: Platform oversight, owner/customer verification approvals, promo code lifecycle management, platform analytics dashboard, and system-wide bookings.
2. **Owner**: Vehicle listing submission, vehicle availability & maintenance toggles, and viewing bookings for owned vehicles.
3. **Customer**: Vehicle catalog discovery, booking creation, personal booking history, and reviews for completed trips.

---

## Detailed Test Matrix & Results

### Section 1: Public & Anonymous Endpoint Verification

Verifies that publicly accessible endpoints serve data without requiring authentication tokens or cookies.

| ID | Test Scenario | Target Endpoint | Expected Status | Result |
| :--- | :--- | :--- | :--- | :--- |
| **1.1** | Public vehicle search and filtering catalog | `GET /api/cars` | `200 OK` | **PASS** |
| **1.2** | Public active promo codes discovery | `GET /api/promocodes` | `200 OK` | **PASS** |
| **1.3** | Public vehicle customer ratings and reviews | `GET /api/cars/1/reviews` | `200 OK` | **PASS** |

---

### Section 2: Unauthenticated Access Protection (401 Unauthorized)

Verifies that all secure endpoints reject unauthenticated requests and return HTTP `401 Unauthorized` without session leakage.

| ID | Test Scenario | Target Endpoint | Expected Status | Result |
| :--- | :--- | :--- | :--- | :--- |
| **2.1** | Unauthenticated profile check | `GET /api/auth/me` | `401 Unauthorized` | **PASS** |
| **2.2** | Unauthenticated admin dashboard access | `GET /api/admin/dashboard` | `401 Unauthorized` | **PASS** |
| **2.3** | Unauthenticated personal bookings access | `GET /api/bookings/my` | `401 Unauthorized` | **PASS** |
| **2.4** | Unauthenticated owner vehicles access | `GET /api/cars/my` | `401 Unauthorized` | **PASS** |

---

### Section 3: Authentication Flow & Credential Validation

Verifies credential validation, session issuance (`.DriveEase.Auth` cookie), profile lookup, user registration, and logout invalidation.

| ID | Test Scenario | Payload / Target | Expected Result | Result |
| :--- | :--- | :--- | :--- | :--- |
| **3.1** | Incorrect password attempt | `POST /api/auth/login` (`rahul@gmail.com`, `WRONG_PW`) | `400 Bad Request` | **PASS** |
| **3.2** | Non-existent user login attempt | `POST /api/auth/login` (`nobody_exists@example.com`) | `400 Bad Request` | **PASS** |
| **3.3** | Customer login | `POST /api/auth/login` (`rahul@gmail.com`, `1234`) | `200 OK` (Role: `Customer`) | **PASS** |
| **3.4** | Authenticated session profile retrieval | `GET /api/auth/me` (Customer Session) | `200 OK` (`Rahul Sharma`) | **PASS** |
| **3.5** | Owner login | `POST /api/auth/login` (`owner@driveease.com`, `1234`) | `200 OK` (Role: `Owner`) | **PASS** |
| **3.6** | Admin login | `POST /api/auth/login` (`admin@driveease.com`, `1234`) | `200 OK` (Role: `Admin`) | **PASS** |
| **3.7** | Dynamic customer registration | `POST /api/auth/register` (New user payload) | `201 Created` / `200 OK` | **PASS** |
| **3.8** | Duplicate email registration guard | `POST /api/auth/register` (Reusing email) | `400 Bad Request` | **PASS** |
| **3.9** | Session invalidation on logout | `POST /api/auth/logout` &rarr; subsequent `GET /api/auth/me` | `401 Unauthorized` | **PASS** |

---

### Section 4: Role-Based Authorization & Permission Matrix (RBAC)

Verifies strict role boundaries, preventing privilege escalation between Customer, Owner, and Admin tiers.

#### 4.1 Customer Role Boundaries
| ID | Endpoint | Action | Expected Status | Result |
| :--- | :--- | :--- | :--- | :--- |
| **4.1.1** | `GET /api/bookings/my` | View customer's own bookings | `200 OK` (Allowed) | **PASS** |
| **4.1.2** | `GET /api/admin/dashboard` | Access admin analytics dashboard | `403 Forbidden` (Blocked) | **PASS** |
| **4.1.3** | `GET /api/admin/owners/pending` | Access pending owners list | `403 Forbidden` (Blocked) | **PASS** |
| **4.1.4** | `GET /api/cars/my` | Access owner vehicle management | `403 Forbidden` (Blocked) | **PASS** |
| **4.1.5** | `POST /api/promocodes` | Create platform discount code | `403 Forbidden` (Blocked) | **PASS** |

#### 4.2 Owner Role Boundaries
| ID | Endpoint | Action | Expected Status | Result |
| :--- | :--- | :--- | :--- | :--- |
| **4.2.1** | `GET /api/cars/my` | View owner's listed vehicles | `200 OK` (Allowed) | **PASS** |
| **4.2.2** | `GET /api/owner/bookings` | View bookings for owned vehicles | `200 OK` (Allowed) | **PASS** |
| **4.2.3** | `GET /api/admin/dashboard` | Access admin analytics dashboard | `403 Forbidden` (Blocked) | **PASS** |
| **4.2.4** | `POST /api/promocodes` | Create platform discount code | `403 Forbidden` (Blocked) | **PASS** |

#### 4.3 Admin Role Capabilities
| ID | Endpoint | Action | Expected Status | Result |
| :--- | :--- | :--- | :--- | :--- |
| **4.3.1** | `GET /api/admin/dashboard` | View platform-wide metrics & revenue | `200 OK` (Allowed) | **PASS** |
| **4.3.2** | `GET /api/admin/owners/pending` | Review pending owner verification queue | `200 OK` (Allowed) | **PASS** |
| **4.3.3** | `GET /api/admin/customers/pending`| Review pending customer verification queue | `200 OK` (Allowed) | **PASS** |
| **4.3.4** | `GET /api/bookings` | Access all platform bookings | `200 OK` (Allowed) | **PASS** |
| **4.3.5** | `POST /api/promocodes` | Create promotional code | `201 Created` / `200 OK` (Allowed) | **PASS** |

---

## How to Execute the Test Suite

Run the following command in PowerShell to perform the full automated suite:

```powershell
powershell -ExecutionPolicy Bypass -File d:\wad-project\run_auth_authz_tests.ps1
```
