using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library_API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.Metrics;
using System.Net;

namespace Library_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly LibraryDBContext _context;

        public ReservationsController(LibraryDBContext context)
        {
            _context = context;
        }

        // GET: Reservations
        [HttpGet("GetReservations")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            var libraryDBContext = _context.Reservations;
            return await libraryDBContext.ToListAsync();
        }

        [HttpGet("GetReservation/{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int? id)
        {
            if (id == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
               
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // GET: Reservations/Details/5
        [HttpGet("GetBookReservation/{bookId}")]
        public async Task<ActionResult<Reservation>> GetBookReservation(int? bookId)
        {
            if (bookId == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(m => m.BookId == bookId);
            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        [HttpGet("GetMemberReservations/{memberId}")]
        public async Task<ActionResult<Reservation>> GetMemberReservations(int? memberId)
        {
            if ( memberId == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(m => m.MemberId == memberId);
            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }
        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("CreateReservation")]
        public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservation)
        {
            var checkReservation = ReservationExists(reservation.BookId);
            if (checkReservation == true)
            {
               var result = await GetBookReservation(reservation.BookId);
                return result;


            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReservation", new { id = reservation.ReservationId }, reservation);
        }


        // DELETE: Reservations/Delete/5
        [HttpDelete("DeleteReservation/{Id}")]
        public async Task<ActionResult<Reservation>> DeleteReservation(int Id)
        {
            if (_context.Reservations == null)
            {
                return Problem("Entity set 'LibraryDBContext.Reservations'  is null.");
            }
            var reservation = await _context.Reservations.FindAsync(Id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }
            
            await _context.SaveChangesAsync();
            return reservation;
        }

        [HttpDelete("DeleteBookReservation/{bookId}")]
        public async Task<ActionResult<Reservation>> DeleteBookReservation(int bookId)
        {
            if (_context.Reservations == null)
            {
                return Problem("Entity set 'LibraryDBContext.Reservations'  is null.");
            }
            var reservation = await _context.Reservations.Where(x=>x.BookId == bookId).FirstAsync();
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return reservation;
        }

        private bool ReservationExists(int id)
        {
          return _context.Reservations.Any(e => e.BookId == id);
        }
    }
}
