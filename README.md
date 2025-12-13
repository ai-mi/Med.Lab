# Med.Labs — README

Generated: 2025-12-13T16:16:23Z

This README describes how to build, test, and run the Med.Labs solution locally and with Docker.

Prerequisites
- .NET SDK 8.0+ installed: https://dotnet.microsoft.com/download
- Docker & Docker Compose (for container workflows): https://www.docker.com/get-started

Repository layout
- Solution: Med.Labs.slnx (root)
- Source: src\
- Tests: tests\
- Docker Compose: docker-compose.yml
- CI workflow: .github\workflows\dotnet.yml

Build (local)
1. Open a developer command prompt and go to repository root:
   cd C:\\Work\\Test\\Med.Labs
2. Restore and build the solution:
   dotnet restore "Med.Labs.slnx"
   dotnet build "Med.Labs.slnx" --no-restore -c Release

Test (local)
1. Run unit and integration tests:
   dotnet test "Med.Labs.slnx" --no-build -v minimal
2. To run a specific test project, provide its path or project name:
   dotnet test tests\\MyIntegrationTests\\MyIntegrationTests.csproj

Run locally (dotnet run)
1. From a service project folder (example):
   cd src\\MyApi
   dotnet run --urls "http://localhost:5000"
2. Open http://localhost:5000 (or the configured port) in your browser or API client.

Run with Docker Compose
The repository includes a docker-compose.yml for local containerized runs.

1. Build and start containers (in background):
   docker-compose up --build -d
2. Watch logs:
   docker-compose logs -f
3. Stop and remove containers:
   docker-compose down

Build and run a single container (Docker)
If a Dockerfile exists for a service, build and run it directly:

1. Build image (from repository root or service folder):
   docker build -t medlabs:local .
2. Run container, mapping ports as needed (example):
   docker run --rm -p 8080:80 --name medlabs medlabs:local

Notes for CI (GitHub Actions)
- The CI workflow is located at .github/workflows/dotnet.yml and currently targets GitHub-hosted runners.
- Ensure the workflow's branch filters (main/master) match your repository default branch.
- The workflow restores, builds, and tests the solution using `dotnet` on the runner.

Troubleshooting
- If `dotnet test` fails locally but works in CI, verify OS-specific dependencies and PowerShell Core (pwsh) availability if scripts rely on it.
- For Docker-related issues, check that Docker Desktop is running and resources (memory/CPU) are sufficient.

Further improvements
- Add service-specific Dockerfiles and example docker-compose.override.yml for local development.
- Document environment variables and secrets handling (e.g., via .env or secret manager).

