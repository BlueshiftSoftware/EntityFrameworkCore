using System;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter
{
    /// <inheritdoc />
    /// <summary>
    /// Provides a set of conventions that configures the MongoDb C# Driver to work appropriately with the EntityFrameworkCore.
    /// </summary>
    public class EntityFrameworkConventionPack : ConventionPack
    {
        /// <summary>
        /// Registers the <see cref="EntityFrameworkConventionPack"/>.
        /// </summary>
        /// <param name="typeFilter"></param>
        public static void Register(Func<Type, bool> typeFilter)
        {
            ConventionRegistry.Register(
                "Blueshift.EntityFrameworkCore.MongoDb.Conventions",
                Instance,
                typeFilter);
        }

        /// <summary>
        /// The singleton instance of <see cref="EntityFrameworkConventionPack"/>.
        /// </summary>
        public static EntityFrameworkConventionPack Instance { get; } = new EntityFrameworkConventionPack();

        private EntityFrameworkConventionPack()
        {
            AddRange(new IConvention[]
            {
                new AbstractBaseClassConvention(),
                new KeyAttributeConvention(),
                new NavigationSrializationMemberMapConvention(),
                new NotMappedAttributeConvention() 
            });
        }
    }
}