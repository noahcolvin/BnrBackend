using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BnrBackend.Data;
using BnrBackend.Models;
using BnrBackend.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BnrBackend.Test.Integration.Repositories
{
    [TestFixture]
    public class PostRepositoryTests
    {
        private DataContext _context;
        private IPostRepository _subject;
        private List<Post> _posts;

        [SetUp]
        public async Task Setup()
        {
            var dbContextOptions = new DbContextOptionsBuilder<DataContext>();
            dbContextOptions.UseSqlite("Data Source=:memory:");
            _context = new DataContext(dbContextOptions.Options);
            await _context.Database.OpenConnectionAsync();
            await _context.Database.EnsureCreatedAsync();

            await SeedPosts();

            _subject = new PostRepository(_context);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _context.DisposeAsync();
        }

        [Test]
        public async Task GetsAllPostsWithUsers()
        {
            var result = await _subject.GetAllPosts(null);

            result.Should().BeEquivalentTo(_posts);
        }

        [Test]
        public async Task GetsAllPostsWithUsersFiltered()
        {
            var result = await _subject.GetAllPosts(1);

            result.Should().BeEquivalentTo(new List<Post> { _posts[0] });
        }

        [Test]
        public async Task GetsSinglePostWithUser()
        {
            var result = await _subject.GetPost(1);

            result.Should().BeEquivalentTo(_posts[0]);
        }

        [Test]
        public async Task ReturnsNullIfNoPostFound()
        {
            var result = await _subject.GetPost(911);

            result.Should().BeNull();
        }

        [Test]
        public async Task AddsPostWithValidUser()
        {
            var post = new Post
            {
                Title = "Frist!",
                Body = "Beat you!",
                User = new User { Id = 2 }
            };
            await _subject.AddPost(post);

            var actual = await _context.Posts.Where(p => p.Title == post.Title).SingleOrDefaultAsync();
            actual.Should().BeEquivalentTo(post);
        }

        [Test]
        public async Task UpdatesExistingPost()
        {
            var post = await _context.Posts.FindAsync(1);
            post.Title = "Frist!";
            post.Body = "Beat you!";
            post.User = new User { Id = 2 };

            await _subject.UpdatePost(post);

            var actual = await _context.Posts.Where(p => p.Title == post.Title).SingleOrDefaultAsync();
            actual.Should().BeEquivalentTo(post);
        }

        [Test]
        public async Task DeletesPost()
        {
            var post = await _context.Posts.FindAsync(1);

            await _subject.DeletePost(post);

            var actual = await _context.Posts.Where(p => p.Id == 1).SingleOrDefaultAsync();
            actual.Should().BeNull();
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(911, ExpectedResult = false)]
        public async Task<bool> IndicatesPostExists(int id)
        {
            return await _subject.PostExists(id);
        }

        private async Task SeedPosts()
        {
            var users = new List<User>
            {
                new User {Id = 1, Name = "Some Gal", Email = "person@email.com", Expertise = "Driving"},
                new User {Id = 2, Name = "Some Guy", Email = "human@email.com.com", Expertise = "Walking"}
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            _posts = new List<Post>
            {
                new Post {Id = 1, User = users[0], Title = "Beep Beep", Body = "I'm driving here!"},
                new Post {Id = 2, User = users[1], Title = "Pitter Patter", Body = "Hey, I'm walking here!"}
            };
            await _context.Posts.AddRangeAsync(_posts);
            await _context.SaveChangesAsync();
        }
    }
}