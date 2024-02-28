using eTickets.Data.Cart;
using eTickets.Data.Services;
using eTickets.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Borders;
using iText.Layout.Properties;

namespace eTickets.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IMoviesService _moviesService;
        private readonly ShoppingCart _shoppingCart;
        private readonly IOrdersService _ordersService;

        public OrdersController(IMoviesService moviesService, ShoppingCart shoppingCart, IOrdersService ordersService)
        {
            _moviesService = moviesService;
            _shoppingCart = shoppingCart;
            _ordersService = ordersService;
        }

        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userRole = User.FindFirstValue(ClaimTypes.Role);

            var orders = await _ordersService.GetOrdersByUserIdAndRoleAsync(userId, userRole);
            return View(orders);
        }

        public IActionResult ShoppingCart()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            var response = new ShoppingCartVM()
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal()
            };

            return View(response);
        }

        public async Task<IActionResult> AddItemToShoppingCart(int id)
        {
            var item = await _moviesService.GetMovieByIdAsync(id);

            if (item != null)
            {
                _shoppingCart.AddItemToCart(item);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        public async Task<IActionResult> RemoveItemFromShoppingCart(int id)
        {
            var item = await _moviesService.GetMovieByIdAsync(id);

            if (item != null)
            {
                _shoppingCart.RemoveItemFromCart(item);
            }
            return RedirectToAction(nameof(ShoppingCart));
        }

        public async Task<IActionResult> CompleteOrder()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userEmailAddress = User.FindFirstValue(ClaimTypes.Email);

            // Calculate total based on shopping cart items
            var total = _shoppingCart.GetShoppingCartTotal().ToString("0.00"); // Remove currency symbol here

            // Additional information for displaying in OrderCompleted.cshtml
            var movieNames = string.Join(", ", items.Select(item => item.Movie.Name));
            var ticketPrice = items.FirstOrDefault()?.Movie.Price.ToString("0.00"); // Remove currency symbol here
            var numberOfTickets = items.Sum(item => item.Amount);

            await _ordersService.StoreOrderAsync(items, userId, userEmailAddress);
            await _shoppingCart.ClearShoppingCartAsync();

            // Pass additional information to the view
            ViewData["UserName"] = User.Identity.Name;
            ViewData["MovieName"] = movieNames;
            ViewData["TicketPrice"] = ticketPrice;
            ViewData["NumberOfTickets"] = numberOfTickets;
            ViewData["Total"] = total; // Use the total calculated above

            return View("OrderCompleted");
        }

        public IActionResult GeneratePDF(string userName, string movieName, string ticketPrice, int numberOfTickets, decimal total)
        {
            var pdfContent = GeneratePDFContent(userName, movieName, ticketPrice, numberOfTickets, total);
            return File(pdfContent, "application/pdf", "OrderDetails.pdf");
        }

        private byte[] GeneratePDFContent(string userName, string movieName, string ticketPrice, int numberOfTickets, decimal total)
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Create a table for ticket information
                var table = new Table(2);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                table.SetBorder(Border.NO_BORDER);

                // Add title row
                var titleCell = new Cell(1, 2)
                    .Add(new Paragraph("Movie Ticket"))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetBold();
                table.AddCell(titleCell);

                // Add user information row
                table.AddCell("User Name:");
                table.AddCell(userName);

                // Add movie information row
                table.AddCell("Movie:");
                table.AddCell(movieName);

                // Add ticket details row
                table.AddCell("Ticket Price:");
                table.AddCell(ticketPrice);
                table.AddCell("Number of Tickets:");
                table.AddCell(numberOfTickets.ToString());
                table.AddCell("Total:");
                table.AddCell(total.ToString());

                // Add table to document
                document.Add(table);

                // Close document
                document.Close();

                return memoryStream.ToArray();
            }
        }


    }
}
