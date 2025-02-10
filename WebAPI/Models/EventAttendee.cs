using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    public partial class EventAttendee
    {
        public int EventAttendeesId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }

        public virtual Event Event { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
