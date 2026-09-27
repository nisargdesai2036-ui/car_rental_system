# DriveEase Design System

> Extracted from Stitch Project: **driveEase.** (`projects/14990455982938224880`)  
> Design Theme: **Midnight Obsidian Velocity**  
> Aesthetics: **Modern Corporate Automotive Tech • Glassmorphic Tonal Layering • Electric Violet & Indigo Highlights**

---

## 1. Design Philosophy & Aesthetic Identity

**DriveEase** embodies a luxury automotive self-drive rental platform engineered for modern professionals, road travelers, and auto enthusiasts. 

Key visual principles:
- **Atmospheric Depth**: Deep obsidian slate foundations (`#0E1321` / `#0A0F1D`) with subtle radial light blooms (`#6366F1` & `#8B5CF6`).
- **Precision Glassmorphism**: Translucent panels (`backdrop-blur-xl`, `backdrop-blur-2xl`) encased in ultra-thin, high-contrast borders (`1px solid rgba(255, 255, 255, 0.08)`).
- **High-Octane Accents**: Electric violet-to-indigo gradient CTAs with ambient colored glow shadows.
- **Micro-Data Density**: Compact pill indicators, spec tags, and high-legibility tabular telemetry.

---

## 2. Color Palette & Tokens

### 2.1 Core Brand Colors
| Token Name | Hex Code | Purpose / Usage |
| :--- | :--- | :--- |
| `primary` | `#6366F1` / `#C0C1FF` | Primary electric indigo brand color, active indicators, focused outlines |
| `primary-gradient` | `linear-gradient(135deg, #6366F1 0%, #8B5CF6 100%)` | Primary high-emphasis CTA buttons, hero badges, active step pills |
| `secondary` | `#8B5CF6` / `#D0BCFF` | Secondary violet accent, gradient mid-tones, VIP/Gold highlights |
| `secondary-container` | `#571BC1` | Deep violet ambient glows, elevated active badges |
| `tertiary` | `#38BDF8` / `#7BD0FF` | Electric cyan precision highlight, live GPS tags, spec highlights |
| `tertiary-container` | `#009BD1` | Ambient teal/cyan underlay flares |

### 2.2 Neutral & Surface Hierarchy
| Surface Token | Hex Code | Description & Usage |
| :--- | :--- | :--- |
| `background` / `canvas` | `#0E1321` / `#0A0F1D` | Deepest root background canvas layer |
| `surface-container-lowest` | `#090E1C` | Deepest inset containers, recessed search rails, kbd background |
| `surface-container-low` | `#161B2A` | Form inputs, inactive filter chips, secondary card modules |
| `surface-container` | `#1A1F2E` | Base card backgrounds, floating search widgets, modals |
| `surface-container-high` | `#252A39` | Hover card elevations, elevated pill containers, active sub-tabs |
| `surface-container-highest`| `#303444` | Tooltips, dropdown popovers, high-contrast badges |
| `surface-bright` | `#343948` | Interactive active/pressed states |

### 2.3 Text & Content Colors
| Token Name | Hex Code | Purpose |
| :--- | :--- | :--- |
| `on-surface` / `text-primary` | `#DEE2F6` / `#F8FAFC` | Primary headings, prominent values, active navigation items |
| `on-surface-variant` / `text-muted` | `#C7C4D7` / `#94A3B8` | Subtitles, helper text, inactive labels, spec glyphs |
| `text-subtle` | `#908FA0` / `#64748B` | Disabled labels, placeholder text, border outlines |

### 2.4 Functional & Status Colors
| Status | Hex Code | Usage |
| :--- | :--- | :--- |
| `success` | `#10B981` | Confirmed bookings, available car status, payment success |
| `warning` / `rating` | `#F59E0B` / `#FCD34D` | Customer star ratings, pending verification, alerts |
| `danger` / `error` | `#EF4444` / `#FFB4AB` | Booking cancellation, validation errors, payment failure |
| `info` | `#38BDF8` | Road assistance updates, notification badges |

---

## 3. Typography & Font Hierarchy

### 3.1 Font Families
- **Headlines & Display**: `Plus Jakarta Sans`, sans-serif (Weights: `600`, `700`, `800`)
- **Body, UI & Specifications**: `Inter`, sans-serif (Weights: `400`, `500`, `600`, `700`)
- **Code & Shortcuts**: System Monospace / Inter (`kbd`)

### 3.2 Scale & Line Heights

