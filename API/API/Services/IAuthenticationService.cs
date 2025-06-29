namespace API.Services 
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Returns the ID of the authenticated user
        /// </summary>
        /// <returns></returns>
        string? GetCurrentAuthenticatedUserId();
    }
}
