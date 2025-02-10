using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    public partial class Calendar
    {
        public Calendar()
        {
            Events = new HashSet<Event>();
        }

        public int CalendarId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Event> Events { get; set; }
    }
}
