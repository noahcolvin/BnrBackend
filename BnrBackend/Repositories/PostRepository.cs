using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BnrBackend.Data;
using BnrBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace BnrBackend.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly DataContext _context;

        public PostRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllPosts(int? userId)
        {
            return await _context.Posts
                .Where(p => p.User.Id == userId || userId == null)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<Post> GetPost(int id)
        {
            return await _context.Posts
                .Where(p => p.Id == id)
                .Include(p => p.User)
                .SingleOrDefaultAsync();
        }

        public async Task AddPost(Post post)
        {
            var user = await _context.Users.FindAsync(post.User.Id);
            if (user != null)
                post.User = user;

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePost(Post post)
        {
            var user = await _context.Users.FindAsync(post.User.Id);
            if (user != null)
                post.User = user;

            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePost(Post post)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PostExists(int id)
        {
            return await _context.Posts.AnyAsync(e => e.Id == id);
        }
    }
}