using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    internal class MongoDbNondeterministicResultCheckingVisitor : QueryModelVisitorBase
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public MongoDbNondeterministicResultCheckingVisitor([NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => _logger = logger;

        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<MongoDbNondeterministicResultCheckingVisitor>(this).Visit);

            base.VisitQueryModel(queryModel);
        }

        protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
        {
            if (resultOperators.Any(o => o is SkipResultOperator || o is TakeResultOperator)
                && !queryModel.BodyClauses.OfType<OrderByClause>().Any())
            {
                _logger.RowLimitingOperationWithoutOrderByWarning(queryModel);
            }

            if (resultOperators.Any(o => o is FirstResultOperator)
                && !queryModel.BodyClauses.OfType<OrderByClause>().Any()
                && !queryModel.BodyClauses.OfType<WhereClause>().Any())
            {
                _logger.FirstWithoutOrderByAndFilterWarning(queryModel);
            }

            base.VisitResultOperators(resultOperators, queryModel);
        }
    }
}