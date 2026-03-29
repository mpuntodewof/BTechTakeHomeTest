using AuthAssessment.Application.Interfaces;
using AuthAssessment.Domain.Entities;
using AuthAssessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthAssessment.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
        => await context.Users.AsNoTracking().OrderBy(u => u.CreatedAt).ToListAsync(ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await context.Users.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await context.Users.AddAsync(user, ct);

    public void Update(User user)
        => context.Users.Update(user);
}
