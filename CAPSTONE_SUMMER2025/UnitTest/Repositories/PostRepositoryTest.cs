using API.DTO.PostDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTest;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.IO;
using API.Utils.Constants;

namespace UnitTest.Repositories
{
    public class PostRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IPostRepository _postRepository;
        private readonly Mock<IFilebaseHandler> _filebaseHandlerMock;

        public PostRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _filebaseHandlerMock = new Mock<IFilebaseHandler>();
            _filebaseHandlerMock.Setup(f => f.UploadMediaFile(It.IsAny<IFormFile>())).ReturnsAsync("http://mocked-url.com/media");
            _filebaseHandlerMock.Setup(f => f.DeleteFileByUrlAsync(It.IsAny<string>())).ReturnsAsync(true);
            _filebaseHandlerMock.Setup(f => f.GeneratePreSignedUrl(It.IsAny<string>())).Returns<string>(url => url);
            _postRepository = new PostRepository(_filebaseHandlerMock.Object, _context);
        }

        #region CreatePost
        [Fact]
        public async Task CreatePost_ValidPostWithAccountId_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "avatar.jpg" } };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            var reqPostDTO = new ReqPostDTO
            {
                AccountId = 1,
                Title = "Test Post",
                Content = "Test Content",
                MediaFiles = new List<IFormFile> { CreateMockFormFile("image.jpg") }
            };

            // Act
            var result = await _postRepository.CreatePost(reqPostDTO);

            // Assert
            Assert.True(result);
            var savedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Title == "Test Post");
            Assert.NotNull(savedPost);
            Assert.Equal(1, savedPost.AccountId);
            Assert.Equal("Test Post", savedPost.Title);
            Assert.Equal("Test Content", savedPost.Content);
            var savedMedia = await _context.PostMedia.FirstOrDefaultAsync(m => m.PostId == savedPost.PostId);
            Assert.NotNull(savedMedia);
            Assert.Equal("http://mocked-url.com/media", savedMedia.MediaUrl);
            Assert.Equal(0, savedMedia.DisplayOrder);
        }

        [Fact]
        public async Task CreatePost_ValidPostWithStartupId_ReturnsTrue()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup", Logo = "logo.jpg" };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();
            var reqPostDTO = new ReqPostDTO
            {
                StartupId = 1,
                Title = "Test Post",
                Content = "Test Content",
                MediaFiles = new List<IFormFile> { CreateMockFormFile("image.jpg") }
            };

            // Act
            var result = await _postRepository.CreatePost(reqPostDTO);

            // Assert
            Assert.True(result);
            var savedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Title == "Test Post");
            Assert.NotNull(savedPost);
            Assert.Equal(1, savedPost.StartupId);
            Assert.Equal("Test Post", savedPost.Title);
            Assert.Equal("Test Content", savedPost.Content);
            var savedMedia = await _context.PostMedia.FirstOrDefaultAsync(m => m.PostId == savedPost.PostId);
            Assert.NotNull(savedMedia);
            Assert.Equal("http://mocked-url.com/media", savedMedia.MediaUrl);
            Assert.Equal(0, savedMedia.DisplayOrder);
        }

        [Fact]
        public async Task CreatePost_NoMediaFiles_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1 };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            var reqPostDTO = new ReqPostDTO
            {
                AccountId = 1,
                Title = "Post Without Media",
                Content = "No media here"
            };

            // Act
            var result = await _postRepository.CreatePost(reqPostDTO);

            // Assert
            Assert.True(result);
            var savedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Title == "Post Without Media");
            Assert.NotNull(savedPost);
            var media = await _context.PostMedia.Where(m => m.PostId == savedPost.PostId).ToListAsync();
            Assert.Empty(media);
        }
        #endregion

        #region UpdatePostAsync
        [Fact]
        public async Task UpdatePostAsync_ValidPost_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Old Title", Content = "Old Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UpdatePostAsync(1, "New Title", "New Content");

            // Assert
            Assert.True(result);
            var updatedPost = await _context.Posts.FindAsync(1);
            Assert.NotNull(updatedPost);
            Assert.Equal("New Title", updatedPost.Title);
            Assert.Equal("New Content", updatedPost.Content);
        }

        [Fact]
        public async Task UpdatePostAsync_NonExistingPost_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = await _postRepository.UpdatePostAsync(999, "New Title", "New Content");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdatePostAsync_TitleOrContentIsNull_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 2, AccountProfile = new AccountProfile { FirstName = "Jane", LastName = "Doe" } };
            var post = new Post { PostId = 2, AccountId = 2, Title = "Original Title", Content = "Original Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UpdatePostAsync(2, null, null);

            // Assert
            Assert.True(result);
            var updatedPost = await _context.Posts.FindAsync(2);
            Assert.Null(updatedPost.Title);
            Assert.Null(updatedPost.Content);
        }

        [Fact]
        public async Task UpdatePostAsync_TitleOrContentIsEmpty_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 4, AccountProfile = new AccountProfile { FirstName = "Empty", LastName = "Case" } };
            var post = new Post { PostId = 4, AccountId = 4, Title = "Original Title", Content = "Original Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UpdatePostAsync(4, "", "");

            // Assert
            Assert.True(result);
            var updatedPost = await _context.Posts.FindAsync(4);
            Assert.Equal("", updatedPost.Title);
            Assert.Equal("", updatedPost.Content);
        }

        [Fact]
        public async Task UpdatePostAsync_SameValues_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 3, AccountProfile = new AccountProfile { FirstName = "Alex", LastName = "Smith" } };
            var post = new Post { PostId = 3, AccountId = 3, Title = "Same Title", Content = "Same Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UpdatePostAsync(3, "Same Title", "Same Content");

            // Assert
            Assert.True(result);
            var updatedPost = await _context.Posts.FindAsync(3);
            Assert.Equal("Same Title", updatedPost.Title);
            Assert.Equal("Same Content", updatedPost.Content);
        }
        #endregion

        #region DeletePostAsync
        [Fact]
        public async Task DeletePostAsync_ValidPost_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post
            {
                PostId = 1,
                AccountId = 1,
                Title = "Test Post",
                Content = "Test Content",
                PostMedia = new List<PostMedium> { new PostMedium { PostMediaId = 1, MediaUrl = "http://mocked-url.com/media", DisplayOrder = 0 } },
                PostComments = new List<PostComment> { new PostComment { PostcommentId = 1, Content = "Comment" } },
                PostLikes = new List<PostLike> { new PostLike { PostLikeId = 1, AccountId = 1 } }
            };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.DeletePostAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal(0, await _context.Posts.CountAsync());
            Assert.Equal(0, await _context.PostMedia.CountAsync());
            Assert.Equal(0, await _context.PostComments.CountAsync());
            Assert.Equal(0, await _context.PostLikes.CountAsync());
            _filebaseHandlerMock.Verify(f => f.DeleteFileByUrlAsync("http://mocked-url.com/media"), Times.Once());
        }

        [Fact]
        public async Task DeletePostAsync_NonExistingPost_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = await _postRepository.DeletePostAsync(999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetPostByPostIdAsync
        [Fact]
        public async Task GetPostByPostIdAsync_ValidId_ReturnsPost()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "avatar.jpg" } };
            var post = new Post
            {
                PostId = 1,
                AccountId = 1,
                Title = "Test Post",
                Content = "Test Content",
                PostMedia = new List<PostMedium> { new PostMedium { PostMediaId = 1, MediaUrl = "media.jpg", DisplayOrder = 0 } }
            };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostByPostIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PostId);
            Assert.Equal("Test Post", result.Title);
            Assert.Equal("Test Content", result.Content);
            Assert.Single(result.PostMedia);
            Assert.Equal("media.jpg", result.PostMedia.First().MediaUrl);
            Assert.Equal("John Doe", result.Account.AccountProfile.FirstName + " " + result.Account.AccountProfile.LastName);
        }

        [Fact]
        public async Task GetPostByPostIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _postRepository.GetPostByPostIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region CreatePostComment
        [Fact]
        public async Task CreatePostComment_ValidComment_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var reqPostCommentDTO = new reqPostCommentDTO
            {
                AccountId = 1,
                PostId = 1,
                Content = "Test Comment",
                ParentCommentId = null
            };

            // Act
            var result = await _postRepository.CreatePostComment(reqPostCommentDTO);

            // Assert
            Assert.True(result);
            var savedComment = await _context.PostComments.FirstOrDefaultAsync(c => c.Content == "Test Comment");
            Assert.NotNull(savedComment);
            Assert.Equal(1, savedComment.AccountId);
            Assert.Equal(1, savedComment.PostId);
            Assert.Equal("Test Comment", savedComment.Content);
            Assert.Null(savedComment.ParentCommentId);
        }

        [Fact]
        public async Task CreatePostComment_NonExistingAccountId_ReturnsFalse()
        {
            // Arrange
            var post = new Post { PostId = 1, Title = "Test Post", Content = "Test Content" };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var reqPostCommentDTO = new reqPostCommentDTO
            {
                AccountId = 999,
                PostId = 1,
                Content = "Test Comment"
            };

            // Act
            var result = await _postRepository.CreatePostComment(reqPostCommentDTO);

            // Assert
            Assert.False(result);
            Assert.Empty(await _context.PostComments.ToListAsync());
        }

        [Fact]
        public async Task CreatePostComment_NonExistingPostId_ReturnsFalse()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            var reqPostCommentDTO = new reqPostCommentDTO
            {
                AccountId = 1,
                PostId = 999,
                Content = "Test Comment"
            };

            // Act
            var result = await _postRepository.CreatePostComment(reqPostCommentDTO);

            // Assert
            Assert.False(result);
            Assert.Empty(await _context.PostComments.ToListAsync());
        }
        #endregion

        #region UpdateCommentAsync
        [Fact]
        public async Task UpdateCommentAsync_ValidComment_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var comment = new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Old Comment" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UpdateCommentAsync(1, "New Comment");

            // Assert
            Assert.True(result);
            var updatedComment = await _context.PostComments.FindAsync(1);
            Assert.NotNull(updatedComment);
            Assert.Equal("New Comment", updatedComment.Content);
        }

        [Fact]
        public async Task UpdateCommentAsync_NullContent_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var comment = new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Old Comment" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UpdateCommentAsync(1, null);

            // Assert
            Assert.True(result);
            var updatedComment = await _context.PostComments.FindAsync(1);
            Assert.NotNull(updatedComment);
            Assert.Equal(null, updatedComment.Content);
        }

        [Fact]
        public async Task UpdateCommentAsync_EmptyContent_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var comment = new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Old Comment" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UpdateCommentAsync(1, "");

            // Assert
            Assert.True(result);
            var updatedComment = await _context.PostComments.FindAsync(1);
            Assert.NotNull(updatedComment);
            Assert.Equal("", updatedComment.Content);
        }

        [Fact]
        public async Task UpdateCommentAsync_NonExistingComment_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = await _postRepository.UpdateCommentAsync(999, "New Comment");

            // Assert
            Assert.False(result);
        }
        #endregion

        #region DeleteCommentAsync
        [Fact]
        public async Task DeleteCommentAsync_ValidCommentWithChildren_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var parentComment = new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Parent Comment" };
            var childComment = new PostComment { PostcommentId = 2, PostId = 1, AccountId = 1, Content = "Child Comment", ParentCommentId = 1 };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.AddRange(parentComment, childComment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.DeleteCommentAsync(1);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.PostComments.ToListAsync());
        }

        [Fact]
        public async Task DeleteCommentAsync_NonExistingComment_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = await _postRepository.DeleteCommentAsync(999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region LikePostAsync
        [Fact]
        public async Task LikePostAsync_ValidPostAndAccount_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.LikePostAsync(1, 1);

            // Assert
            Assert.True(result);
            var like = await _context.PostLikes.FirstOrDefaultAsync(pl => pl.PostId == 1 && pl.AccountId == 1);
            Assert.NotNull(like);
        }

        [Fact]
        public async Task LikePostAsync_AlreadyLiked_ReturnsFalse()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var like = new PostLike { PostLikeId = 1, PostId = 1, AccountId = 1 };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.LikePostAsync(1, 1);

            // Assert
            Assert.False(result);
            Assert.Single(await _context.PostLikes.ToListAsync());
        }
        #endregion

        #region UnlikePostAsync
        [Fact]
        public async Task UnlikePostAsync_ValidLike_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var like = new PostLike { PostLikeId = 1, PostId = 1, AccountId = 1 };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UnlikePostAsync(1, 1);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.PostLikes.ToListAsync());
        }

        [Fact]
        public async Task UnlikePostAsync_NonExistingLike_ReturnsFalse()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.UnlikePostAsync(1, 1);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetPostLikeCountAsync
        [Fact]
        public async Task GetPostLikeCountAsync_ValidPost_ReturnsCorrectCount()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var likes = new List<PostLike>
            {
                new PostLike { PostLikeId = 1, PostId = 1, AccountId = 1 },
                new PostLike { PostLikeId = 2, PostId = 1, AccountId = 2 }
            };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostLikes.AddRange(likes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostLikeCountAsync(1);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetPostLikeCountAsync_NonExistingPost_ReturnsZero()
        {
            // Arrange
            
            // Act
            var result = await _postRepository.GetPostLikeCountAsync(10000);

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region GetPostCommentCountAsync
        [Fact]
        public async Task GetPostCommentCountAsync_ValidPost_ReturnsCorrectCount()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var comments = new List<PostComment>
            {
                new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Comment 1" },
                new PostComment { PostcommentId = 2, PostId = 1, AccountId = 1, Content = "Comment 2" }
            };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.AddRange(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostCommentCountAsync(1);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetPostCommentCountAsync_NonExistingPost_ReturnsZero()
        {
            // Arrange
           
            // Act
            var result = await _postRepository.GetPostCommentCountAsync(10000);

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region GetPostCommentByPostId
        [Fact]
        public async Task GetPostCommentByPostId_ValidPostId_ReturnsPagedComments()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var comments = new List<PostComment>
            {
                new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Comment 1", ParentCommentId = null },
                new PostComment { PostcommentId = 2, PostId = 1, AccountId = 1, Content = "Comment 2", ParentCommentId = null },
                new PostComment { PostcommentId = 3, PostId = 1, AccountId = 1, Content = "Child Comment", ParentCommentId = 1 }
            };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.AddRange(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostCommentByPostId(1, 1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, c => c.Content == "Comment 1");
            Assert.Contains(result.Items, c => c.Content == "Comment 2");
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task GetPostCommentByPostId_NonExistingPostId_ReturnsEmptyList()
        {
            // Arrange

            // Act
            var result = await _postRepository.GetPostCommentByPostId(999, 1, 10);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetPostCommentChildByPostIdAndParentCommentId
        [Fact]
        public async Task GetPostCommentChildByPostIdAndParentCommentId_ValidParentCommentId_ReturnsPagedChildComments()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var comments = new List<PostComment>
                {
                    new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Parent Comment", ParentCommentId = null },
                    new PostComment { PostcommentId = 2, PostId = 1, AccountId = 1, Content = "Child Comment 1", ParentCommentId = 1 },
                    new PostComment { PostcommentId = 3, PostId = 1, AccountId = 1, Content = "Child Comment 2", ParentCommentId = 1 }
                };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.AddRange(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostCommentChildByPostIdAndParentCommentId(1, 2, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, c => c.Content == "Child Comment 1");
            Assert.Contains(result.Items, c => c.Content == "Child Comment 2");
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task GetPostCommentChildByPostIdAndParentCommentId_NonExistingParentCommentId_ReturnsEmptyList()
        {
            // Arrange
            
            // Act
            var result = await _postRepository.GetPostCommentChildByPostIdAndParentCommentId(1, 10, 10000);

            // Assert
            Assert.Equal(0, result.Items.Count);
        }
        #endregion

        #region CountChildCommentByPostCommentId
        [Fact]
        public async Task CountChildCommentByPostCommentId_ValidParentCommentId_ReturnsCorrectCount()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var comments = new List<PostComment>
            {
                new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Parent Comment", ParentCommentId = null },
                new PostComment { PostcommentId = 2, PostId = 1, AccountId = 1, Content = "Child Comment 1", ParentCommentId = 1 },
                new PostComment { PostcommentId = 3, PostId = 1, AccountId = 1, Content = "Child Comment 2", ParentCommentId = 1 }
            };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.AddRange(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.CountChildCommentByPostCommentId(1);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task CountChildCommentByPostCommentId_NonExistingParentCommentId_ReturnsZero()
        {
            // Arrange
            
            // Act
            var result = await _postRepository.CountChildCommentByPostCommentId(1);

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region GetPostsByAccountId
        [Fact]
        public async Task GetPostsByAccountId_ValidAccountId_ReturnsPagedPosts()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "avatar.jpg" } };
            var posts = new List<Post>
            {
                new Post { PostId = 1, AccountId = 1, Title = "Post 1", Content = "Content 1", CreateAt = DateTime.UtcNow },
                new Post { PostId = 2, AccountId = 1, Title = "Post 2", Content = "Content 2", CreateAt = DateTime.UtcNow.AddDays(-1) }
            };
            _context.Accounts.Add(account);
            _context.Posts.AddRange(posts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostsByAccountId(1, 1, 2, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, p => p.Title == "Post 1");
            Assert.Contains(result.Items, p => p.Title == "Post 2");
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task GetPostsByAccountId_NonExistingAccountId_ReturnsEmptyList()
        {
            // Arrange

            // Act
            var result = await _postRepository.GetPostsByAccountId(999, 1, 10, 1);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region HidePostAsync
        [Fact]
        public async Task HidePostAsync_ValidPostAndAccount_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.HidePostAsync(1, 1);

            // Assert
            Assert.True(result);
            var hide = await _context.PostHides.FirstOrDefaultAsync(ph => ph.AccountId == 1 && ph.PostId == 1);
            Assert.NotNull(hide);
        }

        [Fact]
        public async Task HidePostAsync_AlreadyHidden_ReturnsFalse()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var hide = new PostHide { AccountId = 1, PostId = 1, HideAt = DateTime.UtcNow };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostHides.Add(hide);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.HidePostAsync(1, 1);

            // Assert
            Assert.False(result);
            Assert.Single(await _context.PostHides.ToListAsync());
        }
        #endregion

        #region GetAllReportReasonAsync
        [Fact]
        public async Task GetAllReportReasonAsync_ReturnsAllReportReasons()
        {
            // Arrange
            var reasons = new List<ReportReason>
            {
                new ReportReason { ReasonId = 1, Reason = "Spam", Description = "Spammy content" },
                new ReportReason { ReasonId = 2, Reason = "Inappropriate", Description = "Inappropriate content" }
            };
            _context.ReportReasons.AddRange(reasons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetAllReportReasonAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Reason == "Spam");
            Assert.Contains(result, r => r.Reason == "Inappropriate");
        }
        #endregion

        #region GetReportReasonByIdAsync
        [Fact]
        public async Task GetReportReasonByIdAsync_ValidReasonId_ReturnsReportReason()
        {
            // Arrange
            var reportReason = new ReportReason
            {
                ReasonId = 1,
                Reason = "Inappropriate Content",
                Description = "Content violates community guidelines"
            };
            _context.ReportReasons.Add(reportReason);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetReportReasonByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ReasonId);
            Assert.Equal("Inappropriate Content", result.Reason);
            Assert.Equal("Content violates community guidelines", result.Description);
        }

        [Fact]
        public async Task GetReportReasonByIdAsync_NonExistingReasonId_ReturnsNull()
        {
            // Act
            var result = await _postRepository.GetReportReasonByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region CreateReportReasonAsync
        [Fact]
        public async Task CreateReportReasonAsync_ValidReason_AddsSuccessfully()
        {
            // Arrange
            var reason = new ReportReason { Reason = "Spam", Description = "Spammy content" };

            // Act
            var result = await _postRepository.CreateReportReasonAsync(reason);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Spam", result.Reason);
            var savedReason = await _context.ReportReasons.FirstOrDefaultAsync(r => r.Reason == "Spam");
            Assert.NotNull(savedReason);
            Assert.Equal("Spammy content", savedReason.Description);
        }
        #endregion

        #region UpdateReportReasonAsync
        [Fact]
        public async Task UpdateReportReasonAsync_ValidReason_ReturnsTrue()
        {
            // Arrange
            var reason = new ReportReason { ReasonId = 1, Reason = "Spam", Description = "Old Description" };
            _context.ReportReasons.Add(reason);
            await _context.SaveChangesAsync();
            var updatedReason = new ReportReason { ReasonId = 1, Reason = "Inappropriate", Description = "New Description" };

            // Act
            var result = await _postRepository.UpdateReportReasonAsync(updatedReason);

            // Assert
            Assert.True(result);
            var savedReason = await _context.ReportReasons.FindAsync(1);
            Assert.NotNull(savedReason);
            Assert.Equal("Inappropriate", savedReason.Reason);
            Assert.Equal("New Description", savedReason.Description);
        }

        [Fact]
        public async Task UpdateReportReasonAsync_NonExistingReason_ReturnsFalse()
        {
            // Arrange
            var reason = new ReportReason { ReasonId = 999, Reason = "Spam", Description = "Description" };

            // Act
            var result = await _postRepository.UpdateReportReasonAsync(reason);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region DeleteReportReasonAsync
        [Fact]
        public async Task DeleteReportReasonAsync_ValidReasonId_ReturnsTrue()
        {
            // Arrange
            var reason = new ReportReason { ReasonId = 1, Reason = "Spam", Description = "Spammy content" };
            _context.ReportReasons.Add(reason);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.DeleteReportReasonAsync(1);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.ReportReasons.ToListAsync());
        }

        [Fact]
        public async Task DeleteReportReasonAsync_NonExistingReasonId_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = await _postRepository.DeleteReportReasonAsync(999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region CreatePostReportAsync
        [Fact]
        public async Task CreatePostReportAsync_ValidReport_AddsSuccessfully()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var reason = new ReportReason { ReasonId = 1, Reason = "Spam" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.ReportReasons.Add(reason);
            await _context.SaveChangesAsync();
            var report = new PostReport { AccountId = 1, PostId = 1, ReasonId = 1, Status = "Pending", CreateAt = DateTime.UtcNow };

            // Act
            var result = await _postRepository.CreatePostReportAsync(report);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AccountId);
            Assert.Equal(1, result.PostId);
            Assert.Equal(1, result.ReasonId);
            Assert.Equal("Pending", result.Status);
            var savedReport = await _context.PostReports.FirstOrDefaultAsync(r => r.PostId == 1);
            Assert.NotNull(savedReport);
        }
        #endregion

        #region GetPostReportByIdAsync
        [Fact]
        public async Task GetPostReportByIdAsync_ValidId_ReturnsReport()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Test Content" };
            var reason = new ReportReason { ReasonId = 1, Reason = "Spam" };
            var report = new PostReport { ReportId = 1, AccountId = 1, PostId = 1, ReasonId = 1, Status = "Pending" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.ReportReasons.Add(reason);
            _context.PostReports.Add(report);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostReportByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ReportId);
            Assert.Equal(1, result.AccountId);
            Assert.Equal(1, result.PostId);
            Assert.Equal(1, result.ReasonId);
            Assert.Equal("Pending", result.Status);
            Assert.Equal("Spam", result.Reason.Reason);
        }

        [Fact]
        public async Task GetPostReportByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _postRepository.GetPostReportByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region AddInternshipPostAsync
        [Fact]
        public async Task AddInternshipPostAsync_ValidPost_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internshipPost = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Description = "Internship Description",
                Status = StatusInternshipPost.ACTIVE
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            await _context.SaveChangesAsync();

            // Act
            await _postRepository.AddInternshipPostAsync(internshipPost);

            // Assert
            var savedPost = await _context.InternshipPosts.FirstOrDefaultAsync(p => p.InternshipId == 1);
            Assert.NotNull(savedPost);
            Assert.Equal("Internship Description", savedPost.Description);
            Assert.Equal(StatusInternshipPost.ACTIVE, savedPost.Status);
        }
        #endregion

        #region GetInternshipPostByIdAsync
        [Fact]
        public async Task GetInternshipPostByIdAsync_ValidId_ReturnsPost()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internshipPost = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Description = "Internship Description",
                Status = StatusInternshipPost.ACTIVE
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internshipPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetInternshipPostByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.InternshipId);
            Assert.Equal("Internship Description", result.Description);
            Assert.Equal(StatusInternshipPost.ACTIVE, result.Status);
        }

        [Fact]
        public async Task GetInternshipPostByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _postRepository.GetInternshipPostByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region UpdateInternshipPostAsync
        [Fact]
        public async Task UpdateInternshipPostAsync_ValidPost_UpdatesSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internshipPost = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Description = "Old Description",
                Status = StatusInternshipPost.ACTIVE
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internshipPost);
            await _context.SaveChangesAsync();
            var updatedPost = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Description = "New Description",
                Status = StatusInternshipPost.ACTIVE
            };

            // Act
            var result = await _postRepository.UpdateInternshipPostAsync(1, updatedPost);

            // Assert
            Assert.True(result);
            var savedPost = await _context.InternshipPosts.FindAsync(1);
            Assert.NotNull(savedPost);
            Assert.Equal("New Description", savedPost.Description);
        }

        [Fact]
        public async Task UpdateInternshipPostAsync_NonExistingPost_ReturnsFalse()
        {
            // Arrange
            var updatedPost = new InternshipPost
            {
                InternshipId = 999,
                StartupId = 1,
                PositionId = 1,
                Description = "New Description"
            };

            // Act
            var result = await _postRepository.UpdateInternshipPostAsync(999, updatedPost);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetTopInternshipPostsByCVCountAsync
        [Fact]
        public async Task GetTopInternshipPostsByCVCountAsync_ValidData_ReturnsTopPosts()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup", Logo = "logo.jpg" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internshipPost = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Description = "Internship Description",
                Requirement = "Requirement",
                Benefits = "Benefits",
                Address = "Address",
                Salary = "1000",
                Deadline = DateTime.UtcNow.AddDays(30),
                Status = StatusInternshipPost.ACTIVE,
                CreateAt = DateTime.UtcNow
            };
            var cvs = new List<CandidateCv>
            {
                new CandidateCv { CandidateCvId = 1, InternshipId = 1, AccountId = 1, Cvurl = "cv1.pdf", Status = "Submitted", CreateAt = DateTime.UtcNow },
                new CandidateCv { CandidateCvId = 2, InternshipId = 1, AccountId = 2, Cvurl = "cv2.pdf", Status = "Submitted", CreateAt = DateTime.UtcNow }
            };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            _context.Accounts.Add(account);
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internshipPost);
            _context.CandidateCvs.AddRange(cvs);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetTopInternshipPostsByCVCountAsync(5);

            // Assert
            Assert.Single(result);
            var topPost = result[0];
            Assert.Equal(1, topPost.InternshipId);
            Assert.Equal("Internship Description", topPost.Description);
            Assert.Equal("Requirement", topPost.Requirement);
            Assert.Equal("Benefits", topPost.Benefits);
            Assert.Equal("Address", topPost.Address);
            Assert.Equal("1000", topPost.Salary);
            Assert.Equal(2, topPost.TotalCVs);
            Assert.Equal("Test Startup", topPost.StartupName);
            Assert.Equal("logo.jpg", topPost.Logo);
            Assert.Equal("Developer", topPost.PositionTitle);
            Assert.Equal(StatusInternshipPost.ACTIVE, topPost.Status);
        }
        #endregion

        #region GetSearchPosts
        [Fact]
        public async Task GetSearchPosts_ValidKeyword_ReturnsMatchingPosts()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Content with keyword" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = _postRepository.GetSearchPosts("keyword", 1).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Post", result[0].Title);
        }

        [Fact]
        public async Task GetSearchPosts_NullKeyword_ReturnsAllPosts()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Content with keyword" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = _postRepository.GetSearchPosts(null, 1).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Post", result[0].Title);
        }

        [Fact]
        public async Task GetSearchPosts_EmptyKeyword_ReturnsAllPosts()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Content with keyword" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = _postRepository.GetSearchPosts("", 1).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Post", result[0].Title);
        }

        [Fact]
        public async Task GetSearchPosts_HiddenPost_ExcludesHidden()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Content with keyword" };
            var hide = new PostHide { AccountId = 1, PostId = 1, HideAt = DateTime.UtcNow };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostHides.Add(hide);
            await _context.SaveChangesAsync();

            // Act
            var result = _postRepository.GetSearchPosts("keyword", 1).ToList();

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetRecommendedFeedAsync
        [Fact]
        public async Task GetRecommendedFeedAsync_ValidUserId_ReturnsFeedItems()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Content", CreateAt = DateTime.UtcNow };
            var follow = new Follow { FollowerAccountId = 2, FollowingAccountId = 1 };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetRecommendedFeedAsync(2, 1, 10);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Post", result[0].Title);
            Assert.Equal(2, result[0].Priority); // Priority for followed account
        }

        [Fact]
        public async Task GetRecommendedFeedAsync_HiddenPost_ExcludesHidden()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post", Content = "Content", CreateAt = DateTime.UtcNow };
            var hide = new PostHide { AccountId = 2, PostId = 1, HideAt = DateTime.UtcNow };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostHides.Add(hide);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetRecommendedFeedAsync(2, 1, 10);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region IsPostLikedAsync
        [Fact]
        public async Task IsPostLikedAsync_AlreadyLiked_ReturnsTrue()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post" };
            var like = new PostLike { PostLikeId = 1, PostId = 1, AccountId = 1 };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.IsPostLikedAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsPostLikedAsync_NotLiked_ReturnsFalse()
        {
            // Arrange
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.IsPostLikedAsync(1, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsPostLikedAsync_NonExistingPostOrAccount_ReturnsFalse()
        {
            // Act
            var result = await _postRepository.IsPostLikedAsync(999, 999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetSearchPostsByStartup
        [Fact]
        public async Task GetSearchPostsByStartup_ValidStartupId_ReturnsPosts()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup", Logo = "logo.jpg" };
            var posts = new List<Post>
            {
                new Post { PostId = 1, StartupId = 1, Title = "Post 1", Content = "Content 1", CreateAt = DateTime.UtcNow },
                new Post { PostId = 2, StartupId = 1, Title = "Post 2", Content = "Content 2", CreateAt = DateTime.UtcNow.AddDays(-1) }
            };
            _context.Startups.Add(startup);
            _context.Posts.AddRange(posts);
            await _context.SaveChangesAsync();

            // Act
            var result = _postRepository.GetSearchPostsByStartup(1).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Title == "Post 1");
            Assert.Contains(result, p => p.Title == "Post 2");
            Assert.All(result, p => Assert.Equal(1, p.StartupId));
        }

        [Fact]
        public async Task GetSearchPostsByStartup_NonExistingStartupId_ReturnsEmpty()
        {
            // Act
            var result = _postRepository.GetSearchPostsByStartup(999).ToList();

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetStartupInternshipPost
        [Fact]
        public async Task GetStartupInternshipPost_ValidStartupId_ReturnsInternshipPosts()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internships = new List<InternshipPost>
            {
                new InternshipPost { InternshipId = 1, StartupId = 1, PositionId = 1, Description = "Internship 1", Status = StatusInternshipPost.ACTIVE },
                new InternshipPost { InternshipId = 2, StartupId = 1, PositionId = 1, Description = "Internship 2", Status = StatusInternshipPost.ACTIVE }
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.AddRange(internships);
            await _context.SaveChangesAsync();

            // Act
            var result = _postRepository.GetStartupInternshipPost(1).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, i => i.Description == "Internship 1");
            Assert.Contains(result, i => i.Description == "Internship 2");
        }

        [Fact]
        public async Task GetStartupInternshipPost_NonExistingStartupId_ReturnsEmpty()
        {
            // Act
            var result = _postRepository.GetStartupInternshipPost(999).ToList();

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetAccountIdByPostIDAsync
        [Fact]
        public async Task GetAccountIdByPostIDAsync_ValidPostId_ReturnsAccountId()
        {
            // Arrange
            var account = new Account { AccountId = 1 };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Test Post" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetAccountIdByPostIDAsync(1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetAccountIdByPostIDAsync_NonExistingPostId_ReturnsNull()
        {
            // Act
            var result = await _postRepository.GetAccountIdByPostIDAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAccountIdByCommentId
        [Fact]
        public async Task GetAccountIdByCommentId_ValidCommentId_ReturnsAccountId()
        {
            // Arrange
            var account = new Account { AccountId = 1 };
            var post = new Post { PostId = 1, AccountId = 1 };
            var comment = new PostComment { PostcommentId = 1, PostId = 1, AccountId = 1, Content = "Test Comment" };
            _context.Accounts.Add(account);
            _context.Posts.Add(post);
            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetAccountIdByCommentId(1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetAccountIdByCommentId_NonExistingCommentId_ReturnsNull()
        {
            // Act
            var result = await _postRepository.GetAccountIdByCommentId(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetStartupFeedItemsAsync
        [Fact]
        public async Task GetStartupFeedItemsAsync_ValidStartupId_ReturnsFeedItems()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup", Logo = "logo.jpg" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var post = new Post { PostId = 1, StartupId = 1, Title = "Test Post", Content = "Content", CreateAt = DateTime.UtcNow };
            var internship = new InternshipPost { InternshipId = 1, StartupId = 1, PositionId = 1, Description = "Internship", Status = StatusInternshipPost.ACTIVE, CreateAt = DateTime.UtcNow };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.Posts.Add(post);
            _context.InternshipPosts.Add(internship);
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _postRepository.GetStartupFeedItemsAsync(1, 0, 10);

            // Assert
            Assert.Equal(2, items.Count);
            Assert.Equal(2, totalCount);
            Assert.Contains(items, i => i.Type == "Post" && i.Title == "Test Post");
            Assert.Contains(items, i => i.Type == "Internship" && i.Title == "Developer");
            _context.StartupClicks.AnyAsync(c => c.StartupId == 1);
        }

        [Fact]
        public async Task GetStartupFeedItemsAsync_NonExistingStartupId_ReturnsEmpty()
        {
            // Act
            var (items, totalCount) = await _postRepository.GetStartupFeedItemsAsync(999, 0, 10);

            // Assert
            Assert.Empty(items);
            Assert.Equal(0, totalCount);
        }

        [Fact]
        public async Task GetStartupFeedItemsAsync_InvalidSkipTake_ReturnsCorrectPagedItems()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var post = new Post { PostId = 1, StartupId = 1, Title = "Test Post", CreateAt = DateTime.UtcNow };
            _context.Startups.Add(startup);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _postRepository.GetStartupFeedItemsAsync(1, 1, 1);

            // Assert
            Assert.Empty(items);
            Assert.Equal(1, totalCount);
        }

        [Fact]
        public async Task GetStartupFeedItemsAsync_OnlyPosts_ReturnsOnlyPostItems()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var post = new Post { PostId = 1, StartupId = 1, Title = "Test Post", CreateAt = DateTime.UtcNow };
            _context.Startups.Add(startup);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _postRepository.GetStartupFeedItemsAsync(1, 0, 10);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, totalCount);
            Assert.Contains(items, i => i.Type == "Post" && i.Title == "Test Post");
        }

        [Fact]
        public async Task GetStartupFeedItemsAsync_OnlyInternships_ReturnsOnlyInternshipItems()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internship = new InternshipPost { InternshipId = 1, StartupId = 1, PositionId = 1, Description = "Internship", Status = StatusInternshipPost.ACTIVE, CreateAt = DateTime.UtcNow };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internship);
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _postRepository.GetStartupFeedItemsAsync(1, 0, 10);

            // Assert
            Assert.Single(items);
            Assert.Equal(1, totalCount);
            Assert.Contains(items, i => i.Type == "Internship" && i.Title == "Developer");
        }

        [Fact]
        public async Task GetStartupFeedItemsAsync_ZeroTake_ReturnsEmpty()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var post = new Post { PostId = 1, StartupId = 1, Title = "Test Post", CreateAt = DateTime.UtcNow };
            _context.Startups.Add(startup);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _postRepository.GetStartupFeedItemsAsync(1, 0, 0);

            // Assert
            Assert.Empty(items);
            Assert.Equal(1, totalCount); 
        }
        #endregion

        #region ShareAsync
        [Fact]
        public async Task ShareAsync_ValidPost_AddsSuccessfully()
        {
            // Arrange
            var account = new Account { AccountId = 1 };
            var post = new Post { PostId = 1, AccountId = 1, Title = "Shared Post", Content = "Content" };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            await _postRepository.ShareAsync(post);

            // Assert
            var savedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Title == "Shared Post");
            Assert.NotNull(savedPost);
            Assert.Equal("Content", savedPost.Content);
        }
        #endregion

        #region GetPostLikeByPostId
        [Fact]
        public async Task GetPostLikeByPostId_ValidPostId_ReturnsPagedLikes()
        {
            // Arrange
            var account1 = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var account2 = new Account { AccountId = 2, AccountProfile = new AccountProfile { FirstName = "Jane", LastName = "Doe" } };
            var post = new Post { PostId = 1, AccountId = 1 };
            var likes = new List<PostLike>
            {
                new PostLike { PostLikeId = 1, PostId = 1, AccountId = 1 },
                new PostLike { PostLikeId = 2, PostId = 1, AccountId = 2 }
            };
            _context.Accounts.AddRange(account1, account2);
            _context.Posts.Add(post);
            _context.PostLikes.AddRange(likes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostLikeByPostId(1, 1, 2);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, l => l.AccountId == 1);
            Assert.Contains(result.Items, l => l.AccountId == 2);
        }

        [Fact]
        public async Task GetPostLikeByPostId_NonExistingPostId_ReturnsNull()
        {
            // Act
            var result = await _postRepository.GetPostLikeByPostId(999, 1, 10);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetPostsByStartupId
        [Fact]
        public async Task GetPostsByStartupId_ValidStartupId_ReturnsPagedPosts()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var posts = new List<Post>
            {
                new Post { PostId = 1, StartupId = 1, AccountId = 1, Title = "Post 1", CreateAt = DateTime.UtcNow },
                new Post { PostId = 2, StartupId = 1, AccountId = 1, Title = "Post 2", CreateAt = DateTime.UtcNow.AddDays(-1) }
            };
            _context.Startups.Add(startup);
            _context.Accounts.Add(account);
            _context.Posts.AddRange(posts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostsByStartupId(1, 1, 2);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, p => p.Title == "Post 1");
            Assert.Contains(result.Items, p => p.Title == "Post 2");
        }

        [Fact]
        public async Task GetPostsByStartupId_NonExistingStartupId_ReturnsEmpty()
        {
            // Act
            var result = await _postRepository.GetPostsByStartupId(999, 1, 10);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
        #endregion

        #region GetStartupByIdAsync
        [Fact]
        public async Task GetStartupByIdAsync_ValidStartupId_ReturnsStartup()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup", Stage = new StartupStage { StageId = 1, StageName = "Seed" } };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetStartupByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Startup", result.StartupName);
            Assert.Equal("Seed", result.Stage.StageName);
        }

        [Fact]
        public async Task GetStartupByIdAsync_NonExistingStartupId_ReturnsNull()
        {
            // Act
            var result = await _postRepository.GetStartupByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetInternshipPostsAsync
        [Fact]
        public async Task GetInternshipPostsAsync_ValidStartupId_ReturnsPagedInternshipPosts()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internships = new List<InternshipPost>
            {
                new InternshipPost { InternshipId = 1, StartupId = 1, PositionId = 1, Description = "Internship 1", CreateAt = DateTime.UtcNow },
                new InternshipPost { InternshipId = 2, StartupId = 1, PositionId = 1, Description = "Internship 2", CreateAt = DateTime.UtcNow.AddDays(-1) }
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.AddRange(internships);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetInternshipPostsAsync(1, 2, 1);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, i => i.Description == "Internship 1");
            Assert.Contains(result.Items, i => i.Description == "Internship 2");
        }

        [Fact]
        public async Task GetInternshipPostsAsync_NonExistingStartupId_ReturnsEmpty()
        {
            // Act
            var result = await _postRepository.GetInternshipPostsAsync(1, 10, 999);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
        #endregion

        #region GetInternshipPostWithNavigationAsync
        [Fact]
        public async Task GetInternshipPostWithNavigationAsync_ValidId_ReturnsPostWithNavigation()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement { PositionId = 1, Title = "Developer" };
            var internship = new InternshipPost { InternshipId = 1, StartupId = 1, PositionId = 1, Description = "Internship" };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetInternshipPostWithNavigationAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Internship", result.Description);
            Assert.Equal("Test Startup", result.Startup.StartupName);
            Assert.Equal("Developer", result.Position.Title);
        }

        [Fact]
        public async Task GetInternshipPostWithNavigationAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _postRepository.GetInternshipPostWithNavigationAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion


        private IFormFile CreateMockFormFile(string fileName)
        {
            var content = "Mock file content";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.FileName).Returns(fileName);
            formFile.Setup(f => f.Length).Returns(stream.Length);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);
            formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            return formFile.Object;
        }
    }
}