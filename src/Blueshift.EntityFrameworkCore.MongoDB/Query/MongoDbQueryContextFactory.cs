using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbQueryContextFactory : QueryContextFactory
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        /// <inheritdoc />
        public MongoDbQueryContextFactory(
            [NotNull] QueryContextDependencies queryContextDependencies,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(Check.NotNull(queryContextDependencies, nameof(queryContextDependencies)))
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <inheritdoc />
        public override QueryContext Create()
            => new MongoDbQueryContext(Dependencies, CreateQueryBuffer, _mongoDbConnection);

        /// <inheritdoc />
        protected override IQueryBuffer CreateQueryBuffer()
            => new MongoDbQueryBuffer(Dependencies);
    }
}