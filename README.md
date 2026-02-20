# AutoGen SDK

A modern supply chain consensus platform built with Vue 3 and .NET 9.

## Tech Stack

- **Frontend:** Vue 3 + TypeScript + Vite + Pinia
- **Backend:** .NET 9 + Minimal APIs + Swagger
- **Containerization:** Docker
- **Deployment:** Azure Container Apps / Azure Web Apps

## Project Structure

```
04-supply-consensus/
├── frontend/           # Vue 3 frontend application
│   ├── src/
│   │   ├── components/   # Vue components
│   │   ├── views/        # Page views
│   │   ├── router/       # Vue Router configuration
│   │   ├── stores/       # Pinia stores
│   │   └── App.vue       # Root component
│   ├── package.json
│   ├── vite.config.ts
│   └── tsconfig.json
├── backend/            # .NET 9 backend API
│   ├── Program.cs
│   ├── AutoGen SDK.csproj
│   └── appsettings.json
├── .github/workflows/  # CI/CD pipelines
├── Dockerfile          # Multi-stage Docker build
├── README.md
└── CLAUDE.md
```

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- Docker (optional)

### Backend Development

```bash
cd backend
dotnet restore
dotnet run
```

The API will be available at `http://localhost:8080`

API Endpoints:
- `GET /health` - Health check endpoint
- `GET /api` - API information

### Frontend Development

```bash
cd frontend
npm install
npm start
```

The frontend will be available at `http://localhost:3000`

### Docker Build

```bash
docker build -t supply-consensus .
docker run -p 8080:8080 supply-consensus
```

## Deployment to Azure

### Option 1: Azure Container Apps

1. Create an Azure Container Registry (ACR):
```bash
az acr create --resource-group myResourceGroup --name myContainerRegistry --sku Basic
```

2. Build and push the image:
```bash
az acr build --registry myContainerRegistry --image supply-consensus:latest .
```

3. Create a Container App:
```bash
az containerapp create \
  --name supply-consensus \
  --resource-group myResourceGroup \
  --image myContainerRegistry.azurecr.io/supply-consensus:latest \
  --target-port 8080 \
  --ingress external
```

### Option 2: Azure Web Apps for Containers

1. Create an Azure Web App with container support:
```bash
az webapp create \
  --resource-group myResourceGroup \
  --plan myAppServicePlan \
  --name supply-consensus \
  --deployment-container-image-name myContainerRegistry.azurecr.io/supply-consensus:latest
```

2. Configure continuous deployment from GitHub Actions (see `.github/workflows/ci.yml`)

### Required GitHub Secrets

For automated deployment, add these secrets to your GitHub repository:

- `AZURE_CREDENTIALS` - Azure service principal credentials
- `AZURE_CONTAINER_REGISTRY` - Full name of your ACR (e.g., `myregistry.azurecr.io`)

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/ci.yml`) includes:

1. **Backend Tests** - Build and test .NET application
2. **Frontend Tests** - Lint, type-check, and build Vue application
3. **Docker Build** - Build and cache Docker image
4. **Deploy** - Push to Azure (on main branch only)

## Features

- Vue 3 Composition API with TypeScript
- .NET 9 Minimal APIs
- Health check endpoint
- CORS configuration for local development
- Docker multi-stage build
- GitHub Actions CI/CD
- Azure deployment ready

## License

MIT
