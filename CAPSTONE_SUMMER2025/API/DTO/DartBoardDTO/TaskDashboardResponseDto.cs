namespace API.DTO.DartBoardDTO
{
    public class TaskDashboardResponseDto
    {
        public List<TaskStatusCountDto> StatusCounts { get; set; }
        public List<TaskDashboardDto> MemberTaskStats { get; set; }
    }

    public class TaskStatusCountDto
    {
        public int ColumnStatusId { get; set; }
        public string StatusName { get; set; }
        public int Count { get; set; }
    }

    public class TaskDashboardDto
    {
        public string AccountName { get; set; }
        public string AccountAvatar { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate => TotalTasks == 0 ? 0 : (double)CompletedTasks / TotalTasks;
    }
}
