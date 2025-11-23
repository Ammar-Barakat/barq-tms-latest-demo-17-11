namespace BarqTMS.API.Models.Enums
{
    public enum UserRole
    {
        Manager = 1,
        AssistantManager = 2,
        AccountManager = 3,
        TeamLeader = 4,
        Employee = 5,
        Client = 6
    }

    public enum ProjectStatus
    {
        Planned,
        Active,
        Completed,
        OnHold
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        InReview,
        Completed
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum RelatedEntityType
    {
        Task,
        Project,
        Company
    }

    public enum NotificationType
    {
        TaskAssigned,
        NewClient,
        DeadlineApproaching,
        TaskCompleted,
        TaskRejected,
        General
    }

    public enum ChangeRequestType
    {
        ProfileUpdate,
        PasswordChange
    }

    public enum ChangeRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum EventType
    {
        Meeting = 1,
        Deadline = 2,
        Task = 3,
        Reminder = 4
    }
}
