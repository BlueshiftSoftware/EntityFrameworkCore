using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        /// <inheritdoc />
        public override object GetEntity(IKey key, EntityLoadInfo entityLoadInfo, bool queryStateManager, bool throwOnNullKey)
            => base.GetEntity(key, entityLoadInfo, queryStateManager, throwOnNullKey);

        /// <inheritdoc />
        public override void IncludeCollection<TEntity, TRelated, TElement>(
            int includeId,
            INavigation navigation,
            INavigation inverseNavigation,
            IEntityType targetEntityType,
            IClrCollectionAccessor clrCollectionAccessor,
            IClrPropertySetter inverseClrPropertySetter,
            bool tracking,
            TEntity entity,
            Func<IEnumerable<TRelated>> relatedEntitiesFactory,
            Func<TEntity, TRelated, bool> joinPredicate)
        {
            foreach (TRelated related in relatedEntitiesFactory())
            {
            }
        }

        /// <inheritdoc />
        public override async Task IncludeCollectionAsync<TEntity, TRelated, TElement>(
            int includeId,
            INavigation navigation,
            INavigation inverseNavigation,
            IEntityType targetEntityType,
            IClrCollectionAccessor clrCollectionAccessor,
            IClrPropertySetter inverseClrPropertySetter,
            bool tracking,
            TEntity entity,
            Func<IAsyncEnumerable<TRelated>> relatedEntitiesFactory,
            Func<TEntity, TRelated, bool> joinPredicate,
            CancellationToken cancellationToken)
        {
            IAsyncEnumerator<TRelated> asyncEnumerator = relatedEntitiesFactory().GetEnumerator();
            while (await asyncEnumerator.MoveNext())
            {
                TRelated related = asyncEnumerator.Current;
            }
        }

        /// <inheritdoc />
        public override void StartTracking(object entity, EntityTrackingInfo entityTrackingInfo)
            =>  base.StartTracking(entity, entityTrackingInfo);

        /// <inheritdoc />
        public override void StartTracking(object entity, IEntityType entityType)
            => base.StartTracking(entity, entityType);
    }
}
