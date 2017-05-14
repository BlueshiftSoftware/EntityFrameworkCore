using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using Blueshift.EntityFrameworkCore.MongoDB.Update;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
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
            var mockStateManager = new Mock<IStateManager>();
            var mockMongoCollection = new Mock<IMongoCollection<Employee>>();
            var mockValueGenerationManager = new Mock<IValueGenerationManager>();
            var mockInternalEntityEntryNotifier = new Mock<IInternalEntityEntryNotifier>();
            var mockWriteModelFactorySelector = new Mock<IMongoDbWriteModelFactorySelector>();
            mockStateManager.SetupGet(stateManager => stateManager.ValueGeneration)
                .Returns(() => mockValueGenerationManager.Object);
            mockStateManager.SetupGet(stateManager => stateManager.Notify)
                .Returns(() => mockInternalEntityEntryNotifier.Object);
            mockMongoDbConnection.Setup(mockedMongoDbConnection => mockedMongoDbConnection.GetCollection<Employee>())
                .Returns(() => mockMongoCollection.Object);
            mockMongoCollection.Setup(mongoCollection => mongoCollection.BulkWrite(
                    It.IsAny<IEnumerable<WriteModel<Employee>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<WriteModel<Employee>> list, BulkWriteOptions options, CancellationToken token)
                    => new BulkWriteResult<Employee>.Acknowledged(
                        list.Count(),
                        matchedCount: 0,
                        deletedCount: list.OfType<DeleteOneModel<Employee>>().Count(),
                        insertedCount: list.OfType<InsertOneModel<Employee>>().Count(),
                        modifiedCount: list.OfType<UpdateOneModel<Employee>>().Count(),
                        processedRequests: list,
                        upserts: new List<BulkWriteUpsert>()));
            var databaseDepedencies = new DatabaseDependencies(queryCompilationContextFactory);
            mockWriteModelFactorySelector
                .Setup(selector => selector.CreateFactory<Employee>(It.IsAny<IEntityType>()))
                .Returns((IEntityType wmfentityType) => new MongoDbWriteModelFactory<Employee>(
                    Mock.Of<IValueGeneratorSelector>(),
                    wmfentityType));
            var mongoDbDatabase = new MongoDbDatabase(
                databaseDepedencies,
                mockMongoDbConnection.Object,
                mockWriteModelFactorySelector.Object);

            var model = new Model(
                new MongoDbConventionSetBuilder(
                    new MongoDbConventionSetBuilderDependencies(
                        new CurrentDbContext(
                            new ZooDbContext(
                                new DbContextOptions<ZooDbContext>()))))
                    .AddConventions(new CoreConventionSetBuilder().CreateConventionSet()));
            EntityType entityType = model.AddEntityType(typeof(Employee));
            entityType.Builder
                .GetOrCreateProperties(typeof(Employee).GetTypeInfo().GetProperties(), ConfigurationSource.Convention);
            new EntityTypeBuilder(entityType.Builder)
                .ForMongoDbFromCollection(collectionName: "employees");

            IReadOnlyList<InternalEntityEntry> entityEntries = new[] { EntityState.Added, EntityState.Deleted, EntityState.Modified }
                .Select(entityState =>
                    {
                        var entityEntry = new InternalClrEntityEntry(mockStateManager.Object, entityType, new Employee());
                        entityEntry.SetEntityState(entityState, acceptChanges: true);
                        return entityEntry;
                    })
                .ToList();

            Assert.Equal(entityEntries.Count, mongoDbDatabase.SaveChanges(entityEntries));
        }
    }
}