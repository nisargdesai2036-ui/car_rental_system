# ==============================================================================================
# DRIVEEASE CAR RENTAL SYSTEM - UNIFIED AUTHENTICATION & AUTHORIZATION TEST SUITE
# ==============================================================================================
$baseUrl = "http://localhost:5124"
$passCount = 0
$failCount = 0
$totalCount = 0

function Assert-Test([string]$testName, [bool]$condition, [string]$details = "") {
    $global:totalCount++
    if ($condition) {
        $global:passCount++
        Write-Host "  [PASS] $testName" -ForegroundColor Green
    } else {
        $global:failCount++
        Write-Host "  [FAIL] $testName ($details)" -ForegroundColor Red
    }
}

function Invoke-ApiRequest {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [object]$Body = $null,
        [Microsoft.PowerShell.Commands.WebRequestSession]$Session = $null
    )
    
    $params = @{
        Uri = $Uri
        Method = $Method
        ContentType = "application/json"
    }
    if ($Session) { $params["WebSession"] = $Session }
    if ($Body) { 
        if ($Body -is [string]) {
            $params["Body"] = $Body
        } else {
            $params["Body"] = ($Body | ConvertTo-Json -Depth 5)
        }
    }

    try {
        $res = Invoke-WebRequest @params -UseBasicParsing
        $jsonObj = $null
        if ($res.Content) {
            try { $jsonObj = $res.Content | ConvertFrom-Json } catch {}
        }
        return @{
            StatusCode = [int]$res.StatusCode
            Content = $res.Content
            Json = $jsonObj
            Error = $null
        }
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        $statusCode = 0
        $content = ""
        if ($resp) {
            $statusCode = [int]$resp.StatusCode
            $stream = $resp.GetResponseStream()
            if ($stream) {
                $reader = New-Object System.IO.StreamReader($stream)
                $content = $reader.ReadToEnd()
            }
        }
        $jsonObj = $null
        if ($content) {
            try { $jsonObj = $content | ConvertFrom-Json } catch {}
        }
        return @{
            StatusCode = $statusCode
            Content = $content
            Json = $jsonObj
            Error = $_.Exception.Message
        }
    } catch {
        return @{
            StatusCode = 0
            Content = ""
            Json = $null
            Error = $_.Exception.Message
        }
    }
}

Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "         DRIVEEASE AUTHENTICATION & AUTHORIZATION TEST SUITE                   " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "Target Server: $baseUrl"
Write-Host "Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n"

# ----------------------------------------------------------------------------------------------
# SUITE SECTION 1: PUBLIC & ANONYMOUS ACCESS VERIFICATION
# ----------------------------------------------------------------------------------------------
Write-Host "--------------------------------------------------------------------------------" -ForegroundColor Yellow
Write-Host "SECTION 1: Public & Anonymous Endpoint Verification" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------------------------"

$res1 = Invoke-ApiRequest -Uri "$baseUrl/api/cars" -Method "GET"
Assert-Test "1.1 Anonymous GET /api/cars -> 200 OK" ($res1.StatusCode -eq 200) "Got status $($res1.StatusCode)"

$res2 = Invoke-ApiRequest -Uri "$baseUrl/api/promocodes" -Method "GET"
Assert-Test "1.2 Anonymous GET /api/promocodes -> 200 OK" ($res2.StatusCode -eq 200) "Got status $($res2.StatusCode)"

$res3 = Invoke-ApiRequest -Uri "$baseUrl/api/cars/1/reviews" -Method "GET"
Assert-Test "1.3 Anonymous GET /api/cars/1/reviews -> 200 OK" ($res3.StatusCode -eq 200) "Got status $($res3.StatusCode)"


# ----------------------------------------------------------------------------------------------
# SUITE SECTION 2: UNAUTHENTICATED ACCESS PROTECTION (401 UNAUTHORIZED)
# ----------------------------------------------------------------------------------------------
Write-Host "`n--------------------------------------------------------------------------------" -ForegroundColor Yellow
Write-Host "SECTION 2: Unauthenticated Access Protection (Must return 401 Unauthorized)" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------------------------"

$unauth1 = Invoke-ApiRequest -Uri "$baseUrl/api/auth/me" -Method "GET"
Assert-Test "2.1 Unauthenticated GET /api/auth/me -> 401 Unauthorized" ($unauth1.StatusCode -eq 401) "Got status $($unauth1.StatusCode)"

