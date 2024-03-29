﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using FluentValidation;

namespace Energinet.DataHub.PostOffice.Common
{
    internal static class FluentValidationHelper
    {
        public static void SetupErrorCodeResolver()
        {
            ValidatorOptions.Global.ErrorCodeResolver = (propertyValidator) =>
                propertyValidator.Name switch
                {
                    "NotEmptyValidator" => "value_not_specified",
                    "StringEnumValidator" => "invalid_enum_value",
                    "NotEqualValidator" => "value_not_equal_to",
                    "GreaterThanValidator" => "value_not_greater_than",
                    _ => propertyValidator.Name
                };
        }
    }
}
