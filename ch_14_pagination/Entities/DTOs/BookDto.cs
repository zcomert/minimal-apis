using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs;

public abstract record BookDto
{
    [MinLength(2, ErrorMessage = "The title should include at least two characters.")]
    [MaxLength(25, ErrorMessage = "The title must be 25 characters or less.")]
    public String Title { get; init; }

    [Range(1, 100, ErrorMessage = "Price must be between 1 and 100.")]
    public Decimal Price { get; init; }
}