$unauth2 = Invoke-ApiRequest -Uri "$baseUrl/api/admin/dashboard" -Method "GET"
Assert-Test "2.2 Unauthenticated GET /api/admin/dashboard -> 401 Unauthorized" ($unauth2.StatusCode -eq 401) "Got status $($unauth2.StatusCode)"

$unauth3 = Invoke-ApiRequest -Uri "$baseUrl/api/bookings/my" -Method "GET"
Assert-Test "2.3 Unauthenticated GET /api/bookings/my -> 401 Unauthorized" ($unauth3.StatusCode -eq 401) "Got status $($unauth3.StatusCode)"

$unauth4 = Invoke-ApiRequest -Uri "$baseUrl/api/cars/my" -Method "GET"
Assert-Test "2.4 Unauthenticated GET /api/cars/my -> 401 Unauthorized" ($unauth4.StatusCode -eq 401) "Got status $($unauth4.StatusCode)"


# ----------------------------------------------------------------------------------------------
# SUITE SECTION 3: AUTHENTICATION FLOW (LOGIN, CREDENTIAL VALIDATION, PROFILE, LOGOUT)
# ----------------------------------------------------------------------------------------------
Write-Host "`n--------------------------------------------------------------------------------" -ForegroundColor Yellow
Write-Host "SECTION 3: Authentication Flow & Credential Validation" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------------------------"

# 3.1 Invalid Password
$badLoginRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/login" -Method "POST" -Body @{
    emailOrMobile = "rahul@gmail.com"
    password = "WRONG_PASSWORD_XYZ"
}
Assert-Test "3.1 Login with incorrect password rejected -> 400 Bad Request" ($badLoginRes.StatusCode -eq 400) "Got status $($badLoginRes.StatusCode)"

# 3.2 Non-existent User
$ghostLoginRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/login" -Method "POST" -Body @{
    emailOrMobile = "nobody_exists_123@example.com"
    password = "Password@123"
}
Assert-Test "3.2 Login with non-existent email rejected -> 400 Bad Request" ($ghostLoginRes.StatusCode -eq 400) "Got status $($ghostLoginRes.StatusCode)"

# 3.3 Customer Login (Rahul Sharma)
$custSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$custLoginRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/login" -Method "POST" -Body @{
    emailOrMobile = "rahul@gmail.com"
    password = "1234"
} -Session $custSession
$custRoleMatch = ($custLoginRes.Json.data.role -in @("Customer", 1))
Assert-Test "3.3 Customer Login (rahul@gmail.com) -> 200 OK & Authenticated as Customer" ($custLoginRes.StatusCode -eq 200 -and $custRoleMatch) "Got status $($custLoginRes.StatusCode), role $($custLoginRes.Json.data.role)"

# 3.4 Customer Session Verification via /api/auth/me
$custMeRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/me" -Method "GET" -Session $custSession
Assert-Test "3.4 Customer GET /api/auth/me returns 'Rahul Sharma'" ($custMeRes.StatusCode -eq 200 -and $custMeRes.Json.data.fullName -eq "Rahul Sharma") "Got $($custMeRes.Json.data.fullName)"

# 3.5 Owner Login (Vikram Singh)
$ownerSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$ownerLoginRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/login" -Method "POST" -Body @{
    emailOrMobile = "owner@driveease.com"
    password = "1234"
} -Session $ownerSession
$ownerRoleMatch = ($ownerLoginRes.Json.data.role -in @("Owner", 3))
Assert-Test "3.5 Owner Login (owner@driveease.com) -> 200 OK & Role is Owner" ($ownerLoginRes.StatusCode -eq 200 -and $ownerRoleMatch) "Got status $($ownerLoginRes.StatusCode), role $($ownerLoginRes.Json.data.role)"

# 3.6 Admin Login (admin@driveease.com)
$adminSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$adminLoginRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/login" -Method "POST" -Body @{
    emailOrMobile = "admin@driveease.com"
    password = "1234"
} -Session $adminSession
$adminRoleMatch = ($adminLoginRes.Json.data.role -in @("Admin", 2))
Assert-Test "3.6 Admin Login (admin@driveease.com) -> 200 OK & Role is Admin" ($adminLoginRes.StatusCode -eq 200 -and $adminRoleMatch) "Got status $($adminLoginRes.StatusCode), role $($adminLoginRes.Json.data.role)"