```css
/* Typography Scale */
--font-headline-xl: 40px; line-height: 48px; font-weight: 700; /* Plus Jakarta Sans (Desktop) */
--font-headline-xl-mobile: 28px; line-height: 36px; font-weight: 700;
--font-headline-lg: 30px; line-height: 38px; font-weight: 700; /* Plus Jakarta Sans (Desktop) */
--font-headline-lg-mobile: 24px; line-height: 32px; font-weight: 700;
--font-headline-md: 22px; line-height: 28px; font-weight: 600; /* Plus Jakarta Sans */
--font-headline-sm: 18px; line-height: 24px; font-weight: 600; /* Plus Jakarta Sans */

--font-body-lg: 16px; line-height: 24px; font-weight: 400;     /* Inter */
--font-body-md: 14px; line-height: 20px; font-weight: 400;     /* Inter */
--font-body-sm: 12px; line-height: 16px; font-weight: 400;     /* Inter */

--font-label-lg: 14px; line-height: 20px; font-weight: 600;    /* Inter */
--font-label-md: 12px; line-height: 16px; font-weight: 600;    /* Inter */
--font-label-sm: 10px; line-height: 14px; font-weight: 700;    /* Inter (Uppercase/Tracking) */
```

---

## 4. Spacing, Grid & Layout System

The spatial hierarchy uses an **8-point harmonic rhythm**.

### 4.1 Spacing Tokens
| Token | REM Value | Pixel Value | Typical Application |
| :--- | :--- | :--- | :--- |
| `space-xs` | `0.25rem` | `4px` | Tag padding, icon-to-text spacing, micro badges |
| `space-sm` | `0.5rem` | `8px` | Inner input padding, pill gaps, tight list items |
| `space-md` | `1.0rem` | `16px` | Standard component padding, form row spacing |
| `space-lg` | `1.5rem` | `24px` | Card internal padding, section subdivisions |
| `space-xl` | `2.25rem` | `36px` | Hero padding, major section gaps |

### 4.2 Layout Grid & Margins
- **Desktop Grid**: 12-column layout, `max-width: 1440px`, outer margin `margin: 2rem (32px)`, column gutter `gutter: 1rem (16px)`.
- **Tablet Grid**: 8-column layout, outer margin `24px`, column gutter `16px`.
- **Mobile Grid**: 4-column fluid layout, outer margin `margin-mobile: 1rem (16px)`, column gutter `gutter-mobile: 0.75rem (12px)`.

---

## 5. Border Radius & Elevation Tokens

### 5.1 Curvature Rules
| Token | Value | Applied To |
| :--- | :--- | :--- |
| `rounded-sm` | `0.25rem (4px)` | Checkboxes, status badges |
| `rounded-md` | `0.5rem (8px)` | Form dropdowns, filter items, keyboard shortcut badges |
| `rounded-lg` | `0.75rem (12px)` | Form input boxes, small cards |
| `rounded-xl` | `1.0rem (16px)` | Metrics cards, vehicle spec pills |
| `rounded-2xl`| `1.25rem (20px)`| Vehicle listing cards, review boxes, summary cards |
| `rounded-3xl`| `1.5rem (24px)` | Hero containers, floating search booking widget, modal dialogs |
| `rounded-full`| `9999px` | Buttons, navigation pills, status badges, avatar badges |

### 5.2 Shadows & Depth
- **Level 0 (Flat Ground)**: `background: #0E1321`
- **Level 1 (Subtle Glass Surface)**: `background: rgba(26, 31, 46, 0.85); backdrop-filter: blur(16px); border: 1px solid rgba(255, 255, 255, 0.08);`
- **Level 2 (Elevated Card / Modal)**: `background: #1A1F2E; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5); border: 1px solid rgba(255, 255, 255, 0.12);`
- **Level 3 (Electric Glow CTA)**: `box-shadow: 0 8px 24px rgba(99, 102, 241, 0.35);` (Hover: `0 12px 32px rgba(99, 102, 241, 0.50)`)

---

## 6. Core Component Specifications

### 6.1 Buttons & Interactive Triggers

1. **Primary Action Button (Electric Gradient)**
   - Fill: `linear-gradient(135deg, #6366F1 0%, #8B5CF6 100%)`
   - Text: `#FFFFFF`, `font-label-lg` (Weight 600/700)
   - Shape: `rounded-full`
   - Icon: Trailing arrow (`arrow_forward`) with hover translate effect (`group-hover:translate-x-1`)
   - Shadow: `0 8px 24px rgba(99, 102, 241, 0.35)`
   - Hover: Transform `-translate-y-0.5`, scale `0.98` on click.

2. **Secondary / Ghost Button**
   - Fill: `bg-surface-container-high` (`#252A39`) or `bg-surface-container-low`
   - Border: `1px solid rgba(255, 255, 255, 0.08)`
   - Text: `text-on-surface` (`#DEE2F6`)
   - Shape: `rounded-full`
   - Hover: `bg-surface-bright` (`#343948`), border color transitions to `rgba(99, 102, 241, 0.4)`

3. **Segmented Rail Filter Buttons**
   - Inactive: `text-on-surface-variant`, transparent background
   - Active: `bg-surface-container-high text-on-surface font-semibold rounded-full shadow-sm`

---

