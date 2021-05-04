using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagement.Models;
using InventoryManagement.ProtoModels;
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
    [Route("authenticate")]
    public class AuthenticationController : ControllerBase
    {
        /// <summary>
        /// Internal logger provided by ASP.Net
        /// </summary>
        private readonly ILogger<AuthenticationController> _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger provided by ASP.Net</param>
        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Mapping usernames to their authorization tokens
        /// </summary>
        /// <remarks>
        /// (Username, Token)
        /// </remarks>
        private static readonly List<Tuple<string, string>> AuthorizationTokens = new List<Tuple<string, string>>();

        /// <summary>
        /// Check an http context to see if it's authorized
        /// </summary>
        /// <param name="httpContext">Context checking</param>
        /// <param name="userName">Optional parameter saying the user logged in</param>
        /// <returns>Whether the user is authorized</returns>
        internal static bool CheckToken(HttpContext httpContext, out string userName)
        {
            var token = httpContext.Request.Cookies["auth-token"];
            if (token == null)
            {
                userName = null;
                return false;
            }

            var validPair = AuthorizationTokens.Where(pair => pair.Item2 == token);

            if (validPair.Any())
            {
                userName = validPair.First().Item1;
                return true;
            }
            else
            {
                userName = null;
                return false;
            }
        }

        /// <summary>
        /// Random number generator
        /// </summary>
        private static readonly Random random = new Random();

        /// <summary>
        /// Generate a random base 64 auth token
        /// </summary>
        /// <returns></returns>
        private static string GenerateAuthToken()
        {
            byte[] rand = new byte[2048];
            random.NextBytes(rand);
            return Convert.ToBase64String(rand);
        }

        [HttpPost]
        public IActionResult Authenticate()
        {
            var stream = Request.BodyReader.AsStream();
            var protoModel = ProtoModels.AuthenticationModel.Parser.ParseFrom(stream);

            using (var db = new DatabaseAccess())
            {
                var user = db.Users.Where(
                    user => user.Username == protoModel.Username.ToLower())
                    .FirstOrDefault();

                if (user == null)
                {
                    return BadRequest("User doesn't exist.");
                }

                bool valid = user.VerifyPassword(protoModel.Password);

                if (valid)
                {
                    string token = GenerateAuthToken();

                    // remove previous token if it exists
                    for (int i = 0; i < AuthorizationTokens.Count; i++)
                    {
                        if (AuthorizationTokens[i].Item1 == user.Username)
                        {
                            AuthorizationTokens.RemoveAt(i);
                            break;
                        }
                    }

                    // set it in memory
                    AuthorizationTokens.Add(new Tuple<string, string>(user.Username, token));

                    var cookie = new CookieHeaderValue("auth-token", token);

                    HttpContext.Response.Cookies.Append(
                        "auth-token",
                        token,
                        new Microsoft.AspNetCore.Http.CookieOptions
                        {
                            HttpOnly = true
                        });

                    return Ok();
                }
                else
                {
                    return BadRequest("Password is incorrect.");
                }
            }
        }
    }
}
