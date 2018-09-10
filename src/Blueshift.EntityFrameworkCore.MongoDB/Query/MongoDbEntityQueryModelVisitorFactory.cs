using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbEntityQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        /// <inheritdoc />
        public MongoDbEntityQueryModelVisitorFactory(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies,
            [NotNull] MongoDbEntityQueryModelVisitorDependencies mongoDbEntityQueryModelVisitorDependencies)
            : base(Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)))
        {
            MongoDbDependencies
                = Check.NotNull(mongoDbEntityQueryModelVisitorDependencies, nameof(mongoDbEntityQueryModelVisitorDependencies));
        }

        /// <summary>
        /// Depedencies used to create a <see cref="MongoDbEntityQueryModelVisitor"/>.
        /// </summary>
        public MongoDbEntityQueryModelVisitorDependencies MongoDbDependencies { get; }

        /// <inheritdoc />
        public override EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new MongoDbEntityQueryModelVisitor(
                Dependencies,
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                MongoDbDependencies);
    }
}