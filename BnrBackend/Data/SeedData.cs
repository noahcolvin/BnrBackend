using System.Collections.Generic;
using System.Linq;
using BnrBackend.Models;

namespace BnrBackend.Data
{
    public class SeedData
    {
        public static List<Post> Initialize(DataContext context)
        {
            var users = new List<User>
            {
                new User {Id = 1, Name = "Ryan Dahl", Email = "node4lyfe@example.com", Expertise = "Node"},
                new User {Id = 2, Name = "Rob Pike", Email = "gofarther@example.com", Expertise = "Go"},
                new User {Id = 3, Name = "DHH", Email = "magic@example.com", Expertise = "Rails"},
                new User {Id = 4, Name = "John Watkins", Email = "jwats@example.com", Expertise = ".NET"}
            };

            if (!context.Users.Any())
            {
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (context.Posts.Any()) return null;

            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    User = users.Single(u => u.Id == 1),
                    Title = "Node is awesome",
                    Body = "Node.js is a JavaScript runtime built on Chrome's V8 JavaScript engine."
                },
                new Post
                {
                    Id = 2,
                    User = users.Single(u => u.Id == 1),
                    Title = "Spring Boot is cooler", Body = "Spring Boot makes it easy to create stand-alone, production-grade Spring based Applications that you can \"just run\"."
                },
                new Post
                {
                    Id = 3,
                    User = users.Single(u => u.Id == 2),
                    Title = "Go is faster", Body = "Go is an open source programming language that makes it easy to build simple, reliable, and efficient software."
                },
                new Post
                {
                    Id = 4,
                    User = users.Single(u => u.Id == 3),
                    Title = "'What about me?' -Rails",
                    Body = "Ruby on Rails makes it much easier and more fun. It includes everything you need to build fantastic applications, and you can learn it with the support of our large, friendly community."
                },
                new Post
                {
                    Id = 5,
                    User = users.Single(u => u.Id == 4),
                    Title = ".NET has the gravy",
                    Body = ".NET enables engineers to develop blazing fast web services with ease, utilizing tools developed by Microsoft!"
                }
            };
            context.Posts.AddRange(posts);
            context.SaveChanges();

            return posts;
        }
    }
}