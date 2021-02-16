module "azfun_outbound" {
  source                                    = "../modules/function-app"
  name                                      = "azfun-outbound-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_outbound_stor.primary_access_key
  storage_account_name                      = module.azfun_outbound_stor.name
  app_service_plan_id                       = module.azfun_outbound_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.postoffice.tags
  app_settings                              = {
    POSTOFFICE_DB_CONNECTION_STRING = azurerm_cosmosdb_account.postoffice.endpoint,
    POSTOFFICE_DB_KEY               = azurerm_cosmosdb_account.postoffice.primary_key
  }
  dependencies                              = [
    module.azfun_outbound_plan.dependent_on,
    module.azfun_outbound_stor.dependent_on,
  ]
}

module "azfun_outbound_plan" {
  source              = "../modules/app-service-plan"
  name                = "asp-outbound-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "azfun_outbound_stor" {
  source                    = "../modules/storage-account"
  name                      = "stor${random_string.outbound.result}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "outbound" {
  length  = 10
  special = false
  upper   = false
}