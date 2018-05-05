using System;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using Blueshift.EntityFrameworkCore.MongoDB.Update;
using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Update
{
    public class MongoDbWriteModelFactoryTests
    {
        private IUpdateEntry GetUpdateEntry(EntityState entityState, object entity)
        {
            var zooDbContext = new ZooDbContext();
            var entityEntry = zooDbContext.Add(entity);

            entityEntry.State = entityState;
            return ((IInfrastructure<InternalEntityEntry>)entityEntry).Instance;
        }

        private IMongoDbWriteModelFactory<TEntity> CreateMongoDbWriteModelFactory<TEntity>(IUpdateEntry updateEntry)
            => new MongoDbWriteModelFactorySelector(
                    new ValueGeneratorSelector(
                        new ValueGeneratorSelectorDependencies(
                            new ValueGeneratorCache(
                                new ValueGeneratorCacheDependencies()))),
                    new MongoDbWriteModelFactoryCache())
                .Select<TEntity>(updateEntry);

        [Fact]
        public void Creates_insert_one_model_for_added_entity()
        {
            var employee = new Employee();
            var updateEntry = GetUpdateEntry(EntityState.Added, employee);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Employee>(updateEntry);
            var insertOneModel = mongoDbWriteModelFactory.CreateWriteModel(updateEntry) as InsertOneModel<Employee>;
            Assert.NotNull(insertOneModel);
            Assert.Same(employee, insertOneModel.Document);
        }

        [Fact]
        public void Creates_insert_one_model_for_added_entity_and_updates_concurrency_field()
        {
            var tiger = new Tiger() { Name = "Pantheris" };
            Assert.Null(tiger.ConcurrencyField);
            var entityEntry = GetUpdateEntry(EntityState.Added, tiger);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Animal>(entityEntry);
            var insertOneModel = mongoDbWriteModelFactory.CreateWriteModel(entityEntry) as InsertOneModel<Animal>;
            Assert.NotNull(insertOneModel);
            Assert.Same(tiger, insertOneModel.Document);
            Assert.NotNull(tiger.ConcurrencyField);
            Assert.NotEmpty(tiger.ConcurrencyField);
        }

        [Fact]
        public void Creates_update_one_model_for_modified_entity_referencing_only_id()
        {
            var employee = new Employee();
            var updateEntry = GetUpdateEntry(EntityState.Modified, employee);
            employee.FirstName = "Bob";
            IMongoDbWriteModelFactory<Employee> mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Employee>(updateEntry);
            var updateOneModel = mongoDbWriteModelFactory.CreateWriteModel(updateEntry) as UpdateOneModel<Employee>;
            FilterDefinition<Employee> filter = Builders<Employee>.Filter.Eq(record => record.Id, employee.Id);
            Assert.NotNull(updateOneModel);
            Assert.Equal(updateOneModel.Filter.ToJson(),
                filter.ToJson());
        }

        [Fact]
        public void Creates_update_one_model_for_modified_entity_referencing_concurrency_field()
        {
            var tiger = new Tiger() { Name = "Pantheris" };
            var updtaeEntry = GetUpdateEntry(EntityState.Modified, tiger);
            var concurrencyToken = Guid.NewGuid().ToString();
            typeof(Animal).GetProperty(nameof(Animal.ConcurrencyField)).SetValue(tiger, concurrencyToken);
            Assert.Equal(concurrencyToken, tiger.ConcurrencyField);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Animal>(updtaeEntry);
            var updateOneModel = mongoDbWriteModelFactory.CreateWriteModel(updtaeEntry) as UpdateOneModel<Animal>;
            FilterDefinition<Animal> filter = Builders<Animal>.Filter.And(
                Builders<Animal>.Filter.Eq(record => record.Id, tiger.Id),
                Builders<Animal>.Filter.Eq(record => record.ConcurrencyField, tiger.ConcurrencyField));
            Assert.NotNull(updateOneModel);
            Assert.Equal(updateOneModel.Filter.ToJson(),
                filter.ToJson());
        }

        [Fact]
        public void Creates_delete_one_model_for_deleted_entity_referencing_only_id()
        {
            var employee = new Employee();
            var updateEntry = GetUpdateEntry(EntityState.Deleted, employee);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Employee>(updateEntry);
            var deleteOneModel = mongoDbWriteModelFactory.CreateWriteModel(updateEntry) as DeleteOneModel<Employee>;
            FilterDefinition<Employee> filter = Builders<Employee>.Filter.Eq(record => record.Id, employee.Id);
            Assert.NotNull(deleteOneModel);
            Assert.Equal(filter.ToJson(), deleteOneModel.Filter.ToJson());
        }

        [Fact]
        public void Creates_delete_one_model_for_deleted_entity_referencing_concurrency_field()
        {
            var tiger = new Tiger() { Name = "Pantheris" };
            var concurrencyToken = Guid.NewGuid().ToString();
            typeof(Animal).GetProperty(nameof(Animal.ConcurrencyField)).SetValue(tiger, concurrencyToken);
            Assert.Equal(concurrencyToken, tiger.ConcurrencyField);
            var updateEntry = GetUpdateEntry(EntityState.Deleted, tiger);
            var mongoDbWriteModelFactory = CreateMongoDbWriteModelFactory<Animal>(updateEntry);
            var deleteOneModel = mongoDbWriteModelFactory.CreateWriteModel(updateEntry) as DeleteOneModel<Animal>;
            FilterDefinition<Animal> filter = Builders<Animal>.Filter.And(
                Builders<Animal>.Filter.Eq(record => record.Id, tiger.Id),
                Builders<Animal>.Filter.Eq(record => record.ConcurrencyField, tiger.ConcurrencyField));
            Assert.NotNull(deleteOneModel);
            Assert.Equal(filter.ToJson(), deleteOneModel.Filter.ToJson());
        }
    }
}