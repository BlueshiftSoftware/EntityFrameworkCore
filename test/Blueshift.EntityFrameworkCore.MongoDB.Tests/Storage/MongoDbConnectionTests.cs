using System.Threading;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbConnectionTests
    {
        private readonly Mock<IMongoDatabase> _mockMongoDatabase;
        private readonly Mock<IMongoClientFactory> _mockMongoClientFactory;
        private readonly Mock<IMongoClient> _mockMongoClient;
        private readonly Mock<IMongoCollection<Employee>> _mockEmployee;
        private readonly Mock<IMongoDbTypeMappingSource> _mockMongoDbTypeMappingSource;
        private readonly IModel _model;

        public MongoDbConnectionTests()
        {
            _mockMongoDbTypeMappingSource = MockMongoDbTypeMappingSource();
            _model = GetModel();
            _mockMongoClient = MockMongoClient();
            _mockMongoClientFactory = MockMongoClientFactory();
            _mockMongoDatabase = MockMongoDatabase();
            _mockEmployee = MockEmployee();
        }

        private IModel GetModel()
        {
            var model = new ModelBuilder(
                new MongoDbConventionSetBuilder(
                    new MongoDbConventionSetBuilderDependencies(
                        new CurrentDbContext(
                            new ZooDbContext(
                                new DbContextOptions<ZooDbContext>())),
                        _mockMongoDbTypeMappingSource.Object,
                        Mock.Of<IMemberClassifier>(),
                        Mock.Of<IDiagnosticsLogger<DbLoggerCategory.Model>>()))
                    .AddConventions(
                        new CoreConventionSetBuilder(
                            new CoreConventionSetBuilderDependencies(
                                _mockMongoDbTypeMappingSource.Object,
                                null,
                                null,
                                null,
                                null))
                            .CreateConventionSet()))
                .ForMongoDbFromDatabase("zooDb")
                .Model
                .AsModel();
            new EntityTypeBuilder(model.Builder
                .Entity(typeof(Employee), ConfigurationSource.Explicit))
                .ForMongoDbFromCollection("employees");
            return model;
        }

        private Mock<IMongoClient> MockMongoClient()
        {
            var mockMongoClient = new Mock<IMongoClient>();
            mockMongoClient
                .Setup(mongoClient => mongoClient.GetDatabase("zooDb", It.IsAny<MongoDatabaseSettings>()))
                .Returns(() => _mockMongoDatabase.Object)
                .Verifiable();
            mockMongoClient
                .Setup(mongoClient => mongoClient.DropDatabase("zooDb", It.IsAny<CancellationToken>()))
                .Verifiable();
            return mockMongoClient;
        }

        private Mock<IMongoClientFactory> MockMongoClientFactory()
        {
            var mockMongoClientFactory = new Mock<IMongoClientFactory>();
            mockMongoClientFactory
                .Setup(mongoClientFactory => mongoClientFactory.CreateMongoClient())
                .Returns(() => _mockMongoClient.Object)
                .Verifiable();
            return mockMongoClientFactory;
        }

        private Mock<IMongoDatabase> MockMongoDatabase()
        {
            var mockMongoDatabase = new Mock<IMongoDatabase>();
            mockMongoDatabase
                .Setup(mongoDatabase => mongoDatabase.GetCollection<Employee>(
                    "employees",
                    It.IsAny<MongoCollectionSettings>()))
                .Returns(() => _mockEmployee.Object)
                .Verifiable();
            return mockMongoDatabase;
        }

        private Mock<IMongoCollection<Employee>> MockEmployee()
            => new Mock<IMongoCollection<Employee>>();

        private Mock<IMongoDbTypeMappingSource> MockMongoDbTypeMappingSource()
            => new Mock<IMongoDbTypeMappingSource>();

        [Fact]
        public void Get_database_calls_mongo_client_get_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClientFactory.Object, _model);
            Assert.Equal(_mockMongoDatabase.Object, mongoDbConnection.GetDatabase());
            _mockMongoClient
                .Verify(mongoClient => mongoClient.GetDatabase(
                        "zooDb",
                        It.IsAny<MongoDatabaseSettings>()),
                    Times.Once);
        }

        [Fact]
        public async Task Get_database_async_calls_mongo_client_get_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClientFactory.Object, _model);
            Assert.Equal(_mockMongoDatabase.Object, await mongoDbConnection.GetDatabaseAsync());
            _mockMongoClient
                .Verify(mongoClient => mongoClient.GetDatabase(
                        "zooDb",
                        It.IsAny<MongoDatabaseSettings>()),
                    Times.Once);
        }

        [Fact]
        public void Drop_database_calls_mongo_client_drop_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClientFactory.Object, _model);
            mongoDbConnection.DropDatabase();
            _mockMongoClient
                .Verify(mongoClient => mongoClient.DropDatabase(
                        "zooDb",
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task Drop_database_async_calls_mongo_client_drop_database_async()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClientFactory.Object, _model);
            await mongoDbConnection.DropDatabaseAsync();
            _mockMongoClient
                .Verify(mongoClient => mongoClient.DropDatabaseAsync(
                        "zooDb",
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public void Get_collection_calls_mongo_database_get_collection()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClientFactory.Object, _model);
            Assert.Equal(_mockEmployee.Object, mongoDbConnection.GetCollection<Employee>());
            _mockMongoDatabase
                .Verify(mongoDatabase => mongoDatabase.GetCollection<Employee>(
                        "employees",
                        It.IsAny<MongoCollectionSettings>()),
                    Times.Once);
        }
    }
}