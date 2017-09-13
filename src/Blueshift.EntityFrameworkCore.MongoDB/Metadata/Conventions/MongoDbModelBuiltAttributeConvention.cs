using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class MongoDbModelBuiltAttributeConvention<TModelAttribute> : IModelBuiltConvention
        where TModelAttribute : Attribute
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected MongoDbModelBuiltAttributeConvention([NotNull] DbContext dbContext)
        {
            DbContext = Check.NotNull(dbContext, nameof(dbContext));
        }

        /// <summary>
        /// Gets the <see cref="DbContext"/> whose model is being configured.
        /// </summary>
        protected DbContext DbContext { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            IEnumerable<TModelAttribute> modelAttributes = DbContext.GetType()
                .GetTypeInfo()
                .GetCustomAttributes<TModelAttribute>();
            foreach (TModelAttribute modelAttribute in modelAttributes)
            {
                if (!Apply(modelBuilder, modelAttribute))
                {
                    break;
                }
            }
            return modelBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract bool Apply([NotNull] InternalModelBuilder modelBuilder,
            [NotNull] TModelAttribute modelAttribute);
    }
}