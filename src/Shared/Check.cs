// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    [DebuggerStepThrough]
    internal static class Check
    {
        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static object IsInstanceOfType(
            [NoEnumeration] object value,
            [NotNull] Type type,
            [InvokerParameterName] [NotNull] string parameterName)
        {
            if (!Check.NotNull(type, nameof(type)).IsInstanceOfType(value))
            {
                NotEmpty(parameterName, nameof(parameterName));
                throw new ArgumentException($@"Argument {parameterName} is not an instance of {type.FullName}.", parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static T Is<T>(
            [NoEnumeration] object value,
            [InvokerParameterName] [NotNull] string parameterName)
            where T : class
            => IsInstanceOfType(value, typeof(T), parameterName) as T;

        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>(
            [NoEnumeration] T value,
            [InvokerParameterName] [NotNull] string parameterName,
            [NotNull] string propertyName)
        {
            if (ReferenceEquals(value, null))
            {
                NotEmpty(parameterName, nameof(parameterName));
                NotEmpty(propertyName, nameof(propertyName));

                throw new ArgumentException(CoreStrings.ArgumentPropertyNull(propertyName, parameterName));
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static IReadOnlyList<T> NotEmpty<T>(IReadOnlyList<T> value, [InvokerParameterName] [NotNull] string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Count == 0)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CoreStrings.CollectionArgumentIsEmpty(parameterName));
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotEmpty(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            Exception e = null;
            if (ReferenceEquals(value, null))
            {
                e = new ArgumentNullException(parameterName);
            }
            else if (value.Trim().Length == 0)
            {
                e = new ArgumentException(CoreStrings.ArgumentIsEmpty(parameterName));
            }

            if (e != null)
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }

        public static string NullButNotEmpty(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (!ReferenceEquals(value, null)
                && (value.Length == 0))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(CoreStrings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static IReadOnlyList<T> HasNoNulls<T>(IReadOnlyList<T> value, [InvokerParameterName] [NotNull] string parameterName)
            where T : class
        {
            NotNull(value, parameterName);

            if (value.Any(e => e == null))
            {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(parameterName);
            }

            return value;
        }

        public static IEntityType NotOwned(IEntityType entityType,
            [InvokerParameterName] [NotNull] string parameterName)
            => NotNull(entityType, parameterName).IsOwned()
                ? throw new ArgumentException($@"{entityType.Name} is an owned EntityType.", parameterName)
                : entityType;
    }
}
