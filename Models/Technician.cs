using System.ComponentModel.DataAnnotations;

public class Technician
{
    // Primary key alinhado ao banco INT IDENTITY
    public int Id { get; set; }

    [Required(ErrorMessage = "Inserire il nome del tecnico")]
    [MinLength(3, ErrorMessage = "Numero di caratteri non sufficiente per il nome del tecnico")]
    [MaxLength(200)]
    public string TechnicianName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Surname { get; set; }

    [MaxLength(200)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Inserire la password")]
    [MinLength(4, ErrorMessage = "La password deve avere almeno 4 caratteri")]
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Role { get; set; }
}