# 3.7 Dynamic New Customer Registration
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$newCustEmail = "cust_$ts@testdrive.com"
$newCustPhone = "9" + "$ts".Substring(0, 9)
$tempSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$regRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/register" -Method "POST" -Body @{
    fullName = "Test Customer $ts"
    email = $newCustEmail
    mobileNumber = $newCustPhone
    drivingLicenseNumber = "DL-TEST-$ts"
    password = "1234"
    confirmPassword = "1234"
} -Session $tempSession
Assert-Test "3.7 New Customer Registration -> 201 Created / 200 OK" ($regRes.StatusCode -in @(200, 201)) "Got status $($regRes.StatusCode)"

# 3.8 Duplicate Email Registration Prevention
$dupRegRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/register" -Method "POST" -Body @{
    fullName = "Duplicate User"
    email = $newCustEmail
    mobileNumber = "9111222333"
    drivingLicenseNumber = "DL-DUP-001"
    password = "1234"
    confirmPassword = "1234"
}
Assert-Test "3.8 Duplicate Email Registration rejected -> 400 Bad Request" ($dupRegRes.StatusCode -eq 400) "Got status $($dupRegRes.StatusCode)"

# 3.9 Logout Flow Verification
$logoutRes = Invoke-ApiRequest -Uri "$baseUrl/api/auth/logout" -Method "POST" -Session $tempSession
$postLogoutMe = Invoke-ApiRequest -Uri "$baseUrl/api/auth/me" -Method "GET" -Session $tempSession
Assert-Test "3.9 Logout invalidates session -> subsequent GET /api/auth/me returns 401" ($postLogoutMe.StatusCode -eq 401) "Got status $($postLogoutMe.StatusCode)"


# ----------------------------------------------------------------------------------------------
# SUITE SECTION 4: ROLE-BASED AUTHORIZATION & ACCESS CONTROL (RBAC)
# ----------------------------------------------------------------------------------------------
Write-Host "`n--------------------------------------------------------------------------------" -ForegroundColor Yellow
Write-Host "SECTION 4: Role-Based Authorization & Permission Matrix (RBAC)" -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------------------------"

# --- 4.1 CUSTOMER PERMISSIONS ---
Write-Host "  --> Sub-tier: Customer Authorization Boundaries" -ForegroundColor Gray
$custBookingsRes = Invoke-ApiRequest -Uri "$baseUrl/api/bookings/my" -Method "GET" -Session $custSession
Assert-Test "4.1.1 Customer ALLOWED to access /api/bookings/my -> 200 OK" ($custBookingsRes.StatusCode -eq 200) "Got status $($custBookingsRes.StatusCode)"

$custAdminDash = Invoke-ApiRequest -Uri "$baseUrl/api/admin/dashboard" -Method "GET" -Session $custSession
Assert-Test "4.1.2 Customer BLOCKED from /api/admin/dashboard -> 403 Forbidden" ($custAdminDash.StatusCode -eq 403) "Got status $($custAdminDash.StatusCode)"

$custAdminOwners = Invoke-ApiRequest -Uri "$baseUrl/api/admin/owners/pending" -Method "GET" -Session $custSession
Assert-Test "4.1.3 Customer BLOCKED from /api/admin/owners/pending -> 403 Forbidden" ($custAdminOwners.StatusCode -eq 403) "Got status $($custAdminOwners.StatusCode)"

$custMyCars = Invoke-ApiRequest -Uri "$baseUrl/api/cars/my" -Method "GET" -Session $custSession
Assert-Test "4.1.4 Customer BLOCKED from /api/cars/my -> 403 Forbidden" ($custMyCars.StatusCode -eq 403) "Got status $($custMyCars.StatusCode)"

$custCreatePromo = Invoke-ApiRequest -Uri "$baseUrl/api/promocodes" -Method "POST" -Body @{
    code = "HACKDISCOUNT"
    discountType = 1
    discountValue = 50
    validFrom = (Get-Date).ToString("o")
    validTo = (Get-Date).AddMonths(1).ToString("o")
} -Session $custSession
Assert-Test "4.1.5 Customer BLOCKED from creating promo codes -> 403 Forbidden" ($custCreatePromo.StatusCode -eq 403) "Got status $($custCreatePromo.StatusCode)"


# --- 4.2 OWNER PERMISSIONS ---
Write-Host "`n  --> Sub-tier: Owner Authorization Boundaries" -ForegroundColor Gray
$ownerCarsRes = Invoke-ApiRequest -Uri "$baseUrl/api/cars/my" -Method "GET" -Session $ownerSession
Assert-Test "4.2.1 Owner ALLOWED to access /api/cars/my -> 200 OK" ($ownerCarsRes.StatusCode -eq 200) "Got status $($ownerCarsRes.StatusCode)"

