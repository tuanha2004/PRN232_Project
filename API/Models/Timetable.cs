using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Timetable
{
    public int TimetableId { get; set; }

    public int? StudentId { get; set; }

    public int? JobId { get; set; }

    public string? DayOfWeek { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public virtual Job? Job { get; set; }

    public virtual User? Student { get; set; }
}
