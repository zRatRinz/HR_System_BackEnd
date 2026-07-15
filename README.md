# HR System

ระบบจัดการข้อมูลทรัพยากรบุคคล (HR Management System) สำหรับจัดการพนักงาน การลา เวลาปฏิบัติงาน และเงินเดือน

## Architecture

ระบบใช้ **Clean Architecture** ร่วมกับ **Domain-Driven Design (DDD)** patterns เพื่อแยก concerns อย่างชัดเจน

```
┌─────────────────────────────────────────────────────────┐
│                      API Layer                          │
│   Controllers, Middleware, Filters, API Documentation    │
├─────────────────────────────────────────────────────────┤
│                   Application Layer                      │
│   Use Cases, DTOs, Interfaces, Business Logic          │
├─────────────────────────────────────────────────────────┤
│                    Domain Layer                         │
│   Entities, Enums, Domain Events, Business Rules       │
├─────────────────────────────────────────────────────────┤
│                Infrastructure Layer                      │
│   Repositories, External Services, Database Access      │
└─────────────────────────────────────────────────────────┘
```

### Layer Dependencies

```
Domain (Core)           → ไม่มี dependencies
    ↑ Application        → รู้จักแค่ Domain
        ↑ Infrastructure → ใช้ interfaces จาก Domain/Application
            ↑ API        → ขึ้นกับ Application และ Infrastructure
```

## Tech Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 10 |
| Web Framework | ASP.NET Core |
| Database | SQL Server |
| ORM | Dapper |
| Authentication | JWT Bearer Token |
| API Documentation | Scalar (OpenAPI) |
| Architecture | Clean Architecture + DDD |

## Features

ระบบประกอบด้วย modules หลักดังนี้:

| Module | คำอธิบาย |
|--------|-----------|
| **Employee Management** | จัดการข้อมูลพนักงาน ตำแหน่ง แผนก ฝ่าย |
| **Leave Management** | สร้างและอนุมัติใบลา ตรวจสอบวันลาคงเหลือ |
| **Attendance** | ลงเวลาเข้า-ออก ดูประวัติการลงเวลา |
| **Payroll** | ประมวลผลเงินเดือน ดูรายละเอียดเงินเดือน |
| **Approval Workflow** | อนุมัติ/ปฏิเสธคำขอต่างๆ |
| **Dashboard** | ภาพรวมข้อมูลสำคัญ |
| **Reports** | รายงานต่างๆ ส่งออกเป็น PDF |
| **Role & Permission** | ระบบสิทธิ์การใช้งานตามบทบาท (RBAC) |

## API Documentation

เข้าถึงเอกสาร API ได้ที่ [Scalar](http://localhost:5051/scalar) เมื่อระบบทำงานใน Development mode
