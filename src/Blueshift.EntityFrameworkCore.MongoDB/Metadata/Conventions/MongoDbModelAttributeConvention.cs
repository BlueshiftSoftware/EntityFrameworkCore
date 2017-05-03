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
    public abstract class MongoDbModelAttributeConvention<TModelAttribute> : IModelConvention
        where TModelAttribute : Attribute, IModelConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected MongoDbModelAttributeConvention([NotNull] DbContext dbContext)
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
        public virtual InternalModelBuilder Apply([NotNull] InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            foreach (TModelAttribute modelAttribute in GetAttributes(DbContext.GetType()))
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
        protected virtual IEnumerable<TModelAttribute> GetAttributes([NotNull] Type dbContextType)
            => Check.NotNull(dbContextType, nameof(dbContextType))
                .GetTypeInfo()
                .GetCustomAttributes<TModelAttribute>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool Apply([NotNull] InternalModelBuilder modelBuilder,
            [NotNull] TModelAttribute modelAttribute)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(modelAttribute, nameof(modelAttribute));
            modelAttribute.Apply(modelBuilder);
            return true;
        }
    }
}