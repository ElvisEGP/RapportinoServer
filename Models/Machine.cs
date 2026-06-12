using System.ComponentModel.DataAnnotations;

namespace RapportinoServer.Models
{
    public sealed class Machine
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        [StringLength(200)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Serial { get; set; } = string.Empty;
    }
}