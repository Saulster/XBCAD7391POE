using WebApplication1.Models;

namespace WebApplication1.Services 
{
    /// <summary>
    /// this class defines the contract for user-related operations within the application.
    /// Author: Allana Morris
    /// </summary>
    /// -----------------------------------------------------------------------------------------------------------------------------
    public interface IUserService
    {
        User ValidateUserCredentials(string username, string password);
        User GetUser(string username);
    }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------