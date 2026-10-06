using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InitiativeNextGen.Infra.Data
{
    public partial class MSSQL
    {
        private const int DefaultCommandTimeout = 3600;
        private const int DefaultBulkCopyTimeout = 3600;
    }







    // Base DB Access Code
    public partial class MSSQL
    {

        //Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        public int ExecuteNonQuery(string connectionString,
            string queryString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Invalid connection string.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(queryString))
                throw new ArgumentException("Invalid query.", nameof(queryString));

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                using SqlCommand command = new SqlCommand(queryString, connection)
                {
                    CommandTimeout = DefaultCommandTimeout
                };

                connection.Open();
                return command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                // Handle SQL-specific errors here if needed.
                // Log or perform some action specific to SQL-related issues.
                throw;
            }
            catch (Exception ex)
            {
                // Log the error. This is just a simple example. In production, use a logging framework.
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        public object? ExecuteScalar(string connectionString, string queryString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(queryString))
                throw new ArgumentException("Query string cannot be null or empty.", nameof(queryString));

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.CommandTimeout = DefaultCommandTimeout;
                        connection.Open();
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing scalar: {ex.Message}");
                throw;  // Re-throwing the exception if you want the calling code to handle it or be aware of it
            }
        }

        //Sends the CommandText to the Connection and builds a SqlDataReader.
        /*
         * USAGE
         * 
            SqlConnection connection;
            SqlDataReader reader = ExecuteReader(connectionString, queryString, out connection);
            using (reader)
            using (connection)
            {
                while (reader.Read())
                {
                    // Process data
                }
            }
         * 
        */
        public SqlDataReader ExecuteReader(string connectionString, string queryString, out SqlConnection connection)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(queryString))
                throw new ArgumentException("Query string cannot be null or empty.", nameof(queryString));

            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();

                SqlCommand command = new SqlCommand(queryString, connection)
                {
                    CommandTimeout = DefaultCommandTimeout
                };
                return command.ExecuteReader(CommandBehavior.CloseConnection); // This will ensure connection is closed when SqlDataReader is closed
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing query: {ex.Message}");
                connection.Dispose();  // Dispose the connection if an error occurs
                throw;
            }
        }



    }

    // Async Base DB Access Code
    public partial class MSSQL
    {
        //Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        public async Task<int> ExecuteNonQueryAsync(string connectionString,
            string queryString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Invalid connection string.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(queryString))
                throw new ArgumentException("Invalid query.", nameof(queryString));

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                using SqlCommand command = new SqlCommand(queryString, connection)
                {
                    CommandTimeout = DefaultCommandTimeout
                };

                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
            catch (SqlException sqlEx)
            {
                // Handle SQL-specific errors here if needed.
                // Log or perform some action specific to SQL-related issues.
                throw;
            }
            catch (Exception ex)
            {
                // Log the error. This is just a simple example. In production, use a logging framework.
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        public async Task<object?> ExecuteScalarAsync(string connectionString, string queryString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(queryString))
                throw new ArgumentException("Query string cannot be null or empty.", nameof(queryString));

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.CommandTimeout = DefaultCommandTimeout;
                        await connection.OpenAsync();
                        return await command.ExecuteScalarAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing scalar asynchronously: {ex.Message}");
                throw;  // Re-throwing the exception if you want the calling code to handle it or be aware of it
            }
        }


        //Sends the CommandText to the Connection and builds a SqlDataReader.
        /*
         *  USAGE
         * 
            using SqlDataReader reader = await ExecuteReaderAsync(connectionString, queryString);
            while (await reader.ReadAsync())
            {
                // Process data
            }       
         * 
        */
        public async Task<SqlDataReader> ExecuteReaderAsync(string connectionString, string queryString, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(queryString))
                throw new ArgumentException("Query string cannot be null or empty.", nameof(queryString));

            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                SqlCommand command = new SqlCommand(queryString, connection)
                {
                    CommandTimeout = DefaultCommandTimeout
                };
                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false); // This will ensure connection is closed when SqlDataReader is closed
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing query: {ex.Message}");
                await connection.DisposeAsync().ConfigureAwait(false);  // Dispose the connection if an error occurs
                throw;
            }
        }



    }









    // Base DB Access Code Stored Procedure
    public partial class MSSQL
    {
        //Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        public int ExecuteNonQuerySP(string connectionString,
            string storedProcedureName,
            List<SqlParameter> sqlParameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Invalid connection string.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Invalid stored procedure name.", nameof(storedProcedureName));

            if (sqlParameters == null)
                throw new ArgumentNullException(nameof(sqlParameters));

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                using SqlCommand command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = DefaultCommandTimeout
                };

                command.Parameters.AddRange(sqlParameters.ToArray());

                connection.Open();
                return command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                // Consider logging the error here using a logging library.
                throw; // You can either rethrow the original exception or wrap it in a custom exception.
            }
            catch (Exception ex)
            {
                // Consider logging the error here using a logging library.
                throw;
            }
        }

        //Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        public object ExecuteScalarSP(string connectionString, string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = DefaultCommandTimeout;

                        if (sqlParameters != null)
                        {
                            foreach (var parameter in sqlParameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }

                        connection.Open();
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing scalar on stored procedure: {ex.Message}");
                throw;
            }
        }

        //Sends the CommandText to the Connection and builds a SqlDataReader.
        /*
         *  USAGE
         * 
            string connectionString = "your_connection_string_here";
            string storedProcedureName = "YourStoredProcedureName";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@param1", SqlDbType.VarChar) { Value = "SomeValue" },
                //... Add other parameters as needed
            };

            SqlConnection connection;
            SqlDataReader reader = ExecuteReaderSP(connectionString, storedProcedureName, parameters, out connection);
            using (reader)
            using (connection)
            {
                while (reader.Read())
                {
                    // Process data, for instance:
                    string result = reader.GetString(0);
                    // ... and so on for other columns
                }
            }
         * 
        */
        public SqlDataReader ExecuteReaderSP(string connectionString, string storedProcedureName, List<SqlParameter> sqlParameters, out SqlConnection connection)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));

            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();

                SqlCommand command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = DefaultCommandTimeout
                };

                if (sqlParameters != null)
                {
                    command.Parameters.AddRange(sqlParameters.ToArray());
                }

                return command.ExecuteReader(CommandBehavior.CloseConnection); // Ensure connection is closed when SqlDataReader is closed
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing stored procedure: {ex.Message}");
                connection.Dispose();  // Dispose the connection if an error occurs
                throw;
            }
        }


    }

    // Async Base DB Access Code Stored Procedure
    public partial class MSSQL
    {
        //Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        public async Task<int> ExecuteNonQuerySPAsync(string connectionString,
            string storedProcedureName,
            List<SqlParameter> sqlParameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Invalid connection string.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Invalid stored procedure name.", nameof(storedProcedureName));

            if (sqlParameters == null)
                throw new ArgumentNullException(nameof(sqlParameters));

            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                using SqlCommand command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = DefaultCommandTimeout
                };

                command.Parameters.AddRange(sqlParameters.ToArray());

                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
            catch (SqlException sqlEx)
            {
                // Consider logging the error here using a logging library.
                throw; // You can either rethrow the original exception or wrap it in a custom exception.
            }
            catch (Exception ex)
            {
                // Consider logging the error here using a logging library.
                throw;
            }
        }

        //Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        public async Task<object?> ExecuteScalarSPAsync(string connectionString, string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = DefaultCommandTimeout;

                        if (sqlParameters != null)
                        {
                            foreach (var parameter in sqlParameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }

                        await connection.OpenAsync();
                        return await command.ExecuteScalarAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing scalar on stored procedure asynchronously: {ex.Message}");
                throw;
            }
        }

        //Sends the CommandText to the Connection and builds a SqlDataReader.
        /*
         *  USAGE
         * 
            string connectionString = "your_connection_string_here";
            string storedProcedureName = "YourStoredProcedureName";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@param1", SqlDbType.VarChar) { Value = "SomeValue" },
                //... Add other parameters as needed
            };

            // It's generally a good practice to use the async/await pattern all the way up, including in your method signatures.
            // This example is a standalone async task. In real applications, this would be part of an async method.
            async Task YourMethodAsync()
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataReader reader = await ExecuteReaderAsyncSP(connectionString, storedProcedureName, parameters))
                {
                    while (await reader.ReadAsync())
                    {
                        // Process data, for instance:
                        string result = reader.GetString(0);
                        // ... and so on for other columns
                    }
                }
            }

            // Somewhere in your code, you'll call this method.
            // Remember, in most scenarios, avoid calling .Result or .Wait() on tasks, as it can lead to deadlocks.
            await YourMethodAsync();
         * 
        */
        public async Task<SqlDataReader> ExecuteReaderAsyncSP(string connectionString, string storedProcedureName, List<SqlParameter> sqlParameters, CancellationToken cancellationToken = default)
        {

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(storedProcedureName))
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));

            var connection = new SqlConnection(connectionString);

            try
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                SqlCommand command = new SqlCommand(storedProcedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = DefaultCommandTimeout
                };

                if (sqlParameters != null)
                {
                    command.Parameters.AddRange(sqlParameters.ToArray());
                }

                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Error executing stored procedure: {ex.Message}");
                await connection.DisposeAsync().ConfigureAwait(false);  // Dispose the connection if an error occurs
                throw;
            }
        }


    }










    // DB Access Code For Bulk Copy
    public partial class MSSQL
    {
        public void SqlBulkCopy(
            string connectionString,
            DataTable dataTable,
            string tableName,
            int batchSize,
            List<SqlBulkCopyColumnMapping> sqlBulkCopyColumnMappings)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Invalid connection string.", nameof(connectionString));

            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Invalid table name.", nameof(tableName));

            if (sqlBulkCopyColumnMappings == null)
                throw new ArgumentNullException(nameof(sqlBulkCopyColumnMappings));

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.FireTriggers, transaction))
                        {
                            bulkCopy.BatchSize = batchSize;
                            bulkCopy.DestinationTableName = tableName;
                            bulkCopy.BulkCopyTimeout = DefaultBulkCopyTimeout;

                            foreach (var columnMapping in sqlBulkCopyColumnMappings)
                            {
                                bulkCopy.ColumnMappings.Add(columnMapping);
                            }

                            bulkCopy.WriteToServer(dataTable);
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

    }

    // Async DB Access Code For Bulk Copy
    public partial class MSSQL
    {
        public async Task SqlBulkCopyAsync(
            string connectionString,
            DataTable dataTable,
            string tableName,
            int batchSize,
            List<SqlBulkCopyColumnMapping> sqlBulkCopyColumnMappings)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Invalid connection string.", nameof(connectionString));

            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Invalid table name.", nameof(tableName));

            if (sqlBulkCopyColumnMappings == null)
                throw new ArgumentNullException(nameof(sqlBulkCopyColumnMappings));

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.FireTriggers, transaction))
                        {
                            bulkCopy.BatchSize = batchSize;
                            bulkCopy.DestinationTableName = tableName;

                            foreach (var columnMapping in sqlBulkCopyColumnMappings)
                            {
                                bulkCopy.ColumnMappings.Add(columnMapping);
                            }

                            await bulkCopy.WriteToServerAsync(dataTable);
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

    }

}
