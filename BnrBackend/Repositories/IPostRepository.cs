using System.Collections.Generic;
using System.Threading.Tasks;
using BnrBackend.Models;

namespace BnrBackend.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllPosts();
        Task<Post> GetPost(int id);
        Task AddPost(Post post);
        Task UpdatePost(Post post);
        Task DeletePost(Post post);
        Task<bool> PostExists(int id);
    }
}