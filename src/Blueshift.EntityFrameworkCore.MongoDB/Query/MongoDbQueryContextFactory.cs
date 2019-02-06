using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbQueryContextFactory : QueryContextFactory
    {
        [NotNull] private readonly IEntityLoadInfoFactory _entityLoadInfoFactory;

        /// <inheritdoc />
        public MongoDbQueryContextFactory(
            [NotNull] QueryContextDependencies queryContextDependencies,
            [NotNull] IEntityLoadInfoFactory entityLoadInfoFactory)
            : base(
                Check.NotNull(queryContextDependencies, nameof(queryContextDependencies)))
        {
            _entityLoadInfoFactory = Check.NotNull(entityLoadInfoFactory, nameof(entityLoadInfoFactory));
        }

        /// <inheritdoc />
        public override QueryContext Create()
            => new MongoDbQueryContext(
                Dependencies,
                CreateQueryBuffer);

        /// <inheritdoc />
        protected override IQueryBuffer CreateQueryBuffer()
            => new MongoDbQueryBuffer(
                Dependencies,
                _entityLoadInfoFactory);
    }
}