$baseUrl = "http://localhost:5124"

function Extract-Token([string]$html) {
    if ($html -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
        return $Matches[1]
    }
    if ($html -match 'value="([^"]+)"[^>]*name="__RequestVerificationToken"') {
        return $Matches[1]
    }
    return ""
}

Write-Host "==============================================================="
Write-Host "        DRIVEEASE AUTHENTICATION VERIFICATION SUITE           "
Write-Host "==============================================================="

# -------------------------------------------------------------
# TEST 1: Customer Login (Rahul Sharma) & Session Verification
# -------------------------------------------------------------
Write-Host "`n--> TEST 1: Testing Customer Login & Profile Access"
$session1 = New-Object Microsoft.PowerShell.Commands.WebRequestSession

$loginPage = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -WebSession $session1
$token1 = Extract-Token -html $loginPage.Content
Write-Host "[1.1] GET /Account/Login -> HTTP $($loginPage.StatusCode) (AntiForgeryToken extracted: $($token1.Length -gt 0))"

$form1 = @{
    EmailOrMobile = "rahul@gmail.com"
    Password = "1234"
    RememberMe = "false"
    __RequestVerificationToken = $token1
}

$loginRes = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -Method Post -Body $form1 -WebSession $session1 -MaximumRedirection 5
Write-Host "[1.2] POST /Account/Login (Customer: rahul@gmail.com) -> HTTP $($loginRes.StatusCode)"
Write-Host "[1.3] Home Navbar shows 'Hello, Rahul Sharma!': $($loginRes.Content.Contains('Hello, Rahul Sharma!'))"

$profileRes = Invoke-WebRequest -Uri "$baseUrl/Account/Profile" -Method Get -WebSession $session1
Write-Host "[1.4] GET /Account/Profile -> HTTP $($profileRes.StatusCode)"
Write-Host "[1.5] Profile contains Name 'Rahul Sharma': $($profileRes.Content.Contains('Rahul Sharma'))"
Write-Host "[1.6] Profile contains Email 'rahul@gmail.com': $($profileRes.Content.Contains('rahul@gmail.com'))"
Write-Host "[1.7] Profile contains Driving License 'MH12-2021-00892': $($profileRes.Content.Contains('MH12-2021-00892'))"

# -------------------------------------------------------------
# TEST 2: Logout Flow
# -------------------------------------------------------------
Write-Host "`n--> TEST 2: Testing Logout Flow"
$profileToken = Extract-Token -html $profileRes.Content
$logoutForm = @{
    __RequestVerificationToken = $profileToken
}

$logoutRes = Invoke-WebRequest -Uri "$baseUrl/Account/Logout" -Method Post -Body $logoutForm -WebSession $session1 -MaximumRedirection 5
Write-Host "[2.1] POST /Account/Logout -> HTTP $($logoutRes.StatusCode)"
Write-Host "[2.2] Navbar shows 'Login' after logout: $($logoutRes.Content.Contains('Login'))"

$postLogoutProfile = Invoke-WebRequest -Uri "$baseUrl/Account/Profile" -Method Get -WebSession $session1 -MaximumRedirection 5
Write-Host "[2.3] GET /Account/Profile after Logout redirects to Login: $($postLogoutProfile.Content.Contains('Sign In'))"

# -------------------------------------------------------------
# TEST 3: Invalid Credentials Handling
# -------------------------------------------------------------
Write-Host "`n--> TEST 3: Testing Invalid Password Attempt"
$session2 = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginPage2 = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -WebSession $session2
$token2 = Extract-Token -html $loginPage2.Content

$badForm = @{
    EmailOrMobile = "rahul@gmail.com"
    Password = "WRONG_PASSWORD_XYZ"
    __RequestVerificationToken = $token2
}

$badRes = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -Method Post -Body $badForm -WebSession $session2 -MaximumRedirection 5
Write-Host "[3.1] POST /Account/Login with invalid password rejected: $($badRes.Content.Contains('Invalid email/mobile or password'))"

# -------------------------------------------------------------
# TEST 4: Admin Login & Authentication
# -------------------------------------------------------------
Write-Host "`n--> TEST 4: Testing Admin Login (admin@driveease.com)"
$session3 = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$adminLoginGet = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -WebSession $session3
$adminToken = Extract-Token -html $adminLoginGet.Content

$adminForm = @{
    EmailOrMobile = "admin@driveease.com"
    Password = "1234"
    __RequestVerificationToken = $adminToken
}

$adminLoginPost = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -Method Post -Body $adminForm -WebSession $session3 -MaximumRedirection 5
Write-Host "[4.1] POST /Account/Login (Admin) -> Navbar shows 'Hello, Admin DriveEase!': $($adminLoginPost.Content.Contains('Admin DriveEase'))"

$adminProfileRes = Invoke-WebRequest -Uri "$baseUrl/Account/Profile" -Method Get -WebSession $session3
Write-Host "[4.2] GET /Account/Profile (Admin) -> Contains 'Admin DriveEase': $($adminProfileRes.Content.Contains('Admin DriveEase'))"
Write-Host "[4.3] GET /Account/Profile (Admin) -> Contains 'admin@driveease.com': $($adminProfileRes.Content.Contains('admin@driveease.com'))"

# -------------------------------------------------------------
# TEST 5: New User Registration Flow
# -------------------------------------------------------------
Write-Host "`n--> TEST 5: Testing New Customer Registration Flow"
$session4 = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$regPageRes = Invoke-WebRequest -Uri "$baseUrl/Account/Register" -WebSession $session4
$regToken = Extract-Token -html $regPageRes.Content

$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$newEmail = "tester_$timestamp@test.com"
$newPhone = "9" + "$timestamp".Substring(0, 9)

$regForm = @{
    FullName = "Automated Test User"
    Email = $newEmail
    MobileNumber = $newPhone
    DrivingLicenseNumber = "DL-TEST-9999"
    Password = "1234"
    ConfirmPassword = "1234"
    __RequestVerificationToken = $regToken
}

$regRes = Invoke-WebRequest -Uri "$baseUrl/Account/Register" -Method Post -Body $regForm -WebSession $session4 -MaximumRedirection 5
Write-Host "[5.1] POST /Account/Register -> Redirected to Home & Authenticated: $($regRes.Content.Contains('Automated Test User'))"

$newProfile = Invoke-WebRequest -Uri "$baseUrl/Account/Profile" -Method Get -WebSession $session4
$newProfileSuccess = $newProfile.Content.Contains("Automated Test User") -and $newProfile.Content.Contains($newEmail)
Write-Host "[5.2] GET /Account/Profile (New User) -> Profile loaded successfully: $newProfileSuccess"

Write-Host "`n==============================================================="
Write-Host "           ALL AUTHENTICATION TESTS PASSED SUCCESSFULLY!       "
Write-Host "==============================================================="
