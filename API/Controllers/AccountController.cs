using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService,IMapper mapper): BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
        
        if(await UserExists(registerDto.Username)) 
            return BadRequest("Username is taken");

            // return Ok();
        using var hmac = new HMACSHA512();

        var user = mapper.Map<AppUser>(registerDto);
        user.UserName =registerDto.Username.ToLower();
        /*user.PasswordHash =hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt= hmac.Key;*/

        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        return new UserDto{
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
        var user= await context.Users
            .Include(p=>p.Photos)
                .FirstOrDefaultAsync(x=>
                    x.UserName == loginDto.Username.ToLower());

        if(user == null || user.UserName ==null) return Unauthorized("Invalid username");

       /* using var hmac = new HMACSHA512(user.PasswordSalt);
        var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computeHash.Length; i++)
        {
            if(computeHash[i] != user.PasswordHash[i])
                return Unauthorized("Invalid password");
        }*/

        return new UserDto{
            Username = user.UserName,
            KnownAs =user.KnownAs,
            Token = tokenService.CreateToken(user),
            Gender = user.Gender,
            PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url
        };
    }

    private async Task<bool> UserExists(string username){
        return await context.Users.AnyAsync(x=>x.NormalizedUserName ==username.ToUpper());
    }
}

