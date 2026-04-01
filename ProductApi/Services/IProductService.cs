using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<ProductDto?>             GetByIdAsync(int id);
    Task<ProductDto>              CreateAsync(CreateProductDto dto);
    Task<ProductDto?>             UpdateAsync(int id, UpdateProductDto dto);
    Task<bool>                    DeleteAsync(int id);
}
