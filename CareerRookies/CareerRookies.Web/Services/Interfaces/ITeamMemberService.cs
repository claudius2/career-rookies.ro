using CareerRookies.Web.Models;

namespace CareerRookies.Web.Services.Interfaces;

public interface ITeamMemberService
{
    Task<List<TeamMember>> GetActiveAsync();
    Task<List<TeamMember>> GetAllAsync();
    Task<TeamMember?> GetByIdAsync(int id);
    Task CreateAsync(TeamMember teamMember);
    Task UpdateAsync(TeamMember teamMember);
    Task DeleteAsync(int id);
}