$ownerBookingsRes = Invoke-ApiRequest -Uri "$baseUrl/api/owner/bookings" -Method "GET" -Session $ownerSession
Assert-Test "4.2.2 Owner ALLOWED to access /api/owner/bookings -> 200 OK" ($ownerBookingsRes.StatusCode -eq 200) "Got status $($ownerBookingsRes.StatusCode)"

$ownerAdminDash = Invoke-ApiRequest -Uri "$baseUrl/api/admin/dashboard" -Method "GET" -Session $ownerSession
Assert-Test "4.2.3 Owner BLOCKED from /api/admin/dashboard -> 403 Forbidden" ($ownerAdminDash.StatusCode -eq 403) "Got status $($ownerAdminDash.StatusCode)"

$ownerCreatePromo = Invoke-ApiRequest -Uri "$baseUrl/api/promocodes" -Method "POST" -Body @{
    code = "OWNERDISCOUNT"
    discountType = 1
    discountValue = 20
    validFrom = (Get-Date).ToString("o")
    validTo = (Get-Date).AddMonths(1).ToString("o")
} -Session $ownerSession
Assert-Test "4.2.4 Owner BLOCKED from creating promo codes -> 403 Forbidden" ($ownerCreatePromo.StatusCode -eq 403) "Got status $($ownerCreatePromo.StatusCode)"


# --- 4.3 ADMIN PERMISSIONS ---
Write-Host "`n  --> Sub-tier: Admin Authorization Capabilities" -ForegroundColor Gray
$adminDashRes = Invoke-ApiRequest -Uri "$baseUrl/api/admin/dashboard" -Method "GET" -Session $adminSession
Assert-Test "4.3.1 Admin ALLOWED to access /api/admin/dashboard -> 200 OK" ($adminDashRes.StatusCode -eq 200 -and $adminDashRes.Json.success -eq $true) "Got status $($adminDashRes.StatusCode)"

$adminPendingOwners = Invoke-ApiRequest -Uri "$baseUrl/api/admin/owners/pending" -Method "GET" -Session $adminSession
Assert-Test "4.3.2 Admin ALLOWED to access /api/admin/owners/pending -> 200 OK" ($adminPendingOwners.StatusCode -eq 200) "Got status $($adminPendingOwners.StatusCode)"

$adminPendingCustomers = Invoke-ApiRequest -Uri "$baseUrl/api/admin/customers/pending" -Method "GET" -Session $adminSession
Assert-Test "4.3.3 Admin ALLOWED to access /api/admin/customers/pending -> 200 OK" ($adminPendingCustomers.StatusCode -eq 200) "Got status $($adminPendingCustomers.StatusCode)"

$adminAllBookings = Invoke-ApiRequest -Uri "$baseUrl/api/bookings" -Method "GET" -Session $adminSession
Assert-Test "4.3.4 Admin ALLOWED to view all system bookings -> 200 OK" ($adminAllBookings.StatusCode -eq 200) "Got status $($adminAllBookings.StatusCode)"

$promoCodeName = "PR" + [Guid]::NewGuid().ToString("N").Substring(0, 8).ToUpper()
$adminCreatePromo = Invoke-ApiRequest -Uri "$baseUrl/api/promocodes" -Method "POST" -Body @{
    code = $promoCodeName
    discountType = 1
    discountValue = 15
    minBookingAmount = 500
    validFrom = (Get-Date).ToString("o")
    validTo = (Get-Date).AddMonths(1).ToString("o")
    isActive = $true
} -Session $adminSession
Assert-Test "4.3.5 Admin ALLOWED to create Promo Codes -> 200/201 OK" ($adminCreatePromo.StatusCode -in @(200, 201)) "Got status $($adminCreatePromo.StatusCode)"


# ----------------------------------------------------------------------------------------------
# TEST SUMMARY
# ----------------------------------------------------------------------------------------------
Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host "                         TEST EXECUTION SUMMARY                                " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "Total Checks Executed : $totalCount"
Write-Host "Passed                : $passCount" -ForegroundColor Green
Write-Host "Failed                : $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
Write-Host "Pass Rate             : $([math]::Round(($passCount / $totalCount) * 100, 2))%"

if ($failCount -eq 0) {
    Write-Host "`n>>> ALL AUTHENTICATION & AUTHORIZATION TESTS PASSED SUCCESSFULLY! (100%) <<<" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n>>> SOME TESTS FAILED! PLEASE CHECK THE LOG ABOVE. <<<" -ForegroundColor Red
    exit 1
}
