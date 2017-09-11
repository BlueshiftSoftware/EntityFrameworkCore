using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbEntityQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(
                Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext))
            )
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo EntityQueryMethodInfo
            = typeof(MongoDbEntityQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(EntityQuery));

        [UsedImplicitly]
        private static IQueryable<TEntity> EntityQuery<TEntity>(QueryContext queryContext)
            where TEntity : class
            => ((MongoDbQueryContext)queryContext).MongoDbConnection
                .Query<TEntity>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo SubEntityQueryMethodInfo
            = typeof(MongoDbEntityQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(SubEntityQuery));

        [UsedImplicitly]
        private static IQueryable<TEntity> SubEntityQuery<TBaseEntity, TEntity>(QueryContext queryContext)
            where TBaseEntity : class
            where TEntity : class, TBaseEntity
            => EntityQuery<TBaseEntity>(queryContext)
                .OfType<TEntity>();
    }
}
