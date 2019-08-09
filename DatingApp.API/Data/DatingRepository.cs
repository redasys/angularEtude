using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _ctx;
        public DatingRepository(DataContext ctx)
        {
            _ctx = ctx;

        }
        public void Add<T>(T entity) where T : class
        {
            _ctx.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _ctx.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _ctx.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _ctx.Photos.FirstOrDefaultAsync(x => x.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _ctx.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _ctx.Users.Include(p => p.Photos).ToListAsync();
            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await _ctx.SaveChangesAsync() > 0;
        }
    }
}