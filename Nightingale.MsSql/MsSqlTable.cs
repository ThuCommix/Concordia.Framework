﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nightingale.Entities;
using Nightingale.Metadata;
using Nightingale.Queries;

namespace Nightingale.MsSql
{
    public class MsSqlTable<T> : Table<T> where T : Entity
    {
        private static Dictionary<string, string> DataTypeMapping = new Dictionary<string, string>
        {
            {"int", "int" },
            {"decimal", "decimal" },
            {"bool", "bit" },
            {"DateTime", "datetime" }
        };

        /// <summary>
        /// Initializes a new MsSqlTable class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public MsSqlTable(IConnection connection) : base(connection)
        {
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        public override void Create()
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.Tables WHERE Name=N'{Metadata.Table}' AND Type=N'U')");
            commandBuilder.AppendLine($"CREATE TABLE {Metadata.Table} (");
            var fieldDefinitions = new List<string>();
            fieldDefinitions.Add("Id int NOT NULL IDENTITY(1, 1) PRIMARY KEY");
            fieldDefinitions.Add("Deleted bit NOT NULL");
            fieldDefinitions.Add("Version int NOT NULL");

            foreach (var field in Metadata.Fields.Where(x => x.Name != "Id" && x.Name != "Deleted" && x.Name != "Version"))
            {
                fieldDefinitions.Add($"{field.Name} {GetDataTypeMapping(field)}{GetDecimalPrecisionCommand(field)} {GetMandatoryCommand(field.Mandatory)} {GetUniqueCommand(field.Unique)}");
            }

            commandBuilder.AppendLine(string.Join(",", fieldDefinitions));

            commandBuilder.AppendLine(");");

            var query = new Query(commandBuilder.ToString(), Type);
            Connection.ExecuteNonQuery(query);
        }

        /// <summary>
        /// Deletes the table.
        /// </summary>
        public override void Delete()
        {
            var query = new Query($"DROP TABLE IF EXISTS {Metadata.Table}", Type);
            Connection.ExecuteNonQuery(query);
        }

        /// <summary>
        /// Gets a value indicating whether the table exists.
        /// </summary>
        public override bool Exists()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the data type mapping.
        /// </summary>
        /// <param name="field">The field metadata.</param>
        /// <returns>Returns the datatype mapped string.</returns>
        private string GetDataTypeMapping(FieldMetadata field)
        {
            return field.FieldType == "string" ? GetStringFieldCommand(field.MaxLength) : DataTypeMapping[field.IsComplexFieldType ? "int" : field.FieldType];
        }

        /// <summary>
        /// Gets the mandatory sql command.
        /// </summary>
        /// <param name="mandatory">The mandatory flag.</param>
        /// <returns>Returns the command.</returns>
        private string GetMandatoryCommand(bool mandatory)
        {
            return mandatory ? "NOT NULL" : string.Empty;
        }

        /// <summary>
        /// Gets the unqiue sql command.
        /// </summary>
        /// <param name="unique">The unique flag.</param>
        /// <returns>Returns the command.</returns>
        private string GetUniqueCommand(bool unique)
        {
            return unique ? "UNIQUE" : string.Empty;
        }

        /// <summary>
        /// Gets the decimal precision/scale sql command.
        /// </summary>
        /// <param name="field">The field metadata.</param>
        /// <returns>Returns the command.</returns>
        private string GetDecimalPrecisionCommand(FieldMetadata field)
        {
            if (field.FieldType == "decimal")
            {
                return $"({field.DecimalPrecision}, {field.DecimalScale})";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the datatype for string fields.
        /// </summary>
        /// <param name="maxLength">The maxlength.</param>
        /// <returns>Returns the command.</returns>
        private string GetStringFieldCommand(int maxLength)
        {
            return maxLength == 0 ? "text" : $"varchar({maxLength})";
        }
    }
}
