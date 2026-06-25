# Database Structure

## Tables Overview

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│  Divisions   │       │ Departments │       │  Positions  │
└──────┬───────┘       └──────┬───────┘       └──────┬───────┘
       │                       │                      │
       │                 ┌─────┴─────┐                │
       │                 │           │                │
       ▼                 ▼           ▼                ▼
┌─────────────────────────────────────────────────────────────┐
│                       Employees                              │
│  (FK: UserId, DivisionId, DepartmentId, PositionId)          │
└──────┬───────────────┬───────────────┬─────────────────────┘
       │               │               │
       ▼               ▼               ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│LeaveRequests│ │AttendanceRec│ │PayrollRecords│
└─────────────┘ └─────────────┘ └─────────────┘

┌─────────────┐
│  Holidays   │
└─────────────┘

┌─────────────┐       ┌─────────────┐
│    Users     │       │    Roles     │
└──────┬───────┘       └──────┬───────┘
       │                       │
       │     ┌─────────────────┤
       ▼     ▼                 │
┌─────────────────────┐        │
│     UserRoles       │◄───────┘
│ (Junction Table)    │
└─────────────────────┘

┌─────────────┐
│ApprovalItems│
│(FK: EmployeeId, ApprovedBy UserId)
└─────────────┘
```

## Table Details

### Divisions
| Column | Type | Constraints |
|--------|------|-------------|
| DivisionId | INT | PK, IDENTITY |
| DivisionName | NVARCHAR(100) | NOT NULL |
| Description | NVARCHAR(500) | NULL |
| CreatedAt | DATETIME | NOT NULL |

### Departments
| Column | Type | Constraints |
|--------|------|-------------|
| DepartmentId | INT | PK, IDENTITY |
| DivisionId | INT | FK → Divisions |
| DepartmentName | NVARCHAR(100) | NOT NULL |
| CreatedAt | DATETIME | NOT NULL |

### Positions
| Column | Type | Constraints |
|--------|------|-------------|
| PositionId | INT | PK, IDENTITY |
| PositionName | NVARCHAR(100) | NOT NULL |
| Description | NVARCHAR(500) | NULL |
| CreatedAt | DATETIME | NOT NULL |

### Users
| Column | Type | Constraints |
|--------|------|-------------|
| UserId | INT | PK, IDENTITY |
| Email | NVARCHAR(255) | UNIQUE, NOT NULL |
| PasswordHash | NVARCHAR(255) | NOT NULL |
| Name | NVARCHAR(100) | NOT NULL |
| Status | NVARCHAR(50) | NOT NULL (Role name) |
| CreatedAt | DATETIME | NOT NULL |
| UpdatedAt | DATETIME | NULL |

### Roles
| Column | Type | Constraints |
|--------|------|-------------|
| RoleId | INT | PK, IDENTITY |
| RoleName | NVARCHAR(50) | UNIQUE, NOT NULL |
| Description | NVARCHAR(255) | NULL |

### UserRoles (Junction Table)
| Column | Type | Constraints |
|--------|------|-------------|
| UserId | INT | PK, FK → Users |
| RoleId | INT | PK, FK → Roles |

### Employees
| Column | Type | Constraints |
|--------|------|-------------|
| EmployeeId | INT | PK, IDENTITY |
| UserId | INT | FK → Users, UNIQUE |
| FirstName | NVARCHAR(50) | NOT NULL |
| LastName | NVARCHAR(50) | NOT NULL |
| DivisionId | INT | FK → Divisions, NULL |
| DepartmentId | INT | FK → Departments, NULL |
| PositionId | INT | FK → Positions, NULL |
| HireDate | DATETIME | NOT NULL |
| Salary | DECIMAL(18,2) | NOT NULL |
| Status | NVARCHAR(50) | NOT NULL |
| CreatedAt | DATETIME | NOT NULL |
| UpdatedAt | DATETIME | NULL |

### LeaveRequests
| Column | Type | Constraints |
|--------|------|-------------|
| LeaveRequestId | INT | PK, IDENTITY |
| EmployeeId | INT | FK → Employees |
| LeaveType | NVARCHAR(50) | NOT NULL |
| StartDate | DATE | NOT NULL |
| EndDate | DATE | NOT NULL |
| TotalDays | DECIMAL(5,2) | NOT NULL |
| Reason | NVARCHAR(500) | NULL |
| Status | NVARCHAR(50) | NOT NULL |
| ApprovedBy | INT | FK → Users, NULL |
| ApprovedAt | DATETIME | NULL |
| CreatedAt | DATETIME | NOT NULL |
| UpdatedAt | DATETIME | NULL |

### AttendanceRecords
| Column | Type | Constraints |
|--------|------|-------------|
| AttendanceId | INT | PK, IDENTITY |
| EmployeeId | INT | FK → Employees |
| Date | DATE | NOT NULL |
| CheckIn | DATETIME | NULL |
| CheckOut | DATETIME | NULL |
| Status | NVARCHAR(50) | NOT NULL |
| Notes | NVARCHAR(255) | NULL |
| CreatedAt | DATETIME | NOT NULL |
| UpdatedAt | DATETIME | NULL |

### PayrollRecords
| Column | Type | Constraints |
|--------|------|-------------|
| PayrollId | INT | PK, IDENTITY |
| EmployeeId | INT | FK → Employees |
| PayPeriod | NVARCHAR(50) | NOT NULL |
| BasicSalary | DECIMAL(18,2) | NOT NULL |
| Allowances | DECIMAL(18,2) | NOT NULL |
| Deductions | DECIMAL(18,2) | NOT NULL |
| NetSalary | DECIMAL(18,2) | NOT NULL |
| Status | NVARCHAR(50) | NOT NULL |
| ProcessedAt | DATETIME | NULL |
| CreatedAt | DATETIME | NOT NULL |

### ApprovalItems
| Column | Type | Constraints |
|--------|------|-------------|
| ApprovalItemId | INT | PK, IDENTITY |
| LeaveRequestId | INT | FK → LeaveRequests |
| RequesterEmployeeId | INT | FK → Employees |
| ApproverEmployeeId | INT | FK → Employees |
| Type | VARCHAR(20) | NOT NULL |
| Status | VARCHAR(20) | NOT NULL |
| Comment | VARCHAR(500) | NULL |
| CreatedAt | DATETIME | NOT NULL |
| UpdatedAt | DATETIME | NULL |

### Holidays
| Column | Type | Constraints |
|--------|------|-------------|
| HolidayId | INT | PK, IDENTITY |
| HolidayName | NVARCHAR(100) | NOT NULL |
| HolidayDate | DATE | NOT NULL, UNIQUE |
| HolidayYear | INT | NOT NULL |
| HolidayType | NVARCHAR(50) | NOT NULL |
| IsRecurring | BIT | NOT NULL |
| HolidayDescription | NVARCHAR(255) | NULL |
| CreatedAt | DATETIME | NOT NULL |

## Foreign Key Dependencies (Delete Order)

```
Child Tables (Delete First)          Parent Tables (Delete Last)
────────────────────────────────      ──────────────────────────────
1. LeaveRequests    → Employees      6. Employees     → Users
2. AttendanceRecords → Employees      7. UserRoles     → Users, Roles
3. PayrollRecords   → Employees      8. Users
4. ApprovalItems    → Employees, Users
5. Employees        → Users, Departments, Positions
                                            9. Departments → Divisions
                                           10. Positions
                                           11. Divisions
```

## Seeded Data

### Roles (7 roles)
- Admin
- Manager
- Employee
- HeadDivision
- HeadDepartment
- HR
- Audit

### Multi-Role Support
User สามารถมีได้หลาย roles ผ่าน UserRoles junction table:
- `hr.head@company.com` → HeadDivision, HR
- `hr.manager@company.com` → Manager, HR
- และอื่นๆ ดูใน `Scripts/Reset_and_Seed_All.sql`

### Holidays
ดูข้อมูลวันหยุดใน `Scripts/Reset_and_Seed_All.sql`