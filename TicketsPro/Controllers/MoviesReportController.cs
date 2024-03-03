using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eTickets.Data;
using eTickets.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Net.Http;

namespace TicketsPro.Controllers
{
    public class MoviesReportController : Controller
    {
        private readonly AppDbContext _context;

        public MoviesReportController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MoviesReport
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Movies.Include(m => m.Cinema).Include(m => m.Producer);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MoviesReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Cinema)
                .Include(m => m.Producer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: MoviesReport/Create
        public IActionResult Create()
        {
            ViewData["CinemaId"] = new SelectList(_context.Cinemas, "Id", "Name");
            ViewData["ProducerId"] = new SelectList(_context.Producers, "Id", "Name");
            return View();
        }

        // POST: MoviesReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,ImageURL,StartDate,EndDate,MovieCategory,CinemaId,ProducerId")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CinemaId"] = new SelectList(_context.Cinemas, "Id", "Name", movie.CinemaId);
            ViewData["ProducerId"] = new SelectList(_context.Producers, "Id", "Name", movie.ProducerId);
            return View(movie);
        }

        // GET: MoviesReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            ViewData["CinemaId"] = new SelectList(_context.Cinemas, "Id", "Name", movie.CinemaId);
            ViewData["ProducerId"] = new SelectList(_context.Producers, "Id", "Name", movie.ProducerId);
            return View(movie);
        }

        // POST: MoviesReport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageURL,StartDate,EndDate,MovieCategory,CinemaId,ProducerId")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            ViewData["CinemaId"] = new SelectList(_context.Cinemas, "Id", "Name", movie.CinemaId);
            ViewData["ProducerId"] = new SelectList(_context.Producers, "Id", "Name", movie.ProducerId);
            return View(movie);
        }

        // GET: MoviesReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Cinema)
                .Include(m => m.Producer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: MoviesReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: MoviesReport/DownloadPdf
        public async Task<IActionResult> DownloadPdf()
        {
            var movies = await _context.Movies.Include(m => m.Cinema).Include(m => m.Producer).ToListAsync();

            // Create a new PDF document
            var pdfDoc = new PdfDocument(new PdfWriter("MoviesReport.pdf"));
            var document = new Document(pdfDoc);

            // Add title with styling
            Paragraph title = new Paragraph("Movies Report")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20);
            document.Add(title);

            // Create table for movies' information
            Table table = new Table(8).UseAllAvailableWidth(); // Reduce the number of columns to 8

            // Add table header
            table.AddHeaderCell("Name");
            table.AddHeaderCell("Description");
            table.AddHeaderCell("Price");
            table.AddHeaderCell("Start Date");
            table.AddHeaderCell("End Date");
            table.AddHeaderCell("Category");
            table.AddHeaderCell("Cinema");
            table.AddHeaderCell("Producer Name");

            // Add movies' information to the table
            foreach (var movie in movies)
            {
                table.AddCell(movie.Name);
                table.AddCell(movie.Description);
                table.AddCell(movie.Price.ToString());
                table.AddCell(movie.StartDate.ToString());
                table.AddCell(movie.EndDate.ToString());
                table.AddCell(movie.MovieCategory.ToString()); // Convert movie category to string
                table.AddCell(movie.Cinema.Name); // Use cinema name instead of description
                table.AddCell(movie.Producer.FullName); // Use producer name instead of bio
            }

            // Set table properties
            table.SetWidth(UnitValue.CreatePercentValue(100)); // Set table width to 100% of available width

            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return a file result for downloading
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync("MoviesReport.pdf");
            return File(fileBytes, "application/pdf", "MoviesReport.pdf");
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
