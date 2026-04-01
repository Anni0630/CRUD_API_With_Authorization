using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Repositories;

public interface IProductRepository
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, string? search);

    Task<Product?> GetByIdAsync(int id);
    Task<Product>  CreateAsync(Product product);
    Task<Product?> UpdateAsync(int id, Product product);
    Task<bool>     DeleteAsync(int id);
}
