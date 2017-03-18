using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter
{
    public class EntityFrameworkConventionPack : ConventionPack
    {
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