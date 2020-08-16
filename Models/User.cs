using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace InventoryManagement.Models
{
    public class User
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        private User()
        {
            // seed the salt
            random.NextBytes(this.Salt);
        }

        /// <summary>
        /// Create a user with a provided authentication
        /// </summary>
        /// <param name="authentication">The user credentials</param>
        public User(AuthenticationModel authentication) : base()
        {
            this.Username = authentication.Username.ToLower();
            this.PasswordSaltedHash = HashAndSalt(authentication.Password, this.Salt);
        }

        /// <summary>
        /// Random generator for this class
        /// </summary>
        [NotMapped]
        private static readonly Random random = new Random();

        /// <summary>
        /// Hash and salt a password
        /// </summary>
        /// <param name="password">Password in</param>
        /// <param name="salt">Salt in</param>
        /// <returns>The hashed and salted result</returns>
        public static byte[] HashAndSalt(string password, byte[] salt)
        {
            return new SHA256Managed().ComputeHash(
                Encoding.UTF8.GetBytes(password).Concat(salt).ToArray());
        }

        /// <summary>
        /// Check if a password is correct for this user
        /// </summary>
        /// <param name="password">The password being checked for this user</param>
        /// <returns>Whether the password matches</returns>
        public bool VerifyPassword(string password)
        {
            byte[] hashed = HashAndSalt(password, this.Salt);
            return hashed.SequenceEqual(this.PasswordSaltedHash);
        }

        /// <summary>
        /// The Username of this user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The Hashed and Salted password of this user
        /// </summary>
        public byte[] PasswordSaltedHash { get; set; }

        /// <summary>
        /// The unique salt generated for this user
        /// </summary>
        public byte[] Salt { get; set; } = new byte[16];

        /// <summary>
        /// Autoincremented key for database
        /// </summary>
        [Key]
        public long Id { get; set; } = 0;
    }
}
