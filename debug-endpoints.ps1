# TimeFlow Backend - Debug Endpoint Issues
# This script helps identify why endpoints are returning 400 responses

$baseUrl = "http://localhost:5155"

Write-Host "🔍 Debugging TimeFlow Backend Endpoints..." -ForegroundColor Yellow
Write-Host "Base URL: $baseUrl" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if application is running
Write-Host "1. Checking if application is running..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger" -Method GET -ErrorAction Stop
    Write-Host "✅ Application is running - Swagger accessible" -ForegroundColor Green
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor White
} catch {
    Write-Host "❌ Application is not running or not accessible" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please start the application first with: dotnet run" -ForegroundColor Yellow
    exit
}

Write-Host ""

# Test 2: Test authentication endpoint without data
Write-Host "2. Testing authentication endpoint without data..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -Headers @{"Content-Type"="application/json"} -Body "{}" -ErrorAction Stop
    Write-Host "✅ Authentication endpoint accessible" -ForegroundColor Green
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor White
    Write-Host "   Response: $($response.Content)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Authentication endpoint error" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "   Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        Write-Host "   Response: $($_.Exception.Response.Content)" -ForegroundColor Red
    }
}

Write-Host ""

# Test 3: Test authentication endpoint with valid data
Write-Host "3. Testing authentication endpoint with valid data..." -ForegroundColor Green
try {
    $loginBody = @{
        email = "admin@timeflow.com"
        password = "Admin123!"
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -Headers @{"Content-Type"="application/json"} -Body $loginBody -ErrorAction Stop
    Write-Host "✅ Authentication successful" -ForegroundColor Green
    Write-Host "   Status: $($response.StatusCode)" -ForegroundColor White
    Write-Host "   Response: $($response.Content)" -ForegroundColor Gray
    
    # Parse response to get token
    $loginData = $response.Content | ConvertFrom-Json
    if ($loginData.IsSuccess -and $loginData.Data.AccessToken) {
        $token = $loginData.Data.AccessToken
        Write-Host "   Token obtained: $($token.Substring(0, 20))..." -ForegroundColor Green
        
        # Test 4: Test authenticated endpoint
        Write-Host ""
        Write-Host "4. Testing authenticated endpoint..." -ForegroundColor Green
        try {
            $authResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/me" -Method GET -Headers @{"Authorization"="Bearer $token"} -ErrorAction Stop
            Write-Host "✅ Authenticated endpoint working" -ForegroundColor Green
            Write-Host "   Status: $($authResponse.StatusCode)" -ForegroundColor White
            Write-Host "   Response: $($authResponse.Content)" -ForegroundColor Gray
        } catch {
            Write-Host "❌ Authenticated endpoint error" -ForegroundColor Red
            Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ No token in response" -ForegroundColor Red
        Write-Host "   Response structure: $($response.Content)" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Authentication failed" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "   Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        Write-Host "   Response: $($_.Exception.Response.Content)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Debug completed!" -ForegroundColor Green
