using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Controllers
{

    public class EventController : ODataController
    {
        private readonly PROJECT_PRN231Context _context;

        public EventController(PROJECT_PRN231Context context)
        {
            _context = context;
        }
        [HttpGet("GetEventsByUserId/{userId}")]
        public IQueryable<Event> GetEventsByUserId(int userId)
        {

            var events = _context.Events
                            .Where(e => e.Calendar.UserId == userId).Include(e => e.EventAttendees).ThenInclude(e => e.User)
                            .AsQueryable(); ;

            return events;
        }
        [EnableQuery]
        [HttpGet("GetEventsByUserId/{userId}/calendarId/{calendarId}")]
        [Authorize]
        public IQueryable<Event> GetEventsByUserId(int userId,int calendarId)
        {

            var events = _context.Events
                            .Where(e => e.Calendar.UserId == userId && e.CalendarId == calendarId)
                            .AsQueryable(); ;

            return events;
        }




        // GET: odata/Event
        [EnableQuery]
        [Authorize]
        public IQueryable<Event> Get()
        {
            return _context.Events;
        }

      


        // GET: odata/Event(key)
        [EnableQuery]

        public IActionResult Get([FromRoute] int key)
        {
            var eventData = _context.Events
                .Where(e => e.EventId == key)
                .Select(e => new {
                    e.EventId,
                    e.CalendarId,
                    e.Title,
                    e.Description,
                    e.Location,
                    e.StartTime,
                    e.EndTime,
                    e.AllDay,
                    EventAttendees = e.EventAttendees.Select(ea => new {
                        ea.EventAttendeesId,
                        ea.EventId,
                        ea.UserId,
                        ea.Status,
                        User = ea.User // Include User details if needed
                    })
                })
                .FirstOrDefault();

            if (eventData == null)
            {
                return NotFound();
            }

            return Ok(eventData);
        }

        // POST: odata/Event
        [Authorize]
        public IActionResult Post([FromBody] Event @event)
        {
            if(@event.StartTime == null || @event.EndTime == null)
            {
                return BadRequest(new { message = "Start time and end time must be provided" });
            }
            if(@event.StartTime < System.DateTime.Now || @event.EndTime <System.DateTime.Now)
            {
                return BadRequest(new { message = "Start time or end time must be in the future" });
            }
           
            if(@event.StartTime > @event.EndTime)
            {
                return BadRequest(new { message = "Start time must be before end time" });
            }
            @event.Calendar = null;

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            _context.Events.Add(@event);
            _context.SaveChanges();

            return Created(@event);
        }

        // PUT: odata/Event(key)
        public IActionResult Put([FromRoute] int key, [FromBody] Event eventData)
        {
            if (eventData == null || key != eventData.EventId)
            {
                return BadRequest();
            }

            var existingEvent = _context.Events.FirstOrDefault(e => e.EventId == key);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // Update fields of the existing event
            existingEvent.Title = eventData.Title;
            existingEvent.Description = eventData.Description;
            existingEvent.Location = eventData.Location;
            existingEvent.StartTime = eventData.StartTime;
            existingEvent.EndTime = eventData.EndTime;
            existingEvent.AllDay = eventData.AllDay;
            existingEvent.CalendarId = eventData.CalendarId;
            existingEvent.UpdatedAt = DateTime.Now;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Events.Any(e => e.EventId == key))
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

        // DELETE: odata/Event(key)
        public IActionResult Delete([FromRoute] int key)
        {
            var @event = _context.Events.Include(e => e.EventAttendees).FirstOrDefault(e => e.EventId == key);

            if (@event == null)
            {
                return NotFound();
            }
            _context.EventAttendees.RemoveRange(@event.EventAttendees);

            _context.Events.Remove(@event);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
