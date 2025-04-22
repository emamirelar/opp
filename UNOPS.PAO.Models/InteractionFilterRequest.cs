// Define the request model that extends PaginationRequest
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;

public class InteractionFilterRequest : PaginationRequest
    {
        public int? ContactId { get; set; }
        public InteractionType? Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchText { get; set; }
    }
