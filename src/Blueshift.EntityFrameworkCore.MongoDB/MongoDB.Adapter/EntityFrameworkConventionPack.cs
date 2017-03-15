using Blueshift.EntityFrameworkCore.Annotations;
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
                new AbstractClassMapConvention(),
                new BsonClassMapAttributeConvention<DerivedTypeAttribute>(),
                new BsonClassMapAttributeConvention<DiscriminatorAttribute>(),
                new BsonClassMapAttributeConvention<RootTypeAttribute>(),
                new IgnoreEmptyEnumerablesConvention(),
                new IgnoreNullOrEmptyStringsConvention(),
                new KeyAttributeConvention()
            });
        }
    }
}