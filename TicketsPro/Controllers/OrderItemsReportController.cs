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
    public class OrderItemsReportController : Controller
    {
        private readonly AppDbContext _context;

        public OrderItemsReportController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderItemsReport
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OrderItems.Include(o => o.Movie).Include(o => o.Order);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OrderItemsReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Movie)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // GET: OrderItemsReport/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Id");
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
        }

        // POST: OrderItemsReport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Price,MovieId,OrderId")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Id", orderItem.MovieId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItemsReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Id", orderItem.MovieId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            return View(orderItem);
        }

        // POST: OrderItemsReport/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Price,MovieId,OrderId")] OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItem.Id))
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
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Id", orderItem.MovieId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItemsReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Movie)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItemsReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Action to download PDF for order items
        public async Task<IActionResult> DownloadPdf()
        {
            var orderItems = await _context.OrderItems
                .Include(o => o.Movie)
                .Include(o => o.Order)
                .ToListAsync();

            // Create a new PDF document
            var pdfDoc = new PdfDocument(new PdfWriter("OrderItemsReport.pdf"));
            var document = new Document(pdfDoc);

            // Add title with styling
            Paragraph title = new Paragraph("Order Items Report")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20);
            document.Add(title);

            // Create table for order items' information
            Table table = new Table(4).UseAllAvailableWidth();

            // Add table header
            table.AddHeaderCell("Amount");
            table.AddHeaderCell("Price");
            table.AddHeaderCell("Movie");
            table.AddHeaderCell("Order");

            // Add order items' information to the table
            foreach (var orderItem in orderItems)
            {
                table.AddCell(orderItem.Amount.ToString());
                table.AddCell(orderItem.Price.ToString());
                table.AddCell(orderItem.Movie?.Name ?? "N/A"); // Assuming Movie has a Name property
                table.AddCell(orderItem.Order?.Id.ToString() ?? "N/A"); // Assuming Order has an Id property
            }

            // Add the table to the document
            document.Add(table);

            // Close the document
            document.Close();

            // Return a file result for downloading
            byte[] fileBytes = System.IO.File.ReadAllBytes("OrderItemsReport.pdf");
            return File(fileBytes, "application/pdf", "OrderItemsReport.pdf");
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}
