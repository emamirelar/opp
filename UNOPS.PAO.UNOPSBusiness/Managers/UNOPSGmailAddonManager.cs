using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Integrations;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSGmailAddonManager : BaseUNOPSManager, IGmailAddonManager
{
    private readonly IContactManager _contactManager;
    private readonly IPartnerManager _partnerManager;
    private readonly IUserDataManager _userDataManager;
    private readonly IInteractionManager _interactionManager;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserInfoService _userInfoService;
    private readonly ILogger<UNOPSGmailAddonManager>? _logger;
    private readonly NotificationManager _notificationManager;

    public UNOPSGmailAddonManager(IMapper mapper, UNOPSAppDbContext context,
        IContactManager contactManager,
        IPartnerManager partnerManager,
        IUserDataManager userDataManager,
        IInteractionManager interactionManager,
        IPermissionService permissionService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IUserInfoService userInfoService,
        ILogger<UNOPSGmailAddonManager> logger,
        NotificationManager notificationManager)
        : base(mapper, context, configuration, null, null, permissionService, httpContextAccessor)
    {
        _contactManager = contactManager;
        _partnerManager = partnerManager;
        _userDataManager = userDataManager;
        _interactionManager = interactionManager;
        _permissionService = permissionService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _userInfoService = userInfoService;
        _logger = logger;
        _notificationManager = notificationManager;
    }

    public async Task<GmailRelatedRecordsResponse> FindRelatedRecordsAsync(GmailRelatedRecordsRequest input, ClaimsPrincipal user)
    {
        try
        {
            var response = new GmailRelatedRecordsResponse();
            var unmatchedEmailStrings = new List<string>(input.EmailAddresses);

            // Initialize permissions
            await InitializeResponsePermissionsAsync(response, user);

            // Process contacts and get their associated partner IDs
            var contactPartnerIds = await ProcessContactsAsync(input, response, unmatchedEmailStrings, user);

            // Process partners using the contact partner IDs
            input.partnerIds = contactPartnerIds;
            await ProcessPartnersAsync(input, response, user);

            // Process users and update unmatched emails
            await ProcessUsersAsync(input, response, unmatchedEmailStrings);

            // Process unmatched emails
            await ProcessUnmatchedEmailsAsync(unmatchedEmailStrings, response, user);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding related records for Gmail addon");
            throw;
        }
    }

    private async Task<bool> InitializeResponsePermissionsAsync(GmailRelatedRecordsResponse response, ClaimsPrincipal user)
    {
        var contactCreatePermissionResult = await _permissionService.CanPerformActionAsync("Contact", "create", user);
        response.CanCreateContacts = contactCreatePermissionResult;
        var partnerCreatePermissionResult = await _permissionService.CanPerformActionAsync("Partner", "create", user);
        response.CanCreatePartners = partnerCreatePermissionResult;
        var interactionCreatePermissionResult = await _permissionService.CanPerformActionAsync("Interaction", "create", user);
        response.CanCreateInteractions = interactionCreatePermissionResult;
        return true;
    }

    public async Task<GmailCreateRecordsResult> CreateRecordsFromEmailsAsync(GmailCreateRecordsRequest request, ClaimsPrincipal user)
    {
        try
        {
            // Validate request and permissions
            await ValidateCreateRecordsRequestAsync(request, user);

            // Initialize state tracking
            var state = InitializeCreationState();

            // Process partners (create or find existing ones)
            await ProcessPartnersForCreationAsync(request, user, state);

            // Process contacts (create new ones, skip existing)
            await ProcessContactsForCreationAsync(request, user, state);

            // Update existing interaction with new contact and partner IDs
            await UpdateExistingInteractionAsync(request, user, state);

            // Send in-app notifications for created records
            await SendCreationNotificationsAsync(user, state);

            // Build and return the result
            return BuildCreateRecordsResult(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contacts from emails");
            throw;
        }
    }

    #region CreateRecordsFromEmailsAsync Helper Methods

    /// <summary>
    /// State tracking class for the creation process
    /// </summary>
    private class CreationState
    {
        public List<ContactModel> CreatedContacts { get; set; } = new List<ContactModel>();
        public List<string> FailedEmails { get; set; } = new List<string>();
        public Dictionary<string, int> CreatedPartners { get; set; } = new Dictionary<string, int>(); // Track created partners by name
        public List<ContactModel> ExistingContacts { get; set; } = new List<ContactModel>(); // Track existing contacts that were found
        public int NewPartnersCreated { get; set; } = 0; // Track truly new partners created
    }

    /// <summary>
    /// Validates the create records request and user permissions
    /// </summary>
    private async Task ValidateCreateRecordsRequestAsync(GmailCreateRecordsRequest request, ClaimsPrincipal user)
        {
            if (request.SelectedContacts == null || !request.SelectedContacts.Any())
            {
                throw new ArgumentException("No emails selected for contact creation");
            }

            // Check permissions
        var contactCreatePermissionResult = await _permissionService.CanPerformActionAsync("Contact", "create", user);
        var partnerCreatePermissionResult = await _permissionService.CanPerformActionAsync("Partner", "create", user);

        if (!contactCreatePermissionResult || !partnerCreatePermissionResult)
            {
                throw new UnauthorizedAccessException("User does not have necessary permission to create");
        }
    }

    /// <summary>
    /// Initializes the state tracking for the creation process
    /// </summary>
    private CreationState InitializeCreationState()
    {
        return new CreationState();
    }

    /// <summary>
    /// Processes partners - creates new ones or finds existing ones
    /// </summary>
    private async Task ProcessPartnersForCreationAsync(GmailCreateRecordsRequest request, ClaimsPrincipal user, CreationState state)
    {
        var partnersToCreate = GetPartnersToCreate(request);
        string currentPartnerName = string.Empty;
        foreach (var partnerGroup in partnersToCreate)
        {
            try
            {
                currentPartnerName = partnerGroup.PartnerName;
                await ProcessSinglePartnerAsync(user, partnerGroup.PartnerName, state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create partner {PartnerName}", currentPartnerName);
                var failedEmailsForPartner = GetFailedEmailsForPartner(request, partnerGroup.PartnerName);
                state.FailedEmails.AddRange(failedEmailsForPartner);
            }
        }
    }

    /// <summary>
    /// Processes contacts - creates new ones and skips existing ones
    /// </summary>
    private async Task ProcessContactsForCreationAsync(GmailCreateRecordsRequest request, ClaimsPrincipal user, CreationState state)
    {
        foreach (var selectedEmail in request.SelectedContacts)
        {
            try
            {
                await ProcessSingleContactAsync(selectedEmail, user, state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create contact for email {EmailAddress}", selectedEmail.EmailAddress);
                state.FailedEmails.Add(selectedEmail.EmailAddress);
            }
        }
    }

    /// <summary>
    /// Gets the unique partners that need to be created
    /// </summary>
    private IEnumerable<dynamic> GetPartnersToCreate(GmailCreateRecordsRequest request)
    {
        return request.SelectedContacts
                .Where(email => !email.PartnerId.HasValue)
                .Select(email => new
                {
                    Email = email,
                    PartnerName = GetPartnerNameFromEmail(email)
                })
                .GroupBy(x => x.PartnerName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { PartnerName = g.Key, FirstEmail = g.First() });
    }

    /// <summary>
    /// Gets partner name from email - uses provided name or generates from email domain
    /// </summary>
    private string GetPartnerNameFromEmail(GmailSelectedEmailModel email)
    {
        if (!string.IsNullOrEmpty(email.PartnerName))
        {
            return email.PartnerName;
        }

        var domain = email.EmailAddress.Split('@')[1];
        return $"{char.ToUpper(domain[0])}{domain.Substring(1)}";
    }

    /// <summary>
    /// Processes a single partner - creates or finds existing
    /// </summary>
    private async Task ProcessSinglePartnerAsync(ClaimsPrincipal user, string partnerName, CreationState state)
    {
        // Check if partner already exists by name
        var existingPartner = await _partnerManager.GetPartnerByNameAsync(user, partnerName);
        if (existingPartner != null)
        {
            _logger.LogInformation("Partner '{PartnerName}' already exists with ID {PartnerId}, using existing partner", partnerName, existingPartner.Id);
            state.CreatedPartners[partnerName] = existingPartner.Id;
            return;
        }

        // Create new partner
        var partnerRequest = CreatePartnerRequest(partnerName);
        var createdPartner = await _partnerManager.CreatePartnerAsync(user, partnerRequest);
        
        if (createdPartner != null)
        {
            state.CreatedPartners[partnerName] = createdPartner.Id;
            state.NewPartnersCreated++;
            _logger.LogInformation("Created new partner '{PartnerName}' with ID {PartnerId}", partnerName, createdPartner.Id);
        }
    }

    /// <summary>
    /// Creates a partner request with default values
    /// </summary>
    private PartnerRequest CreatePartnerRequest(string partnerName)
    {
        return new PartnerRequest
        {
            Name = partnerName,
                        UNAndStateEntity = false,
                        Status = EntityStatus.Draft.ToString(),
                        CanCreateNewOpportunities = false, // Default to not allowed for auto-created partners
                        PooledFund = false
                    };
    }

    /// <summary>
    /// Gets failed emails for a specific partner when partner creation fails
    /// </summary>
    private IEnumerable<string> GetFailedEmailsForPartner(GmailCreateRecordsRequest request, string partnerName)
    {
        return request.SelectedContacts
                        .Where(email => !email.PartnerId.HasValue &&
                   ((!string.IsNullOrEmpty(email.PartnerName) && email.PartnerName.Equals(partnerName, StringComparison.OrdinalIgnoreCase)) ||
                    (string.IsNullOrEmpty(email.PartnerName) && partnerName.Equals(GetPartnerNameFromEmail(email), StringComparison.OrdinalIgnoreCase))))
                        .Select(email => email.EmailAddress);
    }

    /// <summary>
    /// Processes a single contact - creates new or skips existing
    /// </summary>
    private async Task ProcessSingleContactAsync(GmailSelectedEmailModel selectedEmail, ClaimsPrincipal user, CreationState state)
                {
                    // Skip if this email already failed during partner creation
        if (state.FailedEmails.Contains(selectedEmail.EmailAddress))
        {
            return;
        }

        // Check if contact already exists with this email
        var existingContact = await _contactManager.GetContactByEmailAsync(user, selectedEmail.EmailAddress);
        if (existingContact != null)
        {
            _logger.LogInformation("Contact with email '{Email}' already exists with ID {ContactId}, skipping creation", selectedEmail.EmailAddress, existingContact.Id);
            state.ExistingContacts.Add(existingContact);
            return;
        }

        // Get partner ID for this contact
        var partnerId = GetPartnerIdForContact(selectedEmail, state);

        // Create contact request
        var contactRequest = CreateContactRequest(selectedEmail, partnerId);

        // Create the contact
        var createdContact = await _contactManager.CreateContactAsync(contactRequest);
        if (createdContact != null)
        {
            state.CreatedContacts.Add(createdContact);
            _logger.LogInformation("Created new contact with email '{Email}' and ID {ContactId}", selectedEmail.EmailAddress, createdContact.Id);
        }
    }

    /// <summary>
    /// Gets the partner ID for a contact
    /// </summary>
    private int GetPartnerIdForContact(GmailSelectedEmailModel selectedEmail, CreationState state)
    {
        // Use provided partner ID if available
                    if (selectedEmail.PartnerId.HasValue)
                    {
            return selectedEmail.PartnerId.Value;
        }

        // Use the partner that was created/found for this email
        var partnerName = GetPartnerNameFromEmail(selectedEmail);
        if (!state.CreatedPartners.TryGetValue(partnerName, out int partnerId))
                        {
                            throw new Exception($"Partner {partnerName} was not created successfully");
                        }

        return partnerId;
                    }

    /// <summary>
    /// Creates a contact request from email information
    /// </summary>
    private ContactRequest CreateContactRequest(GmailSelectedEmailModel selectedEmail, int partnerId)
    {
        // Use provided name information if available, otherwise extract from email
        string firstName = selectedEmail.FirstName;
        string middleName = selectedEmail.MiddleName;
        string lastName = selectedEmail.LastName;

        // If no name information provided, fall back to extracting from email prefix
        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
        {
            var emailParts = selectedEmail.EmailAddress.Split('@');
            var namePart = emailParts[0];
            var nameComponents = ExtractNameFromEmail(namePart);
            firstName = nameComponents.FirstName;
            lastName = nameComponents.LastName;
        }

        // Ensure LastName is always populated since it's a required field
        if (string.IsNullOrEmpty(lastName))
        {
            if (!string.IsNullOrEmpty(firstName))
            {
                // Use FirstName as LastName if LastName is empty
                lastName = firstName;
                firstName = ""; // Clear FirstName to avoid duplication
            }
            else
            {
                // As a last resort, use the email prefix as LastName
                var emailParts = selectedEmail.EmailAddress.Split('@');
                var namePart = emailParts[0];
                lastName = char.ToUpper(namePart[0]) + namePart.Substring(1).ToLower();
            }
        }

        return new ContactRequest
        {
            Email = selectedEmail.EmailAddress,
            FirstName = firstName ?? "",
            MiddleName = middleName ?? "",
            LastName = lastName ?? "",
            PartnerId = partnerId,
            // Set default values for required fields
            Salutation = "",
            Title = "",
            Status = EntityStatus.Active.ToString()
        };
    }

    /// <summary>
    /// Builds the final result from the creation state
    /// </summary>
    private GmailCreateRecordsResult BuildCreateRecordsResult(CreationState state)
    {
        return new GmailCreateRecordsResult
        {
            CreatedContacts = state.CreatedContacts.Count,
            CreatedPartners = state.NewPartnersCreated,
            FailedEmails = state.FailedEmails.Distinct().ToList(),
            Success = state.CreatedContacts.Any() || state.ExistingContacts.Any(),
            Message = BuildResultMessage(state)
        };
    }

    /// <summary>
    /// Builds the result message with detailed statistics
    /// </summary>
    private string BuildResultMessage(CreationState state)
    {
        var message = $"Processed {state.CreatedContacts.Count + state.ExistingContacts.Count} contact(s): " +
                     $"{state.CreatedContacts.Count} newly created, {state.ExistingContacts.Count} already existed";

        if (state.NewPartnersCreated > 0)
        {
            message += $" | Created {state.NewPartnersCreated} new partner(s)";
        }

        var existingPartnersFound = state.CreatedPartners.Count - state.NewPartnersCreated;
        if (existingPartnersFound > 0)
        {
            message += $" | Found {existingPartnersFound} existing partner(s)";
        }

        if (state.FailedEmails.Any())
        {
            message += $" | {state.FailedEmails.Count} failed";
        }

        return message;
    }

    /// <summary>
    /// Updates existing interaction with newly created/found contact and partner IDs
    /// </summary>
    private async Task UpdateExistingInteractionAsync(GmailCreateRecordsRequest request, ClaimsPrincipal user, CreationState state)
    {
        // Check if Gmail thread/message IDs are provided
        if (string.IsNullOrWhiteSpace(request.GmailThreadId) && string.IsNullOrWhiteSpace(request.GmailMessageId))
        {
            _logger.LogInformation("No Gmail thread or message ID provided, skipping interaction update");
            return;
        }

        try
        {
            // Find existing interaction by Gmail thread and message ID
            var gmailRequest = new GmailInteractionRequest
            {
                GmailThreadId = request.GmailThreadId ?? string.Empty,
                GmailMessageId = request.GmailMessageId ?? string.Empty
            };

            var existingInteraction = await _interactionManager.FindGmailInteractionAsync(gmailRequest);
            if (existingInteraction == null)
            {
                _logger.LogInformation("No existing interaction found for Gmail thread '{ThreadId}' or message '{MessageId}'", 
                    request.GmailThreadId, request.GmailMessageId);
                return;
            }

            // Update the interaction with new contact and partner IDs
            await UpdateInteractionWithRecordsAsync(existingInteraction, state, user);
            
            _logger.LogInformation("Successfully updated interaction {InteractionId} with new records", existingInteraction.Id);
                }
                catch (Exception ex)
                {
            _logger.LogError(ex, "Error updating existing interaction");
            // Don't throw - we don't want to fail the entire operation if interaction update fails
        }
    }

    /// <summary>
    /// Updates an interaction with newly created/found contact and partner IDs
    /// </summary>
    private async Task UpdateInteractionWithRecordsAsync(InteractionModel existingInteraction, CreationState state, ClaimsPrincipal user)
    {
        bool needsUpdate = false;

        // Initialize lists if null
        existingInteraction.ContactIds ??= new List<int>();
        existingInteraction.PartnerIds ??= new List<int>();

        // Add newly created contact IDs
        foreach (var createdContact in state.CreatedContacts)
        {
            if (!existingInteraction.ContactIds.Contains(createdContact.Id))
            {
                existingInteraction.ContactIds.Add(createdContact.Id);
                needsUpdate = true;
                _logger.LogInformation("Added new contact {ContactId} to interaction {InteractionId}", 
                    createdContact.Id, existingInteraction.Id);
            }
        }

        // Add existing contact IDs that were found
        foreach (var existingContact in state.ExistingContacts)
        {
            if (!existingInteraction.ContactIds.Contains(existingContact.Id))
            {
                existingInteraction.ContactIds.Add(existingContact.Id);
                needsUpdate = true;
                _logger.LogInformation("Added existing contact {ContactId} to interaction {InteractionId}", 
                    existingContact.Id, existingInteraction.Id);
            }
        }

        // Add partner IDs from created/found partners
        foreach (var partnerPair in state.CreatedPartners)
        {
            var partnerId = partnerPair.Value;
            if (!existingInteraction.PartnerIds.Contains(partnerId))
            {
                existingInteraction.PartnerIds.Add(partnerId);
                needsUpdate = true;
                _logger.LogInformation("Added partner {PartnerId} to interaction {InteractionId}", 
                    partnerId, existingInteraction.Id);
            }
        }

        // Update the interaction if changes were made
        if (needsUpdate)
        {
            var updateRequest = new UpdateInteractionRequest
            {
                Id = existingInteraction.Id,
                Type = existingInteraction.Type,
                Date = existingInteraction.Date,
                Description = existingInteraction.Description,
                Subject = existingInteraction.Subject,
                ContactIds = existingInteraction.ContactIds,
                PartnerIds = existingInteraction.PartnerIds,
                UserIds = existingInteraction.Users?.Select(u => u.Id).ToList() ?? new List<int>(),
                EmailAddresses = existingInteraction.EmailAddresses,
                Location = existingInteraction.Location,
                GmailThreadId = existingInteraction.GmailThreadId,
                GmailMessageId = existingInteraction.GmailMessageId
            };

            await _interactionManager.UpdateInteractionAsync(user, updateRequest);
            _logger.LogInformation("Updated interaction {InteractionId} with {ContactCount} contacts and {PartnerCount} partners", 
                existingInteraction.Id, existingInteraction.ContactIds.Count, existingInteraction.PartnerIds.Count);
        }
        else
        {
            _logger.LogInformation("No updates needed for interaction {InteractionId}", existingInteraction.Id);
        }
    }

    #endregion

    #region Helper Methods - Mapping

    private GmailRelatedContact MapContactToGmailContact(ContactModel contact, bool canRead)
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

    private GmailRelatedPartner MapPartnerToGmailPartner(PartnerModel partner, bool canRead)
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
            Phone = null, // Phone field no longer exists in enhanced PartnerModel
            LogoUrl = partner.LogoUrl,
            Location = null, // Address fields no longer exist in enhanced PartnerModel
            CanRead = true,
            Contacts = new List<GmailRelatedContact>(),
            Interactions = new List<GmailRelatedInteraction>()
        };

        // Add partner interactions if available
        if (partner.Interactions != null && partner.Interactions.Any())
        {
            currentPartner.Interactions = MapInteractionsToGmailInteractions(partner.Interactions);
        }

        // Add partner contacts
        if (partner.Contacts != null && partner.Contacts.Any())
        {
            currentPartner.Contacts = MapContactsToGmailContacts(partner.Contacts);
        }

        return currentPartner;
    }

    private GmailRelatedUser MapUserToGmailUser(PAOUserModel user, UserProfile? userProfile = null)
    {
        // Use enhanced data from UserProfile if available, otherwise fallback to PAOUser data
        var name = userProfile?.Name ?? user.Email ?? user.Id.ToString();
        var orgUnit = userProfile?.OrgUnit ?? "Unknown";

        return new GmailRelatedUser
        {
            Id = user.Id,
            Name = name,
            Email = user.Email,
            OrgUnit = orgUnit, 
            CanRead = true // Assuming all users can be read for now
        };
    }

    private List<GmailRelatedInteraction> MapInteractionsToGmailInteractions(IEnumerable<InteractionModel> interactions)
    {
        return interactions
            .Where(i => i.Permissions != null && i.Permissions.CanRead)
            .Select(i => new GmailRelatedInteraction
            {
                Id = i.Id,
                Type = i.Type.ToString(),
                Description = i.Description,
                Date = i.Date,
                CanRead = i.Permissions.CanRead
            }).ToList();
    }

    private List<GmailRelatedContact> MapContactsToGmailContacts(IEnumerable<ContactModel> contacts)
    {
        return contacts
            .Where(c => c.Permissions != null && c.Permissions.CanRead)
            .Select(c => new GmailRelatedContact
            {
                Name = $"{c.Salutation} {c.FirstName} {c.MiddleName} {c.LastName}",
                Title = c.Title,
                Id = c.Id,
                EmailAddress = c.Email,
                CanRead = c.Permissions.CanRead
            }).ToList();
    }

    #endregion

    #region Helper Methods - Processing

    private async Task<List<int>> ProcessContactsAsync(GmailRelatedRecordsRequest input, GmailRelatedRecordsResponse response, List<string> unmatchedEmailStrings, ClaimsPrincipal user)
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

    private async Task ProcessPartnersAsync(GmailRelatedRecordsRequest input, GmailRelatedRecordsResponse response, ClaimsPrincipal user)
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

    private async Task ProcessUsersAsync(GmailRelatedRecordsRequest input, GmailRelatedRecordsResponse response, List<string> unmatchedEmailStrings)
    {
        try
        {
            // Bulk lookup users by email addresses for efficiency
            var users = await _userDataManager.GetUsersByEmailsAsync(input.EmailAddresses);
            
            if (users.Any())
            {
                // Get the email addresses of found users for additional UserProfile lookup
                var foundUserEmails = users.Where(u => !string.IsNullOrEmpty(u.Email))
                                          .Select(u => u.Email)
                                          .ToList();

                // Get additional details from UserProfileService for the found users
                Dictionary<string, UserProfile> userProfileLookup = new Dictionary<string, UserProfile>(StringComparer.OrdinalIgnoreCase);

                var userProfiles = await _userInfoService.GetUserInfosByEmailsAsync(foundUserEmails);

                if(userProfiles != null && userProfiles.Any()) { 
                    // Handle potential duplicate emails by taking the first occurrence of each email
                    userProfileLookup = userProfiles
                        .GroupBy(up => up.UserEmail, StringComparer.OrdinalIgnoreCase)
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
                    UserProfile? userProfile = null;
                    if (!string.IsNullOrEmpty(user.Email) && userProfileLookup.ContainsKey(user.Email))
                    {
                        userProfile = userProfileLookup[user.Email];
                    }

                    var gmailUser = MapUserToGmailUser(user, userProfile);
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

    private async Task ProcessUnmatchedEmailsAsync(List<string> unmatchedEmailStrings, GmailRelatedRecordsResponse response, ClaimsPrincipal user)
    {
        response.UnmatchedEmails = await _contactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(unmatchedEmailStrings, user);
    }

    #endregion

    #region Helper Methods - Utilities

    private (string FirstName, string LastName) ExtractNameFromEmail(string emailPrefix)
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

    private string GetValidAudienceForCurrentHost()
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



    /// <summary>
    /// Sends in-app notifications for created contacts and partners
    /// </summary>
    private async Task SendCreationNotificationsAsync(ClaimsPrincipal user, CreationState state)
    {
        try
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == 0)
            {
                _logger?.LogWarning("Could not extract user ID from claims for notifications");
                return;
            }

            // Send combined notification if both were created
            if (state.CreatedContacts.Any() && state.NewPartnersCreated > 0)
            {
                var combinedMessage = $"Created {state.CreatedContacts.Count} contact(s) and {state.NewPartnersCreated} partner(s) from Gmail";

                await _notificationManager.CreateNotification(
                    userId,
                    combinedMessage,
                    "gmail_records_creation",
                    "GmailCreation",
                    new
                    {
                        ContactCount = state.CreatedContacts.Count,
                        PartnerCount = state.NewPartnersCreated,
                        ContactNames = state.CreatedContacts.Select(c => $"{c.FirstName} {c.LastName}".Trim()).ToList(),
                        PartnerNames = state.CreatedPartners.Keys.ToList(),
                        Source = "Gmail"
                    });
            }
            // Send notification for created contacts
            else if (state.CreatedContacts.Any())
            {
                var contactMessage = state.CreatedContacts.Count == 1 
                    ? $"New contact '{state.CreatedContacts.First().FirstName} {state.CreatedContacts.First().LastName}' created from Gmail"
                    : $"{state.CreatedContacts.Count} new contacts created from Gmail";

                await _notificationManager.CreateNotification(
                    userId,
                    contactMessage,
                    "gmail_contact_creation",
                    "Contact",
                    new
                    {
                        ContactIds = state.CreatedContacts.Select(c => c.Id).ToList(),
                        ContactNames = state.CreatedContacts.Select(c => $"{c.FirstName} {c.LastName}".Trim()).ToList(),
                        CreatedCount = state.CreatedContacts.Count,
                        Source = "Gmail"
                    });
            }

            // Send notification for created partners
            else if (state.NewPartnersCreated > 0)
            {
                var partnerNames = state.CreatedPartners.Keys.ToList();
                var partnerMessage = state.NewPartnersCreated == 1 
                    ? $"New partner '{partnerNames.First()}' created from Gmail"
                    : $"{state.NewPartnersCreated} new partners created from Gmail";

                await _notificationManager.CreateNotification(
                    userId,
                    partnerMessage,
                    "gmail_partner_creation",
                    "Partner",
                    new
                    {
                        PartnerNames = partnerNames,
                        CreatedCount = state.NewPartnersCreated,
                        Source = "Gmail"
                    });
            }

            _logger?.LogInformation("Successfully sent Gmail creation notifications for user {UserId}: {ContactCount} contacts, {PartnerCount} partners",
                userId, state.CreatedContacts.Count, state.NewPartnersCreated);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending Gmail creation notifications");
            // Don't rethrow - notifications are not critical to the main flow
        }
    }

    /// <summary>
    /// Helper method to extract user ID from claims
    /// </summary>
    private int GetUserIdFromClaims(ClaimsPrincipal user)
    {
        if (user == null) return 0;
        
        // Try multiple claim types that might contain the user ID
        var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                         user.FindFirst("sub")?.Value ??
                         user.FindFirst("userId")?.Value;
        
        if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        
        // Try to get from email-based lookup if direct ID not available
        var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ??
                   user.FindFirst("email")?.Value;
        
        if (!string.IsNullOrEmpty(email))
        {
            // This would require a user lookup service which may not be available here
            // For now, log and return 0
            _logger?.LogWarning("User ID not found in claims, only email available: {Email}", email);
        }
        
        return 0;
    }

    #endregion

    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        return null;
    }
}
