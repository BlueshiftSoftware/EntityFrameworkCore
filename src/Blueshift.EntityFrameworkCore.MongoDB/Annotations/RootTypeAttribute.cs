using System;
using Blueshift.EntityFrameworkCore.Metadata.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RootTypeAttribute : Attribute, IEntityTypeAttribute, IBsonClassMapAttribute
    {
        public virtual void Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder == null)
            {
                throw new ArgumentNullException(nameof(entityTypeBuilder));
            }
            entityTypeBuilder
                .MongoDb(ConfigurationSource.DataAnnotation)
                .SetIsRootType(true);
        }

        public virtual void Apply([NotNull] BsonClassMap classMap)
        {
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            classMap.SetIsRootClass(true);
        }
    }
}