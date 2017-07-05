﻿using System;
using System.Linq.Expressions;

namespace Concordia.Framework.Queries
{
    internal static class QueryHelper
    {
        /// <summary>
        /// Gets the property path.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>Returns the property path.</returns>
        public static string GetPropertyPath(Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;
            MemberExpression memberExpression = null;

            if(lambdaExpression.Body is BinaryExpression)
            {
                var binaryExpression = lambdaExpression.Body as BinaryExpression;
                memberExpression = binaryExpression.Left as MemberExpression;
            }
            else
            {
                memberExpression = lambdaExpression.Body as MemberExpression;
            }

            var propertyPath = (memberExpression.Expression.ToString() + $".{memberExpression.Member.Name}");
            var lambdaParameterName = $"{lambdaExpression.Parameters[0].Name}.";
            if (propertyPath.StartsWith(lambdaParameterName))
            {
                propertyPath = propertyPath.Substring(lambdaParameterName.Length, propertyPath.Length - lambdaParameterName.Length);
            }

            return propertyPath;
        }

        /// <summary>
        /// Gets the operator symbol.
        /// </summary>
        /// <param name="op">The operator.</param>
        /// <returns>Returns the operator sql symbol.</returns>
        public static string GetOperatorSymbol(Operator op, bool isNullType = false)
        {
            if (isNullType && op == Operator.Equal)
                return "IS";

            if (isNullType && op == Operator.NotEqual)
                return "IS NOT";

            switch (op)
            {
                case Operator.Equal:
                    return "=";
                case Operator.NotEqual:
                    return "!=";
                case Operator.GreaterThan:
                    return ">";
                case Operator.GreaterThanOrEqual:
                    return ">=";
                case Operator.LessThan:
                    return "<";
                case Operator.LessThanOrEqual:
                    return "<=";
                case Operator.AndAlso:
                    return "AND";
                case Operator.OrElse:
                    return "OR";
                case Operator.Like:
                    return "LIKE";
                default:
                    throw new NotSupportedException($"The operator '{op}' was not supported.");
            }
        }

        public static Operator ConvertOperator(ExpressionType expressionType)
        {
            switch(expressionType)
            {
                case ExpressionType.Equal:
                    return Operator.Equal;
                case ExpressionType.NotEqual:
                    return Operator.NotEqual;
                case ExpressionType.GreaterThan:
                    return Operator.GreaterThan;
                case ExpressionType.GreaterThanOrEqual:
                    return Operator.GreaterThanOrEqual;
                case ExpressionType.LessThan:
                    return Operator.LessThan;
                case ExpressionType.LessThanOrEqual:
                    return Operator.LessThanOrEqual;
                case ExpressionType.AndAlso:
                    return Operator.AndAlso;
                case ExpressionType.OrElse:
                    return Operator.OrElse;
                default:
                    throw new NotSupportedException($"The expression-type '{expressionType} was not supported.'");
            }
        }
    }
}
