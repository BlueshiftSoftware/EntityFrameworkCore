using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class ValueBufferFactory : IValueBufferFactory
    {
        /// <inheritdoc />
        public ValueBuffer CreateFromInstance(
            object instance,
            IEntityType entityType,
            object owner,
            INavigation owningNavigation)
        {
            Check.NotNull(instance, nameof(instance));
            Check.NotNull(entityType, nameof(entityType));

            PropertyCounts propertyCounts = entityType.AsEntityType().Counts;

            var propertyValues = new object[propertyCounts.PropertyCount];

            IEnumerable<Property> properties = entityType
                .GetProperties()
                .Where(property => !property.IsForeignKey())
                .Select(property => property.AsProperty());

            foreach (Property property in properties)
            {
                if (!property.IsShadowProperty)
                {
                    propertyValues[property.GetIndex()] = property.Getter.GetClrValue(instance);
                }
                else if (property.IsPrimaryKey())
                {
                    Debug.Assert(entityType.IsOwned(), $"Non-owned entity type {entityType.Name} with a shadow primary key.");
                    propertyValues[property.GetShadowIndex()] = instance.GetHashCode();
                }
            }

            if (owningNavigation != null)
            {
                Check.NotNull(owner, nameof(owner));

                IForeignKey foreignKey = owningNavigation.ForeignKey;

                for (int i = 0; i < foreignKey.Properties.Count; i++)
                {
                    IProperty foreignKeyProperty = foreignKey.Properties[i];
                    IProperty principalKeyProperty = foreignKey.PrincipalKey.Properties[i];

                    propertyValues[foreignKeyProperty.GetIndex()] = foreignKey.IsOwnership
                                                                    && principalKeyProperty.IsShadowProperty
                        ? owner.GetHashCode()
                        : principalKeyProperty.GetGetter().GetClrValue(owner);
                }
            }

            IEnumerable<INavigation> navigations = entityType
                .GetNavigations()
                .Where(navigation => navigation.IsDependentToPrincipal()
                                     && !navigation.IsCollection());

            foreach (INavigation navigation in navigations)
            {
                var related = navigation.GetGetter().GetClrValue(instance);
                if (related != null)
                {
                    IForeignKey foreignKey = navigation.ForeignKey;

                    for (int i = 0; i < foreignKey.Properties.Count; i++)
                    {
                        IProperty foreignKeyProperty = foreignKey.Properties[i];
                        IProperty principalKeyProperty = foreignKey.PrincipalKey.Properties[i];

                        propertyValues[foreignKeyProperty.GetIndex()] = foreignKey.IsOwnership
                                                                        && principalKeyProperty.IsShadowProperty
                            ? related.GetHashCode()
                            : principalKeyProperty.GetGetter().GetClrValue(related);
                    }
                }
            }

            return new ValueBuffer(propertyValues, 0);
        }
    }
}