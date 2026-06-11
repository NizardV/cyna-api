using Tools;

namespace Infrastructure.Interfaces;

/// <summary>
/// Contrat pour la gestion des paramètres de configuration du site.
/// </summary>
public interface ISiteSettingRepository
{
    /// <summary>
    /// Récupère la valeur traduite d'un paramètre via sa clé unique.
    /// </summary>
    Task<string?> GetSettingValueAsync(string key, LocaleLang locale);
}