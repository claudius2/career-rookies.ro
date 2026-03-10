using CareerRookies.Web.Models;

namespace CareerRookies.Web.Services.Interfaces;

public interface IStudentClassService
{
    Task<List<StudentClass>> GetActiveAsync();
    Task<List<StudentClass>> GetAllAsync();
    Task<StudentClass?> GetByIdAsync(int id);
    Task<StudentClass> CreateAsync(StudentClass studentClass);
    Task UpdateAsync(StudentClass studentClass);
    Task DeleteAsync(int id);
}
