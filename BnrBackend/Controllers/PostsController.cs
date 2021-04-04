using System.Collections.Generic;
using System.Threading.Tasks;
using BnrBackend.Models;
using BnrBackend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BnrBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        public PostsController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts(int? userId)
        {
            return await _postRepository.GetAllPosts(userId);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _postRepository.GetPost(id);

            if (post == null)
                return NotFound();

            return post;
        }

        [HttpPost]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            if (await _postRepository.PostExists(post.Id))
                return BadRequest();

            await _postRepository.AddPost(post);

            return CreatedAtAction("GetPost", new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            if (id != post.Id)
                return BadRequest();

            try
            {
                await _postRepository.UpdatePost(post);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _postRepository.PostExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Post>> DeletePost(int id)
        {
            var post = await _postRepository.GetPost(id);
            if (post == null)
                return NotFound();

            await _postRepository.DeletePost(post);

            return post;
        }
    }
}