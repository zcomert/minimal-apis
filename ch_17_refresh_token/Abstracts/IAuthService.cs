using Entities.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Abstracts;

public interface IAuthService
{
    Task<IdentityResult> RegisterUserAsync(UserForRegistrationDto userDto);
    Task<bool> ValidateUserAsync(UserForAuthenticationDto userDto);
    Task<TokenDto> CreateTokenAsync(bool populateExp);
    Task<TokenDto> RefreshTokenAsync(TokenDto tokenDto);
}
