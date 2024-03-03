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
using iText.IO.Image;

namespace TicketsPro.Controllers
{
    public class ActorsReportController : Controller
    {
        private readonly AppDbContext _context;

        public ActorsReportController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ActorsReport
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: ActorsReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // GET: ActorsReport/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ActorsReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProfilePictureURL,FullName,Bio")] Actor actor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: ActorsReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: ActorsReport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProfilePictureURL,FullName,Bio")] Actor actor)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: ActorsReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: ActorsReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }

        // Action to download PDF
        public async Task<IActionResult> DownloadPdf()
        {
            var actors = await _context.Actors.ToListAsync();

            // Create a new HTTP client with a proper User-Agent header
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "YourCustomUserAgentString");

            // Create a new PDF document
            var pdfDoc = new PdfDocument(new PdfWriter("ActorsReport.pdf"));
            var document = new Document(pdfDoc);

            // Add title with styling
            Paragraph title = new Paragraph("Actors Report")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20);
            document.Add(title);

            // Create table for actors' information
            Table table = new Table(3).UseAllAvailableWidth();

            // Add table header
            table.AddHeaderCell("Profile Picture");
            table.AddHeaderCell("Full Name");
            table.AddHeaderCell("Bio");

            // Set fixed height for cells
            float cellHeight = 62f; // Adjust height as needed
            float pictureWidth = 1f; // Adjust width as needed

            // Add actors' information to the table
            foreach (var actor in actors)
            {
                // Add profile picture
                var profilePicCell = new Cell().SetHeight(cellHeight).SetWidth(pictureWidth);

                try
                {
                    // Fetch the profile picture using HttpClient
                    byte[] imageData = await httpClient.GetByteArrayAsync(actor.ProfilePictureURL);
                    var profilePic = ImageDataFactory.Create(imageData);
                    profilePicCell.Add(new Image(profilePic).SetAutoScale(true));
                }
                catch (HttpRequestException ex)
                {
                    // Log the error or handle it as needed
                    Console.WriteLine($"Error fetching profile picture for actor {actor.Id}: {ex.Message}");
                }

                // Add full name
                var fullNameCell = new Cell().SetHeight(cellHeight).Add(new Paragraph(actor.FullName));

                // Add bio
                var bioCell = new Cell().SetHeight(cellHeight).SetWidth(100f).Add(new Paragraph(actor.Bio));

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
            byte[] fileBytes = System.IO.File.ReadAllBytes("ActorsReport.pdf");
            return File(fileBytes, "application/pdf", "ActorsReport.pdf");
        }





    }
}
