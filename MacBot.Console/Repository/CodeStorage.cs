using MacBot.ConsoleApp.Models;
using MacBot.ConsoleApp.Repository.Database;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository
{
    public class CodeStorage : ICodeStorage
    {
        private readonly BotDbContext _context;

        public CodeStorage(BotDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(InviteCode inviteCode)
        {
            await _context.InviteCodes.AddAsync(inviteCode);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var inviteCode = await _context.InviteCodes.FindAsync(id);
            if (inviteCode != null)
            {
                _context.InviteCodes.Remove(inviteCode);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string code)
        {
            var links = await _context.InviteCodes.ToListAsync();
            var link = links.Where(x => x.Code == code).First();
            await DeleteAsync(link.Id);
        }

        public async Task<IEnumerable<InviteCode>> GetAllAsync()
        {
            return await _context.InviteCodes.ToListAsync();
        }

        public async Task<InviteCode> GetByIdAsync(Guid id)
        {
            return await _context.InviteCodes.FindAsync(id);
        }

        public async Task<Guid> GetCreatorAsync(string code)
        {
            var inviteCode = await _context.InviteCodes
                .FirstOrDefaultAsync(ic => ic.Code.EndsWith(code));

            return (Guid)inviteCode?.MasterId;
        }

        public async Task UpdateAsync(InviteCode inviteCode)
        {
            _context.InviteCodes.Update(inviteCode);
            await _context.SaveChangesAsync();
        }
    }
}
