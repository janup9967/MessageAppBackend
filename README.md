# MessageApp Backend

This is the backend service for **MessageApp**, a real-time chat application built with:

- **ASP.NET Core Web API**  
- **Entity Framework Core**  
- **SignalR**  

It provides authentication (JWT), user management, conversation management, and real-time messaging.

---

## 🚀 Features

- **User Authentication**  
  JWT-based login & registration

- **Conversations API**  
  Create, fetch, and list conversations

- **Messages API**  
  Send, fetch, and mark messages read/unread

- **Real-time communication**  
  Powered by **SignalR**

- **Entity Framework Core**  
  For database access (SQL Server)

- **Repository Pattern**  
  Implements clean architecture

---

## 🛠️ Tech Stack

- **.NET 6 / .NET 7** (Web API)  
- **Entity Framework Core**  
- **SQL Server**  
- **SignalR** (real-time messaging)  
- **JWT Authentication**  
- **Swagger** (API documentation)

---

## 🧰 Tools Required

Before running the project, ensure you have the following installed:

- **Visual Studio 2022 / VS Code** (with C# extension)  
- **.NET SDK 6.0 / 7.0**  
- **SQL Server** (LocalDB or full version)  
- **Postman / Swagger UI** (for API testing)  
- **Git** (for cloning the repository)  
- **EF Core CLI tools** (`dotnet ef` for migrations)  

---

## 📂 Project Structure 
  
 
 

```

MessageAppBackend/
│── Controllers/       # API Controllers (Auth, Conversations, Messages)
│── Models/            # Entity Models (User, Conversation, Message)
│── Dtos/              # Data Transfer Objects
│── Repositories/      # Repository Pattern Implementations
│── Hubs/              # SignalR Hub (MessageHub)
│── Services/          # Business logic
│── appsettings.json   # Configurations (DB, JWT, etc.)
│── Program.cs         # Application entry point

```

---

## ⚙️ Setup & Installation

### 1. Clone the Repository



```bash

git clone https://bitbucket.org/anupjais1123/messageappbackend.git
cd messageappbackend

```

### 2. Configure Database

Update `appsettings.json` with your SQL Server connection string:

```json

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MessageAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

```

### 3. Apply Migrations

```bash

dotnet ef database update

```

### 4. Run the Application

```bash

dotnet run

```

The API will start on:  

- `https://localhost:5080`  
- `http://localhost:5080`

---

## 🔑 Authentication

- JWT tokens are used for authentication.  
- Add your JWT key inside `appsettings.json`:

```json
"Jwt": {
  "Key": "your_secret_key_here",
  "Issuer": "MessageAppIssuer",
  "Audience": "MessageAppAudience"
}
```

- Use the token in API requests:

```
http Authorization: Bearer <your_token_here>

```

---

## 📡 API Endpoints

### Authentication

- `POST /api/Auth/register` → Register a new user  
- `POST /api/Auth/login` → Login and get JWT token  

### Conversations

- `GET /api/Conversations/All-Conversations` → Get all conversations for a user  
- `POST /api/Conversations/create` → Create a new conversation  

### Messages

- `GET /api/Messages/{conversationId}` → Fetch messages  
- `POST /api/Messages/send` → Send a new message  

---

## 📖 Swagger Dashboard

Swagger UI is enabled for testing and exploring the APIs.  

Once the app is running, open:

- `https://localhost:5080/swagger/index.html`  
- `http://localhost:5080/swagger/index.html`

With Swagger you can:

- View all endpoints  
- Send requests directly from the browser  
- Test authentication by adding JWT tokens in the **Authorize** button

---

## 🔔 SignalR

The backend provides a **SignalR hub** at:  

- `/messageHub`

Clients can connect for:

- Receiving new messages instantly  
- Updating read/unread status  
- Real-time chat sync

---

## 📦 Packages Used

| Package | Version | Purpose |
| ------- | ------- | ------- |
| **BCrypt.Net-Next** | 4.0.3 | Password hashing & verification |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 9.0.8 | JWT authentication middleware |
| **Microsoft.AspNetCore.OpenApi** | 9.0.7 | OpenAPI (Swagger) support |
| **Microsoft.AspNetCore.SignalR** | 1.2.0 | Real-time communication |
| **Microsoft.EntityFrameworkCore** | 9.0.8 | ORM for database access |
| **Microsoft.EntityFrameworkCore.SqlServer** | 9.0.8 | SQL Server provider for EF Core |
| **Microsoft.EntityFrameworkCore.Tools** | 9.0.8 | EF Core migrations & scaffolding |
| **Serilog.AspNetCore** | 9.0.0 | Logging integration for ASP.NET Core |
| **Serilog.Sinks.File** | 7.0.0 | File-based logging |
| **Swashbuckle.AspNetCore** | 9.0.3 | Swagger UI & API documentation |

---

## 📜 License

This project is private but can be used for **learning and development purposes**.
