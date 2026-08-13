using Financial.Application.Interfaces;
using Financial.Domain.Entities;
using Financial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Financial.Infrastructure.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly FinancialDbContext _context;

    public GoalRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Goal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.Status == GoalStatus.Active)
            .ThenBy(g => g.TargetDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Goal>> GetActiveGoalsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId && g.Status == GoalStatus.Active)
            .OrderBy(g => g.TargetDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Goal> AddAsync(Goal goal, CancellationToken cancellationToken = default)
    {
        goal.CreatedAt = DateTime.UtcNow;
        await _context.Goals.AddAsync(goal, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return goal;
    }

    public async Task UpdateAsync(Goal goal, CancellationToken cancellationToken = default)
    {
        goal.UpdatedAt = DateTime.UtcNow;
        
        // If goal reaches target amount, mark as completed
        if (goal.CurrentAmount >= goal.TargetAmount && goal.Status == GoalStatus.Active)
        {
            goal.Status = GoalStatus.Completed;
            goal.CompletedAt = DateTime.UtcNow;
        }

        _context.Goals.Update(goal);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _context.Goals.FindAsync(new object[] { id }, cancellationToken);
        if (goal != null)
        {
            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Goals.AnyAsync(g => g.Id == id, cancellationToken);
    }
}
