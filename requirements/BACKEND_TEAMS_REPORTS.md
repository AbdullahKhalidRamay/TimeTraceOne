# TimeFlow Backend Requirements - Part 3: Team Management & Reports

## Overview
This document contains the team management and reporting APIs required for the TimeFlow application.

---

## 👥 Team Management APIs

### **Teams Endpoints**

#### Get All Teams
```http
GET /api/teams
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - departmentId?: string
  - search?: string
  - page?: number
  - limit?: number

Response:
{
  "success": true,
  "teams": [
    {
      "id": "team_1",
      "name": "Frontend Development Team",
      "description": "Team responsible for frontend development",
      "departmentId": "dept_1",
      "leaderId": "user_1",
      "memberIds": ["user_1", "user_2"],
      "associatedProjects": ["project_1"],
      "associatedProducts": ["product_1"],
      "associatedDepartments": ["dept_1"],
      "createdBy": "user_1",
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

#### Get Team by ID
```http
GET /api/teams/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "team": {
    "id": "team_1",
    "name": "Frontend Development Team",
    "description": "Team responsible for frontend development",
    "departmentId": "dept_1",
    "leaderId": "user_1",
    "memberIds": ["user_1", "user_2"],
    "associatedProjects": ["project_1"],
    "associatedProducts": ["product_1"],
    "associatedDepartments": ["dept_1"],
    "createdBy": "user_1",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Create Team
```http
POST /api/teams
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "New Team",
  "description": "Team description",
  "departmentId": "dept_1",
  "leaderId": "user_1",
  "memberIds": ["user_1", "user_2"]
}

Response:
{
  "success": true,
  "team": {
    "id": "new_team_id",
    "name": "New Team",
    "description": "Team description",
    "departmentId": "dept_1",
    "leaderId": "user_1",
    "memberIds": ["user_1", "user_2"],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Update Team
```http
PUT /api/teams/:id
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "name": "Updated Team",
  "description": "Updated description",
  "leaderId": "user_2"
}

