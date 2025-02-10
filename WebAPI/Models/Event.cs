using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Models
{
    public partial class Event
    {
        public Event()
        {
            EventAttendees = new HashSet<EventAttendee>();
        }

        public int EventId { get; set; }
        public int CalendarId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool? AllDay { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public virtual Calendar Calendar { get; set; } = null!;

        public virtual ICollection<EventAttendee> EventAttendees { get; set; }
    }
}
