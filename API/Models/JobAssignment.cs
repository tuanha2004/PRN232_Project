using System;
using System.Collections.Generic;

namespace API.Models;

public partial class JobAssignment
{
    public int AssignmentId { get; set; }

    public int? StudentId { get; set; }

    public int? JobId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public string? Status { get; set; }

    public virtual Job? Job { get; set; }

    public virtual User? Student { get; set; }
}
