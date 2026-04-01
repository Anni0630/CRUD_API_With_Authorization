namespace ProductApi.Models;

public class Product
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
