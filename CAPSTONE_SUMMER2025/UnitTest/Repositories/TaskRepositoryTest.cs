using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO.TaskDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTest.Repositories
{
    public class TaskRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly ITaskRepository _taskRepository;

        public TaskRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _taskRepository = new TaskRepository(_context);
        }

        #region AddMilestoneAssignmentAsync
        [Fact]
        public async Task AddMilestoneAssignmentAsync_ValidAssignment_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var member = new StartupMember { StartupMemberId = 1, StartupId = 1, AccountId = 1 };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.Accounts.Add(account);
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            var assignment = new MilestoneAssignment { MilestoneId = 1, MemberId = 1 };

            // Act
            await _taskRepository.AddMilestoneAssignmentAsync(assignment);

            // Assert
            var savedAssignment = await _context.MilestoneAssignments.FirstOrDefaultAsync(a => a.MilestoneId == 1 && a.MemberId == 1);
            Assert.NotNull(savedAssignment);
            Assert.Equal(1, savedAssignment.MilestoneId);
            Assert.Equal(1, savedAssignment.MemberId);
        }
        #endregion

        #region AddMilestoneAsync
        [Fact]
        public async Task AddMilestoneAsync_ValidMilestone_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            _context.Startups.Add(startup);
            await _context.SaveChangesAsync();

            var milestone = new Milestone
            {
                StartupId = 1,
                Name = "Test Milestone",
                Description = "Milestone description",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };

            // Act
            var result = await _taskRepository.AddMilestoneAsync(milestone);

            // Assert
            var savedMilestone = await _context.Milestones.FirstOrDefaultAsync(m => m.MilestoneId == result.MilestoneId);
            Assert.NotNull(savedMilestone);
            Assert.Equal("Test Milestone", savedMilestone.Name);
            Assert.Equal(1, savedMilestone.StartupId);
        }
        #endregion

        #region AssignmentExistsAsync
        [Fact]
        public async Task AssignmentExistsAsync_ExistingAssignment_ReturnsTrue()
        {
            // Arrange
            var assignment = new MilestoneAssignment { MilestoneId = 1, MemberId = 1 };
            _context.MilestoneAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.AssignmentExistsAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AssignmentExistsAsync_NonExistingAssignment_ReturnsFalse()
        {
            // Act
            var result = await _taskRepository.AssignmentExistsAsync(999, 999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region AddColumnStatusAsync
        [Fact]
        public async Task AddColumnStatusAsync_ValidColumn_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();

            var column = new ColumnnStatus
            {
                MilestoneId = 1,
                ColumnName = "To Do",
                SortOrder = 1,
                Description = "Tasks to be done"
            };

            // Act
            await _taskRepository.AddColumnStatusAsync(column);

            // Assert
            var savedColumn = await _context.ColumnnStatuses.FirstOrDefaultAsync(c => c.MilestoneId == 1);
            Assert.NotNull(savedColumn);
            Assert.Equal("To Do", savedColumn.ColumnName);
            Assert.Equal(1, savedColumn.SortOrder);
        }
        #endregion

        #region GetMaxSortOrderAsync
        [Fact]
        public async Task GetMaxSortOrderAsync_ColumnsExist_ReturnsMaxSortOrder()
        {
            // Arrange
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1 };
            var columns = new List<ColumnnStatus>
            {
                new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "To Do", SortOrder = 1 },
                new ColumnnStatus { ColumnnStatusId = 2, MilestoneId = 1, ColumnName = "In Progress", SortOrder = 2 }
            };
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.AddRange(columns);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetMaxSortOrderAsync(1);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_NoColumns_ReturnsZero()
        {
            // Act
            var result = await _taskRepository.GetMaxSortOrderAsync(999);

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region GetColumnsByMilestoneIdAsync
        [Fact]
        public async Task GetColumnsByMilestoneIdAsync_ValidMilestoneId_ReturnsColumns()
        {
            // Arrange
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1 };
            var columns = new List<ColumnnStatus>
            {
                new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "To Do", SortOrder = 1 },
                new ColumnnStatus { ColumnnStatusId = 2, MilestoneId = 1, ColumnName = "In Progress", SortOrder = 2 }
            };
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.AddRange(columns);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetColumnsByMilestoneIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("To Do", result.First().ColumnName);
            Assert.Equal(1, result.First().SortOrder);
        }

        [Fact]
        public async Task GetColumnsByIdAsync_NonMilestoneId_ReturnsEmptyList()
        {
            // Act
            var result = await _taskRepository.GetColumnsByMilestoneIdAsync(999);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region AddStartupTaskAsync
        [Fact]
        public async Task AddStartupTaskAsync_ValidTask_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone
            {
                MilestoneId = 1,
                StartupId = 1,
                Name = "Test Milestone",
                Status = "Active"
            };
            var columnStatus = new ColumnnStatus
            {
                ColumnnStatusId = 1,
                MilestoneId = 1,
                ColumnName = "To Do",
                SortOrder = 1
            };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.Add(columnStatus);
            await _context.SaveChangesAsync();

            var task = new StartupTask
            {
                MilestoneId = 1,
                Title = "Test Task",
                Priority = "High",
                Description = "Task description",
                Duedate = DateTime.UtcNow.AddDays(7),
                Progress = 0,
                ColumnnStatusId = 1,
                Note = "Test note"
            };

            // Act
            var result = await _taskRepository.AddStartupTaskAsync(task);

            // Assert
            var savedTask = await _context.StartupTasks.FirstOrDefaultAsync(t => t.TaskId == result.TaskId);
            Assert.NotNull(savedTask);
            Assert.Equal("Test Task", savedTask.Title);
            Assert.Equal("High", savedTask.Priority);
            Assert.Equal(1, savedTask.MilestoneId);
            Assert.Equal(1, savedTask.ColumnnStatusId);
        }
        #endregion

        #region AddTaskAssignmentsAsync
        [Fact]
        public async Task AddTaskAssignmentsAsync_ValidAssignments_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            var task = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Test Task" };
            var account1 = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var account2 = new Account { AccountId = 2, AccountProfile = new AccountProfile { FirstName = "Jane", LastName = "Doe" } };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.StartupTasks.Add(task);
            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            var assignments = new List<TaskAssignment>
            {
                new TaskAssignment { TaskId = 1, AssignedByAccountId = 1, AssignToAccountId = 1, AssignAt = DateTime.UtcNow },
                new TaskAssignment { TaskId = 1, AssignedByAccountId = 1, AssignToAccountId = 2, AssignAt = DateTime.UtcNow }
            };

            // Act
            await _taskRepository.AddTaskAssignmentsAsync(assignments);

            // Assert
            var savedAssignments = await _context.TaskAssignments.Where(a => a.TaskId == 1).ToListAsync();
            Assert.Equal(2, savedAssignments.Count);
            Assert.Contains(savedAssignments, a => a.AssignToAccountId == 1);
            Assert.Contains(savedAssignments, a => a.AssignToAccountId == 2);
        }
        #endregion

        #region GetTasksByMilestoneAsync
        [Fact]
        public async Task GetTasksByMilestoneAsync_ValidMilestoneId_ReturnsTasks()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            var task1 = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Task 1" };
            var task2 = new StartupTask { TaskId = 2, MilestoneId = 1, Title = "Task 2" };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.StartupTasks.AddRange(task1, task2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetTasksByMilestoneAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Title == "Task 1");
            Assert.Contains(result, t => t.Title == "Task 2");
        }

        [Fact]
        public async Task GetTasksByMilestoneAsync_NonExistentMilestone_ReturnsEmptyList()
        {
            // Act
            var result = await _taskRepository.GetTasksByMilestoneAsync(999);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetTaskslable
        [Fact]
        public async Task GetTaskslable_ValidTaskIds_ReturnsLabels()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var label = new Label { LabelId = 1, LabelName = "Bug", Color = "Red" };
            var taskLabel = new StartupTaskLabel { TaskId = 1, LabelId = 1 };
            _context.StartupTasks.Add(task);
            _context.Labels.Add(label);
            _context.StartupTaskLabels.Add(taskLabel);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetTaskslable(new List<int> { 1 });

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].taskid);
            Assert.Equal("Bug", result[0].LabelName);
            Assert.Equal("Red", result[0].Color);
        }

        [Fact]
        public async Task GetTaskslable_EmptyTaskIds_ReturnsEmptyList()
        {
            // Act
            var result = await _taskRepository.GetTaskslable(new List<int>());

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetAllMilestonesWithMembersAsync
        [Fact]
        public async Task GetAllMilestonesWithMembersAsync_ValidStartupId_ReturnsMilestones()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var member = new StartupMember { StartupMemberId = 1, StartupId = 1, AccountId = 1 };
            var assignment = new MilestoneAssignment { MilestoneId = 1, MemberId = 1 };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.Accounts.Add(account);
            _context.StartupMembers.Add(member);
            _context.MilestoneAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetAllMilestonesWithMembersAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Milestone", result[0].Name);
            Assert.Single(result[0].MilestoneAssignments);
        }
        #endregion

        #region UpdateTaskColumnAsync
        [Fact]
        public async Task UpdateTaskColumnAsync_ValidTask_UpdatesSuccessfully()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1, ColumnnStatusId = 1 };
            var columnStatus = new ColumnnStatus { ColumnnStatusId = 2, MilestoneId = 1, ColumnName = "In Progress" };
            _context.StartupTasks.Add(task);
            _context.ColumnnStatuses.Add(columnStatus);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.UpdateTaskColumnAsync(1, 2);

            // Assert
            Assert.True(result);
            var updatedTask = await _context.StartupTasks.FindAsync(1);
            Assert.Equal(2, updatedTask.ColumnnStatusId);
        }

        [Fact]
        public async Task UpdateTaskColumnAsync_NonExistentTask_ReturnsFalse()
        {
            // Act
            var result = await _taskRepository.UpdateTaskColumnAsync(999, 1);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region AssignLabelToTaskAsync
        [Fact]
        public async Task AssignLabelToTaskAsync_NewLabel_AssignsSuccessfully()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var label = new Label { LabelId = 1, LabelName = "Bug" };
            _context.StartupTasks.Add(task);
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.AssignLabelToTaskAsync(1, 1);

            // Assert
            Assert.True(result);
            var savedLabel = await _context.StartupTaskLabels.FirstOrDefaultAsync(tl => tl.TaskId == 1 && tl.LabelId == 1);
            Assert.NotNull(savedLabel);
        }

        [Fact]
        public async Task AssignLabelToTaskAsync_ExistingLabel_ReturnsFalse()
        {
            // Arrange
            var taskLabel = new StartupTaskLabel { TaskId = 1, LabelId = 1 };
            _context.StartupTaskLabels.Add(taskLabel);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.AssignLabelToTaskAsync(1, 1);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region UpdateTaskAsync
        [Fact]
        public async Task UpdateTaskAsync_ValidDto_UpdatesSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            var columnStatus = new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "To Do", SortOrder = 1 };
            var task = new StartupTask
            {
                TaskId = 1,
                MilestoneId = 1,
                Title = "Original Task",
                Priority = "Medium",
                Description = "Original description",
                Duedate = DateTime.UtcNow.AddDays(7),
                Progress = 0,
                ColumnnStatusId = 1,
                Note = "Original note"
            };
            var label = new Label { LabelId = 1, LabelName = "Bug" };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.Add(columnStatus);
            _context.StartupTasks.Add(task);
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateTaskDto
            {
                TaskId = 1,
                Title = "Updated Task",
                Priority = "High",
                Description = "Updated description",
                DueDate = DateTime.UtcNow.AddDays(10),
                Progress = 50,
                ColumnnStatusId = 1,
                Note = "Updated note",
                AccountId = 1,
                labelcolorID = 1
            };

            // Act
            var result = await _taskRepository.UpdateTaskAsync(updateDto);

            // Assert
            Assert.True(result);
            var updatedTask = await _context.StartupTasks.FindAsync(1);
            Assert.NotNull(updatedTask);
            Assert.Equal("Updated Task", updatedTask.Title);
            var savedLabel = await _context.StartupTaskLabels.FirstOrDefaultAsync(tl => tl.TaskId == 1 && tl.LabelId == 1);
            Assert.NotNull(savedLabel);
        }

        [Fact]
        public async Task UpdateTaskAsync_NonExistentTask_ReturnsFalse()
        {
            // Arrange
            var updateDto = new UpdateTaskDto { TaskId = 999, Title = "Updated Task", AccountId = 1 };

            // Act
            var result = await _taskRepository.UpdateTaskAsync(updateDto);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region AddCommentAsync
        [Fact]
        public async Task AddCommentAsync_ValidComment_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            var task = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Test Task" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.StartupTasks.Add(task);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var commentDto = new CreateCommentTaskDto
            {
                TaskId = 1,
                AccountId = 1,
                Comment = "Test comment"
            };

            // Act
            var result = await _taskRepository.AddCommentAsync(commentDto);

            // Assert
            Assert.True(result);
            var savedComment = await _context.CommentTasks.FirstOrDefaultAsync(c => c.TaskId == 1);
            Assert.NotNull(savedComment);
            Assert.Equal("Test comment", savedComment.Comment);
            Assert.Equal(1, savedComment.AccountId);
        }
        #endregion

        #region GetAccountIdsByTaskIdAsync
        [Fact]
        public async Task GetAccountIdsByTaskIdAsync_ValidTaskId_ReturnsAccountIds()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var assignments = new List<TaskAssignment>
            {
                new TaskAssignment { TaskId = 1, AssignToAccountId = 1 },
                new TaskAssignment { TaskId = 1, AssignToAccountId = 2 }
            };
            _context.StartupTasks.Add(task);
            _context.TaskAssignments.AddRange(assignments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetAccountIdsByTaskIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
        }

        [Fact]
        public async Task GetAccountIdsByTaskIdAsync_NonExistentTask_ReturnsEmptyList()
        {
            // Act
            var result = await _taskRepository.GetAccountIdsByTaskIdAsync(999);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region AddTaskAssignmentAsync
        [Fact]
        public async Task AddTaskAssignmentAsync_ValidAssignment_AddsSuccessfully()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            _context.StartupTasks.Add(task);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var assignment = new TaskAssignment
            {
                TaskId = 1,
                AssignedByAccountId = 1,
                AssignToAccountId = 1,
                AssignAt = DateTime.UtcNow
            };

            // Act
            var result = await _taskRepository.AddTaskAssignmentAsync(assignment);

            // Assert
            Assert.True(result);
            var savedAssignment = await _context.TaskAssignments.FirstOrDefaultAsync(a => a.TaskId == 1);
            Assert.NotNull(savedAssignment);
            Assert.Equal(1, savedAssignment.AssignToAccountId);
        }
        #endregion

        #region TaskAssignmentExistsAsync
        [Fact]
        public async Task TaskAssignmentExistsAsync_ExistingAssignment_ReturnsTrue()
        {
            // Arrange
            var assignment = new TaskAssignment { TaskId = 1, AssignToAccountId = 1 };
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.TaskAssignmentExistsAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task TaskAssignmentExistsAsync_NonExistingAssignment_ReturnsFalse()
        {
            // Act
            var result = await _taskRepository.TaskAssignmentExistsAsync(999, 999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetTaskByMilestoneIdPagedAsync (Overload 1)
        [Fact]
        public async Task GetTaskByMilestoneIdPagedAsync_ValidParameters_ReturnsPagedTasks()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone" };
            var columnStatus = new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "To Do" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "avatar.jpg" } };
            var task = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Test Task", ColumnnStatusId = 1 };
            var assignment = new TaskAssignment { TaskId = 1, AssignedByAccountId = 1, AssignToAccountId = 1 };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.Add(columnStatus);
            _context.Accounts.Add(account);
            _context.StartupTasks.Add(task);
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetTaskByMilestoneIdPagedAsync(1, 1, 10);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Test Task", result.Items[0].Title);
            Assert.Equal("To Do", result.Items[0].ColumnStatus);
            Assert.Single(result.Items[0].AsignTo);
            Assert.Equal("John Doe", result.Items[0].AsignTo[0].Fullname);
        }
        #endregion

        #region GetTaskByMilestoneIdPagedAsync (Overload 2)
        [Fact]
        public async Task GetTaskByMilestoneIdPagedAsync_WithSearchAndColumn_ReturnsFilteredTasks()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone" };
            var columnStatus1 = new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "To Do" };
            var columnStatus2 = new ColumnnStatus { ColumnnStatusId = 2, MilestoneId = 1, ColumnName = "Done" };
            var task1 = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Test Task", Description = "Searchable", ColumnnStatusId = 1 };
            var task2 = new StartupTask { TaskId = 2, MilestoneId = 1, Title = "Other Task", ColumnnStatusId = 2 };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.AddRange(columnStatus1, columnStatus2);
            _context.StartupTasks.AddRange(task1, task2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetTaskByMilestoneIdPagedAsync(1, 1, 10, "Searchable", 1);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Test Task", result.Items[0].Title);
        }
        #endregion

        #region AddTaskAssignAsync
        [Fact]
        public async Task AddTaskAssignAsync_NewAssignment_AddsSuccessfully()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            _context.StartupTasks.Add(task);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var assignment = new TaskAssignment
            {
                TaskId = 1,
                AssignToAccountId = 1,
                AssignedByAccountId = 1,
                AssignAt = DateTime.UtcNow
            };

            // Act
            var result = await _taskRepository.AddTaskAssignAsync(assignment);

            // Assert
            Assert.True(result);
            var savedAssignment = await _context.TaskAssignments.FirstOrDefaultAsync(a => a.TaskId == 1 && a.AssignToAccountId == 1);
            Assert.NotNull(savedAssignment);
        }

        [Fact]
        public async Task AddTaskAssignAsync_ExistingAssignment_ReturnsFalse()
        {
            // Arrange
            var assignment = new TaskAssignment { TaskId = 1, AssignToAccountId = 1 };
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var newAssignment = new TaskAssignment { TaskId = 1, AssignToAccountId = 1 };

            // Act
            var result = await _taskRepository.AddTaskAssignAsync(newAssignment);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region RemoveTaskAssignmentAsync
        [Fact]
        public async Task RemoveTaskAssignmentAsync_ExistingAssignment_RemovesSuccessfully()
        {
            // Arrange
            var assignment = new TaskAssignment { TaskId = 1, AssignToAccountId = 1 };
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.RemoveTaskAssignmentAsync(1, 1);

            // Assert
            Assert.True(result);
            var savedAssignment = await _context.TaskAssignments.FirstOrDefaultAsync(a => a.TaskId == 1 && a.AssignToAccountId == 1);
            Assert.Null(savedAssignment);
        }

        [Fact]
        public async Task RemoveTaskAssignmentAsync_NonExistentAssignment_ReturnsFalse()
        {
            // Act
            var result = await _taskRepository.RemoveTaskAssignmentAsync(999, 999);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetCommentsByTaskIdAsync
        [Fact]
        public async Task GetCommentsByTaskIdAsync_ValidTaskId_ReturnsComments()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "avatar.jpg" } };
            var comment = new CommentTask
            {
                CommentTaskId = 1,
                TaskId = 1,
                AccountId = 1,
                Comment = "Test comment",
                CreateAt = DateTime.UtcNow
            };
            _context.StartupTasks.Add(task);
            _context.Accounts.Add(account);
            _context.CommentTasks.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetCommentsByTaskIdAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test comment", result[0].Comment);
            Assert.Equal("John Doe", result[0].FullName);
            Assert.Equal("avatar.jpg", result[0].AvatarUrl);
        }

        [Fact]
        public async Task GetCommentsByTaskIdAsync_NonExistentTask_ReturnsEmptyList()
        {
            // Act
            var result = await _taskRepository.GetCommentsByTaskIdAsync(999);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetTaskByIdAsync
        [Fact]
        public async Task GetTaskByIdAsync_ValidTaskId_ReturnsTask()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone" };
            var columnStatus = new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "To Do" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var task = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Test Task", ColumnnStatusId = 1 };
            var assignment = new TaskAssignment { TaskId = 1, AssignedByAccountId = 1, AssignToAccountId = 1 };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.Add(columnStatus);
            _context.Accounts.Add(account);
            _context.StartupTasks.Add(task);
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetTaskByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
            Assert.Single(result.TaskAssignments);
        }

        [Fact]
        public async Task GetTaskByIdAsync_NonExistentTask_ReturnsNull()
        {
            // Act
            var result = await _taskRepository.GetTaskByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetMilestoneIDByTaskIDAsync
        [Fact]
        public async Task GetMilestoneIDByTaskIDAsync_ValidTaskId_ReturnsMilestoneId()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            var task = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Test Task" };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.StartupTasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetMilestoneIDByTaskIDAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetMilestoneIDByTaskIDAsync_NonExistingTaskId_ReturnsNull()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetMilestoneIDByTaskIDAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetMembersInMilestoneAsync
        [Fact]
        public async Task GetMembersInMilestoneAsync_ValidMilestoneId_ReturnsMembers()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone" };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "avatar.jpg" } };
            var member = new StartupMember { StartupMemberId = 1, StartupId = 1, AccountId = 1 };
            var assignment = new MilestoneAssignment { MilestoneId = 1, MemberId = 1 };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.Accounts.Add(account);
            _context.StartupMembers.Add(member);
            _context.MilestoneAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetMembersInMilestoneAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("John Doe", result[0].FullName);
            Assert.Equal("avatar.jpg", result[0].AvatarUrl);
        }
        #endregion

        #region GetAssignToByTaskIdAsync
        [Fact]
        public async Task GetAssignToByTaskIdAsync_ValidTaskId_ReturnsAssignment()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe" } };
            var assignment = new TaskAssignment { TaskId = 1, AssignToAccountId = 1 };
            _context.StartupTasks.Add(task);
            _context.Accounts.Add(account);
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetAssignToByTaskIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AssignToAccountId);
        }

        [Fact]
        public async Task GetAssignToByTaskIdAsync_NonExistentTask_ReturnsNull()
        {
            // Act
            var result = await _taskRepository.GetAssignToByTaskIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetMemberIDByAccountIdAsync
        [Fact]
        public async Task GetMemberIDByAccountIdAsync_ValidAccountId_ReturnsMemberId()
        {
            // Arrange
            var member = new StartupMember { StartupMemberId = 1, AccountId = 1, StartupId = 1 };
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetMemberIDByAccountIdAsync(1);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetMemberIDByAccountIdAsync_NonExistentAccount_ReturnsNull()
        {
            // Act
            var result = await _taskRepository.GetMemberIDByAccountIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAllAsync (Labels)
        [Fact]
        public async Task GetAllAsync_ReturnsAllLabels()
        {
            // Arrange
            var labels = new List<Label>
            {
                new Label { LabelId = 1, LabelName = "Bug", Color = "Red" },
                new Label { LabelId = 2, LabelName = "Feature", Color = "Green" }
            };
            _context.Labels.AddRange(labels);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, l => l.LabelName == "Bug");
            Assert.Contains(result, l => l.LabelName == "Feature");
        }
        #endregion

        #region GetAllActivityLogsAsync
        [Fact]
        public async Task GetAllActivityLogsAsync_ValidMilestoneId_ReturnsLogs()
        {
            // Arrange
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1 };
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            var account = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "avatar.jpg" } };
            var log = new TaskActivityLog
            {
                ActivityId = 1,
                TaskId = 1,
                ActionType = "Created",
                AtTime = DateTime.UtcNow,
                ByAccountId = 1,
                Content = "Task created"
            };
            _context.Milestones.Add(milestone);
            _context.StartupTasks.Add(task);
            _context.Accounts.Add(account);
            _context.TaskActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetAllActivityLogsAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("Created", result[0].ActionType);
            Assert.Equal("John Doe", result[0].FullName);
        }
        #endregion

        #region AddActivityLogAsync
        [Fact]
        public async Task AddActivityLogAsync_ValidLog_AddsSuccessfully()
        {
            // Arrange
            var task = new StartupTask { TaskId = 1, MilestoneId = 1 };
            _context.StartupTasks.Add(task);
            await _context.SaveChangesAsync();

            var log = new TaskActivityLog
            {
                TaskId = 1,
                ActionType = "Created",
                AtTime = DateTime.UtcNow,
                ByAccountId = 1,
                Content = "Task created"
            };

            // Act
            await _taskRepository.AddActivityLogAsync(log);

            // Assert
            var savedLog = await _context.TaskActivityLogs.FirstOrDefaultAsync(l => l.TaskId == 1);
            Assert.NotNull(savedLog);
            Assert.Equal("Created", savedLog.ActionType);
        }
        #endregion

        #region GetColumnNameAsync
        [Fact]
        public async Task GetColumnNameAsync_ValidColumnId_ReturnsName()
        {
            // Arrange
            var columnStatus = new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "To Do" };
            _context.ColumnnStatuses.Add(columnStatus);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetColumnNameAsync(1);

            // Assert
            Assert.Equal("To Do", result);
        }

        [Fact]
        public async Task GetColumnNameAsync_NonExistentColumn_ReturnsNull()
        {
            // Act
            var result = await _taskRepository.GetColumnNameAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetFullDashboardAsync
        [Fact]
        public async Task GetFullDashboardAsync_ValidMilestoneId_ReturnsDashboardData()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var milestone = new Milestone { MilestoneId = 1, StartupId = 1, Name = "Test Milestone", Status = "Active" };
            var columnStatus1 = new ColumnnStatus { ColumnnStatusId = 1, MilestoneId = 1, ColumnName = "TODO", SortOrder = 1 };
            var columnStatus2 = new ColumnnStatus { ColumnnStatusId = 2, MilestoneId = 1, ColumnName = "DONE", SortOrder = 2 };
            var account1 = new Account { AccountId = 1, AccountProfile = new AccountProfile { FirstName = "John", LastName = "Doe", AvatarUrl = "john.jpg" } };
            var account2 = new Account { AccountId = 2, AccountProfile = new AccountProfile { FirstName = "Jane", LastName = "Doe", AvatarUrl = "jane.jpg" } };
            var task1 = new StartupTask { TaskId = 1, MilestoneId = 1, Title = "Task 1", ColumnnStatusId = 1, Duedate = now.AddDays(-1) }; 
            var task2 = new StartupTask { TaskId = 2, MilestoneId = 1, Title = "Task 2", ColumnnStatusId = 2 }; 
            var task3 = new StartupTask { TaskId = 3, MilestoneId = 1, Title = "Task 3", ColumnnStatusId = 1 };
            var assignments = new List<TaskAssignment>
            {
                new TaskAssignment { TaskId = 1, AssignToAccountId = 1, AssignedByAccountId = 1 },
                new TaskAssignment { TaskId = 2, AssignToAccountId = 1, AssignedByAccountId = 1 },
                new TaskAssignment { TaskId = 3, AssignToAccountId = 2, AssignedByAccountId = 1 }
            };
            _context.Startups.Add(startup);
            _context.Milestones.Add(milestone);
            _context.ColumnnStatuses.AddRange(columnStatus1, columnStatus2);
            _context.Accounts.AddRange(account1, account2);
            _context.StartupTasks.AddRange(task1, task2, task3);
            _context.TaskAssignments.AddRange(assignments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _taskRepository.GetFullDashboardAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.StatusCounts.Count);
            Assert.Contains(result.StatusCounts, s => s.ColumnStatusId == 1 && s.StatusName == "TODO" && s.Count == 2);
            Assert.Contains(result.StatusCounts, s => s.ColumnStatusId == 2 && s.StatusName == "DONE" && s.Count == 1);
            Assert.Equal(2, result.MemberTaskStats.Count);
            var johnStats = result.MemberTaskStats.First(s => s.AccountName == "John Doe");
            Assert.Equal("john.jpg", johnStats.AccountAvatar);
            Assert.Equal(2, johnStats.TotalTasks);
            Assert.Equal(1, johnStats.CompletedTasks);
            Assert.Equal(1, johnStats.OverdueTasks);
            Assert.Equal(0.5, johnStats.CompletionRate, 2);
            var janeStats = result.MemberTaskStats.First(s => s.AccountName == "Jane Doe");
            Assert.Equal("jane.jpg", janeStats.AccountAvatar);
            Assert.Equal(1, janeStats.TotalTasks);
            Assert.Equal(0, janeStats.CompletedTasks);
            Assert.Equal(0, janeStats.OverdueTasks);
            Assert.Equal(0.0, janeStats.CompletionRate, 2);
        }

        [Fact]
        public async Task GetFullDashboardAsync_NonExistingMilestoneId_ReturnsEmptyLists()
        {
            // Act
            var result = await _taskRepository.GetFullDashboardAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.StatusCounts);
            Assert.Empty(result.MemberTaskStats);
        }
        #endregion
    }
}