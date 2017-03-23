using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter
{
    /// <summary>
    /// Provides a set of conventions that configures the MongoDb C# Driver to work appropriately with the EntityFrameworkCore.
    /// </summary>
    public class EntityFrameworkConventionPack : ConventionPack
    {
        /// <summary>
        /// The singleton instance of <see cref="EntityFrameworkConventionPack"/>.
        /// </summary>
        public static EntityFrameworkConventionPack Instance { get; } = new EntityFrameworkConventionPack();

        private EntityFrameworkConventionPack()
        {
            AddRange(new IConvention[]
            {
                new AbstractClassConvention(),
                new IgnoreEmptyEnumerablesConvention(),
                new IgnoreNullOrEmptyStringsConvention(),
                new KeyAttributeConvention()
            });
        }
    }
}