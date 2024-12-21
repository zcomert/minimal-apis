using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Entities;

public class Book
{
    public int Id { get; set; }

    public String? Title { get; set; }
    public Decimal Price { get; set; }
    public String? URL { get; set; } 
    
    public int CategoryId { get; set; }
    public Category? Category { get; set; } // navigation property
    public Book()
    {
        URL = "/images/default.jpg";
    }
}
