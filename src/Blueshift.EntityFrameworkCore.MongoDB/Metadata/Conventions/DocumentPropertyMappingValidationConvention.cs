using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class DocumentPropertyMappingValidationConvention : PropertyMappingValidationConvention
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly IMemberClassifier _memberClassifier;

        /// <inheritdoc />
        public DocumentPropertyMappingValidationConvention(
            [NotNull] ITypeMappingSource typeMappingSource,
            [NotNull] IMemberClassifier memberClassifier)
            : base(
                Check.NotNull(typeMappingSource, nameof(typeMappingSource)),
                Check.NotNull(memberClassifier, nameof(memberClassifier)))
        {
            _typeMappingSource = typeMappingSource;
            _memberClassifier = memberClassifier;
        }

        /// <inheritdoc />
        public override InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var unmappedProperty = entityType.GetProperties().FirstOrDefault(p =>
                    (!ConfigurationSource.Convention.Overrides(p.GetConfigurationSource()) || !p.IsShadowProperty)
                    && !IsMappedPrimitiveProperty(p));

                if (unmappedProperty != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyNotMapped(
                            entityType.DisplayName(), unmappedProperty.Name, unmappedProperty.ClrType.ShortDisplayName()));
                }

                if (!entityType.HasClrType())
                {
                    continue;
                }

                var clrProperties = new HashSet<string>();

                clrProperties.UnionWith(
                    entityType.GetRuntimeProperties().Values
                        .Where(pi => pi.IsCandidateProperty())
                        .Select(pi => pi.Name));

                clrProperties.ExceptWith(entityType.GetProperties().Select(p => p.Name));
                clrProperties.ExceptWith(entityType.GetNavigations().Select(p => p.Name));
                clrProperties.ExceptWith(entityType.GetServiceProperties().Select(p => p.Name));
                clrProperties.RemoveWhere(p => entityType.Builder.IsIgnored(p, ConfigurationSource.Convention));

                if (clrProperties.Count <= 0)
                {
                    continue;
                }

                foreach (var clrProperty in clrProperties)
                {
                    var actualProperty = entityType.GetRuntimeProperties()[clrProperty];
                    var propertyType = actualProperty.PropertyType;
                    var targetSequenceType = propertyType.TryGetSequenceType();

                    if (modelBuilder.IsIgnored(modelBuilder.Metadata.GetDisplayName(propertyType),
                        ConfigurationSource.Convention)
                        || (targetSequenceType != null
                            && modelBuilder.IsIgnored(modelBuilder.Metadata.GetDisplayName(targetSequenceType),
                                ConfigurationSource.Convention)))
                    {
                        continue;
                    }

                    var targetType = base.FindCandidateNavigationPropertyType(actualProperty);

                    var isTargetWeakOrOwned
                        = targetType != null
                          && (modelBuilder.Metadata.HasEntityTypeWithDefiningNavigation(targetType)
                              || modelBuilder.Metadata.ShouldBeOwnedType(targetType));

                    if (targetType != null
                        && targetType.IsValidEntityType()
                        && (isTargetWeakOrOwned
                            || modelBuilder.Metadata.FindEntityType(targetType) != null
                            || targetType.GetRuntimeProperties().Any(p => p.IsCandidateProperty())))
                    {
                        if ((!isTargetWeakOrOwned
                             || !targetType.GetTypeInfo().Equals(entityType.ClrType.GetTypeInfo()))
                            && entityType.GetDerivedTypes().All(
                                dt => dt.FindDeclaredNavigation(actualProperty.Name) == null)
                            && !entityType.IsInDefinitionPath(targetType))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.NavigationNotAdded(
                                    entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                        }
                    }
                    else if (targetSequenceType == null && propertyType.GetTypeInfo().IsInterface
                             || targetSequenceType != null && targetSequenceType.GetTypeInfo().IsInterface)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InterfacePropertyNotAdded(
                                entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyNotAdded(
                                entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                    }
                }
            }

            return modelBuilder;
        }

        /// <inheritdoc />
        protected override bool IsMappedPrimitiveProperty([NotNull] IProperty property)
            => _typeMappingSource.FindMapping(property) != null;

        /// <inheritdoc />
        protected override Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => _memberClassifier.FindCandidateNavigationPropertyType(propertyInfo);
    }
}
