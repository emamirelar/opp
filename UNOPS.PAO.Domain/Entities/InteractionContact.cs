using System;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;


namespace UNOPS.PAO.Domain.Entities
{
    public class InteractionContact
    {
        public int InteractionId { get; set; }
        public int ContactId { get; set; }

        [JsonIgnore]
        public virtual Interaction? Interaction { get; set; }
        [JsonIgnore]
        public virtual Contact? Contact { get; set; }
    }
}