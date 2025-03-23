using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using WebApplication1.Controllers;
using WebApplication1.Models;

namespace Backend.Controllers
{
    /// <summary>
    ///  This controller class facilitates all the user related methods
    ///  Author: Allana Morris
    /// </summary>
    public class UserController : Controller
    {
        private DBController _dbController;

        /// <summary>
        /// General contrstructor to set the db controller variable upon creation
        /// </summary>
        public UserController()
        {
            _dbController = new DBController();
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// User route to get user as well as authenticate them.
        /// </summary>
        [HttpGet("getUser")]
        [Authorize] // Ensure the user is authenticated
        public IActionResult GetUser()
        {
            var usernameClaim = User.Claims.First().Value;
            var username = usernameClaim; // Get the username from the claim

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username not found in token."); // Return a bad request if username is not found
            }

            // Log the username for debugging
           // Console.WriteLine($"Extracted username: {username}");

            var user = _dbController.getUser(username); // Use the extracted username to retrieve the user

            if (user == null)
            {
                Console.WriteLine("User not found in database.");
                return NotFound(); // Return 404 if no user is found
            }

            return Ok(user); // Return the user as a JSON response
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------

