using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs;

public record UserForRegistrationDto
{
    public String? FirstName { get; init; }
    public String? LastName { get; init; }
    
    [Required(ErrorMessage = "Username is required!")]
    public String UserName { get; init; }
    
    [Required(ErrorMessage = "Password is required!")]
    public String Password { get; init; }
   
    public String? Email { get; init; }
    public String? PhoneNumber { get; init; }
    public ICollection<String>? Roles { get; init; }
}
