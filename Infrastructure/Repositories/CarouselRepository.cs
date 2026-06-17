using Domain.Entities.PromoAndCms;

using Infrastructure.Data;
using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

using Tools;

namespace Infrastructure.Repositories;

public class CarouselRepository : ICarouselRepository
{
    private readonly AppDbContext _context;

    public CarouselRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CarouselSlide>> GetActiveSlidesAsync(LocaleLang locale)
    {
        return await _context.CarouselSlides
            .AsNoTracking()
            .Where(slide => slide.IsActive)
            .OrderBy(slide => slide.DisplayOrder)
            .Include(slide => slide.Translations.Where(t => t.Locale == locale))
            .ToListAsync();
    }
}