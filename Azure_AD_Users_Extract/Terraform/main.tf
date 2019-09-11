# Configure the provider
provider "azurerm" {
	version = "~> 1.33"
}

# Create a new resource group
resource "azurerm_resource_group" "rg" {
	name		= "myTfResourceGroup"
	location	= "eastus"
}
