namespace UNOPS.PAO.Models.AI;

using System;

public class SessionResponse
{
    public string id { get; set; } = null!;
    public string app_name { get; set; } = null!;
    public string user_id { get; set; } = null!;
    public object state { get; set; } = null!;
    public object[] events { get; set; } = null!;
    public double last_update_time { get; set; }
}