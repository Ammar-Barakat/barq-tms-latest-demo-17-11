using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarqTMS.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DEPARTMENT",
                columns: table => new
                {
                    dept_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dept_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEPARTMENT", x => x.dept_id);
                });

            migrationBuilder.CreateTable(
                name: "LOGIN_ATTEMPT",
                columns: table => new
                {
                    attempt_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    was_successful = table.Column<bool>(type: "INTEGER", nullable: false),
                    attempted_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    failure_reason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOGIN_ATTEMPT", x => x.attempt_id);
                });

            migrationBuilder.CreateTable(
                name: "PRIORITY",
                columns: table => new
                {
                    priority_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    level = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRIORITY", x => x.priority_id);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    role_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "STATUS",
                columns: table => new
                {
                    status_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    status_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STATUS", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "TASK_CATEGORY",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TASK_CATEGORY", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    role = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_login = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    position = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    team_leader_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_USER_USER_team_leader_id",
                        column: x => x.team_leader_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AUDIT_LOG",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    entity_type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    entity_id = table.Column<int>(type: "INTEGER", nullable: true),
                    action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    changes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    old_values = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    new_values = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    table_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDIT_LOG", x => x.audit_id);
                    table.ForeignKey(
                        name: "FK_AUDIT_LOG_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CLIENT",
                columns: table => new
                {
                    client_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    company = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    account_manager_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENT", x => x.client_id);
                    table.ForeignKey(
                        name: "FK_CLIENT_USER_account_manager_id",
                        column: x => x.account_manager_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PASSWORD_RESET_TOKEN",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_used = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASSWORD_RESET_TOKEN", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_PASSWORD_RESET_TOKEN_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_DEPARTMENTS",
                columns: table => new
                {
                    user_dept_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    dept_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_DEPARTMENTS", x => x.user_dept_id);
                    table.ForeignKey(
                        name: "FK_USER_DEPARTMENTS_DEPARTMENT_dept_id",
                        column: x => x.dept_id,
                        principalTable: "DEPARTMENT",
                        principalColumn: "dept_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_DEPARTMENTS_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_SETTINGS",
                columns: table => new
                {
                    setting_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    theme = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    timezone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    email_notifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    push_notifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    task_reminders = table.Column<bool>(type: "INTEGER", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_SETTINGS", x => x.setting_id);
                    table.ForeignKey(
                        name: "FK_USER_SETTINGS_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROJECT",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    project_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    client_id = table.Column<int>(type: "INTEGER", nullable: true),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    team_leader_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECT", x => x.project_id);
                    table.ForeignKey(
                        name: "FK_PROJECT_CLIENT_client_id",
                        column: x => x.client_id,
                        principalTable: "CLIENT",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PROJECT_USER_team_leader_id",
                        column: x => x.team_leader_id,
                        principalTable: "USER",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "PROJECT_MILESTONE",
                columns: table => new
                {
                    milestone_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    project_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    due_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    completion_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_completed = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECT_MILESTONE", x => x.milestone_id);
                    table.ForeignKey(
                        name: "FK_PROJECT_MILESTONE_PROJECT_project_id",
                        column: x => x.project_id,
                        principalTable: "PROJECT",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TASK",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    priority_id = table.Column<int>(type: "INTEGER", nullable: false),
                    status_id = table.Column<int>(type: "INTEGER", nullable: false),
                    due_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by = table.Column<int>(type: "INTEGER", nullable: false),
                    assigned_to = table.Column<int>(type: "INTEGER", nullable: true),
                    original_assigner_id = table.Column<int>(type: "INTEGER", nullable: true),
                    delegated_by = table.Column<int>(type: "INTEGER", nullable: true),
                    dept_id = table.Column<int>(type: "INTEGER", nullable: false),
                    project_id = table.Column<int>(type: "INTEGER", nullable: true),
                    category_id = table.Column<int>(type: "INTEGER", nullable: true),
                    estimated_hours = table.Column<decimal>(type: "TEXT", nullable: true),
                    actual_hours = table.Column<decimal>(type: "TEXT", nullable: true),
                    tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_recurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    drive_folder_link = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    material_drive_folder_link = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TASK", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_TASK_DEPARTMENT_dept_id",
                        column: x => x.dept_id,
                        principalTable: "DEPARTMENT",
                        principalColumn: "dept_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TASK_PRIORITY_priority_id",
                        column: x => x.priority_id,
                        principalTable: "PRIORITY",
                        principalColumn: "priority_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TASK_PROJECT_project_id",
                        column: x => x.project_id,
                        principalTable: "PROJECT",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TASK_STATUS_status_id",
                        column: x => x.status_id,
                        principalTable: "STATUS",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TASK_TASK_CATEGORY_category_id",
                        column: x => x.category_id,
                        principalTable: "TASK_CATEGORY",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TASK_USER_assigned_to",
                        column: x => x.assigned_to,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TASK_USER_created_by",
                        column: x => x.created_by,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TASK_USER_delegated_by",
                        column: x => x.delegated_by,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TASK_USER_original_assigner_id",
                        column: x => x.original_assigner_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ATTACHMENT",
                columns: table => new
                {
                    file_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    task_id = table.Column<int>(type: "INTEGER", nullable: false),
                    file_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    uploaded_by = table.Column<int>(type: "INTEGER", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATTACHMENT", x => x.file_id);
                    table.ForeignKey(
                        name: "FK_ATTACHMENT_TASK_task_id",
                        column: x => x.task_id,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ATTACHMENT_USER_uploaded_by",
                        column: x => x.uploaded_by,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CALENDAR_EVENT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecurrencePattern = table.Column<int>(type: "INTEGER", nullable: true),
                    RecurrenceInterval = table.Column<int>(type: "INTEGER", nullable: true),
                    RecurrenceEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RecurrenceDays = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALENDAR_EVENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CALENDAR_EVENT_DEPARTMENT_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DEPARTMENT",
                        principalColumn: "dept_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CALENDAR_EVENT_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CALENDAR_EVENT_TASK_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CALENDAR_EVENT_USER_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CALENDAR_EVENT_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATION",
                columns: table => new
                {
                    notif_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_read = table.Column<bool>(type: "INTEGER", nullable: false),
                    task_id = table.Column<int>(type: "INTEGER", nullable: true),
                    project_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION", x => x.notif_id);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_PROJECT_project_id",
                        column: x => x.project_id,
                        principalTable: "PROJECT",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_TASK_task_id",
                        column: x => x.task_id,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RECURRING_TASK",
                columns: table => new
                {
                    recurring_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    template_task_id = table.Column<int>(type: "INTEGER", nullable: false),
                    frequency_type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    frequency_interval = table.Column<int>(type: "INTEGER", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_generated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RECURRING_TASK", x => x.recurring_id);
                    table.ForeignKey(
                        name: "FK_RECURRING_TASK_TASK_template_task_id",
                        column: x => x.template_task_id,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TASK_COMMENT",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    task_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TASK_COMMENT", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_TASK_COMMENT_TASK_task_id",
                        column: x => x.task_id,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TASK_COMMENT_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TASK_DEPENDENCY",
                columns: table => new
                {
                    dependency_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    task_id = table.Column<int>(type: "INTEGER", nullable: false),
                    prerequisite_task_id = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TASK_DEPENDENCY", x => x.dependency_id);
                    table.CheckConstraint("CK_TaskDependency_NoSelf", "[prerequisite_task_id] <> [task_id]");
                    table.ForeignKey(
                        name: "FK_TASK_DEPENDENCY_TASK_prerequisite_task_id",
                        column: x => x.prerequisite_task_id,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TASK_DEPENDENCY_TASK_task_id",
                        column: x => x.task_id,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TIME_LOG",
                columns: table => new
                {
                    time_log_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    task_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    start_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    duration_minutes = table.Column<int>(type: "INTEGER", nullable: true),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_billable = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TIME_LOG", x => x.time_log_id);
                    table.ForeignKey(
                        name: "FK_TIME_LOG_TASK_task_id",
                        column: x => x.task_id,
                        principalTable: "TASK",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TIME_LOG_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CALENDAR_EVENT_ATTENDEE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CalendarEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOrganizer = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALENDAR_EVENT_ATTENDEE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CALENDAR_EVENT_ATTENDEE_CALENDAR_EVENT_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CALENDAR_EVENT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CALENDAR_EVENT_ATTENDEE_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CALENDAR_REMINDER",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CalendarEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    MinutesBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALENDAR_REMINDER", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CALENDAR_REMINDER_CALENDAR_EVENT_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CALENDAR_EVENT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CALENDAR_REMINDER_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ATTACHMENT_task_id",
                table: "ATTACHMENT",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_ATTACHMENT_uploaded_by",
                table: "ATTACHMENT",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_user_id",
                table: "AUDIT_LOG",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_CreatedByUserId",
                table: "CALENDAR_EVENT",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_DepartmentId",
                table: "CALENDAR_EVENT",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_EndDate",
                table: "CALENDAR_EVENT",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_EventType",
                table: "CALENDAR_EVENT",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_ProjectId",
                table: "CALENDAR_EVENT",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_StartDate",
                table: "CALENDAR_EVENT",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_TaskId",
                table: "CALENDAR_EVENT",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_UserId",
                table: "CALENDAR_EVENT",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_ATTENDEE_CalendarEventId",
                table: "CALENDAR_EVENT_ATTENDEE",
                column: "CalendarEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_EVENT_ATTENDEE_UserId",
                table: "CALENDAR_EVENT_ATTENDEE",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_REMINDER_CalendarEventId",
                table: "CALENDAR_REMINDER",
                column: "CalendarEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CALENDAR_REMINDER_UserId",
                table: "CALENDAR_REMINDER",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_account_manager_id",
                table: "CLIENT",
                column: "account_manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENT_email",
                table: "CLIENT",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DEPARTMENT_dept_name",
                table: "DEPARTMENT",
                column: "dept_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_project_id",
                table: "NOTIFICATION",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_task_id",
                table: "NOTIFICATION",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_user_id",
                table: "NOTIFICATION",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORD_RESET_TOKEN_user_id",
                table: "PASSWORD_RESET_TOKEN",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_client_id",
                table: "PROJECT",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_team_leader_id",
                table: "PROJECT",
                column: "team_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_MILESTONE_project_id",
                table: "PROJECT_MILESTONE",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_RECURRING_TASK_template_task_id",
                table: "RECURRING_TASK",
                column: "template_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_role_name",
                table: "ROLE",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TASK_assigned_to",
                table: "TASK",
                column: "assigned_to");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_category_id",
                table: "TASK",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_created_by",
                table: "TASK",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_delegated_by",
                table: "TASK",
                column: "delegated_by");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_dept_id",
                table: "TASK",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_original_assigner_id",
                table: "TASK",
                column: "original_assigner_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_priority_id",
                table: "TASK",
                column: "priority_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_project_id",
                table: "TASK",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_status_id",
                table: "TASK",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_COMMENT_task_id",
                table: "TASK_COMMENT",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_COMMENT_user_id",
                table: "TASK_COMMENT",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_DEPENDENCY_prerequisite_task_id",
                table: "TASK_DEPENDENCY",
                column: "prerequisite_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_TASK_DEPENDENCY_task_id_prerequisite_task_id",
                table: "TASK_DEPENDENCY",
                columns: new[] { "task_id", "prerequisite_task_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TIME_LOG_task_id",
                table: "TIME_LOG",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_TIME_LOG_user_id",
                table: "TIME_LOG",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_email",
                table: "USER",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_USER_team_leader_id",
                table: "USER",
                column: "team_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_username",
                table: "USER",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_DEPARTMENTS_dept_id",
                table: "USER_DEPARTMENTS",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_DEPARTMENTS_user_id_dept_id",
                table: "USER_DEPARTMENTS",
                columns: new[] { "user_id", "dept_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_SETTINGS_user_id",
                table: "USER_SETTINGS",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ATTACHMENT");

            migrationBuilder.DropTable(
                name: "AUDIT_LOG");

            migrationBuilder.DropTable(
                name: "CALENDAR_EVENT_ATTENDEE");

            migrationBuilder.DropTable(
                name: "CALENDAR_REMINDER");

            migrationBuilder.DropTable(
                name: "LOGIN_ATTEMPT");

            migrationBuilder.DropTable(
                name: "NOTIFICATION");

            migrationBuilder.DropTable(
                name: "PASSWORD_RESET_TOKEN");

            migrationBuilder.DropTable(
                name: "PROJECT_MILESTONE");

            migrationBuilder.DropTable(
                name: "RECURRING_TASK");

            migrationBuilder.DropTable(
                name: "ROLE");

            migrationBuilder.DropTable(
                name: "TASK_COMMENT");

            migrationBuilder.DropTable(
                name: "TASK_DEPENDENCY");

            migrationBuilder.DropTable(
                name: "TIME_LOG");

            migrationBuilder.DropTable(
                name: "USER_DEPARTMENTS");

            migrationBuilder.DropTable(
                name: "USER_SETTINGS");

            migrationBuilder.DropTable(
                name: "CALENDAR_EVENT");

            migrationBuilder.DropTable(
                name: "TASK");

            migrationBuilder.DropTable(
                name: "DEPARTMENT");

            migrationBuilder.DropTable(
                name: "PRIORITY");

            migrationBuilder.DropTable(
                name: "PROJECT");

            migrationBuilder.DropTable(
                name: "STATUS");

            migrationBuilder.DropTable(
                name: "TASK_CATEGORY");

            migrationBuilder.DropTable(
                name: "CLIENT");

            migrationBuilder.DropTable(
                name: "USER");
        }
    }
}
