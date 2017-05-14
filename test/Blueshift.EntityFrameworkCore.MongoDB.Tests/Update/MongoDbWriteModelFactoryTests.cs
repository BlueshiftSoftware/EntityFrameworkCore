using System;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.Update;
using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Update
{
    public class MongoDbWriteModelFactoryTests
    {
        private InternalEntityEntry GetEntityEntry(EntityState entityState, object entity)
        {
            var mockStateManager = new Mock<IStateManager>();
            var mockValueGenerationManager = new Mock<IValueGenerationManager>();
            var mockInternalEntityEntryNotifier = new Mock<IInternalEntityEntryNotifier>();
            mockStateManager.SetupGet(stateManager => stateManager.ValueGeneration)
                .Returns(() => mockValueGenerationManager.Object);
            mockStateManager.SetupGet(stateManager => stateManager.Notify)
                .Returns(() => mockInternalEntityEntryNotifier.Object);

            var model = new Model(
                new MongoDbConventionSetBuilder(
                    new MongoDbConventionSetBuilderDependencies(
                        new CurrentDbContext(
                            new ZooDbContext(
                                new DbContextOptions<ZooDbContext>()))))
                    .AddConventions(new CoreConventionSetBuilder().CreateConventionSet()));
            Type entityClrType = entity.GetType();
            EntityType entityType = model.AddEntityType(entityClrType);
            entityType.Builder
                .GetOrCreateProperties(entityClrType.GetTypeInfo().GetProperties(), ConfigurationSource.Convention);
            new EntityTypeBuilder(entityType.Builder)
                .ForMongoDbFromCollection(collectionName: "employees");

            var entityEntry = new InternalClrEntityEntry(mockStateManager.Object, entityType, entity);
            new ObjectIdValueGenerator().Next(entityEntry.ToEntityEntry());
            entityEntry.SetEntityState(entityState, acceptChanges: true);
            return entityEntry;
        }

        private IMongoDbWriteModelFactory<TEntity> CreateMongoDbWriteModelFactory<TEntity>(IEntityType entityType)
        {
            IValueGeneratorSelector valueGeneratorSelector = new ValueGeneratorSelector(
                new ValueGeneratorSelectorDependencies(
                    new ValueGeneratorCache(
                        new ValueGeneratorCacheDependencies())));
            return new MongoDbWriteModelFactory<TEntity>(valueGeneratorSelector, entityType);
        }

        [Fact]
        public void Creates_insert_one_model_for_added_entity()
        {
            var employee = new Employee();
            var entityEntry = GetEntityEntry(EntityState.Added, employee);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Employee>(entityEntry.EntityType);
            var insertOneModel = mongoDbWriteModelFactory.CreateWriteModel(entityEntry) as InsertOneModel<Employee>;
            Assert.NotNull(insertOneModel);
        }

        [Fact]
        public void Creates_insert_one_model_for_added_entity_and_updates_concurrency_field()
        {
            var tiger = new Tiger() { Name = "Pantheris" };
            Assert.Null(tiger.ConcurrencyField);
            var entityEntry = GetEntityEntry(EntityState.Added, tiger);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Animal>(entityEntry.EntityType);
            var insertOneModel = mongoDbWriteModelFactory.CreateWriteModel(entityEntry) as InsertOneModel<Animal>;
            Assert.NotNull(insertOneModel);
            Assert.Same(tiger, insertOneModel.Document);
        }

        [Fact]
        public void Creates_update_one_model_for_modified_entity_referencing_only_id()
        {
            var employee = new Employee();
            var entityEntry = GetEntityEntry(EntityState.Modified, employee);
            employee.FirstName = "Bob";
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Employee>(entityEntry.EntityType);
            var updateOneModel = mongoDbWriteModelFactory.CreateWriteModel(entityEntry) as UpdateOneModel<Employee>;
            FilterDefinition<Employee> filter = Builders<Employee>.Filter.Eq(record => record.Id, employee.Id);
            Assert.NotNull(updateOneModel);
        }

        [Fact]
        public void Creates_update_one_model_for_modified_entity_referencing_concurrency_field()
        {
            var tiger = new Tiger() { Name = "Pantheris" };
            var entityEntry = GetEntityEntry(EntityState.Modified, tiger);
            var concurrencyToken = Guid.NewGuid().ToString();
            typeof(Animal).GetProperty(nameof(Animal.ConcurrencyField)).SetValue(tiger, concurrencyToken);
            Assert.Equal(concurrencyToken, tiger.ConcurrencyField);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Animal>(entityEntry.EntityType);
            var updateOneModel = mongoDbWriteModelFactory.CreateWriteModel(entityEntry) as UpdateOneModel<Animal>;
            FilterDefinition<Animal> filter = Builders<Animal>.Filter.And(
                Builders<Animal>.Filter.Eq(record => record.Id, tiger.Id),
                Builders<Animal>.Filter.Eq(record => record.ConcurrencyField, tiger.ConcurrencyField));
            Assert.NotNull(updateOneModel);
        }

        [Fact]
        public void Creates_delete_one_model_for_deleted_entity_referencing_only_id()
        {
            var employee = new Employee();
            var entityEntry = GetEntityEntry(EntityState.Deleted, employee);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Employee>(entityEntry.EntityType);
            var deleteOneModel = mongoDbWriteModelFactory.CreateWriteModel(entityEntry) as DeleteOneModel<Employee>;
            FilterDefinition<Employee> filter = Builders<Employee>.Filter.Eq(record => record.Id, employee.Id);
            Assert.NotNull(deleteOneModel);
            Assert.Equal(filter.ToBsonDocument(), deleteOneModel.Filter.ToBsonDocument());
        }

        [Fact]
        public void Creates_delete_one_model_for_deleted_entity_referencing_concurrency_field()
        {
            var tiger = new Tiger() { Name = "Pantheris" };
            var concurrencyToken = Guid.NewGuid().ToString();
            typeof(Animal).GetProperty(nameof(Animal.ConcurrencyField)).SetValue(tiger, concurrencyToken);
            Assert.Equal(concurrencyToken, tiger.ConcurrencyField);
            var entityEntry = GetEntityEntry(EntityState.Deleted, tiger);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Animal>(entityEntry.EntityType);
            var deleteOneModel = mongoDbWriteModelFactory.CreateWriteModel(entityEntry) as DeleteOneModel<Animal>;
            FilterDefinition<Animal> filter = Builders<Animal>.Filter.And(
                Builders<Animal>.Filter.Eq(record => record.Id, tiger.Id),
                Builders<Animal>.Filter.Eq(record => record.ConcurrencyField, tiger.ConcurrencyField));
            Assert.NotNull(deleteOneModel);
            Assert.Equal(filter.ToBsonDocument(), deleteOneModel.Filter.ToBsonDocument());
        }
    }
}