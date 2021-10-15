using System;
using FluentValidation.Validators;

namespace Energinet.DataHub.PostOffice.Application.Validation.Rules
{
    public abstract class PropertyValidator<T> : PropertyValidator
    {
        protected override bool IsValid(PropertyValidatorContext context) =>
            context == null
                ? throw new ArgumentNullException(nameof(context))
                : context.PropertyValue is T value && IsValid(value);

        protected abstract bool IsValid(T value);
    }
}
