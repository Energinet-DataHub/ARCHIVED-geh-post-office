data "azurerm_key_vault" "core_keyvault" {
  name                = var.core_keyvault_name
  resource_group_name = var.core_resource_group_name
}

module "kvs_postoffice_queue_connection_string" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/key-vault-secret?ref=1.3.0"
  name                      = "POST-OFFICE-QUEUE-CONNECTION-STRING"
  value                     = module.sbnar_inbound_sender.primary_connection_string
  key_vault_id              = data.azurerm_key_vault.core_keyvault.id
  dependencies              = [
    module.sbnar_inbound_sender.dependent_on
  ]
}

module "kvs_postoffice_queue_marketdata_topic_name" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/key-vault-secret?ref=1.3.0"
  name                      = "POST-OFFICE-QUEUE-MARKETDATA-TOPIC-NAME"
  value                     = module.sbt_marketdata.name
  key_vault_id              = data.azurerm_key_vault.core_keyvault.id
  dependencies              = [
    module.sbt_marketdata.dependent_on
  ]
}

module "kvs_postoffice_queue_timeseries_topic_name" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/key-vault-secret?ref=1.3.0"
  name                      = "POST-OFFICE-QUEUE-TIMESERIES-TOPIC-NAME"
  value                     = module.sbt_timeseries.name
  key_vault_id              = data.azurerm_key_vault.core_keyvault.id
  dependencies              = [
    module.sbt_timeseries.dependent_on
  ]
}

module "kvs_postoffice_queue_aggregations_topic_name" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/key-vault-secret?ref=1.3.0"
  name                      = "POST-OFFICE-QUEUE-AGGREGATIONS-TOPIC-NAME"
  value                     = module.sbt_aggregations.name
  key_vault_id              = data.azurerm_key_vault.core_keyvault.id
  dependencies              = [
    module.sbt_aggregations.dependent_on
  ]
}