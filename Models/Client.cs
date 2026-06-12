using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using RapportinoServer.Models;

public class Client
{
    /// <summary>
    /// Primary key aligned with the database (INT IDENTITY).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Data column (SQL DATE). Prefer DateOnly for clarity; see mapping notes.
    /// </summary>
    [Required]
    public DateTime Data { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Inserire il nome della Azienda")]
    [MinLength(3, ErrorMessage = "Numero di caratteri non sufficiente per il nome della Azienda")]
    [MaxLength(200)]
    public required string CompanyName { get; set; }

    [Required(ErrorMessage = "Inserire l'indirizzo")]
    [MinLength(3, ErrorMessage = "Numero di caratteri non sufficiente per l'indirizzo")]
    [MaxLength(200)]
    public required string Address { get; set; }

    [Required(ErrorMessage = "Inserire il numero civico")]
    [MinLength(1, ErrorMessage = "Numero di caratteri non sufficiente per il numero civico")]
    [MaxLength(50)]
    public required string NumberAddress { get; set; }

    [Required(ErrorMessage = "Inserire il nome della città")]
    [MinLength(3, ErrorMessage = "Numero di caratteri non sufficiente per il nome della città")]
    [MaxLength(100)]
    public required string City { get; set; }

    [Required(ErrorMessage = "Inserire il codice postale")]
    [MinLength(1, ErrorMessage = "Numero di caratteri non sufficiente per il codice postale")]
    [MaxLength(50)]
    public required string PostalCode { get; set; }

    [Required(ErrorMessage = "Inserire il nome della provincia")]
    [MinLength(2, ErrorMessage = "Numero di caratteri non sufficiente per il nome della provincia")]
    [MaxLength(100)]
    public required string State { get; set; }

    [Required(ErrorMessage = "Inserire il nome del paese")]
    [MinLength(3, ErrorMessage = "Numero di caratteri non sufficiente per il nome del paese")]
    [MaxLength(100)]
    public required string Country { get; set; }

    [Required(ErrorMessage = "Inserire il numero di telefono")]
    [Phone(ErrorMessage = "Formato telefono non valido")]
    [MinLength(5, ErrorMessage = "Numero di caratteri non sufficiente per il numero di telefono")]
    [MaxLength(50)]
    public required string Phone { get; set; }

    [Required(ErrorMessage = "Inserire un indirizzo email valido")]
    [EmailAddress(ErrorMessage = "Formato email non valido")]
    [MaxLength(200)]
    public required string Email { get; set; }

    [Url(ErrorMessage = "Formato URL non valido")]
    [MaxLength(200)]
    public string? Website { get; set; }

    public string? Note { get; set; }
    
    public List<Machine> Machines { get; set; } = new();
}