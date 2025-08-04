# Modular Monolith DDD Template

**Author:** dev@geekywolf  
**Template Name:** Modular DDD Template  
**Short Name:** `modmon`

## What is this?

This is a comprehensive .NET template that implements a **Modular Monolith** architecture following **Domain-Driven Design (DDD)** principles. The template provides a production-ready foundation for building scalable, maintainable applications that can evolve from monoliths to microservices when needed.

The template combines modern architectural patterns and practices to create a robust, enterprise-grade application structure that promotes:
- **Modularity** through clear boundaries
- **Testability** with comprehensive test coverage
- **Maintainability** through clean architecture
- **Scalability** through well-defined module separation
- **Database Management** through automated migrations

## How to Use This Template

### Installation & Setup

1. **Clone the template repository:**
   ```bash
   git clone https://github.com/naveen-as-a-geek-wolf/Modular_Monolyth_VS_Template.git
   ```

2. **Navigate to the template directory:**
   ```bash
   cd Modular_Monolyth_VS_Template
   ```

3. **Install the template:**
   ```bash
   dotnet new install .
   ```

4. **Create a new project:**
   ```bash
   dotnet new modmon -n YourProjectName -o YourProjectFolder
   ```
   > **Note:** The `-o YourProjectFolder` parameter is optional. If omitted, the project will be created in the current directory.

5. **Navigate to your project:**
   ```bash
   cd YourProjectFolder
   ```

6. **Set up the database:**
   ```powershell
   # Install Flyway and create database with all modules
   ./development_scripts/runDevFlyway.ps1 -action migrate -module All -createDatabase $true
   ```

### Database Management Commands

```powershell
# Migrate all modules at once (creates database if not exists)
./development_scripts/runDevFlyway.ps1 -action migrate -module All -createDatabase $true

# Migrate specific module
./development_scripts/runDevFlyway.ps1 -action migrate -module User

# Check migration status
./development_scripts/runDevFlyway.ps1 -action info -module All

# Validate migrations
./development_scripts/runDevFlyway.ps1 -action validate -module All

# Clean and re-migrate (development only)
./development_scripts/runDevFlyway.ps1 -action cleanAndMigrate -module All -createDatabase $true

# Create new migration script
./development_scripts/newMigrationScript.ps1 -module User -description "Add new user fields"

# Remove last migration (development only)
./development_scripts/removeLastMigration.ps1 -module User
```

## Architectural Patterns & Design Decisions

### 1. Modular Monolith Architecture

**What it is:** A monolithic application structured as a collection of loosely coupled, highly cohesive modules that could potentially be extracted into separate services.

**Why we use it:**
- **Simplified Deployment:** Single deployable unit reduces operational complexity
- **Inter-module Communication:** Direct in-process calls eliminate network latency
- **Gradual Evolution:** Modules can be extracted to microservices when needed
- **Development Productivity:** Faster development cycles compared to distributed systems
- **Data Consistency:** ACID transactions across modules when needed

**Implementation:**
- Each module (`User`, `Game`) has its own bounded context
- Clear module boundaries with defined interfaces
- Separate database schemas per module
- Independent migration histories

### 2. Minimal Endpoints (Minimal APIs)

**What it is:** A lightweight approach to building HTTP APIs using .NET's minimal API features instead of traditional MVC controllers.

**Why we use it:**
- **Performance:** Reduced overhead and faster startup times
- **Simplicity:** Less boilerplate code and configuration
- **Modern Approach:** Leverages latest .NET features for API development
- **Developer Experience:** More intuitive and straightforward API definition

**Implementation:**
- Endpoint definitions close to business logic
- Built-in dependency injection
- Automatic OpenAPI/Swagger generation

### 3. CQRS (Command Query Responsibility Segregation)

**What it is:** Architectural pattern that separates read (Query) and write (Command) operations into different models.

**Why we use it:**
- **Scalability:** Independent scaling of read and write operations
- **Performance:** Optimized queries without affecting command models
- **Flexibility:** Different data models for different use cases
- **Maintainability:** Clear separation of concerns
- **Evolution:** Easier to implement eventual consistency when moving to microservices

**Implementation:**
- Commands handle business operations and state changes
- Queries handle data retrieval and reporting
- Separate models for read and write operations
- Clear distinction between CQS operations

### 4. Custom MediateX Implementation

**What it is:** A custom implementation of the Mediator pattern called "MediateX" that provides the same functionality as MediatR but remains open-source and free.

**Why we use MediateX instead of MediatR:**
- **Cost Considerations:** MediatR is transitioning to a paid model, making MediateX a cost-effective alternative
- **Open Source:** Maintains the open-source philosophy and avoids licensing costs
- **Control:** Full control over the mediator implementation and its evolution
- **Compatibility:** Provides the same patterns and benefits as MediatR without vendor lock-in
- **Decoupling:** Controllers don't directly depend on business logic
- **Single Responsibility:** Each handler has one specific purpose
- **Cross-cutting Concerns:** Easy to add behaviors like validation, logging, caching
- **Testability:** Individual handlers can be tested in isolation
- **Pipeline Behaviors:** Consistent handling of cross-cutting concerns

