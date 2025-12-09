using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskPilot.Core.Components.Entities
{
    /// <summary>
    /// Represents a milestone within a project that marks significant progress points or deliverables.
    /// Examples: "Alpha Release", "Beta Testing Complete", "Version 1.0 Launch"
    /// </summary>
    [Table("Milestones")]
    [Index(nameof(ProjectId), nameof(Name), IsUnique = true)]
    public class Milestone
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the milestone.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the project identifier this milestone belongs to.
        /// </summary>
        [Required]
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the milestone.
        /// Must be unique within the project.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed description of the milestone.
        /// </summary>
        [MaxLength(4000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the due date for the milestone.
        /// Null indicates no specific deadline.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the color for UI display in hexadecimal format.
        /// Example: "#FF6A2C"
        /// </summary>
        [MaxLength(9)]
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the sort order for manual ordering in UI.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when this milestone was created.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC timestamp when this milestone was last modified.
        /// Null if never modified since creation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the project this milestone belongs to.
        /// </summary>
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of tasks associated with this milestone.
        /// </summary>
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        #endregion
    }
}
