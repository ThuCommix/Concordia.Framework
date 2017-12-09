﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nightingale.Queries.Tokens
{
    internal class SqlTokenComposerService : ISqlTokenComposerService
    {
        /// <summary>
        /// Composes the sql based on the specified tokens.
        /// </summary>
        /// <param name="sqlTokens">The sql tokens.</param>
        /// <returns>Returns the composed sql.</returns>
        public TokenComposerResult ComposeSql(IEnumerable<SqlToken> sqlTokens)
        {
            var rootSelectToken = sqlTokens.FirstOrDefault(x => x.TokenType == SqlTokenType.Select);
            if (rootSelectToken == null)
                throw new QueryException("The query does not contain a select statement.");

            var joinTokens = sqlTokens.Where(x => x.TokenType == SqlTokenType.Join);

            var queryBuilder = new StringBuilder();
            queryBuilder.Append(rootSelectToken.Sql);
            queryBuilder.Append(" ");
            queryBuilder.Append(string.Join(" ", joinTokens.Select(x => x.Sql)));
            queryBuilder.Append(" WHERE ");

            var parameterIndex = 0;
            var parameters = new List<QueryParameter>();

            var isFirstGroup = true;

            foreach(var token in sqlTokens.Where(x => x.TokenType == SqlTokenType.Condition || x.TokenType == SqlTokenType.ConditionLink))
            {
                if(token is ConditionSqlToken conditionToken)
                {
                    var parameter = Query.GetQueryParameter($"@p{parameterIndex++}", conditionToken.Value, conditionToken.FieldMetadata);
                    parameters.Add(parameter);

                    queryBuilder.Append($"{token.Sql} {parameter.Name} ");
                }

                if(token is ConditionLinkSqlToken conditionLinkToken)
                {
                    if(conditionLinkToken.LinkType == ConditionLinkType.Start)
                    {
                        if(isFirstGroup)
                        {
                            isFirstGroup = false;
                            queryBuilder.Append("(");
                        }
                        else
                        {
                            queryBuilder.Append($"{conditionLinkToken.Sql} (");
                        }
                    }
                    else if (conditionLinkToken.LinkType == ConditionLinkType.End)
                    {
                        queryBuilder.Append(")");
                    }
                    else
                    {
                        queryBuilder.Append(token.Sql);
                    }
                }
            }

            var command = queryBuilder.ToString().Replace("  ", " ").Replace(" )", ")");

            return new TokenComposerResult(command, parameters);
        }
    }
}