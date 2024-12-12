using System.ComponentModel.DataAnnotations;

namespace Entities;



public class Book
{
    [Required]
    public int Id { get; set; }

    public String? Title { get; set; }

    public Decimal Price { get; set; }
}