terraform {
  required_version = "~> 0.12.8"
}

provider "azuread" {
  version = "~> 0.6"
}

provider "azurerm" {
  version = "~> 1.34"
}

provider "external" {
  version = "~> 1.2"
}

variable "app_prefix" {
	type = string
	default = "hisc"
}

variable "app_root" {
	type = string
	default = "Azure_AD_Users"
}

variable "app_root_lower" {
	type = string
	default = "azure-ad-users"
}

variable "app_environment_identifier" {
	type = string
	default = "prod"
}

variable "aspnet_environment" {
	type = string
	default = "Staging"
}

variable "default_location" {
	type = string
	default = "Central US"
}

variable "retention_in_days" {
	type = number
	default = 7
}

variable "retention_in_mb" {
	type = number
	default = 35
}

variable "unique_postfix" {
	type = string
	default = ""
}

data "azurerm_client_config" "current" {}

## the following block is a workaround pulled from https://github.com/terraform-providers/terraform-provider-azurerm/issues/3502
data "external" "this_az_account" {
  program = [
    "az",
    "ad",
    "signed-in-user",
    "show",
    "--query",
    "{displayName: displayName,objectId: objectId,objectType: objectType,odata_metadata: \"odata.metadata\"}"
  ]
}

# create the resource groups, these already exist in HISC-DEV
#resource azurerm_resource_group azure-ad-users-rg {
#  name     = var.app_root
#  location = var.default_location
#  tags = {
#    "Project"             = "Integrated Lead Management"
#    "Target"              = "Home Office"
#    "App Name"            =  var.app_root
#    "Assigned Department" = "IT Services"
#    "Assigned Company"    = "Home Office"
#  }
#}
#
#resource azurerm_resource_group salesforce-rg {
#  name     = "SalesForce"
#  location = var.default_location
#  tags = {
#    "Assigned Department" = "IT Services"
#    "Target"              = "Franchise Network"
#    "App Name"            = "Salesforce"
#    "Assigned Company"    = "Home Office"
#  }
#}
#
#resource azurerm_resource_group integrations-rg {
#  name     = "Integrations"
#  location = var.default_location
#  tags = {
#    "Project"             = "Genesis"
#    "Target"              = "Home Office"
#    "App Name"            = "Integrations"
#    "Assigned Department" = "IT Services"
#    "Assigned Company"    = "Home Office"
#  }
#}

# create the service bus
resource azurerm_servicebus_namespace integrations-sb {
  name                = "${var.app_prefix}-integrations-${var.app_environment_identifier}${var.unique_postfix}" #this has to be unique across all subscriptions
  location            = var.default_location
  resource_group_name = "Integrations"
  sku                 = "Standard"
  tags = {
    "Project"             = "Genesis"
    "Target"              = "Home Office"
    "App Name"            = "Integrations"
    "Assigned Department" = "IT Services"
    "Assigned Company"    = "Home Office"
  }
}

# create the topic on the service bus
resource azurerm_servicebus_topic franchiseusers-sbt {
  name                  = "franchiseusers"
  resource_group_name   = "Integrations"
  namespace_name        = "${azurerm_servicebus_namespace.integrations-sb.name}"
  max_size_in_megabytes = 1024
  auto_delete_on_idle   = "P14D" # 14 days
  default_message_ttl   = "P10D" # 10 days
}

# create the subscription on the topic
resource "azurerm_servicebus_subscription" "salesforcefranchiseusers-sbts" {
  name                                 = "salesforcefranchiseuserssubscription"
  resource_group_name                  = "Integrations"
  namespace_name                       = "${azurerm_servicebus_namespace.integrations-sb.name}"
  topic_name                           = "${azurerm_servicebus_topic.franchiseusers-sbt.name}"
  max_delivery_count                   = 10
  dead_lettering_on_message_expiration = true
}

# create a linux app service plan for extract
resource azurerm_app_service_plan linux-extract-asp {
  name                = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract-plan"
  location            = var.default_location
  resource_group_name = "Azure_AD_Users"

  # Define Linux as Host OS
  kind = "Linux"

  # Choose size
  sku {
    tier = "Standard"
    size = "S1"
  }

  reserved = true # Mandatory for Linux plans
}

