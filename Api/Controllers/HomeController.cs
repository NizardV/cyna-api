using Application.Dto.Home;
using Application.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Tools;

namespace Api.Controllers;

/// <summary>
/// Contrôleur gérant les données du CMS (Carrousel, Textes dynamiques, etc.)
/// </summary>
[ApiController]
[Route("[controller]")] 
public class HomeController : ControllerBase
{
    private readonly ICmsService _cmsService;
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du contrôleur <see cref="HomeController"/>.
    /// </summary>
    /// <param name="cmsService">Le service CMS injecté automatiquement pour gérer le contenu dynamique.</param>
    public HomeController(ICmsService cmsService, ILogger<HomeController> logger)
    {
        _cmsService = cmsService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint principal pour charger la page d'accueil.
    /// </summary>
    /// <param name="locale">
    /// La langue demandée par le client (ex: Fr = 0, En = 1). 
    /// Passée en paramètre de requête (Query Parameter) : ?locale=0
    /// Valeur par défaut : LocaleLang.Fr (Français).
    /// </param>
    /// <returns>
    /// Un code HTTP 200 (OK) contenant le dictionnaire complet des données <see cref="HomePageDto"/>.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<HomePageDto>> GetHomeData([FromQuery] LocaleLang locale = LocaleLang.Fr)
    {

        _logger.LogInformation("Récupération de la page d'accueil demandée avec la langue : {Locale}", locale);
        var carousel = await _cmsService.GetHomeCarouselAsync(locale);

        var response = new HomePageDto
        {
            CarouselSlides = carousel
        };

        return Ok(response);
    }
}