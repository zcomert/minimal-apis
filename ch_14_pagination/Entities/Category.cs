using System.Text.Json.Serialization;

namespace Entities;

public class Category
{
    public int CategoryId { get; set; }
    public String? CategoryName { get; set; }
    
    [JsonIgnore]
    public ICollection<Book>? Books { get; set; } // collection navigation property
}