### 6.2 Navigation & Header
- **Fixed Glass Header**: `height: 80px`, `bg-surface/85 backdrop-blur-xl`, border bottom `1px solid rgba(255, 255, 255, 0.06)`.
- **Brand Cluster**: DriveEase gradient logo + bold title + vertical divider + quick location picker pill (`Bangalore BLR`).
- **Pill Navigation Rail**: Floating encapsulated pill container (`bg-surface-container-lowest/60 backdrop-blur-md`) with active highlight pill.
- **Search Trigger**: Quick model/location search with `⌘K` keyboard shortcut badge.
- **User Profile Pill**: Avatar image + Full Name + Membership Tier Badge (`Gold Member` in `#6366F1`) + dropdown chevron.

---

### 6.3 Vehicle Listing Cards
- **Geometry**: `rounded-2xl`, background `bg-surface-container/90`, border `1px solid rgba(255, 255, 255, 0.08)`.
- **Top Badge**: Floating category tag (`"BEST SELLER"`, `"LUXURY"`, `"ELECTRIC"`) with radial gradient pill.
- **Car Showcase**: Vehicle image on subtle cyan/indigo radial glow disc (`blur-2xl`).
- **Telemetry / Spec Badges**: 4-column inline spec strip:
  - Seats (`group` icon, e.g. `5 Seats`)
  - Transmission (`settings` icon, e.g. `Auto / Manual`)
  - Fuel Type (`local_gas_station` or `bolt` for EV)
  - Mileage / Fuel Policy (`speed` or `eco`)
- **Price & CTA Anchor**:
  - Pricing: Large bold rate (`₹2,499` in `Plus Jakarta Sans`) + `/day` or `/hr` text.
  - Action: Gradient primary `"Book Now"` button.

---

### 6.4 Forms & Booking Search Widget
- **Floating Main Booking Widget**: Sits below Hero with `-mt-6` offset, `rounded-3xl bg-surface-container/95 backdrop-blur-2xl shadow-2xl`.
- **Multi-Segment Grid**:
  - `Pick-up Location`: Map pin icon, location selector, subtitle doorstep badge.
  - `Pick-up Date & Time`: Calendar icon, datetime picker.
  - `Drop-off Date & Time`: Calendar icon, duration counter badge.
  - `Vehicle Type / Filter`: Car category chip selector.
  - `Submit CTA`: Full-height gradient search button with icon.
- **Inputs**: Background `bg-surface-container-low`, hover `bg-surface-container-high`, focus ring `2px solid #6366F1`.

---

### 6.5 Booking & Reservation Flow UI
- **Step Progress Bar**: Multi-step indicator (1. Vehicle Selection &rarr; 2. Add-ons & Protection &rarr; 3. Driver Details &rarr; 4. Payment & Confirmation).
- **Protection & Insurance Add-on Cards**: Selectable radio cards (Basic vs Comprehensive Protection) with checkmark status.
- **Pricing & Coupon Breakdown Module**:
  - Base rental charges (Hourly/Daily rate &times; duration)
  - Promo discount badge (shows savings in `#10B981`)
  - Taxes & security deposit breakdown
  - Total payable prominent figure

---

### 6.6 Payment UI
- **Payment Method Switcher**: Tabbed selector (UPI / Google Pay / PhonePe, Credit & Debit Cards, Net Banking, Wallet).
- **Security Trust Badges**: 256-bit SSL encryption badge, instant refund guarantee badge.
- **Transaction Flow**: Simulated instant payment processing state with animated loader and confirmation receipt card.

---

### 6.7 Profile & Customer Dashboard UI
- **Header Profile Card**: User avatar, verified driving license badge (`Verified` in green pill), mobile number, email.
- **Trip History & Status Tabs**:
  - Active Bookings (with live status, countdown, vehicle photo, keyless pickup code)
  - Past / Completed Trips (with invoice download & "Write Review" button)
  - Cancelled Bookings
- **Fleet Admin / Owner Toggle**: Quick switcher for vehicle owners to view listed fleet metrics, earnings, and availability toggles.

---

## 7. Responsive Breakpoint Rules

| Breakpoint | Viewport Range | Layout Adjustments |
| :--- | :--- | :--- |
| **Mobile (`sm`)** | `< 640px` | Single column card stacks, bottom navigation bar, collapsed search modal |
| **Tablet (`md`)** | `640px - 1023px` | 2-column vehicle grid, 2-column form rows, collapsed hamburger nav |
| **Desktop (`lg`)** | `1024px - 1279px`| 3-column vehicle grid, horizontal booking bar, full glass header |
| **Wide Desktop (`xl`)** | `1280px - 1440px+` | 12-column grid, sticky filter rail on explore page, 3/4-column car catalog |

---

## 8. Asset References & Visual Icons

- **Icon Set**: Google Material Symbols Outlined (`family=Material+Symbols+Outlined`)
- **Brand Logos & Imagery**:
  - Brand Logo: High-res SVG with `#6366F1` gradient mark
  - Avatar Headshots: Circular masked portraits with studio lighting
  - Vehicle Assets: High-resolution PNGs with transparent backgrounds and drop-shadow underlays.
