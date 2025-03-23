namespace WebApplication1.Models
{
/// <summary>
/// Class holds the user object, basically admin login stuff
/// Author: Allana Morris
/// </summary>
    public class User
    {
        public string uName { get; set; }
        public string realName { get; set; }
        public string password { get; set; }
        public string passwordHash { get; set; }
        public byte[] profile_image { get; set; }

    }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
