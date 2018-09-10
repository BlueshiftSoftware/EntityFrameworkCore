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
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            var mockMongoCollection = new Mock<IMongoCollection<Employee>>();
            var mockMongoDbTypeMappingSource = new Mock<IMongoDbTypeMappingSource>();

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

            IMongoDbWriteModelFactorySelector writeModelFactorySelector =
                new MongoDbWriteModelFactorySelector(
                    Mock.Of<IValueGeneratorSelector>(),
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
                        mockMongoDbTypeMappingSource.Object))
                    .AddConventions(
                        new CoreConventionSetBuilder(
                            new CoreConventionSetBuilderDependencies(
                                mockMongoDbTypeMappingSource.Object,
                                null,
                                null,
                                null,
                                null))
                            .CreateConventionSet()));

            EntityType zooEntityType = model.AddEntityType(typeof(ZooEntity));
            zooEntityType.Builder
                .GetOrCreateProperties(typeof(ZooEntity).GetTypeInfo().GetProperties(), ConfigurationSource.Explicit);
            EntityType entityType = model.AddEntityType(typeof(Employee));
            entityType.Builder
                .HasBaseType(zooEntityType, ConfigurationSource.Explicit)
                .GetOrCreateProperties(typeof(Employee).GetTypeInfo().GetProperties(), ConfigurationSource.Explicit);

            new EntityTypeBuilder(entityType.Builder)
                .ForMongoDbFromCollection(collectionName: "employees");

            IReadOnlyList<IUpdateEntry> entityEntries = new[] { EntityState.Added, EntityState.Deleted, EntityState.Modified }
                .Select(entityState =>
                    {
                        var mockUpdateEntry = new Mock<InternalEntityEntry>(
                            null,
                            entityType);
                        var entity = new Employee();

                        mockUpdateEntry
                            .SetupGet(updateEntry => updateEntry.EntityState)
                            .Returns(entityState);
                        mockUpdateEntry
                            .SetupGet(updateEntry => updateEntry.EntityType)
                            .Returns(entityType);
                        mockUpdateEntry
                            .SetupGet(updateEntry => updateEntry.Entity)
                            .Returns(entity);

                        return mockUpdateEntry.Object;
                    })
                .ToList();

            Assert.Equal(entityEntries.Count, mongoDbDatabase.SaveChanges(entityEntries));
        }
    }
}