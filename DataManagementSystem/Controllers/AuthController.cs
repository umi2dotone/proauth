using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagementSystem.Models;
using UserManagementSystem.Models.DTOs;

namespace UserManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        // The Bouncer (Manages signing in/out)
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Constructor Injection: Asking the Service Container for tools
        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // 1. REGISTER USER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("User already exists.");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            // This hashes the password and saves to SQLite
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Optional: Default every new user to "User" role
                // await _userManager.AddToRoleAsync(user, "User");

                return Ok(new { message = "User registered successfully!" });
            }

            return BadRequest(result.Errors);
        }

        // 2. SETUP ROLES (Run this once to seed the DB)
        [HttpPost("setup-roles")]
        public async Task<IActionResult> SetupRoles()
        {
            // List of roles we want
            string[] roles = { "Admin", "User", "Manager" };

            foreach (var role in roles)
            {
                // Check if role exists in DB, if not, create it
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            return Ok("Roles generated: Admin, User, Manager");
        }

        // 3. ASSIGN ROLE (Promote a user)
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(string email, string roleName)
        {
            // Find the user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            // Check if the role actually exists
            if (!await _roleManager.RoleExistsAsync(roleName))
                return BadRequest($"Role '{roleName}' does not exist.");

            // Assign the role
            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return Ok($"User {email} added to role {roleName}");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            // The Bouncer checks the ID
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Ok("Welcome back!");
            }
            return Unauthorized("Invalid login attempt.");
        }

    }
}