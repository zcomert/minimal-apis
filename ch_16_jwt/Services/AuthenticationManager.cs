using Abstracts;
using AutoMapper;
using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Services;

public class AuthenticationManager : IAuthService
{
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public AuthenticationManager(IMapper mapper, 
        UserManager<User> userManager)
    {
        _mapper = mapper;
        _userManager = userManager;
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
}
