using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using System.Text.Json;
using UNOPS.PAO.Models.LiaisonOffices;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Models.Links;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Locations;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Notifications;
using UNOPS.PAO.Models.Values;
using UNOPS.PAO.Models.UNCF;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Add mappings here
        CreateMap<PartnerTreeDataModel, PartnerTree>().ReverseMap();
        CreateMap<PartnerTree, PartnerTreeModel>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));
        CreateMap<PartnerTree, PartnerTreeDataModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)); // Ensure Status is mapped
        CreateMap<Link, LinkModel>();
        CreateMap<LinkRequest, Link>();
        CreateMap<UpdateLinkRequest, Link>();
        CreateMap<InteractionContact, InteractionContactModel>();
        CreateMap<InteractionContactModel, InteractionContact>();
        CreateMap<InteractionPartner, InteractionPartnerModel>();
        CreateMap<InteractionPartnerModel, InteractionPartner>();
        CreateMap<InteractionUser, InteractionUserModel>();
        CreateMap<InteractionUserModel, InteractionUser>();

        CreateMap<Partner, PartnerValueModel>();

        // Value entity mappings
        CreateMap<Currency, CurrencyModel>();
        CreateMap<EligibleEntity, EligibleEntityModel>();
        // Country mapping moved to CountryMappingProfile
        CreateMap<Contact, ContactValueModel>();
        CreateMap<PAOUser, UserValueModel>();
        CreateMap<LiaisonOffice, LiaisonOfficeModel>();

        // Proposed Initiative Type and Output mappings
        CreateMap<ProposedInitiativeType, SimpleValueModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        CreateMap<Output, OutputModel>();

        // SDG mappings
        CreateMap<SDG, SDGModel>();

        // UNCF mappings
        CreateMap<UNCFOutcome,UNCFOutcomeModel>()
            .ForMember(dest => dest.UNCFOutcomeExternalId, opt => opt.MapFrom(src => src.UNCFOutcomeId))
            .ForMember(dest => dest.VersionNo, opt => opt.MapFrom(src => src.UNCooperationFrameworkVersionNo));
        CreateMap<UNCFIndicator, UNCFIndicatorModel>()
            .ForMember(dest => dest.UNCFIndicatorExternalId, opt => opt.MapFrom(src => src.UNCFIndicatorId))
            .ForMember(dest => dest.VersionNo, opt => opt.MapFrom(src => src.UNCooperationFrameworkVersionNo));

        // OrganizationHierarchy mappings
        CreateMap<OrganizationHierarchy, OrganizationHierarchyModel>().ReverseMap();
        CreateMap<OrganizationHierarchy, OrganizationHierarchyTreeModel>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));
        CreateMap<OrganizationHierarchy, OrganizationHierarchyDataModel>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

        CreateMap<Notification, NotificationModel>()
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.ResponseType, opt => opt.MapFrom(src => src.ResponseType))
            .ForMember(dest => dest.Records, opt => opt.MapFrom(src => 
                JsonSerializer.Deserialize<List<object>>(src.RecordData, new JsonSerializerOptions()) ?? new List<object>()));
    }
}