Response:
{
  "success": true,
  "team": {
    "id": "team_id",
    "name": "Updated Team",
    "description": "Updated description",
    "leaderId": "user_2",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### Delete Team
```http
DELETE /api/teams/:id
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "Team deleted successfully"
}
```

### **Team Member Management**

#### Add Member to Team
```http
POST /api/teams/:id/members
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "userId": "user_3"
}

Response:
{
  "success": true,
  "message": "Member added to team successfully",
  "team": {
    "id": "team_id",
    "memberIds": ["user_1", "user_2", "user_3"]
  }
}
```

#### Remove Member from Team
```http
DELETE /api/teams/:id/members/:userId
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "Member removed from team successfully",
  "team": {
    "id": "team_id",
    "memberIds": ["user_1", "user_2"]
  }
}
```

#### Update Team Leader
```http
PUT /api/teams/:id/leader
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "leaderId": "user_2"
}

Response:
{
  "success": true,
  "message": "Team leader updated successfully",
  "team": {
    "id": "team_id",
    "leaderId": "user_2"
  }
}
```

### **Team Associations**

#### Associate Team with Project
```http
POST /api/teams/:id/associate-project
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "projectId": "project_1"
}

Response:
{
  "success": true,
  "message": "Team associated with project successfully"
}
```

#### Associate Team with Product
```http
POST /api/teams/:id/associate-product
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "productId": "product_1"
}

Response:
{
  "success": true,
  "message": "Team associated with product successfully"
}
```

#### Associate Team with Department
```http
POST /api/teams/:id/associate-department
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "departmentId": "dept_1"
}

Response:
{
  "success": true,
  "message": "Team associated with department successfully"
}
```

#### Get User Teams
```http
GET /api/teams/user/:userId
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "teams": [
    {
      "id": "team_1",
      "name": "Frontend Development Team",
      "departmentId": "dept_1",
      "leaderId": "user_1"
    }
  ]
}
```

---

## 📊 Reports & Analytics APIs

### **User Reports**

#### Get User Statistics
```http
GET /api/reports/user/:userId
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "userId": "user_1",
  "period": {
    "startDate": "2024-01-01",
    "endDate": "2024-01-31"
  },
  "stats": {
    "totalEntries": 150,
    "approvedEntries": 140,
    "pendingEntries": 8,
    "rejectedEntries": 2,
    "totalActualHours": 1200.5,
    "totalBillableHours": 1100.0,
    "averageHoursPerDay": 8.0,
    "overtimeHours": 40.5,
    "approvalRate": 93.3,
    "billableRate": 91.6
  },
  "dailyBreakdown": [
    {
      "date": "2024-01-01",
      "actualHours": 8.0,
      "billableHours": 7.5,
      "entriesCount": 2
    }
  ]
}
```

#### Get User Weekly Report
```http
GET /api/reports/user/:userId/weekly
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - weekStart?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "userId": "user_1",
  "weekStart": "2024-01-15",
  "weekEnd": "2024-01-21",
  "weeklyStats": {
    "totalActualHours": 40.0,
    "totalBillableHours": 38.5,
    "averageHoursPerDay": 8.0,
    "overtimeHours": 0.0,
    "entriesCount": 20
  },
  "dailyBreakdown": [
    {
      "date": "2024-01-15",
      "actualHours": 8.0,
      "billableHours": 7.5,
      "entriesCount": 2
    }
  ]
}
```

#### Get User Monthly Report
```http
GET /api/reports/user/:userId/monthly
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - month?: string (YYYY-MM)

Response:
{
  "success": true,
  "userId": "user_1",
  "month": "2024-01",
  "monthlyStats": {
    "totalActualHours": 168.0,
    "totalBillableHours": 154.0,
    "averageHoursPerDay": 8.0,
    "overtimeHours": 8.0,
    "entriesCount": 84,
    "workingDays": 21
  },
  "weeklyBreakdown": [
    {
      "weekStart": "2024-01-01",
      "weekEnd": "2024-01-07",
      "actualHours": 40.0,
      "billableHours": 38.5
    }
  ]
}
```

### **Team Reports**

#### Get Team Report
```http
GET /api/reports/team/:teamId
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "teamId": "team_1",
  "teamName": "Frontend Development Team",
  "period": {
    "startDate": "2024-01-01",
    "endDate": "2024-01-31"
  },
  "teamStats": {
    "totalMembers": 5,
    "totalActualHours": 840.0,
    "totalBillableHours": 770.0,
    "averageHoursPerMember": 168.0,
    "totalEntries": 420
  },
  "memberBreakdown": [
    {
      "userId": "user_1",
      "userName": "John Doe",
      "actualHours": 168.0,
      "billableHours": 154.0,
      "entriesCount": 84
    }
  ],
  "projectBreakdown": [
    {
      "projectId": "project_1",
      "projectName": "Mobile App",
      "actualHours": 400.0,
      "billableHours": 380.0
    }
  ]
}
```

#### Get Team Weekly Report
```http
GET /api/reports/team/:teamId/weekly
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - weekStart?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "teamId": "team_1",
  "weekStart": "2024-01-15",
  "weekEnd": "2024-01-21",
  "weeklyStats": {
    "totalActualHours": 200.0,
    "totalBillableHours": 190.0,
    "averageHoursPerMember": 40.0,
    "entriesCount": 100
  },
  "dailyBreakdown": [
    {
      "date": "2024-01-15",
      "actualHours": 40.0,
      "billableHours": 38.0,
      "entriesCount": 20
    }
  ]
}
```

### **System Reports**

#### Get System Overview
```http
GET /api/reports/system/overview
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "period": {
    "startDate": "2024-01-01",
    "endDate": "2024-01-31"
  },
  "systemStats": {
    "totalUsers": 25,
    "totalProjects": 15,
    "totalProducts": 8,
    "totalDepartments": 5,
    "totalTeams": 12,
    "totalTimeEntries": 4200,
    "totalActualHours": 16800.0,
    "totalBillableHours": 15400.0,
    "approvalRate": 92.5,
    "billableRate": 91.7
  },
  "userActivity": {
    "activeUsers": 22,
    "inactiveUsers": 3,
    "newUsersThisMonth": 5
  },
  "projectPerformance": {
    "activeProjects": 12,
    "completedProjects": 3,
    "averageProjectHours": 1400.0
  }
}
```

#### Get Department Performance Report
```http
GET /api/reports/system/departments
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "period": {
    "startDate": "2024-01-01",
    "endDate": "2024-01-31"
  },
  "departments": [
    {
      "id": "dept_1",
      "name": "Engineering",
      "totalMembers": 15,
      "totalActualHours": 5040.0,
      "totalBillableHours": 4620.0,
      "averageHoursPerMember": 336.0,
      "billableRate": 91.7,
      "topProjects": [
        {
          "projectId": "project_1",
          "projectName": "Mobile App",
          "actualHours": 1200.0,
          "billableHours": 1100.0
        }
      ]
    }
  ]
}
```

#### Get Project Performance Report
```http
GET /api/reports/system/projects
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)
  - status?: string (active, completed, inactive)

Response:
{
  "success": true,
  "period": {
    "startDate": "2024-01-01",
    "endDate": "2024-01-31"
  },
  "projects": [
    {
      "id": "project_1",
      "name": "Mobile App Development",
      "status": "active",
      "totalActualHours": 1200.0,
      "totalBillableHours": 1100.0,
      "billableRate": 91.7,
      "teamCount": 3,
      "memberCount": 8,
      "averageHoursPerMember": 150.0,
      "completionPercentage": 65.0
    }
  ]
}
```

### **Export APIs**

#### Export CSV Report
```http
GET /api/reports/export/csv
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - reportType: string (user, team, system, project)
  - userId?: string
  - teamId?: string
  - projectId?: string
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)

Response:
Content-Type: text/csv
Content-Disposition: attachment; filename="report_2024-01.csv"

Date,User,Project,Actual Hours,Billable Hours,Status
2024-01-15,John Doe,Mobile App,8.0,7.5,Approved
2024-01-16,Jane Smith,Web Platform,8.0,8.0,Pending
```

#### Export PDF Report
```http
GET /api/reports/export/pdf
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - reportType: string (user, team, system, project)
  - userId?: string
  - teamId?: string
  - projectId?: string
  - startDate?: string (YYYY-MM-DD)
  - endDate?: string (YYYY-MM-DD)

Response:
Content-Type: application/pdf
Content-Disposition: attachment; filename="report_2024-01.pdf"

[PDF binary content]
```

---

## 🔔 Notification System APIs

### **Notifications Endpoints**

#### Get User Notifications
```http
GET /api/notifications
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - type?: string[] (approval, rejection, reminder, system)
  - isRead?: boolean
  - page?: number
  - limit?: number

Response:
{
  "success": true,
  "notifications": [
    {
      "id": "notif_1",
      "title": "Timesheet Approved",
      "message": "Your timesheet for 2024-01-15 has been approved",
      "type": "approval",
      "isRead": false,
      "relatedEntryId": "entry_1",
      "createdAt": "2024-01-15T18:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 45,
    "totalPages": 3
  },
  "unreadCount": 12
}
```

#### Mark Notification as Read
```http
PUT /api/notifications/:id/read
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "Notification marked as read"
}
```

#### Mark All Notifications as Read
```http
PUT /api/notifications/read-all
Headers: Authorization: Bearer {accessToken}

Response:
{
  "success": true,
  "message": "All notifications marked as read",
  "updatedCount": 12
}
```

---

## 🔧 Utility APIs

### **Data Validation**

#### Validate Time Entry
```http
POST /api/validation/time-entry
Headers: Authorization: Bearer {accessToken}
Request Body:
{
  "date": "2024-01-15",
  "actualHours": 8.0,
  "billableHours": 7.5,
  "userId": "user_1"
}

Response:
{
  "success": true,
  "isValid": true,
  "validationRules": {
    "maxDailyHours": 24,
    "maxWeeklyHours": 168,
    "billableHoursValid": true,
    "noOverlap": true
  }
}
```

#### Get User Available Hours
```http
GET /api/users/:id/available-hours
Headers: Authorization: Bearer {accessToken}
Query Parameters:
  - date?: string (YYYY-MM-DD)

Response:
{
  "success": true,
  "userId": "user_1",
  "date": "2024-01-15",
  "availableHours": 8.0,
  "usedHours": 6.0,
  "remainingHours": 2.0,
  "overtimeHours": 0.0
}
```
