# TimeFlow Backend Requirements - Part 2: Weekly Bulk Operations & Project Management

## Overview
This document contains the weekly bulk time entry operations and project management APIs required for the TimeFlow application.

---

## ⏰ Weekly Bulk Time Entry APIs (Core Feature)

### **Weekly Bulk Operations**

#### Create Weekly Bulk Time Entries
```http
POST /api/time-entries/weekly-bulk
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "weekStart": "2024-01-15", // Monday of the week
  "entries": [
    {
      "projectId": "project_1",
      "productId": null,
      "departmentId": null,
      "dailyHours": {
        "2024-01-15": {
          "actualHours": 8.0,
          "billableHours": 7.5,
          "task": "Monday development work"
        },
        "2024-01-16": {
          "actualHours": 8.0,
          "billableHours": 8.0,
          "task": "Tuesday development work"
        }
      }
    },
    {
      "projectId": null,
      "productId": "product_1",
      "departmentId": null,
      "dailyHours": {
        "2024-01-15": {
          "actualHours": 2.0,
          "billableHours": 2.0,
          "task": "Product testing"
        }
      }
    }
  ]
}

Response:
{
  "success": true,
  "message": "Weekly time entries created successfully",
  "createdEntries": 15,
  "skippedEntries": 2,
  "weekStart": "2024-01-15",
  "weekEnd": "2024-01-21"
}
```

#### Get Weekly Time Entries
```http
GET /api/time-entries/weekly/:weekStart
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - userId?: string (filter by user)

Response:
{
  "success": true,
  "weekStart": "2024-01-15",
  "weekEnd": "2024-01-21",
  "entries": [
    {
      "date": "2024-01-15",
      "entries": [
        {
          "id": "entry_1",
          "projectDetails": {
            "category": "project",
            "name": "Mobile App"
          },
          "actualHours": 8.0,
          "billableHours": 7.5,
          "task": "Development work",
          "status": "pending"
        }
      ],
      "totalActualHours": 8.0,
      "totalBillableHours": 7.5
    }
  ],
  "weeklySummary": {
    "totalActualHours": 40.0,
    "totalBillableHours": 38.5,
    "totalEntries": 20,
    "pendingEntries": 15,
    "approvedEntries": 5
  }
}
```

#### Update Weekly Time Entries
```http
PUT /api/time-entries/weekly/:weekStart
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "entries": [
    {
      "id": "existing_entry_id",
      "actualHours": 8.5,
      "billableHours": 8.0,
      "task": "Updated task"
    }
  ]
}

Response:
{
  "success": true,
  "message": "Weekly time entries updated successfully",
  "updatedEntries": 5
}
```

### **Time Entry Status Management**

#### Approve Time Entry
```http
PUT /api/time-entries/:id/approve
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "message": "Approved - Good work on this task",
  "approvedBy": "manager_id"
}

Response:
{
  "success": true,
  "message": "Time entry approved successfully",
  "timeEntry": {
    "id": "entry_id",
    "status": "approved",
    "updatedAt": "2024-01-15T18:00:00Z"
  }
}
```

#### Reject Time Entry
```http
PUT /api/time-entries/:id/reject
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "message": "Please provide more details about the work done",
  "rejectedBy": "manager_id"
}

Response:
{
  "success": true,
  "message": "Time entry rejected successfully",
  "timeEntry": {
    "id": "entry_id",
    "status": "rejected",
    "updatedAt": "2024-01-15T18:00:00Z"
  }
}
```

#### Get Time Entry Status for Date
```http
GET /api/time-entries/status/:date
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - userId?: string (filter by user)

Response:
{
  "success": true,
  "date": "2024-01-15",
  "status": {
    "hasEntries": true,
    "totalActualHours": 8.0,
    "totalBillableHours": 7.5,
    "entriesCount": 2,
    "statuses": ["pending", "approved"],
    "entries": [
      {
        "id": "entry_1",
        "projectDetails": {
          "category": "project",
          "name": "Mobile App"
        },
        "actualHours": 6.0,
        "billableHours": 5.5,
        "status": "pending"
      }
    ]
  }
}
```

### **Time Entry Search & Filtering**

#### Search Time Entries
```http
GET /api/time-entries/search
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - q: string (search query)
  - userId?: string
  - startDate?: string
  - endDate?: string

Response:
{
  "success": true,
  "query": "development",
  "results": [
    {
      "id": "entry_1",
      "task": "Developed new feature",
      "projectDetails": {
        "name": "Mobile App"
      },
      "date": "2024-01-15",
      "actualHours": 8.0,
      "status": "approved"
    }
  ],
  "totalResults": 25
}
```

#### Advanced Filtering
```http
GET /api/time-entries/filter
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - userId?: string
  - projectId?: string
  - productId?: string
  - departmentId?: string
  - status?: string[]
  - isBillable?: boolean
  - minHours?: number
  - maxHours?: number
  - startDate?: string
  - endDate?: string
  - sortBy?: string (date, hours, status)
  - sortOrder?: string (asc, desc)

Response:
{
  "success": true,
  "filters": {
    "userId": "user_1",
    "status": ["approved"],
    "startDate": "2024-01-01",
    "endDate": "2024-01-31"
  },
  "results": [
    {
      "id": "entry_1",
      "date": "2024-01-15",
      "actualHours": 8.0,
      "billableHours": 7.5,
      "status": "approved"
    }
  ],
  "totalResults": 150,
  "summary": {
    "totalActualHours": 1200.0,
    "totalBillableHours": 1100.0,
    "averageHoursPerDay": 8.0
  }
}
```

