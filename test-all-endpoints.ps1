# TimeTraceOne API Endpoint Testing Script
# Tests all 76 endpoints to ensure they return 200/201 success responses

param(
    [string]$BaseUrl = "http://localhost:5155"
)

# Global variables
$global:authToken = $null
$global:results = @()
$global:existingIds = @{}

# Function to get existing IDs from the database
function Get-ExistingIds {
    Write-Host "🔍 Getting existing IDs from database..." -ForegroundColor Cyan
    
    try {
        # Get existing users
        $usersResponse = Invoke-WebRequest -Uri "$BaseUrl/api/users" -Method GET -Headers @{"Authorization"="Bearer $global:authToken"} -ErrorAction Stop
        if ($usersResponse.StatusCode -eq 200) {
            $users = $usersResponse.Content | ConvertFrom-Json
            if ($users.Data -and $users.Data.Count -gt 0) {
                $global:existingIds.Users = $users.Data | Select-Object -First 3
                Write-Host "✅ Found $($global:existingIds.Users.Count) users" -ForegroundColor Green
            }
        }
        
        # Get existing projects
        $projectsResponse = Invoke-WebRequest -Uri "$BaseUrl/api/projects" -Method GET -Headers @{"Authorization"="Bearer $global:authToken"} -ErrorAction Stop
        if ($projectsResponse.StatusCode -eq 200) {
            $projects = $projectsResponse.Content | ConvertFrom-Json
            if ($projects.Data -and $projects.Data.Count -gt 0) {
                $global:existingIds.Projects = $projects.Data | Select-Object -First 3
                Write-Host "✅ Found $($global:existingIds.Projects.Count) projects" -ForegroundColor Green
            }
        }
        
        # Get existing products
        $productsResponse = Invoke-WebRequest -Uri "$BaseUrl/api/products" -Method GET -Headers @{"Authorization"="Bearer $global:authToken"} -ErrorAction Stop
        if ($productsResponse.StatusCode -eq 200) {
            $products = $productsResponse.Content | ConvertFrom-Json
            if ($products.Data -and $products.Data.Count -gt 0) {
                $global:existingIds.Products = $products.Data | Select-Object -First 3
                Write-Host "✅ Found $($global:existingIds.Products.Count) products" -ForegroundColor Green
            }
        }
        
        # Get existing teams
        $teamsResponse = Invoke-WebRequest -Uri "$BaseUrl/api/teams" -Method GET -Headers @{"Authorization"="Bearer $global:authToken"} -ErrorAction Stop
        if ($teamsResponse.StatusCode -eq 200) {
            $teams = $teamsResponse.Content | ConvertFrom-Json
            if ($teams.Data -and $teams.Data.Count -gt 0) {
                $global:existingIds.Teams = $teams.Data | Select-Object -First 3
                Write-Host "✅ Found $($global:existingIds.Teams.Count) teams" -ForegroundColor Green
            }
        }
        
        # Get existing time entries
        $timeEntriesResponse = Invoke-WebRequest -Uri "$BaseUrl/api/timeentries" -Method GET -Headers @{"Authorization"="Bearer $global:authToken"} -ErrorAction Stop
        if ($timeEntriesResponse.StatusCode -eq 200) {
            $timeEntries = $timeEntriesResponse.Content | ConvertFrom-Json
            if ($timeEntries.Data -and $timeEntries.Data.Count -gt 0) {
                $global:existingIds.TimeEntries = $timeEntries.Data | Select-Object -First 3
                Write-Host "✅ Found $($global:existingIds.TimeEntries.Count) time entries" -ForegroundColor Green
            }
        }
        
        Write-Host "✅ Database ID discovery completed!" -ForegroundColor Green
        
    } catch {
        Write-Host "❌ Error getting existing IDs: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Function to get a safe ID for testing
function Get-SafeId {
    param(
        [string]$EntityType,
        [string]$FallbackId = "00000000-0000-0000-0000-000000000001"
    )
    
    if ($global:existingIds.$EntityType -and $global:existingIds.$EntityType.Count -gt 0) {
        return $global:existingIds.$EntityType[0].Id
    }
    return $FallbackId
}

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
            Success = $response.StatusCode -eq 200 -or $response.StatusCode -eq 201
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
    
    # Get existing IDs from database
    Get-ExistingIds
    
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
    Test-Endpoint -Name "Get User by ID" -Method "GET" -Url "/api/users/$(Get-SafeId 'Users')" -RequiresAuth $true
    Test-Endpoint -Name "Create User" -Method "POST" -Url "/api/users" -Body '{"name":"Test User 13","email":"testuser13@uniqueexample.com","password":"Test123!","role":"Employee","jobTitle":"Developer","availableHours":8.0}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update User" -Method "PUT" -Url "/api/users/$(Get-SafeId 'Users')" -Body '{"name":"Updated User 2","jobTitle":"Senior Developer"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete User" -Method "DELETE" -Url "/api/users/$(Get-SafeId 'Users')" -RequiresAuth $true
    Test-Endpoint -Name "Get User Statistics" -Method "GET" -Url "/api/users/$(Get-SafeId 'Users')/statistics" -RequiresAuth $true
    Test-Endpoint -Name "Get User Reports" -Method "GET" -Url "/api/users/$(Get-SafeId 'Users')/reports?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get User Associated Projects" -Method "GET" -Url "/api/users/$(Get-SafeId 'Users')/projects" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 3: Time Entry Management Endpoints
    Write-Host "Testing Time Entry Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Time Entries" -Method "GET" -Url "/api/timeentries" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entry by ID" -Method "GET" -Url "/api/timeentries/$(Get-SafeId 'TimeEntries')" -RequiresAuth $true
    Test-Endpoint -Name "Create Time Entry" -Method "POST" -Url "/api/timeentries" -Body '{"date":"2024-01-01","actualHours":8.0,"billableHours":8.0,"task":"Development work 7","projectDetails":{"category":"Development","name":"Feature Development 7","task":"Development work 7","description":"Development work on mobile app 7"},"isBillable":true}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Time Entry" -Method "PUT" -Url "/api/timeentries/$(Get-SafeId 'TimeEntries')" -Body '{"actualHours":7.5,"billableHours":7.5,"task":"Updated task 2","projectDetails":{"category":"Development","name":"Updated Feature 2","task":"Updated task 2","description":"Updated description 2"}}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete Time Entry" -Method "DELETE" -Url "/api/timeentries/$(Get-SafeId 'TimeEntries')" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by Date" -Method "GET" -Url "/api/timeentries/date/2024-01-01" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by User" -Method "GET" -Url "/api/timeentries/user/$(Get-SafeId 'Users')" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by Project" -Method "GET" -Url "/api/timeentries/project/$(Get-SafeId 'Projects')" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entries by Date Range" -Method "GET" -Url "/api/timeentries/range?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Time Entry Status" -Method "GET" -Url "/api/timeentries/status/2024-01-01" -RequiresAuth $true
    Test-Endpoint -Name "Search Time Entries" -Method "GET" -Url "/api/timeentries/search?q=development" -RequiresAuth $true
    Test-Endpoint -Name "Filter Time Entries" -Method "GET" -Url "/api/timeentries/filter?startDate=2024-01-01`&endDate=2024-01-31`&projectId=$(Get-SafeId 'Projects')" -RequiresAuth $true
    Test-Endpoint -Name "Update Weekly Time Entries" -Method "PUT" -Url "/api/timeentries/weekly/2024-01-01" -Body '{"entries":[{"id":"$(Get-SafeId 'TimeEntries')","actualHours":8.0,"billableHours":8.0,"task":"Updated weekly task"}]}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    
    Write-Host ""
    
    # Test 4: Project Management Endpoints
    Write-Host "Testing Project Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Projects" -Method "GET" -Url "/api/projects" -RequiresAuth $true
    Test-Endpoint -Name "Get Project by ID" -Method "GET" -Url "/api/projects/$(Get-SafeId 'Projects')" -RequiresAuth $true
    Test-Endpoint -Name "Create Project" -Method "POST" -Url "/api/projects" -Body '{"name":"Test Project 8","description":"A test project","projectType":"TimeAndMaterial","isBillable":true,"status":"Active","clientName":"Test Client","clientEmail":"client@test.com"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Project" -Method "PUT" -Url "/api/projects/$(Get-SafeId 'Projects')" -Body '{"name":"Updated Project 2","description":"Updated description 2"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete Project" -Method "DELETE" -Url "/api/projects/$(Get-SafeId 'Projects')" -RequiresAuth $true
    Test-Endpoint -Name "Get Project Statistics" -Method "GET" -Url "/api/projects/$(Get-SafeId 'Projects')/statistics" -RequiresAuth $true
    Test-Endpoint -Name "Get Project Team Members" -Method "GET" -Url "/api/projects/$(Get-SafeId 'Projects')/team" -RequiresAuth $true
    Test-Endpoint -Name "Add Team to Project" -Method "POST" -Url "/api/projects/$(Get-SafeId 'Projects')/teams" -Body '{"teamId":"$(Get-SafeId 'Teams')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Remove Team from Project" -Method "DELETE" -Url "/api/projects/$(Get-SafeId 'Projects')/teams/$(Get-SafeId 'Teams')" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 5: Product Management Endpoints
    Write-Host "Testing Product Management Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Products" -Method "GET" -Url "/api/products" -RequiresAuth $true
    Test-Endpoint -Name "Get Product by ID" -Method "GET" -Url "/api/products/$(Get-SafeId 'Products')" -RequiresAuth $true
    Test-Endpoint -Name "Create Product" -Method "POST" -Url "/api/products" -Body '{"name":"Test Product 8","productDescription":"A test product","isBillable":true}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Product" -Method "PUT" -Url "/api/products/$(Get-SafeId 'Products')" -Body '{"name":"Updated Product 2","productDescription":"Updated description 2"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete Product" -Method "DELETE" -Url "/api/products/$(Get-SafeId 'Products')" -RequiresAuth $true
    Test-Endpoint -Name "Get Product Statistics" -Method "GET" -Url "/api/products/$(Get-SafeId 'Products')/statistics" -RequiresAuth $true
    
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
    Test-Endpoint -Name "Get Team by ID" -Method "GET" -Url "/api/teams/$(Get-SafeId 'Teams')" -RequiresAuth $true
    Test-Endpoint -Name "Create Team" -Method "POST" -Url "/api/teams" -Body '{"name":"Test Team 8","description":"A test team","departmentId":"00000000-0000-0000-0000-000000000005","leaderId":"$(Get-SafeId 'Users')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Update Team" -Method "PUT" -Url "/api/teams/$(Get-SafeId 'Teams')" -Body '{"name":"Updated Team 2","description":"Updated description 2"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Delete Team" -Method "DELETE" -Url "/api/teams/$(Get-SafeId 'Teams')" -RequiresAuth $true
    Test-Endpoint -Name "Add Team Member" -Method "POST" -Url "/api/teams/$(Get-SafeId 'Teams')/members" -Body '{"userId":"$(Get-SafeId 'Users')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Remove Team Member" -Method "DELETE" -Url "/api/teams/$(Get-SafeId 'Teams')/members/$(Get-SafeId 'Users')" -RequiresAuth $true
    Test-Endpoint -Name "Update Team Leader" -Method "PUT" -Url "/api/teams/$(Get-SafeId 'Teams')/leader" -Body '{"leaderId":"$(Get-SafeId 'Users')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Associate Team Project" -Method "POST" -Url "/api/teams/$(Get-SafeId 'Teams')/projects" -Body '{"projectId":"$(Get-SafeId 'Projects')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Associate Team Product" -Method "POST" -Url "/api/teams/$(Get-SafeId 'Teams')/products" -Body '{"productId":"$(Get-SafeId 'Products')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Associate Team Department" -Method "POST" -Url "/api/teams/$(Get-SafeId 'Teams')/departments" -Body '{"departmentId":"00000000-0000-0000-0000-000000000005"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    
    Write-Host ""
    
    # Test 8: Reports and Analytics Endpoints
    Write-Host "Testing Reports and Analytics Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get User Reports" -Method "GET" -Url "/api/reports/users?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Team Reports" -Method "GET" -Url "/api/reports/teams?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get System Reports" -Method "GET" -Url "/api/reports/system?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Department Performance" -Method "GET" -Url "/api/reports/departments/00000000-0000-0000-0000-000000000005?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Project Performance" -Method "GET" -Url "/api/reports/projects/$(Get-SafeId 'Projects')?startDate=2024-01-01`&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Export CSV Report" -Method "GET" -Url "/api/reports/export/csv?startDate=2024-01-01`&endDate=2024-01-31`&type=users" -RequiresAuth $true
    Test-Endpoint -Name "Export PDF Report" -Method "GET" -Url "/api/reports/export/pdf?startDate=2024-01-01&endDate=2024-01-31&type=users" -RequiresAuth $true
    Test-Endpoint -Name "Search Time Entries" -Method "GET" -Url "/api/reports/search?query=development&startDate=2024-01-01&endDate=2024-01-31" -RequiresAuth $true
    Test-Endpoint -Name "Get Top Projects for Department" -Method "GET" -Url "/api/reports/departments/00000000-0000-0000-0000-000000000005/top-projects?startDate=2024-01-01&endDate=2024-01-31" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 9: Notification System Endpoints
    Write-Host "Testing Notification System Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Get All Notifications" -Method "GET" -Url "/api/notifications" -RequiresAuth $true
    Test-Endpoint -Name "Get Notification by ID" -Method "GET" -Url "/api/notifications/00000000-0000-0000-0000-000000000014" -RequiresAuth $true
    Test-Endpoint -Name "Create Notification" -Method "POST" -Url "/api/notifications" -Body '{"userId":"$(Get-SafeId 'Users')","title":"Test Notification 8","message":"This is a test notification","type":"System","relatedEntryId":"$(Get-SafeId 'TimeEntries')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Mark Notification as Read" -Method "PUT" -Url "/api/notifications/00000000-0000-0000-0000-000000000014/read" -RequiresAuth $true
    Test-Endpoint -Name "Mark All Notifications as Read" -Method "PUT" -Url "/api/notifications/read-all" -RequiresAuth $true
    Test-Endpoint -Name "Delete Notification" -Method "DELETE" -Url "/api/notifications/00000000-0000-0000-0000-000000000014" -RequiresAuth $true
    
    Write-Host ""
    
    # Test 10: Validation and Utility Endpoints
    Write-Host "Testing Validation and Utility Endpoints..." -ForegroundColor Cyan
    Test-Endpoint -Name "Validate Time Entry" -Method "POST" -Url "/api/validation/time-entry" -Body '{"date":"2024-01-01","actualHours":8.0,"userId":"$(Get-SafeId 'Users')","projectId":"$(Get-SafeId 'Projects')"}' -Headers @{"Content-Type"="application/json"} -RequiresAuth $true
    Test-Endpoint -Name "Get User Available Hours" -Method "GET" -Url "/api/validation/available-hours/$(Get-SafeId 'Users')?date=2024-01-01" -RequiresAuth $true
    Test-Endpoint -Name "Validate User Access" -Method "GET" -Url "/api/validation/user-access/$(Get-SafeId 'Users')" -RequiresAuth $true
    Test-Endpoint -Name "Validate Team Access" -Method "GET" -Url "/api/validation/team-access/$(Get-SafeId 'Users')" -RequiresAuth $true
    Test-Endpoint -Name "Validate Project Access" -Method "GET" -Url "/api/validation/project-access/$(Get-SafeId 'Users')" -RequiresAuth $true
    
    Write-Host ""
    
    # Summary Report
    Write-Host "TESTING SUMMARY" -ForegroundColor Magenta
    Write-Host "==================" -ForegroundColor Magenta
    
    $totalTests = $global:results.Count
    $successfulTests = ($global:results | Where-Object { $_.Success }).Count
    $failedTests = ($global:results | Where-Object { -not $_.Success }).Count
    
    Write-Host "Total Endpoints Tested: $totalTests" -ForegroundColor White
    Write-Host "Successful (200/201): $successfulTests" -ForegroundColor Green
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
