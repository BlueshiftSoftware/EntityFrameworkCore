using System;
using System.Reflection;
using Blueshift.EntityFrameworkCore.Metadata.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DiscriminatorAttribute : Attribute, IEntityTypeAttribute, IBsonClassMapAttribute
    {
        public DiscriminatorAttribute([NotNull] string discriminator)
        {
            if (String.IsNullOrWhiteSpace(discriminator))
            {
                throw new ArgumentException("Discriminator cannot be null, empty, or white-space.", nameof(discriminator));
            }
            Discriminator = discriminator;
        }

        public virtual string Discriminator { get; }

        public virtual void Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder == null)
            {
                throw new ArgumentNullException(nameof(entityTypeBuilder));
            }
            entityTypeBuilder
                .MongoDb(ConfigurationSource.DataAnnotation)
                .HasDiscriminator(Discriminator)
                .SetDiscriminatorIsRequired(entityTypeBuilder.Metadata.IsAbstract());
        }

        public virtual void Apply([NotNull] BsonClassMap classMap)
        {
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            classMap.SetDiscriminator(Discriminator);
            if (classMap.ClassType.GetTypeInfo().IsAbstract)
                classMap.SetDiscriminatorIsRequired(true);
        }
    }
}