---

## 🏗️ Project Management APIs

### **Projects Endpoints**

#### Get All Projects
```http
GET /api/projects
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - isBillable?: boolean
  - status?: string (active, inactive)
  - departmentId?: string
  - teamId?: string
  - search?: string
  - page?: number
  - limit?: number

Response:
{
  "success": true,
  "projects": [
    {
      "id": "project_1",
      "name": "Mobile App Development",
      "description": "Development of mobile application",
      "projectType": "Time and Material",
      "clientName": "Mobile Corp",
      "clientEmail": "contact@mobilecorp.com",
      "isBillable": true,
      "status": "active",
      "departmentIds": ["dept_1"],
      "teamIds": ["team_1"],
      "createdBy": "user_1",
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

#### Get Project by ID
```http
GET /api/projects/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "project": {
    "id": "project_1",
    "name": "Mobile App Development",
    "description": "Development of mobile application",
    "projectType": "Time and Material",
    "clientName": "Mobile Corp",
    "clientEmail": "contact@mobilecorp.com",
    "isBillable": true,
    "status": "active",
    "departmentIds": ["dept_1"],
    "teamIds": ["team_1"],
    "createdBy": "user_1",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Create Project
```http
POST /api/projects
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "New Web Project",
  "description": "Web application development",
  "projectType": "Fixed Cost",
  "clientName": "Web Solutions",
  "clientEmail": "info@websolutions.com",
  "isBillable": true,
  "departmentIds": ["dept_1"],
  "teamIds": ["team_1"]
}

Response:
{
  "success": true,
  "project": {
    "id": "new_project_id",
    "name": "New Web Project",
    "description": "Web application development",
    "projectType": "Fixed Cost",
    "clientName": "Web Solutions",
    "clientEmail": "info@websolutions.com",
    "isBillable": true,
    "status": "active",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Update Project
```http
PUT /api/projects/:id
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "Updated Web Project",
  "description": "Updated description",
  "isBillable": false
}

Response:
{
  "success": true,
  "project": {
    "id": "project_id",
    "name": "Updated Web Project",
    "description": "Updated description",
    "isBillable": false,
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Delete Project
```http
DELETE /api/projects/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "Project deleted successfully"
}
```

#### Get User Associated Projects
```http
GET /api/projects/user/:userId
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "projects": [
    {
      "id": "project_1",
      "name": "Mobile App Development",
      "isBillable": true,
      "status": "active"
    }
  ]
}
```

### **Products Endpoints**

#### Get All Products
```http
GET /api/products
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - isBillable?: boolean
  - status?: string (active, inactive)
  - search?: string
  - page?: number
  - limit?: number

Response:
{
  "success": true,
  "products": [
    {
      "id": "product_1",
      "name": "Timesheet Software",
      "productDescription": "Internal timesheet management",
      "isBillable": false,
      "status": "active",
      "createdBy": "user_1",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

#### Create Product
```http
POST /api/products
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "New Product",
  "productDescription": "Product description",
  "isBillable": true
}

Response:
{
  "success": true,
  "product": {
    "id": "new_product_id",
    "name": "New Product",
    "productDescription": "Product description",
    "isBillable": true,
    "status": "active",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Update Product
```http
PUT /api/products/:id
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "Updated Product",
  "isBillable": false
}

Response:
{
  "success": true,
  "product": {
    "id": "product_id",
    "name": "Updated Product",
    "isBillable": false,
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Delete Product
```http
DELETE /api/products/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "Product deleted successfully"
}
```

### **Departments Endpoints**

#### Get All Departments
```http
GET /api/departments
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - isBillable?: boolean
  - status?: string (active, inactive)
  - search?: string
  - page?: number
  - limit?: number

Response:
{
  "success": true,
  "departments": [
    {
      "id": "dept_1",
      "name": "Engineering",
      "departmentDescription": "Software engineering department",
      "isBillable": true,
      "status": "active",
      "createdBy": "user_1",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

#### Create Department
```http
POST /api/departments
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "New Department",
  "departmentDescription": "Department description",
  "isBillable": false
}

Response:
{
  "success": true,
  "department": {
    "id": "new_dept_id",
    "name": "New Department",
    "departmentDescription": "Department description",
    "isBillable": false,
    "status": "active",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Update Department
```http
PUT /api/departments/:id
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "Updated Department",
  "isBillable": true
}

Response:
{
  "success": true,
  "department": {
    "id": "dept_id",
    "name": "Updated Department",
    "isBillable": true,
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Delete Department
```http
DELETE /api/departments/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "Department deleted successfully"
}
```

---

## 🔧 Business Logic Requirements

### **Weekly Time Entry Processing**
```typescript
interface WeeklyTimeEntryRequest {
  weekStart: string; // YYYY-MM-DD
  entries: {
    projectId: string;
    productId?: string;
    departmentId?: string;
    dailyHours: {
      [date: string]: {
        actualHours: number;
        billableHours: number;
        task: string;
      };
    };
  }[];
}
```

### **Time Validation Rules**
- Maximum 24 hours per day
- Maximum 168 hours per week
- Billable hours cannot exceed actual hours
- Cannot have overlapping time entries for same project on same day

### **Approval Workflow**
```typescript
interface ApprovalRequest {
  entryId: string;
  action: 'approve' | 'reject';
  message: string;
  approvedBy: string;
}
```
