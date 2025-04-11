namespace API.Services // Change this to match your project namespace
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
