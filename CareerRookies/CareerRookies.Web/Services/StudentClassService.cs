using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Data;
using CareerRookies.Web.Models;
using CareerRookies.Web.Services.Interfaces;

namespace CareerRookies.Web.Services;

public class StudentClassService : IStudentClassService
{
    private readonly ApplicationDbContext _context;

    public StudentClassService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentClass>> GetActiveAsync()
    {
        return await _context.StudentClasses
            .Where(sc => sc.IsActive)
            .OrderBy(sc => sc.Name)
            .ToListAsync();
    }

    public async Task<List<StudentClass>> GetAllAsync()
    {
        return await _context.StudentClasses
            .OrderBy(sc => sc.Name)
            .ToListAsync();
    }

    public async Task<StudentClass?> GetByIdAsync(int id)
    {
        return await _context.StudentClasses.FindAsync(id);
    }

    public async Task<StudentClass> CreateAsync(StudentClass studentClass)
    {
        _context.StudentClasses.Add(studentClass);
        await _context.SaveChangesAsync();
        return studentClass;
    }

    public async Task UpdateAsync(StudentClass studentClass)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var sc = await _context.StudentClasses.FindAsync(id);
        if (sc == null) return;
        _context.StudentClasses.Remove(sc);
        await _context.SaveChangesAsync();
    }
}
