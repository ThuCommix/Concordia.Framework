﻿using System;

namespace Nightingale.Queries.Tokens
{
    public class TypeContextToken : Token
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Initializes a new TypeContextToken class.
        /// </summary>
        /// <param name="type">The type.</param>
        public TypeContextToken(Type type) : base(TokenType.TypeContext)
        {
            Type = type;
        }
    }
}