namespace UNOPS.PAO.Models.AI
{
    public class SessionUpdateRequest
    {
        public string SessionId { get; set; } = null!;
    }

    public class SessionStarRequest : SessionUpdateRequest
    {
        public bool Starred { get; set; }
    }

    public class SessionArchiveRequest : SessionUpdateRequest
    {
        public bool Archived { get; set; }
    }

    public class SessionTitleRequest : SessionUpdateRequest
    {
        public string Title { get; set; } = null!;
    }
} 