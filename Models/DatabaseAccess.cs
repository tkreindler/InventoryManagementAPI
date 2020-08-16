using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InventoryManagement.Models
{
    public class DatabaseAccess : DbContext
    {
        /// <summary>
        /// A database table with all items
        /// </summary>
        public DbSet<Item> Items { get; set; }

        /// <summary>
        /// A database table with all item types
        /// </summary>
        public DbSet<ItemType> ItemTypes { get; set; }

        /// <summary>
        /// A database table with all users
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DatabaseAccess()
        {
        }

        /// <summary>
        /// Tell it to use sqlite for debugging purposes
        /// </summary>
        /// <param name="options"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(PostgresConnectionString.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly static Lazy<string> PostgresConnectionString = new Lazy<string>(
            () =>
            {
                // get heroku environment variable
                string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
                connectionString.Replace("//", "");

                char[] delimiterChars = { '/', ':', '@', '?' };
                string[] strConn = connectionString.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

                var builder = new NpgsqlConnectionStringBuilder
                {
                    Username = strConn[1],
                    Password = strConn[2],
                    Host = strConn[3],
                    Port = Int32.Parse(strConn[4]),
                    Database = strConn[5],
                    SslMode = SslMode.Require,
                    // Heroku db has an untrusted certificate
                    TrustServerCertificate = true,
                    Timeout = 1000
                };

                return builder.ToString();
            });
    }
}
