using System.Reflection;
using Blueshift.EntityFrameworkCore.Metadata.Builders;
using Blueshift.EntityFrameworkCore.Metadata.Internal;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.Update;
using Blueshift.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Update
{
    public class MongoDbUpdateEntryExtensionsTests
    {
        private InternalEntityEntry GetEntityEntry(EntityState entityState, Employee employee, bool setId)
        {
            var mockStateManager = new Mock<IStateManager>();
            var mockValueGenerationManager = new Mock<IValueGenerationManager>();
            var mockInternalEntityEntryNotifier = new Mock<IInternalEntityEntryNotifier>();
            mockStateManager.SetupGet(stateManager => stateManager.ValueGeneration)
                .Returns(() => mockValueGenerationManager.Object);
            mockStateManager.SetupGet(stateManager => stateManager.Notify)
                .Returns(() => mockInternalEntityEntryNotifier.Object);

            var model = new Model(
                new MongoDbConventionSetBuilder(new CurrentDbContext(new ZooDbContext(new DbContextOptions<ZooDbContext>())))
                    .AddConventions(new CoreConventionSetBuilder().CreateConventionSet()));
            EntityType entityType = model.AddEntityType(typeof(Employee));
            entityType.Builder
                .GetOrCreateProperties(typeof(Employee).GetTypeInfo().GetProperties(), ConfigurationSource.Convention);
            entityType.Builder
                .MongoDb(ConfigurationSource.Convention)
                .FromCollection(collectionName: "employees");

            var entityEntry = new InternalClrEntityEntry(mockStateManager.Object, entityType, employee);
            if (setId)
                new ObjectIdValueGenerator().Next(entityEntry.ToEntityEntry());
            entityEntry.SetEntityState(entityState, acceptChanges: true);
            return entityEntry;
        }

        [Fact]
        public void Creates_insert_one_model_for_added_entity()
        {
            var employee = new Employee();
            var insertOneModel = GetEntityEntry(EntityState.Added, employee, true)
                .ToMongoDbWriteModel<Employee>() as InsertOneModel<Employee>;
            Assert.NotNull(insertOneModel);
        }

        [Fact]
        public void Creates_replace_one_model_for_modified_entity()
        {
            var employee = new Employee();
            var replaceOneModel = GetEntityEntry(EntityState.Modified, employee, true)
                .ToMongoDbWriteModel<Employee>() as ReplaceOneModel<Employee>;
            FilterDefinition<Employee> filter = Builders<Employee>.Filter.Eq(
                record => record.Id, employee.Id);
            Assert.NotNull(replaceOneModel);
            Assert.Equal(filter.ToBsonDocument(), replaceOneModel.Filter.ToBsonDocument());
        }

        [Fact]
        public void Creates_delete_one_model_for_deleted_entity()
        {
            var employee = new Employee();
            var deleteOneModel = GetEntityEntry(EntityState.Deleted, employee, true)
                .ToMongoDbWriteModel<Employee>() as DeleteOneModel<Employee>;
            FilterDefinition<Employee> filter = Builders<Employee>.Filter.Eq(
                record => record.Id, employee.Id);
            Assert.NotNull(deleteOneModel);
            Assert.Equal(filter.ToBsonDocument(), deleteOneModel.Filter.ToBsonDocument());
        }
    }
}