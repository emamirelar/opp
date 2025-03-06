using System;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities
{
    public class Interaction : ModifiableDeletableEntity
    {
        public int Id { get; set; }
        
        public InteractionType Type { get; set; }
        
        public DateTime Date { get; set; }
        
        public byte[]? Data { get; set; }
        
        public required int ContactId { get; set; }
        
        public required virtual Contact Contact { get; set; } = null!;

    }
}