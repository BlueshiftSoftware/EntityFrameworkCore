using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class MongoDbDatabaseGeneratedAttributeConvention : DatabaseGeneratedAttributeConvention
    {
        /// <inheritdoc />
        public override InternalPropertyBuilder Apply(
            InternalPropertyBuilder propertyBuilder,
            DatabaseGeneratedAttribute attribute,
            MemberInfo clrMember)
        {
            if (attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
            {
                propertyBuilder.Metadata
                    .DeclaringEntityType
                    .MongoDb()
                    .AssignIdOnInsert = true;
            }
            return base.Apply(propertyBuilder, attribute, clrMember);
        }
    }
}