using System;
using System.Linq.Expressions;

namespace CommonLib.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> exp1,
            Expression<Func<T, bool>> exp2)
        {
            var parameter = Expression.Parameter(typeof(T), "p");
            var visitor = new ReplaceParameterExpressionVisitor(parameter);
            var left = visitor.Replace(exp1.Body);
            var right = visitor.Replace(exp2.Body);
            var body = Expression.And(left, right);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> exp1,
            Expression<Func<T, bool>> exp2)
        {
            var parameter = Expression.Parameter(typeof(T), "p");
            var visitor = new ReplaceParameterExpressionVisitor(parameter);
            var left = visitor.Replace(exp1.Body);
            var right = visitor.Replace(exp2.Body);
            var body = Expression.Or(left, right);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> Not<T>(
            this Expression<Func<T, bool>> exp)
        {
            var parameter = Expression.Parameter(typeof(T), "p");
            var visitor = new ReplaceParameterExpressionVisitor(parameter);
            var nexp = visitor.Replace(exp.Body);
            var body = Expression.Not(nexp);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

    class ReplaceParameterExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ReplaceParameterExpressionVisitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        public Expression Replace(Expression exp)
        {
            return Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}
