# Attendance & Leave API Documentation

---

## Attendance API

### 1. GET /api/attendance

ดูข้อมูลการเข้า-ออกงาน**ของตัวเอง** (ดึงจาก token)

**Request:**
```
GET /api/attendance?date=2026-06-11&page=1&limit=10
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| date | DateTime? | No | กรองวันที่ (ถ้าไม่ใส่ = วันปัจจุบัน) |
| page | int | No | หน้าที่ (default: 1) |
| limit | int | No | จำนวนต่อหน้า (default: 10) |

**Response:**
```json
{
  "status": "success",
  "message": "",
  "data": {
    "attendance": [
      {
        "attendanceRecordId": 1,
        "employeeId": 5,
        "employeeName": "สมชาย ใจดี",
        "date": "2026-06-11T00:00:00",
        "checkIn": "2026-06-11T08:30:00",
        "checkOut": "2026-06-11T17:30:00",
        "status": "present"
      }
    ],
    "total": 1,
    "page": 1,
    "limit": 10
  }
}
```

---

### 2. GET /api/attendance/overview

ดูข้อมูลการเข้า-ออกงานของ**team** (ตาม scope ของ role)

**Request:**
```
GET /api/attendance/overview?date=2026-06-11&employeeId=5&status=present&divisionId=1&departmentId=1&page=1&limit=10
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| date | DateTime? | No | กรองวันที่ |
| employeeId | int? | No | กรองตามพนักงาน |
| status | string? | No | กรองตามสถานะ (present, absent, late, early) |
| divisionId | int? | No | กรองตามฝ่าย |
| departmentId | int? | No | กรองตามแผนก |
| page | int | No | หน้าที่ (default: 1) |
| limit | int | No | จำนวนต่อหน้า (default: 10) |

**Response:** เหมือนกับ `GET /api/attendance`

---

## Leave API

### 1. GET /api/leave

ดูข้อมูลการลา**ของตัวเอง** (ดึงจาก token)

**Request:**
```
GET /api/leave?status=pending&page=1&limit=10
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | string? | No | กรองตามสถานะ (pending, approved, rejected, cancelled) |
| page | int | No | หน้าที่ (default: 1) |
| limit | int | No | จำนวนต่อหน้า (default: 10) |

**Response:**
```json
{
  "status": "success",
  "message": "",
  "data": {
    "requests": [
      {
        "leaveRequestId": 1,
        "employeeId": 5,
        "employeeName": "สมชาย ใจดี",
        "leaveType": "annual",
        "startDate": "2026-06-15T00:00:00",
        "endDate": "2026-06-17T00:00:00",
        "days": 3,
        "status": "pending",
        "reason": "ลาพักผ่อน"
      }
    ],
    "total": 1,
    "page": 1,
    "limit": 10
  }
}
```

---

### 2. GET /api/leave/overview

ดูข้อมูลการลาของ**team** (ตาม scope ของ role)

**Request:**
```
GET /api/leave/overview?status=pending&employeeId=5&divisionId=1&departmentId=1&page=1&limit=10
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | string? | No | กรองตามสถานะ (pending, approved, rejected, cancelled) |
| employeeId | int? | No | กรองตามพนักงาน |
| divisionId | int? | No | กรองตามฝ่าย |
| departmentId | int? | No | กรองตามแผนก |
| page | int | No | หน้าที่ (default: 1) |
| limit | int | No | จำนวนต่อหน้า (default: 10) |

**Response:** เหมือนกับ `GET /api/leave`

---

### 3. GET /api/leave/balance

ดูยอดวันลาคงเหลือและสถานะการลาของ**ตัวเอง**

**Request:**
```
GET /api/leave/balance
```

