using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Job
{
    public int JobId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public decimal? Salary { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? ProviderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CheckinRecord> CheckinRecords { get; set; } = new List<CheckinRecord>();

    public virtual ICollection<JobAssignment> JobAssignments { get; set; } = new List<JobAssignment>();

    public virtual User? Provider { get; set; }

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
