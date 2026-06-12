using System;
using System.ComponentModel.DataAnnotations;

public class ReportWorkLog
{
    // Primary key alinhado ao banco (INT IDENTITY)
    public int Id { get; set; }

    // Foreign key para Report.Id
    [Required]
    public int ReportId { get; set; }

    // Data do trabalho (usar DateOnly para representar DATE no SQL)
    [Required]
    public DateOnly WorkedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    // Duração do trabalho (mapeia para TIME no SQL)
    [Required]
    public TimeSpan WorkedTime { get; set; } = TimeSpan.Zero;

    // Propriedade auxiliar para binding em Blazor (string <-> TimeSpan)
    public string WorkedTimeString
    {
        get => WorkedTime.ToString(@"hh\:mm");
        set
        {
            if (TimeSpan.TryParse(value, out var ts))
            {
                WorkedTime = ts;
            }
            else
            {
                WorkedTime = TimeSpan.Zero;
            }
        }
    }
}