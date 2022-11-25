#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignmentv1.Data;
using Assignmentv1.Models;
using Assignmentv1.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Assignmentv1.Controllers
    {

    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly Assignmentv1Context _context;

        public RoomsController(Assignmentv1Context context)
        {
            _context = context;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoom()
        {
            return await _context.Room.ToListAsync();
        }

        // GET: api/Rooms/WithFetes
        [HttpGet("WithFetes")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomWithFetes()
        {
            return await _context
                .Room
                .Include(x => x.Fetes)
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Room.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        [HttpGet("WithFetes/{id}")]
        public async Task<ActionResult<Room>> GetRoomWithFetes(int id)
        {
            var room = await _context
                .Room
                .Include(x => x.Fetes)
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync();

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, Room room)
        {
            if (id != room.Id)
            {
                return BadRequest();
            }

            // get the room as it currently exists in the database, 
            // we'll be updating this with the one posted to the method
            var existingRoom =
                await _context.Room
                    .Include(x => x.Fetes)
                    .Where(x => x.Id == id)
                    .SingleOrDefaultAsync();

            if(existingRoom != null)
            {
                // update the existing Room object
                _context.Entry(existingRoom)
                    .CurrentValues
                    .SetValues(room);

                // now handle the fetes, remove any that were selected before but are not not
                foreach (var existingFete in existingRoom.Fetes.ToList())
                {
                    if (!room.Fetes.Any(m => m.Id == existingFete.Id))
                    {
                        existingRoom.Fetes.Remove(existingFete);
                    }
                }

                // add any rooms that are selected now that were not before
                foreach (var fete in room.Fetes)
                {
                    if (!existingRoom.Fetes.Any(m => m.Id == fete.Id))
                    {
                        existingRoom.Fetes.Add(fete);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
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

        // POST: api/Rooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom(Room room)
        {
            try
            {
                foreach(var fete in room.Fetes)
                {
                    _context.Fete.Attach(fete);
                }

                _context.Room.Add(room);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetRoom", new { id = room.Id }, room);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Room.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Room.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(int id)
        {
            return _context.Room.Any(e => e.Id == id);
        }
    }
}
