using Library_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Library_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly LibraryDBContext _context;
        private readonly JWTSettings _jwtsettings;

        public MembersController(LibraryDBContext context, IOptions<JWTSettings> jwtsettings)
        {
            _context = context;
            _jwtsettings = jwtsettings.Value;
        }

        // GET: api/Members
        [HttpGet("GetMembers")]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("GetMember/{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // GET: api/Members/5
        [HttpGet("GetMemberDetails/{id}")]
        public async Task<ActionResult<Member>> GetMemberDetails(int id)
        {
            var member = await _context.Members.Where(u => u.MemberId == id)
                                            .FirstOrDefaultAsync();

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // POST: api/Members
        [HttpPost("Login")]
        public async Task<ActionResult<MemberWithToken>> Login([FromBody] Member member)
        {
            member = await _context.Members.Where(u => u.Email == member.Email && u.Password == member.Password).FirstOrDefaultAsync();

            MemberWithToken memberWithToken = null;

            if (member != null)
            {
                RefreshToken refreshToken = GenerateRefreshToken();
                member.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                memberWithToken = new MemberWithToken(member);
                memberWithToken.RefreshToken = refreshToken.Token;
            }

            if (memberWithToken == null)
            {
                return NotFound();
            }

            //sign your token here here..
            memberWithToken.AccessToken = GenerateAccessToken(member.MemberId);
            return memberWithToken;
        }

        // POST: api/Members
        [HttpPost("RegisterMember")]
        public async Task<ActionResult<MemberWithToken>> RegisterMember([FromBody] Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            //load role for registered member
            member = await _context.Members.Where(u => u.MemberId == member.MemberId).FirstOrDefaultAsync();

            MemberWithToken memberWithToken = null;

            if (member != null)
            {
                RefreshToken refreshToken = GenerateRefreshToken();
                member.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                memberWithToken = new MemberWithToken(member);
                memberWithToken.RefreshToken = refreshToken.Token;
            }

            if (memberWithToken == null)
            {
                return NotFound();
            }

            //sign your token here here..
            memberWithToken.AccessToken = GenerateAccessToken(member.MemberId);
            return memberWithToken;
        }

        // GET: api/Members
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<MemberWithToken>> RefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            Member member = await GetMemberFromAccessToken(refreshRequest.AccessToken);

            if (member != null && ValidateRefreshToken(member, refreshRequest.RefreshToken))
            {
                MemberWithToken memberWithToken = new MemberWithToken(member);
                memberWithToken.AccessToken = GenerateAccessToken(member.MemberId);

                return memberWithToken;
            }

            return null;
        }

        // GET: api/Members
        [HttpPost("GetMemberByAccessToken")]
        public async Task<ActionResult<Member>> GetMemberByAccessToken([FromBody] string accessToken)
        {
            Member member = await GetMemberFromAccessToken(accessToken);

            if (member != null)
            {
                return member;
            }

            return null;
        }

        private bool ValidateRefreshToken(Member member, string refreshToken)
        {

            RefreshToken refreshTokenMember = _context.RefreshTokens.Where(rt => rt.Token == refreshToken)
                                                .OrderByDescending(rt => rt.ExpiryDate)
                                                .FirstOrDefault();

            if (refreshTokenMember != null && refreshTokenMember.MemberId == member.MemberId
                && refreshTokenMember.ExpiryDate > DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }

        private async Task<Member> GetMemberFromAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken securityToken;
                var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);

                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var memberId = principle.FindFirst(ClaimTypes.Name)?.Value;

                    return await _context.Members.Where(u => u.MemberId == Convert.ToInt32(memberId)).FirstOrDefaultAsync();
                }
            }
            catch (Exception)
            {
                return new Member();
            }

            return new Member();
        }

        private RefreshToken GenerateRefreshToken()
        {
            RefreshToken refreshToken = new RefreshToken();

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddMonths(6);

            return refreshToken;
        }

        private string GenerateAccessToken(int memberId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, Convert.ToString(memberId))
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // PUT: api/Members/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("UpdateMember/{id}")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.MemberId)
            {
                return BadRequest();
            }

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
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

        // POST: api/Members
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("CreateMember")]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.MemberId }, member);
        }

        // DELETE: api/Members/5
        [HttpDelete("DeleteMember/{id}")]
        public async Task<ActionResult<Member>> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return member;
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }
    }
}
