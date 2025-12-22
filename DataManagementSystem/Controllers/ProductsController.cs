using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserManagementSystem.Data;
using UserManagementSystem.Models;

namespace UserManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Products
        // Scenario: Admins see ALL. Normal users only see "Public" products.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // 1. Start with all products
            var query = _context.Products.AsQueryable();

            // 2. Security Check: Is the user an Admin?
            // User.IsInRole checks the "claims" inside their auth token/cookie
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                // 3. Filter: If not admin, only show public items
                query = query.Where(p => p.IsPublic == true);
            }

            return await query.ToListAsync();
        }

        // POST: api/Products
        // Scenario: Only logged-in users can create. System auto-assigns the creator.
        [HttpPost]
        [Authorize] // <--- Gate 1: Rejects anyone not logged in (401 Unauthorized)
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            // 1. Get the current user's ID from the "User" object (the claims principal)
            // This is safer than trusting client-side data.
            var userId = _userManager.GetUserId(User);

            // 2. Set the relationship fields
            product.CreatedById = userId;

            // 3. Save to DB
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProducts", new { id = product.Id }, product);
        }
    }
}