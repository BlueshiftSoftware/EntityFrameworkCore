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
                valueBuffer.IsEmpty
                    ? CreateValueBuffer(entityType, entity)
                    : valueBuffer);

        /// <inheritdoc />
        public override InternalEntityEntry Create(
            IStateManager stateManager,
            IEntityType entityType,
            object entity)
            => Create(stateManager, entityType, entity, ValueBuffer.Empty);

        private ValueBuffer CreateValueBuffer(IEntityType entityType, object entity)
        {
            object[] values = new object[entityType.PropertyCount()];

            foreach (IProperty property in entityType.GetProperties())
            {                
                values[property.GetIndex()] = GetPropertyValue(entity, property);
            }

            return new ValueBuffer(values);
        }

        private object GetPropertyValue(object entity, IProperty property)
        {
            if (property.IsShadowProperty && property.IsForeignKey())
            {
                IForeignKey foreignKey = property.AsProperty().ForeignKeys.First();
                INavigation navigationProperty = property.DeclaringEntityType == foreignKey.PrincipalEntityType
                                                 && !foreignKey.IsSelfPrimaryKeyReferencing()
                    ? foreignKey.PrincipalToDependent
                    : foreignKey.DependentToPrincipal;

                if (navigationProperty != null)
                {
                    entity = navigationProperty.GetGetter().GetClrValue(entity);

                    IEntityType targetEntityType = navigationProperty.GetTargetType();
                    property = targetEntityType.FindPrimaryKey().Properties.Single();
                }
                else
                {
                    entity = null;
                }
            }

            return entity == null
                ? null
                : property.IsShadowProperty
                    ? entity.GetHashCode()
                    : property.GetGetter().GetClrValue(entity);
        }
    }
}
