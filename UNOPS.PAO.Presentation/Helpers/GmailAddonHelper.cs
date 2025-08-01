using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Presentation.Helpers;

public class GmailAddonHelper
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IManagerWrapper _managerWrapper;
    private readonly IContactManager _contactManager;
    private readonly IPartnerManager _partnerManager;
    private readonly IUserDataManager _userDataManager;
    private readonly IInteractionManager _interactionManager;
    private readonly IUserInfoService _userInfoService;
    private readonly ILogger<GmailAddonHelper> _logger;

    public GmailAddonHelper(
        IConfiguration configuration, 
        IHttpContextAccessor httpContextAccessor,
        IManagerWrapper managerWrapper,
        IUserInfoService userInfoService,
        ILogger<GmailAddonHelper> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _managerWrapper = managerWrapper ?? throw new ArgumentNullException(nameof(managerWrapper));
        _userInfoService = userInfoService ?? throw new ArgumentNullException(nameof(userInfoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Assign individual managers from wrapper for consistency
        _contactManager = _managerWrapper.ContactManager;
        _partnerManager = _managerWrapper.PartnerManager;
        _userDataManager = _managerWrapper.UserDataManager;
        _interactionManager = _managerWrapper.InteractionManager;
    }

    public string GetValidAudienceForCurrentHost()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new ApplicationException("HttpContext is not available. This method can only be called in the context of an HTTP request.");
        }

        string hostUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        string normalizedHostUrl = hostUrl.TrimEnd('/');

        return normalizedHostUrl;
    }

    #region Mapping Methods

    public GmailRelatedContact MapContactToGmailContact(ContactModel contact, bool canRead)
    {
        if (!canRead)
        {
            return new GmailRelatedContact
            {
                EmailAddress = contact.Email,
                CanRead = false
            };
        }

        var gmailContact = new GmailRelatedContact
        {
            Name = $"{contact.Salutation} {contact.FirstName} {contact.MiddleName} {contact.LastName}",
            Title = contact.Title,
            PartnerName = contact.Partner?.Name ?? string.Empty,
            Id = contact.Id,
            EmailAddress = contact.Email,
            Location = !string.IsNullOrEmpty(contact.MailingCity) && !string.IsNullOrEmpty(contact.MailingCountry)
                        ? $"{contact.MailingCity}, {contact.MailingCountry}"
                        : null,
            Phone = contact.Phone,
            ProfilePictureUrl = contact.ProfilePictureUrl,
            CanRead = true
        };

        // Add interactions if available
        if (contact.Interactions != null && contact.Interactions.Any())
        {
            gmailContact.Interactions = MapInteractionsToGmailInteractions(contact.Interactions);
        }

        return gmailContact;
    }

    public GmailRelatedPartner MapPartnerToGmailPartner(PartnerModel partner, bool canRead)
    {
        if (!canRead)
        {
            return new GmailRelatedPartner
            {
                Name = partner.Name,
                CanRead = false
            };
        }

        var currentPartner = new GmailRelatedPartner
        {
            Id = partner.Id,
            Name = partner.Name,
            PartnerCode = partner.PartnerCode,
            Phone = partner.Phone,
            LogoUrl = partner.LogoUrl,
            Location = !string.IsNullOrEmpty(partner.Address1City) && !string.IsNullOrEmpty(partner.Address1Country)
                        ? $"{partner.Address1City}, {partner.Address1Country}"
                        : null,
            CanRead = true,
            Contacts = new List<GmailRelatedContact>()
        };

        // Add partner interactions if available
        if (partner.Interactions != null && partner.Interactions.Any())
        {
            currentPartner.Interactions = MapInteractionsToGmailInteractions(partner.Interactions);
        }

        // Add partner contacts
        if (partner.Contacts != null && partner.Contacts.Any())
        {
            foreach (ContactModel? contact in partner.Contacts)
            {
                if (contact != null)
                {
                    var gmailContact = new GmailRelatedContact
                    {
                        Name = $"{contact.Salutation} {contact.FirstName} {contact.MiddleName} {contact.LastName}",
                        Title = contact.Title,
                        Id = contact.Id,
                        EmailAddress = contact.Email,
                        CanRead = contact.Permissions.CanRead
                    };
                    currentPartner.Contacts.Add(gmailContact);
                }
            }
        }

        return currentPartner;
    }

    public GmailRelatedUser MapUserToGmailUser(PAOUserModel user, UserInfo? userInfo = null)
    {
        // Use enhanced data from UserInfo if available, otherwise fallback to PAOUser data
        var name = userInfo?.Name ?? user.Email ?? user.Id.ToString();
        var orgUnit = userInfo?.OrgUnit ?? "Unknown";

        return new GmailRelatedUser
        {
            Id = user.Id,
            Name = name,
            Email = user.Email,
            OrgUnit = orgUnit, 
            CanRead = true // Assuming all users can be read for now
        };
    }

    public List<GmailRelatedInteraction> MapInteractionsToGmailInteractions(IEnumerable<InteractionModel> interactions)
    {
        return interactions
            .Where(i => i.Permissions.CanRead)
            .Select(i => new GmailRelatedInteraction
            {
                Id = i.Id,
                Type = i.Type.ToString(),
                Description = i.Description,
                Date = i.Date,
                CanRead = i.Permissions.CanRead
            }).ToList();
    }

    #endregion

    #region Processing Methods

    public async Task<List<int>> ProcessContactsAsync(GmailRelatedRecordsRequest input, GmailRelatedRecordsResponse response, List<string> unmatchedEmailStrings, ClaimsPrincipal user)
    {
        var contactPartnerIds = new List<int>();
        var contacts = await _contactManager.GetContactsForGmailAddon(input, user);
        
        if (contacts != null && contacts.Any())
        {
            foreach (ContactModel? contact in contacts)
            {
                if (contact != null)
                {
                    var gmailContact = MapContactToGmailContact(contact, contact.Permissions.CanRead);
                    response.Contacts.Add(gmailContact);
                    
                    if (contact.Partner != null && !contactPartnerIds.Contains(contact.Partner.Id))
                    {
                        contactPartnerIds.Add(contact.Partner.Id);
                    }

                    // Remove the contact's email from unmatched emails
                    // Find the original email that matched this contact (case-insensitive)
                    var matchedEmail = input.EmailAddresses.FirstOrDefault(email =>
                        string.Equals(email, contact.Email, StringComparison.OrdinalIgnoreCase));
                    if (matchedEmail != null)
                    {
                        unmatchedEmailStrings.Remove(matchedEmail);
                    }
                }
            }
        }

        return contactPartnerIds;
    }

    public async Task ProcessPartnersAsync(GmailRelatedRecordsRequest input, GmailRelatedRecordsResponse response, ClaimsPrincipal user)
    {
        var partners = await _partnerManager.GetPartnersForGmailAddon(input, user);
        
        if (partners != null && partners.Any())
        {
            foreach (PartnerModel? partner in partners)
            {
                if (partner != null)
                {
                    var gmailPartner = MapPartnerToGmailPartner(partner, partner.Permissions.CanRead);
                    response.Partners.Add(gmailPartner);
                }
            }
        }
    }

    public async Task ProcessUsersAsync(GmailRelatedRecordsRequest input, GmailRelatedRecordsResponse response, List<string> unmatchedEmailStrings)
    {
        try
        {
            // Bulk lookup users by email addresses for efficiency
            var users = await _userDataManager.GetUsersByEmailsAsync(input.EmailAddresses);
            
            if (users.Any())
            {
                // Get the email addresses of found users for additional UserInfo lookup
                var foundUserEmails = users.Where(u => !string.IsNullOrEmpty(u.Email))
                                          .Select(u => u.Email)
                                          .ToList();

                // Get additional details from UserInfoService for the found users
                Dictionary<string, UserInfo> userInfoLookup = new Dictionary<string, UserInfo>(StringComparer.OrdinalIgnoreCase);

                var userInfos = await _userInfoService.GetUserInfosByEmailsAsync(foundUserEmails);

                if(userInfos != null && userInfos.Any()) { 
                    // Handle potential duplicate emails by taking the first occurrence of each email
                    userInfoLookup = userInfos
                        .GroupBy(ui => ui.UserEmail, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            group => group.Key, 
                            group => group.First(), 
                            StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    _logger.LogWarning("Failed to get additional user info details");
                }

                // Process each user with enhanced data
                foreach (var user in users)
                {
                    UserInfo? userInfo = null;
                    if (!string.IsNullOrEmpty(user.Email) && userInfoLookup.ContainsKey(user.Email))
                    {
                        userInfo = userInfoLookup[user.Email];
                    }

                    var gmailUser = MapUserToGmailUser(user, userInfo);
                    response.Users.Add(gmailUser);
                    
                    // Remove the user's email from unmatched emails
                    // Find the original email that matched this user (case-insensitive)
                    var matchedEmail = input.EmailAddresses.FirstOrDefault(email => 
                        string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase));
                    if (matchedEmail != null)
                    {
                        unmatchedEmailStrings.Remove(matchedEmail);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error looking up users by emails: {ex.Message}");
            // Continue processing without users if bulk lookup fails
        }
    }

    public async Task ProcessUnmatchedEmailsAsync(List<string> unmatchedEmailStrings, GmailRelatedRecordsResponse response, ClaimsPrincipal user)
    {
        response.UnmatchedEmails = await _contactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(unmatchedEmailStrings, user);
    }

    #endregion

    #region Utility Methods

    public (string FirstName, string LastName) ExtractNameFromEmail(string emailPrefix)
    {
        // Simple name extraction logic - can be enhanced
        var parts = emailPrefix.Split(new char[] { '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
        {
            return (
                FirstName: char.ToUpper(parts[0][0]) + parts[0].Substring(1).ToLower(),
                LastName: char.ToUpper(parts[1][0]) + parts[1].Substring(1).ToLower()
            );
        }
        else if (parts.Length == 1)
        {
            return (
                FirstName: char.ToUpper(parts[0][0]) + parts[0].Substring(1).ToLower(),
                LastName: ""
            );
        }

        return (FirstName: emailPrefix, LastName: "");
    }

    #endregion
}
