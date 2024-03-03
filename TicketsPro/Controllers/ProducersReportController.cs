using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eTickets.Data;
using eTickets.Models;
using iText.IO.Image;
using iText.Layout.Properties;

namespace TicketsPro.Controllers
{
    public class ProducersReportController : Controller
    {
        private readonly AppDbContext _context;

        public ProducersReportController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ProducersReport
        public async Task<IActionResult> Index()
        {
            return View(await _context.Producers.ToListAsync());
        }

        // GET: ProducersReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producer = await _context.Producers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }

            return View(producer);
        }

        // GET: ProducersReport/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProducersReport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProfilePictureURL,FullName,Bio")] Producer producer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(producer);
        }

        // GET: ProducersReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producer = await _context.Producers.FindAsync(id);
            if (producer == null)
            {
                return NotFound();
            }
            return View(producer);
        }

        // POST: ProducersReport/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProfilePictureURL,FullName,Bio")] Producer producer)
        {
            if (id != producer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProducerExists(producer.Id))
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
            return View(producer);
        }

        // GET: ProducersReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producer = await _context.Producers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }

            return View(producer);
        }

        // POST: ProducersReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producer = await _context.Producers.FindAsync(id);
            if (producer != null)
            {
                _context.Producers.Remove(producer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Action to download PDF
        public async Task<IActionResult> DownloadPdf()
        {
            var producers = await _context.Producers.ToListAsync();

            // Create a new HTTP client with a proper User-Agent header
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "YourCustomUserAgentString");

            // Create a new PDF document
            var pdfDoc = new PdfDocument(new PdfWriter("ProducersReport.pdf"));
            var document = new Document(pdfDoc);

            // Add title with styling
            Paragraph title = new Paragraph("Producers Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20);
            document.Add(title);

            // Create table for producers' information
            Table table = new Table(3).UseAllAvailableWidth();

            // Add table header
            table.AddHeaderCell("Profile Picture");
            table.AddHeaderCell("Full Name");
            table.AddHeaderCell("Bio");

            // Set fixed height for cells
            float cellHeight = 62f; // Adjust height as needed
            float pictureWidth = 1f; // Adjust width as needed

            // Add producers' information to the table
            foreach (var producer in producers)
            {
                // Add profile picture
                var profilePicCell = new Cell().SetHeight(cellHeight).SetWidth(pictureWidth);

                try
                {
                    // Fetch the profile picture using HttpClient
                    byte[] imageData = await httpClient.GetByteArrayAsync(producer.ProfilePictureURL);
                    var profilePic = ImageDataFactory.Create(imageData);
                    profilePicCell.Add(new Image(profilePic).SetAutoScale(true));
                }
                catch (HttpRequestException ex)
                {
                    // Log the error or handle it as needed
                    Console.WriteLine($"Error fetching profile picture for producer {producer.Id}: {ex.Message}");
                }

                // Add full name
                var fullNameCell = new Cell().SetHeight(cellHeight).Add(new Paragraph(producer.FullName));

                // Add bio
                var bioCell = new Cell().SetHeight(cellHeight).SetWidth(100f).Add(new Paragraph(producer.Bio));

                // Add cells to the table
                table.AddCell(profilePicCell);
                table.AddCell(fullNameCell);
                table.AddCell(bioCell);
            }

            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return a file result for downloading
            byte[] fileBytes = System.IO.File.ReadAllBytes("ProducersReport.pdf");
            return File(fileBytes, "application/pdf", "ProducersReport.pdf");
        }

        private bool ProducerExists(int id)
        {
            return _context.Producers.Any(e => e.Id == id);
        }
    }
}
