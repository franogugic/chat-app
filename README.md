# ChatApp - Real-Time Messaging Application

A modern, scalable real-time chat application built with ASP.NET Core and SignalR, following Clean Architecture principles. This application provides secure user authentication, private messaging, and real-time communication features.

## Features

- **User Authentication & Authorization**
  - JWT-based authentication with refresh tokens
  - Secure password hashing using BCrypt
  - Cookie-based token storage for enhanced security

- **Real-Time Messaging**
  - Instant message delivery using SignalR
  - Read receipts and message status tracking
  - Real-time online/offline status updates

- **Private Conversations**
  - One-on-one private messaging
  - Conversation history and message retrieval
  - User search functionality

- **Modern Architecture**
  - Clean Architecture pattern for maintainability
  - Dependency Injection throughout
  - Repository pattern for data access
  - Comprehensive error handling middleware

## Tech Stack

### Backend
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API development
- **SignalR** - Real-time web functionality
- **Entity Framework Core 9** - ORM for database operations
- **PostgreSQL** - Relational database
- **AutoMapper** - Object-to-object mapping

### Security & Authentication
- **JWT Bearer Authentication** - Secure token-based auth
- **BCrypt.Net** - Password hashing

### Development Tools
- **Docker** - Containerization for PostgreSQL
- **xUnit** - Unit and integration testing

## Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
ChatApp/
├── src/
│   ├── ChatApp.Api/              # Presentation Layer
│   │   ├── Controllers/          # API endpoints
│   │   └── Middleware/           # Request/response pipeline
│   ├── ChatApp.Application/      # Application Layer
│   │   ├── Services/             # Business logic
│   │   ├── Interfaces/           # Service contracts
│   │   ├── DTOs/                 # Data transfer objects
│   │   └── Hubs/                 # SignalR hubs
│   ├── ChatApp.Domain/           # Domain Layer
│   │   └── Entities/             # Core business entities
│   └── ChatApp.Infrastructure/   # Infrastructure Layer
│       ├── Repositories/         # Data access
│       ├── Authentication/       # Auth implementation
│       └── Db/                   # Database context
└── tests/
    ├── ChatApp.UnitTests/        # Unit tests
    └── ChatApp.IntegrationTests/ # Integration tests
```

## Prerequisites

Before running this project, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (for PostgreSQL)
- [PostgreSQL](https://www.postgresql.org/download/) (optional if using Docker)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/chat-app.git
cd chat-app
```

### 2. Start PostgreSQL with Docker

```bash
docker-compose up -d
```

This will start a PostgreSQL container with the following credentials:
- **Host:** localhost
- **Port:** 5432
- **Database:** chatAppDb
- **Username:** user
- **Password:** password

### 3. Update Connection String (Optional)

If you're using a different PostgreSQL setup, update the connection string in `src/ChatApp.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=5432;Database=chatAppDb;Username=user;Password=password;"
}
```

### 4. Apply Database Migrations

```bash
cd src/ChatApp.Api
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### 6. Access API Documentation

In development mode, you can access the OpenAPI documentation at:
- `https://localhost:5001/openapi/v1.json`

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Login user | No |
| POST | `/api/auth/refresh` | Refresh access token | No |
| POST | `/api/auth/logout` | Logout user | Yes |
| GET | `/api/auth/{id}` | Get user by ID | Yes |
| GET | `/api/auth/search` | Search users | Yes |

### Conversations

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/conversation` | Get all conversations | Yes |
| POST | `/api/conversation/private` | Create/get private conversation | Yes |

### Messages

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/message/{conversationId}` | Get messages by conversation | Yes |
| POST | `/api/message` | Send a message | Yes |

### SignalR Hub

| Hub | Endpoint | Description |
|-----|----------|-------------|
| ChatHub | `/chatHub` | Real-time messaging and status updates |

## Configuration

### JWT Settings

Configure JWT settings in `appsettings.json`:

```json
"Jwt": {
  "Key": "YourSuperSecretKeyForChatAppIsHere",
  "Issuer": "ChatApp",
  "Audience": "ChatAppUsers",
  "ExpireMinutes": 5,
  "RefreshTokenExpireDays": 7
}
```

**Important:** Change the `Key` value in production to a secure random string.

### CORS Configuration

The API is configured to accept requests from `http://localhost:5173` (Vite default port). Update the CORS policy in `Program.cs` if your frontend runs on a different port:

```csharp
policy.WithOrigins("http://localhost:YOUR_PORT")
```

## Running Tests

### Unit Tests

```bash
cd tests/ChatApp.UnitTests
dotnet test
```

### Integration Tests

```bash
cd tests/ChatApp.IntegrationTests
dotnet test
```

### Run All Tests

```bash
dotnet test
```

## Real-Time Features with SignalR

The application uses SignalR for real-time communication. Key features include:

- **Message Broadcasting:** Messages are instantly delivered to all participants
- **Read Receipts:** Blue checkmarks indicate when messages are read
- **Online Status:** Real-time user presence tracking
- **Typing Indicators:** (Can be extended)

### Connecting to SignalR Hub

The SignalR hub is available at `/chatHub`. Authentication is required via:
- Access token in cookies (`access-token`)
- Query string parameter (`?access_token=...`)

## Database Schema

### Main Entities

- **User:** User account information
- **Conversation:** Chat conversations (private/group)
- **Message:** Individual messages
- **ConversationParticipant:** Links users to conversations
- **RefreshToken:** Token management for authentication

## Error Handling

The application includes global exception middleware that:
- Catches unhandled exceptions
- Returns consistent error responses
- Logs errors for debugging
- Provides appropriate HTTP status codes

## Security Features

- **Password Hashing:** BCrypt with work factor 12
- **JWT Tokens:** Short-lived access tokens (5 minutes)
- **Refresh Tokens:** Long-lived tokens (7 days) for renewed access
- **HTTP-Only Cookies:** Prevents XSS attacks
- **CORS Protection:** Restricts cross-origin requests
- **Authorization:** Protected endpoints require valid JWT

## Development

### Project Structure Highlights

- **Clean Architecture:** Clear separation between layers
- **Dependency Injection:** All services registered in IoC container
- **Repository Pattern:** Abstraction over data access
- **DTO Pattern:** Separate models for API contracts
- **Async/Await:** Non-blocking I/O operations throughout

### Adding New Features

1. **Domain Layer:** Add entity models in `ChatApp.Domain/Entities`
2. **Application Layer:** Create services and interfaces
3. **Infrastructure Layer:** Implement repositories and data access
4. **API Layer:** Add controllers and endpoints

## Future Enhancements

- [ ] Group chat functionality
- [ ] File and image sharing
- [ ] Message editing and deletion
- [ ] Typing indicators
- [ ] Message reactions
- [ ] Push notifications
- [ ] User profiles and avatars
- [ ] Message search functionality
- [ ] Voice and video calling

## Troubleshooting

### Database Connection Issues

If you encounter database connection errors:
1. Ensure PostgreSQL is running: `docker ps`
2. Check connection string in `appsettings.json`
3. Verify database exists: `docker exec -it my_postgres psql -U user -d chatAppDb`

### Migration Issues

If migrations fail:
```bash
# Remove existing migrations
dotnet ef migrations remove

# Create new migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

### SignalR Connection Issues

If real-time features don't work:
1. Verify JWT token is valid
2. Check CORS settings allow credentials
3. Ensure SignalR client uses correct hub URL


