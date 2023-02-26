using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library_API.Models;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Authorization;

namespace Library_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    
    public class BooksController : ControllerBase
    {
        private readonly LibraryDBContext _context;

        public BooksController(LibraryDBContext context)
        {
            _context = context;
        }

        // GET: Books
        [HttpGet("GetBooks")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        // GET: Books/Details/5
        [HttpGet("GetBook/{id}")]
        public async Task<ActionResult<Book>> GetBook(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }


        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("CreateBook")]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {          
                _context.Add(book);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetBook", new { id = book.BookId }, book);          
        }


        // PUT: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut("UpdateBook/{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return NoContent();
        }


        // DELETE: Books/Delete/5
        [HttpDelete("DeleteBook/{id}")]
        public async Task<ActionResult<Book>> DeleteBook(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'LibraryDBContext.Books'  is null.");
            }
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }
            
            await _context.SaveChangesAsync();
            return book;
        }

        private bool BookExists(int id)
        {
          return _context.Books.Any(e => e.BookId == id);
        }
    }
}
