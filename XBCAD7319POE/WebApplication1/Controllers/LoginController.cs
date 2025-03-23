using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models; 
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
/// <summary>
/// API controller class for user login procedures
/// Author: Allana Morris
/// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        //Global variables
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        /// <summary>
        /// Constructor method, sets up global vars
        /// Author: Allana Morris
        /// </summary>
        public LoginController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService; // Dependency injection for user service
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Login route for user
        /// Author: Allana Morris
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                // Validate user credentials
                var user = _userService.ValidateUserCredentials(loginDto.uName, loginDto.Password);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logging framework)
                Console.WriteLine(ex);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// User logout route/method
        /// Author: Allana Morris
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            //for stateless JWT, logging out might just mean removing the token on the client side
            return Ok(new { message = "User logged out successfully." });
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// User refresh token route
        /// Author: Allana Morris
        /// </summary>
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            // Validate the incoming refresh token
            if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            // Extract claims from the expired token
            var principal = GetClaimsFromExpiredToken(refreshTokenDto.RefreshToken);
            if (principal == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            var userName = principal.Identity.Name; // Extract username from claims

            // Validate user against the database to ensure they exist
            var user = _userService.GetUser(userName); // Fetch user using username
            if (user == null)
            {
                return Unauthorized(new { message = "User not found." });
            }

            // Generate new JWT and refresh token
            var token = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Optionally, store the new refresh token in the database for tracking

            return Ok(new { token, refreshToken = newRefreshToken });
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generate a jwt token using the user info
        /// Author: Allana Morris
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            // Create a symmetric security key from the configured secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Specify the signing credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define the claims for the token
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.uName), // Username as the subject
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique identifier for the token
            };

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"], // Issuer from configuration
                audience: _configuration["Jwt:Audience"], // Audience from configuration
                claims: claims, // Claims to include in the token
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])), // Expiration time
                signingCredentials: creds // Signing credentials
            );

            // Return the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// User route to generate a refresh token
        /// Author: Allana Morris
        /// </summary>
        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// User route to get claims from expired token
        /// Author: Allana Morris
        /// </summary>
        private ClaimsPrincipal GetClaimsFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                // Validate the token without checking the lifetime
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false, // We don't validate the lifetime here
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                }, out SecurityToken validatedToken);

                return principal; // Return the claims principal
            }
            catch (Exception)
            {
                // Log the exception (optional)
                return null; // Return null if there's an issue with the token
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------

    /// <summary>
    /// User login data transfer object class
    /// Author: Allana Morris
    /// </summary>
    public class UserLoginDto
    {
        public string uName { get; set; }
        public string Password { get; set; }
    }
    //--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------

    /// <summary>
    /// Refresh token data transfer object class
    /// Author: Allana Morris
    /// </summary>
    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; }
    }//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
