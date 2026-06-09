namespace Application.Dto.Home;

/// <summary>
/// Représente un slide du carrousel de la page d'accueil, formaté pour l'affichage.
/// </summary>
public class CarouselSlideDto
{
    /// <summary>
    /// Identifiant unique du slide.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// URL complète de l'image (ex: hébergée sur un CDN ou locale).
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Titre principal affiché sur le slide. Peut être nul si aucune traduction n'est trouvée pour la langue demandée.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Sous-titre ou description courte.
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Texte à afficher sur le bouton d'action (Call-to-Action).
    /// </summary>
    public string? ButtonText { get; set; }
}