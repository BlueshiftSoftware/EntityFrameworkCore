using Blueshift.EntityFrameworkCore.Metadata;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Metadata
{
    public class MongoDbEntityTypeAnnotationsTests
    {
        [Fact]
        public void Collection_name_is_pluralized_camel_cased_entity_type_by_default()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Animal), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType);
            Assert.Equal(MongoDbUtilities.Pluralize(MongoDbUtilities.ToCamelCase(nameof(Animal))),
                mongoDbEntityTypeAnnotations.CollectionName);
        }

        [Fact]
        public void Can_write_collection_name()
        {
            var collectionName = "myCollection";
            var model = new Model();
            var entityType = new EntityType(typeof(Animal), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType)
            {
                CollectionName = collectionName
            };
            Assert.Equal(collectionName, mongoDbEntityTypeAnnotations.CollectionName);
        }

        [Fact]
        public void Discriminator_is_type_name_by_default()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Animal), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType);
            Assert.Equal(typeof(Animal).Name, mongoDbEntityTypeAnnotations.Discriminator);
        }

        [Fact]
        public void Can_write_discriminator()
        {
            var discriminator = "discriminator";
            var model = new Model();
            var entityType = new EntityType(typeof(Animal), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType)
            {
                Discriminator = discriminator
            };
            Assert.Equal(discriminator, mongoDbEntityTypeAnnotations.Discriminator);
        }
    }
}