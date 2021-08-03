﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    internal class CommentServiceTests
    {
        private DbSet<Post> _postsDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Comment> _commentsDbSet;
        private DbSet<WallModerator> _wallModeratorsDbSet;
        private ISystemClock _systemClock;
        private IPermissionService _permissionService;
        private ICommentService _commentService;

        private readonly string _userId = Guid.NewGuid().ToString();

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _postsDbSet = uow.MockDbSetForAsync<Post>();
            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _commentsDbSet = uow.MockDbSetForAsync<Comment>();
            _wallModeratorsDbSet = uow.MockDbSetForAsync<WallModerator>();
            _ = uow.MockDbSetForAsync<PostWatcher>();

            _systemClock = Substitute.For<ISystemClock>();
            _permissionService = Substitute.For<IPermissionService>();

            _commentService = new CommentService(uow, _systemClock, _permissionService);
        }

        [Test]
        public void Should_Like_Comment()
        {
            var comment = new Comment
            {
                Id = 1,
                Likes = new LikesCollection(),
                Post = new Post
                {
                    Wall = new Wall
                    {
                        OrganizationId = 2
                    }
                }
            };

            _commentsDbSet.SetDbSetDataForAsync(new List<Comment> { comment }.AsQueryable());
            _commentService.ToggleLikeAsync(1, new UserAndOrganizationDTO { UserId = "user1", OrganizationId = 2 });

            Assert.AreEqual("user1", _commentsDbSet.First().Likes.First().UserId);
        }

        [Test]
        public void Should_Throw_If_There_Is_No_Comment_To_Be_Liked()
        {
            _commentsDbSet.SetDbSetDataForAsync(new List<Comment>().AsQueryable());

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _commentService.ToggleLikeAsync(1, new UserAndOrganizationDTO { UserId = "user1", OrganizationId = 2 }));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public async Task Should_Unlike_Post()
        {
            var comment = new Comment
            {
                Id = 1,
                Likes = new LikesCollection { new Like("user1") },
                Post = new Post
                {
                    Wall = new Wall
                    {
                        OrganizationId = 2
                    }
                }
            };

            _commentsDbSet.SetDbSetDataForAsync(new List<Comment> { comment }.AsQueryable());
            await _commentService.ToggleLikeAsync(1, new UserAndOrganizationDTO { UserId = "user1", OrganizationId = 2 });

            Assert.AreEqual(0, (await _commentsDbSet.FirstAsync()).Likes.Count);
        }

        [Test]
        public async Task Should_Create_New_Comment()
        {
            // Setup
            var posts = new List<Post>
            {
                new Post { Id = 1, Wall = new Wall { OrganizationId = 2 } }
            };
            _postsDbSet.SetDbSetDataForAsync(posts.AsQueryable());

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = _userId
                }
            };
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());

            var expectedDateTime = DateTime.UtcNow;
            _systemClock.UtcNow.Returns(expectedDateTime);

            var newCommentDto = new NewCommentDTO
            {
                MessageBody = "test",
                OrganizationId = 2,
                PictureId = "pic",
                PostId = 1,
                UserId = _userId
            };

            // Act
            await _commentService.CreateCommentAsync(newCommentDto);

            // Assert
            _commentsDbSet.Received(1)
                .Add(Arg.Is<Comment>(c => c.AuthorId == _userId && c.MessageBody == "test" && c.PostId == 1));

            Assert.AreEqual((await _postsDbSet.FirstAsync()).LastActivity, expectedDateTime);
        }

        [Test]
        public void Should_Not_Throw_If_Moderator_Edits_Other_User_Comment()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };
            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var editCommentDto = new EditCommentDTO
            {
                Id = 1,
                MessageBody = "edited comment",
                UserId = "user2",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(editCommentDto, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _commentService.EditCommentAsync(editCommentDto));
        }

        [Test]
        public void Should_Not_Throw_If_Administrator_Edits_Other_User_Comment()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };
            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var editCommentDto = new EditCommentDTO
            {
                Id = 1,
                MessageBody = "edited comment",
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(editCommentDto, AdministrationPermissions.Post).Returns(true);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _commentService.EditCommentAsync(editCommentDto));
        }

        [Test]
        public void Should_Throw_If_User_Edits_Other_User_Comment()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };
            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var editCommentDto = new EditCommentDTO
            {
                Id = 1,
                MessageBody = "edited comment",
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(editCommentDto, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () => await _commentService.EditCommentAsync(editCommentDto));
        }

        [Test]
        public void Should_Throw_If_Comment_To_Be_Edited_Does_Not_Exist()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };

            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var editCommentDto = new EditCommentDTO
            {
                Id = 2,
                MessageBody = "edited comment",
                UserId = "user1",
                OrganizationId = 2
            };

            // Act
            // Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _commentService.EditCommentAsync(editCommentDto));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_Comment_To_Be_Deleted_Does_Not_Exist()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };

            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user1",
                OrganizationId = 2
            };

            // Act
            // Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _commentService.DeleteCommentAsync(2, userOrg));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_If_User_Deletes_Other_User_Comment()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };
            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () => await _commentService.DeleteCommentAsync(1, userOrg));
        }

        [Test]
        public void Should_Not_Throw_If_Administrator_Deletes_Other_User_Comment()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };
            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user3",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(true);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _commentService.DeleteCommentAsync(1, userOrg));
        }

        [Test]
        public void Should_Not_Throw_If_Moderator_Deletes_Other_User_Comment()
        {
            // Setup
            var wall = new Wall { Id = 1, OrganizationId = 2 };
            var post = new Post { Id = 1, Wall = wall, WallId = wall.Id };
            var comments = new List<Comment>
            {
                new Comment { Id = 1, AuthorId = "user1", Post = post }
            };
            _commentsDbSet.SetDbSetDataForAsync(comments.AsQueryable());

            var wallModerators = new List<WallModerator>
            {
                new WallModerator { WallId = wall.Id, UserId = "user2" }
            };
            _wallModeratorsDbSet.SetDbSetDataForAsync(wallModerators.AsQueryable());

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "user2",
                OrganizationId = 2
            };

            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Post).Returns(false);

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await _commentService.DeleteCommentAsync(1, userOrg));
        }
    }
}
