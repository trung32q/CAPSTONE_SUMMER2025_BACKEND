
using API.DTO.NotificationDTO;
using API.Service.Interface;
using API.Utils.Constants;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

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
                    var tomorrow = now.AddDays(1);

                    // Lấy các task sắp hết hạn trong 24h tới và chưa gửi nhắc hạn
                    var tasks = await db.StartupTasks.Include
                        (x=>x.TaskAssignments)
                        .Where(t => t.Duedate >= now && t.Duedate < tomorrow /* && !t.IsDueSoonNotified*/)
                        .ToListAsync();

                    foreach (var task in tasks)
                    {
                        var assignments = await db.TaskAssignments
                            .Where(a => a.TaskId == task.TaskId)                     
                            .ToListAsync();

                        foreach (var assign in assignments)
                        {
                            await notificationService.CreateAndSendAsync(new reqNotificationDTO
                            {
                                UserId = assign.AssignToAccountId.Value,
                                Message = $"Task '{task.Title}' sẽ hết hạn vào {task.Duedate:dd/MM/yyyy HH:mm}",
                                CreatedAt = DateTime.Now,
                                IsRead = false,
                                senderid = (int)assign.AssignedByAccountId, // chính là người giao task này cho user này!
                                NotificationType = NotiConst.Task,
                                TargetURL = $"/task/{task.TaskId}"
                            });
                        }

                        // Nếu cần: đánh dấu đã gửi
                        // task.IsDueSoonNotified = true;
                    }
                    // Nếu đã đánh dấu, nhớ db.SaveChangesAsync();
                }
                // Lặp lại mỗi tiếng/lần hoặc 1 ngày/lần tuỳ nhu cầu
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
