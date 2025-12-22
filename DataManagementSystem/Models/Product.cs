using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsPublic { get; set; }

        // --- NEW CODE STARTS HERE ---

        // 1. The Foreign Key: This creates the actual column in the DB table
        public string? CreatedById { get; set; }

        // 2. The Navigation Property: This lets EF Core link the tables
        // "ForeignKey" tells EF that 'CreatedById' is the link to this object
        [ForeignKey("CreatedById")]
        public ApplicationUser? CreatedBy { get; set; }
    }
}