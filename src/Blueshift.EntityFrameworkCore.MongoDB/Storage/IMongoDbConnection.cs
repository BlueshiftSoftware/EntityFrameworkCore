using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.Storage
{
    public interface IMongoDbConnection
    {
        IMongoDatabase GetDatabase();

        Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

        void DropDatabase();

        Task DropDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

        IMongoCollection<TEntity> GetCollection<TEntity>();

        IQueryable<TEntity> Query<TEntity>();
    }
}