using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Services;

public class TeamMemberService : ITeamMemberService
{
    private readonly ApplicationDbContext _context;

    public TeamMemberService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeamMember>> GetActiveAsync()
    {
        return await _context.TeamMembers
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<List<TeamMember>> GetAllAsync()
    {
        return await _context.TeamMembers
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<TeamMember?> GetByIdAsync(int id)
    {
        return await _context.TeamMembers.FindAsync(id);
    }

    public async Task CreateAsync(TeamMember teamMember)
    {
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TeamMember teamMember)
    {
        _context.TeamMembers.Update(teamMember);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _context.TeamMembers.FindAsync(id);
        if (member != null)
        {
            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
    }
}
