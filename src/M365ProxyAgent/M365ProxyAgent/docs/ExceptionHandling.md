# Exception Handling Guide

## Overview

The M365 Proxy Agent implements a comprehensive exception handling system with domain-specific exceptions and centralized error handling middleware.

## Exception Hierarchy

### Base Exception: `ProxyAgentException`
- **Purpose**: Base class for all domain-specific exceptions
- **Features**: Correlation ID tracking, context information, structured logging
- **Usage**: Abstract base class - not thrown directly

### Domain-Specific Exceptions

#### 1. `AuthenticationException`
- **When to use**: Authentication failures, token acquisition errors, authorization issues
- **HTTP Status**: 401 Unauthorized
- **Context**: Authentication method, tenant ID, correlation ID
- **Example**:
  ```csharp
  throw new AuthenticationException(
      "Token acquisition failed",
      "ClientCredentials",
      "tenant-id",
      correlationId);
  ```

#### 2. `ConfigurationException`
- **When to use**: Configuration validation failures, missing settings, invalid configuration
- **HTTP Status**: 500 Internal Server Error
- **Context**: Configuration section, validation errors
- **Example**:
  ```csharp
  throw new ConfigurationException(
      "Invalid configuration detected",
      "CopilotStudioClientSettings",
      validationErrors,
      correlationId);
  ```

#### 3. `CopilotClientException`
- **When to use**: Copilot Studio service communication errors, API failures
- **HTTP Status**: 502 Bad Gateway
- **Context**: Request details, response status, error codes
- **Example**:
  ```csharp
  throw new CopilotClientException(
      "Failed to communicate with Copilot Studio",
      requestId,
      HttpStatusCode.ServiceUnavailable,
      correlationId);
  ```

#### 4. `ValidationException`
- **When to use**: Input validation failures, business rule violations, data validation errors
- **HTTP Status**: 400 Bad Request
- **Context**: Field names, invalid values, validation errors
- **Example**:
  ```csharp
  throw new ValidationException(
      "Invalid request data",
      validationErrors,
      correlationId);
  ```

## Exception Handling Middleware

### `ExceptionHandlingMiddleware`
- **Purpose**: Centralized exception handling and error response generation
- **Registration**: Automatically registered via `UseExceptionHandling()` extension method
- **Features**:
  - Converts exceptions to appropriate HTTP responses
  - Logs all unhandled exceptions with correlation IDs
  - Provides consistent error response format
  - Handles both domain-specific and system exceptions

### Error Response Format
```json
{
  "error": "ValidationError",
  "message": "Input validation failed",
  "correlationId": "12345678-1234-1234-1234-123456789012",
  "details": [
    "Field 'email' is required",
    "Field 'name' must be at least 2 characters"
  ],
  "context": {
    "fieldName": "email",
    "invalidValue": "",
    "errorType": "FieldValidationError"
  },
  "timestamp": "2025-07-26T10:30:00.000Z"
}
```

## Usage Guidelines

### 1. Creating Custom Exceptions
```csharp
// Use correlation service for tracking
var correlationId = correlationService.GetCorrelationId();

// Throw domain-specific exception with context
throw new AuthenticationException(
    "Token validation failed",
    "Bearer",
    tenantId,
    correlationId,
    innerException);
```

### 2. Exception Context
Always provide meaningful context information:
- **Correlation IDs**: For request tracking
- **Context dictionaries**: For additional error details
- **Inner exceptions**: For root cause analysis

### 3. Logging Integration
Exceptions are automatically logged by the middleware with:
- Full exception details and stack traces
- Correlation IDs for request tracking
- Request path and HTTP method
- Structured logging format

### 4. Testing Exception Handling
```csharp
[Test]
public async Task Should_Return_BadRequest_For_ValidationException()
{
    // Arrange
    var validationErrors = new[] { "Field 'name' is required" };
    var exception = new ValidationException("Validation failed", validationErrors);
    
    // Act & Assert
    // Test that middleware converts to 400 Bad Request
}
```

## Best Practices

1. **Use Specific Exceptions**: Always throw the most specific exception type available
2. **Include Context**: Provide relevant context information for debugging
3. **Correlation IDs**: Always include correlation IDs for request tracking
4. **Don't Expose Internals**: Avoid exposing sensitive information in error messages
5. **Log Appropriately**: Let the middleware handle logging - don't double-log exceptions
6. **Handle Inner Exceptions**: Include inner exceptions for better root cause analysis

## HTTP Status Code Mapping

| Exception Type | HTTP Status | Description |
|---|---|---|
| `ValidationException` | 400 Bad Request | Input validation failures |
| `AuthenticationException` | 401 Unauthorized | Authentication/authorization failures |
| `ConfigurationException` | 500 Internal Server Error | Configuration issues |
| `CopilotClientException` | 502 Bad Gateway | External service communication failures |
| `ProxyAgentException` | 500 Internal Server Error | General application errors |
| `TimeoutException` | 408 Request Timeout | Operation timeout |
| `Unknown Exception` | 500 Internal Server Error | Unexpected system errors |

## Monitoring and Alerting

The exception handling system provides structured logging that can be used for:
- **Error Rate Monitoring**: Track exception frequency by type
- **Performance Monitoring**: Identify slow operations and timeouts
- **Business Intelligence**: Analyze validation failures and user patterns
- **Incident Response**: Correlation ID-based request tracing
