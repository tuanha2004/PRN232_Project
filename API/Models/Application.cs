using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int? StudentId { get; set; }

    public int? JobId { get; set; }

    public DateTime? AppliedAt { get; set; }

    public string? Status { get; set; }

    public string? Phone { get; set; }

    public string? StudentYear { get; set; }

    public string? WorkType { get; set; }

    public string? Notes { get; set; }

    public virtual Job? Job { get; set; }

    public virtual User? Student { get; set; }
}
