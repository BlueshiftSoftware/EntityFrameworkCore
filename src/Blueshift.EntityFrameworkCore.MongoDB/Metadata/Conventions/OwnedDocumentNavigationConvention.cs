using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc cref="INavigationAddedConvention" />
    /// <inheritdoc cref="INavigationRemovedConvention" />
    public class OwnedDocumentNavigationConvention : INavigationAddedConvention, INavigationRemovedConvention
    {
        /// <inheritdoc />
        public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
        {

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Apply(
            InternalEntityTypeBuilder sourceEntityTypeBuilder,
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo memberInfo)
        {
            throw new NotImplementedException();
        }
    }
}
