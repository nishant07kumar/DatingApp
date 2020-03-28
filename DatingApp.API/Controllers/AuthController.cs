using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Dtos;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDtos userForRegisterDtos)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            userForRegisterDtos.Username = userForRegisterDtos.Username.ToLower();
            if (await _authRepository.UserExists(userForRegisterDtos.Username))
            {
                return BadRequest("User already exist. Can't create new user with same name.");
            }

            var userToCreate = new User
            {
                Username = userForRegisterDtos.Username
            };
            var createdNewUser = await _authRepository.Register(userToCreate, userForRegisterDtos.Password);

            return StatusCode(201);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDtos userForLoginDtos)
        {
            var _userInfo =await _authRepository.Login(userForLoginDtos.Username.ToLower(), userForLoginDtos.Password);

            if (_userInfo == null)
                return Unauthorized();
            var claim = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,_userInfo.Id.ToString()),
                new Claim(ClaimTypes.Name,_userInfo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claim),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}