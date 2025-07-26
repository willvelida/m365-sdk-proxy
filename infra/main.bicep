/*
  PARAMETERS
*/
@description('The name of the Container App')
param containerAppName string

@description('Location for all resources')
param location string

@description('The name of the Container App Environment')
param containerAppEnvName string

@description('The name of the Azure Container Registry')
param containerRegistryName string

@description('The Docker Image that this Container App will use')
param imageName string

@description('The name of the Application Insights workspace')
param appInsightsName string

@description('The Bot Service name')
param botServiceName string

/*
  EXISTING RESOURCES
*/
resource env 'Microsoft.App/managedEnvironments@2025-01-01' existing = {
  name: containerAppEnvName
}

resource acr 'Microsoft.ContainerRegistry/registries@2025-04-01' existing = {
  name: containerRegistryName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

/*
  VARIABLES
*/
var registryPasswordRef = 'registry-password'
var appClientSecret = ''
var copilotClientSecret = ''

resource proxyAgent 'Microsoft.App/containerApps@2025-01-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: env.id
    configuration: {
      activeRevisionsMode: 'Single'
      secrets: [
        {
          name: registryPasswordRef
          value: acr.listCredentials().passwords[0].value
        }
        {
          name: 'client-secret'
          value: appClientSecret
        }
        {
          name: 'copilot-client-secret'
          value: copilotClientSecret
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: acr.properties.loginServer
          username: acr.listCredentials().username
          passwordSecretRef: registryPasswordRef
        }
      ]
    }
    template: {
      containers: [
        {
          name: containerAppName
          image: imageName
                    env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString
            }
            {
              name: 'TokenValidation__Audiences__0'
              value: '<bot_client_id>'
            }
            {
              name: 'Connections__ServiceConnection__Settings__AuthType'
              value: 'ClientSecret'
            }
            {
              name: 'Connections__ServiceConnection__Settings__AuthorityEndpoint'
              value: 'https://login.microsoftonline.com/${subscription().tenantId}'
            }
            {
              name: 'Connections__ServiceConnection__Settings__ClientId'
              value: '<bot_client_id>'
            }
            {
              name: 'Connections__ServiceConnection__Settings__ClientSecret'
              secretRef: 'client-secret'
            }
            {
              name: 'Connections__ServiceConnection__Settings__Scopes__0'
              value: 'https://api.botframework.com/.default'
            }
            {
              name: 'ConnectionsMap__0__ServiceUrl'
              value: '*'
            }
            {
              name: 'ConnectionsMap__0__Connection'
              value: 'ServiceConnection'
            }
            {
              name: 'CopilotStudioClientSettings__TenantId'
              value: subscription().tenantId
            }
            {
              name: 'CopilotStudioClientSettings__AppClientId'
              value: '<copilot_studio_app_client>'
            }
            {
              name: 'CopilotStudioClientSettings__SchemaName'
              value: '<nlu_bot>'
            }
            {
              name: 'CopilotStudioClientSettings__EnvironmentId'
              value: '<copilot_studio_environment_id>'
            }
            {
              name: 'CopilotStudioClientSettings__AppClientSecret'
              secretRef: 'copilot-client-secret'
            }
            {
              name: 'CopilotStudioClientSettings__UseS2SConnection'
              value: string(true)
            }           
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
        rules: [
          {
            name: 'http-rule'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

resource botService 'Microsoft.BotService/botServices@2023-09-15-preview' = {
  name: botServiceName
  kind: 'azurebot'
  location: 'global'
  properties: {
    displayName: botServiceName
    endpoint: 'https://${proxyAgent.properties.configuration.ingress.fqdn}/api/messages'
    msaAppId: '<bot_client_id>'
    msaAppTenantId: subscription().tenantId
    msaAppType: 'SingleTenant'
    developerAppInsightKey: appInsights.properties.InstrumentationKey
  }
  sku: {
    name: 'S1'
  }
}
