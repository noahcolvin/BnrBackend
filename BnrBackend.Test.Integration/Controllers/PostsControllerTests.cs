using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BnrBackend.Data;
using BnrBackend.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BnrBackend.Test.Integration.Controllers
{
    [TestFixture]
    public class PostsControllerTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private List<Post> _posts;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        static Post[] MissingDataCases =
        {
            new Post { Title = null, Body = "McPost", User = new User { Id = 1 } },
            new Post { Title = "Posty", Body = null, User = new User { Id = 1 } },
            new Post { Title = "Posty", Body = "McPost", User = null }
        };

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    var projectDir = Directory.GetCurrentDirectory();
                    var configPath = Path.Combine(projectDir, "appsettings.json");
                    configurationBuilder.AddJsonFile(configPath);
                });
                builder.ConfigureTestServices(services =>
                {
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var context = scopedServices.GetRequiredService<DataContext>();
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    _posts = SeedData.Initialize(context);
                });
            });
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.CancelPendingRequests();
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GetsAllPosts()
        {
            var result = await _client.GetStringAsync("api/posts");
            var actual = JsonSerializer.Deserialize<List<Post>>(result, _jsonSerializerOptions);
            actual.Should().BeEquivalentTo(_posts);
        }

        [Test]
        public async Task GetsAllPostsForUser()
        {
            var result = await _client.GetStringAsync($"api/posts?userId={_posts[0].User.Id}");
            var actual = JsonSerializer.Deserialize<List<Post>>(result, _jsonSerializerOptions);
            actual.Should().BeEquivalentTo(_posts.Where(p => p.User == _posts[0].User));
        }

        [Test]
        public async Task GetsSinglePost()
        {
            var result = await _client.GetStringAsync("api/posts/1");
            var actual = JsonSerializer.Deserialize<Post>(result, _jsonSerializerOptions);
            actual.Should().BeEquivalentTo(_posts[0]);
        }

        [Test]
        public async Task IndicatesSinglePostNotFound()
        {
            var result = await _client.GetAsync("api/posts/123");
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task AddsPost()
        {
            var post = new Post { Title = "Posty", Body = "McPost", User = new User { Id = 1 } };
            var jsonPost = JsonSerializer.Serialize(post, _jsonSerializerOptions);

            var result = await _client.PostAsync("api/posts", new StringContent(jsonPost, Encoding.UTF8, "application/json"));
            result.StatusCode.Should().Be(HttpStatusCode.Created);
            var actual = JsonSerializer.Deserialize<Post>(await result.Content.ReadAsStringAsync(), _jsonSerializerOptions);
            actual.Should().BeEquivalentTo(post, o => o.Excluding(x => x.Id).Excluding(x => x.User).Including(x => x.User.Id));
        }

        [TestCaseSource(nameof(MissingDataCases))]
        public async Task DoesNotAddEmptyPostMissingRequiredData(Post post)
        {
            var jsonPost = JsonSerializer.Serialize(post, _jsonSerializerOptions);

            var result = await _client.PostAsync("api/posts", new StringContent(jsonPost, Encoding.UTF8, "application/json"));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdatesPost()
        {
            var post = _posts[2];
            post.Title = "Change me";
            var jsonPost = JsonSerializer.Serialize(post, _jsonSerializerOptions);

            var result = await _client.PutAsync($"api/posts/{post.Id}", new StringContent(jsonPost, Encoding.UTF8, "application/json"));
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using var context = GetNewContext();
            (await context.Posts.FindAsync(_posts[2].Id)).Title.Should().Be("Change me");
        }

        [TestCaseSource(nameof(MissingDataCases))]
        public async Task DoesNotUpdatePostMissingRequiredData(Post postMissingData)
        {
            var post = _posts[2];
            post.Title = postMissingData.Title;
            post.Body = postMissingData.Body;
            post.User = postMissingData.User;
            var jsonPost = JsonSerializer.Serialize(post, _jsonSerializerOptions);

            var result = await _client.PutAsync($"api/posts/{post.Id}", new StringContent(jsonPost, Encoding.UTF8, "application/json"));
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task DeletesPost()
        {
            var result = await _client.DeleteAsync($"api/posts/{_posts[2].Id}");
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var actual = JsonSerializer.Deserialize<Post>(await result.Content.ReadAsStringAsync(), _jsonSerializerOptions);
            actual.Should().BeEquivalentTo(_posts[2]);

            await using var context = GetNewContext();
            (await context.Posts.FindAsync(_posts[2].Id)).Should().BeNull();
        }

        private DataContext GetNewContext()
        {
            var dbContextOptions = new DbContextOptionsBuilder<DataContext>();
            dbContextOptions.UseSqlite("Data Source=application.test.db;Cache=Shared");
            return new DataContext(dbContextOptions.Options);
        }
    }
}