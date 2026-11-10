# RaceDay Platform (PROG6212 Part 3)

A full-stack application with an ASP.NET Core Web API and an MVC frontend.

## Architecture
- **RaceDay.API**: RESTful API with JWT Auth, EF Core, and Azure Blob Storage.
- **RaceDay.MVC**: MVC frontend consuming the API via HttpClient. Uses session management.
- **RaceDay.API.Tests**: xUnit unit tests.

## Run Locally
1. Open `RaceDay.sln` in Visual Studio.
2. Set both `RaceDay.API` and `RaceDay.MVC` as startup projects.
3. Press `F5`.

## Run with Docker
1. Ensure Docker Desktop is running.
2. From the root directory, run:
   ```bash
   docker-compose up --build
   Access the MVC app at http://localhost:7219

Access the API at http://localhost:7201/swagger