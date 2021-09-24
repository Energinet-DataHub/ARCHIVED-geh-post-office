# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
module "stor_marketoperator_response" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.8.0"
  name                      = "storout${lower(var.project)}${lower(var.organisation)}${lower(var.environment)}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Hot"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

module "container_postoffice_reply" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.8.0"
  container_name        = "postoffice-blobstorage"
  storage_account_name  = module.stor_marketoperator_response.name
  container_access_type = "private"
  dependencies          = [
    module.stor_marketoperator_response.dependent_on
  ]
}

module "container_timeseries_reply" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.8.0"
  container_name        = "timeseries-postoffice-blobstorage"
  storage_account_name  = module.stor_marketoperator_response.name
  container_access_type = "private"
  dependencies          = [
    module.stor_marketoperator_response.dependent_on
  ]
}

module "container_changes_reply" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.8.0"
  container_name        = "charges-postoffice-blobstorage"
  storage_account_name  = module.stor_marketoperator_response.name
  container_access_type = "private"
  dependencies          = [
    module.stor_marketoperator_response.dependent_on
  ]
}

module "container_marketroles_reply" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.8.0"
  container_name        = "marketroles-postoffice-blobstorage"
  storage_account_name  = module.stor_marketoperator_response.name
  container_access_type = "private"
  dependencies          = [
    module.stor_marketoperator_response.dependent_on
  ]
}

module "container_aggregations_reply" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.8.0"
  container_name        = "aggregations-postoffice-blobstorage"
  storage_account_name  = module.stor_marketoperator_response.name
  container_access_type = "private"
  dependencies          = [
    module.stor_marketoperator_response.dependent_on
  ]
}

module "container_meteringpoint_reply" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.8.0"
  container_name        = "meteringpoint-postoffice-blobstorage"
  storage_account_name  = module.stor_marketoperator_response.name
  container_access_type = "private"
  dependencies          = [
    module.stor_marketoperator_response.dependent_on
  ]
}
