// Copyright 2020 Energinet DataHub A/S
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
using FluentValidation.Validators;

namespace Energinet.DataHub.PostOffice.Application.Validation.Rules
{
    public sealed class LegacyActorIdValidationRule<T> : PropertyValidator<T, string?>
    {
        private readonly GlobalLocationNumberValidationRule<T> _globalLocationNumberValidationRule;
        private readonly UuidValidationRule<T> _uuidValidationRule;

        public LegacyActorIdValidationRule()
        {
            _globalLocationNumberValidationRule = new GlobalLocationNumberValidationRule<T>();
            _uuidValidationRule = new UuidValidationRule<T>();
        }

        public override string Name => "invalid_actor";

        public override bool IsValid(ValidationContext<T> context, string? value)
        {
            return _globalLocationNumberValidationRule.IsValid(context, value) ||
                   _uuidValidationRule.IsValid(context, value);
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
        {
            return "'{PropertyName}' must have a valid actor.";
        }
    }
}
