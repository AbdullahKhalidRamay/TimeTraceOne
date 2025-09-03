# TimeFlow Backend Requirements - Part 1: Core API Endpoints

## Overview
This document contains the core API endpoints required to replace localStorage functionality in the TimeFlow application.

## Base Configuration
- **Base URL**: `https://api.timeflow.com/v1`
- **Authentication**: JWT with refresh tokens
- **Content-Type**: `application/json`
- **Rate Limiting**: 100 requests/minute per user

---

## 🔐 Authentication & User Management APIs

### **Authentication Endpoints**

#### Login
```http
POST /api/auth/login
Request Body:
{
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "success": true,
  "accessToken": "jwt_token_here",
  "refreshToken": "refresh_token_here",
  "user": {
    "id": "user_id",
    "name": "John Doe",
    "email": "user@example.com",
    "role": "manager",
    "jobTitle": "Project Manager",
    "availableHours": 8.0
  }
}
```

#### Refresh Token
```http
POST /api/auth/refresh
Request Body:
{
  "refreshToken": "refresh_token_here"
}

Response:
{
  "success": true,
  "accessToken": "new_jwt_token_here"
}
```

#### Logout
```http
POST /api/auth/logout
Request Body:
{
  "refreshToken": "refresh_token_here"
}

Response:
{
  "success": true,
  "message": "Logged out successfully"
}
```

#### Get Current User
```http
GET /api/auth/me
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "user": {
    "id": "user_id",
    "name": "John Doe",
    "email": "user@example.com",
    "role": "manager",
    "jobTitle": "Project Manager",
    "availableHours": 8.0,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

### **User Management Endpoints**

#### Get All Users
```http
GET /api/users
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - role?: string (filter by role)
  - departmentId?: string (filter by department)
  - teamId?: string (filter by team)
  - search?: string (search by name/email)
  - page?: number (pagination)
  - limit?: number (items per page)

