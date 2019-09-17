terraform {
  required_version = ">= 0.12.8"
}

provider "azuread" {
  version = "~> 0.6"
}

provider "azurerm" {
  version = ">= 1.33"
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

variable "app_environment" {
	type = string
	default = "dev"
}

variable "default_location" {
	type = string
	default = "Central US"
}

variable "unique_postfix" {
	type = string
	default = ""
}

data "azurerm_client_config" "current" {}

# create the resource groups
resource azurerm_resource_group azure-ad-users-rg {
  name     = var.app_root
  location = var.default_location
  tags = {
    "Project"             = "Integrated Lead Management"
    "Target"              = "Home Office"
    "App Name"            =  var.app_root
    "Assigned Department" = "IT Services"
    "Environment"         = "Development"
    "Assigned Company"    = "Home Office"
  }
}

resource azurerm_resource_group salesforce-rg {
  name     = "Salesforce"
  location = var.default_location
  tags = {
    "Environment"         = "Development"
    "Assigned Department" = "IT Services"
    "Target"              = "Franchise Network"
    "App Name"            = "Salesforce"
    "Assigned Company"    = "Home Office"
  }
}

resource azurerm_resource_group integrations-rg {
  name     = "Integrations"
  location = var.default_location
  tags = {
    "Project"             = "Genesis"
    "Target"              = "Home Office"
    "App Name"            = "Integrations"
    "Assigned Department" = "IT Services"
    "environment"         = "Development"
    "Assigned Company"    = "Home Office"
  }
}

# create the service bus
resource azurerm_servicebus_namespace integrations-sb {
  name                = "${var.app_prefix}-integrations-${var.app_environment}${var.unique_postfix}" #this has to be unique across all subscriptions
  location            = var.default_location
  resource_group_name = "${azurerm_resource_group.integrations-rg.name}"
  sku                 = "Standard"
  tags = {
    "Project"             = "Genesis"
    "Target"              = "Home Office"
    "App Name"            = "Integrations"
    "Assigned Department" = "IT Services"
    "environment"         = "Development"
    "Assigned Company"    = "Home Office"
  }
}

# create the topic on the service bus
resource azurerm_servicebus_topic franchiseusers-sbt {
  name                  = "franchiseusers"
  resource_group_name   = "${azurerm_resource_group.integrations-rg.name}"
  namespace_name        = "${azurerm_servicebus_namespace.integrations-sb.name}"
  max_size_in_megabytes = 1024
}

# create the subscription on the topic
resource "azurerm_servicebus_subscription" "salesforcefranchiseusers-sbts" {
  name                                 = "salesforcefranchiseuserssubscription"
  resource_group_name                  = "${azurerm_resource_group.integrations-rg.name}"
  namespace_name                       = "${azurerm_servicebus_namespace.integrations-sb.name}"
  topic_name                           = "${azurerm_servicebus_topic.franchiseusers-sbt.name}"
  max_delivery_count                   = 10
  dead_lettering_on_message_expiration = true
}

# create a linux app service plan for extract
resource azurerm_app_service_plan linux-extract-asp {
  name                = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract-plan"
  location            = var.default_location
  resource_group_name = "${azurerm_resource_group.azure-ad-users-rg.name}"

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
  name                = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher-plan"
  location            = var.default_location
  resource_group_name = "${azurerm_resource_group.salesforce-rg.name}"

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
  name                       = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher-app-registration"
  homepage                   = "https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net/"
  identifier_uris            = ["https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net"]
  reply_urls                 = ["https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
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

resource azuread_application extract_app {
  name                       = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract-app-registration"
  homepage                   = "https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net/"
  identifier_uris            = ["https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net"]
  reply_urls                 = ["https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
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

# create an app service for the extract service
resource azurerm_app_service extract-as {
  name                = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract${var.unique_postfix}" #this has to be unique across all subscriptions, used for the hostname
  location            = var.default_location
  resource_group_name = "${azurerm_resource_group.azure-ad-users-rg.name}"
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
		allowed_audiences = ["https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
    }
  }
  
  logs {
    http_logs {
	  file_system  {
	    retention_in_days        = 7
		retention_in_mb          = 35
	  }
	}
  }
  
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
	ASPNETCORE_ENVIRONMENT              = "Development"
	APPLICATION_AI_KEY                  = "${azurerm_application_insights.extract-ai.instrumentation_key}"
	APPLICATION_KEYVAULTURL             = "https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}.vault.azure.net/secrets/" #the j should be removed/added when "local"
	WEBSITE_HTTPLOGGING_RETENTION_DAYS  = 7
  }
}

# create an app service for the publisher service
resource azurerm_app_service publisher-as {
  name                = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher${var.unique_postfix}" #this has to be unique across all subscriptions, used for the hostname
  location            = var.default_location
  resource_group_name = "${azurerm_resource_group.salesforce-rg.name}"
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
		allowed_audiences = ["https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher${var.unique_postfix}.azurewebsites.net/.auth/login/aad/callback"]
    }
  }
  
  logs {
    http_logs {
	  file_system  {
	    retention_in_days        = 7
		retention_in_mb          = 35
	  }
	}
  }
  
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
	ASPNETCORE_ENVIRONMENT              = "Development"
	APPLICATION_AI_KEY                  = "${azurerm_application_insights.publisher-ai.instrumentation_key}"
	APPLICATION_KEYVAULTURL             = "https://${var.app_prefix}-${var.app_environment}-${var.app_root_lower}.vault.azure.net/secrets/" #the j should be removed/added when "local"
	WEBSITE_HTTPLOGGING_RETENTION_DAYS  = 7
  }
}

# create the key vault
resource azurerm_key_vault azure-ad-users-kv {
  name                            = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}" #this has to be unique across all subscriptions, the j should be removed/added when "local"
  location                        = var.default_location
  resource_group_name             = "${azurerm_resource_group.azure-ad-users-rg.name}"
  sku_name                        = "standard"
  tenant_id                       = "${data.azurerm_client_config.current.tenant_id}"
  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false
  
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

# create the application insight for extract
resource azurerm_application_insights extract-ai {
  name                = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-extract-ai"
  location            = var.default_location
  resource_group_name = "${azurerm_resource_group.azure-ad-users-rg.name}"
  application_type    = "web"
}

resource azurerm_application_insights publisher-ai {
  name                = "${var.app_prefix}-${var.app_environment}-${var.app_root_lower}-publisher-ai"
  location            = var.default_location
  resource_group_name = "${azurerm_resource_group.salesforce-rg.name}"
  application_type    = "web"
}

output "extract_instrumentation_key" {
  value = "${azurerm_application_insights.extract-ai.instrumentation_key}"
}

output "publisher_instrumentation_key" {
  value = "${azurerm_application_insights.publisher-ai.instrumentation_key}"
}

#output "extract_principal_id" {
#  value = "${azurerm_app_service.extract-as.identity[0].principal_id}"
#}
#
#output "data_tenant_id" {
#  value = "${data.azurerm_client_config.current.tenant_id}"
#}