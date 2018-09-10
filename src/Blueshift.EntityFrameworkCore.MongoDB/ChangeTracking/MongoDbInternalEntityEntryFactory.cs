using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Blueshift.EntityFrameworkCore.MongoDB.ChangeTracking
{
    /// <inheritdoc />
    public class MongoDbInternalEntityEntryFactory : InternalEntityEntryFactory
    {
        /// <inheritdoc />
        public override InternalEntityEntry Create(
            IStateManager stateManager,
            IEntityType entityType,
            object entity,
            in ValueBuffer valueBuffer)
            => base.Create(
                stateManager,
                entityType,
                entity,
                valueBuffer.IsEmpty && CreateValueBuffer(entityType, entity, out ValueBuffer repairedValueBuffer)
                    ? repairedValueBuffer
                    : valueBuffer);

        private bool CreateValueBuffer(IEntityType entityType, object entity, out ValueBuffer valueBuffer)
        {
            int propertyCount = entityType.PropertyCount(),
                relationshipPropertyCount = entityType.RelationshipPropertyCount(),
                totalCount = propertyCount + relationshipPropertyCount;

            valueBuffer = new ValueBuffer(new object[totalCount], 0);

            foreach (IProperty property in entityType.GetProperties())
            {                
                valueBuffer[property.GetIndex()] = GetPropertyValue(entity, property);
            }

            foreach (INavigation navigation in entityType.GetNavigations())
            {
                valueBuffer[propertyCount + navigation.GetRelationshipIndex()] = navigation.GetGetter().GetClrValue(entity);
            }

            return true;
        }

        private object GetPropertyValue(object entity, IProperty property)
        {
            if (property.IsShadowProperty && property.IsForeignKey())
            {
                IForeignKey foreignKey = property.AsProperty().ForeignKeys[0];
                INavigation navigationProperty = property.DeclaringEntityType == foreignKey.PrincipalEntityType
                    ? foreignKey.PrincipalToDependent
                    : foreignKey.DependentToPrincipal;
                entity = navigationProperty.GetGetter().GetClrValue(entity);
                property = foreignKey.PrincipalKey.Properties[0];
            }

            return property.IsShadowProperty
                ? entity.GetHashCode()
                : property.GetGetter().GetClrValue(entity);
        }
    }
}
