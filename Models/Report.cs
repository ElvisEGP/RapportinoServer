using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RapportinoServer.Models
{
    public class Report
    {
        // Primary key aligned with DB (INT IDENTITY)
        public int Id { get; set; }

        // Data do relatório (SQL DATE) — sua propriedade original
        [Required]
        public DateOnly Data { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        // Compatibilidade: páginas esperam ReportDate
        public DateOnly ReportDate
        {
            get => Data;
            set => Data = value;
        }

        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string Serial { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public string ReportDescription { get; set; } = string.Empty;

        // Compatibilidade: páginas podem usar Description
        public string Description
        {
            get => ReportDescription;
            set => ReportDescription = value;
        }

        [Required]
        [MinLength(1)]
        [MaxLength(200)]
        public string TechnicianName { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        [MaxLength(200)]
        public string ServiceType { get; set; } = string.Empty;

        public string? ChangedMaterial { get; set; }
        public string? OrderedMaterial { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Checkbox: Macchina con tutti dispositivi di sicurezza presente e funzionante
        /// </summary>
        public bool SafetyDevicesCheck { get; set; } = true;

        // Coleção de logs de trabalho
        public List<ReportWorkLog> WorkLogs { get; set; } = new();

        // Compatibilidade: páginas esperam ClientId / MachineId / Title
        // Adicionamos propriedades simples; ajuste conforme seu DB/schema se necessário.
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public int? MachineId { get; set; }

        // Title não existia no seu modelo original — deixamos como campo livre.
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
    }
}
