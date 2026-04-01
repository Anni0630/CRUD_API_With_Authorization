using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Repositories;

namespace ProductApi.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repo, ILogger<ProductService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        page     = page     < 1  ? 1  : page;
        pageSize = pageSize < 1  ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;   // cap at 100

        var (items, total) = await _repo.GetAllAsync(page, pageSize, search);

        _logger.LogDebug("GetAll products – page {Page}/{PageSize}, search='{Search}', total={Total}",
            page, pageSize, search, total);

        return new PagedResult<ProductDto>
        {
            Items      = items.Select(MapToDto),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name        = dto.Name,
            Description = dto.Description,
            Price       = dto.Price
        };

        var created = await _repo.CreateAsync(product);
        _logger.LogInformation("Product created – Id={Id}, Name={Name}", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = new Product
        {
            Name        = dto.Name,
            Description = dto.Description,
            Price       = dto.Price
        };

        var updated = await _repo.UpdateAsync(id, product);
        if (updated is null)
        {
            _logger.LogWarning("Update failed – product not found: Id={Id}", id);
            return null;
        }

        _logger.LogInformation("Product updated – Id={Id}", id);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repo.DeleteAsync(id);
        if (!deleted)
            _logger.LogWarning("Delete failed – product not found: Id={Id}", id);
        else
            _logger.LogInformation("Product deleted – Id={Id}", id);
        return deleted;
    }

    // ── Mapping helper ──────────────────────────────────────────────────────────
    private static ProductDto MapToDto(Product p) => new()
    {
        Id          = p.Id,
        Name        = p.Name,
        Description = p.Description,
        Price       = p.Price,
        CreatedDate = p.CreatedDate
    };
}
