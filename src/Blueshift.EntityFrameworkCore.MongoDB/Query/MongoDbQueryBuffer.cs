using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
            Check.NotNull(clrCollectionAccessor, nameof(clrCollectionAccessor));
            Check.NotNull(inverseNavigation, nameof(inverseNavigation));
            Check.NotNull(inverseClrPropertySetter, nameof(inverseClrPropertySetter));

            ICollection<TRelated> collection = (ICollection<TRelated>) clrCollectionAccessor
                .GetOrCreate(entity);

            IClrPropertyGetter primaryKeyPropertyGetter = inverseNavigation
                .DeclaringEntityType
                .FindPrimaryKey()
                .Properties
                .Single()
                .GetGetter();

            IEnumerable<(TRelated, TRelated)> relatedEntities = relatedEntitiesFactory()
                .Join(collection,
                    related => primaryKeyPropertyGetter.GetClrValue(related),
                    related => primaryKeyPropertyGetter.GetClrValue(related),
                    (related, original) => (related, original))
                .ToList();

            foreach ((TRelated related, TRelated original) in relatedEntities)
            {
                collection.Remove(original);
                collection.Add(related);
                inverseClrPropertySetter.SetClrValue(related, entity);
            }
        }
    }
}
