#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignmentv1.Data;
using Assignmentv1.Models;

namespace FeteWebAPIV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class FetesController : ControllerBase
    {
        private readonly Assignmentv1Context _context;

        public FetesController(Assignmentv1Context context)
        {
            _context = context;
        }

        // GET: api/Fetes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fete>>> GetFete()
        {
            return await _context.Fete.ToListAsync();
        }

        // GET: api/Fetes/WithRelatedObjects
        [HttpGet("WithRelatedObjects/{fetename?}")]
        //[AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Fete>>> GetFeteWithRelatedObjects(string fetename = null)
        {
            var queryableFetes =
                this._context
                    .Fete
                    .Include(x => x.Rooms)
                    .Include(x => x.Bookings)
                    .AsNoTracking()
                    .AsQueryable();

            if (!string.IsNullOrEmpty(fetename))
            {
                queryableFetes = queryableFetes.Where(x => x.Name.Contains(fetename));
            }

            var list = await queryableFetes.ToListAsync();

            return list;
        }

        // GET: api/Fetes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fete>> GetFete(int id)
        {
            var fete = await _context.Fete.FindAsync(id);

            if (fete == null)
            {
                return NotFound();
            }

            return fete;
        }

        // GET: api/Fetes/IdOrFeteName
        [HttpGet("ByIdOrFeteName/{IdOrFeteName}")]
        public async Task<ActionResult<Fete>> GetFeteByIdOrFeteName(string idOrFeteName)
        {
            var feteQuery = _context
                .Fete
                .Include(x => x.Rooms)
                .Include(x => x.Bookings);
                
                
            var fete = feteQuery                
                .Where(x => x.Name == idOrFeteName)
                .FirstOrDefault();

            if (fete == null &&
                int.TryParse(idOrFeteName, out int feteId))
            {
                // the fete was not found by fetename, so if search value casts to an int, assume it was an Id entered...
                fete = await feteQuery
                    .Where(x => x.Id == feteId)
                    .SingleOrDefaultAsync();
            }

            if (fete == null)
            {
                return NotFound();
            }

            return fete;
        }

        // PUT: api/Fetes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        //[Authorize(Roles = Roles.Admin + "," + Roles.Staff)]
        public async Task<IActionResult> PutFete(int id, Fete fete)
        {
            if (id != fete.Id)
            {
                return BadRequest();
            }

            // get the fete as it currently exists in the database, 
            // we'll be updating this with the one posted to the method
            var existingFete =
                await _context.Fete
                    .Include(x => x.Bookings)
                    .Include(x => x.Rooms)
                    .Where(x => x.Id == id)
                    .SingleOrDefaultAsync();

            if (existingFete != null)
            {
                // update the existing Fete object
                _context.Entry(existingFete)
                    .CurrentValues
                    .SetValues(fete);

                // now handle the rooms, remove any that were selected before but are not not
                foreach (var existingRoom in existingFete.Rooms.ToList())
                {
                    if (!fete.Rooms.Any(a => a.Id == existingRoom.Id))
                    {
                        existingFete.Rooms.Remove(existingRoom);
                    }
                }

                // add any rooms that are selected now that were not before
                foreach (var room in fete.Rooms)
                {
                    if (!existingFete.Rooms.Any(a => a.Id == room.Id))
                    {
                        existingFete.Rooms.Add(room);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeteExists(id))
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

        // POST: api/Fetes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        //[Authorize(Roles = Roles.Admin + "," + Roles.Staff)]
        public async Task<ActionResult<Fete>> PostFete(Fete fete)
        {
            try
            {
                foreach (var room in fete.Rooms)
                {
                    _context.Room.Attach(room);
                }

                //if(fete.Bookings != null)
               // {
                //    _context.Booking.Attach(fete.Bookings);
                //}

                _context.Fete.Add(fete);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction("GetFete", new { id = fete.Id }, fete);
        }

        // DELETE: api/Fetes/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = Roles.Admin + "," + Roles.Staff)]
        public async Task<IActionResult> DeleteFete(int id)
        {
            var fete = await _context.Fete.FindAsync(id);
            if (fete == null)
            {
                return NotFound();
            }

            _context.Fete.Remove(fete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FeteExists(int id)
        {
            return _context.Fete.Any(e => e.Id == id);
        }
    }
}
