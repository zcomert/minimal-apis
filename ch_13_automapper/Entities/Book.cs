using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Book
{
    public int Id { get; set; }

    public String? Title { get; set; }

    public Decimal Price { get; set; }
}