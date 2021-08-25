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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;

namespace Energinet.DataHub.PostOffice.Common.Extensions
{
    public static class UriExtensions
    {
        public static T ParseQuery<T>(this Uri uri)
        {
            static Type GetUnderlyingType(Type type)
            {
                return Nullable.GetUnderlyingType(type) ?? type;
            }

            static object ChangeType(string val, Type type)
            {
                return Convert.ChangeType(val, type, CultureInfo.CurrentCulture);
            }

            static IEnumerable Cast(IEnumerable enumerable, Type type)
            {
                return (IEnumerable)typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(type).Invoke(null, new object[] { enumerable })!;
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            try
            {
                var dict = QueryHelpers.ParseQuery(uri.Query).ToDictionary(x => x.Key, x => x.Value, new CaseIgnorantStringComparer());
                var ctorParams = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance).First().GetParameters().OrderBy(x => x.Position).Select(parameterInfo =>
                {
                    if (!dict.TryGetValue(parameterInfo.Name!, out var rawStringValues))
                        return null;

                    if (rawStringValues.Count == 1)
                        return ChangeType(rawStringValues.First(), GetUnderlyingType(parameterInfo.ParameterType));

                    var genericTypeArgument = parameterInfo.ParameterType.GenericTypeArguments[0];
                    return Cast(rawStringValues.Select(value => ChangeType(value, GetUnderlyingType(genericTypeArgument))), GetUnderlyingType(genericTypeArgument));
                });
                return (T)Activator.CreateInstance(typeof(T), ctorParams.ToArray())!;
            }
            catch (Exception)
            {
                throw new ArgumentException($"Could not parse query '{uri.Query}' to type {nameof(T)}");
            }
        }

        private sealed class CaseIgnorantStringComparer : IEqualityComparer<string>
        {
            public bool Equals(string? x, string? y)
            {
                return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode(StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
