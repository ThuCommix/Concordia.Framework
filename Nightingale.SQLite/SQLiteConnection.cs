﻿using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Nightingale.Entities;
using Nightingale.Queries;

namespace Nightingale.SQLite
{
    [DisplayName("SQLite")]
    public class SQLiteConnection : IConnection
    {
        /// <summary>
        /// A value indicating whether the connection is open.
        /// </summary>
        public bool IsOpen => !_isDisposed && _connection.State != ConnectionState.Closed;

        private readonly SqliteConnection _connection;
        private SqliteTransaction _currentTransaction;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new SQLiteConnection class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SQLiteConnection(string connectionString)
        {
            _connection = new SqliteConnection(connectionString);
        }

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        public void Open()
        {
            _connection.Open();
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public void Close()
        {
            _connection.Close();
        }

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level.</param>
        /// <returns>Returns an IDisposeable instance.</returns>
        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            _currentTransaction = _connection.BeginTransaction(isolationLevel == IsolationLevel.Serializable 
                || isolationLevel == IsolationLevel.ReadCommitted
                ? isolationLevel : IsolationLevel.ReadCommitted);

            return _currentTransaction;
        }

        /// <summary>
        /// Rollbacks the current transaction.
        /// </summary>
        public void Rollback()
        {
            _currentTransaction.Rollback();
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }

        /// <summary>
        /// Rollsbacks to the specified save point.
        /// </summary>
        /// <param name="savePoint">The save point.</param>
        public void RollbackTo(string savePoint)
        {
            var command = _connection.CreateCommand();
            command.Transaction = _currentTransaction;
            command.CommandText = $"ROLLBACK TRANSACTION TO SAVEPOINT {savePoint}";

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Releases the specified save point.
        /// </summary>
        /// <param name="savePoint">The save point.</param>
        public void Release(string savePoint)
        {
            var command = _connection.CreateCommand();
            command.Transaction = _currentTransaction;
            command.CommandText = $"RELEASE SAVEPOINT {savePoint}";

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        public void Commit()
        {
            _currentTransaction.Commit();
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }

        /// <summary>
        /// Creates a save point with the specified name.
        /// </summary>
        /// <param name="savePoint">The save point.</param>
        public void Save(string savePoint)
        {
            var command = _connection.CreateCommand();
            command.Transaction = _currentTransaction;
            command.CommandText = $"SAVEPOINT {savePoint}";

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Returns the count of the affected rows.</returns>
        public int ExecuteNonQuery(IQuery query)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query.Command;
            command.Transaction = _currentTransaction;
            command.Parameters.AddRange(query.Parameters.Select(x => new SqliteParameter(x.Name, x.Value ?? DBNull.Value)).ToArray());
            command.Prepare();

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the scalar query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Returns the result as object.</returns>
        public object ExecuteScalar(IQuery query)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query.Command;
            command.Transaction = _currentTransaction;
            command.Parameters.AddRange(query.Parameters.Select(x => new SqliteParameter(x.Name, x.Value ?? DBNull.Value)).ToArray());
            command.Prepare();

            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Returns an instance of the IDataReader.</returns>
        public IDataReader ExecuteReader(IQuery query)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query.Command;
            command.Transaction = _currentTransaction;
            command.Parameters.AddRange(query.Parameters.Select(x => new SqliteParameter(x.Name, x.Value ?? DBNull.Value)).ToArray());
            command.Prepare();

            return command.ExecuteReader();
        }

        /// <summary>
        /// Executes an insert and returns the id.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Returns the id.</returns>
        public int ExecuteInsert(IQuery query)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query.Command + " SELECT last_insert_rowid()";
            command.Transaction = _currentTransaction;
            command.Parameters.AddRange(query.Parameters.Select(x => new SqliteParameter(x.Name, x.Value ?? DBNull.Value)).ToArray());
            command.Prepare();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Disposes the data provider.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the data provider.
        /// </summary>
        /// <param name="disposing">The disposing state.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                _connection.Close();
                _connection.Dispose();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Gets the table object.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>Returns the table object.</returns>
        public Table<T> GetTable<T>() where T : Entity
        {
            return new SQLiteTable<T>(this);
        }
    }
}
