namespace UNOPS.PAO.UNOPSBusiness.Models;

using System;

public class MyPubSubMessage
{
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    public string Content { get; set; }
}