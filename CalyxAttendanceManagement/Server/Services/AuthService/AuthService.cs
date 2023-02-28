using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CalyxAttendanceManagement.Server.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(DataContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        public string GetUserEmail() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);

        public Task<User> GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string>();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
            }
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Wrong password";
            }
            else if (!user.IsAuthenticated)
            {
                response.Success = false;
                response.Message = "You need to verify email.";
            }
            else
            {
                response.Data = CreateToken(user);
            }

            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            if (await UserExists(user.Email))
            {
                return new ServiceResponse<int> { 
                    Success = false,
                    Message = "User already exists."
                };
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.Name = user.FirstName + " " + user.LastName;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            if (user.Email == "wayne_kim@calyxsoftware.com")
                user.Role = "Admin";

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            await SendEmail( new SendEmail { Name= user.Name, Email = user.Email });

            return new ServiceResponse<int> { Data = user.Id, Message = "Registration successful and Sent email for verify." };
        }

        public async Task<bool> UserExists(string email)
        {
            if (await _context.Users.AnyAsync(user => user.Email.ToLower()
                .Equals(email.ToLower())))
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // cryptography algorithm
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // TODO -- token role
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public async Task<ServiceResponse<bool>> VerifyEmail(string email)
        {
            var response = new ServiceResponse<bool>();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
            }
            else
            {
                user.IsAuthenticated = true;

                await _context.SaveChangesAsync();

                response.Success = true;
            }

            return response;
        }

        private async Task<bool> SendEmail(SendEmail request)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("koreaus1@naver.com", "Calyx Attendance Management");
            var to = new EmailAddress(request.Email, request.Name);

            var templateId = "d-45b32869cd3e47f68af6a88fa36598b0";
            var dynamicTemplateData = new
            {
                subject = "Verify Email Address for Calyx Attendance Management",
                sender_name = request.Name,
                sender_email = request.Email
            };

            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, dynamicTemplateData);
            var response = await client.SendEmailAsync(msg);

            return true;
        }

        public async Task<ServiceResponse<User>> GetUser()
        {
            var id = GetUserId();
            var email = GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email) && u.Id.Equals(id));

            return new ServiceResponse<User> { Data = user };
        }

        public async Task<ServiceResponse<List<User>>> GetUsers()
        {
            var id = GetUserId();
            var email = GetUserEmail();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email) && u.Id.Equals(id));

            if(user != null && user.Role == "Admin")
            {
                var users = await _context.Users.ToListAsync();

                return new ServiceResponse<List<User>> { Data = users };

            } else
            {
                return new ServiceResponse<List<User>>
                {
                    Success = false,
                    Message = "Sorry. This data only can see admin.",
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateProfile(int userId, UpdateProfile profile)
        {
            var email = GetUserEmail();

            var user = await _context.Users.Where(u => u.Id.Equals(userId) && u.Email.Equals(email)).FirstOrDefaultAsync();

            if (user == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Save failed.",
                };
            }

            user.Name = profile.UserName;
            user.PhoneNumber = profile.PhoneNumber;
            //user.DateUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true, Message = "Profile is saved." };
        }

        public async Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "User not found.",
                };
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true, Message = "Password has been changed." };
        }

    }
}