Response:
{
  "success": true,
  "users": [
    {
      "id": "user_id",
      "name": "John Doe",
      "email": "user@example.com",
      "role": "manager",
      "jobTitle": "Project Manager",
      "availableHours": 8.0,
      "departmentIds": ["dept1", "dept2"],
      "teamIds": ["team1", "team2"],
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "totalPages": 3
  }
}
```

#### Get User by ID
```http
GET /api/users/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "user": {
    "id": "user_id",
    "name": "John Doe",
    "email": "user@example.com",
    "role": "manager",
    "jobTitle": "Project Manager",
    "availableHours": 8.0,
    "departmentIds": ["dept1", "dept2"],
    "teamIds": ["team1", "team2"],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Create User
```http
POST /api/users
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "Jane Smith",
  "email": "jane@example.com",
  "password": "password123",
  "role": "employee",
  "jobTitle": "Developer",
  "availableHours": 8.0,
  "departmentIds": ["dept1"],
  "teamIds": ["team1"]
}

Response:
{
  "success": true,
  "user": {
    "id": "new_user_id",
    "name": "Jane Smith",
    "email": "jane@example.com",
    "role": "employee",
    "jobTitle": "Developer",
    "availableHours": 8.0,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Update User
```http
PUT /api/users/:id
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "Jane Smith Updated",
  "jobTitle": "Senior Developer",
  "availableHours": 8.5
}

Response:
{
  "success": true,
  "user": {
    "id": "user_id",
    "name": "Jane Smith Updated",
    "jobTitle": "Senior Developer",
    "availableHours": 8.5,
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Delete User
```http
DELETE /api/users/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "User deleted successfully"
}
```

#### Get User Statistics
```http
GET /api/users/:id/stats
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "stats": {
    "totalEntries": 150,
    "approvedEntries": 140,
    "pendingEntries": 8,
    "rejectedEntries": 2,
    "totalActualHours": 1200.5,
    "totalBillableHours": 1100.0,
    "averageHoursPerDay": 8.0,
    "overtimeHours": 40.5,
    "approvalRate": 93.3
  }
}
```

---

## ⏰ Time Entry Management APIs (Core Feature)

### **Individual Time Entry Endpoints**

#### Get All Time Entries
```http
GET /api/time-entries
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - userId?: string (filter by user)
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)
  - status?: string[] (pending, approved, rejected)
  - projectId?: string
  - productId?: string
  - departmentId?: string
  - isBillable?: boolean
  - search?: string (search in task/description)
  - page?: number
  - limit?: number

Response:
{
  "success": true,
  "timeEntries": [
    {
      "id": "entry_id",
      "userId": "user_id",
      "userName": "John Doe",
      "date": "2024-01-15",
      "actualHours": 8.0,
      "billableHours": 7.5,
      "totalHours": 8.0,
      "availableHours": 8.0,
      "task": "Developed new feature",
      "projectDetails": {
        "category": "project",
        "name": "Mobile App",
        "task": "UI Development",
        "description": "Frontend implementation"
      },
      "isBillable": true,
      "status": "approved",
      "createdAt": "2024-01-15T09:00:00Z",
      "updatedAt": "2024-01-15T17:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 150,
    "totalPages": 8
  }
}
```

#### Get Time Entry by ID
```http
GET /api/time-entries/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "timeEntry": {
    "id": "entry_id",
    "userId": "user_id",
    "userName": "John Doe",
    "date": "2024-01-15",
    "actualHours": 8.0,
    "billableHours": 7.5,
    "totalHours": 8.0,
    "availableHours": 8.0,
    "task": "Developed new feature",
    "projectDetails": {
      "category": "project",
      "name": "Mobile App",
      "task": "UI Development",
      "description": "Frontend implementation"
    },
    "isBillable": true,
    "status": "approved",
    "createdAt": "2024-01-15T09:00:00Z",
    "updatedAt": "2024-01-15T17:00:00Z"
  }
}
```

#### Create Time Entry
```http
POST /api/time-entries
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "date": "2024-01-15",
  "actualHours": 8.0,
  "billableHours": 7.5,
  "task": "Developed new feature",
  "projectDetails": {
    "category": "project",
    "name": "Mobile App",
    "task": "UI Development",
    "description": "Frontend implementation"
  },
  "isBillable": true
}

Response:
{
  "success": true,
  "timeEntry": {
    "id": "new_entry_id",
    "userId": "user_id",
    "userName": "John Doe",
    "date": "2024-01-15",
    "actualHours": 8.0,
    "billableHours": 7.5,
    "totalHours": 8.0,
    "availableHours": 8.0,
    "task": "Developed new feature",
    "projectDetails": {
      "category": "project",
      "name": "Mobile App",
      "task": "UI Development",
      "description": "Frontend implementation"
    },
    "isBillable": true,
    "status": "pending",
    "createdAt": "2024-01-15T09:00:00Z",
    "updatedAt": "2024-01-15T09:00:00Z"
  }
}
```

#### Update Time Entry
```http
PUT /api/time-entries/:id
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "actualHours": 8.5,
  "billableHours": 8.0,
  "task": "Updated task description"
}

Response:
{
  "success": true,
  "timeEntry": {
    "id": "entry_id",
    "actualHours": 8.5,
    "billableHours": 8.0,
    "totalHours": 8.5,
    "task": "Updated task description",
    "updatedAt": "2024-01-15T18:00:00Z"
  }
}
```

#### Delete Time Entry
```http
DELETE /api/time-entries/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "Time entry deleted successfully"
}
```

---

## 🔐 Security Requirements

### **JWT Implementation**
```typescript
interface JWTPayload {
  userId: string;
  email: string;
  role: string;
  permissions: string[];
}

interface RefreshToken {
  token: string;
  userId: string;
  expiresAt: Date;
}
```

### **Role-Based Access Control**
```typescript
enum UserRole {
  OWNER = 'owner',
  MANAGER = 'manager',
  EMPLOYEE = 'employee'
}

interface Permission {
  resource: string;
  action: string;
  conditions?: object;
}
```

### **API Security**
- Rate limiting (100 requests per minute per user)
- Input validation and sanitization
- SQL injection prevention
- CORS configuration
- Helmet.js for security headers
