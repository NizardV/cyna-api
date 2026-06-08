using Application.Dto.Home;
using Application.Interfaces;

using Microsoft.Extensions.Logging;

using Infrastructure.Interfaces; 

using Tools;

namespace Application.Services;

/// <inheritdoc />
public class CmsService : ICmsService
{
    private readonly ICarouselRepository _carouselRepository;
    private readonly ILogger<CmsService> _logger;

    
    public CmsService(ICarouselRepository carouselRepository, ILogger<CmsService> logger)
    {
        _carouselRepository = carouselRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CarouselSlideDto>> GetHomeCarouselAsync(LocaleLang locale)
    {
        var slides = await _carouselRepository.GetActiveSlidesAsync(locale);

        if (!slides.Any())
        {
            _logger.LogWarning("Aucun slide de carrousel actif n'a été trouvé dans la base de données.");
        }

        return slides.Select(slide =>
        {
            var translation = slide.Translations.FirstOrDefault();

            return new CarouselSlideDto
            {
                Id = slide.Id,
                ImageUrl = slide.ImageUrl,
                Title = translation?.Title,
                Subtitle = translation?.Subtitle,
                ButtonText = translation?.ButtonText
            };
        });
    }
}