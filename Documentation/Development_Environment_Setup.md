# User Management

## Steps to Create Azure Resources

1. Clone the AD_Users_Publisher github repository:
	1. Open git bash
	2. Navigate to the root directory:
		1. "cd /c/"
		2. Create a repos directory
		3. "mkdir repos"
		4. Change your directory to the newly created repos directory
		5. "cd repos"
		6. Clone the repository from github
		7. "git clone https://github.com/HISC/AD_Users_Publisher.git"
2. Submit a ticket to the PitCrew to elevate your permissions to have "Contributor" to the appropriate Azure Subscription.
	1. We will be running against the HISC-DEV, HISC-QA, and HISC-PROD Azure Subscriptions
3. For each Azure Subscription, once permissions have been elevated
	1. Using the Azure CLI:
		1. "az login" into your Azure account
		2. "az account list --output table" to see which Azure Subscription is the Default
			1. The Default subscription is where the Azure resources will be created when running terraform
		3. If you need to change the Default Azure Subscription:
			1. "az account set --subscription [Subscription GUID]"
			2. "az account list --output table" to verify that the Default is set to the desired Subscription
	2. Execute the terraform scripts in relevant subscription directory from the github repo
		1. In git bash, navigate to the terraform directory
		2. Navigate to the [Subscription] directory
		3. It is **HIGHLY** recommended that you review the terraform script before running so that you have an understanding.
			1. run "terraform init" to initialize terraform
			2. run "terraform plan" to review. If you are happy with what will occur, continue.
			3. run "terraform apply", and then type "yes" when prompted to create the Azure resources
4. After the resources have been created
	1. Login to Azure via a browser
	2. Navigate to the hisc-[Environment]-azure-ad-users-extract App Service
		1. *note:* As of 2019-09-23, terraform does not have a way to set the following values and therefore we need to do this step manually
		2. Select the "Configuration" menu item under "Settings"
		3. Select the "General settings" tab
		4. Change the Stack to ".NET Core"
		5. Set the Major version to "3.1" and Minor version to "3.1.0" (2020-02-12)
		6. Click "Save"
	3. Navigate to the hisc-[Environment]-azure-ad-users-publisher App Service
		1. *note:* As of 2019-09-23, terraform does not have a way to set the following values and therefore we need to do this step manually
		2. Select the "Configuration" menu item under "Settings"
		3. Select the "General settings" tab
		4. Change the Stack to ".NET Core"
		5. Set the Major and Minor version to ".NET Core 2.2"
		6. Click "Save"
	4. Navigate to the hisc-[Environment]-azure-ad-users Key vault
		1. Select the "Secrets" menu item under "Settings"
		2. For each Secret, if it needs to be updated
			1. Open the Secret
			2. Open the Current Version
			3. Set "Enabled?" to "No"
			4. Click "Save"
			5. Click the Secret name toward the top of the page
			6. Click "New Version"
			7. Set the "Value"
			8. Click "Create"

## How to find a specific terraform azuread_application required_resource_access

While creating the terraform for the Azure_AD_Publisher project, I needed to have the terraform add the Azure Active Directory Graph API Permissions. Specifically I needed to grant the application the User.Read permission. What follows are the steps I took to track down the resource_app_id and resource_access.id values as terraform, as of 2019-09-24, does not grant resource access via named identifiers.

From the azure cli run "az ad app list --all"

This generates a **WHOLE BUNCH** of json.

I then copy/pasted all of the json into a notepad++ document.

I then search for the previously created App Registration that contained the User.Read permission.

Once found, it has a section like:
```
    "requiredResourceAccess": [
      {
        "additionalProperties": null,
        "resourceAccess": [
          {
            "additionalProperties": null,
            "id": "311a71cc-e848-46a1-bdf8-97ff7156d8e6",
            "type": "Scope"
          }
        ],
        "resourceAppId": "00000002-0000-0000-c000-000000000000"
      }
    ],
```
    
This gave me the resourceAppid and the resourceAccess id I needed to use in the terraform.

## Azure Key Vaults for Users Management 

### Environment Key Vaults Generated After Running Terraform

