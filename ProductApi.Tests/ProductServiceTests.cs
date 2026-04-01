using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Repositories;
using ProductApi.Services;
using Xunit;

namespace ProductApi.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repoMock   = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _service    = new ProductService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenProductExists()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product", Price = 10.99m };
        _repoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedDto()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "New", Price = 50 };
        var createdProduct = new Product { Id = 10, Name = "New", Price = 50, CreatedDate = DateTime.UtcNow };
        
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Id.Should().Be(10);
        result.Name.Should().Be("New");
        _repoMock.Verify(r => r.CreateAsync(It.Is<Product>(p => p.Name == "New")), Times.Once);
    }
}
