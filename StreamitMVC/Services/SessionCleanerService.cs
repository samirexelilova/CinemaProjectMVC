using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Services.Interfaces;

namespace StreamitMVC.Services
{
    public class SessionCleanerService : ISessionCleanerService
    {
        private readonly AppDbContext _context;

        public SessionCleanerService(AppDbContext context)
        {
            _context = context;
        }
        public async Task CleanOldSessionsAsync()
        {
            var currentTime = DateTime.Now;

            var finishedSessions = await _context.Sessions
                .Where(s => s.EndTime < currentTime)
                .ToListAsync();

            if (finishedSessions.Any())
            {
                var finishedSessionIds = finishedSessions.Select(s => s.Id).ToList();

                var oldTickets = await _context.Tickets
                    .Where(t => finishedSessionIds.Contains(t.SessionId))
                    .ToListAsync();

                _context.Tickets.RemoveRange(oldTickets);
                _context.Sessions.RemoveRange(finishedSessions);

                await _context.SaveChangesAsync();
            }
        }
    }

}
