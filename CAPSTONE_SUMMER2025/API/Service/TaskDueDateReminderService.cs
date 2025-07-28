
using API.DTO.NotificationDTO;
using API.Service.Interface;
using API.Utils.Constants;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace API.Service
{
    public class TaskDueDateReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public TaskDueDateReminderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<CAPSTONE_SUMMER2025Context>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var now = DateTime.Now;
                    var tomorrow = now.AddDays(1).Date;
                    var today = now.Date;

                    // Lấy tasks sắp hết hạn trong 24h tới (chưa DONE)
                    var dueSoonTasks = await db.StartupTasks.Include(x => x.TaskAssignments)
                        .Where(t => t.Duedate != null
                            && t.Duedate.Value.Date == tomorrow // chỉ gửi khi còn đúng 1 ngày!
                            && !t.ColumnnStatus.ColumnName.Equals("DONE"))
                        .ToListAsync();

                    foreach (var task in dueSoonTasks)
                    {
                        var assignments = await db.TaskAssignments
                            .Where(a => a.TaskId == task.TaskId)
                            .ToListAsync();
                       var milestoneid=  await db.StartupTasks
                        .Where(x => x.TaskId == task.TaskId)
                        .Select(x => (int?)x.MilestoneId)
                        .FirstOrDefaultAsync();
                        foreach (var assign in assignments)
                        {
                            await notificationService.CreateAndSendAsync(new reqNotificationDTO
                            {
                                UserId = assign.AssignToAccountId.Value,
                                Message = $"Task '{task.Title}' will be due at {task.Duedate:dd/MM/yyyy HH:mm}",
                                CreatedAt = DateTime.Now,
                                IsRead = false,
                                senderid = (int)assign.AssignedByAccountId,
                                NotificationType = NotiConst.Task,
                                TargetURL = $"/me/milestones/{milestoneid}"
                            });
                        }
                        // Nếu muốn: task.IsDueSoonNotified = true;
                    }

                    // Lấy tasks quá hạn (chưa DONE)
                    var overdueTasks = await db.StartupTasks.Include(x => x.TaskAssignments)
                        .Where(t => t.Duedate != null
                            && t.Duedate.Value.Date < today
                            && !t.ColumnnStatus.ColumnName.Equals("DONE"))
                        .ToListAsync();

                    foreach (var task in overdueTasks)
                    {
                        int daysOverdue = (today - task.Duedate.Value.Date).Days;

                        var assignments = await db.TaskAssignments
                            .Where(a => a.TaskId == task.TaskId)
                            .ToListAsync();
                        var milestoneid = await db.StartupTasks
                      .Where(x => x.TaskId == task.TaskId)
                      .Select(x => (int?)x.MilestoneId)
                      .FirstOrDefaultAsync();
                        foreach (var assign in assignments)
                        {
                            await notificationService.CreateAndSendAsync(new reqNotificationDTO
                            {
                                UserId = assign.AssignToAccountId.Value,
                                Message = $"Task '{task.Title}' is overdue by {daysOverdue} day(s)!",
                                CreatedAt = DateTime.Now,
                                IsRead = false,
                                senderid = (int)assign.AssignedByAccountId,
                                NotificationType = NotiConst.Task,
                                TargetURL = $"/me/milestones/{milestoneid}"
                            });
                        }
                        // Nếu muốn: task.IsOverdueNotified = true;
                    }

                    // Nếu đã set flag, nhớ db.SaveChangesAsync()
                }

                // Chạy lại sau 15 giây (test), thực tế nên để 1 giờ/lần
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

    }
}

