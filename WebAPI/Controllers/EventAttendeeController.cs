using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using WebAPI.Models;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{

    public class EventAttendeeController : ODataController
    {
        PROJECT_PRN231Context _context = new PROJECT_PRN231Context();
        [Authorize]
        [HttpPost("AddEventAttendees")]
        public IActionResult AddEventAttendees([FromBody] List<EventAttendee> attendees)
        {
            if (attendees == null || !attendees.Any())
            {
                return BadRequest("Attendees list is empty or null.");
            }
            //var eventIds = attendees.Select(a => a.EventId).Distinct().ToList();
            //var events = _context.Events
            //    .Where(e => eventIds.Contains(e.EventId))
            //    .ToDictionary(e => e.EventId, e => e);
            //var eventAttendees = new List<EventAttendee>();
            //foreach (var attendeeDto in attendees)
            //{
            //    if (events.TryGetValue(attendeeDto.EventId, out var eventEntity))
            //    {
            //        var attendee = new EventAttendee
            //        {
            //            EventId = attendeeDto.EventId,
            //            UserId = attendeeDto.UserId, // Assuming UserId is part of the DTO
            //            Event = eventEntity // Associate with the event
            //        };
            //        eventAttendees.Add(attendee);
            //    }
            //    else
            //    {
            //        return BadRequest($"Event with ID {attendeeDto.EventId} not found.");
            //    }
            //}
            _context.EventAttendees.AddRange(attendees);
            _context.SaveChanges();

            return Ok(new { message = "Attendees added successfully!" });
        }

        [HttpGet("GetUserEvents")]
        public IActionResult GetUserEvents()
        {
            String userIds = HttpContext.Items["UserId"] as String;
            int userId = int.Parse(userIds);
            var events = _context.EventAttendees
                .Include(ea => ea.Event).ThenInclude(x =>x.Calendar).ThenInclude(x =>x.User)
                .Where(ea => ea.UserId == userId)
                .Select(ea => new
                {
                    title = ea.Event.Title,
                    eventId = ea.EventId,
                    location = ea.Event.Location,
                    startTime = ea.Event.StartTime,
                    endTime = ea.Event.EndTime,
                    username=ea.User.Username,
                    email = ea.User.Email,
                   status =ea.Status

                })
                .ToList();

            return Ok(events);
        }

        [HttpPost("UpdateEventStatus")]
        [Authorize]
        public IActionResult UpdateEventStatus([FromBody] EventStatusUpdateModel model)
        {
            String userIds = HttpContext.Items["UserId"] as String;
            int userId = int.Parse(userIds);

            var eventAttendee = _context.EventAttendees
                .FirstOrDefault(ea => ea.EventId == model.EventId && ea.UserId == userId);

            if (eventAttendee == null)
            {
                return NotFound();
            }

            eventAttendee.Status = model.Status;
            _context.SaveChanges();

            return Ok(new { message = "Status updated successfully" });
        }

        public class EventStatusUpdateModel
        {
            public int EventId { get; set; }
            public string Status { get; set; }
        }
        // GET: api/EventAttendee
        [EnableQuery]
        public IQueryable<EventAttendee> Get()
        {
            return _context.EventAttendees;
        }

        // GET: api/EventAttendee(5)
        [EnableQuery]
        public SingleResult<EventAttendee> Get([FromRoute] int key)
        {
            return SingleResult.Create(_context.EventAttendees.Where(e => e.EventAttendeesId == key));
        }
        [EnableQuery]
        [HttpGet("user/{userId}/event/{eventId}")]
        public IActionResult GetEventAttendeesByUserIdAndEventId([FromRoute] int userId, [FromRoute] int eventId)
        {
            var eventAttendees = _context.EventAttendees
                                        .Where(e => e.UserId == userId && e.EventId == eventId);

            if (!eventAttendees.Any())
            {
                return NotFound();
            }

            return Ok(eventAttendees);
        }


        // POST: api/EventAttendee
        public IActionResult Post([FromBody] EventAttendee eventAttendee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.EventAttendees.Add(eventAttendee);
            _context.SaveChanges();

            return Created(eventAttendee);
        }

        // PUT: api/EventAttendee(5)
        public IActionResult Put([FromRoute] int key, [FromBody] EventAttendee eventAttendee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != eventAttendee.EventAttendeesId)
            {
                return BadRequest();
            }

            _context.Entry(eventAttendee).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EventAttendees.Any(e => e.EventAttendeesId == key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/EventAttendee(5)
        public IActionResult Delete([FromRoute] int key)
        {
            var eventAttendee = _context.EventAttendees.Find(key);
            if (eventAttendee == null)
            {
                return NotFound();
            }

            _context.EventAttendees.Remove(eventAttendee);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
