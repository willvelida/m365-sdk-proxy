# M365 SDK Proxy

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Supported-2496ED?style=flat-square&logo=docker)](https://www.docker.com/)
[![Azure](https://img.shields.io/badge/Azure-Container%20Apps-0078D4?style=flat-square&logo=microsoftazure)](https://azure.microsoft.com/en-us/products/container-apps)
[![License](https://img.shields.io/github/license/willvelida/m365-sdk-proxy?style=flat-square)](LICENSE)

A high-performance .NET 8 proxy agent that facilitates secure communication between Bot Framework activities and Microsoft Copilot Studio. Built with enterprise-grade patterns including comprehensive error handling, structured logging, resilience patterns, and cloud-native deployment support.

## 🏗️ Architecture Overview

The M365 SDK Proxy follows clean architecture principles with a modular design that separates concerns and ensures maintainability:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Bot Framework │───▶│  M365 SDK Proxy │───▶│ Copilot Studio  │
│    Activities   │    │     Agent       │    │    Service      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Core Components

- **ProxyAgent**: Main orchestrator implementing message handler factory pattern
- **ConversationService**: Manages Copilot Studio communication lifecycle with streaming responses
- **Authentication**: Server-to-Server token handling using MSAL with secure token caching
- **Exception Handling**: Comprehensive middleware with structured error responses and correlation tracking
- **Configuration**: Strongly-typed settings with FluentValidation
- **Resilience**: Built-in retry policies, circuit breakers, and timeout management

### Key Features

- ✅ **Enterprise Security**: S2S authentication with Azure AD and secure secret management
- ✅ **High Performance**: Async streaming responses with efficient resource utilization
- ✅ **Observability**: Structured logging, correlation IDs, and Azure Application Insights integration
- ✅ **Resilience**: Circuit breakers, retry policies, and graceful degradation
- ✅ **Cloud Native**: Container-ready with Azure Container Apps deployment
- ✅ **Testability**: Comprehensive unit test coverage with builder patterns

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerization)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)
- Azure subscription with appropriate permissions

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/willvelida/m365-sdk-proxy.git
   cd m365-sdk-proxy
   ```

2. **Configure application settings**
   
   Copy the example configuration and update with your values:
   ```bash
   cd src/M365ProxyAgent/M365ProxyAgent
   cp appsettings.Example.json appsettings.Development.json
   ```

3. **Update configuration values**
   
   Edit `appsettings.Development.json`:
   ```json
   {
     "AgentApplicationOptions": {
       "StartTypingTimer": true,
       "RemoveRecipientMention": false,
       "NormalizeMentions": false
     },
     "connectionString": "YOUR_APPLICATION_INSIGHTS_CONNECTION_STRING",
     "TokenValidation": {
       "Audiences": ["YOUR_BOT_CLIENT_ID"]
     },
     "Connections": {
       "ServiceConnection": {
         "Settings": {
           "AuthType": "ClientSecret",
           "AuthorityEndpoint": "https://login.microsoftonline.com/YOUR_TENANT_ID",
           "ClientId": "YOUR_BOT_CLIENT_ID",
           "ClientSecret": "YOUR_CLIENT_SECRET",
           "Scopes": ["https://api.botframework.com/.default"]
         }
       }
     },
     "ConnectionsMap": [
       {
         "ServiceUrl": "*",
         "Connection": "ServiceConnection"
       }
     ],
     "CopilotStudioClientSettings": {
       "TenantId": "YOUR_TENANT_ID",
       "AppClientId": "YOUR_COPILOT_STUDIO_APP_CLIENT_ID",
       "SchemaName": "YOUR_NLU_BOT_SCHEMA",
       "EnvironmentId": "YOUR_COPILOT_STUDIO_ENVIRONMENT_ID",
       "AppClientSecret": "YOUR_COPILOT_STUDIO_CLIENT_SECRET",
       "UseS2SConnection": true
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/M365ProxyAgent/M365ProxyAgent
   ```

   The API will be available at `https://localhost:7071` and `http://localhost:5071`.

### Running with Docker

1. **Build the Docker image**
   ```bash
   cd src/M365ProxyAgent
   docker build -t m365-proxy-agent .
   ```

2. **Run the container**
   ```bash
   docker run -p 8080:8080 \
     -e ConnectionString="YOUR_APP_INSIGHTS_CONNECTION_STRING" \
     -e CopilotStudioClientSettings__TenantId="YOUR_TENANT_ID" \
     -e CopilotStudioClientSettings__AppClientId="YOUR_APP_CLIENT_ID" \
     -e CopilotStudioClientSettings__AppClientSecret="YOUR_CLIENT_SECRET" \
     m365-proxy-agent
   ```

## ⚙️ Configuration

### Required Configuration Sections

| Section | Description |
|---------|-------------|
| `AgentApplicationOptions` | Bot Framework agent configuration |
| `connectionString` | Application Insights connection string |
| `TokenValidation` | JWT token validation settings |
| `Connections` | Bot Framework connection settings |
| `ConnectionsMap` | Service URL to connection mapping |
| `CopilotStudioClientSettings` | Copilot Studio integration settings |

### Environment Variables

For production deployments, use environment variables:

```bash
# Application Insights
APPLICATIONINSIGHTS_CONNECTION_STRING=your_connection_string

# Bot Framework Authentication
TokenValidation__Audiences__0=your_bot_client_id
Connections__ServiceConnection__Settings__ClientId=your_bot_client_id
Connections__ServiceConnection__Settings__ClientSecret=your_client_secret

# Copilot Studio Settings
CopilotStudioClientSettings__TenantId=your_tenant_id
CopilotStudioClientSettings__AppClientId=your_copilot_app_id
CopilotStudioClientSettings__AppClientSecret=your_copilot_secret
CopilotStudioClientSettings__EnvironmentId=your_environment_id
CopilotStudioClientSettings__SchemaName=your_schema_name
```

### Configuration Validation

The application includes comprehensive configuration validation that runs at startup. Invalid configurations will prevent the application from starting with detailed error messages.

## 🌐 API Reference

### Health Check Endpoint

```http
GET /
```

Returns a simple health check response.

**Response:**
```
M365 Agent Proxy
```

### Bot Framework Messages Endpoint

```http
POST /api/messages
```

Processes Bot Framework activities and proxies them to Copilot Studio.

**Headers:**
- `Authorization: Bearer <token>` - Required JWT token
- `Content-Type: application/json`

**Request Body:**
Bot Framework Activity object

**Response:**
Stream of Bot Framework Activity responses from Copilot Studio

## 🏗️ Deployment

### Azure Container Apps Deployment

The project includes Infrastructure as Code using Bicep templates for Azure deployment.

### Infrastructure Components

The Bicep template deploys:
- **Azure Container App**: Hosts the proxy agent with auto-scaling
- **Bot Service**: Registers the bot with Azure Bot Framework
- **Application Insights**: Monitoring and telemetry
- **Container Registry Integration**: Secure image deployment

## 🧪 Testing

### Running Unit Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in watch mode
dotnet watch test --project tests/M365ProxyAgent.UnitTests
```

### Test Architecture

The test suite uses:
- **xUnit**: Testing framework
- **FluentAssertions**: Fluent assertion library
- **Moq**: Mocking framework
- **Builder Pattern**: Test data creation with `CopilotSettingsBuilder`, `ActivityBuilder`, etc.

### Test Coverage Areas

- ✅ Agent orchestration and message handling
- ✅ Conversation service with Copilot Studio integration
- ✅ Authentication and token management
- ✅ Exception handling middleware
- ✅ Configuration validation
- ✅ Resilience patterns

## 🔍 Monitoring and Observability

### Structured Logging

The application implements comprehensive structured logging with:
- **Correlation IDs**: Track requests across service boundaries
- **Performance Metrics**: Request duration and activity counts
- **Error Context**: Detailed error information with correlation tracking

### Application Insights Integration

- **Request Telemetry**: HTTP request tracking
- **Dependency Telemetry**: External service calls
- **Exception Telemetry**: Unhandled exceptions
- **Custom Metrics**: Business-specific metrics

### Health Monitoring

Monitor these key metrics:
- Response times for `/api/messages` endpoint
- Copilot Studio service availability
- Authentication success rates
- Exception rates by type

## 🛠️ Development Guidelines

### Code Style

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use nullable reference types
- Implement comprehensive XML documentation
- Follow SOLID principles

### Error Handling

- Use custom exception types with context information
- Implement correlation ID tracking
- Provide structured error responses
- Log errors with appropriate levels

### Adding New Features

1. Create feature branch from `main`
2. Implement with unit tests (aim for >90% coverage)
3. Update documentation
4. Create pull request with detailed description

