using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eTickets.Data;
using eTickets.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;

namespace TicketsPro.Controllers
{
    public class CinemasReportController : Controller
    {
        private readonly AppDbContext _context;

        public CinemasReportController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CinemasReport
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cinemas.ToListAsync());
        }

        // GET: CinemasReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cinema == null)
            {
                return NotFound();
            }

            return View(cinema);
        }

        // GET: CinemasReport/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CinemasReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Logo,Name,Description")] Cinema cinema)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cinema);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        // GET: CinemasReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }
            return View(cinema);
        }

        // POST: CinemasReport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Logo,Name,Description")] Cinema cinema)
        {
            if (id != cinema.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cinema);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CinemaExists(cinema.Id))
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
            return View(cinema);
        }

        // GET: CinemasReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cinema == null)
            {
                return NotFound();
            }

            return View(cinema);
        }

        // POST: CinemasReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema != null)
            {
                _context.Cinemas.Remove(cinema);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Generate PDF report for cinemas
        public async Task<IActionResult> DownloadPdf()
        {
            var cinemas = await _context.Cinemas.ToListAsync();

            // Create a new PDF document
            var pdfDoc = new PdfDocument(new PdfWriter("CinemasReport.pdf"));
            var document = new Document(pdfDoc);

            // Add title with styling
            Paragraph title = new Paragraph("Cinemas Report")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20);
            document.Add(title);

            // Create table for cinemas' information
            Table table = new Table(3).UseAllAvailableWidth();

            // Add table header
            table.AddHeaderCell("Logo");
            table.AddHeaderCell("Name");
            table.AddHeaderCell("Description");

            // Set fixed height for cells
            float cellHeight = 100f; // Adjust height as needed

            // Add cinemas' information to the table
            foreach (var cinema in cinemas)
            {
                // Add logo
                var logoCell = new Cell().SetHeight(cellHeight);
                if (!string.IsNullOrEmpty(cinema.Logo))
                {
                    try
                    {
                        // Fetch the logo using HttpClient
                        byte[] imageData = await new HttpClient().GetByteArrayAsync(cinema.Logo);
                        var logo = ImageDataFactory.Create(imageData);
                        logoCell.Add(new Image(logo).SetAutoScale(true));
                    }
                    catch (HttpRequestException ex)
                    {
                        // Log the error or handle it as needed
                        Console.WriteLine($"Error fetching logo for cinema {cinema.Id}: {ex.Message}");
                    }
                }

                // Add name
                var nameCell = new Cell().SetHeight(cellHeight).Add(new Paragraph(cinema.Name));

                // Add description
                var descriptionCell = new Cell().SetHeight(cellHeight).Add(new Paragraph(cinema.Description));

                // Add cells to the table
                table.AddCell(logoCell);
                table.AddCell(nameCell);
                table.AddCell(descriptionCell);
            }

            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return a file result for downloading
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync("CinemasReport.pdf");
            return File(fileBytes, "application/pdf", "CinemasReport.pdf");
        }

        private bool CinemaExists(int id)
        {
            return _context.Cinemas.Any(e => e.Id == id);
        }
    }
}
