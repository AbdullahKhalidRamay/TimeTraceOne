# TimeFlow Backend - Endpoint Testing Script
# This script tests all API endpoints to ensure they return 200 success responses

$baseUrl = "http://localhost:5155"
$results = @()

Write-Host "🚀 Starting TimeFlow Backend Endpoint Testing..." -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Test 1: Authentication - Login
Write-Host "Testing Authentication Endpoints..." -ForegroundColor Cyan
try {
    $loginBody = @{
        email = "admin@timeflow.com"
        password = "Admin123!"
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -Headers @{"Content-Type"="application/json"} -Body $loginBody
    $token = ($response.Content | ConvertFrom-Json).Data.AccessToken
    
    $results += [PSCustomObject]@{
        Endpoint = "POST /api/auth/login"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Authentication successful, token obtained"
    }
    
    Write-Host "✅ POST /api/auth/login - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "POST /api/auth/login"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ POST /api/auth/login - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get Current User (requires authentication)
try {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/me" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/auth/me"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Current user retrieved successfully"
    }
    Write-Host "✅ GET /api/auth/me - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/auth/me"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/auth/me - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get Users (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/users"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Users list retrieved successfully"
    }
    Write-Host "✅ GET /api/users - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/users"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/users - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get Projects (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/projects" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/projects"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Projects list retrieved successfully"
    }
    Write-Host "✅ GET /api/projects - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/projects"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/projects - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Get Teams (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/teams" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/teams"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Teams list retrieved successfully"
    }
    Write-Host "✅ GET /api/teams - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/teams"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/teams - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Get Time Entries (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/time-entries" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/time-entries"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Time entries list retrieved successfully"
    }
    Write-Host "✅ GET /api/time-entries - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/time-entries"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/time-entries - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Get Notifications (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/notifications" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/notifications"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Notifications list retrieved successfully"
    }
    Write-Host "✅ GET /api/notifications - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/notifications"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/notifications - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 8: Get Reports (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/reports/system" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/reports/system"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "System report retrieved successfully"
    }
    Write-Host "✅ GET /api/reports/system - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/reports/system"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/reports/system - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 9: Get Validation (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/validation/users/00000000-0000-0000-0000-000000000000/available-hours" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/validation/users/{id}/available-hours"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "User available hours validation retrieved successfully"
    }
    Write-Host "✅ GET /api/validation/users/{id}/available-hours - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/validation/users/{id}/available-hours"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/validation/users/{id}/available-hours - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 10: Get Products (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/products" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/products"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Products list retrieved successfully"
    }
    Write-Host "✅ GET /api/products - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/products"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/products - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 11: Get Departments (requires authentication)
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/departments" -Method GET -Headers $headers
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/departments"
        StatusCode = $response.StatusCode
        Success = $response.StatusCode -eq 200
        Notes = "Departments list retrieved successfully"
    }
    Write-Host "✅ GET /api/departments - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    $results += [PSCustomObject]@{
        Endpoint = "GET /api/departments"
        StatusCode = "ERROR"
        Success = $false
        Notes = $_.Exception.Message
    }
    Write-Host "❌ GET /api/departments - ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Summary
Write-Host ""
Write-Host "📊 TESTING SUMMARY" -ForegroundColor Magenta
Write-Host "==================" -ForegroundColor Magenta

$totalTests = $results.Count
$successfulTests = ($results | Where-Object { $_.Success }).Count
$failedTests = $totalTests - $successfulTests

Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Successful: $successfulTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor Red

if ($failedTests -eq 0) {
    Write-Host ""
    Write-Host "🎉 ALL ENDPOINTS RETURNED 200 SUCCESS RESPONSES! 🎉" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "⚠️  Some endpoints failed. Check the details above." -ForegroundColor Yellow
}

# Display detailed results
Write-Host ""
Write-Host "Detailed Results:" -ForegroundColor Cyan
$results | Format-Table -AutoSize

# Export results to CSV
$results | Export-Csv -Path "endpoint-test-results.csv" -NoTypeInformation
Write-Host ""
Write-Host "Results exported to: endpoint-test-results.csv" -ForegroundColor Green
