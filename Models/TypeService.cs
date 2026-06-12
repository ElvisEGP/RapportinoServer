using System;

namespace RapportinoServer.Models
{
    public class TypeService
    {
        // Chave primária (INT IDENTITY)
        public int Id { get; set; }

        // Nome do tipo de serviço — propriedade principal
        public string ServiceName { get; set; } = string.Empty;

        // Compatibilidade: algumas páginas usam "Name"
        public string Name
        {
            get => ServiceName;
            set => ServiceName = value;
        }

        // Descrição opcional
        public string? Description { get; set; }
    }
}