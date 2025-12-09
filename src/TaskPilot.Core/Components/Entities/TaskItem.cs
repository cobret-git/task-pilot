using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskPilot.Core.Components.Data;

namespace TaskPilot.Core.Components.Entities
{
    /// <summary>
    /// Represents an actual work item/task with support for hierarchical subtasks.
    /// Examples: "Fix bug #123", "Release v2024.1", "Implement dark mode"
    /// </summary>
    [Table("Tasks")]
    public class TaskItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the task.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the task.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed description of the task.
        /// </summary>
        [MaxLength(4000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the parent task identifier for hierarchical subtask organization.
        /// Null indicates a root-level task.
        /// </summary>
        public int? ParentTaskId { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy depth level (0-based).
        /// Level 0 = Root task, Level 1+ = Subtask.
        /// Maximum recommended depth is 5 levels.
        /// </summary>
        public int HierarchyLevel { get; set; }

        /// <summary>
        /// Gets or sets the project identifier this task belongs to.
        /// Nullable - tasks can exist outside of projects.
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the task type/category identifier.
        /// Nullable - tasks can exist without a specific type.
        /// </summary>
        public int? TaskTypeId { get; set; }

        /// <summary>
        /// Gets or sets the milestone identifier this task belongs to.
        /// Nullable - tasks can exist without a specific milestone.
        /// Mutually exclusive with ProjectId and ParentTaskId - only one can be set.
        /// </summary>
        public int? MilestoneId { get; set; }

        /// <summary>
        /// Gets or sets the due date for the task.
        /// Null indicates no deadline.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the start date for the task.
        /// Null indicates the task hasn't been started or scheduled.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the task.
        /// </summary>
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        /// <summary>
        /// Gets or sets the current status of the task.
        /// </summary>
        [Required]
        public Data.TaskStatus Status { get; set; } = Data.TaskStatus.NotStarted;

        /// <summary>
        /// Gets the progress percentage (0-100) calculated dynamically based on completed subtasks.
        /// Returns 0 if there are no subtasks.
        /// </summary>
        [NotMapped]
        public int ProgressPercentage => CalculateProgressPercentage();

        /// <summary>
        /// Gets or sets the UTC timestamp when this task was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC timestamp when this task was last modified.
        /// Null if never modified since creation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the parent task for hierarchical organization.
        /// </summary>
        [ForeignKey(nameof(ParentTaskId))]
        public TaskItem? ParentTask { get; set; }

        /// <summary>
        /// Gets or sets the collection of child subtasks.
        /// </summary>
        public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();

        /// <summary>
        /// Gets or sets the project this task belongs to.
        /// </summary>
        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the task type/category.
        /// </summary>
        [ForeignKey(nameof(TaskTypeId))]
        public TaskType? TaskType { get; set; }

        /// <summary>
        /// Gets or sets the milestone this task belongs to.
        /// </summary>
        [ForeignKey(nameof(MilestoneId))]
        public Milestone? Milestone { get; set; }

        #endregion

        #region Helpers

        /// <summary>
        /// Calculates the progress percentage based on completed subtasks.
        /// </summary>
        /// <returns>Progress percentage (0-100). Returns 0 if there are no subtasks.</returns>
        private int CalculateProgressPercentage()
        {
            if (SubTasks == null || SubTasks.Count == 0)
            {
                return 0;
            }

            int completedCount = 0;
            foreach (var subTask in SubTasks)
            {
                if (subTask.Status == Data.TaskStatus.Completed)
                {
                    completedCount++;
                }
            }

            return (int)((double)completedCount / SubTasks.Count * 100);
        }

        #endregion
    }
}