**Implementation:**
- Command and Query handlers in Application layer
- Pipeline behaviors for validation, logging, and error handling
- Request/response patterns for consistent API structure
- Drop-in replacement for MediatR with the same API surface

### 5. Infrastructure and Module-wise Database Context

**What it is:** Each module has its own database context and migration history, providing true data isolation.

**Why we use it:**
- **Module Independence:** Each module can evolve its data model independently
- **Migration Safety:** Module-specific migrations reduce risk of conflicts
- **Future Microservices:** Easy to extract modules with their data
- **Team Autonomy:** Different teams can work on different modules without conflicts
- **Bounded Contexts:** True DDD implementation with separate persistence per context

**Implementation:**
```
MyCustomApp.Infrastructure/
├── Data/
│   ├── UserModule/
│   │   ├── UserDbContext.cs
│   │   └── Migrations/
│   └── GameModule/
│       ├── GameDbContext.cs
│       └── Migrations/
```

### 6. Flyway Database Migrations

**What it is:** Version control for your database schema with automatic migration management.

**Why we use it:**
- **Reliable Migrations:** Ensures consistent database state across environments
- **Version Control:** Database changes tracked alongside code changes
- **Team Collaboration:** Eliminates merge conflicts in database schemas
- **Environment Consistency:** Same database state in dev, test, and production
- **Rollback Capability:** Safe rollback of database changes when needed
- **Module Isolation:** Independent migration histories per module

**Benefits over Entity Framework Migrations:**
- **Technology Agnostic:** Works with any database platform
- **Team Friendly:** Better handling of parallel development
- **Production Ready:** Robust migration execution and validation
- **Cross-Platform:** Consistent behavior across different environments

## Project Structure

```
MyCustomApp/
├── MyCustomApp.API/                    # Presentation Layer (Minimal APIs)
├── MyCustomApp.Application/            # Application Layer (CQRS, Handlers)
│   ├── Modules/
│   │   ├── User/                       # User Module Commands/Queries
│   │   └── Game/                       # Game Module Commands/Queries
│   └── Contracts/                      # Shared Application Contracts
├── MyCustomApp.Domain/                 # Domain Layer (Entities, Value Objects)
├── MyCustomApp.Infrastructure/         # Infrastructure Layer (Data, External Services)
│   └── Data/                          # Module-specific DbContexts
├── MyCustomApp.UnitTest/              # Unit Tests
├── MyCustomApp.IntegrationTest/       # Integration Tests
├── Database_scripts/                   # Flyway Migration Scripts
│   └── MyCustomApp/
│       ├── User/                      # User Module Migrations
│       └── Game/                      # Game Module Migrations
├── development_scripts/               # Development Automation Scripts
│   ├── flyway-10.18.0/               # Flyway Binaries
│   ├── runDevFlyway.ps1              # Main Migration Script
│   ├── newMigrationScript.ps1         # Create New Migrations
│   └── removeLastMigration.ps1        # Remove Last Migration (Dev Only)
└── docker-compose.yml                 # Container Orchestration
```

## Available Modules

The template includes the following modules by default:

- **User Module:** User management and authentication
- **Game Module:** Game-related functionality
- **All:** Special module identifier for operations across all modules

## Development Workflow

1. **Start Development:**
   ```powershell
   # Set up database
   ./development_scripts/runDevFlyway.ps1 -action migrate -module All -createDatabase $true
   
   # Run the application
   dotnet run --project MyCustomApp.API
   ```

2. **Add New Features:**
   - Create commands/queries in the appropriate module
   - Add corresponding handlers
   - Create database migrations if needed

3. **Database Changes:**
   ```powershell
   # Create new migration
   ./development_scripts/newMigrationScript.ps1 -module User -description "Add user preferences"
   
   # Apply migration
   ./development_scripts/runDevFlyway.ps1 -action migrate -module User
   ```

4. **Testing:**
   ```bash
   # Run unit tests
   dotnet test MyCustomApp.UnitTest
   
   # Run integration tests
   dotnet test MyCustomApp.IntegrationTest
   ```

## Benefits of This Architecture

1. **Monolith First Approach:** Start simple, evolve to microservices when needed
2. **Clear Boundaries:** Well-defined module boundaries support future extraction
3. **Independent Evolution:** Modules can evolve independently
4. **Testability:** Comprehensive testing strategy at all levels
5. **Developer Productivity:** Rich tooling and automation for common tasks
6. **Production Ready:** Battle-tested patterns and practices
7. **Database Reliability:** Robust migration strategy with Flyway
8. **Team Scalability:** Multiple teams can work on different modules

This template provides a solid foundation for building modern, scalable applications while maintaining the simplicity of a monolith during early development phases.