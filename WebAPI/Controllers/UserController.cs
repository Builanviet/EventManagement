using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using WebAPI.Models;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace WebAPI.Controllers
{

    public class UserController : ODataController
    {
        private readonly PROJECT_PRN231Context _context ;
        private readonly AppSettings _appSettings;

        public UserController (PROJECT_PRN231Context context, IConfiguration configuration)
        {
            _context = context;
            _appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        [HttpPost("Validate")]
        public IActionResult Validate([FromBody] LoginModel user)
        {
            if (user == null)
            {
                return BadRequest("Invalid request data");
            }

            var existingUser = _context.Users.SingleOrDefault(p => p.Email == user.Email);
            if (existingUser == null)
            {
                return NotFound("This account is not available");
            }
            if (!VerifyPassword(user.Password, existingUser.Passwordhash, existingUser.Passwordsalt))
            {
                return BadRequest("Incorrect password");
            }



            var token = GenerateToken(existingUser);
            return Ok(token);
        }


        private string GenerateToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyByte = Encoding.UTF8.GetBytes(_appSettings.SecretKey);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString()),
                    new Claim(ClaimTypes.Email, user.Email.ToString()),
                    new Claim("Id", user.UserId.ToString()),
                    new Claim("TokenId", Guid.NewGuid().ToString())

                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyByte),SecurityAlgorithms.HmacSha512Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescription);
            return jwtTokenHandler.WriteToken(token);

        }
        private void CreatePasswordHash(string password, out byte[] passwordHash,out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody]User user)
        {
            CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.Passwordhash = passwordHash;
            user.Passwordsalt = passwordSalt;
            user.Password = "";
            user.CreatedAt = DateTime.UtcNow;
            var userCheck = _context.Users.FirstOrDefault(x => x.Email == user.Email);
            if (userCheck != null)
            {
                return BadRequest("Email has existed");
            }
            else
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            return Ok();
        }
        // GET: api/User
        [EnableQuery]
        public IQueryable<User> Get()
        {
            return _context.Users;
        }
        [HttpGet("GetUsersByGmail/{gmail}")]
        public IActionResult GetUsers(string gmail)
        {
            var user = _context.Users.Where(x => x.Email.Contains(gmail)).Select(x => new
            {
                userId = x.UserId,
                username = x.Username,
                email = x.Email,
            }).ToList();
            return Ok(user);
        }

        [HttpGet("GetUsersLikeGmail/{gmail}")]
        public IActionResult GetUsersAll(string gmail)
        {
            var user = _context.Users.Where(x => x.Email.Trim().Equals(gmail.Trim())).Select(x => new
            {
                userId = x.UserId,
                username = x.Username,
                email = x.Email,
            }).FirstOrDefault();
            return Ok(user);
        }

        // GET: api/User(5)
        [EnableQuery]
        public SingleResult<User> Get([FromRoute] int key)
        {
            return SingleResult.Create(_context.Users.Where(user => user.UserId == key));
        }

        // POST: api/User
        public IActionResult Post([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Created(user);
        }

        [EnableQuery]

        // PUT: api/User(5)
        public IActionResult Put([FromRoute] int key, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.UserId == key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/User(5)
        public IActionResult Delete([FromRoute] int key)
        {
            var user = _context.Users.Find(key);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
