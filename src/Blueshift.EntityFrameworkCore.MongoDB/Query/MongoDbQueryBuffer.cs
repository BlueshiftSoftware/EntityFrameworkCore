using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
        private readonly IModel _model;
        private readonly IStateManager _stateManager;
        [NotNull] private readonly IEntityLoadInfoFactory _entityLoadInfoFactory;

        /// <inheritdoc />
        public MongoDbQueryBuffer(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] IEntityLoadInfoFactory entityLoadInfoFactory)

            : base(dependencies)
        {
            _model = dependencies.CurrentDbContext.Context.Model;
            _stateManager = dependencies.StateManager;
            _entityLoadInfoFactory = Check.NotNull(entityLoadInfoFactory, nameof(entityLoadInfoFactory));
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

            IClrPropertyGetter primaryKeyPropertyGetter = navigation
                .GetTargetType()
                .FindPrimaryKey()
                .Properties
                .Single()
                .GetGetter();

            IDictionary<object, TRelated> replacementMap = relatedEntitiesFactory()
                .ToDictionary(
                    related => primaryKeyPropertyGetter.GetClrValue(related));

            IEnumerable<object> newCollectionItems = collection
                .Select(original =>
                    replacementMap.TryGetValue(
                            primaryKeyPropertyGetter.GetClrValue(original),
                            out TRelated related)
                        ? related
                        : original)
                .Cast<object>()
                .ToList();

            collection.Clear();

            foreach (TRelated item in newCollectionItems)
            {
                inverseClrPropertySetter.SetClrValue(item, entity);

                if (tracking)
                {
                    InternalEntityEntry originalEntry = _stateManager.TryGetEntry(item);
                    if (originalEntry != null)
                    {
                        _stateManager.StopTracking(originalEntry);
                    }

                    base.StartTracking(
                        LoadEntity(
                            item,
                            targetEntityType,
                            entity,
                            inverseNavigation),
                        targetEntityType);
                }
            }
        }

        /// <inheritdoc />
        public override void StartTracking(object entity, EntityTrackingInfo entityTrackingInfo)
            => base.StartTracking(
                LoadEntity(
                    Check.NotNull(entity, nameof(entity)),
                    _model.FindEntityType(entity.GetType()),
                    null,
                    null),
                entityTrackingInfo);

        /// <inheritdoc />
        public override void StartTracking(object entity, IEntityType entityType)
            => base.StartTracking(
                LoadEntity(entity, entityType, null, null),
                entityType);

        private object LoadEntity(object entity, IEntityType entityType, object owner, INavigation owningNavigation)
        {
            if (entity.GetType() != entityType.ClrType)
            {
                entityType = _model.FindEntityType(entity.GetType());
            }

            entity = GetEntity(
                entityType.FindPrimaryKey(),
                _entityLoadInfoFactory.Create(entity, entityType, owner, owningNavigation),
                true,
                true);

            IEnumerable<INavigation> ownedNavigations = entityType
                .GetNavigations()
                .Where(navigation => navigation.ForeignKey.IsOwnership);

            foreach (INavigation ownedNavigation in ownedNavigations)
            {
                IEntityType targetEntityType = ownedNavigation.GetTargetType();
                var documentOrCollection = ownedNavigation.GetGetter().GetClrValue(entity);

                if (ownedNavigation.IsCollection())
                {
                    IEnumerable collection = (IEnumerable) documentOrCollection;
                    foreach (object document in collection)
                    {
                        base.StartTracking(
                            LoadEntity(document, targetEntityType, entity, ownedNavigation),
                            targetEntityType);
                    }
                }
                else
                {
                    base.StartTracking(
                        LoadEntity(documentOrCollection, targetEntityType, entity, ownedNavigation),
                        targetEntityType);
                }
            }

            return entity;
        }
    }
}
