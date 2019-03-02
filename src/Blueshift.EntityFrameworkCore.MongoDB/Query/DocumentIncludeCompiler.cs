using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    /// <summary>
    ///     Compiles <see cref="EntityFrameworkQueryableExtensions.Include{TEntity, TProperty}"/> references for a document-based database.
    /// </summary>
    public class DocumentIncludeCompiler : IIncludeCompiler
    {
        [NotNull] private readonly QueryCompilationContext _queryCompilationContext;
        [NotNull] private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;

        private IList<IncludeResultOperator> _includeResultOperators;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DocumentIncludeCompiler(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
        {
            _queryCompilationContext = Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            _querySourceTracingExpressionVisitorFactory
                = Check.NotNull(
                    querySourceTracingExpressionVisitorFactory,
                    nameof(querySourceTracingExpressionVisitorFactory));

            _includeResultOperators
                = _queryCompilationContext.QueryAnnotations
                    .OfType<IncludeResultOperator>()
                    .ToList();
        }

        /// <inheritdoc />
        public void CompileIncludes(QueryModel queryModel, bool trackingQuery, bool asyncQuery)
        {
            //throw new NotImplementedException();
        }
    }
}
