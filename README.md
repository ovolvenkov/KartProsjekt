# KartProsjekt

A Windows desktop application for project management, built with C#, .NET 10, and WinUI 3 using the MVVM architecture.

The project is designed for learning and exploring technologies for future KOS application development.

## Features

- Dashboard and project management.
- ListView and experimental Microsoft TableView.
- Sorting, filtering, grouping, and inline editing.
- SQL Server integration.
- Test data generation.

## Technology Stack

- C# / .NET 10
- WinUI 3 / XAML
- Windows App SDK 2.5.4-experimental
- CommunityToolkit.Mvvm
- Entity Framework Core
- Dependency Injection
- SQL Server 2022
- Docker / Docker Compose

## Prerequisites

- Windows 11
- Visual Studio with WinUI development components
- .NET SDK 10 or compatible newer version
- Docker Desktop with WSL 2
- SQL Server Management Studio (optional)

## Getting Started

### 1. Configure Docker

Create a `.env` file in the repository root:

```env
MSSQL_SA_PASSWORD=YourStrongPassword123!
```

Start Docker Desktop and run:

```powershell
docker compose up -d
```

### 2. Configure Database Connection

Set the connection string in PowerShell:

```powershell
[Environment]::SetEnvironmentVariable(
    "KARTPROSJEKT_CONNECTION_STRING",
    "Server=127.0.0.1,1433;Database=KartProsjektDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;Encrypt=True;",
    "User"
)
```

Use the same password as in `.env`.

Restart Visual Studio and PowerShell.

### 3. Restore Dependencies and Database

```powershell
cd src
dotnet restore
dotnet tool install --global dotnet-ef --version 10.0.15
dotnet ef database update
dotnet build
```

Skip the tool installation if the required version is already installed.

### 4. Run the Application

Open the project in Visual Studio, select **KartProsjekt (Unpackaged)** and press **F5**.

The application uses self-contained deployment and does not require Windows Developer Mode.

## Project Structure

- `Models` — Data models.
- `Views` — XAML pages and dialogs.
- `ViewModels` — Application state and presentation logic.
- `Services` — Application and database operations.
- `Data` — Entity Framework Core configuration.
- `Migrations` — Database migrations.

## Documentation

- [WinUI 3](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [Microsoft TableView](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/table-view)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Docker](https://docs.docker.com/)

## Notes

- NuGet dependencies are restored automatically.
- Database data is persisted in a Docker volume.
- Never commit `.env` or database credentials.
- The project is under active development.