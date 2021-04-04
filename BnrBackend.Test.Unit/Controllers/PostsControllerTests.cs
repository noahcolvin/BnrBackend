using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BnrBackend.Controllers;
using BnrBackend.Models;
using BnrBackend.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace BnrBackend.Test.Unit.Controllers
{
    [TestFixture]
    public class PostsControllerTests
    {
        private PostsController _subject;
        private Mock<IPostRepository> _repoMock;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IPostRepository>();
            _subject = new PostsController(_repoMock.Object);
        }

        [Test]
        public async Task GetPosts_GetsAllPosts()
        {
            await _subject.GetPosts(10);
            _repoMock.Verify(r => r.GetAllPosts(10));
        }

        [Test]
        public async Task GetPost_ReturnsPost()
        {
            var expected = new Post();
            _repoMock.Setup(r => r.GetPost(90))
                .ReturnsAsync(expected);

            var result = await _subject.GetPost(90);
            result.Value.Should().Be(expected);
        }

        [Test]
        public async Task GetPost_Returns404IfNotFound()
        {
            var result = await _subject.GetPost(90);
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task PostPost_Returns400IfAlreadyExists()
        {
            var post = new Post { Id = 911 };
            _repoMock.Setup(r => r.PostExists(post.Id))
                .ReturnsAsync(true);

            var result = await _subject.PostPost(post);
            result.Result.Should().BeOfType<BadRequestResult>();
        }

        [Test]
        public async Task PostPost_AddsNewPost()
        {
            var post = new Post { Id = 911 };
            _repoMock.Setup(r => r.PostExists(post.Id))
                .ReturnsAsync(false);

            await _subject.PostPost(post);
            _repoMock.Verify(r => r.AddPost(post));
        }

        [Test]
        public async Task PostPost_RedirectsToGetAction()
        {
            var post = new Post { Id = 911 };
            _repoMock.Setup(r => r.PostExists(post.Id))
                .ReturnsAsync(false);

            var result = await _subject.PostPost(post);
            result.Result.Should().BeOfType<CreatedAtActionResult>().Which.ActionName.Should().Be(nameof(PostsController.GetPost));
        }

        [Test]
        public async Task PutPost_Returns400IfIdsMismatch()
        {
            var post = new Post { Id = 911 };
            var result = await _subject.PutPost(90, post);
            result.Should().BeOfType<BadRequestResult>();
        }

        [Test]
        public async Task PutPost_UpdatesPost()
        {
            var post = new Post { Id = 911 };
            await _subject.PutPost(911, post);
            _repoMock.Verify(r => r.UpdatePost(post));
        }

        [Test]
        public async Task PutPost_Returns404OnConcurrencyExceptionIfPostDoesNotExist()
        {
            var post = new Post { Id = 911 };
            _repoMock.Setup(r => r.PostExists(post.Id))
                .ReturnsAsync(false);
            _repoMock.Setup(r => r.UpdatePost(post))
                .Throws<DbUpdateConcurrencyException>();

            var result = await _subject.PutPost(911, post);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void PutPost_RethrowsConcurrencyExceptionIfPostDoesExists()
        {
            var post = new Post { Id = 911 };
            _repoMock.Setup(r => r.PostExists(post.Id))
                .ReturnsAsync(true);
            _repoMock.Setup(r => r.UpdatePost(post))
                .Throws<DbUpdateConcurrencyException>();

            Func<Task> f = () => _subject.PutPost(911, post);
            f.Should().Throw<DbUpdateConcurrencyException>();
        }

        [Test]
        public async Task PutPost_Returns204IfAllGood()
        {
            var post = new Post { Id = 911 };
            var result = await _subject.PutPost(911, post);
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task DeletePost_Returns404IfPostDoesNotExist()
        {
            _repoMock.Setup(r => r.GetPost(911))
                .ReturnsAsync(null as Post);

            var result = await _subject.DeletePost(911);
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task DeletePost_DeletesIfFound()
        {
            var post = new Post();
            _repoMock.Setup(r => r.GetPost(911))
                .ReturnsAsync(post);

            await _subject.DeletePost(911);
            _repoMock.Verify(r => r.DeletePost(post));
        }

        [Test]
        public async Task DeletePost_ReturnsDeletedPost()
        {
            var post = new Post();
            _repoMock.Setup(r => r.GetPost(911))
                .ReturnsAsync(post);

            var result = await _subject.DeletePost(911);
            result.Value.Should().Be(post);
        }
    }
}