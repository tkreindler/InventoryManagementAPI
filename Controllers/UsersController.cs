using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace InventoryManagement.Controllers
{
    /// <summary>
    /// Class to handle all requests on the base route
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Internal logger provided by ASP.Net
        /// </summary>
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger provided by ASP.Net</param>
        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Delete a specific Item
        /// </summary>
        /// <param name="id">The Id of this item</param>
        /// <returns>HTTP response code</returns>
        [HttpDelete("{username}")]
        public IActionResult Delete(string username)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string usernameLoggedIn))
            {
                return Unauthorized();
            }

            // don't let a user delete themself
            if (username == usernameLoggedIn)
            {
                return Unauthorized("Can't delete yourself");
            }

            using (var db = new DatabaseAccess())
            {
                var user = db.Users.Where(
                    user => user.Username == username.ToLower())
                    .FirstOrDefault();

                if (user == null)
                {
                    return NotFound();
                }

                db.Users.Remove(user);
                db.SaveChanges();
                return Accepted();
            }
        }

        /// <summary>
        /// Update a specific Item
        /// </summary>
        /// <param name="userIn">The new contents to replace with</param>
        /// <returns>HTTP response code</returns>
        [HttpPut("{username}")]
        public IActionResult ChangePassword(string username, string password)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string usernameLoggedIn))
            {
                return Unauthorized();
            }

            // if another user is trying to change your password
            if (username != usernameLoggedIn)
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var user = db.Users.Where(
                    user => user.Username == username.ToLower())
                    .FirstOrDefault();

                user.PasswordSaltedHash = Models.User.HashAndSalt(password, user.Salt);

                db.SaveChanges();

                return Accepted();
            }
        }

        /// <summary>
        /// Add a new Item to the server
        /// </summary>
        /// <param name="itemNoId">Item to add</param>
        /// <returns>HTTP response information and the new id of this item</returns>
        [HttpPost]
        public IActionResult Create(AuthenticationModel userIn)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                if (db.Users.Where(
                    user => user.Username == userIn.Username.ToLower()).Any())
                {
                    return Conflict("A user with that name already exists");
                }

                db.Users.Add(new Models.User(userIn));
                db.SaveChanges();

                return Accepted();
            }
        }
    }
}
