using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbDatabaseTests
    {
        [Fact]
        public void Save_changes_returns_requested_document_count()
        {
            var queryCompilationContextFactory = Mock.Of<IQueryCompilationContextFactory>();
            var mockMongoDbConnection = new Mock<IMongoDbConnection>();
            var mockMongoCollection = new Mock<IMongoCollection<Animal>>();
            var mockMongoDbTypeMappingSource = new Mock<IMongoDbTypeMappingSource>();

            mockMongoCollection.Setup(mongoCollection => mongoCollection.BulkWrite(
                    It.IsAny<IEnumerable<WriteModel<Animal>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<WriteModel<Animal>> list, BulkWriteOptions options, CancellationToken token)
                    => new BulkWriteResult<Animal>.Acknowledged(
                        list.Count(),
                        matchedCount: 0,
                        deletedCount: list.OfType<DeleteOneModel<Animal>>().Count(),
                        insertedCount: list.OfType<InsertOneModel<Animal>>().Count(),
                        modifiedCount: list.OfType<ReplaceOneModel<Animal>>().Count(),
                        processedRequests: list,
                        upserts: new List<BulkWriteUpsert>()));

            mockMongoDbConnection.Setup(mockedMongoDbConnection => mockedMongoDbConnection.GetCollection<Animal>())
                .Returns(() => mockMongoCollection.Object);

            var databaseDepedencies = new DatabaseDependencies(queryCompilationContextFactory);

            IMongoDbWriteModelFactorySelector writeModelFactorySelector =
                new MongoDbWriteModelFactorySelector(
                    new MongoDbValueGeneratorSelector(
                        new ValueGeneratorSelectorDependencies(
                            new ValueGeneratorCache(
                                new ValueGeneratorCacheDependencies()))), 
                    new MongoDbWriteModelFactoryCache());
            var mongoDbDatabase = new MongoDbDatabase(
                databaseDepedencies,
                mockMongoDbConnection.Object,
                writeModelFactorySelector);

            var model = new Model(
                new MongoDbConventionSetBuilder(
                        new MongoDbConventionSetBuilderDependencies(
                            new CurrentDbContext(
                                new ZooDbContext(
                                    new DbContextOptions<ZooDbContext>())),
                            mockMongoDbTypeMappingSource.Object,
                            Mock.Of<IMemberClassifier>(),
                            Mock.Of<IDiagnosticsLogger<DbLoggerCategory.Model>>()))
                    .AddConventions(
                        new CoreConventionSetBuilder(
                                new CoreConventionSetBuilderDependencies(
                                    mockMongoDbTypeMappingSource.Object,
                                    null,
                                    null,
                                    null,
                                    null))
                            .CreateConventionSet()));

            EntityType animalEntityType = model.AddEntityType(typeof(Animal));
            animalEntityType.Builder
                .GetOrCreateProperties(typeof(Animal).GetTypeInfo().GetProperties(), ConfigurationSource.Explicit);
            EntityType tigerEntityType = model.GetOrAddEntityType(typeof(Tiger));

            IReadOnlyList<IUpdateEntry> entityEntries = new[] { EntityState.Added, EntityState.Deleted, EntityState.Modified }
                .Select(entityState =>
                    {
                        var entity = new Tiger();
                        var mockUpdateEntry = new Mock<InternalEntityEntry>(
                            null,
                            tigerEntityType);

                        mockUpdateEntry
                            .SetupGet(updateEntry => updateEntry.EntityState)
                            .Returns(entityState);
                        mockUpdateEntry
                            .SetupGet(updateEntry => updateEntry.EntityType)
                            .Returns(tigerEntityType);
                        mockUpdateEntry
                            .SetupGet(updateEntry => updateEntry.Entity)
                            .Returns(entity);
                        mockUpdateEntry
                            .Setup(updateEntry => updateEntry.ToEntityEntry())
                            .Returns(new EntityEntry(mockUpdateEntry.Object));

                        return mockUpdateEntry.Object;
                    })
                .ToList();

            Assert.Equal(entityEntries.Count, mongoDbDatabase.SaveChanges(entityEntries));
        }
    }
}