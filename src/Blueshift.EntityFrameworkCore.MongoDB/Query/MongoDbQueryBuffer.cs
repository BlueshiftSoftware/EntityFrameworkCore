using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbQueryBuffer : QueryBuffer
    {
        /// <inheritdoc />
        public MongoDbQueryBuffer(
            [NotNull] QueryContextDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override object GetPropertyValue(object entity, IProperty property)
        {
            if (property.IsShadowProperty && property.IsForeignKey())
            {
                IForeignKey foreignKey = property.AsProperty().ForeignKeys[0];
                INavigation localProperty = property.DeclaringEntityType == foreignKey.PrincipalEntityType
                    ? foreignKey.PrincipalToDependent
                    : foreignKey.DependentToPrincipal;
                entity = localProperty.GetGetter().GetClrValue(entity);
                property = foreignKey.PrincipalKey.Properties[0];
            }
            return property.GetGetter().GetClrValue(entity);
        }
    }
}
