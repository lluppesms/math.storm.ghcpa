// ----------------------------------------------------------------------------------------------------
// Bicep Parameter File
// ----------------------------------------------------------------------------------------------------
using './main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCEGROUP_LOCATION}#'

param principalId = '#{PRINCIPALID}#'
param deploySqlServer = #{deploySqlServer}#

param sqlAdminUser = '#{SQL_ADMIN_USER}#'
param sqlAdminPassword = '#{SQL_ADMIN_PASSWORD}#'

param OpenAI_Endpoint = '#{OPENAI_ENDPOINT}#'
param OpenAI_ApiKey = '#{OPENAI_APIKEY}#'

param servicePlanName = '#{EXISTING_SERVICEPLAN_NAME}#'
param servicePlanResourceGroupName = '#{EXISTING_SERVICEPLAN_RESOURCEGROUP_NAME}#'

