# 🎓 EnterpriseCollegeManagement

<p align="center">
  <strong>Enterprise-Style College Management System</strong>
</p>

<p align="center">
  A modern microservices-based college management platform built with
  ASP.NET Core, JWT authentication, Google authentication, YARP API Gateway,
  SQL Server, Serilog, Docker and xUnit.
</p>

<p align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

</p>

<p align="center">

![Microservices](https://img.shields.io/badge/Architecture-Microservices-blue?style=for-the-badge)
![JWT](https://img.shields.io/badge/Auth-JWT-orange?style=for-the-badge)
![Google](https://img.shields.io/badge/Google-Authentication-red?style=for-the-badge&logo=google)
![Gemini AI](https://img.shields.io/badge/AI-Google%20Gemini-4285F4?style=for-the-badge&logo=googlegemini&logoColor=white)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge)

</p>

---

## 📌 Overview

**EnterpriseCollegeManagement** is an enterprise-style College Management
System designed using a **microservices architecture**.

The application separates major business domains into independent services
while providing centralized authentication, authorization, API routing and a
dedicated MVC-based web portal.

The project is being developed with a focus on:

- 🔐 Secure authentication and authorization
- 🧩 Microservices architecture
- 🗄️ Independent service data ownership
- 📊 Audit logging and traceability
- 🔄 JWT and refresh-token authentication
- 🌐 Google authentication
- 🛡️ Role-based authorization
- 🧠 AI-powered exam question generation (Google Gemini)
- ⚡ Redis caching for frequently-read academic data
- 👨‍🏫 Teacher profile management and course–subject assignment
- 📝 Assignment submission with AI-assisted review and feedback (Gemini)
- 🚀 Docker-based deployment readiness
- 🧪 Automated unit testing
- 📦 Clean and maintainable architecture
- 🌍 Production deployment readiness

---

# 🏗️ System Architecture

```text
                               ┌──────────────────────┐
                               │       Browser        │
                               └──────────┬───────────┘
                                          │
                                          ▼
                               ┌──────────────────────┐
                               │        Portal        │
                               │    ASP.NET Core MVC  │
                               └──────────┬───────────┘
                                          │
                                          ▼
                               ┌──────────────────────┐
                               │     API Gateway      │
                               │        YARP          │
                               └──────────┬───────────┘
                                          │
                  ┌───────────────────────┼───────────────────────┐
                  │                       │                       │
                  ▼                       ▼                       ▼
         ┌──────────────────┐   ┌──────────────────┐   ┌──────────────────┐
         │ IdentityService  │   │ StudentService   │   │ AcademicService  │
         │                  │   │                  │   │                  │
         │ Authentication   │   │ Student Domain   │   │ Academic Domain  │
         │ Authorization    │   │ Student Data     │   │ Academic Data    │
         │ JWT              │   │ Department Data  │   │                  │
         │ Google Login     │   │                  │   │                  │
         └──────────────────┘   └──────────────────┘   └──────────────────┘
                                          │
                  ┌───────────────────────┴───────────────────────┐
                  ▼                                               ▼
         ┌──────────────────┐                            ┌──────────────────┐
         │ TeacherService   │                            │ AssignmentService│
         │                  │                            │ Assignments      │
         │ Teacher Profiles │                            │ Submissions      │
         │ Course Mapping   │                            │ PDF Uploads      │
         │ Profile Photos   │                            │ AI Review        │
         └──────────────────┘                            └──────────────────┘
```

## 🔗 Service-to-Service Communication

Services own their data and talk to each other over REST using typed
`HttpClient` clients registered with `AddHttpClient<TInterface, TImplementation>()`:

| Caller | Calls | Client |
|---|---|---|
| StudentService | IdentityService | `IdentityServiceClient` |
| AcademicService | StudentService | `StudentServiceClient` |
| TeacherService | IdentityService | `IdentityServiceClient` |
| TeacherService | StudentService | `StudentServiceClient` |
| TeacherService | AcademicService | `AcademicServiceClient` |
| TeacherService | Audit logging | `AuditServiceClient` |
| AssignmentService | TeacherService | `TeacherServiceClient` |
| AssignmentService | AcademicService | `AcademicServiceClient` |
| AssignmentService | StudentService | `StudentServiceClient` |
| AssignmentService | Audit logging | `AuditServiceClient` |

---

## 📂 Solution Structure

```text
EnterpriseCollegeManagement
│
├── EnterpriseCollegeManagement.IdentityService         ✅ Completed
├── EnterpriseCollegeManagement.StudentService          ✅ Completed
├── EnterpriseCollegeManagement.StudentService.Tests    ✅ 12 tests
├── EnterpriseCollegeManagement.AcademicService         ✅ Completed
├── EnterpriseCollegeManagement.AcademicService.Tests   ✅ 50 tests
├── EnterpriseCollegeManagement.TeacherService          ✅ Completed
├── EnterpriseCollegeManagement.TeacherService.Tests    ✅ 10 tests
├── EnterpriseCollegeManagement.AssignmentService       ✅ Completed (unit tests planned)
├── EnterpriseCollegeManagement.ApiGateway              ⏳ Planned
├── EnterpriseCollegeManagement.Portal                  ⏳ Planned
├── EnterpriseCollegeManagement.slnx
└── README.md
```

---

# 🧰 Technology Stack

| Category | Technology |
|---|---|
| Language | C# |
| Framework | ASP.NET Core |
| Architecture | Microservices |
| API | ASP.NET Core Web API |
| Frontend | ASP.NET Core MVC |
| ORM | Entity Framework Core |
| Database | Microsoft SQL Server |
| Identity | ASP.NET Core Identity |
| Authentication | JWT |
| External Authentication | Google |
| Authorization | Role-Based Authorization |
| Generative AI | Google Gemini API |
| Caching | Redis (StackExchange.Redis) |
| API Gateway | YARP |
| Logging | Serilog |
| API Documentation | Swagger / OpenAPI |
| Testing | xUnit, Moq, EF Core InMemory |
| Containerization | Docker |
| Source Control | Git / GitHub |
| Secret Management | User Secrets / Environment Variables |

---

# 🔐 IdentityService

The IdentityService is responsible for authentication, authorization,
user management and security-related functionality.

## 🔑 Authentication

Implemented:

- ✅ User Registration
- ✅ User Login
- ✅ JWT Authentication
- ✅ Refresh Tokens
- ✅ Refresh Token Rotation
- ✅ Refresh Token Expiration
- ✅ Refresh Token Revocation
- ✅ Logout
- ✅ Google Authentication
- ✅ External Google Login

## 👥 Authorization & Roles

The application currently supports three main roles:

| Role | Description |
|---|---|
| 👑 Admin | System administration and user management |
| 👨‍🏫 Teacher | Teaching and academic-related functionality |
| 🎓 Student | Student-related functionality |

### Role Rules

- New normal users are automatically assigned the Student role.
- New Google users are automatically assigned the Student role.
- Only Admin users can change Student/Teacher roles.
- Normal users cannot assign themselves the Admin role.
- Admin endpoints are protected using role-based authorization.

Example:

```csharp
[Authorize(Roles = "Admin")]
```

# 🔒 Password Management

IdentityService provides:

- ✅ Change Password
- ✅ Forgot Password
- ✅ Reset Password
- ✅ Password Confirmation
- ✅ Refresh Token Revocation after Password Change
- ✅ Refresh Token Revocation after Password Reset

## Development Password Reset

During development, the password-reset token/link can be tested using the
development environment.

## Production Password Reset

The planned production flow is:

```text
User
  │
  ▼
Portal
  │
  ▼
Forgot Password
  │
  ▼
IdentityService
  │
  ▼
Email Provider
  │
  ▼
Password Reset Link
  │
  ▼
Portal Reset Password Page
  │
  ▼
IdentityService
  │
  ▼
Password Updated
```

# 🔍 Audit Logging

Important security-sensitive operations are recorded through the audit system.

Audit information can include:

- Actor
- Action
- Entity
- Entity ID
- Previous value
- New value
- Timestamp

The audit system supports:

- 📅 Date filtering
- 📄 Pagination
- 👑 Admin-only access

# 👤 User Management

Administrators can:

- View application users
- View user roles
- Assign Student role
- Assign Teacher role
- Track role changes through audit logs

The system prevents normal users from assigning the Admin role.

# 🎟️ JWT Authentication

The application uses short-lived JWT access tokens.

JWT tokens contain relevant identity information such as:

- User ID
- Username
- Email
- Role
- Token ID

# 🔄 Refresh Token Architecture

Refresh tokens provide a mechanism for obtaining a new access token without
requiring the user to log in again.

```text
                    Login
                      │
           ┌──────────┴──────────┐
           ▼                     ▼
     Access Token          Refresh Token
           │                     │
           │                     ▼
           │               Token Refresh
           │                     │
           │                     ▼
           │              Old Token Revoked
           │                     │
           │                     ▼
           │              New Refresh Token
           │                     │
           └─────────────────────┘
```

Refresh-token security includes:

- 🔄 Rotation
- ⏳ Expiration
- 🚫 Revocation
- 🔓 Logout
- 🔐 Revocation after password changes
- 🔐 Revocation after password resets

# 🌐 Google Authentication

Google authentication is integrated into the IdentityService.

```text
User
  │
  ▼
Google Login
  │
  ▼
Google Authentication
  │
  ▼
Google Callback
  │
  ▼
IdentityService
  │
  ▼
Find / Create ApplicationUser
  │
  ▼
Assign Student Role
  │
  ▼
Generate Application JWT
  │
  ▼
Generate Refresh Token
```

Google authenticates the external identity.

The application's IdentityService remains responsible for application users,
roles, JWT tokens and refresh tokens.

---

# 🎓 StudentService

The StudentService manages student profiles and department data for the college
management system.

## 👨‍🎓 Student Management

Implemented:

- ✅ Student profile creation
- ✅ Student profile retrieval
- ✅ Student self-profile retrieval
- ✅ Student self-profile update
- ✅ Admin student update
- ✅ Student search and filtering
- ✅ Pagination
- ✅ Profile photo upload/replacement
- ✅ Soft delete
- ✅ Audit fields
- ✅ Department relationship management

## 🏢 Department Management

Implemented:

- ✅ Department creation
- ✅ Department retrieval
- ✅ Department listing
- ✅ Department update
- ✅ Department soft delete
- ✅ Prevention of department deletion when active students are assigned
- ✅ Department-based student relationships
- ✅ Department audit fields

### Department Relationship

```text
Department
   │
   │ 1
   │
   ├───────────────< Student
                    many
```

A student belongs to a department through `DepartmentId`, which references the
department primary key.

## 🗑️ Soft Delete

Student and department records use soft deletion instead of physical deletion.

```text
IsDeleted = false  → Active record
IsDeleted = true   → Soft-deleted record
```

Deletion metadata includes fields such as:

- `DeletedBy`
- `DeletedDate`

## 📝 Audit Fields

Entities support audit information such as:

- `CreatedBy`
- `CreatedDate`
- `UpdatedBy`
- `UpdatedDate`
- `DeletedBy`
- `DeletedDate`

## 🔎 Student Search

Student search supports filtering by:

- Student ID
- Admission Number
- Department
- Name/search text
- Pagination

## 🧪 StudentService Unit Testing

The StudentService now has automated unit tests using:

- xUnit
- Moq
- EF Core InMemory

The test suite covers important business scenarios including:

- ✅ Successful student creation
- ✅ Duplicate admission number validation
- ✅ Invalid department validation
- ✅ Identity user not found
- ✅ Invalid Identity role
- ✅ Existing student profile validation
- ✅ Successful student update
- ✅ Student soft deletion

## 🧪 DepartmentService Unit Testing

DepartmentService also has automated tests covering the main business flows:

- ✅ Successful department creation
- ✅ Department retrieval by ID
- ✅ Successful department update
- ✅ Preventing deletion when active students are assigned

## 🧪 AcademicService Unit Testing

AcademicService has the largest test suite — **50 tests** across Course,
Subject, Exam, Question, and ExamAttendance services (xUnit, Moq, EF Core InMemory).

Key scenarios covered:

- ✅ Course/Subject creation with department-not-found and duplicate-code conflict handling
- ✅ Exam creation, update, soft deletion, and publishing (question marks must match exam total marks)
- ✅ Question validation — invalid marks rejected, and published exams cannot be edited or have questions removed
- ✅ Starting an exam — only for published exams, once per student, and only when the student's course, department, and semester match
- ✅ Submitting an exam — all questions must be answered, duplicate submission blocked
- ✅ **Auto-submission of expired exams** (including `ProcessExpiredExamsAsync`)
- ✅ Result publishing rules — exam must be submitted, result cannot be published twice, students only see published results

## 🧪 TeacherService Unit Testing

TeacherService has **10 tests** covering:

- ✅ Teacher profile creation, and rejection when the Identity user is not a Teacher
- ✅ Self-service profile retrieval, update, and profile-photo upload
- ✅ Course–subject assignment, duplicate-assignment conflict, and missing course–subject handling
- ✅ Listing assigned course–subjects and soft-deleting an assignment

---

# 🛡️ Security

Security is a major focus of this project.

Implemented security features include:

- 🔐 JWT authentication
- 👥 Role-based authorization
- 🔄 Refresh-token rotation
- 🚫 Refresh-token revocation
- 🔑 Password hashing through ASP.NET Core Identity
- 🌐 Google authentication
- 📝 Audit logging
- 🧱 Global exception handling
- 📋 Secure configuration
- 🔒 Production secrets kept outside source control

---

# 🔑 Secret Management

The project separates source code from environment-specific secrets.

## Development

Development secrets are stored using:

**ASP.NET Core User Secrets**

Examples include:

- Database Connection String
- JWT Signing Key
- Google Client ID
- Google Client Secret
- Admin Credentials
- Email Credentials

These values should not be committed to Git.

## Production

Production secrets will be supplied separately through:

- Environment Variables
- Hosting-provider secret-management systems

Production credentials will not be stored in the Git repository.

---

# 🧱 Microservice Responsibilities

## 🔐 IdentityService

Responsible for:

- Authentication
- Authorization
- Users
- Roles
- JWT
- Refresh Tokens
- Google Authentication
- Password Management
- Audit Logging

**Status: ✅ Completed**

## 🎓 StudentService

Responsible for:

- Student profiles
- Student information
- Department management
- Student-related business operations
- Department-related business operations

**Status: ✅ Completed**

## 📚 AcademicService

Responsible for:

- Course and subject management
- Exam creation and lifecycle management
- AI-powered exam question generation
- Student exam attendance and answer submission
- Automatic exam auto-submission on time expiry
- Exam result publishing

### 🧠 AI-Powered Quiz Generation

AcademicService integrates with a generative AI API to automatically draft
multiple-choice exam questions from instructor-provided topic text,
reducing manual question-authoring effort.

Implemented:

- ✅ AI quiz generation via the Google Gemini API
- ✅ Provider-agnostic AI client abstraction (`IAIQuizClient`) — the
  underlying AI provider can be swapped (Gemini, OpenAI, or a local model)
  without changing business logic
- ✅ Structured JSON question/answer generation, parsed and validated
  before persistence
- ✅ Generated questions stored in SQL Server for reuse and editing
- ✅ Manual question creation also supported alongside AI generation

```csharp
public interface IAIQuizClient
{
    Task<List<GeneratedQuestionDto>> GenerateQuestionsAsync(
        QuizGenerationContextDto context);
}
```

### 📝 Exam Lifecycle

- ✅ Exam creation and scheduling
- ✅ Exam attendance tracking with expiry
- ✅ Student answer submission
- ✅ Automatic exam auto-submission via a background service once the
  allotted time expires
- ✅ Exam result publishing

### ⏱️ Background Processing

A hosted `BackgroundService` (`ExamAutoSubmitBackgroundService`) runs
continuously to detect exams whose attendance window has expired and
auto-submits them, ensuring students are graded on their in-progress
answers instead of being left in an unsubmitted state.

### ⚡ Redis Caching

AcademicService uses Redis as a cache-aside (read-through) cache for frequently
requested reference data, reducing repeated SQL Server queries.

Implemented:

- ✅ Cache-aside reads for course, subject, and course–subject lookups by ID
  (keys: `course:{id}`, `subject:{id}`, `coursesubject:{id}`)
- ✅ DTOs serialized to JSON and stored with a 20-minute expiry
- ✅ A single shared `IConnectionMultiplexer` registered as a singleton
- ✅ `IRedisCacheService` abstraction (`Get` / `Set` / `Remove`) so services stay
  testable — it is mocked in the unit tests

Planned improvements:

- ⏳ Remove cached entries when a course, subject, or course–subject is updated or deleted
- ⏳ Fail open — fall back to the database if Redis is unavailable
- ⏳ Unit tests that verify cache hit/miss behaviour

**Status: ✅ Completed**

## 👨‍🏫 TeacherService

Responsible for:

- Teacher profile management (each profile is linked to an Identity user through `UserId`)
- Teacher self-service: view/update own profile and upload a profile photo
- Assigning teachers to course–subject combinations
- Admin-controlled teacher updates
- Audit-friendly records (created/updated/deleted by + timestamps) with soft delete

### 🔌 API Endpoints

| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/api/Teacher/create-profile` | Admin | Create a teacher profile for an existing Identity user |
| GET | `/api/Teacher/me` | Teacher | View own profile |
| PUT | `/api/Teacher/me` | Teacher | Update own profile |
| POST | `/api/Teacher/me/profile-photo` | Teacher | Upload profile photo |
| PUT | `/api/Teacher/{userId}` | Admin | Admin update of a teacher |
| POST | `/api/TeacherCourseSubject/assign` | Admin | Assign a teacher to a course–subject |
| GET | `/api/TeacherCourseSubject/teacher/{teacherId}` | Admin, Teacher | View a teacher's assignments |
| DELETE | `/api/TeacherCourseSubject/teacher/{teacherId}/course-subject/{courseSubjectId}` | Admin | Remove an assignment |

### 🧱 Design Notes

- Layered structure: Controllers → Services → EF Core (`TeacherDbContext`), with DTOs for every request/response
- JWT bearer authentication with role-based `[Authorize(Roles = ...)]` on every endpoint
- Typed clients to IdentityService, StudentService, AcademicService, and audit logging
- `GlobalExceptionMiddleware` with custom exceptions (`NotFound`, `Conflict`, `BadRequest`, `Unauthorized`)
- Serilog logging and Swagger with a JWT bearer scheme
- Soft delete (`IsDeleted`, `DeletedBy`, `DeletedDate`) and audit columns on entities
- 3 EF Core migrations: `InitialCreate`, `AddTeacherCourseSubject`, `AddTeacherProfilePhoto`

**Status: ✅ Completed**

## 📝 AssignmentService

Responsible for:

- Assignment creation, publishing, and management by teachers
- Student assignment submission (text and/or PDF upload)
- Teacher grading and feedback
- **AI-assisted assignment review** — an AI-generated mark suggestion and feedback draft that the teacher can accept or ignore
- Audit-friendly records with soft delete

### 🔌 API Endpoints

| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/api/Assignment` | Admin, Teacher | Create an assignment |
| PUT | `/api/Assignment/{id}` | Admin, Teacher | Update an assignment |
| GET | `/api/Assignment/{id}` | Admin, Teacher | Get an assignment |
| GET | `/api/Assignment/my-assignments` | Admin, Teacher | List the caller's assignments |
| PATCH | `/api/Assignment/{id}/publish` | Admin, Teacher | Publish an assignment |
| DELETE | `/api/Assignment/{id}` | Admin, Teacher | Soft-delete an assignment |
| POST | `/api/AssignmentSubmission/submit` | Student | Submit text and/or a PDF |
| GET | `/api/AssignmentSubmission/my-submissions` | Student | List own submissions |
| GET | `/api/AssignmentSubmission/assignment/{assignmentId}/submissions` | Teacher, Admin | List submissions for an assignment |
| GET | `/api/AssignmentSubmission/{id}` | Student, Teacher, Admin | Get one submission |
| PUT | `/api/AssignmentSubmission/{id}/grade` | Teacher, Admin | Grade a submission and give feedback |
| POST | `/api/AssignmentSubmission/assignment/{assignmentId}/submission/{submissionId}/ai-review` | Teacher, Admin | Get an AI review suggestion |

### 🧠 AI Assignment Review

The review endpoint gives teachers a **suggestion**, never a final grade.

```text
Teacher requests AI review
        │
        ▼
Authorize: only the assignment's owning teacher (or an Admin)
        │
        ▼
Collect submission content: typed text + text extracted from the PDF (PdfPig)
        │
        ▼
Build prompt: assignment title, requirements, max marks, and the
student's work wrapped in <student_submission> tags
        │
        ▼
Gemini (structured JSON schema, temperature 0.2)
        │
        ▼
Validate the response: all fields present, marks between 0 and max marks
        │
        ▼
Return: suggested marks, summary, strengths, improvements, suggested feedback
        │
        ▼
Teacher decides → grades through the separate PUT .../grade endpoint
```

Design decisions:

- ✅ **Human in the loop** — the AI never writes marks to the database; grading is a separate teacher action
- ✅ **Provider-agnostic** — `IAssignmentReviewClient` hides the Gemini implementation (`GeminiAssignmentReviewClient`)
- ✅ **Structured output** — a JSON response schema is sent to Gemini and the reply is validated before it is returned
- ✅ **Prompt-injection resistance** — student text is delimited and the prompt tells the model to treat it as data, ignore instructions inside it, and not invent evidence
- ✅ **Output validation** — incomplete responses and marks outside `0..MaxMarks` are rejected
- ✅ **Clear failure for scanned PDFs** — if no text can be extracted, the teacher is told OCR would be required

### 📎 Submission and File Rules

- A student can submit only to a **published** assignment, **before the due date**, and **once**
- A submission needs text, a PDF, or both
- Uploads: **PDF only, maximum 5 MB**, PDF signature check, stored under a generated GUID file name (never the client's file name)

### 🧱 Design Notes

- Layered structure: Controllers → Services → EF Core (`AssignmentDbContext`), DTOs for every request/response
- JWT bearer authentication with role-based authorization on every endpoint
- Typed clients to TeacherService, AcademicService, StudentService, and audit logging
- `GlobalExceptionMiddleware`, Serilog, and Swagger with a JWT bearer scheme
- EF Core migration: `InitialAssignmentSchema`
- Packages: `Google.GenAI` (Gemini), `PdfPig` (PDF text extraction)

**Status: ✅ Completed (unit tests planned)**

---

# 🌐 API Gateway

The API Gateway provides a single entry point for clients and routes requests
to the appropriate microservice.

Technology:

**YARP - Yet Another Reverse Proxy**

Planned routing:

```text
/api/auth/*        → IdentityService
/api/users/*       → IdentityService
/api/students/*    → StudentService
/api/academic/*    → AcademicService
/api/teacher/*     → TeacherService
/api/teachercoursesubject/* → TeacherService
/api/assignment/*  → AssignmentService
/api/assignmentsubmission/* → AssignmentService
```

**Status: ⏳ Planned**

---

# 🖥️ Portal

The Portal is the user-facing web application built using:

**ASP.NET Core MVC**

The Portal will communicate with backend services through the API Gateway.

Planned functionality includes:

- User login
- Google login
- Registration
- Password management
- Student functionality
- Teacher functionality
- Admin functionality
- Assignment functionality
- Academic functionality

**Status: ⏳ Planned**

---

# 📐 Architecture Principles

## Independent Service Ownership

Each microservice owns its business domain and its data.

```text
IdentityService
      │
      ▼
Identity Data

StudentService
      │
      ▼
Student Data

AcademicService
      │
      ▼
Academic Data

AssignmentService
      │
      ▼
Assignment Data
```

Services should communicate through APIs rather than directly accessing another
microservice's database.

## Separation of Responsibilities

```text
Browser
   │
   ▼
Portal
   │
   ▼
API Gateway
   │
   ▼
Microservices
   │
   ▼
Service-specific Data
```

---

# 🧪 Testing

The project uses:

- **xUnit** for test execution and assertions
- **Moq** for mocking dependencies
- **EF Core InMemory** for isolated service-level database testing

Current automated test coverage focuses on important business rules in the
completed Identity/Student work rather than attempting to test every line of
code.

The StudentService and DepartmentService test suite currently contains **12
meaningful unit tests** covering create, validation, update and soft-delete
scenarios.

Testing will continue to expand as additional services are completed.

**Status: ✅ StudentService and DepartmentService unit testing completed**

---

# 🐳 Docker

The project is being prepared for containerized deployment using Docker.

Planned container structure:

```text
┌──────────────────────┐
│        Portal        │
└──────────────────────┘

┌──────────────────────┐
│     API Gateway      │
└──────────────────────┘

┌──────────────────────┐
│   IdentityService    │
└──────────────────────┘

┌──────────────────────┐
│    StudentService    │
└──────────────────────┘

┌──────────────────────┐
│   AcademicService    │
└──────────────────────┘

┌──────────────────────┐
│  AssignmentService   │
└──────────────────────┘

┌──────────────────────┐
│        Redis         │
└──────────────────────┘
```

**Status: ⏳ Planned**

---

# 🚀 Deployment Strategy

The application is designed to support environment-specific configuration.

## Development Environment

```text
Local Development
     │
     ├── User Secrets
     ├── Development Database
     ├── Development Google OAuth
     └── Local Services
```

## Production Environment

```text
Production Hosting
     │
     ├── Environment Variables
     ├── Production Database
     ├── Production Google OAuth
     ├── Production Email Provider
     └── HTTPS
```

The application code remains the same while environment-specific values are
provided through configuration.

---

# 🌍 Production Showcase

The long-term goal is to deploy the project as a working production-style
application.

Expected production architecture:

```text
                          Internet
                             │
                             ▼
                        ┌─────────────┐
                        │   Portal    │
                        └──────┬──────┘
                               │
                               ▼
                        ┌─────────────┐
                        │ API Gateway │
                        └──────┬──────┘
                               │
               ┌───────────────┼───────────────┐
               │               │               │
               ▼               ▼               ▼
          Identity          Student        Academic
          Service           Service        Service
               │               │               │
               └───────────────┼───────────────┘
                               ▼
                         Assignment
                           Service
```

---

# 📧 Production Email Flow

The final production password-reset architecture will use a real email
provider.

```text
User
 │
 ▼
Forgot Password
 │
 ▼
IdentityService
 │
 ▼
Email Provider
 │
 ▼
User Email
 │
 ▼
Secure Reset Link
 │
 ▼
Portal
 │
 ▼
IdentityService
 │
 ▼
Password Reset
```

Sensitive information such as passwords and tokens will not be written to
application logs.

---

# ⚙️ Configuration and Running Locally

Services read their settings from **.NET User Secrets** — `appsettings.json` is intentionally
empty, so no keys or connection strings are committed to the repository.

Example for AssignmentService:

```bash
cd EnterpriseCollegeManagement.AssignmentService

dotnet user-secrets set "ConnectionStrings:AssignmentDb" "<SQL Server connection string>"
dotnet user-secrets set "Jwt:Key"      "<signing key, 32+ characters>"
dotnet user-secrets set "Jwt:Issuer"   "<issuer>"
dotnet user-secrets set "Jwt:Audience" "<audience>"
dotnet user-secrets set "AI:Gemini:ApiKey" "<your Gemini API key>"
dotnet user-secrets set "ServiceUrls:TeacherService"  "https://localhost:<port>"
dotnet user-secrets set "ServiceUrls:AcademicService" "https://localhost:<port>"
dotnet user-secrets set "ServiceUrls:IdentityService" "https://localhost:<port>"
dotnet user-secrets set "ServiceUrls:StudentService"  "https://localhost:<port>"

dotnet ef database update
dotnet run
```

Notes:

- `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience` must match across services, because every service validates tokens issued by IdentityService
- `AI:Gemini:Model` is optional and overrides the default Gemini model
- The other services follow the same pattern — check each service's `Program.cs` for the keys it reads
- AcademicService also needs `Redis:ConnectionString`. For local development, run Redis in Docker (`docker run -d --name ecm-redis -p 6379:6379 redis:7`) and use `localhost:6379`
- Never commit API keys, signing keys, or uploaded student files

---

# 📊 Project Status

| Component | Status |
|---|---|
| 🔐 IdentityService | ✅ Completed |
| 🎓 StudentService | ✅ Completed |
| 📚 AcademicService (Exams + AI Quiz Generation) | ✅ Completed |
| ⚡ Redis Caching (AcademicService) | 🚧 In Progress |
| 👨‍🏫 TeacherService | ✅ Completed |
| 📝 AssignmentService (+ AI Assignment Review) | ✅ Completed |
| 🌐 API Gateway | ⏳ Planned |
| 🖥️ Portal | ⏳ Planned |
| 🧪 StudentService Unit Testing (12 tests) | ✅ Completed |
| 🧪 AcademicService Unit Testing (50 tests) | ✅ Completed |
| 🧪 TeacherService Unit Testing (10 tests) | ✅ Completed |
| 🧪 AssignmentService Unit Testing | ⏳ Planned |
| 🐳 Docker | ⏳ Planned |
| 🔄 CI/CD | ⏳ Planned |
| 🌍 Production Deployment | ⏳ Planned |

---

# 🗺️ Roadmap

```text
[x] IdentityService
        │
        ▼
[x] StudentService + Department Management + Unit Tests
        │
        ▼
[x] AcademicService + Exam Module + AI Quiz Generation (Gemini) + Unit Tests
        │
        ▼
[x] TeacherService + Course-Subject Assignment + Unit Tests
        │
        ▼
[x] AssignmentService + AI Assignment Review (Gemini)
        │
        ▼
[ ] API Gateway
        │
        ▼
[ ] Portal
        │
        ▼
[ ] Docker
        │
        ▼
[ ] CI/CD
        │
        ▼
[ ] Production Deployment
```

---

# 🔀 Git Workflow

The project uses feature-based Git branches.

```text
main
 │
 ├── feature/identity-service      ✅ Merged
 │
 ├── feature/student-service       ✅ Merged
 │
 ├── feature/academic-service      ✅ Merged
 │
 ├── feature/teacher-service       ✅ Merged
 │
 ├── feature/assignment-service    ✅ Completed (pending merge to main)
 │
 ├── feature/api-gateway           ⏳ Next
 │
 └── feature/portal
```

## Development Workflow

```text
Create Feature Branch
        │
        ▼
     Develop
        │
        ▼
       Test
        │
        ▼
      Commit
        │
        ▼
       Push
        │
        ▼
   Merge into main
        │
        ▼
Create Next Feature Branch
```

---

# 💻 Local Development

## Prerequisites

Install the following tools:

- .NET SDK
- Visual Studio
- SQL Server
- SQL Server Management Studio
- Git
- Docker Desktop

## Run the Services

Each microservice is designed to be developed and tested independently.

API documentation is available through Swagger/OpenAPI for the Web API services.

The README intentionally does not store environment-specific localhost URLs;
use the URL generated by the local development profile for each service.

---

# 🔐 Configuration

The repository contains only safe configuration.

The following information must never be committed:

- ❌ Production database passwords
- ❌ JWT signing keys
- ❌ Google Client Secrets
- ❌ Admin passwords
- ❌ SMTP credentials
- ❌ API keys
- ❌ Other sensitive credentials

Development secrets should be stored using:

**ASP.NET Core User Secrets**

Production secrets should be supplied through:

- Environment Variables
- Hosting-provider secure secret-management systems

---

# 📝 Development Notes

This project is being developed as an enterprise-style personal project to
demonstrate modern .NET architecture, microservices, authentication,
authorization, API design, security practices, testing and deployment concepts.

The project is intended as a technical portfolio project and does not
represent professional production microservices experience at an organization.

---

# 🎯 Project Goals

The primary goals of this project are to demonstrate practical experience with:

- Modern ASP.NET Core development
- C# backend development
- Microservices architecture
- RESTful Web APIs
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT authentication
- Refresh-token security
- Role-based authorization
- Google authentication
- Generative AI integration for content generation (Google Gemini)
- Distributed caching with Redis
- Audit logging
- API Gateway architecture
- ASP.NET Core MVC
- Docker
- Automated testing
- CI/CD
- Production deployment concepts
- Secure configuration and secret management

---

# 👨‍💻 Author

**Shajan J Jacob**

EnterpriseCollegeManagement

- ASP.NET Core
- C#
- Microservices
- SQL Server
- JWT
- Google Authentication
- Google Gemini AI
- Redis
- YARP
- Serilog
- Docker
- xUnit

<p align="center">
  ⭐ <strong>Built with ASP.NET Core and modern .NET architecture</strong>
</p>