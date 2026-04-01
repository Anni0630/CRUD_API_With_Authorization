using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/product")]
[Produces("application/json")]
[Authorize]   // All endpoints require a valid JWT by default
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Get all products (paginated, optional search).</summary>
    /// <remarks>Access: Any authenticated user.</remarks>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 10, max 100).</param>
    /// <param name="search">Optional keyword to search Name or Description.</param>
    /// <response code="200">Returns paged list of products.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int    page     = 1,
        [FromQuery] int    pageSize = 10,
        [FromQuery] string? search  = null)
    {
        var result = await _productService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    /// <summary>Get a single product by ID.</summary>
    /// <remarks>Access: Any authenticated user.</remarks>
    /// <param name="id">Product ID.</param>
    /// <response code="200">Product found.</response>
    /// <response code="404">Product not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
            return NotFound(new { message = $"Product with ID {id} not found." });

        return Ok(product);
    }

    /// <summary>Create a new product.</summary>
    /// <remarks>Access: Admin only.</remarks>
    /// <response code="201">Product created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Only admins can create products.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing product.</summary>
    /// <remarks>Access: Admin only.</remarks>
    /// <param name="id">Product ID to update.</param>
    /// <response code="200">Product updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Only admins can update products.</response>
    /// <response code="404">Product not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _productService.UpdateAsync(id, dto);
        if (updated is null)
            return NotFound(new { message = $"Product with ID {id} not found." });

        return Ok(updated);
    }

    /// <summary>Delete a product.</summary>
    /// <remarks>Access: Admin only.</remarks>
    /// <param name="id">Product ID to delete.</param>
    /// <response code="204">Product deleted successfully.</response>
    /// <response code="403">Only admins can delete products.</response>
    /// <response code="404">Product not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Product with ID {id} not found." });

        return NoContent();
    }
}
