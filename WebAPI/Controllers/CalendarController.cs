using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAPI.Models;

namespace WebAPI.Controllers
{

    public class CalendarController : ODataController
    {
        PROJECT_PRN231Context _context = new PROJECT_PRN231Context();


        // GET: odata/Calendar
        [EnableQuery]
        public IQueryable<Calendar> Get()
        {
            return _context.Calendars;
        }

        [EnableQuery]
        public SingleResult<Calendar> Get([FromODataUri] int key)
        {
            return SingleResult.Create(_context.Calendars.Where(e => e.CalendarId == key));
        }
        [EnableQuery]
        [Authorize]
        [HttpGet("Calendar/GetByUserId")]
        public IActionResult GetByUserId(ODataQueryOptions<Calendar> options) {

            String userIds = HttpContext.Items["UserId"] as String;
            int userId = int.Parse(userIds);
            if (userId == null)
            {
                return Unauthorized();
            }

            var calendars = _context.Calendars.Where(c => c.UserId == userId);
            var results = options.ApplyTo(calendars);

            return Ok(results);
        }
        public IActionResult Post([FromBody] Calendar calendar)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Calendars.Add(calendar);
            _context.SaveChanges();

            return Created(calendar);
        }

        // PUT: odata/Calendar(key)
        public IActionResult Put([FromODataUri] int key, [FromBody] Calendar calendar)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != calendar.CalendarId)
            {
                return BadRequest();
            }

            _context.Entry(calendar).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Calendars.Any(e => e.CalendarId == key))
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

        // DELETE: odata/Calendar(key)
        public IActionResult Delete([FromODataUri] int key)
        {
            var calendar = _context.Calendars.Find(key);

            if (calendar == null)
            {
                return NotFound();
            }

            _context.Calendars.Remove(calendar);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
