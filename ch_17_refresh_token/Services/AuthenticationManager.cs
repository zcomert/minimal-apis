using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Abstracts;
using AutoMapper;
using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Services;

public class AuthenticationManager : IAuthService
{
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private User? _user;

    public AuthenticationManager(IMapper mapper,
        UserManager<User> userManager,
        IConfiguration configuration)
    {
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> CreateTokenAsync()
    {
        var signinCredentials = GetSigningCredentials();
        var claims = await GetClaims();
        var tokenOptions = GenerateTokenOptions(signinCredentials, claims);
        return new JwtSecurityTokenHandler()
            .WriteToken(tokenOptions);
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signinCredentials, 
        List<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        
        var tokenOptions = new JwtSecurityToken(
            issuer: jwtSettings["validIssuer"],
            audience: jwtSettings["validAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expire"])),
            signingCredentials: signinCredentials);
        return tokenOptions;
    }

    private async Task<List<Claim>> GetClaims()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, _user.UserName)
        };

        var roles = await _userManager
            .GetRolesAsync(_user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return claims;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["secretKey"]);
        var secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }

    public async Task<IdentityResult> RegisterUserAsync(UserForRegistrationDto userDto)
    {
        var user = _mapper.Map<User>(userDto);
        
        var result = await _userManager
            .CreateAsync(user, userDto.Password); 

        if(result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, userDto.Roles);
        }
        return result;
    }

    public async Task<bool> ValidateUserAsync(UserForAuthenticationDto userDto)
    {
        Validate(userDto);
        _user = await _userManager
            .FindByNameAsync(userDto.UserName);

        var result = (_user is not null
            && await _userManager.CheckPasswordAsync(_user, userDto.Password));
        
        return result;
    }

    private void Validate<T>(T item)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(item);
        var isValid = Validator.TryValidateObject(item, context, validationResults, true);

        if (!isValid)
        {
            var errors = string.Join(" ", validationResults.Select(v => v.ErrorMessage));
            throw new ValidationException(errors);
        }
    }
}
