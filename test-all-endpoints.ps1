# TimeTraceOne API Endpoint Testing Script
# Tests all 67 endpoints to ensure they return 200 success responses

param(
    [string]$BaseUrl = "http://localhost:5155"
)

# Global variables
$global:authToken = $null
$global:results = @()

# Function to test an endpoint
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [hashtable]$Headers = @{},
        [bool]$RequiresAuth = $false
    )
    
    $fullUrl = "$BaseUrl$Url"
    $requestHeaders = $Headers.Clone()
    
    if ($RequiresAuth -and $global:authToken) {
        $requestHeaders["Authorization"] = "Bearer $global:authToken"
    }
    
    try {
        $params = @{
            Uri = $fullUrl
            Method = $Method
            Headers = $requestHeaders
            TimeoutSec = 30
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-WebRequest @params -ErrorAction Stop
        
        $result = @{
            Name = $Name
            Method = $Method
            Url = $Url
            StatusCode = $response.StatusCode
            Success = $response.StatusCode -eq 200
            Error = $null
            ResponseTime = $response.BaseResponse.ResponseTime
        }
        
        Write-Host "✅ $Name - $Method $Url - Status: $($response.StatusCode)" -ForegroundColor Green
        
    } catch {
        $result = @{
            Name = $Name
            Method = $Method
            Url = $Url
            StatusCode = $null
            Success = $false
            Error = $_.Exception.Message
            ResponseTime = $null
        }
        
        Write-Host "❌ $Name - $Method $Url - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    $global:results += $result
}

# Function to authenticate and get token
function Get-AuthToken {
    Write-Host "Authenticating..." -ForegroundColor Cyan
    
    try {
        $loginBody = @{
            email = "admin@timeflow.com"
            password = "Admin123!"
        } | ConvertTo-Json
        
        $response = Invoke-WebRequest -Uri "$BaseUrl/api/auth/login" -Method POST -Headers @{"Content-Type"="application/json"} -Body $loginBody -ErrorAction Stop
        
        if ($response.StatusCode -eq 200) {
            $loginData = $response.Content | ConvertFrom-Json
            $global:authToken = $loginData.Data.AccessToken
            Write-Host "✅ Authentication successful!" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Authentication failed with status: $($response.StatusCode)" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Authentication error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main testing function
function Start-EndpointTesting {
    Write-Host "Starting TimeTraceOne API Endpoint Testing..." -ForegroundColor Magenta
    Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Magenta
    
    # Authenticate first
    if (-not (Get-AuthToken)) {
        Write-Host "❌ Cannot proceed without authentication!" -ForegroundColor Red
        return
    }
    
    Write-Host ""
    
    # Test 1: Authentication Endpoints
    Write-Host "Testing Authentication Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Login" -Method "POST" -Url "/api/auth/login" -Body '{"email":"admin@timeflow.com","password":"Admin123!"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $false
    Test-Endpoint -Name "Get Current User" -Method "GET" -Url "/api/auth/me" -RequiresAuth $true
    Test-Endpoint -Name "Refresh Token" -Method "POST" -Url "/api/auth/refresh" -Body '{"refreshToken":"test-refresh-token"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $false
    Test-Endpoint -Name "Logout" -Method "POST" -Url "/api/auth/logout" -Body '{"refreshToken":"test-refresh-token"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    
    Write-Host ""
    
    # Test 2: User Management Endpoints
    Write-Host "Testing User Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Users" -Method "GET" -Url "/api/users" -RequiresAuth $true
    Test-Endpoint -Name "Get User by ID" -Method "GET" -Url "/api/users/00000000-0000-0000-0000-000000000001" -RequiresAuth $true
    Test-Endpoint -Name "Create User" -Method "POST" -Url "/api/users" -Body '{"name":"Test User 13","email":"testuser13@uniqueexample.com","password":"Test123!","role":"Employee","jobTitle":"Developer","availableHours":8.0}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update User" -Method "PUT" -Url "/api/users/00000000-0000-0000-0000-000000000001" -Body '{"name":"Updated User 2","jobTitle":"Senior Developer"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete User" -Method "DELETE" -Url "/api/users/d195464a-d2f8-448e-9669-29549db22179" -RequiresAuth $true
    Test-Endpoint -Name "Get User Statistics" -Method "GET" -Url "/api/users/00000000-0000-0000-0000-000000000001/statistics" -RequiresAuth $true
    Test-Endpoint -Name "Get User Reports" -Method "GET" -Url "/api/users/00000000-0000-0000-0000-000000000001/reports?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get User Associated Projects" -Method "GET" -Url "/api/users/00000000-0000-0000-0000-000000000001/projects" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 3: Time Entry Management Endpoints
    Write-Host "Testing Time Entry Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Time Entries" -Method "GET" -Url "/api/timeentries" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entry by ID" -Method "GET" -Url "/api/timeentries/00000000-0000-0000-0000-000000000012" -RequiresAuth $true
    Test-Endpoint -Name "Create Time Entry" -Method "POST" -Url "/api/timeentries" -Body '{"date":"2024-01-01","actualHours":8.0,"billableHours":8.0,"task":"Development work 7","projectDetails":{"category":"Development","name":"Feature Development 7","task":"Development work 7","description":"Development work on mobile app 7"},"isBillable":true}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Time Entry" -Method "PUT" -Url "/api/timeentries/00000000-0000-0000-0000-000000000012" -Body '{"actualHours":7.5,"billableHours":7.5,"task":"Updated task 2","projectDetails":{"category":"Development","name":"Updated Feature 2","task":"Updated task 2","description":"Updated description 2"}}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete Time Entry" -Method "DELETE" -Url "/api/timeentries/00000000-0000-0000-0000-000000000012" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by Date" -Method "GET" -Url "/api/timeentries/date/2024-01-01" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by User" -Method "GET" -Url "/api/timeentries/user/00000000-0000-0000-0000-000000000004" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by Project" -Method "GET" -Url "/api/timeentries/project/f01c9e30-727a-436e-9113-17d6329c46f6" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by Date Range" -Method "GET" -Url "/api/timeentries/range?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entry Status" -Method "GET" -Url "/api/timeentries/status/2024-01-01" -RequiresAuth $true
    Test-Endpoint -Name "Search Time Entries" -Method "GET" -Url "/api/timeentries/search?q=development" -RequiresAuth $true
    Test-Endpoint -Name "Filter Time Entries" -Method "GET" -Url "/api/timeentries/filter?startDate=2024-01-01`&endDate=2024-01-31`&projectId=f01c9e30-727a-436e-9113-17d6329c46f6" -RequiresAuth $true
    Test-Endpoint -Name "Update Weekly Time Entries" -Method "PUT" -Url "/api/timeentries/weekly/2024-01-01" -Body '{"entries":[{"id":"00000000-0000-0000-0000-000000000012","actualHours":8.0,"billableHours":8.0,"task":"Updated weekly task"}]}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    
    Write-Host ""
    
    # Test 4: Project Management Endpoints
    Write-Host "Testing Project Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Projects" -Method "GET" -Url "/api/projects" -RequiresAuth $true
    Test-Endpoint -Name "Get Project by ID" -Method "GET" -Url "/api/projects/f01c9e30-727a-436e-9113-17d6329c46f6" -RequiresAuth $true
    Test-Endpoint -Name "Create Project" -Method "POST" -Url "/api/projects" -Body '{"name":"Test Project 8","description":"A test project","projectType":"TimeAndMaterial","isBillable":true,"status":"Active","clientName":"Test Client","clientEmail":"client@test.com"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Project" -Method "PUT" -Url "/api/projects/f01c9e30-727a-436e-9113-17d6329c46f6" -Body '{"name":"Updated Project 2","description":"Updated description 2"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
Test-Endpoint -Name "Delete Project" -Method "DELETE" -Url "/api/projects/f01c9e30-727a-436e-9113-17d6329c46f6" -RequiresAuth $true
Test-Endpoint -Name "Get Project Statistics" -Method "GET" -Url "/api/projects/f01c9e30-727a-436e-9113-17d6329c46f6/statistics" -RequiresAuth $true
Test-Endpoint -Name "Get Project Team Members" -Method "GET" -Url "/api/projects/f01c9e30-727a-436e-9113-17d6329c46f6/team" -RequiresAuth $true
Test-Endpoint -Name "Add Team to Project" -Method "POST" -Url "/api/projects/f01c9e30-727a-436e-9113-17d6329c46f6/teams" -Body '{"teamId":"72a35a9f-8433-41af-bc65-047433ec9e49"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
Test-Endpoint -Name "Remove Team from Project" -Method "DELETE" -Url "/api/projects/f01c9e30-727a-436e-9113-17d6329c46f6/teams/72a35a9f-8433-41af-bc65-047433ec9e49" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 5: Product Management Endpoints
    Write-Host "Testing Product Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Products" -Method "GET" -Url "/api/products" -RequiresAuth $true
    Test-Endpoint -Name "Get Product by ID" -Method "GET" -Url "/api/products/234e02d9-6675-4725-a9b1-11536a112f65" -RequiresAuth $true
    Test-Endpoint -Name "Create Product" -Method "POST" -Url "/api/products" -Body '{"name":"Test Product 8","productDescription":"A test product","isBillable":true}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Product" -Method "PUT" -Url "/api/products/234e02d9-6675-4725-a9b1-11536a112f65" -Body '{"name":"Updated Product 2","productDescription":"Updated description 2"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete Product" -Method "DELETE" -Url "/api/products/234e02d9-6675-4725-a9b1-11536a112f65" -RequiresAuth $true
    Test-Endpoint -Name "Get Product Statistics" -Method "GET" -Url "/api/products/871e9cd3-17d1-4d63-8b3b-153a498d990a/statistics" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 6: Department Management Endpoints
    Write-Host "Testing Department Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Departments" -Method "GET" -Url "/api/departments" -RequiresAuth $true
    Test-Endpoint -Name "Get Department by ID" -Method "GET" -Url "/api/departments/00000000-0000-0000-0000-000000000005" -RequiresAuth $true
    Test-Endpoint -Name "Create Department" -Method "POST" -Url "/api/departments" -Body '{"name":"Test Department 8","departmentDescription":"A test department","isBillable":true}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Department" -Method "PUT" -Url "/api/departments/00000000-0000-0000-0000-000000000005" -Body '{"name":"Updated Department 2","departmentDescription":"Updated description 2"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete Department" -Method "DELETE" -Url "/api/departments/00000000-0000-0000-0000-000000000005" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 7: Team Management Endpoints
    Write-Host "Testing Team Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Teams" -Method "GET" -Url "/api/teams" -RequiresAuth $true
    Test-Endpoint -Name "Get Team by ID" -Method "GET" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49" -RequiresAuth $true
    Test-Endpoint -Name "Create Team" -Method "POST" -Url "/api/teams" -Body '{"name":"Test Team 8","description":"A test team","departmentId":"00000000-0000-0000-0000-000000000005","leaderId":"00000000-0000-0000-0000-000000000003"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Team" -Method "PUT" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49" -Body '{"name":"Updated Team 2","description":"Updated description 2"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
Test-Endpoint -Name "Delete Team" -Method "DELETE" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49" -RequiresAuth $true
Test-Endpoint -Name "Add Team Member" -Method "POST" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49/members" -Body '{"userId":"00000000-0000-0000-0000-000000000004"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
Test-Endpoint -Name "Remove Team Member" -Method "DELETE" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49/members/00000000-0000-0000-0000-000000000004" -RequiresAuth $true
Test-Endpoint -Name "Update Team Leader" -Method "PUT" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49/leader" -Body '{"leaderId":"00000000-0000-0000-0000-000000000003"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
Test-Endpoint -Name "Associate Team Project" -Method "POST" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49/projects" -Body '{"projectId":"f01c9e30-727a-436e-9113-17d6329c46f6"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
Test-Endpoint -Name "Associate Team Product" -Method "POST" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49/products" -Body '{"productId":"234e02d9-6675-4725-a9b1-11536a112f65"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
Test-Endpoint -Name "Associate Team Department" -Method "POST" -Url "/api/teams/72a35a9f-8433-41af-bc65-047433ec9e49/departments" -Body '{"departmentId":"00000000-0000-0000-0000-000000000005"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    
    Write-Host ""
    
    # Test 8: Reports and Analytics Endpoints
    Write-Host "Testing Reports and Analytics Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get User Reports" -Method "GET" -Url "/api/reports/users?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Team Reports" -Method "GET" -Url "/api/reports/teams?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get System Reports" -Method "GET" -Url "/api/reports/system?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Department Performance" -Method "GET" -Url "/api/reports/departments/00000000-0000-0000-0000-000000000005?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Project Performance" -Method "GET" -Url "/api/reports/projects/f01c9e30-727a-436e-9113-17d6329c46f6?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Export CSV Report" -Method "GET" -Url "/api/reports/export/csv?startDate=2024-01-01`&endDate=2024-01-31`&type=users" -RequiresAuth $true
    Test-Endpoint -Name "Export PDF Report" -Method "GET" -Url "/api/reports/export/pdf?startDate=2024-01-01`&endDate=2024-01-31`&type=users" -RequiresAuth $true
    Test-Endpoint -Name "Search Time Entries" -Method "GET" -Url "/api/reports/search?query=development`&startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Top Projects for Department" -Method "GET" -Url "/api/reports/departments/00000000-0000-0000-0000-000000000005/top-projects?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 9: Notification System Endpoints
    Write-Host "Testing Notification System Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Notifications" -Method "GET" -Url "/api/notifications" -RequiresAuth $true
    Test-Endpoint -Name "Get Notification by ID" -Method "GET" -Url "/api/notifications/00000000-0000-0000-0000-000000000014" -RequiresAuth $true
    Test-Endpoint -Name "Create Notification" -Method "POST" -Url "/api/notifications" -Body '{"userId":"00000000-0000-0000-0000-000000000001","title":"Test Notification 8","message":"This is a test notification","type":"System","relatedEntryId":"00000000-0000-0000-0000-000000000012"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Mark Notification as Read" -Method "PUT" -Url "/api/notifications/00000000-0000-0000-0000-000000000014/read" -RequiresAuth $true
    Test-Endpoint -Name "Mark All Notifications as Read" -Method "PUT" -Url "/api/notifications/read-all" -RequiresAuth $true
    Test-Endpoint -Name "Delete Notification" -Method "DELETE" -Url "/api/notifications/00000000-0000-0000-0000-000000000014" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 10: Validation and Utility Endpoints
    Write-Host "Testing Validation and Utility Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Validate Time Entry" -Method "POST" -Url "/api/validation/time-entry" -Body '{"date":"2024-01-01","actualHours":8.0,"userId":"00000000-0000-0000-0000-000000000001","projectId":"00000000-0000-0000-0000-000000000008"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Get User Available Hours" -Method "GET" -Url "/api/validation/available-hours/00000000-0000-0000-0000-000000000001?date=2024-01-01" -RequiresAuth $true
    Test-Endpoint -Name "Validate User Access" -Method "GET" -Url "/api/validation/user-access/00000000-0000-0000-0000-000000000001" -RequiresAuth $true
    Test-Endpoint -Name "Validate Team Access" -Method "GET" -Url "/api/validation/team-access/00000000-0000-0000-0000-000000000001" -RequiresAuth $true
    Test-Endpoint -Name "Validate Project Access" -Method "GET" -Url "/api/validation/project-access/00000000-0000-0000-0000-000000000001" -RequiresAuth $true
    
    Write-Host ""
    
    # Summary Report
    Write-Host "TESTING SUMMARY" -ForegroundColor Magenta
    Write-Host "==================" -ForegroundColor Magenta
    
    $totalTests = $global:results.Count
    $successfulTests = ($global:results | Where-Object { $_.Success }).Count
    $failedTests = ($global:results | Where-Object { -not $_.Success }).Count
    
    Write-Host "Total Endpoints Tested: $totalTests" -ForegroundColor White
    Write-Host "Successful (200): $successfulTests" -ForegroundColor Green
    Write-Host "Failed/Errors: $failedTests" -ForegroundColor Red
    Write-Host "Success Rate: $([math]::Round(($successfulTests / $totalTests) * 100, 2))%" -ForegroundColor Cyan
    
    Write-Host ""
    
    if ($failedTests -gt 0) {
        Write-Host "FAILED ENDPOINTS:" -ForegroundColor Red
        Write-Host "===================" -ForegroundColor Red
        $global:results | Where-Object { -not $_.Success } | ForEach-Object {
            Write-Host "• $($_.Name) - $($_.Method) $($_.Url)" -ForegroundColor Red
            if ($_.Error) {
                Write-Host "  Error: $($_.Error)" -ForegroundColor Red
            }
            Write-Host ""
        }
    }
    
    Write-Host "SUCCESSFUL ENDPOINTS:" -ForegroundColor Green
    Write-Host "=======================" -ForegroundColor Green
    $global:results | Where-Object { $_.Success } | ForEach-Object {
        Write-Host "• $($_.Name) - $($_.Method) $($_.Url) - Status: $($_.StatusCode)" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Testing completed!" -ForegroundColor Green
    
    # Export results to CSV
    $global:results | Export-Csv -Path "endpoint-test-results.csv" -NoTypeInformation
    Write-Host "Results exported to: endpoint-test-results.csv" -ForegroundColor Yellow
}

# Start the testing
Start-EndpointTesting
