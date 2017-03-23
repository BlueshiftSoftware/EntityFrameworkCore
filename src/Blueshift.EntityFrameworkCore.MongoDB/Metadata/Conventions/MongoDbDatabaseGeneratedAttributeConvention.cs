using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbDatabaseGeneratedAttributeConvention : DatabaseGeneratedAttributeConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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