namespace RestaurantMS.Domain.Entities;

public class Area
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // Tầng 1, Sân vườn...
    public ICollection<Table> Tables { get; set; } = new List<Table>();
}