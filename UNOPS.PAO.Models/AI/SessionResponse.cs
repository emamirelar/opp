namespace UNOPS.PAO.Models.AI;

using System;

public class SessionResponse
{
    public string id { get; set; }
    public string app_name { get; set; }
    public string user_id { get; set; }
    public object state { get; set; }
    public object[] events { get; set; }
    public double last_update_time { get; set; }
}