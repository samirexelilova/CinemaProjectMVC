namespace StreamitMVC.Services.Interfaces
{
    public interface ISessionCleanerService
    {
        Task CleanOldSessionsAsync();
    }

}
