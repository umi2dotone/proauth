namespace UserManagementSystem.Models.DTOs
{
    public class RegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? FullName { get; set; }
    }
}