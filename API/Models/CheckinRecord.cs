using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Models;

public partial class CheckinRecord
{
    [Key]
    public int CheckinId { get; set; }

    public int? StudentId { get; set; }

    public int? JobId { get; set; }

    public DateTime? CheckinTime { get; set; }

    public DateTime? CheckoutTime { get; set; }

    public virtual Job? Job { get; set; }

    public virtual User? Student { get; set; }
}
