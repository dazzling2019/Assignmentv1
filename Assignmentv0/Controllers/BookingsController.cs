#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignmentv1.Data;
using Assignmentv1.Models;

namespace Assignmentv1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly Assignmentv1Context _context;

        public BookingsController(Assignmentv1Context context)
        {
            _context = context;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBooking()
        {
            return await _context.Booking.ToListAsync();
        }

        // GET: api/Bookings/WithRelatedObjects
        [HttpGet("WithRelatedObjects/{initials?}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingWithRelatedObjects(int Id)
        {
            var queryableBookings =
            this._context
            .Booking
            .Include(x => x.Fete)
            .Include(x => x.Student)
            .AsNoTracking()
            .AsQueryable();

           
            
                queryableBookings = queryableBookings.Where(x => x.StudentId == Id);
            

            var list = await queryableBookings.ToListAsync();
            return list;
        }



        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Booking.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        // GET: api/Bookings/IdOrInitials
        [HttpGet("ByIdOrInitials/{IdOrInitials}")]
        public async Task<ActionResult<Booking>> GetBookingByIdOrInitials(int Id)
        {
            var bookingQuery = _context
            .Booking
            .Include(x => x.Fete)
            .Include(x => x.Student);

           
            
                // booking was not found by initials, so if search value casts to an int, assume it was an Id entered...
                var booking = await bookingQuery
                .Where(x => x.Id == Id)
                .SingleOrDefaultAsync();
            
            if (booking == null)
            {
                return NotFound();
            }
            return booking;
        }


        // PUT: api/Bookings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Student)]
        public async Task<IActionResult> PutBooking(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return BadRequest();
            }

            // get the booking as it currently exists in the database,
            // updating this with the one posted to the method
            var existingBooking =
            await _context.Booking
                .Include(x => x.Fete)
                .Include(x => x.Student)
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync();

            if (existingBooking != null)
            {
                // update the existing Bookings object
                _context.Entry(existingBooking)
                .CurrentValues
                .SetValues(booking);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }                
            }
            return NoContent();
        }

        // POST: api/Bookings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.Student)]
        public async Task<ActionResult<Booking>> PostBooking(Booking booking)
        {
            try
            {
                if (booking.Fete != null)
                {
                    _context.Attach(booking.Fete);
                }
                _context.Booking.Add(booking);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return CreatedAtAction("GetBooking", new { id = booking.Id }, booking);
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Student)]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.Id == id);
        }
    }
}
