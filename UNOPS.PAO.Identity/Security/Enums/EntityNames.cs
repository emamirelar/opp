namespace UNOPS.PAO.Identity.Security.Enums
{
    public static class EntityNames
    {
        public const string Contact = nameof(Contact);
        public const string Partner = nameof(Partner);
        public const string Interaction = nameof(Interaction);
        public const string PartnerTree = nameof(PartnerTree);
        public const string Opportunity = nameof(Opportunity);

        public static string ByName(string name) => name switch
        {
            "contact" => Contact,
            "Contact" => Contact,
            "partner" => Partner,
            "Partner" => Partner,
            "interaction" => Interaction,
            "Interaction" => Interaction,
            "partnerTree" => PartnerTree,
            "PartnerTree" => PartnerTree,
            "opportunity" => Opportunity,
            "Opportunity" => Opportunity,
            _ => string.Empty
        };
    }
}
