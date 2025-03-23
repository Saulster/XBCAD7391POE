
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;
using WebApplication1.Controllers;
using WebApplication1.Models; 

namespace WebApplication1.Services 
{
/// <summary>
/// This class provides methods for user credential authentication and validation.
/// Authors: Allana Morris, Sven Masche
/// </summary>
    public class UserService : IUserService
    {
        //setting db controler for database related actions
        private readonly DBController _dbController;

        /// <summary>
        /// General constructor accepting db controller
        /// Authors: Allana Morris
        /// </summary>
        public UserService(DBController dbController)
        {
            _dbController = dbController;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method to validate user log-in using thier user name ans passwords, as well as checking role.
        /// Authors: Allana Morris.
        /// Edited By: Sven Masche
        /// </summary>
        public User ValidateUserCredentials(string uName, string password)
        {
            var user = _dbController.getUser(uName); // Get user from DB

            if (user != null && VerifyPassword(password, user.password))
            {
                return user; // Return user if credentials are valid and user is admin
            }

            return null; // Return null if invalid credentials or non-admin role
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method facilitates the hashin of a provided password
        /// Ultimately is never used
        /// Authors: Sven Masche
        /// </summary>
        private string HashPassword(string password)
        {
            // Generate a salted hash for the password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method compares a input string against a stored hashed password and returns a bool result
        /// Authors: Allana Morris
        /// Edited by: Sven Masche
        /// </summary>
        private bool VerifyPassword(string password, string storedHash)
        {
            // Verify the password against the stored hash
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method just retrieves a user stored in the database
        /// Author: Allana Morris
        /// </summary>
        public User GetUser(string username)
        {
            return _dbController.getUser(username);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
