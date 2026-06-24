# Set up GitHub Secrets

The GitHub workflows in this project require several secrets set at the repository level.

---

## Azure Resource Creation Credentials

You need to set up the Azure Credentials secret in the GitHub Secrets at the Repository level before you do anything else.

See [https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions) for more info.

To create these secrets, customize and run this command::

``` bash
gh auth login

gh secret set AZURE_CLIENT_ID -b <GUID>
gh secret set AZURE_CLIENT_SECRET -b <GUID>
gh secret set AZURE_TENANT_ID -b <GUID>
gh secret set AZURE_SUBSCRIPTION_ID -b <yourAzureSubscriptionId>
```

---

## SQL Server Secrets and Variables (NEW — replaces Cosmos DB)

This project has migrated from Azure Cosmos DB to Azure SQL Server. If you previously had Cosmos DB secrets, they are no longer needed and can be removed.

### Secrets to REMOVE (if upgrading from Cosmos DB)

If you had any of the following Cosmos DB secrets, they can be safely deleted:

``` bash
# These are no longer used — remove them if present:
gh secret delete COSMOS_CONNECTION_STRING
gh secret delete COSMOS_KEY
gh secret delete COSMOS_ENDPOINT
```

### New Secrets to ADD for SQL Server

These secrets are required for Bicep infrastructure deployment and for deploying the SQL DACPAC.  
Set them at the **environment level** (e.g. `dev`, `qa`, `prod`) so each environment can use its own database.

```bash
# SQL Server admin password — used by Bicep when creating the SQL Server
# (substituted for the #{SQL_ADMIN_PASSWORD}# token in main.gha.bicepparam)
gh secret set --env <ENV-NAME> SQL_ADMIN_PASSWORD -b <yourStrongPassword>

# Full ADO.NET connection string — used by the DACPAC deploy workflow to publish the schema.
# Example (SQL auth):
#   "Server=tcp:<server>.database.windows.net,1433;Initial Catalog=MathStormDB-dev;User Id=<adminUser>;******;Encrypt=True;"
# Example (Managed Identity — recommended for production):
#   "Server=tcp:<server>.database.windows.net,1433;Initial Catalog=MathStormDB-dev;Authentication=Active Directory Default;Encrypt=True;"
gh secret set --env <ENV-NAME> SQL_CONNECTION_STRING -b "<yourConnectionString>"
```

### New Variables to ADD for SQL Server

These variables are substituted into the Bicep parameter file during deployment.

```bash
# SQL Server admin username — substituted for the #{SQL_ADMIN_USER}# token in main.gha.bicepparam
gh variable set SQL_ADMIN_USER -b sqladmin

# Whether to deploy SQL Server via Bicep — set to true to provision the server
gh variable set deploySqlServer -b true
```

---

## Bicep Configuration Values

These variables and secrets are used by the Bicep templates to configure the resource names that are deployed.  Make sure the App_Name variable is unique to your deploy. It will be used as the basis for the website name and for all the other Azure resources, which must be globally unique.
To create these additional secrets and variables, customize and run this command:

Secret Values:

``` bash
gh auth login

gh variable set APP_NAME -b lll-mathstorm
gh variable set RESOURCEGROUP_LOCATION -b eastus
gh variable set RESOURCEGROUP_PREFIX -b rg_mathstorm-webg 

gh variable set APP_PROJECT_FOLDER_NAME -b src/web/MathStorm/MathStorm.Web
gh variable set APP_PROJECT_NAME -b MathStorm.Web
gh variable set APP_TEST_FOLDER_NAME -b src/web/MathStorm.Web.Tests/MathStorm.Web.Tests
gh variable set APP_TEST_PROJECT_NAME -b MathStorm.Web.Tests

gh variable set FUNC_PROJECT_FOLDER_NAME -b src/functions/MathStorm.Functions
gh variable set FUNC_PROJECT_NAME -b MathStorm.Functions
gh variable set FUNC_TEST_FOLDER_NAME -b src/functions/MathStorm.Functions.Tests
gh variable set FUNC_TEST_PROJECT_NAME -b MathStorm.Functions.Tests
```

---

## References

[Deploying ARM Templates with GitHub Actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions)

[GitHub Secrets CLI](https://cli.github.com/manual/gh_secret_set)
