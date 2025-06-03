namespace UNOPS.PAO.Identity.Security.Enums
{
    public static class EntityNames
    {
        public const string Contact = nameof(Contact);
        public const string Partner = nameof(Partner);
        public const string Interaction = nameof(Interaction);
        public const string PartnerTree = nameof(PartnerTree);

        public static string ByName(string name) => name switch
        {
            "contact" => Contact,
            "partner" => Partner,
            "interaction" => Interaction,
            "partnerTree" => PartnerTree,
            _ => string.Empty
        };
    }
}