# create a linux app service plan for publisher
resource azurerm_app_service_plan linux-publisher-asp {
  name                = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher-plan"
  location            = var.default_location
  resource_group_name = "SalesForce"

  # Define Linux as Host OS
  kind = "Linux"

  # Choose size
  sku {
    tier = "Standard"
    size = "S1"
  }

  reserved = true # Mandatory for Linux plans
}

resource azuread_application publisher_app {
  name                       = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher-app"
  homepage                   = "https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net/"
  identifier_uris            = ["https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net"]
  reply_urls                 = ["https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
  available_to_other_tenants = false
  oauth2_allow_implicit_flow = true

  required_resource_access {
    # Azure Active Directory Graph
    resource_app_id = "00000002-0000-0000-c000-000000000000"

    # User.Read
    resource_access {
      id = "311a71cc-e848-46a1-bdf8-97ff7156d8e6"
      type = "Scope"
    }
  }
}

resource "azuread_service_principal" "publisher_sp" {
  application_id                = "${azuread_application.publisher_app.application_id}"
  app_role_assignment_required  = false
}

resource azuread_application extract_app {
  name                       = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract-app"
  homepage                   = "https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net/"
  identifier_uris            = ["https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net"]
  reply_urls                 = ["https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
  available_to_other_tenants = false
  oauth2_allow_implicit_flow = true
  
  required_resource_access {
    # Azure Active Directory Graph
    resource_app_id = "00000002-0000-0000-c000-000000000000"

    # User.Read
    resource_access {
      id = "311a71cc-e848-46a1-bdf8-97ff7156d8e6"
      type = "Scope"
    }
  }
}

resource "azuread_service_principal" "extract_sp" {
  application_id                = "${azuread_application.extract_app.application_id}"
  app_role_assignment_required  = false
}

# create an app service for the extract service
resource azurerm_app_service extract-as {
  name                = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract${var.unique_postfix}" #this has to be unique across all subscriptions, used for the hostname
  location            = var.default_location
  resource_group_name = "Azure_AD_Users"
  app_service_plan_id = "${azurerm_app_service_plan.linux-extract-asp.id}"

  identity {
    type = "SystemAssigned"
  }

  # require https
  https_only = true
  
  auth_settings {
    enabled          = true
    default_provider = "AzureActiveDirectory"
    issuer           = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
	active_directory  {
        client_id         = "${azuread_application.extract_app.application_id}"
		allowed_audiences = ["https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
    }
  }
  
  site_config {
    always_on = true
  }
  
  logs {
    http_logs {
	  file_system  {
	    retention_in_days        = var.retention_in_days
		retention_in_mb          = var.retention_in_mb
	  }
	}
  }
  
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
	APPLICATION_AI_KEY                  = "${azurerm_application_insights.extract-ai.instrumentation_key}"
	APPLICATION_KEYVAULTURL             = "https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}.vault.azure.net/secrets/"
	WEBSITE_HTTPLOGGING_RETENTION_DAYS  = var.retention_in_days
  }
}

# create an app service for the publisher service
resource azurerm_app_service publisher-as {
  name                = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher${var.unique_postfix}" #this has to be unique across all subscriptions, used for the hostname
  location            = var.default_location
  resource_group_name = "SalesForce"
  app_service_plan_id = "${azurerm_app_service_plan.linux-publisher-asp.id}"

  identity {
    type = "SystemAssigned"
  }

  # require https
  https_only = true
  
  auth_settings {
    enabled          = true
    default_provider = "AzureActiveDirectory"
    issuer           = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
	active_directory  {
      client_id         = "${azuread_application.publisher_app.application_id}"
	  allowed_audiences = ["https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
    }
  }
  
  site_config {
    always_on = true
  }
  
  logs {
    http_logs {
	  file_system  {
	    retention_in_days        = var.retention_in_days
		retention_in_mb          = var.retention_in_mb
	  }
	}
  }
  
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
	APPLICATION_AI_KEY                  = "${azurerm_application_insights.publisher-ai.instrumentation_key}"
	APPLICATION_KEYVAULTURL             = "https://${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}.vault.azure.net/secrets/"
	WEBSITE_HTTPLOGGING_RETENTION_DAYS  = var.retention_in_days
  }
}

# create the key vault
resource azurerm_key_vault azure-ad-users-kv {
  name                            = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}" #this has to be unique across all subscriptions
  location                        = var.default_location
  resource_group_name             = "Azure_AD_Users"
  sku_name                        = "standard"
  tenant_id                       = "${data.azurerm_client_config.current.tenant_id}"
  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false
  
  access_policy {
    tenant_id = "${data.azurerm_client_config.current.tenant_id}"
    object_id = "${data.external.this_az_account.result.objectId}" #"431d2385-8c08-4237-9614-185b61dacf79"
	key_permissions = []
    secret_permissions = [
      "Get",
	  "List",
	  "Set",
	  "Delete",
    ]
	certificate_permissions = []
  }
  
  access_policy {
    tenant_id = "${data.azurerm_client_config.current.tenant_id}"
    object_id = "${azurerm_app_service.extract-as.identity[0].principal_id}"
    key_permissions = []
    secret_permissions = [
      "Get",
      "List",
    ]
    certificate_permissions = []
  }
  
  access_policy {
    tenant_id = "${data.azurerm_client_config.current.tenant_id}"
    object_id = "${azurerm_app_service.publisher-as.identity[0].principal_id}"
    key_permissions = []
    secret_permissions = [
      "Get",
      "List",
    ]
    certificate_permissions = []
  }
}

resource azurerm_key_vault_secret "BearerTokenClientId" {
  name         = "BearerTokenClientId"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "BearerTokenClientSecret" {
  name         = "BearerTokenClientSecret"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "FranchiseUsersReoccurrenceGroupId" {
  name         = "FranchiseUsersReoccurrenceGroupId"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "FranchiseUsersReoccurrenceSyncDurationInHours" {
  name         = "FranchiseUsersReoccurrenceSyncDurationInHours"
  value        = "4"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "GoogleApiKey" {
  name         = "GoogleApiKey"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "HOAppKey" {
  name         = "HOAppKey"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "HOClientId" {
  name         = "HOClientId"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "HOTenant" {
  name         = "HOTenant"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "NAAppKey" {
  name         = "NAAppKey"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "NAClientId" {
  name         = "NAClientId"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "NATenant" {
  name         = "NATenant"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "SalesforceTokenClientId" {
  name         = "SalesforceTokenClientId"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "SalesforceTokenClientSecret" {
  name         = "SalesforceTokenClientSecret"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "SalesforceTokenPassword" {
  name         = "SalesforceTokenPassword"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "SalesforceTokenUsername" {
  name         = "SalesforceTokenUsername"
  value        = "replace_me_once_created"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

resource azurerm_key_vault_secret "ServiceBusConnectionString" {
  name         = "ServiceBusConnectionString"
  value        = "${azurerm_servicebus_namespace.integrations-sb.default_primary_connection_string}"
  key_vault_id = "${azurerm_key_vault.azure-ad-users-kv.id}"
}

# create the application insight for extract
resource azurerm_application_insights extract-ai {
  name                = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-extract-ai"
  location            = var.default_location
  resource_group_name = "Azure_AD_Users"
  application_type    = "web"
}

resource azurerm_application_insights publisher-ai {
  name                = "${var.app_prefix}-${var.app_environment_identifier}-${var.app_root_lower}-publisher-ai"
  location            = var.default_location
  resource_group_name = "SalesForce"
  application_type    = "web"
}

#print out values after run
output "extract_instrumentation_key" {
  value = "${azurerm_application_insights.extract-ai.instrumentation_key}"
}

output "publisher_instrumentation_key" {
  value = "${azurerm_application_insights.publisher-ai.instrumentation_key}"
}

#output "current_tenant_id" {
#  value = "${data.azurerm_client_config.current.tenant_id}"
#}
#
#output "current_service_principal_object_id" {
#  value = "${data.azurerm_client_config.current.service_principal_object_id}"
#}
#
#output "current_user_object_id" {
#  value = "${data.external.this_az_account.result.objectId}"
#}