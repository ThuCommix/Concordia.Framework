﻿using System.Data;
using ThuCommix.EntityFramework.Metadata;

namespace ThuCommix.EntityFramework.Extensions
{
    public static class FieldMetadataExtensions
    {
        /// <summary>
        /// Gets the sql db type based on the specified field metadata.
        /// </summary>
        /// <param name="fieldMetadata">The field metadata.</param>
        /// <returns>Returns the sqldbtype.</returns>
        public static SqlDbType GetSqlDbType(this FieldMetadata fieldMetadata)
        {
            if (fieldMetadata.FieldType == "string")
            {
                return fieldMetadata.MaxLength > 0 ? SqlDbType.VarChar : SqlDbType.Text;
            }

            if (fieldMetadata.FieldType == "decimal")
                return SqlDbType.Decimal;

            if (fieldMetadata.FieldType == "bool")
                return SqlDbType.Bit;

            if (fieldMetadata.FieldType == "DateTime")
                return SqlDbType.DateTime;

            return SqlDbType.Int;
        }
    }
}
