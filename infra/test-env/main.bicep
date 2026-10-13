// Entra identity used by the integration-tests workflow (#3193).
// No secret or certificate: GitHub Actions signs in with OIDC, and only jobs running in the listed
// GitHub environments of this repository are trusted. Deploy with Setup-TestEnvironment.ps1.
extension microsoftGraphV1

@description('Stable unique name of the Entra application.')
param appUniqueName string = 'adomt-integration-tests'

@description('GitHub repository, owner/name.')
param repository string = 'nkdAgility/azure-devops-migration-tools'

@description('GitHub environments whose jobs may sign in as this identity.')
param environments array = [
  'integration-tests'
  'integration-tests-forks'
]

resource app 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: appUniqueName
  displayName: 'Azure DevOps Migration Tools - integration tests'
  signInAudience: 'AzureADMyOrg'
}

resource federatedCredentials 'Microsoft.Graph/applications/federatedIdentityCredentials@v1.0' = [for env in environments: {
  name: '${app.uniqueName}/github-${env}'
  issuer: 'https://token.actions.githubusercontent.com'
  subject: 'repo:${repository}:environment:${env}'
  audiences: [
    'api://AzureADTokenExchange'
  ]
  description: 'GitHub Actions jobs in the ${env} environment of ${repository}'
}]

resource servicePrincipal 'Microsoft.Graph/servicePrincipals@v1.0' = {
  appId: app.appId
}

output clientId string = app.appId
output servicePrincipalObjectId string = servicePrincipal.id
output tenantId string = tenant().tenantId
