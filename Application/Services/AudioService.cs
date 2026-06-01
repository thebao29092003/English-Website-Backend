using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Application.Services
{
   
    public class AudioService
    {
        private readonly EnglishDBContext _context;
         public AudioService(EnglishDBContext context)
        {
            _context = context;
        }

        public async Task<List<VideoGame>> GetAll()
        {
            return await _context.VideoGames.ToListAsync();
        }
    }
}
