using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whizible26.Application.General
{
    //Class Updated by Ajit L on 08/10/2025 for Centralized Connection String Management
    public sealed class ConnectionStringManager
    {
        // OLD CODE - COMMENTED OUT
        /*
        private static readonly Lazy<ConnectionStringManager> instance =
            new Lazy<ConnectionStringManager>(() => new ConnectionStringManager());

        public static ConnectionStringManager Instance => instance.Value;
        public string ConnectionString { get; private set; }
        public string StorageConnection { get; private set; }
        */

        // NEW CODE - IMPROVED CONNECTION STRING MANAGEMENT
        private static readonly Lazy<ConnectionStringManager> instance =
            new Lazy<ConnectionStringManager>(() => new ConnectionStringManager());

        public static ConnectionStringManager Instance => instance.Value;
        public string ConnectionString { get; private set; }
        public string StorageConnection { get; private set; }


        private ConnectionStringManager()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            ConnectionString = configuration.GetConnectionString("WhizibleDbConnection");
            
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException("Connection string 'WhizibleDbConnection' not found in configuration.");
            }
        }



    }
}
