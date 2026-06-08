using Infrastructure.Data;
using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

using Tools;

namespace Infrastructure.Repositories;

public class SiteSettingRepository : ISiteSettingRepository
{
    private readonly AppDbContext _context;

    public SiteSettingRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<string?> GetSettingValueAsync(string key, LocaleLang locale)
    {
        // On récupère le paramètre exact avec sa traduction spécifique
        var setting = await _context.SiteSettings
            .AsNoTracking()
            .Include(s => s.Translations.Where(t => t.Locale == locale))
            .FirstOrDefaultAsync(s => s.SettingKey == key);

        // On retourne la valeur texte de la traduction, ou null si elle n'existe pas
        return setting?.Translations.FirstOrDefault()?.SettingValue;
    }
}