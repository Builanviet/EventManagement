using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    public partial class User
    {
        public User()
        {
            Calendars = new HashSet<Calendar>();
            EventAttendees = new HashSet<EventAttendee>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte[]? Passwordsalt { get; set; }
        public byte[]? Passwordhash { get; set; }

        public virtual ICollection<Calendar> Calendars { get; set; }
        public virtual ICollection<EventAttendee> EventAttendees { get; set; }
    }
}
