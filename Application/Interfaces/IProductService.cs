using Domain.Dto.Product;

using Tools;

namespace Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDetailsDto?> GetProductDetailsAsync(int id, LocaleLang locale);
}