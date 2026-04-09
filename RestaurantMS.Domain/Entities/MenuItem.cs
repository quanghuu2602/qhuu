using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RestaurantMS.Domain.Entities;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string TagsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public List<string> Tags
    {
        get => JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new();
        set => TagsJson = JsonSerializer.Serialize(value);
    }
}