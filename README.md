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
- 🚀 Docker-based deployment readiness
- 🧪 Automated testing
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
        │ JWT              │   │                  │   │                  │
        │ Google Login     │   │                  │   │                  │
        └──────────────────┘   └──────────────────┘   └──────────────────┘
                                         │
                                         ▼
                              ┌──────────────────────┐
                              │  AssignmentService  │
                              │                      │
                              │  Assignment Domain   │
                              └──────────────────────┘

```

## 📂 Solution Structure
EnterpriseCollegeManagement
│
├── src
│   │
│   ├── EnterpriseCollegeManagement.AcademicService
│   │
│   ├── EnterpriseCollegeManagement.ApiGateway
│   │
│   ├── EnterpriseCollegeManagement.AssignmentService
│   │
│   ├── EnterpriseCollegeManagement.IdentityService
│   │
│   ├── EnterpriseCollegeManagement.Portal
│   │
│   └── EnterpriseCollegeManagement.StudentService
│
├── tests
│
├── docs
│
├── docker
│
└── README.md
# 🧰 Technology Stack
Category	Technology
Language	C#
Framework	ASP.NET Core
Architecture	Microservices
API	ASP.NET Core Web API
Frontend	ASP.NET Core MVC
ORM	Entity Framework Core
Database	Microsoft SQL Server
Identity	ASP.NET Core Identity
Authentication	JWT
External Authentication	Google
Authorization	Role-Based Authorization
API Gateway	YARP
Logging	Serilog
API Documentation	Swagger / OpenAPI
Testing	xUnit
Containerization	Docker
Source Control	Git / GitHub
Secret Management	User Secrets / Environment Variables
# 🔐 IdentityService

The IdentityService is responsible for authentication, authorization,
user management and security-related functionality.

## 🔑 Authentication

Implemented:

✅ User Registration
✅ User Login
✅ JWT Authentication
✅ Refresh Tokens
✅ Refresh Token Rotation
✅ Refresh Token Expiration
✅ Refresh Token Revocation
✅ Logout
✅ Google Authentication
✅ External Google Login
## 👥 Authorization & Roles

The application currently supports three main roles:

Role	Description
👑 Admin	System administration and user management
👨‍🏫 Teacher	Teaching and academic-related functionality
🎓 Student	Student-related functionality
### Role Rules
New normal users are automatically assigned the Student role.
New Google users are automatically assigned the Student role.
Only Admin users can change Student/Teacher roles.
Normal users cannot assign themselves the Admin role.
Admin endpoints are protected using role-based authorization.

Example:

[Authorize(Roles = "Admin")]
# 🔒 Password Management

IdentityService provides:

✅ Change Password
✅ Forgot Password
✅ Reset Password
✅ Password Confirmation
✅ Refresh Token Revocation after Password Change
✅ Refresh Token Revocation after Password Reset
## Development Password Reset

During development, the password-reset token/link can be tested using the
development environment.

## Production Password Reset

The planned production flow is:

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
# 🔍 Audit Logging

Important security-sensitive operations are recorded through the audit system.

Audit information can include:

Actor
Action
Entity
Entity ID
Previous value
New value
Timestamp

The audit system supports:

📅 Date filtering
📄 Pagination
👑 Admin-only access

Example:

Admin
  │
  ▼
Role Changed
  │
  ▼
Target User
  │
  ├── Previous Role: Student
  │
  └── New Role: Teacher
# 👤 User Management

Administrators can:

View application users
View user roles
Assign Student role
Assign Teacher role
Track role changes through audit logs

The system prevents normal users from assigning the Admin role.

# 🎟️ JWT Authentication

The application uses short-lived JWT access tokens.

JWT tokens contain relevant identity information such as:

User ID
Username
Email
Role
Token ID

Authentication flow:

Login
  │
  ▼
IdentityService
  │
  ├──────────────► JWT Access Token
  │
  └──────────────► Refresh Token
# 🔄 Refresh Token Architecture

Refresh tokens provide a mechanism for obtaining a new access token without
requiring the user to log in again.

                    Login
                      │
          ┌───────────┴───────────┐
          ▼                       ▼
    Access Token            Refresh Token
          │                       │
          │                       ▼
          │                 Token Refresh
          │                       │
          │                       ▼
          │                Old Token Revoked
          │                       │
          │                       ▼
          │                New Refresh Token
          │                       │
          └───────────────────────┘

Refresh-token security includes:

🔄 Rotation
⏳ Expiration
🚫 Revocation
🔓 Logout
🔐 Revocation after password changes
🔐 Revocation after password resets
# 🌐 Google Authentication

Google authentication is integrated into the IdentityService.

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

Google authenticates the external identity.

The application's IdentityService remains responsible for application users,
roles, JWT tokens and refresh tokens.

# 🛡️ Security

Security is a major focus of this project.

Implemented security features include:

🔐 JWT authentication
👥 Role-based authorization
🔄 Refresh-token rotation
🚫 Refresh-token revocation
🔑 Password hashing through ASP.NET Core Identity
🌐 Google authentication
📝 Audit logging
🧱 Global exception handling
📋 Secure configuration
🔒 Production secrets kept outside source control
# 🔑 Secret Management

The project separates source code from environment-specific secrets.

Development

Development secrets are stored using:

ASP.NET Core User Secrets

Examples include:

Database Connection String
JWT Signing Key
Google Client ID
Google Client Secret
Admin Credentials
Email Credentials

These values should not be committed to Git.

Production

Production secrets will be supplied separately through:

Environment Variables

or the hosting provider's secure secret-management system.

Production credentials will not be stored in the Git repository.

# 🧱 Microservice Responsibilities
# 🔐 IdentityService

Responsible for:

Authentication
Authorization
Users
Roles
JWT
Refresh Tokens
Google Authentication
Password Management
Audit Logging

Status: ✅ Completed

## 🎓 StudentService

Responsible for:

Student profiles
Student information
Student-related business operations

Status: 🚧 In Development

## 📚 AcademicService

Responsible for:

Courses
Subjects
Academic information
Academic-related business operations

Status: ⏳ Planned

## 📝 AssignmentService

Responsible for:

Assignments
Assignment management
Assignment-related business operations

Status: ⏳ Planned

# 🌐 API Gateway

The API Gateway provides a single entry point for clients and routes requests
to the appropriate microservice.

Technology:

YARP - Yet Another Reverse Proxy

Planned routing:

/api/auth/*        → IdentityService
/api/users/*       → IdentityService
/api/students/*    → StudentService
/api/academic/*    → AcademicService
/api/assignments/* → AssignmentService

Status: ⏳ Planned

# 🖥️ Portal

The Portal is the user-facing web application built using:

ASP.NET Core MVC

The Portal will communicate with backend services through the API Gateway.

Planned functionality includes:

User login
Google login
Registration
Password management
Student functionality
Teacher functionality
Admin functionality
Assignment functionality
Academic functionality

Status: ⏳ Planned

# 📐 Architecture Principles
## Independent Service Ownership

Each microservice owns its business domain and its data.

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

Services should communicate through APIs rather than directly accessing another
microservice's database.

## Separation of Responsibilities
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
# 🧪 Testing

The project uses:

xUnit

Testing focuses on important business and security scenarios including:

Authentication
Authorization
Role management
User management
JWT functionality
Refresh tokens
Business rules

Testing coverage will continue to expand as additional services are completed.

Status: 🚧 In Progress

# 🐳 Docker

The project is being prepared for containerized deployment using Docker.

Planned container structure:

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

Status: ⏳ Planned

# 🚀 Deployment Strategy

The application is designed to support environment-specific configuration.

## Development Environment
Local Machine
     │
     ├── User Secrets
     ├── Development Database
     ├── Development Google OAuth
     └── Local Services
## Production Environment
Production Hosting
       │
       ├── Environment Variables
       ├── Production Database
       ├── Production Google OAuth
       ├── Production Email Provider
       └── HTTPS

The application code remains the same while environment-specific values are
provided through configuration.

# 🌍 Production Showcase

The long-term goal is to deploy the project as a working production-style
application.

Expected production architecture:

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
             ┌──────────────┼──────────────┐
             │              │              │
             ▼              ▼              ▼
        Identity        Student        Academic
        Service         Service        Service
                            │
                            ▼
                     Assignment
                       Service
# 📧 Production Email Flow

The final production password-reset architecture will use a real email
provider.

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

Sensitive information such as passwords and tokens will not be written to
application logs.

# 📊 Project Status
Component	Status
🔐 IdentityService	✅ Completed
🎓 StudentService	🚧 In Development
📚 AcademicService	⏳ Planned
📝 AssignmentService	⏳ Planned
🌐 API Gateway	⏳ Planned
🖥️ Portal	⏳ Planned
🧪 Automated Testing	🚧 In Progress
🐳 Docker	⏳ Planned
🔄 CI/CD	⏳ Planned
🌍 Production Deployment	⏳ Planned
# 🗺️ Roadmap
[x] IdentityService
        │
        ▼
[ ] StudentService
        │
        ▼
[ ] AcademicService
        │
        ▼
[ ] AssignmentService
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
# 🔀 Git Workflow

The project uses feature-based Git branches.

main
 │
 ├── feature/identity-service      ✅ Merged
 │
 ├── feature/student-service       🚧 Current
 │
 ├── feature/academic-service
 │
 ├── feature/assignment-service
 │
 ├── feature/api-gateway
 │
 └── feature/portal
## Development Workflow
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
# 💻 Local Development
## Prerequisites

Install the following tools:

.NET SDK
Visual Studio
SQL Server
SQL Server Management Studio
Git
Docker Desktop
## Run the Services

Each microservice is designed to be developed and tested independently.

API documentation is available through Swagger for the Web API services.

Example IdentityService Swagger URL:

https://localhost:7319/swagger/index.html
# 🔐 Configuration

The repository contains only safe configuration.

The following information must never be committed:

❌ Production database passwords
❌ JWT signing keys
❌ Google Client Secrets
❌ Admin passwords
❌ SMTP credentials
❌ API keys
❌ Other sensitive credentials

Development secrets should be stored using:

ASP.NET Core User Secrets

Production secrets should be supplied through:

Environment Variables

or the hosting provider's secure secret-management mechanism.

# 📝 Development Notes

This project is being developed as an enterprise-style personal project to
demonstrate modern .NET architecture, microservices, authentication,
authorization, API design, security practices, testing and deployment concepts.

The project is intended as a technical portfolio project and does not
represent professional production microservices experience at an organization.

# 🎯 Project Goals

The primary goals of this project are to demonstrate practical experience with:

Modern ASP.NET Core development
C# backend development
Microservices architecture
RESTful Web APIs
Entity Framework Core
SQL Server
ASP.NET Core Identity
JWT authentication
Refresh-token security
Role-based authorization
Google authentication
Audit logging
API Gateway architecture
ASP.NET Core MVC
Docker
Automated testing
CI/CD
Production deployment
Secure configuration and secret management
# 👨‍💻 Author
Shajan J Jacob

EnterpriseCollegeManagement

ASP.NET Core
C#
Microservices
SQL Server
JWT
Google Authentication
YARP
Serilog
Docker
xUnit
<p align="center">

⭐ <strong>Built with ASP.NET Core and modern .NET architecture</strong>

</p> ```
```