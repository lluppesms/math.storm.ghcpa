// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// ----------------------------------------------------------------------------------------------------
using './main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCEGROUP_LOCATION}#'

param principalId = '#{PRINCIPALID}#'
// param myIpAddress = '#{MYIPADDRESS}#'
param deployCosmos = #{deployCosmos}#

param OpenAI_Endpoint = '#{OPENAI_ENDPOINT}#'
param OpenAI_ApiKey = '#{OPENAI_APIKEY}#'

param servicePlanName = '#{SERVICE_PLAN_NAME}#'
param servicePlanResourceGroupName = '#{SERVICE_PLAN_RESOURCE_GROUP_NAME}#'

// ----------------------------------------------------------------------------------------------------
// Experiment: change this to read environment instead of using Quezta replace
// See: https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/bicep-functions-parameters-file
// ----------------------------------------------------------------------------------------------------
// Conclusion: it works, but you will have to hard-code SET the environment variables in the 
// GitHub Actions workflow, so that makes your template unique and non-reusable...
// ----------------------------------------------------------------------------------------------------
// param appName = readEnvironmentVariable('APP_NAME')
// param environmentCode =  readEnvironmentVariable('ENVIRONMENTCODE')
// param location = readEnvironmentVariable('LOCATION')

// param principalId = readEnvironmentVariable('PRINCIPALID')
// param myIpAddress = readEnvironmentVariable('MYIPADDRESS')
// param deployCosmos = readEnvironmentVariable('deployCosmos') == 'true'
