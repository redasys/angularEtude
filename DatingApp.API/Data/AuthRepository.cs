using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _ctx;

        public AuthRepository(DataContext context)
        {
            _ctx = context;
        }

        public async Task<User> Login(string userName, string password)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                return null;
            }
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] pwHash, pwSalt;
            CreatePasswordHash(password, out pwHash, out pwSalt);

            user.PasswordHash = pwHash;
            user.PasswordSalt = pwSalt;
            await _ctx.Users.AddAsync(user);
            await _ctx.SaveChangesAsync();

            return user;
        }


        public async Task<bool> UserExists(string userName)
        {
            return await _ctx.Users.AnyAsync(x=>x.UserName==userName);
        }

        private void CreatePasswordHash(string password, out byte[] pwHash, out byte[] pwSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                pwSalt = hmac.Key;
                pwHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }

            return true;
        }
    }
}