**Response:**
```json
{
  "status": "success",
  "message": "",
  "data": {
    "annual": {
      "total": 20,
      "used": 6,
      "balance": 14
    },
    "sick": {
      "total": 10,
      "used": 2,
      "balance": 8
    },
    "pendingRequests": 2,
    "leaveTakenYtd": 8
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| annual.total | int | วันลากิจสิทธิ์ที่ได้รับต่อปี |
| annual.used | int | วันลากิจที่ใช้ไป (approved) |
| annual.balance | int | วันลากิจคงเหลือ = total - used |
| sick.total | int | วันลาป่วยที่ได้รับต่อปี |
| sick.used | int | วันลาป่วยที่ใช้ไป (approved) |
| sick.balance | int | วันลาป่วยคงเหลือ = total - used |
| pendingRequests | int | จำนวนคำขอลาที่รออนุมัติ (ทั้งหมด) |
| leaveTakenYtd | int | จำนวนวันลาที่ใช้ไปทั้งปี (approved) |

---

### 4. POST /api/leave

สร้างคำขอลาใหม่

**Request:**
```
POST /api/leave
```

```json
{
  "leaveType": "annual",
  "startDate": "2026-06-20",
  "endDate": "2026-06-22",
  "reason": "ลาพักผ่อน"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| leaveType | string | Yes | ประเภทการลา (annual, sick, personal, other) |
| startDate | DateTime | Yes | วันที่เริ่มลา |
| endDate | DateTime | Yes | วันที่สิ้นสุดการลา |
| reason | string? | No | เหตุผลการลา |

**LeaveType ที่รองรับ (case-insensitive):**
| ค่า | คำอธิบาย |
|-----|---------|
| `annual` | ลากิจสิทธิ์ |
| `sick` | ลาป่วย |

**Response:**
```json
{
  "status": "success",
  "message": "Leave request created",
  "data": {
    "leaveRequestId": 5,
    "employeeId": 5,
    "employeeName": "สมชาย ใจดี",
    "leaveType": "annual",
    "startDate": "2026-06-20T00:00:00",
    "endDate": "2026-06-22T00:00:00",
    "days": 3,
    "status": "pending",
    "reason": "ลาพักผ่อน"
  }
}
```

---

### 5. GET /api/leave/calendar

ดูปฏิทินการลาและวันหยุดของ**เดือน/ปี**ที่ระบุ รวม leaves ที่ approved และ holidays จากฐานข้อมูล

**Request:**
```
GET /api/leave/calendar?month=6&year=2026
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| month | int? | No | เดือน (1-12) ถ้าไม่ใส่ = เดือนปัจจุบัน |
| year | int? | No | ปี ถ้าไม่ใส่ = ปีปัจจุบัน |

**Response:**
```json
{
  "status": "success",
  "message": "",
  "data": {
    "month": 6,
    "year": 2026,
    "holidays": [
      {
        "date": "2026-06-01",
        "name": "วันหยุดชดเชย"
      }
    ],
    "leaves": [
      {
        "employeeName": "สมชาย ใจดี",
        "leaveType": "Annual",
        "startDate": "2026-06-15",
        "endDate": "2026-06-16",
        "days": 2,
        "reason": "ลาพักผ่อน"
      }
    ]
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| month | int | เดือนที่ค้นหา |
| year | int | ปีที่ค้นหา |
| holidays | array | รายการวันหยุดในเดือน |
| holidays[].date | string | วันที่ (yyyy-MM-dd) |
| holidays[].name | string | ชื่อวันหยุด |
| leaves | array | รายการการลาที่ approved ในเดือน |
| leaves[].employeeName | string? | ชื่อพนักงาน |
| leaves[].leaveType | string | ประเภทการลา เช่น `"Annual"`, `"Sick"` |
| leaves[].startDate | string | วันที่เริ่มลา (yyyy-MM-dd) |
| leaves[].endDate | string | วันที่สิ้นสุดการลา (yyyy-MM-dd) |
| leaves[].days | int | จำนวนวันลา |
| leaves[].reason | string? | เหตุผลการลา |

**หมายเหตุ:**
- แสดงเฉพาะ `leave` ที่มี status = `approved`
- `holiday` ดึงจากตาราง Holidays ในฐานข้อมูล
- `holidays` และ `leaves` แยก array กันอย่างชัดเจน

### Calendar Scope Rules

| Role | Leaves ที่เห็นใน Calendar |
|------|---------------------------|
| Admin, Manager, HR | เห็น approved leaves ทุกคน |
| HeadDivision | เห็น approved leaves ทุกคนใน division ของตัวเอง |
| HeadDepartment | เห็น approved leaves ใน department ของตัวเอง |
| Employee | เห็นเฉพาะ approved leaves ของตัวเอง |
| Audit | **ไม่มีสิทธิ์เข้าถึง** |

---

## Permissions

### Overview Permissions (แยกจาก view ปกติ)

| Endpoint | Permission ที่ต้องมี | Roles ที่มีสิทธิ์ |
|----------|---------------------|------------------|
| `GET /api/leave/overview` | `leaves.view_overview` | Admin, HR, Manager, HeadDivision, HeadDepartment |
| `GET /api/attendance/overview` | `attendance.view_overview` | Admin, HR, Manager, HeadDivision, HeadDepartment |

**หมายเหตุ:** `leaves.view` และ `attendance.view` ใช้สำหรับดูข้อมูลส่วนตัว ไม่สามารถใช้ดู overview ได้

### Leave Permissions

| Permission | Description | Roles |
|------------|-------------|-------|
| `leaves.view` | ดู leave ของตัวเอง, ดูปฏิทินการลา/วันหยุด | Admin, HR, Manager, HeadDivision, HeadDepartment, Employee |
| `leaves.view_overview` | ดู leave ของ team/division/department | Admin, HR, Manager, HeadDivision, HeadDepartment |
| `leaves.create` | สร้าง leave request | Admin, HR, Employee |
| `leaves.approve` | อนุมัติ/ปฏิเสธ leave request | Admin, HR, Manager, HeadDivision, HeadDepartment |

### Attendance Permissions

| Permission | Description | Roles |
|------------|-------------|-------|
| `attendance.view` | ดู attendance ของตัวเอง | ทุก role |
| `attendance.view_overview` | ดู attendance ของ team/division/department | Admin, HR, Manager, HeadDivision, HeadDepartment |
| `attendance.checkin` | บันทึกเข้างาน | ทุก role |
| `attendance.checkout` | บันทึกออกงาน | ทุก role |

---

## Scope Rules

**ถ้าใส่ divisionId/departmentId ที่ไม่ตรงกับ token scope:**
```json
{
  "status": "success",
  "data": {
    "attendance": [],
    "total": 0,
    "page": 1,
    "limit": 10
  }
}
```

---

## Common Response Format

```json
{
  "status": "success",
  "message": "",
  "data": { ... }
}
```

**Error Response:**
```json
{
  "status": "fail",
  "message": "Error description",
  "data": null
}
```

---

## หมายเหตุสำหรับ Frontend

1. **Token Scope** - ทุก request ต้องมี JWT token เพื่อระบุตัวตนและ scope
2. **Pagination** - ใช้ `page` และ `limit` สำหรับ pagination
3. **Status Filter** - `status` ใช้ค่า lowercase
4. **EmployeeName** - ดึงจากการ join กับ Employees table แล้ว (ไม่ต้องส่งจาก request)
5. **LeaveRequestId** - ใช้ `leaveRequestId` แทน `id` ในการอ้างอิง
6. **AttendanceRecordId** - ใช้ `attendanceRecordId` แทน `id` ในการอ้างอิง

---

## Changelog

### 2026-06-25

**New Features:**
- `GET /api/leave/calendar` - เพิ่ม `reason` field ใน leaves response

**Permission Changes:**
- ลบ `leaves.view` ออกจาก Audit role → Audit ไม่สามารถเข้าถึง Calendar API ได้

**Scope Rules:**
- เพิ่ม Calendar Scope Rules แสดงว่าแต่ละ role เห็น leaves อะไรบ้าง

### 2026-06-24

**New API:**
- `GET /api/leave/calendar` - ดูปฏิทินการลาและวันหยุดประจำเดือน
  - รวม leaves ที่ approved และ holidays จากฐานข้อมูล
  - Query params: `month` (int?), `year` (int?) - default เป็น current month/year
  - Response: `{ month, year, holidays[], leaves[] }` แยก holidays และ leaves ออกจากกัน

**API Changes:**
- `GET /api/leave/calendar` - เปลี่ยน response structure จาก `events[]` เป็น `holidays[]` และ `leaves[]` แยกกัน

### 2026-06-15

**Bug Fixes:**
- `GET /api/leave/overview` - แก้ไขการกรอง division/department สำหรับ Admin, HR, Manager ที่ bypass scope (ก่อนหน้านี้ filter ไม่ทำงาน)

**API Changes:**
- `GET /api/leave/balance` - เปลี่ยนโครงสร้าง response:
  - ลบ `personal` ออกจาก balance items
  - เพิ่ม `pendingRequests` (จำนวนคำขอที่รออนุมัติ)
  - เพิ่ม `leaveTakenYtd` (วันลาที่ใช้ไปทั้งปี)
- เพิ่ม permissions ใหม่ `leaves.view_overview` และ `attendance.view_overview` สำหรับ overview endpoints
- เพิ่ม `POST /api/leave` - สร้างคำขอลาใหม่ พร้อมรายละเอียด leaveType ที่รองรับ