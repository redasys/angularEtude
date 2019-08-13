using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
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

        public async Task<Like> GetLike(int useriD, int recipientId)
        {
            return await _ctx.Likes.FirstOrDefaultAsync(u => u.LikerId == useriD && u.LikeeId == recipientId);
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

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            var users = _ctx.Users.Include(p => p.Photos).Where(u => u.Id != userParams.UserId && u.Gender == userParams.Gender
                && u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            if (userParams.Likers)
            {
                var likers = await GetUserLikes(userParams.UserId, true);
                users = users.Where(u => likers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var likees = await GetUserLikes(userParams.UserId, false);
                users = users.Where(u => likees.Contains(u.Id));
            }

            if (!userParams.OrderBy.IsNullOrEmpty())
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(x => x.Created);
                        break;
                    default:
                        users = users.OrderByDescending(x => x.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }


        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _ctx.Users.Include(x => x.Likers).Include(x => x.Likees).FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(u => u.LikerId);
            }

            return user.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
        }

        public async Task<bool> SaveAll()
        {
            return await _ctx.SaveChangesAsync() > 0;
        }
    }
}