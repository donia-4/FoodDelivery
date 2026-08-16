using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Models;
using Restaurant.Domain.AddOns;
using Restaurant.Domain.Foods;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.Repositories;

public sealed class FoodRepository(RestaurantDbContext context)
    : IFoodRepository
{
    private readonly RestaurantDbContext _context = context;

    public async Task AddAsync(
        Food food,
        CancellationToken cancellationToken = default)
    {
        await _context.Foods.AddAsync(food, cancellationToken);
    }

    public async Task<Food?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Foods
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Food?> GetByIdWithAddOnsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Foods
            //.AsNoTracking()
            .Include(x => x.AddOns)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public IQueryable<AddOn> GetAddOnsByFoodId(Guid foodId)
    {
        return _context.AddOns
            .AsNoTracking()
            .Where(x => x.FoodId == foodId);
    }

    public async Task<IReadOnlyList<Food>> GetByRestaurantIdAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Foods
            //.AsNoTracking()
            .Where(x => x.RestaurantId == restaurantId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Food>> GetByCategoryIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Foods
            //.AsNoTracking()
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Food food)
    {
        food.IsDeleted = true;
        _context.Foods.Update(food);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<PaginatedList<Food>> SearchAsync(
        Guid restaurantId,
        string? name,
        string? categoryName,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Foods
            .AsNoTracking()
            .Include(f => f.Category)
            .Where(f => f.RestaurantId == restaurantId && f.IsVisible);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(f => f.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(categoryName))
            query = query.Where(f => f.Category != null && f.Category.Name.Contains(categoryName));

        if (minPrice.HasValue)
            query = query.Where(f => f.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(f => f.Price <= maxPrice.Value);

        query = query.OrderBy(f => f.Name);

        return await PaginatedList<Food>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
    }

    public async Task<AddOn?> GetAddOnByIdAsync(Guid addOnId, CancellationToken cancellationToken = default)
    {
        return await _context.AddOns.FirstOrDefaultAsync(x => x.Id == addOnId, cancellationToken);
    }
    public async Task<bool> ExistsWithTheGivenName(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Foods.AnyAsync(f => f.Name.ToLower() == name, cancellationToken);
    }
}