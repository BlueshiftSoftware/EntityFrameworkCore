//using System.Reflection;
//using JetBrains.Annotations;
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.EntityFrameworkCore.Utilities;
//using MongoDB.Driver;
//using Microsoft.EntityFrameworkCore.Infrastructure;

//// ReSharper disable once CheckNamespace
//namespace Blueshift.EntityFrameworkCore.Infrastructure
//{
//    public class MongoDbDatabaseProvider : DatabaseProvider<MongoDbOptionsExtension>, IMongoDbDatabaseProvider
//    {
//        private readonly IDbContextOptions _dbContextOptions;

//        public MongoDbDatabaseProvider(
//            [NotNull] DatabaseProviderDependencies dependencies,
//            [NotNull] IDbContextOptions dbContextOptions)
//            : base(Check.NotNull(dependencies, nameof(dependencies)))
//        {
//            _dbContextOptions = Check.NotNull(dbContextOptions, nameof(dbContextOptions));
//        }

//        public override string InvariantName
//            => GetType().GetTypeInfo().Assembly.GetName().Name;

//        public virtual IMongoClient MongoClient
//            => _dbContextOptions.FindExtension<MongoDbOptionsExtension>()?.MongoClient
//               ?? new MongoClient();
//    }
//}