|Environment | Resource Name          | URI                                             |
|------------|:-----------------------|:------------------------------------------------|
|Development |hisc-dev-azure-ad-users |https://hisc-dev-azure-ad-users.vault.azure.net/ |
|QA          |hisc-qa-azure-ad-users  |https://hisc-qa-azure-ad-users.vault.azure.net/  |
|Production  |hisc-prod-azure-ad-users|https://hisc-prod-azure-ad-users.vault.azure.net/|

### Required Secrets

The following secrets are generated when running terraform, however the majority of them are initialized with a value that needs to be replaced.

| Secret Name                                   | Secret Description                                          |
|-----------------------------------------------|:------------------------------------------------------------|
| BearerTokenClientId                           | client_Id value used when trying to retrieve a bearer token |
| BearerTokenClientSecret                       | client_secret value used when trying to retrieve a bearer token |
| FranchiseUsersReoccurrenceGroupId             | GroupId value that will be used when automatically retrieving user information from the graph api |
| FranchiseUsersReoccurrenceSyncDurationInHours | Sync duration that will be used when automatically retrieving user information from the graph api |
| GoogleApiKey                                  | Google API Key to use when calling the Google GeoCode and TimeZone APIs |
| HOAppKey                                      | clientSecret for when instantiating ClientCredentials for Home Office |
| HOClientId                                    | clientId for when instantiating ClientCredentials for Home Office |
| HOTenant                                      | Tenant to use when creating AuthenticationContext for Home Office |
| NAAppKey                                      | clientSecret for when instantiating ClientCredentials for Franchises |
| NAClientId                                    | clientId for when instantiating ClientCredentials for Franchises |
| NATenant                                      | Tenant to use when creating AuthenticationContext for Franchises |
| SalesforceTokenClientId                       | client_id value used when trying to retrieve a bearer token from Salesforce |
| SalesforceTokenClientSecret                   | client_secret value used when trying to retrieve a bearer token from Salesforce |
| SalesforceTokenPassword                       | password value used when trying to retrieve a bearer token from Salesforce |
| SalesforceTokenUsername                       | username value used when trying to retrieve a bearer token from Salesforce |
| ServiceBusConnectionString                    | Azure service bus connection string |

### Salesforce Publish Endpoints

The following are the endpoints that we've used when testing.  The Development and QA environments use the Development endpoint as Salesforce did not have a QA environment.  When testing locally with Salesforce UserTest0[1-5] were used.

| Environment | URI |
|-------------|:----|
| Development | https://homeinsteadinc--dev.my.salesforce.com/services/apexrest/UserManager/V1/ |
| UserTest01  | https://homeinsteadinc--usertest01.my.salesforce.com/services/apexrest/UserManager/V1/ |
| UserTest02  | https://homeinsteadinc--usertest02.my.salesforce.com/services/apexrest/UserManager/V1/ |
| UserTest03  | https://homeinsteadinc--usertest03.my.salesforce.com/services/apexrest/UserManager/V1/ |
| UserTest04  | https://homeinsteadinc--usertest04.my.salesforce.com/services/apexrest/UserManager/V1/ |
| UserTest05  | https://homeinsteadinc--usertest05.my.salesforce.com/services/apexrest/UserManager/V1/ |
| Production  | https://homeinsteadinc.my.salesforce.com/services/apexrest/UserManager/V1/ |

### Salesforce Token Endpoints

The following are the endpoints that we've used when testing to retrieve a bearer token.  The Development and QA environments use the Development endpoint as Salesforce did not have a QA environment.  When testing locally with Salesforce UserTest0[1-5] were used.

| Environment | URI |
|-------------|:----|
| Development | https://test.salesforce.com/services/oauth2/token?  |
| UserTest01  | https://test.salesforce.com/services/oauth2/token?  |
| UserTest02  | https://test.salesforce.com/services/oauth2/token?  |
| UserTest03  | https://test.salesforce.com/services/oauth2/token?  |
| UserTest04  | https://test.salesforce.com/services/oauth2/token?  |
| UserTest05  | https://test.salesforce.com/services/oauth2/token?  |
| Production  | https://login.salesforce.com/services/oauth2/token? |

### Salesforce Key Vault Where Salesforce Credentials Can Be Found

- https://salesforce-dev.vault.azure.net/


