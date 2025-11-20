using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class InteractionFromEventSeeder_v3
    {
        public static async Task SeedInteractionsFromEventsAsync(UNOPSAppDbContext context)
        {
            // Create mapping from Contact.ContactNumber to ContactId
            var contactMapping = await context.Contacts
                .Where(c => !string.IsNullOrEmpty(c.ContactNumber))
                .ToDictionaryAsync(c => c.ContactNumber, c => c.Id);

            // Create mapping from Partner.ErpDimValue to PartnerId
            var partnerMapping = await context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .ToDictionaryAsync(p => p.ErpDimValue.Value, p => p.Id);

            // Create mapping from PAOUser.Email to UserId
            var paoUserEmailMapping = await context.PAOUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .ToDictionaryAsync(u => u.Email.ToLower(), u => u.Id);

            // Create mapping from OrganizationHierarchy.Code to OrganizationHierarchyId
            var orgHierarchyMapping = await context.OrganizationHierarchies
                .Where(oh => !string.IsNullOrEmpty(oh.Code) && oh.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                .ToDictionaryAsync(oh => oh.Code, oh => oh.Id);

            // Create mapping from Contact.Email to ContactId (for email-based lookups)
            var contactEmailMapping = await context.Contacts
                .Where(c => !string.IsNullOrEmpty(c.Email))
                .GroupBy(c => c.Email.ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.First().Id);

            // Get all Partner IDs with their Contact relationships for parent partner lookup
            var contactPartnerMapping = await context.Contacts
                .ToDictionaryAsync(c => c.Id, c => c.PartnerId);

            var newInteractionsCount = 0;

            // Process interactions
            var interactionsToProcess = new List<(string GmailMessageId, UNOPSInteraction Interaction, List<string> ContactIds, List<int> PartnerErpValues, List<string> OwnerEmails, List<string> OrgCodes)>
            {
                new (
                    "00UQx000001Ki7ZMAS",
                    new UNOPSInteraction
                    {
                        Name = "Check Surge Assignments",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-11-02").ToUniversalTime(),
                        Subject = "Check Surge Assignments",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000001Ki7ZMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("carriedi@unops.org".ToLower()) ? paoUserEmailMapping["carriedi@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2023, 11, 1, 13, 24, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "carriedi@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000001KidpMAC",
                    new UNOPSInteraction
                    {
                        Name = "My new event",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-11-01").ToUniversalTime(),
                        Subject = "My new event",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000001KidpMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("carriedi@unops.org".ToLower()) ? paoUserEmailMapping["carriedi@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2023, 11, 1, 13, 31, 32, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "carriedi@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000001Ko3KMAS",
                    new UNOPSInteraction
                    {
                        Name = "new event",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-11-02").ToUniversalTime(),
                        Subject = "new event",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000001Ko3KMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("carriedi@unops.org".ToLower()) ? paoUserEmailMapping["carriedi@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2023, 11, 1, 14, 44, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "carriedi@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000001g2RRMAY",
                    new UNOPSInteraction
                    {
                        Name = "meet with Salesforce today",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-11-14").ToUniversalTime(),
                        Subject = "meet with Salesforce today",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000001g2RRMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("carriedi@unops.org".ToLower()) ? paoUserEmailMapping["carriedi@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2023, 11, 14, 12, 15, 29, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "carriedi@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000001mk3BMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Other",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-11-17").ToUniversalTime(),
                        Subject = "Other",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000001mk3BMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("carriedi@unops.org".ToLower()) ? paoUserEmailMapping["carriedi@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2023, 11, 17, 9, 7, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "carriedi@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000002RSXOMA4",
                    new UNOPSInteraction
                    {
                        Name = "Meeting to resume discussions about UNOPS-KOICA global MoU",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-10-13").ToUniversalTime(),
                        Subject = "Meeting to resume discussions about UNOPS-KOICA global MoU",
                        Description = "Meeting focused on resuming discussions about the UNOPS-KOICA MoU, agree on options and next steps.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000002RSXOMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2023, 12, 11, 12, 9, 32, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000070LrqIAE" },
                    new List<int> { 1105 },
                    new List<string> { "arnauds@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003ACpNMAW",
                    new UNOPSInteraction
                    {
                        Name = "LA TEST",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-22").ToUniversalTime(),
                        Subject = "LA TEST",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003ACpNMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laitha@unops.org".ToLower()) ? paoUserEmailMapping["laitha@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 1, 10, 11, 37, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "laitha@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000003ANXtMAO",
                    new UNOPSInteraction
                    {
                        Name = "Call",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-10").ToUniversalTime(),
                        Subject = "Call",
                        Description = null,
                        Location = "Buenos Aires",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003ANXtMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("joseme@unops.org".ToLower()) ? paoUserEmailMapping["joseme@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 1, 10, 12, 55, 27, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "joseme@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003Bk5tMAC",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up ahead of EB session",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-11").ToUniversalTime(),
                        Subject = "Catch-up ahead of EB session",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003Bk5tMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 1, 11, 8, 59, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000099IS3IAM" },
                    new List<int> { 1144 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003KZ6PMAW",
                    new UNOPSInteraction
                    {
                        Name = "RD AFR Mission to Geneva with GF",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-26").ToUniversalTime(),
                        Subject = "RD AFR Mission to Geneva with GF",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003KZ6PMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("louisel@unops.org".ToLower()) ? paoUserEmailMapping["louisel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 1, 16, 14, 21, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1448 },
                    new List<string> { "louisel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003UgL3MAK",
                    new UNOPSInteraction
                    {
                        Name = "Virtual meeting between USG Moreira da Silva and Ambassador Kalkku",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-25").ToUniversalTime(),
                        Subject = "Virtual meeting between USG Moreira da Silva and Ambassador Kalkku",
                        Description = "________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NmM3NTBlNTItMGExOC00OGEwLWJlMmUtZWM1MTg2MWQ0OGIy%40thread.v2/0?context=%7b%22Tid%22%3a%229ed1dc60-55cb-4459-9432-57a7768f1bed%22%2c%22Oid%22%3a%228222b6d5-a928-4c42-9ff8-62d2a6d79db3%22%7d>\n\nMeeting ID: 393 410 478 838\nPasscode: dT6Sf7\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\n\nJoin with a video conferencing device\nmfa.teams@video.valtori.fi<mailto:mfa.teams@video.valtori.fi>\nVideo Conference ID: 126 085 103 8\nAlternate VTC instructions<https://meet.video.valtori.fi/teams/?conf=1260851038&ivr=mfa.teams&d=video.valtori.fi&prefix=mfa.>\n\nOr call in (audio only)\n+358 9 85626463,,353854704#<tel:+358985626463,,353854704#>   Finland, Helsinki\nPhone Conference ID: 353 854 704#\nFind a local number<https://dialin.teams.microsoft.com/0cff18c0-8493-4212-91b7-eb6c70ce3685?id=353854704> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=8222b6d5-a928-4c42-9ff8-62d2a6d79db3&tenantId=9ed1dc60-55cb-4459-9432-57a7768f1bed&threadId=19_meeting_NmM3NTBlNTItMGExOC00OGEwLWJlMmUtZWM1MTg2MWQ0OGIy@thread.v2&messageId=0&language=en-US>",
                        Location = "Teams",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003UgL3MAK",
                        EmailAddresses = new List<string> { "mfa.teams@video.valtori.fi" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 1, 22, 14, 20, 59, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1124 },
                    new List<string> { "asbjornb@unops.org", "sonjalk@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003Wd4XMAS",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up: UNOPS/Sweden",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-25").ToUniversalTime(),
                        Subject = "Catch-up: UNOPS/Sweden",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003Wd4XMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 1, 23, 12, 22, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OobWtIAJ" },
                    new List<int> { 1267 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003u34mMAA",
                    new UNOPSInteraction
                    {
                        Name = "KOICA - UNOPS follow up meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-01").ToUniversalTime(),
                        Subject = "KOICA - UNOPS follow up meeting",
                        Description = "Follow-up meeting on the UNOPS-KOICA MoU. Key topics of discussion:\n1. Two options to update the MoU\na. A fundamental rethink/simplification of the MoU so it becomes less prescriptive, more streamlined and operational;\nb. A more simple updating of the current MoU despite its complexities.\n=> This will be discussed with the newly appointed Director General of the Department of Country Project Management Department\n2. Approach and next steps\na. UNOPS to liaise with IPAS Legal and share the outline of what the outline of a simplified MoU could look like\nb. KOICA to \"escalate the discussion about potential options for the MoU\n=> Next meeting scheduled for end of Feb.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003u34mMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yoogyoungk@unops.org".ToLower()) ? paoUserEmailMapping["yoogyoungk@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 5, 12, 19, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000070LrqIAE" },
                    new List<int> { 1105 },
                    new List<string> { "yoogyoungk@unops.org", "arnauds@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003uKC2MAM",
                    new UNOPSInteraction
                    {
                        Name = "Meeting 1: UNOPS-UN Women",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-05").ToUniversalTime(),
                        Subject = "Meeting 1: UNOPS-UN Women",
                        Description = "<p>Dear colleagues, </p><p><span>I hope this finds you well. </span><br></p><p><span>With many thanks to Jacqueline for confirming your availability, we are pleased to send a calendar invitation for the first meeting between UNOPS and UN Women - on </span><span>February 5 at 9am New York / 3pm Copenhagen. </span><br></p><p><span>The purpose of this meeting is to initiate a discussion around potential areas of collaboration, specifically within joint project implementation.</span><br></p><p><span>Google Meet: </span><a href=\"https://meet.google.com/tnm-kfhk-grf\"><u>https://meet.google.com/tnm-kfhk-grf</u></a><span> </span><br></p><p><span>We look forward to a productive discussion. </span><br></p><p><span>Kind regards, </span><br></p><p>Mikaela</p><p>Partnerships and Liaison Group | UNOPS HQ Copenhagen, Denmark </p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003uKC2MAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jean-vincentc@unops.org".ToLower()) ? paoUserEmailMapping["jean-vincentc@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 5, 13, 40, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1222 },
                    new List<string> { "jean-vincentc@unops.org", "daniele@unops.org", "mikaelag@unops.org", "robertgodin@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003ugUIMAY",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with UN Women (joint project implementation)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-05").ToUniversalTime(),
                        Subject = "Meeting with UN Women (joint project implementation)",
                        Description = "Purpose: initiate a discussion around potential areas of collaboration, specifically within joint project implementation.",
                        Location = "Online",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003ugUIMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mikaelag@unops.org".ToLower()) ? paoUserEmailMapping["mikaelag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 5, 15, 52, 22, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000B5lnxIAB" },
                    new List<int> { 1222 },
                    new List<string> { "mikaelag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003vvCjMAI",
                    new UNOPSInteraction
                    {
                        Name = "Sweden engagement - Moldova",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-01").ToUniversalTime(),
                        Subject = "Sweden engagement - Moldova",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003vvCjMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 6, 8, 58, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1267 },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org", "eleneag@unops.org", "elenage@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000003vvKnMAI",
                    new UNOPSInteraction
                    {
                        Name = "Lunch meeting with the WEOG",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-30").ToUniversalTime(),
                        Subject = "Lunch meeting with the WEOG",
                        Description = null,
                        Location = "UNOPS office",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000003vvKnMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("hilaryb@unops.org".ToLower()) ? paoUserEmailMapping["hilaryb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 6, 8, 58, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1267 },
                    new List<string> { "hilaryb@unops.org", "emiliep@unops.org", "oonaa@unops.org", "hafidal@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000043aCmMAI",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Rainer Frauenfeld and Ala’a Nemer with INTPA",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-31").ToUniversalTime(),
                        Subject = "Meeting Rainer Frauenfeld and Ala’a Nemer with INTPA",
                        Description = "10.00-11.00:\nMeeting with Hans Stausboll, INTPA, Africa Acting Director \nAccompanied by: Mariacarmen Colitti, Laetitia Kraus",
                        Location = "Rue de la Loi 41, 1000 Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000043aCmMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 9, 15, 16, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BGpWcIAL" },
                    new List<int> { 1025 },
                    new List<string> { "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000043co5MAA",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Rainer Frauenfeld and Ala’a Nemer with FPI",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-31").ToUniversalTime(),
                        Subject = "Meeting Rainer Frauenfeld and Ala’a Nemer with FPI",
                        Description = "Meeting with Silvia BOTTONE, FPI Programme Manager, IcSP for the East and Central Africa and South East Asia regions, and Anne-Sophie LEQUARRE, FPI Programme Manager\nAccompanied by: Mariacarmen Colitti, Laetitia Kraus",
                        Location = "EEAS Schuman",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000043co5MAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 9, 15, 19, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1031 },
                    new List<string> { "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000043kiLMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Rainer Frauenfeld and Ala’a Nemer with DG ECHO",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-09").ToUniversalTime(),
                        Subject = "Meeting Rainer Frauenfeld and Ala’a Nemer with DG ECHO",
                        Description = "Meeting with Sandra Goffin, ECHO, Head of Sector, Kenya and Somalia (Ethiopia and Djibouti) \nAccompanied by: Laetitia Kraus",
                        Location = "Rue de la Loi 86,1049 Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000043kiLMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 9, 15, 20, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BGljPIAT" },
                    new List<int> { 1029 },
                    new List<string> { "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000045mo7MAA",
                    new UNOPSInteraction
                    {
                        Name = "Frokost - Julia/Asbjørn",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-13").ToUniversalTime(),
                        Subject = "Frokost - Julia/Asbjørn",
                        Description = null,
                        Location = "Kanal-Caféen, Frederiksholms Kanal 18, 1220 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000045mo7MAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 12, 7, 17, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BQI9SIAX" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000048J9eMAE",
                    new UNOPSInteraction
                    {
                        Name = "Meeting 2: UNOPS-UN WOMEN",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-12").ToUniversalTime(),
                        Subject = "Meeting 2: UNOPS-UN WOMEN",
                        Description = "<p>Dear colleagues, </p><p>I hope this finds you well. </p><p>With many thanks to Jacqueline for confirming your availability, we are pleased to send a calendar invitation for the second meeting between UNOPS and UN Women - on February 12, at 9am New York / 3pm Copenhagen. </p><p>The purpose of this meeting is to initiate a discussion around potential areas of collaboration, specifically within knowledge/best practice sharing as well as joint communication/advocacy.</p><p>Google Meet: <a href=\"https://meet.google.com/ahc-nyad-yez\"><u>https://meet.google.com/ahc-nyad-yez</u></a> </p><p>We look forward to a productive discussion. </p><p>Best regards, </p><p>Mikaela</p><p>Partnerships and Liaison Group | UNOPS HQ Copenhagen, Denmark <br></p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000048J9eMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mikaelag@unops.org".ToLower()) ? paoUserEmailMapping["mikaelag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 13, 8, 39, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000B5lnxIAB" },
                    new List<int> { 1222 },
                    new List<string> { "mikaelag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004ExSZMA0",
                    new UNOPSInteraction
                    {
                        Name = "Meeting to discuss UNOPS client board meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-16").ToUniversalTime(),
                        Subject = "Meeting to discuss UNOPS client board meeting",
                        Description = "Meeting to discuss KOICA participation at UNOPS CB meeting on March 13. \n\nOptions being considered:\n1/ KOICA HQ director/director general\n2/ MOFA: senior officer\n3/ KOICA: a country director, probably based in AFR\n4/ KOICA: a regional director\n5/ An \"attaché\" based in the EU\n=> Options 3 and 4 are the ones that Mr Lee sees as making the most sense,  considering the time difference.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004ExSZMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 16, 8, 29, 32, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000070LrqIAE" },
                    new List<int> { 1105 },
                    new List<string> { "arnauds@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004IOfuMAG",
                    new UNOPSInteraction
                    {
                        Name = "Kick-off: Regeringens arbejde med styrket engagement i Afrika",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-11").ToUniversalTime(),
                        Subject = "Kick-off: Regeringens arbejde med styrket engagement i Afrika",
                        Description = "<a href=\"https://www.altinget.dk/live-arrangementer/kick-off-regeringens-arbejde-med-styrket-engagement-i-Afrika\">https://www.altinget.dk/live-arrangementer/kick-off-regeringens-arbejde-med-styrket-engagement-i-Afrika</a>",
                        Location = "Det Kgl. Bibliotek, The Royal Library, Søren Kierkegaards Pl. 1, 1221 København K, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004IOfuMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 19, 8, 31, 1, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004LM1kMAG",
                    new UNOPSInteraction
                    {
                        Name = "Intro in-person meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-23").ToUniversalTime(),
                        Subject = "Intro in-person meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004LM1kMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 20, 14, 31, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000A9u0RIAR" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004LodpMAC",
                    new UNOPSInteraction
                    {
                        Name = "IFC-UNOPS meeting",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-18").ToUniversalTime(),
                        Subject = "IFC-UNOPS meeting",
                        Description = null,
                        Location = "IFC HQ offices",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004LodpMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 20, 16, 21, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C2CKjIAN" },
                    new List<int> { 1547 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004LoynMAC",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-IFC",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-24").ToUniversalTime(),
                        Subject = "UNOPS-IFC",
                        Description = "________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NTI3ZmQwOWEtNzk1YS00NWE1LThkMzMtMTI4ZmQxZjQxNDU0%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22ec7c522a-a4b0-4f83-8c67-ca962f9cc058%22%7d>\nMeeting ID: 285 873 280 003\nPasscode: gQL2Xi\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com\nVideo Conference ID: 114 460 529 5\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1144605295&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,464411796#<tel:+15094080991,,464411796#>   United States, Spokane\nPhone Conference ID: 464 411 796#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=464411796> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=ec7c522a-a4b0-4f83-8c67-ca962f9cc058&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NTI3ZmQwOWEtNzk1YS00NWE1LThkMzMtMTI4ZmQxZjQxNDU0@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004LoynMAC",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 20, 16, 23, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C284HIAR" },
                    new List<int> { 1547 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004LpBhMAK",
                    new UNOPSInteraction
                    {
                        Name = "Shayna / Christine lunch",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-31").ToUniversalTime(),
                        Subject = "Shayna / Christine lunch",
                        Description = null,
                        Location = "KAZ Sushi Bistro, 1915 I St NW, Washington, DC 20006, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004LpBhMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 20, 16, 24, 44, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C2DofIAF" },
                    new List<int> { 1198 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004Mx7BMAS",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/Swedish Embassy in DK",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-21").ToUniversalTime(),
                        Subject = "UNOPS/Swedish Embassy in DK",
                        Description = null,
                        Location = "UN City, Marmorvej 51, 2100 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004Mx7BMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 21, 8, 42, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1267 },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004UZqrMAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with  UNOPS Acting RD for Europe and Central Asia, Mr. Tim Lardner",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-29").ToUniversalTime(),
                        Subject = "Meeting with  UNOPS Acting RD for Europe and Central Asia, Mr. Tim Lardner",
                        Description = "Dear Catherine, many thanks for your email, and Mr. Lardner is looking forward to meeting H.E. Mr. Lauber on Thursday 29 of February. Mr. Lardner will be accompanied by Ms. Louise Leech, UNOPS Liaison Officer in Geneva.\n\nMany thanks,\n\nBest,\nNele\n\n\nWith YEC\n\nDear Nele,\n\nFurther to your request regarding a meeting with Ambassador Lauber and Mr. Lardner, I can suggest the following slot:\n\n\n  *   Friday 29th at 14h30.\n\nHoping that this slot is feasible, I remain with kind regards.\n\nC. Sanz\n\nCatherine Sanz Depierre\nAssistante du Chef de Mission\nDépartement fédéral des affaires étrangères DFAE\nMission permanente de la Suisse auprès de l'ONUG\nRue de Varembé 9-11, CP 194, CH- 1211 Genève 20\nTél: + 41  (0)58 482 24 24\ncatherine.sanzdepierre@eda.admin.ch<mailto:catherine.sanzdepierre@eda.admin.ch>\nwww.dfae.admin.ch/geneve<http://www.dfae.admin.ch/geneve>\nThis e-mail may contain trade secrets or privileged, undisclosed or otherwise confidential information. If you have received this e-mail in error,you are hereby notified that any review, copying or distribution of it is strictly prohibited. Please inform us immediately and destroy the original transmittal. Thank you for your cooperation.\n[cid:image001.png@01DA5B4F.1638B4D0]<http://www.unpluspourlapaix.ch/>\n\n\nFrom: Nele DEMEULEMEESTER <neled@unops.org<mailto:neled@unops.org>>\nSent: Friday, February 9, 2024 9:21 AM\nTo: Sanz Depierre Catherine EDA SANCA <catherine.sanzdepierre@eda.admin.ch<mailto:catherine.sanzdepierre@eda.admin.ch>>\nCc: Sophie DJUGELI <sophiedj@unops.org<mailto:sophiedj@unops.org>>; Alla PSHENYCHNYKH <allap@unops.org<mailto:allap@unops.org>>\nSubject: [EXTERNAL] Meeting with His Excellency Mr. Jürg Lauber - UNOPS Acting RD for Europe and Central Asia, Mr. Tim Lardner\n\nDear Catherine, I trust this email finds you well.\n\nI would like to seek your kind assistance in scheduling a courtesy meeting with His Excellency Mr. Lauber for UNOPS Acting Regional Director for Europe and Central Asia, Mr. Tim Lardner, who is also serving as Director in Ukraine.\n\nMr. Lardner will be in Geneva next week on Friday 16 February and if H.E. Mr. Lauber's schedule allows, he would be grateful for a brief courtesy meeting preferably between 11:00-12:15, or after 14:30?\n\nIf the proposed time slot would not be feasible, could you kindly indicate H.E. Mr. Lauber's availability on Thursday 29 February in the afternoon, or on Friday 1 March before 12:00 or after 14:30CET.\n\nKindly find attached Mr. Lardner's bio, accessible also on UNOPS website<https://www.unops.org/about/our-story/leadership> under the Regional Offices header, and please don't hesitate to contact me, should you need any further information or assistance.\n\nWith best regards,\nNele\n\nNele Demeulemeester | Executive Assistant to the Regional Director | Europe and Central Asia | Geneva, Switzerland | Tel: +41 79 317 8569 | www.unops.org<http://www.unops.org/>\n[Image removed by sender.]",
                        Location = "Bureau LJG",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004UZqrMAG",
                        EmailAddresses = new List<string> { "allap@unops.org", "catherine.sanzdepierre@eda.admin.ch", "sophiedj@unops.org", "neled@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("timl@unops.org".ToLower()) ? paoUserEmailMapping["timl@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 26, 9, 30, 14, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1142 },
                    new List<string> { "timl@unops.org", "louisel@unops.org", "neled@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004VSaPMAW",
                    new UNOPSInteraction
                    {
                        Name = "Lunch w Asbjørn Brink UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-28").ToUniversalTime(),
                        Subject = "Lunch w Asbjørn Brink UNOPS",
                        Description = null,
                        Location = "NN1 Guest Canteen",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004VSaPMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 26, 15, 12, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1123 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004aXuRMAU",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with 株式会社ツインバード",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-22").ToUniversalTime(),
                        Subject = "Meeting with 株式会社ツインバード",
                        Description = null,
                        Location = "UNOPS Office in UNU Tokyo",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004aXuRMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 29, 5, 59, 19, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000CdE7RIAV" },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004aq4EMAQ",
                    new UNOPSInteraction
                    {
                        Name = "KOICA-UNOPS MOU",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-29").ToUniversalTime(),
                        Subject = "KOICA-UNOPS MOU",
                        Description = "Key discussion points:\n1. CB: KOICA not yet clear regarding their potential attendance. The main challenge seems related to the time difference / their decision to select a KOICA representative from HQ\n2. MoU\na. KOICA indicated their preference to opt for a minor revision of the MOU, as a more ambitious change might be harder to justify/get approved from their end\nb. KOICA asked UNOPS to start the review process and to share a suggested updated version of the MOU\n3. KOICA to inform UNOPS of potential high-level visits from KOICA to Asia countries (President or Vice-President)\n4. Anna to visit KOICA HQ while she is in Seoul\n\nNext steps\n- UNOPS to start working on the MOU\n- Determine the best option and key changes we want to push for based on the consultation/feedback from colleagues",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004aq4EMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yoogyoungk@unops.org".ToLower()) ? paoUserEmailMapping["yoogyoungk@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 2, 29, 8, 26, 54, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000070LrqIAE" },
                    new List<int> { 1105 },
                    new List<string> { "yoogyoungk@unops.org", "arnauds@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004pmt9MAA",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS GAVI framework agreement - updates",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-08").ToUniversalTime(),
                        Subject = "UNOPS GAVI framework agreement - updates",
                        Description = null,
                        Location = "CPH-5-7.33-Room (12) [Google Meet]",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004pmt9MAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("ekaterinapo@unops.org".ToLower()) ? paoUserEmailMapping["ekaterinapo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 3, 8, 11, 1, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BbV1kIAF" },
                    new List<int> {  },
                    new List<string> { "ekaterinapo@unops.org", "devorahfd@unops.org", "louisel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004wO4TMAU",
                    new UNOPSInteraction
                    {
                        Name = "WB/ UNOPS mtg on Sierra Leone",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-08").ToUniversalTime(),
                        Subject = "WB/ UNOPS mtg on Sierra Leone",
                        Description = "Dear all,\n\nI have booked a room on the 11th floor in the J building as well as a teams connection.\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_OTRlYzdkMDEtMjA1NS00ZTJmLTk1NTUtYjU0OGY0NWU2MmY5%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22632c8e2b-0902-4a55-afc3-05bdc207816e%22%7d>\nMeeting ID: 240 362 189 083\nPasscode: ncoWmA\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com\nVideo Conference ID: 115 373 356 9\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1153733569&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,949887638#<tel:+15094080991,,949887638#>   United States, Spokane\nPhone Conference ID: 949 887 638#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=949887638> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=632c8e2b-0902-4a55-afc3-05bdc207816e&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_OTRlYzdkMDEtMjA1NS00ZTJmLTk1NTUtYjU0OGY0NWU2MmY5@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "J 11-155 (15) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004wO4TMAU",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 3, 12, 21, 22, 56, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IoZ8aIAF", "003Qx00000DUYmaIAH" },
                    new List<int> {  },
                    new List<string> { "sofiag@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004wO65MAE",
                    new UNOPSInteraction
                    {
                        Name = "World Bank/UNOPS meeting on Sierra Leone",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-07").ToUniversalTime(),
                        Subject = "World Bank/UNOPS meeting on Sierra Leone",
                        Description = "Hi all,\n\nI am sharing the calendar invite for the Bank discussion with UNOPS Sierra Leone country manager to talk about how UNOPS do targeting for renewable energy projects.\n\nProposed agenda:\n\n  *   How UNOPS selects and using what criteria does UNOPS evaluate the sites it will target for solar sites\n  *   How does UNOPS incentivize the sustainable use of solar systems in the communities where they are located.\n  *   WB SPJ and ESMAP team to provide insights on how targeting is conducted in their projects\n\nRegards,\nMohamed\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NWMzODk5M2EtM2MxMS00Mjg4LTk4ZmEtMDZkZGViMjJkNzRj%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22662b0886-ad9c-4b59-af9b-7117152e6b30%22%7d>\nMeeting ID: 291 899 867 52\nPasscode: 9Ns6vF\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com<mailto:wbg@m.webex.com>\nVideo Conference ID: 116 410 488 8\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1164104888&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,486000181#<tel:+15094080991,,486000181#>   United States, Spokane\nPhone Conference ID: 486 000 181#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=486000181> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=662b0886-ad9c-4b59-af9b-7117152e6b30&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NWMzODk5M2EtM2MxMS00Mjg4LTk4ZmEtMDZkZGViMjJkNzRj@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting; G 8-011 (42) (VC), G 8-011 (42) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004wO65MAE",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 3, 12, 21, 23, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000DUfW8IAL" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000004wOHNMA2",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with EU Delegation, Washington",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-26").ToUniversalTime(),
                        Subject = "Meeting with EU Delegation, Washington",
                        Description = "Subject: Afghanistan\nNick George covered the operational landscape in the country and the results of the Community Resilience and Livelihoods Project, funded by the World Bank Afghanistan Resilience Trust Fund (ARTF). He briefly touched on the EU-funded humanitarian call centre and thanked the EU and member states for their ongoing engagement -- both on the ground in Kabul and through various funding mechanisms. \n\n\nParticipants included Petra Mijic (Political Counselor covering Afghanistan), Marcin Gluchowski (Humanitarian Counselor), and Carolina Lasso-Navarro (Development Counselor). Member states represented included Latvia, Poland, Slovenia, Netherlands, Ireland, and Belgium. A couple others joined but did not have time to introduce themselves. \n\n\nKey areas covered included:\n\nQuestions (especially from the Netherlands) about the impact of the ban on women in NGOs etc, women’s role in UN agencies, and about women's ability to benefit from UNOPS projects. \n\nDiscussion of the opening of IDA funding for Afghanistan and EU members' concern with traceability of funds contributed to the Afghanistan Resilience Trust Fund. EU Commission made a recent $50M contribution to the trust fund. As \"development\" is not possible under the current circumstances in Afghanistan -- all our work is focused on “basic human needs,” a term unique to the Afghanistan context. \n\nDiscussions around climate change/drought and the Herat earthquake, and its influence on relations with the de facto authorities, given the additional need for UN assistance.",
                        Location = "Delegation of the European Union, 2175 K St NW, Washington, DC 20437, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000004wOHNMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 3, 12, 21, 26, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000DUbh5IAD" },
                    new List<int> { 1031 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "nicholasg@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005O5dCMAS",
                    new UNOPSInteraction
                    {
                        Name = "Gaza crisis coordination meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-19").ToUniversalTime(),
                        Subject = "Gaza crisis coordination meeting",
                        Description = "Directors’ level Gaza crisis coordination meeting, co-chaired by the European Commission and France, held in the margins of the European Humanitarian Forum.\n- Chaired by Mr Andreas Papaconstantinou, Director for Neighbourhood, Middle East, South-West and Central Asia, DG ECHO, European Commission\n- Co-chaired by Mr Philippe Lalliot, Director of the Crisis and Support Centre, Ministry for Europe and Foreign Affairs, France\n\nUNOPS presented the following:\n- Presence in Gaza (WB and East Jerusalem)\n- Humanitarian response in Gaza (NFI, ASU and Fuel for emergency)\n- Fuel mechanism (supply and monitoring)\n- Looking at the needs from the clusters, fuel quantity should be increased\n- Source of energy should be diversified, including the solar energy\n- Member state to use their leverage to advocate for more fuel and solar equipments to enter Gaza\n\nAfter UNOPS 'words, the Netherlands’ Representative requested other countries to support the Sigrid mission and the coming mechanism. Sweden mentioned that they are looking into the possibilities to support Sigrid and the mechanisms. The UK representatives made reference to UNOPS work, highlighting how the situation is difficult for humanitarian workers, while advocating for more support for Gaza.\n\nMr Andreas Papaconstantinou is planning to be in Palestine in the coming weeks. He informed Sophie NYIRABAKWIYE (Jerusalem Office) that he will look into the possibility of meeting UNOPS.",
                        Location = "Online",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005O5dCMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 3, 28, 9, 55, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000EaDwZIAV" },
                    new List<int> { 1029 },
                    new List<string> { "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005a54kMAA",
                    new UNOPSInteraction
                    {
                        Name = "Sahel",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-05").ToUniversalTime(),
                        Subject = "Sahel",
                        Description = "Som aftalt. Jeg henter jer i receptionen.\nBh Marie",
                        Location = "UM 2EF",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005a54kMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 5, 7, 12, 56, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000F6NXaIAN" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005fhNuMAI",
                    new UNOPSInteraction
                    {
                        Name = "Frokostmøde: UNOPS / UM",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-09").ToUniversalTime(),
                        Subject = "Frokostmøde: UNOPS / UM",
                        Description = null,
                        Location = "Kanal-Caféen, Frederiksholms Kanal 18, 1220 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005fhNuMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 9, 8, 52, 57, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005pvcVMAQ",
                    new UNOPSInteraction
                    {
                        Name = "DK MFA / Office of the Special Coordinator Gaza_DK contribution",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-15").ToUniversalTime(),
                        Subject = "DK MFA / Office of the Special Coordinator Gaza_DK contribution",
                        Description = "Dear Marija\n\nShould you still be available, we would be keen to proceed with the call. Mainly with some technical questions on the new mechanism.\n\nBest\nKatrine\n\nKATRINE SIIG KRISTENSEN / kasikr@um.dk<mailto:kasikr@um.dk>\nCHIEF COUNSELLOR, HUMANITARIAN AFFAIRS\nDEPARTMENT FOR HUMANITARIAN AFFAIRS, CIVIL SOCIETY AND ENGAGEMENT (HCE)\nMOBILE +45 2381 5045\n\nMINISTRY OF FOREIGN AFFAIRS\nASIATISK PLADS 2 / DK-1448 KØBENHAVN K\nPHONE +45 3392 0000\n\n[Danida-English [17739] (4)]\n\n\n________________________________________________________________________________\nMicrosoft Teams Har du brug for hjælp?<https://aka.ms/JoinTeamsMeeting?omkt=da-DK>\nTilmeld dig mødet nu<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NmM2YmFmYWMtZWVkYi00MWVjLTg4N2YtZDg5M2M4OWVmNTJm%40thread.v2/0?context=%7b%22Tid%22%3a%2248dc02d0-bd56-411d-b7c5-a814743bafc6%22%2c%22Oid%22%3a%2206a41aca-fdbe-4569-9b43-85450821605e%22%7d>\nMøde-id: 320 012 355 657\nAdgangskode: L5upfx\n________________________________\nTilmeld dig på en enhed til videomøder\nLejernøgle: teams@meet.um.dk\nVideo-id: 125 181 807 2\nFlere oplysninger<https://pexip.me/teams/meet.um.dk/1251818072>\nFor arrangører: Mødeindstillinger<https://teams.microsoft.com/meetingOptions/?organizerId=06a41aca-fdbe-4569-9b43-85450821605e&tenantId=48dc02d0-bd56-411d-b7c5-a814743bafc6&threadId=19_meeting_NmM2YmFmYWMtZWVkYi00MWVjLTg4N2YtZDg5M2M4OWVmNTJm@thread.v2&messageId=0&language=da-DK> | Nulstil opkaldspinkode<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams-møde",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005pvcVMAQ",
                        EmailAddresses = new List<string> { "teams@meet.um.dk", "kasikr@um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("marijab@unops.org".ToLower()) ? paoUserEmailMapping["marijab@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 15, 12, 33, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "marijab@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005pxL1MAI",
                    new UNOPSInteraction
                    {
                        Name = "Introfrokost: DRC/UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-17").ToUniversalTime(),
                        Subject = "Introfrokost: DRC/UNOPS",
                        Description = null,
                        Location = "UN City, Marmorvej 51, 2100 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005pxL1MAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 15, 12, 33, 35, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000F6aMkIAJ" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005q2PRMAY",
                    new UNOPSInteraction
                    {
                        Name = "Brainstorme podcast mellem UGKM'en og FN ASG",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-19").ToUniversalTime(),
                        Subject = "Brainstorme podcast mellem UGKM'en og FN ASG",
                        Description = null,
                        Location = "Klima-, Energi- og Forsyningsministeriet, Holmens Kanal 20, 1060 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005q2PRMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 15, 12, 33, 52, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1123 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005sXQjMAM",
                    new UNOPSInteraction
                    {
                        Name = "CEB in Copenhagen in 2025",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-16").ToUniversalTime(),
                        Subject = "CEB in Copenhagen in 2025",
                        Description = "Pending confirmation of availabilities for Danish MFA colleagues. <br><br>@Julia: Please feel free to include relevant colleagues on your side - including perhaps Astrid from protocol? <br><br><b>Tent. topics for discussion</b><br><ul><li><span>Division of labour</span></li><li><span>Ideas for program, visits, etc.</span></li><li><span>Venue</span></li><li><span>Outreach </span></li><li><span>Etc. </span></li></ul>",
                        Location = "Ministry of Foreign Affairs of Denmark, Asiatisk Pl. 2, 1448 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005sXQjMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("raady@unops.org".ToLower()) ? paoUserEmailMapping["raady@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 16, 14, 29, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BQI9SIAX" },
                    new List<int> {  },
                    new List<string> { "raady@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000005vnVnMAI",
                    new UNOPSInteraction
                    {
                        Name = "Discuss with IPAS Admin the possibility of adding UNHCR IICA medical insurance",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "Discuss with IPAS Admin the possibility of adding UNHCR IICA medical insurance",
                        Description = "UNHCR’s initiative to extend medical insurance to all categories of personnel. As we have discussed earlier, UNHCR’s directly hired individual contract holders will soon be enrolled to a new scheme, and with this, the only remaining group in our workforce without corporate level standard health insurance are the International UNOPS ICA holders.\n\n \n\n- UNHCR’s management is committed to exercise and extend medical insurance to all its personnel, including IICA\n- Have internal discussion with PCG and IPAS Admin for the potential/requirements of including the IICAs in the newly established scheme later this year.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000005vnVnMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("lorrainea@unops.org".ToLower()) ? paoUserEmailMapping["lorrainea@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 18, 10, 28, 12, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000063ljjMAA",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up: Sweden/UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-25").ToUniversalTime(),
                        Subject = "Catch-up: Sweden/UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000063ljjMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 23, 11, 53, 6, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx000009zlegIAA" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000063yfMMAQ",
                    new UNOPSInteraction
                    {
                        Name = "DK/UNOPS: ARKAS Discussion",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-23").ToUniversalTime(),
                        Subject = "DK/UNOPS: ARKAS Discussion",
                        Description = "Discussion\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NTAyNjIwYzgtNGI4MS00OWYzLWEzMmEtZDc1ZjA5ZTczOThl%40thread.v2/0?context=%7b%22Tid%22%3a%2248dc02d0-bd56-411d-b7c5-a814743bafc6%22%2c%22Oid%22%3a%22155e082c-ee70-4d40-989a-23d45261e256%22%7d>\nMeeting ID: 366 460 597 068\nPasscode: chEe56\n________________________________\nJoin on a video conferencing device\nTenant key: teams@meet.um.dk<mailto:teams@meet.um.dk>\nVideo ID: 124 244 419 4\nMore info<https://pexip.me/teams/meet.um.dk/1242444194>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=155e082c-ee70-4d40-989a-23d45261e256&tenantId=48dc02d0-bd56-411d-b7c5-a814743bafc6&threadId=19_meeting_NTAyNjIwYzgtNGI4MS00OWYzLWEzMmEtZDc1ZjA5ZTczOThl@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000063yfMMAQ",
                        EmailAddresses = new List<string> { "teams@meet.um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("svitlanaz@unops.org".ToLower()) ? paoUserEmailMapping["svitlanaz@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 23, 11, 53, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF" },
                    new List<int> {  },
                    new List<string> { "svitlanaz@unops.org", "asbjornb@unops.org", "teresam@unops.org", "janphilipk@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006GgXhMAK",
                    new UNOPSInteraction
                    {
                        Name = "Extension of UNHCR’s medical insurance to the IICA holders",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-24").ToUniversalTime(),
                        Subject = "Extension of UNHCR’s medical insurance to the IICA holders",
                        Description = "Dear Colleagues,\n\nAs agreed below, this is to invite you to discuss the extension of UNHCR’s medical insurance to the IICA holders working for UNHCR.\n\nThank you and best regards,\nGergo\n\n\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NGQ3NmUyYWYtNzg1NS00MmNkLWFiYTYtZDVlMGE0ZDc3M2M5%40thread.v2/0?context=%7b%22Tid%22%3a%22e5c37981-6664-4134-8a0c-6543d2af80be%22%2c%22Oid%22%3a%22c4127218-f379-4334-9dad-a29d425b3cd8%22%7d>\nMeeting ID: 390 372 824 949\nPasscode: pHnEBH\n________________________________\nJoin on a video conferencing device\nTenant key: unhcr2@m.webex.com\nVideo ID: 126 377 822 5\nMore info<https://www.webex.com/msteams?confid=1263778225&tenantkey=unhcr2&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=c4127218-f379-4334-9dad-a29d425b3cd8&tenantId=e5c37981-6664-4134-8a0c-6543d2af80be&threadId=19_meeting_NGQ3NmUyYWYtNzg1NS00MmNkLWFiYTYtZDVlMGE0ZDc3M2M5@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________\n\n\n_____________________________________________\nFrom: Lorraine ANABTAWI <lorrainea@unops.org>\nSent: Wednesday, April 17, 2024 2:12 PM\nTo: Gergo Gelsei <gelsei@unhcr.org>\nCc: Laurentiu MASTACAN <laurentium@unops.org>; Zen Patel <patelz@unhcr.org>; Mihaela Szekely <szekely@unhcr.org>; Svetlana Chapurskaya <CHAPURSK@unhcr.org>; Miroslav Medic <medic@unhcr.org>; Diego Ruiz Proano <ruizproa@unhcr.org>\nSubject: Re: Proposed meeting for next week - medical insurance\n\n\nAttention: This email is from an external sender. Please be careful with any links or attachments.\nDear Gergo,\n\nI would like to confirm the following time as suitable: Wednesday, 24th from 13:30 to 14:30, from our side we have RoseAnne from insurance team roseanneb@unops.org<mailto:roseanneb@unops.org> who will be joining, in addition to Arnaud, Laurentiu and myself.\n\nBest,\nLorraine\n\n\n\n\nOn Tue, Apr 16, 2024 at 11:02 AM Gergo Gelsei <gelsei@unhcr.org<mailto:gelsei@unhcr.org>> wrote:\n\nDear Lorraine,\n\nWe would like to arrange a call with you for next week to discuss UNHCR’s initiative to extend medical insurance to all categories of personnel. As we have discussed earlier, UNHCR’s directly hired individual contract holders will soon be enrolled to a new scheme, and with this, the only remaining group in our workforce without corporate level standard health insurance are the International UNOPS ICA holders.\n\n\n\nAs UNHCR’s management is committed to exercise and extend such aspect of duty of care to all its personnel, we would like to discuss with you the potential of including the IICAs in the newly established scheme later this year.\n\n\n\nOur Chief of Section and Head of Service would also attend the call, so please feel free to invite the necessary level of officials from UNOPS side.\n\n\n\nHere are some suggested timeslots for a meeting:\n\n\n\nWednesday, 24th 13:30 – 14:30\n\nThursday 25th 13:00 – 14:00\n\n\n\nThank you in advance and best regards,\n\nGergo\n\n\n\n--\nLorraine Anabtawi | HR Partnerships Specialist | Partnerships and Liaison Group | UNOPS | Tel: 972548174189 | Skype: lorraine.anabtawi | www.unops.org<https://www.unops.org/english/Pages/Home.aspx> | Supplying value, delivering results, changing lives\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org/>, LinkedIn<https://www.linkedin.com/uas/login?trk=bf&trkInfo=AQHx5A9hof9xkQAAAVsAW80gHaHhbaTtICmxtAOm2W6Ua5W8nccrqq8bMvUrGFLjXf9hAZeDMt9gmVTYthXhD7TKYde595-FRBaIxU4=&session_redirect=https%3A%2F%2Fwww.linkedin.com%2Fcompany%2Funops>, Twitter<https://twitter.com/UNOPS?ref_src=twsrc%5Egoogle%7Ctwcamp%5Eserp%7Ctwgr%5Eauthor>, Instagram<https://www.instagram.com/unops_official/?hl=en>, YouTube<https://www.youtube.com/user/UNOPSofficial> Subscribe to our external newsletter in English<https://confirmsubscription.com/h/r/28CBB1F85AE31216>, French<https://confirmsubscription.com/h/r/5BC15A59F87CC82D> or Spanish<https://confirmsubscription.com/h/r/1E87E822D07D72F0>",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006GgXhMAK",
                        EmailAddresses = new List<string> { "patelz@unhcr.org", "ruizproa@unhcr.org", "laurentium@unops.org", "szekely@unhcr.org", "lorrainea@unops.org", "medic@unhcr.org", "roseanneb@unops.org", "gelsei@unhcr.org", "unhcr2@m.webex.com", "chapursk@unhcr.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("lorrainea@unops.org".ToLower()) ? paoUserEmailMapping["lorrainea@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 4, 30, 11, 20, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GmsjPIAR" },
                    new List<int> {  },
                    new List<string> { "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006IraHMAS",
                    new UNOPSInteraction
                    {
                        Name = "ED Jorge Moreira da Silva and WB VP OPCS Ed Mountfield",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-19").ToUniversalTime(),
                        Subject = "ED Jorge Moreira da Silva and WB VP OPCS Ed Mountfield",
                        Description = "Full mission report: https://docs.google.com/document/d/13q-NdKrQhRkTUyN2msVgISGiPHCPgF9ZliYJqioc__8/edit\n\nMeeting readout:\n\nED updated on UNOPS reforms and Ukraine delivery.\nVP noted importance of financial controls and transparency. Reporting issues in a timely way has been a problem with other UN agencies. Serious issues must be reported within 24 hours, per the framework contract. \nWB sees UNOPS as an institution that can “fill gaps” under difficult circumstances.\nVP thanked UNOPS for implementation support with IDA. Noted dissatisfaction with “critical” op-ed the ED placed in Project Syndicate. ED discussed with VP issue of IDA disbursement – ED said that undisbursed IDA balances are unsurprising, given the financing model and long timelines for repayment. \nED highlighted his goal to relate the implementation gap to the broader financing and policy gaps. Noted how critical technical assistance and capacity building are.\nDirector Saum noted importance of 3 issues going forward with UN agencies: 1) fees; 2) access to information and reporting, to ensure the WB has timely access to monitoring reports, verifications (made clear this has nothing to do with auditing), and 3) Environmental and Social Framework (ESF). \nDirector Saum requested that UNOPS share messages about the need for a results-focus on procurement, including on scope 3 emissions, and requested that the ED discuss this with other heads of agencies. ED noted UNOPS goal to create a “race-to-the-top” on procurement standards. \nVP noted that IDA21 replenishment is proving challenging. Cash contributions need to go up, but we could see a “nominal shrinking” or a “hardening of terms” of grants/loans, which might lead to more debt distress. \nVP stressed the importance of positive messages going out about IDA, sharing positive stories of multilateralism. Suggested starting with case studies of good WB-UNOPS success stories with IDA funds.\nVP noted the new Corporate Scorecard, which will be embedded in new lending with retroactive adjustments to existing portfolio. Full data will be available on the indicators in time for the 2024 Annual Meetings.\nED invited VP to meet him in CPH.",
                        Location = "World Bank Main Complex (MC) Building, 10th/F, MC 10-605",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006IraHMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 1, 16, 11, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hy0TCIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "alistairs@unops.org", "jorge.moreiradasilva@unops.org", "paulom@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006J0H4MAK",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS ED and BHA Administrator Sonali Korde and ME DAA Pryor Jeanne Pryor",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-19").ToUniversalTime(),
                        Subject = "UNOPS ED and BHA Administrator Sonali Korde and ME DAA Pryor Jeanne Pryor",
                        Description = "The ED outlined UNOPS plans for the Gaza Humanitarian Access Mechanism, monitored fuel deliveries for humanitarian actors, and other work in Gaza. He also asked for US financial and political support for Gaza, including deconfliction.  \nHe also described the importance of UNVIM and UNOPS work in Yemen to support regional stability. \nME DAA Pryor and BHA AA were very interested in further details on the Gaza mechanism and UNOPS work in the territory. BHA AA noted that she was in touch with Yahev on Sigrid’s team and had good conversations with Bana in Amman.  \nME DAA Pryor indicated that she was more focused on recovery, although, with budgets available, USAID was unlikely to be focused on major infrastructure works.\nBHA AA asked about solar panels and whether it would be possible, technically, to tag and geolocate them to allow confidence they weren’t being diverted. ED indicated that ICT and smart metering make it easier to know who uses solar power rather than diesel.\nThe ED outlined the reforms made to date and his vision for the organisation and asked what else the US would like to see before returning to a normal business relationship.\nUSAID said they were pleased with progress on reforms and looked forward to greater cooperation in the near future. \nCharles Kiamie noted the excess reserve refund, which is helpful internally. He was pleased with how UNOPS frames its core mandate and focuses on core business; he noted USG concern of ‘multilateral cannibalization’.",
                        Location = "Ronald Reagan Building and International Trade Center, 1300 Pennsylvania Avenue NW, Washington, DC 20004, USA, US-RRB-ConfRm-6.08.83-USAID",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006J0H4MAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 1, 18, 49, 28, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GstOAIAZ" },
                    new List<int> { 1112 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KMCEMA4",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with USAiD re Refunds",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-09").ToUniversalTime(),
                        Subject = "Meeting with USAiD re Refunds",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KMCEMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 38, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GvpkaIAB" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "iraklij@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KOsQMAW",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS meeting with DeMark Schulze (Sen. Todd Young)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-04").ToUniversalTime(),
                        Subject = "UNOPS meeting with DeMark Schulze (Sen. Todd Young)",
                        Description = null,
                        Location = "Dirksen Senate Building, Room 185",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KOsQMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("marijab@unops.org".ToLower()) ? paoUserEmailMapping["marijab@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 58, 12, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw3p8IAB" },
                    new List<int> {  },
                    new List<string> { "marijab@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KQB0MAO",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up chat",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-09").ToUniversalTime(),
                        Subject = "Catch-up chat",
                        Description = "Can also chat on whatsapp, either way.\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_YjJhYmUyODMtOWJkMi00MGYzLWI5NWEtYmVhZmNkMmY4ZDhk%40thread.v2/0?context=%7b%22Tid%22%3a%22612e3f19-36e9-44c6-a7f0-9daa3a334fb9%22%2c%22Oid%22%3a%2259f853f4-8834-4bba-9d5c-d253b3924ead%22%7d>\nMeeting ID: 283 261 103 132\nPasscode: bsLMWx\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nOr call in (audio only)\n+1 613-701-1213,,709510750#<tel:+16137011213,,709510750#>   Canada, Ottawa-Hull\nPhone Conference ID: 709 510 750#\nFind a local number<https://dialin.teams.microsoft.com/64c4fd66-d988-4bb5-995a-78b374ddb774?id=709510750> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=59f853f4-8834-4bba-9d5c-d253b3924ead&tenantId=612e3f19-36e9-44c6-a7f0-9daa3a334fb9&threadId=19_meeting_YjJhYmUyODMtOWJkMi00MGYzLWI5NWEtYmVhZmNkMmY4ZDhk@thread.v2&messageId=0&language=en-US>\n..............................................................\nRéunion Microsoft Teams\nParticipez à partir de votre ordinateur, de l’application mobile ou d’un appareil de la salle\nCliquez ici pour vous joindre à la réunion<https://teams.microsoft.com/l/meetup-join/19%3ameeting_YjJhYmUyODMtOWJkMi00MGYzLWI5NWEtYmVhZmNkMmY4ZDhk%40thread.v2/0?context=%7b%22Tid%22%3a%22612e3f19-36e9-44c6-a7f0-9daa3a334fb9%22%2c%22Oid%22%3a%2259f853f4-8834-4bba-9d5c-d253b3924ead%22%7d>\nID de la réunion : 283 261 103 132\nCode secret : bsLMWx\nTéléchargez Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Participez sur le web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nOu composez le numéro de téléphone (audio seulement)\n+1 613-701-1213,,709510750#<tel:+16137011213,,709510750#>   Canada, Ottawa-Hull\nNo de conférence téléphonique : 709 510 750#\nRecherchez un numéro local<https://dialin.teams.microsoft.com/64c4fd66-d988-4bb5-995a-78b374ddb774?id=709510750> | Réinitialisez le NIP<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nPour en savoir plus<https://aka.ms/JoinTeamsMeeting> | Options de réunion<https://teams.microsoft.com/meetingOptions/?organizerId=59f853f4-8834-4bba-9d5c-d253b3924ead&tenantId=612e3f19-36e9-44c6-a7f0-9daa3a334fb9&threadId=19_meeting_YjJhYmUyODMtOWJkMi00MGYzLWI5NWEtYmVhZmNkMmY4ZDhk@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KQB0MAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 39, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1024 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KSJFMA4",
                    new UNOPSInteraction
                    {
                        Name = "UN Web Buy Briefing for Canadian Department of National Defense",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-11").ToUniversalTime(),
                        Subject = "UN Web Buy Briefing for Canadian Department of National Defense",
                        Description = "<p>Invite re-sent with MS teams meeting link.</p><p><a href=\"https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZDE1MGEwYWUtNzM1OS00YTY5LTkxNjEtYTYxMDNkZDFhMGQy%40thread.v2/0?context=%7b%22Tid%22%3a%22325b4494-1587-40d5-bb31-8b660b7f1038%22%2c%22Oid%22%3a%22faf19d1c-79d2-480c-b9fc-5edf0ce0fe6d%22%7d\">Click here to join the meeting</a><br></p><p><u></u></p><p>Meeting ID: 275 322 020 04 <br></p>",
                        Location = "CPH-5-7.33-Room (12) [Google Meet]",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KSJFMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 50, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GvqS6IAJ" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KScbMAG",
                    new UNOPSInteraction
                    {
                        Name = "Connecting on reserves instructions timeframe",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-15").ToUniversalTime(),
                        Subject = "Connecting on reserves instructions timeframe",
                        Description = "Good morning/afternoon, everyone:\n\nRe-sending invite with a teams link so can access.\n\nSpeak soon,\nlayal\n\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MjQyYWNiZTYtMzc1Yi00ZWIyLTlkYmUtNzMyMGZhZTQzNDdi%40thread.v2/0?context=%7b%22Tid%22%3a%22612e3f19-36e9-44c6-a7f0-9daa3a334fb9%22%2c%22Oid%22%3a%2259f853f4-8834-4bba-9d5c-d253b3924ead%22%7d>\nMeeting ID: 295 881 746 395\nPasscode: kFqwLD\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nOr call in (audio only)\n+1 613-701-1213,,400885652#<tel:+16137011213,,400885652#>   Canada, Ottawa-Hull\nPhone Conference ID: 400 885 652#\nFind a local number<https://dialin.teams.microsoft.com/64c4fd66-d988-4bb5-995a-78b374ddb774?id=400885652> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=59f853f4-8834-4bba-9d5c-d253b3924ead&tenantId=612e3f19-36e9-44c6-a7f0-9daa3a334fb9&threadId=19_meeting_MjQyYWNiZTYtMzc1Yi00ZWIyLTlkYmUtNzMyMGZhZTQzNDdi@thread.v2&messageId=0&language=en-US>\n..............................................................\nRéunion Microsoft Teams\nParticipez à partir de votre ordinateur, de l’application mobile ou d’un appareil de la salle\nCliquez ici pour vous joindre à la réunion<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MjQyYWNiZTYtMzc1Yi00ZWIyLTlkYmUtNzMyMGZhZTQzNDdi%40thread.v2/0?context=%7b%22Tid%22%3a%22612e3f19-36e9-44c6-a7f0-9daa3a334fb9%22%2c%22Oid%22%3a%2259f853f4-8834-4bba-9d5c-d253b3924ead%22%7d>\nID de la réunion : 295 881 746 395\nCode secret : kFqwLD\nTéléchargez Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Participez sur le web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nOu composez le numéro de téléphone (audio seulement)\n+1 613-701-1213,,400885652#<tel:+16137011213,,400885652#>   Canada, Ottawa-Hull\nNo de conférence téléphonique : 400 885 652#\nRecherchez un numéro local<https://dialin.teams.microsoft.com/64c4fd66-d988-4bb5-995a-78b374ddb774?id=400885652> | Réinitialisez le NIP<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nPour en savoir plus<https://aka.ms/JoinTeamsMeeting> | Options de réunion<https://teams.microsoft.com/meetingOptions/?organizerId=59f853f4-8834-4bba-9d5c-d253b3924ead&tenantId=612e3f19-36e9-44c6-a7f0-9daa3a334fb9&threadId=19_meeting_MjQyYWNiZTYtMzc1Yi00ZWIyLTlkYmUtNzMyMGZhZTQzNDdi@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________\n\n_____________________________________________\nFrom: Sarrouh, Layal -MIO\nSent: Monday, January 15, 2024 9:43 AM\nTo: 'Hafida LAHIOUEL' <hafidal@unops.org>\nCc: 'Lilian Aluoch NYANGAYA' <liliann@unops.org>; Castonguay, Marie-Eve -PRMNY -DA <Marie-Eve.Castonguay@international.gc.ca>; 'Sven ECKERT' <svene@unops.org>; 'Tanvi MANI' <tanvim@unops.org>; 'Vilhelm KLARESKOV' <vilhelmk@unops.org>; 'patrickel@unops.org' <patrickel@unops.org>\nSubject: RE: Connecting on reserves instructions timeframe\n\nDear Hafida,\n\nI hope you had a good weekend.\n\nThank you for sending the invite. Unfortunately, we are not able to use google meets, so I will share a teams invite.\n\nLook forward to speaking soon.\n\nBest,\nlayal\n\n--\nLayal T. E. Sarrouh\nSenior Advisor, UNDP | Conseillère Principale, PNUD\nUnited Nations Division | Division Nations Unies\ne: layal.sarrouh@international.gc.ca<mailto:layal.sarrouh@international.gc.ca>\nm: +1.343.550.0462 (and whatsapp)\nGlobal Affairs Canada | Affaires mondiales Canada\nGovernment of Canada | Gouvernement du Canada\n\n[cid:image002.png@01DA4799.3D3FF120]\n\nFrom: Hafida LAHIOUEL <hafidal@unops.org<mailto:hafidal@unops.org>>\nSent: Friday, January 12, 2024 6:16 PM\nTo: Sarrouh, Layal -MIO <Layal.Sarrouh@international.gc.ca<mailto:Layal.Sarrouh@international.gc.ca>>\nSubject: Re: Connecting on reserves instructions timeframe\n\nThank you Layal.\n\nI just shared an invitation via GoogleMeet. If you cannot use this platform, please do not hesitate to share your own invite to us and I will cancel the GoogleMeet one.\n\nMonday is busy and 11am is the only available time in the morning for me but if Lilian and colleagues can do earlier, this is fine too and I will just get briefed laterZ\n\nKind regards,\n\nHafida\n\n\n\nOn Fri, Jan 12, 2024 at 17:03 <Layal.Sarrouh@international.gc.ca<mailto:Layal.Sarrouh@international.gc.ca>> wrote:\nDear Hafida,\n\nThank you for your reply.\n\nMonday at 11am EST works well. I can also do earlier if easier for colleagues in Copenhagen.\n\nWishing you all a good weekend!\n\nBest,\nlayal\n\n--\nLayal T. E. Sarrouh\nSenior Advisor, UNDP | Conseillère Principale, PNUD\nUnited Nations Division | Division Nations Unies\ne: layal.sarrouh@international.gc.ca<mailto:layal.sarrouh@international.gc.ca>\nm: +1.343.550.0462 (and whatsapp)\nGlobal Affairs Canada | Affaires mondiales Canada\nGovernment of Canada | Gouvernement du Canada\n\nFrom: Hafida LAHIOUEL <hafidal@unops.org<mailto:hafidal@unops.org>>\nSent: Thursday, January 11, 2024 4:47 PM\nTo: Sarrouh, Layal -MIO <Layal.Sarrouh@international.gc.ca<mailto:Layal.Sarrouh@international.gc.ca>>\nCc: Lilian Aluoch NYANGAYA <liliann@unops.org<mailto:liliann@unops.org>>; Castonguay, Marie-Eve -PRMNY -DA <Marie-Eve.Castonguay@international.gc.ca<mailto:Marie-Eve.Castonguay@international.gc.ca>>; Sven ECKERT <svene@unops.org<mailto:svene@unops.org>>; Tanvi MANI <tanvim@unops.org<mailto:tanvim@unops.org>>; Vilhelm KLARESKOV <vilhelmk@unops.org<mailto:vilhelmk@unops.org>>; patrickel@unops.org<mailto:patrickel@unops.org>\nSubject: Re: Connecting on reserves instructions timeframe\n\nHi Layal,\n\nThanks for reaching out.\n\nYes we agreed with Marie-Eve we should aim to resolve this issue ahead of the Board and including Lilian and her team in the conversation would be great.\n\nCould we speak on Monday morning at 11am? Tomorrow is too tight for us in view of the Board’s orientation.\n\nKind regards,\n\nHafida\n\n\nOn Thu, Jan 11, 2024 at 16:41 <Layal.Sarrouh@international.gc.ca<mailto:Layal.Sarrouh@international.gc.ca>> wrote:\nDear Hafida,\n\nWishing you a very happy new year! I hope you enjoyed the holidays.\n\nI am following up on your initial message to Marie-Eve on the reserves, and the timeline for instructions to be shared with UNOPS. I am happy to help see if we can sort this out, especially ahead of Boards.\n\nI would like to suggest having a call to discuss, and I am thinking with Liliane as well? Might be the easiest way to sort things, though I am open to your suggestion.\n\nBest,\nlayal\n\n--\nLayal T. E. Sarrouh\nSenior Advisor, UNDP | Conseillère Principale, PNUD\nUnited Nations Division | Division Nations Unies\ne: layal.sarrouh@international.gc.ca<mailto:layal.sarrouh@international.gc.ca>\nm: +1.343.550.0462 (and whatsapp)\nGlobal Affairs Canada | Affaires mondiales Canada\nGovernment of Canada | Gouvernement du Canada",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KScbMAG",
                        EmailAddresses = new List<string> { "vilhelmk@unops.org", "liliann@unops.org", "hafidal@unops.org", "tanvim@unops.org", "marie-eve.castonguay@international.gc.ca", "patrickel@unops.org", "layal.sarrouh@international.gc.ca", "svene@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("vilhelmk@unops.org".ToLower()) ? paoUserEmailMapping["vilhelmk@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 51, 52, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw3KTIAZ" },
                    new List<int> {  },
                    new List<string> { "vilhelmk@unops.org", "liliann@unops.org", "hafidal@unops.org", "patrickel@unops.org", "svene@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KSfpMAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Nancy (USUN)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-16").ToUniversalTime(),
                        Subject = "Meeting with Nancy (USUN)",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KSfpMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 52, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw3p7IAB" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KT0nMAG",
                    new UNOPSInteraction
                    {
                        Name = "Meridian-UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-17").ToUniversalTime(),
                        Subject = "Meridian-UNOPS",
                        Description = "Let’s move this to virtual for this afternoon. Thanks for the flexibility!\n\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_YmI1Y2U0ZjktYWU3Yy00NDcyLThjOWMtM2E2ZDk1MGJmYzMy%40thread.v2/0?context=%7b%22Tid%22%3a%22ecf6566c-a840-47d5-9eb0-3e24777f891d%22%2c%22Oid%22%3a%22f5617038-bcf9-4785-b84d-30c829e55ec0%22%7d>\nMeeting ID: 220 484 372 520\nPasscode: DRXisi\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=f5617038-bcf9-4785-b84d-30c829e55ec0&tenantId=ecf6566c-a840-47d5-9eb0-3e24777f891d&threadId=19_meeting_YmI1Y2U0ZjktYWU3Yy00NDcyLThjOWMtM2E2ZDk1MGJmYzMy@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________\n\n_____________________________________________\nFrom: Alistair Peter SOMERVILLE <alistairs@unops.org<mailto:alistairs@unops.org>>\nSent: Friday, December 8, 2023 1:23 PM\nTo: Boyce, Katherine <KBoyce@meridian.org<mailto:KBoyce@meridian.org>>\nCc: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>; Girgenti, Sienna <SGirgenti@meridian.org<mailto:SGirgenti@meridian.org>>; Justice, Frank <FJUSTICE@meridian.org<mailto:FJUSTICE@meridian.org>>\nSubject: Re: Happy holidays and possible meeting in Jan\n\nThis email originated from outside Meridian. Do not click links or open attachments unless you recognize the sender and know the content is safe.\nHi Katherine and Sienna,\n\nThanks so much for getting back to me so quickly. How about 2pm on Jan 11th at Meridian for a coffee?\n\nOur head of office Christine Bowers (in cc) also plans to join the meeting.\n\nBest wishes,\n\nAlistair\n\nOn Thu, Dec 7, 2023 at 10:37 AM Boyce, Katherine <KBoyce@meridian.org<mailto:KBoyce@meridian.org>> wrote:\nAlistair-\nGreat to see you too, albeit too briefly. We’d love to get together to discuss ways to collaborate across Meridian, within our Center for Diplomatic Engagement, through our large private-sector network, or other interested thought-leaders.\n\nFrank is out on parental leave til the new year, but I’m comfortable scheduling for him. I will likely include Sienna Girgenti, our new Director of the Center for Diplomatic Engagement. I believe I mentioned the last time we spoke that my role has transitioned, so I have a great eye into other ways we can plug in throughout the organization.\n\nHow about January 11th in the late morning or afternoon? Also happy to look at other times, just let me know when you’re available. Let’s do in-person, coffee or lunch at Meridian or a good location for you.\n\nAll my best and happy holidays,\nKatherine\n\n[A picture containing logo    Description automatically generated]<http://www.meridian.org/>\n\nKatherine Boyce\nSenior Director, External Affairs\nMeridian International Center\n1630 Crescent Place, NW, Washington, DC 20009\nT. (202) 939-5546 | kboyce@meridian.org<mailto:kboyce@meridian.org> | www.meridian.org<http://www.meridian.org/>\nFollow us on:\n[signature_761790023]<https://www.facebook.com/MeridianInternationalCenter>\n[A picture containing text, clipart    Description automatically generated]<https://www.linkedin.com/company/meridian-international-center/>\n[A picture containing text, clipart, gear    Description automatically generated]<https://twitter.com/MeridianIntl>\n[Icon    Description automatically generated]<https://www.instagram.com/meridianintl/>\n[signature_819036109]<https://www.youtube.com/user/MeridianCommunity>\n\n\n\n\n[A close-up of a logo    Description automatically generated with low confidence]<http://www.meridian.org/corporate>\n\nFrom: Alistair Peter SOMERVILLE <alistairs@unops.org<mailto:alistairs@unops.org>>\nSent: Thursday, December 7, 2023 10:18 AM\nTo: Justice, Frank <FJUSTICE@meridian.org<mailto:FJUSTICE@meridian.org>>; Boyce, Katherine <KBoyce@meridian.org<mailto:KBoyce@meridian.org>>\nSubject: Happy holidays and possible meeting in Jan\n\nThis email originated from outside Meridian. Do not click links or open attachments unless you recognize the sender and know the content is safe.\nDear Frank and Katherine,\n\nI hope all is well with you, and, Katherine, great to see you briefly at the USGLC event.\n\nI’m reaching out to wish you all the best for the holiday season and to see if we might organize a meeting in the new year, either virtually or in person at the UN offices/Meridian, to discuss potential opportunities to collaborate more with CDE. Specifically, I would be interested in exploring ways for my current office, UNOPS, to engage the diplomatic community in D.C. through your speaker series and other programming with the diplomatic corps.\n\nPlease let me know this would be of interest and when in the new year would be a good time to meet.\n\nBest wishes,\n\nAlistair\n\n\n--\n\nAlistair Somerville | Senior Officer | Partnerships and Liaison Group | UNOPS Washington Liaison Office, 1775 K Street NW, Washington, DC 20006, USA | www.unops.org<https://www.unops.org/english/Pages/Home.aspx> | +1 (229) 586-9792\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official>, YouTube<https://www.youtube.com/user/UNOPSofficial>.\n\nSubscribe to our external newsletter in English<https://mailchi.mp/8987deaa0e61/uojskr902j>, French<https://mailchi.mp/unops.org/gu0ld93is9> or Spanish<https://mailchi.mp/edf4b84d88cf/on3qnjwzkh>.\n________________________________________________________________________________",
                        Location = "Teams",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KT0nMAG",
                        EmailAddresses = new List<string> { "sgirgenti@meridian.org", "christinebo@unops.org", "fjustice@meridian.org", "alistairs@unops.org", "kboyce@meridian.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 55, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw17RIAR" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KTLlMAO",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with CRS re. Middle East(Virtual)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-04").ToUniversalTime(),
                        Subject = "Meeting with CRS re. Middle East(Virtual)",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KTLlMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("marijab@unops.org".ToLower()) ? paoUserEmailMapping["marijab@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 12, 56, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw0UjIAJ" },
                    new List<int> {  },
                    new List<string> { "marijab@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KTtdMAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with CRS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-19").ToUniversalTime(),
                        Subject = "Meeting with CRS",
                        Description = "Dear Luisa and Rhodie: Please let us know if this time works. Our schedules are pretty flexible right now for the time slot. Also, happy to connect on whatever platform is best. We use Google, but can connect on Zoom or Teams if that is better, \n\nBest regards Patrick",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KTtdMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 13, 0, 48, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw0UjIAJ" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KU05MAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Nancy USUN",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-26").ToUniversalTime(),
                        Subject = "Meeting with Nancy USUN",
                        Description = null,
                        Location = "Pennylane Coffee, 305 E 45th St, New York, NY 10017, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KU05MAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 13, 1, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw3p7IAB" },
                    new List<int> {  },
                    new List<string> { "christinebo@unops.org", "emiliep@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KXIvMAO",
                    new UNOPSInteraction
                    {
                        Name = "WRI/UNOPS Washington",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-30").ToUniversalTime(),
                        Subject = "WRI/UNOPS Washington",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KXIvMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 13, 23, 35, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gvm3XIAR" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KY5JMAW",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Washington / Wilson Center Wahba Institute",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-31").ToUniversalTime(),
                        Subject = "UNOPS Washington / Wilson Center Wahba Institute",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KY5JMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 13, 27, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GwDenIAF" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006KYQHMA4",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Senator Kaine's Office",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-04").ToUniversalTime(),
                        Subject = "Meeting with Senator Kaine's Office",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006KYQHMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 2, 13, 29, 29, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GwFqHIAV" },
                    new List<int> { 1145 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006Q1D9MAK",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with WB VP for Human Development Mamta Murthi",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "Meeting with WB VP for Human Development Mamta Murthi",
                        Description = "Full mission report: https://docs.google.com/document/d/13q-NdKrQhRkTUyN2msVgISGiPHCPgF9ZliYJqioc__8/edit\n\nMeeting readout: \n\nHDVP did not seem especially familiar with UNOPS. Asked about UNOPS personnel numbers, government contracting vs. third-party implementation. \n40% of WB’s FCV work is under HD: SPJ, Health, Education. \nHD is focused on delivering new WB commitment to deliver better healthcare access to 1.5 billion people, as well as new Gender Strategy. Additional commitments focused on women and girls are forthcoming after they are endorsed at the WB management and Board levels. \nKey question for HD-UNOPS partnership: How do we turn crisis engagements into building country systems? \nHealth: WB is increasingly focused on climate and health, as well as pandemic preparedness and response. Local capacity, including digital and systems capacity, are also key. Human resources in health are also a critical area. \nED and WLO outlined UNOPS key value addition.",
                        Location = "MC 11-215 (35) (VC) Private",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006Q1D9MAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 6, 17, 16, 59, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HByC0IAL" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006Q1EkMAK",
                    new UNOPSInteraction
                    {
                        Name = "ED Jorge Moreira da Silva and WB VP Sustainable Development Juergen Voegele",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "ED Jorge Moreira da Silva and WB VP Sustainable Development Juergen Voegele",
                        Description = "Full mission report: https://docs.google.com/document/d/13q-NdKrQhRkTUyN2msVgISGiPHCPgF9ZliYJqioc__8/edit\n\nMeeting readout:\n\nVP laid out massive scale up in SD on water – one of the six new Global Challenge Programs, many of which are housed within HD.\nWB reforms: VP explained that WB is no longer an organisation that can “spend money and do good things”. It must focus much more on impact, and “must work with partners.” He mentioned the WB President’s focus on identifying specific ideas that are highly scalable.\nVP reiterated E and S requirements and UNOPS role as a good example. \nGlobal Director for Social Sustainability complimented UNOPS on its work in Afghanistan. \nLoss and Damage Fund is a Financial Intermediary Fund (FIF): VP explained WB’s role as a trustee, but not a “manager” of the Fund. Money never comes onto the WB balance sheet; he called WB’s role “very arm's length” when it comes to implementation. WB policies do not apply to FIF money, unless the WB itself implements a particular project funded from the fund.  DSO SD represents WB @ Board. \nED briefed on the Santiago Network and UNOPS’ role.\nGlobal Director for Environment noted WB preference for a unified offer from UN for programming. She noted that UN infighting can scare clients away.\nLuis Tineo noted potential for UNOPS to do more with WB on disaster risk management and resilience, noting UNOPS’ good record on adaptation.\nWB looking for more opportunities to share results stories on IDA. Story telling through case studies, getting behind the numbers, is key.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006Q1EkMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 6, 17, 21, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hy9zWIAR" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006Q2PJMA0",
                    new UNOPSInteraction
                    {
                        Name = "ED Jorge Moreira da Silva and WB MENA Director of Strategy and Operations Stefan Koerbele",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "ED Jorge Moreira da Silva and WB MENA Director of Strategy and Operations Stefan Koerbele",
                        Description = "Full mission report: https://docs.google.com/document/d/13q-NdKrQhRkTUyN2msVgISGiPHCPgF9ZliYJqioc__8/edit\n\nMeeting readout:\nDSO stated importance of partnership with UNOPS, and noted the importance of safeguards and other capacities. DSO thanked UNOPS for increasing number of E&S experts. Reiterated 24 hour reporting requirement on serious incidents, and that WB was under a lot of scrutiny on E&S. This would require real-time access to implementation information. He would like to be able to link financial aspects to the expected results on the ground.\nYemen: new Country Engagement Note in preparation, to cover the upcoming IDA cycle. Dina indicated UNOPS would be part of these discussions.\nYemen: discussed SDR fluctuations and losses on IDA projects in dollar amounts. Noted option of interest-bearing accounts to cover costs. However, WLO noted that this was still not enough to offset recent large losses. WLO referenced recent OPCS guidance (from WB Finance and Accounting) to include 5% buffer for future direct award projects. \nED updated on UNOPS reforms, and enhanced evaluation framework focused on outcomes and impact. \nRegional Procurement Manager noted some concerns expressed by the Iraqi government around pricing – that UNOPS’ prices were “20-25% higher” than other prices the government was able to obtain. He also praised UNOPS for its recent procurement award. \nOn Gaza, the ED briefed on development of the humanitarian access mechanism for Gaza, which the Bank said was “very welcome” to “avoid arbitrariness” and improve upon the former Gaza Reconstruction Mechanism, which had “not worked well” due to arbitrary and discretionary nature. \nWB is disbursing the last $7M tranche of $35M allotted for humanitarian response. Disbursed but 80% has not reached the territory. \nA Rapid Damage and Needs Assessment will be conducted when time allows. DSO indicated they are working closely with the Office of the Quartet.\nKey priority recovery steps: rubble clearance, water, and electricity. Described the need for “no regret” early rehabilitation. Emergency response plan for 18-24 months. \nRD for Sustainable Development asked about rubble removal and the need for a coordinated approach.\nED highlighted the importance of solarization and solar panel access to prevent overreliance on fuel imports or transmission lines from Israel. This received a very positive reaction from the WB.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006Q2PJMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 6, 17, 27, 14, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hy4yFIAR" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RRf3MAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with MCC",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-13").ToUniversalTime(),
                        Subject = "Meeting with MCC",
                        Description = "Prabhat: Let me know if this time works for you. We can use video or if you would rather do it by phone, we can do that too. If you give me your number, I can plan on calling you. My cell phone is 703-350-2832.  I look forward to catching up!  \n\nBest regards, Patrick",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RRf3MAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 11, 21, 22, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFAoXIAX" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RS4pMAG",
                    new UNOPSInteraction
                    {
                        Name = "Call with Layal",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-16").ToUniversalTime(),
                        Subject = "Call with Layal",
                        Description = "Let me know if this time works for you. Happy to connect via Whatsapp or Teams. Cheers, Patrick",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RS4pMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 12, 7, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFQA5IAP" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RTyXMAW",
                    new UNOPSInteraction
                    {
                        Name = "DMTC - UNOPS UN Web Buy",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-05").ToUniversalTime(),
                        Subject = "DMTC - UNOPS UN Web Buy",
                        Description = "See link below for our discussion on IT equipment project.\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZTBkN2MxMTUtOWJmNi00Mzk5LTk3ZjUtNDFiYzM0NzQwOWMx%40thread.v2/0?context=%7b%22Tid%22%3a%22325b4494-1587-40d5-bb31-8b660b7f1038%22%2c%22Oid%22%3a%227c57e5b9-538b-47ea-8248-b81f2f453802%22%7d>\nMeeting ID: 272 640 507 632\nPasscode: cwVPbB\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nteams@dnd-mdn.video.canada.ca<mailto:teams@dnd-mdn.video.canada.ca>\nVideo Conference ID: 117 823 814 0\nAlternate VTC instructions<https://pexip.me/teams/dnd-mdn.video.canada.ca/1178238140>\nOr call in (audio only)\n+1 343-803-5382,,337734844#<tel:+13438035382,,337734844#>   Canada, Ottawa-Hull\nPhone Conference ID: 337 734 844#\nFind a local number<https://dialin.teams.microsoft.com/3940e74b-d178-42f6-9c3f-0501ec86ba9e?id=337734844> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=7c57e5b9-538b-47ea-8248-b81f2f453802&tenantId=325b4494-1587-40d5-bb31-8b660b7f1038&threadId=19_meeting_ZTBkN2MxMTUtOWJmNi00Mzk5LTk3ZjUtNDFiYzM0NzQwOWMx@thread.v2&messageId=0&language=en-US>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RTyXMAW",
                        EmailAddresses = new List<string> { "teams@dnd-mdn.video.canada.ca" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("eleneag@unops.org".ToLower()) ? paoUserEmailMapping["eleneag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 11, 19, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GvqS6IAJ" },
                    new List<int> {  },
                    new List<string> { "eleneag@unops.org", "timl@unops.org", "patrickel@unops.org", "davidnm@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RYetMAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Stephanie INL",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-14").ToUniversalTime(),
                        Subject = "Meeting with Stephanie INL",
                        Description = null,
                        Location = "Tatte Bakery & Cafe, 2129 I St NW, Washington, DC 20037, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RYetMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 12, 4, 43, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000EqNQIIA3" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RZO2MAO",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS - MTCP Ukraine Cyber",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-21").ToUniversalTime(),
                        Subject = "UNOPS - MTCP Ukraine Cyber",
                        Description = "11:00 AM Ottawa / 6:00 PM KYIV\n_____________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_M2VlMTQ1M2EtZGZhMi00NjFiLThmOTAtYWIyOWUwZTBmOWU4%40thread.v2/0?context=%7b%22Tid%22%3a%22325b4494-1587-40d5-bb31-8b660b7f1038%22%2c%22Oid%22%3a%22faf19d1c-79d2-480c-b9fc-5edf0ce0fe6d%22%7d>\nMeeting ID: 240 378 715 649\n\nPasscode: ZDZjyd\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nteams@dnd-mdn.video.canada.ca\nVideo Conference ID: 116 259 163 8\nAlternate VTC instructions<https://pexip.me/teams/dnd-mdn.video.canada.ca/1162591638>\nOr call in (audio only)\n+1 343-803-5382,,703956568#<tel:+13438035382,,703956568>   Canada, Ottawa-Hull\nPhone Conference ID: 703 956 568#\nFind a local number<https://dialin.teams.microsoft.com/3940e74b-d178-42f6-9c3f-0501ec86ba9e?id=703956568> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=faf19d1c-79d2-480c-b9fc-5edf0ce0fe6d&tenantId=325b4494-1587-40d5-bb31-8b660b7f1038&threadId=19_meeting_M2VlMTQ1M2EtZGZhMi00NjFiLThmOTAtYWIyOWUwZTBmOWU4@thread.v2&messageId=0&language=en-US>\n___________________________________________________________________\nFrom: Elene AGLADZE <eleneag@unops.org>\nSent: Tuesday, February 20, 2024 10:34 AM\nTo: Raymond C@ADM(Pol) DMTC@Ottawa-Hull <CAMILLE.RAYMOND@forces.gc.ca>\nCc: Nguyen C@ADM(Pol) DMTC@Ottawa-Hull <Christina.Nguyen@forces.gc.ca>; davidnm@unops.org; timl@unops.org; patrickel@unops.org; valentynp@unops.org\nSubject: Re: UNOPS Field office Ukraine\n\nDear Camille,\n\nAbsolutely - we'd be happy to talk. Would it be possible to meet tomorrow at or after 17:30 pm Kyiv time? We have an in person meeting running until 5pm and with the commute may not make it until then.\n\nWe're also checking on some of the questions you raised and hope to address them at tomorrow's call.\n\nLooking forward to talking to you soon.\n\nKind regards,\nElene\n\nOn Mon, Feb 19, 2024 at 9:35 PM <CAMILLE.RAYMOND@forces.gc.ca<mailto:CAMILLE.RAYMOND@forces.gc.ca>> wrote:\nDear Elene,\n\nThank you very much for your email, and the quick response. This is great!\n\nI think it would be ideal to schedule a meeting this week to discuss the next steps. Would you be available this Wednesday Feb 21 at 9:30 am (Ottawa) / 16:30 pm (Kyiv)? Let me know and I’ll send a calendar invitation with a MS Teams link.\n\nTo answer and to add to some of your points below:\n\n  *   We just received approval for the attached DND-UNOPS contribution agreement template, which was used for another project for Haiti. Hopefully it will enable us to move forward quickly.\n  *   Regarding the justification for branding of the laptops, could you confirm in what form your would need this? Would the Annex A in the attached contribution agreement template be a good mechanism?\n  *   I have inquired about the technical questions below for the Yubikey and will let you know.\n  *   Does the approximation of $600K include UNOPS operational costs and the shipping?\n\nLooking forward to working with you!\n\nBest regards,\nCamille\n\nCamille Raymond (she/elle)\n\nPolicy Officer / Military Training and Cooperation Program\nDepartment of National Defence / Government of Canada\nOffice: 613-996-6670 / Mobile: 343-597-4136\n\nAgente des politiques / Programme de l’instruction et de la coopération militaire\nMinistère de la Défense nationale / Gouvernement du Canada\nBureau: 613-996-6670 / Cellulaire : 343-597-4136\n\n\n\nFrom: Elene AGLADZE <eleneag@unops.org<mailto:eleneag@unops.org>>\nSent: Monday, February 19, 2024 7:21 AM\nTo: Raymond C@ADM(Pol) DMTC@Ottawa-Hull <CAMILLE.RAYMOND@forces.gc.ca<mailto:CAMILLE.RAYMOND@forces.gc.ca>>\nCc: Nguyen C@ADM(Pol) DMTC@Ottawa-Hull <Christina.Nguyen@forces.gc.ca<mailto:Christina.Nguyen@forces.gc.ca>>; davidnm@unops.org<mailto:davidnm@unops.org>; timl@unops.org<mailto:timl@unops.org>; patrickel@unops.org<mailto:patrickel@unops.org>; Valentyn POVROZNYUK <valentynp@unops.org<mailto:valentynp@unops.org>>\nSubject: Re: UNOPS Field office Ukraine\n\nDear Camille,\n\nThank you for reaching out and sharing the exact list of items. Indeed, we confirm that UNOPS is ready to support the Ukraine cyber defence project through the procurement of below items.\n\nFew points to keep in mind:\n\n  *   Some of the equipment you highlight is branded (i.e. MacBook). Per our  usual practice, we do not procure a special band, but rely on the procurement process to identify the right goods, unless it is specifically justified by the donor/beneficiary. If we were to proceed with the purchase of particular branded items, we would require respective justification for branding.\n  *   Our initial market research showed that for the provision of MacBooks, we would need to factor in the minimum of 6 months for the suppliers.\n  *   From the technical perspective: For the Authentication keys (Yubikey 5), procurement colleagues would require information on the interface (USB-A or USB-C); The LG 34WP60C-B is out of market, however we can ensure alternative that can be used as  LG 34WP60C-B.\n  *   For the project to be launched, we might need to complete the UNOPS internal EAC and Human Rights Due Diligence process (HRDDP) for the beneficiary. We have done deliveries for the MoD of non-lethal goods before, so it should not be an issue, but I'm flagging it to consider it as part of the process.\n  *   Based on our preliminary estimation, the overall costs for the implementation of this project would amount to approximately USD 0.6 mln.\nWith these considerations in mind, if agreeable we would be happy to proceed further with this engagement and would be happy to discuss further.\n\nWith kind regards,\nElene\n\nOn Thu, Feb 15, 2024 at 5:28 PM <CAMILLE.RAYMOND@forces.gc.ca<mailto:CAMILLE.RAYMOND@forces.gc.ca>> wrote:\nGood day UNOPS colleagues,\n\nThank you again for meeting with us last week! As discussed, please find in the table below the complete list of equipment as well as the quantities requested we are looking into purchasing for our Ukraine cyber defence project. These items would be donated to the Armed Forces of Ukraine and would have to be delivered in Kyiv.\n\nCould you please advise on UNOPS ability to assist with the purchase of these items?\n\nItem\n\nAmount\n\nModel\n\nLaptops\n\n50\n\nMacBook Pro 14-inch M3 16Gb 512 SSD\n\n25\n\nMacBook Air 15-inch M2 16Gb 512 SSD\n\nMonitors\n\n30\n\nDell P2722H 27\" 16:9\n\n30\n\nASUS 29” 1080P Ultrawide HDR Monitor (VP299CL)\n\n15\n\nLG 34WP60C-B 34-Inch Curved UltraWide\n\nAuthentication Keys\n\n1,000\n\nYubikey 5 NFC\n\n1,000\n\nYubikey 5с NFC\n\n\nHappy to discuss as needed. Thank you for your time.\n\nKind regards,\nCamille\n\nCamille Raymond (she/elle)\n\nPolicy Officer / Military Training and Cooperation Program\nDepartment of National Defence / Government of Canada\nOffice: 613-996-6670 / Mobile: 343-597-4136\n\nAgente des politiques / Programme de l’instruction et de la coopération militaire\nMinistère de la Défense nationale / Gouvernement du Canada\nBureau: 613-996-6670 / Cellulaire : 343-597-4136",
                        Location = "MS Teams",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RZO2MAO",
                        EmailAddresses = new List<string> { "camille.raymond@forces.gc.ca", "davidnm@unops.org", "christina.nguyen@forces.gc.ca", "valentynp@unops.org", "teams@dnd-mdn.video.canada.ca", "eleneag@unops.org", "timl@unops.org", "patrickel@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("davidnm@unops.org".ToLower()) ? paoUserEmailMapping["davidnm@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 12, 8, 7, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw1sAIAR" },
                    new List<int> {  },
                    new List<string> { "davidnm@unops.org", "valentynp@unops.org", "eleneag@unops.org", "timl@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RatZMAS",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Ron (USAID) (T)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-14").ToUniversalTime(),
                        Subject = "Meeting with Ron (USAID) (T)",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RatZMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 12, 6, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GvpkaIAB" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RbHlMAK",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with State/USAID Gaza Coordination Cell",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-26").ToUniversalTime(),
                        Subject = "Meeting with State/USAID Gaza Coordination Cell",
                        Description = null,
                        Location = "U.S. Department of State, 2201 C St NW, Washington, DC 20451, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RbHlMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 12, 9, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFQV3IAP" },
                    new List<int> {  },
                    new List<string> { "christinebo@unops.org", "alistairs@unops.org", "usmana@unops.org", "patrickel@unops.org", "marijab@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006ReCPMA0",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Meeting with USAID Middle East",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-26").ToUniversalTime(),
                        Subject = "UNOPS Meeting with USAID Middle East",
                        Description = null,
                        Location = "ME Conference Room",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006ReCPMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 12, 28, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFQV3IAP" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006RecDMAS",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS West Bank",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-27").ToUniversalTime(),
                        Subject = "UNOPS West Bank",
                        Description = null,
                        Location = "U.S. Department of State Annex 1 (SA-1), 2401/2305 E St NW, Washington, DC 20226, USA, L630 Conference Room   Location – SA1",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006RecDMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 7, 12, 30, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000EqNQIIA3" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006TmAgMAK",
                    new UNOPSInteraction
                    {
                        Name = "SAVE THE DATE | UN Secretariat KPWG - Meeting 21",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "SAVE THE DATE | UN Secretariat KPWG - Meeting 21",
                        Description = "- UN Secretariat Key Partner Working Group meeting\nThe agenda: \nNew 2024 MoU\nHR services/partner personnel",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006TmAgMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("waedo@unops.org".ToLower()) ? paoUserEmailMapping["waedo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 8, 11, 49, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "waedo@unops.org", "sadias@unops.org", "amenthij@unops.org", "christaa@unops.org", "lorrainea@unops.org", "tatianaw@unops.org", "entelas@unops.org", "gracelh@unops.org", "francescap@unops.org", "cordulau@unops.org", "mikeb@unops.org", "rainerf@unops.org", "vineshw@unops.org", "aleksandrar@unops.org", "udanid@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006TzWdMAK",
                    new UNOPSInteraction
                    {
                        Name = "Foster Family homes in Ukraine",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "Foster Family homes in Ukraine",
                        Description = "UNOPS will present 2 potential setups and draft budget estimates for the construction of Foster Family Homes in Ukraine on behalf of the AP Møller Foundation and in close collaboration with the Olena Zelenska Foundation. <br><br>The two options will be: <br><ol><li>UNOPS acting as the implementer of the project </li><li>UNOPS providing technical, financial advice and supervision on the project</li></ol>At the meeting we will openly discuss pros and cons of the two different setups aiming to agree on the best approach to ensure that the project is realized, minimize donor concerns and maximize value for money.<br><br>Olena Zelenska Foundation <a href=\"https://drive.google.com/file/d/1iAnAE_guAyy3sTPEd7Zyoo8nv7mbA3QZ/view?usp=sharing\" target=\"_blank\">slidedeck </a>on the project for ease of reference. <br><br>UNOPS <a href=\"https://drive.google.com/file/d/1FhnyBdlyVxUEQZpjwTUCXdkrfwEmHwHW/view?usp=sharing\" target=\"_blank\">fee setting one-pager</a> for ease of reference. <br><br>Participants: <br><ul><li>Mette Thybo, EIFO<br></li><li>Mathias Secher, EIFO</li><li>Julie Munck Ewert, EIFO</li><li>Mads Ammitzbøll Thomsen, Ministry of Business<br></li><li>Simon August Søndergaard, Ministry of Business<br></li><li>Anne Kahl, Ministry of Foreign Affairs</li><li>Philip Klever, UNOPS</li><li>Elene Agladze, UNOPS</li><li>Teressa Rodriguez, UNOPS</li><li>Asbjørn Brink, UNOPS </li></ul>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006TzWdMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arunn@unops.org".ToLower()) ? paoUserEmailMapping["arunn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 8, 12, 56, 55, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF" },
                    new List<int> {  },
                    new List<string> { "arunn@unops.org", "teresam@unops.org", "valentynp@unops.org", "janphilipk@unops.org", "eleneag@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006U4dpMAC",
                    new UNOPSInteraction
                    {
                        Name = "Meeting PM office Jean Ellermann-Kingombe",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "Meeting PM office Jean Ellermann-Kingombe",
                        Description = "Here is your briefing material: <ul><li dir=\"ltr\"><p dir=\"ltr\"><a href=\"https://drive.google.com/drive/folders/11-QqzLl-kutGkpcFHpQGes4Cp65Vrfp7\" target=\"_blank\" class=\"pastedDriveLink-0\"><u>Jean Ellermann-Kingombe, Permanent Under-Secretary of State, The Prime Minister's Office</u></a></p></li></ul>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006U4dpMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mikaelag@unops.org".ToLower()) ? paoUserEmailMapping["mikaelag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 8, 13, 18, 4, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1123 },
                    new List<string> { "mikaelag@unops.org", "asbjornb@unops.org", "paulom@unops.org", "jorge.moreiradasilva@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006U4h3MAC",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Ambassador Nathalia Feinberg, Chief of Protocol",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "Meeting Ambassador Nathalia Feinberg, Chief of Protocol",
                        Description = "Ms. Astrid Ruge, Deputy Director of Protocol will also participate in the meeting.<br><br>Here is your briefing material: <ul><li dir=\"ltr\"><p dir=\"ltr\"><a href=\"https://drive.google.com/drive/folders/118oDUnfGoTjzGSmiYUeVeVnRDvDl9ccM\" target=\"_blank\" class=\"pastedDriveLink-0\"><u>Nathalia Feinberg, Chief of Protocol, Ministry of Foreign Affairs of Denmark</u></a></p></li></ul>",
                        Location = "Danish MFA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006U4h3MAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("raady@unops.org".ToLower()) ? paoUserEmailMapping["raady@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 8, 13, 18, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "raady@unops.org", "jorge.moreiradasilva@unops.org", "mikaelag@unops.org", "paulom@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006U5LNMA0",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with IFAD",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-02").ToUniversalTime(),
                        Subject = "Meeting with IFAD",
                        Description = "Introductory conversation to explore UNOPS's capacity to provide HR Services",
                        Location = "Copenhagen HQ",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006U5LNMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laurentium@unops.org".ToLower()) ? paoUserEmailMapping["laurentium@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 8, 13, 21, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HLkNpIAL" },
                    new List<int> { 1247 },
                    new List<string> { "laurentium@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006cMIcMAM",
                    new UNOPSInteraction
                    {
                        Name = "Involvering af OZ fonden",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-14").ToUniversalTime(),
                        Subject = "Involvering af OZ fonden",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006cMIcMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 7, 38, 32, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HhVBfIAN" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006cQ4TMAU",
                    new UNOPSInteraction
                    {
                        Name = "Follow-up on possible Novo Nordisk donation to Ukraine",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-15").ToUniversalTime(),
                        Subject = "Follow-up on possible Novo Nordisk donation to Ukraine",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_M2QwZGEyMjYtNmY2MC00NDg2LWFlNWMtOTRmYzc3NTU2MWYx%40thread.v2/0?context=%7b%22Tid%22%3a%22fdfed7bd-9f6a-44a1-b694-6e39c468c150%22%2c%22Oid%22%3a%225ae3671f-c152-49b5-8d11-5214c8772902%22%7d>\nMeeting ID: 385 478 035 290\nPasscode: obPHu5\n________________________________\nDial-in by phone\n+45 32 72 47 12,,875052709#<tel:+4532724712,,875052709> Denmark, All locations\nFind a local number<https://dialin.teams.microsoft.com/83985741-09c1-45b8-922e-280972b6be96?id=875052709>\nPhone conference ID: 875 052 709#\nJoin on a video conferencing device\nTenant key: meetnovonordisk@m.webex.com<mailto:meetnovonordisk@m.webex.com>\nVideo ID: 128 891 734 1\nMore info<https://www.webex.com/msteams?confid=1288917341&tenantkey=meetnovonordisk&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=5ae3671f-c152-49b5-8d11-5214c8772902&tenantId=fdfed7bd-9f6a-44a1-b694-6e39c468c150&threadId=19_meeting_M2QwZGEyMjYtNmY2MC00NDg2LWFlNWMtOTRmYzc3NTU2MWYx@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n[https://www.novonordisk.com/content/dam/Denmark/HQ/Commons/images/true-blue-logo-600px.image.68.0.png]Note: For an optimal meeting experience always use the Teams client instead of the dial-in number.\nOrg help<https://novonordisk.sharepoint.com/sites/LearnMicrosoft365/SitePages/Get-started-with-Microsoft-Teams.aspx>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting; NN1.2.43 DKBA MeetingRoom 4 pers.",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006cQ4TMAU",
                        EmailAddresses = new List<string> { "meetnovonordisk@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 7, 43, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hhaw6IAB" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006dndKMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with DOS ENR Bureau",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-08").ToUniversalTime(),
                        Subject = "Meeting with DOS ENR Bureau",
                        Description = null,
                        Location = "1775 K St NW, Washington, DC 20006, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006dndKMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 18, 15, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HkeFLIAZ" },
                    new List<int> {  },
                    new List<string> { "sofiag@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006doPkMAI",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Conor Savoy (T)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-12").ToUniversalTime(),
                        Subject = "Meeting with Conor Savoy (T)",
                        Description = "Conor: Let me know if you can still meet tomorrow. Also happy to meet later in the week. Best regards, Patrick",
                        Location = "TBD.",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006doPkMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 19, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hke0pIAB" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006dpnCMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Sarah Krech, Sierra Leone Desk Officer DOS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-04").ToUniversalTime(),
                        Subject = "Meeting with Sarah Krech, Sierra Leone Desk Officer DOS",
                        Description = null,
                        Location = "1775 K St NW, Washington, DC 20002, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006dpnCMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 15, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HkhWlIAJ" },
                    new List<int> {  },
                    new List<string> { "sofiag@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006dq9mMAA",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Shayna Halliwell, WFP",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-04").ToUniversalTime(),
                        Subject = "Meeting with Shayna Halliwell, WFP",
                        Description = null,
                        Location = "1775 K St NW, Washington, DC 20002, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006dq9mMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 15, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C2DofIAF" },
                    new List<int> {  },
                    new List<string> { "sofiag@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006dq9qMAA",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Update",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-25").ToUniversalTime(),
                        Subject = "UNOPS Update",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NTY2MmFjZmEtZDhjMi00MzJmLWJlYjUtMGU1YTM0ZmRjNTA1%40thread.v2/0?context=%7b%22Tid%22%3a%2266cf5074-5afe-48d1-a691-a12b2121f44b%22%2c%22Oid%22%3a%221a6180ac-f3b2-459e-9cd3-19a123cdd84f%22%7d>\nMeeting ID: 292 791 384 264\nPasscode: enF5Zp\n________________________________\nDial-in by phone\n+1 509-824-1908,,24729225#<tel:+15098241908,,24729225> United States, Spokane\nFind a local number<https://dialin.teams.microsoft.com/24359e3a-e3fd-47fe-8c6b-14f6797733ce?id=24729225>\nPhone conference ID: 247 292 25#\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=1a6180ac-f3b2-459e-9cd3-19a123cdd84f&tenantId=66cf5074-5afe-48d1-a691-a12b2121f44b&threadId=19_meeting_NTY2MmFjZmEtZDhjMi00MzJmLWJlYjUtMGU1YTM0ZmRjNTA1@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006dq9qMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 24, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HkZz8IAF" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006dqksMAA",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Greg Garramone",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-13").ToUniversalTime(),
                        Subject = "Meeting with Greg Garramone",
                        Description = null,
                        Location = "Casey's Coffee Inc, 508 23rd St NW, Washington, DC 20037, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006dqksMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 19, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw2THIAZ" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006dr7RMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Christopher Merriam USAID Sierra Leonne",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-05").ToUniversalTime(),
                        Subject = "Meeting with Christopher Merriam USAID Sierra Leonne",
                        Description = null,
                        Location = "1775 K St NW, Washington, DC 20006, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006dr7RMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 16, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HkidvIAB" },
                    new List<int> {  },
                    new List<string> { "sofiag@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006drCHMAY",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with MCC Steven Grudda",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-07").ToUniversalTime(),
                        Subject = "Meeting with MCC Steven Grudda",
                        Description = null,
                        Location = "Millennium Challenge Corporation, 1099 14th St NW Suite 700, Washington, DC 20005, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006drCHMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 17, 22, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HkiAmIAJ" },
                    new List<int> {  },
                    new List<string> { "sofiag@unops.org", "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006drKLMAY",
                    new UNOPSInteraction
                    {
                        Name = "Catch up with Nancy USUN",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-14").ToUniversalTime(),
                        Subject = "Catch up with Nancy USUN",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006drKLMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 14, 20, 19, 28, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw3p7IAB" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006etwrMAA",
                    new UNOPSInteraction
                    {
                        Name = "GRAND DANISH WATER DAY",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-15").ToUniversalTime(),
                        Subject = "GRAND DANISH WATER DAY",
                        Description = null,
                        Location = "Hannemanns Allé 53, 2300 København S",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006etwrMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 15, 11, 17, 58, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1123 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006gZkHMAU",
                    new UNOPSInteraction
                    {
                        Name = "CFO informal meetings with AA and BMF",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-14").ToUniversalTime(),
                        Subject = "CFO informal meetings with AA and BMF",
                        Description = "Karl-Ludwig SOLL <karls@unops.org>\nMay 15, 2024, 2:15 PM (18 hours ago)\nto me, Mariacarmen, Laetitia, Sonja, Lilian, Alexandra\n\nDear All,\n\nI just returned from a 2 day conference organized by the German Ministry of Foreign Affairs (AA).  The programme included meetings in the Ministry, the Bundeskanzleramt (BK) and members of parliament.  Fortunately the facilitators accommodated me to deviate from the standard programm, so there were opportunities for individual meetings with UNOPS stakeholders such as\n- Ms. Susanne Fries-Gaier - AA - Director for Humanitarian Assistance\n- Mr. Jens Hoch - AA - Head of Budget of Directorates-General\n- Ms. Anita Jansen - Ministry of Finance (BMF) - Focal point for UN budget matters\n\nI understand that there is a recognition of the need of collaboration with and support for UNOPS in the AA and the BMZ, however, all project funding suggested by them will have to be approved by the BMF. \n\nIn this regard Ms. Jansen clarified her expectations to be addressed before the BMF would authorize project funding for UNOPS again:\n- higher frequency of updates on COREP implementation and activities\n- successful finalization of efforts to recover S3i assets including the results of the UN tribunal case with respect to the former DED\n- enhancing transparency about the funds held by UNOPS (in her view Net-Zero should be interpreted as \"UNOPS no longer having slush funds\" rather than a requirement to return all project surplus)\n- addressing all concerns/recommendations raised by ACABQ, BoA, KPMG and other external parties - including the German supreme audit institution (Rechnungshof)\n- transparency about the profitability of UNOPS - concerns that UNOPS cross-subsidizes non-UN projects by overcharging UN organizations for services\n- concerns about UNOPS holding a quasi-monopoly on certain services in the system (e.g. mine-clearing)\n\nI understand these stakeholder positions are well known and expectations need to be managed; so I am looking forward to working on our approach for communications / explanations / responses to these concerns, so the reestablished goodwill of AA, BMZ and other German entities can be turned into future projects with approval by the BMF.  \n\nPlease let me know if you want to discuss this further, I am happy to explain further details.\n\nBest regards\nKarl\n\n--\nKarl-Ludwig W. Soll  | Chief Financial Officer | UNOPS Headquarters |  Marmorvej 51 | 2100 Copenhagen | Tel: +45 3154 5478 | www.unops.org \n\n\n\nEmilie POTVIN <emiliep@unops.org>\nMay 15, 2024, 4:57 PM (16 hours ago)\nto Karl-Ludwig, Alexandra, Lilian, Sonja, Laetitia, Mariacarmen\n\nDear Karl,\n\nThank you for having taken the time to informally meet with AA and BMF on the sidelines of the conference. As Laetitia would have discussed with you before your meeting, all projects discussed and proposed to BMZ/ AA / BMWK and BMUV in the last two years were in fact approved. BMF has not blocked any engagements so far, but is taking a 'stance' which they made clear in our last senior meetings with both Jorge and Jens previously.\n\nIn terms of engaging officially with BMF, as you may know, BMZ is the leading Ministry holding the partnership with UNOPS. BMZ State Secretary advised us that he would sort out the remaining issues internally with BMF- at his level. We continue to follow his position and cannot engage/exchange documents officially with BMF without them having consulted BMZ first (we just confirmed with BMZ; they are not aware of this additional list of documents requested by the BMF).\n  \nUNOPS’ message to the BMF colleague would therefore be that we would be happy to provide additional documents or organise a meeting to further discuss outstanding concerns, if we have an official request (i.e. BMZ being in the loop). \n\nHappy to discuss further and looking forward to our 1:1 tomorrow.\n\nÉmilie",
                        Location = "Berlin, Germany",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006gZkHMAU",
                        EmailAddresses = new List<string> { "emiliep@unops.org", "karls@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("emiliep@unops.org".ToLower()) ? paoUserEmailMapping["emiliep@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 16, 7, 28, 56, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1737 },
                    new List<string> { "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006jVFWMA2",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between Chinese Embassy in Denmark, Counselor M. Gao Xingle, and PLG Director",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-17").ToUniversalTime(),
                        Subject = "Meeting between Chinese Embassy in Denmark, Counselor M. Gao Xingle, and PLG Director",
                        Description = "The Counselor of Economic and Commercial of the Chinese Embassy in Denmark, Xingle GAO, met with PLG Director, Emilie Potvin, for the second time. He was primarily interested in having Chinese nationals join UNOPS, either as JPOs or in more senior positions. Emilie promised to put them in touch with PCG so they would receive more information on the JPO programmes, UNOPS contract modalities, and potential secondment opportunities. She also reminded the Counselor that China is welcome to encourage their nationals to apply for UNOPS positions and go through the competitive process.\n\nThe opening of a new Liaison Office to manage China, ADB and other Asian partners will help strengthen the collaboration between the two countries. Opportunities for collaboration between UNOPS and China were explored including:\n\n1. China's support to reconstructions efforts in conflict areas including Ukraine and Gaza;\n2. Support to the Chinese government in areas of joint interest including health procurement;\n3. Meeting between Jorge and the new Chinese Ambassador to Denmark.\n\nAction point:\n- Put PCG in contact with Chinese Embassy for JPOs\n- Follow up on potential meeting between the ED and the new Chinese Ambassador.",
                        Location = "UN City, Copenhagen",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006jVFWMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("emiliep@unops.org".ToLower()) ? paoUserEmailMapping["emiliep@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 17, 14, 35, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hw6T4IAJ" },
                    new List<int> { 1122 },
                    new List<string> { "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006oDReMAM",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS ICA templates to revise",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-29").ToUniversalTime(),
                        Subject = "UNOPS ICA templates to revise",
                        Description = "Meeting with UNHCR as per our request to update us on the recent changes to the UNHCR AI, policy changes and so on and to check what documents/processes might need changing.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006oDReMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laurentium@unops.org".ToLower()) ? paoUserEmailMapping["laurentium@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 21, 10, 3, 14, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GmsjPIAR" },
                    new List<int> { 1183 },
                    new List<string> { "laurentium@unops.org", "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006sHy6MAE",
                    new UNOPSInteraction
                    {
                        Name = "Catch up - UNOPS/DK",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-23").ToUniversalTime(),
                        Subject = "Catch up - UNOPS/DK",
                        Description = null,
                        Location = "Ministry of Foreign Affairs of Denmark, Asiatisk Pl. 2, 1448 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006sHy6MAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 23, 10, 30, 30, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006sRJ9MAM",
                    new UNOPSInteraction
                    {
                        Name = "Virtual meeting Mr. Hartzell (Sweden)Head of Department for Multilateral Governance and Humanitarian Policy at the Swedish MFA",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-21").ToUniversalTime(),
                        Subject = "Virtual meeting Mr. Hartzell (Sweden)Head of Department for Multilateral Governance and Humanitarian Policy at the Swedish MFA",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006sRJ9MAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 23, 10, 30, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1267 },
                    new List<string> { "asbjornb@unops.org", "mikaelag@unops.org", "jorge.moreiradasilva@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006sTEWMA2",
                    new UNOPSInteraction
                    {
                        Name = "Dokumenter til kommende EB session",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-23").ToUniversalTime(),
                        Subject = "Dokumenter til kommende EB session",
                        Description = "Kære Asbjørn – har I mulighed for at rykke dette møde et kvarter frem således at vi starter kl. 14.00?\n\nKære Asbjørn,\n\nMange tak for din mail og deling af materiale og vejledning. Vi vil meget gerne tage imod jeres tilbud om at mødes bilateralt inden annual session.\n\nVil I i samme ombæring have mulighed for at vende perspektiver for mulig UNOPS projektassistance til implementering af et potentielt flagskibsprojekt (beskednet budget), eller vil det da være andre UNOPS kollegaer vi skal have fat i?\n\nBedste hilsner,\nRikke\n\n_____________________________________________\nFrom: Asbjorn BRINK <asbjornb@unops.org<mailto:asbjornb@unops.org>>\nSent: 15 May 2024 09:36\nTo: Rikke Enggaard Olsen <rikoln@um.dk<mailto:rikoln@um.dk>>\nCc: Naimo HASSAN HIRSI <naimoh@unops.org<mailto:naimoh@unops.org>>; Julia Winding <julwin@um.dk<mailto:julwin@um.dk>>\nSubject: Dokumenter til kommende EB session\n\n[CAUTION - EXTERNAL EMAIL] This email was sent from outside the MFA organisation. DO NOT reply, click on links, or open attachments unless you have verified the sender and know the content is safe.\n\nKære Rikke,\n\n\n\nJeg rækker ud til dig da vi allerede nu nærmer os den næste årsmøde i EB den 3.-7. juni. Jeg har nedenfor indsat et kort overblik over de (mange!) dokumenter og emner, der skal drøftes på det kommende EB, der vedrører UNOPS.\n\n\n\nNedenfor finder du kort oversigt og links til alle dokumenter. Når du har haft mulighed for at danne dig et overblik, håbede jeg, at vi kunne mødes til en uformel snak om jeres umiddelbare og uformelle feedback på emnerne og dokumenterne. Hvis det er muligt, gerne i løbet af i næste uge, når det kunne passe dig?\n\n\n\n\n\nEmner og dokumenter til juni EB session\n\n\n\nIntern revision og efterforskningsaktiviteter i 2023\no\n\no\n\no   Rapporten indeholder: (a) en udtalelse om tilstrækkeligheden og effektiviteten af UNOPS-rammerne\n\no    for styring, risikostyring og kontrol; b) et resumé af arbejdet og de kriterier, der understøtter udtalelsen; (c) en erklæring om overensstemmelse med de interne revisionsstandarder, der overholdes; og (d) et syn på, hvorvidt ressourcer til funktionen er passende,\n\no    tilstrækkelig og effektivt implementeret for at opnå den ønskede interne revisions- og undersøgelsesdækning.\no\no\n\no\n\no   Hoveddokument<https://urldefense.com/v3/__https:/undocs.org/en/DP/OPS/2024/4__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvItQ-PL4A$>\n\no    fremsendt til EB<https://urldefense.com/v3/__https:/undocs.org/en/DP/OPS/2024/4__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvItQ-PL4A$>\no\no\n                                                                   i.\n\n                                                                  ii.\n\n                                                               iii.          bilag<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-1-Audit-and-advisory-reports-issued-in-2023.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvL_F29Kvw$>\n\n                                                                         iv.1<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-1-Audit-and-advisory-reports-issued-in-2023.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvL_F29Kvw$> (revisionsrapporter i 2023)\n                                                                  v.\n                                                                 vi.\n\n                                                               vii.\n\n                                                            viii.          bilag<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-2-Open-agreed-actions-older-than-18-months-as-at-31-December-2023.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvIiEU8AvA$>\n\n                                                                         ix.2<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-2-Open-agreed-actions-older-than-18-months-as-at-31-December-2023.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvIiEU8AvA$> (åbne handlinger ældre end 18 måneder)\n                                                                  x.\n                                                                 xi.\n\n                                                               xii.\n\n                                                            xiii.          bilag<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-3-Recommendations-closed-due-to-risks-being-accepted-being-no-longer-applicable-or-being-withdrawn-or-due-to-disagreements-with-audit-recommendations.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvKeVYvDPA$>\n\n                                                                      xiv.3<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-3-Recommendations-closed-due-to-risks-being-accepted-being-no-longer-applicable-or-being-withdrawn-or-due-to-disagreements-with-audit-recommendations.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvKeVYvDPA$> (anbefalinger lukket)\n                                                                xv.\n                                                              xvi.\n\n                                                             xvii.\n\n                                                        xviii.          bilag<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-4-Charter-of-the-Office-of-Audit-and-Investigations.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvKQITllGw$>\n\n                                                                      xix.4<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-4-Charter-of-the-Office-of-Audit-and-Investigations.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvKQITllGw$> (IAIGs charter)\n                                                                xx.\n                                                              xxi.\n\n                                                             xxii.\n\n                                                        xxiii.          bilag<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-internal-audit-and-investigation/en/dpops2024-4-Annex-5-Key-performance-indicators.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!NDPhUZ67Yhw__9b5Jy1eWBr_OrhAHmKp_cm2Se7DfbB1XcC0g789Evg78GJFf1UI_-cvpvLGmXFJQQ$>\n\n                                                                   xxiv.5<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/annual-session/joint-segment-item-3-i",
                        Location = "UM (lokale 2EF)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006sTEWMA2",
                        EmailAddresses = new List<string> { "julwin@um.dk", "naimoh@unops.org", "asbjornb@unops.org", "rikoln@um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 23, 10, 30, 25, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BQI9SIAX" },
                    new List<int> {  },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006sUfBMAU",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up ahead of June EB session",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-21").ToUniversalTime(),
                        Subject = "Catch-up ahead of June EB session",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006sUfBMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 23, 10, 30, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000099IS3IAM" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006tNzpMAE",
                    new UNOPSInteraction
                    {
                        Name = "Meridian Election Briefing",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-20").ToUniversalTime(),
                        Subject = "Meridian Election Briefing",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006tNzpMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 23, 19, 6, 6, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1145 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006tT17MAE",
                    new UNOPSInteraction
                    {
                        Name = "Spring Meetings | in-person prep meeting",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-18").ToUniversalTime(),
                        Subject = "Spring Meetings | in-person prep meeting",
                        Description = "Preparatory discussion and catch up before ED’s arrival in Washington.",
                        Location = "World Bank",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006tT17MAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 23, 20, 51, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HxyhbIAB" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006tT9BMAU",
                    new UNOPSInteraction
                    {
                        Name = "UN-IFI Ad Hoc Working Group Mtg with Catherine Defontaine (FCV Group)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-07").ToUniversalTime(),
                        Subject = "UN-IFI Ad Hoc Working Group Mtg with Catherine Defontaine (FCV Group)",
                        Description = "Briefing by Catherine DeFontaine, Senior Operations Officer, FCV Group. Discussed marginalization of FCV as a theme within the Bank, challenges around linking Risk and Resilience Assessments to project outcomes. Reach out to Alistair Somerville for more information.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006tT9BMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 23, 20, 56, 3, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hy4wbIAB" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000006u3z3MAA",
                    new UNOPSInteraction
                    {
                        Name = "Ukraine MFA support: Request for a meeting regarding overall progress",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-22").ToUniversalTime(),
                        Subject = "Ukraine MFA support: Request for a meeting regarding overall progress",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_OWMyYmE4NGQtZTBkMy00YzA4LWExZjMtYmQyYTgxNzM3MzY5%40thread.v2/0?context=%7b%22Tid%22%3a%2241cdff95-bd23-4ad6-8e53-b8528f9a4259%22%2c%22Oid%22%3a%226b925156-47be-4322-a9d7-068bc8d7c340%22%7d>\nMeeting ID: 339 618 360 163\nPasscode: ziMMGk\n________________________________\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=6b925156-47be-4322-a9d7-068bc8d7c340&tenantId=41cdff95-bd23-4ad6-8e53-b8528f9a4259&threadId=19_meeting_OWMyYmE4NGQtZTBkMy00YzA4LWExZjMtYmQyYTgxNzM3MzY5@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________\n\n\n_____________________________________________\nFrom: Jan Philip KLEVER <janphilipk@unops.org>\nSent: Wednesday, 15 May 2024 17.51\nTo: Anders Thyge <ate@ncg.dk>\nCc: Christian Krone <christian@krone-controlling.com>; Johnny Flentø <johfle@um.dk>; Max Mortensen <maxmor@um.dk>; Naimo HASSAN HIRSI <naimoh@unops.org>; Asbjorn BRINK <asbjornb@unops.org>; Teresa MOLERO RODRIGUEZ <teresam@unops.org>; Arun NARAYANAN <arunn@unops.org>; Svitlana ZAKRYNYTSKA <svitlanaz@unops.org>; Svitlana Stadnyk <svitlanas@unops.org>\nSubject: Re: Ukraine MFA support: Request for a meeting regarding overall progress\n\nDear Anders,\n\nWe look forward to having a project progress discussion with you. We just recently welcomed a delegation from the Danish MFA to Mykjolaiv and Kyiv for a monitoring / progress visit, including Mr. Mortensen.\n\nWe therefore have everything ready for any questions you may have. We take note of the five points of discussion to be covered listed below as per your ToR.\n\nIn terms of timing, from today's perspective we could suggest Wednesday 22 May, between 1100 and 1300, Thursday 23 May, 1000 - 1100, and 1300 - 1400, or Friday 24 May between 1400 and 1600. The week of 27 May most of us will be travelling hence it would be challenging to arrange for a meeting.\n\nI have copied @Arun NARAYANAN<mailto:arunn@unops.org>, our Infrastructure Portfolio Lead, @Teresa MOLERO RODRIGUEZ<mailto:teresam@unops.org> the Project Manager on the infrastructure project, and @Svitlana Stadnyk<mailto:svitlanas@unops.org> the Project Manager on the procurement project.\n\nBest Regards,\n\nPhilip\n\nJan Philip Klever | Head of Programme | UNOPS Ukraine Multi-Country Office | Kyiv, Ukraine | Tel. (+380) 95 278 4072  | Email: janphilipk@unops.org<mailto:janphilipk@unops.org>\n[https://content.unops.org/assets/img/logos/UNOPS_Logo_Email.png]\n\n\nOn Wed, May 15, 2024 at 6:48 AM Asbjorn BRINK <asbjornb@unops.org<mailto:asbjornb@unops.org>> wrote:\nDear Anders\n\nMy colleague Philip (cc this email) who is our Ukraine Country Director ai will reach out to you to identify possible time slots for the interview. Kindly include me in the Teams Invite once you’ve identified a workable time slot.\n\nThank you in advance.\n\nBest,\nAsbjørn\n\nAsbjørn Brink | Head of Northern Europe Liaison Office | Partnerships and Liaison Group | UNOPS HQ | Copenhagen, Denmark | Mob: +45 40 80 36 54 | www.unops.org<https://www.unops.org/english/Pages/Home.aspx>\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org/>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official/?hl=sv>, YouTube<https://www.youtube.com/user/UNOPSofficial>\nSubscribe to our external newsletter in English<https://mailchi.mp/8987deaa0e61/uojskr902j>, French<https://mailchi.mp/unops.org/gu0ld93is9> or Spanish<https://mailchi.mp/edf4b84d88cf/on3qnjwzkh>.\n\n[https://lh7-us.googleusercontent.com/nPxpbWSDHa0CL7JyQZX-4y85Mxn4y5gEFRdjC7t_CF_ptBC0_9XBSY2ZptDg9-nyWZQzuWxNqh-iqm0hCjymAwDk6KVXl7CvNjzi3PTAkCxhXNXzCWRmkUFCpULjbizB2mni1nMhp9Bzvs4_VZ_uU2I]\n\n\nOn Tue, 14 May 2024 at 22.38, Anders Thyge <ate@ncg.dk<mailto:ate@ncg.dk>> wrote:\nDear Asbjørn,\n\nThe Danish Ministry of Foreign Affairs is currently undertaking a thematic review of Denmark’s support to early recovery and reconstruction in Mykolaiv and Ukraine covering the period 2022-24. Your organisation has received support during this period.\n\nMy name is Anders Thyge, and I am a consultant hired to support and assist the MFA on assessing overall progress on the different projects. I am therefore contacting you to request a meeting to discuss the overall organisational set-up, progress on the ground, ownership by national stakeholders and international coordination. I will be joined by Johnny Flentø, Team Leader on the Review from MFA.\n\nWho would we like to meet with?\nWe would request a meeting with the person responsible for the Danish grant. Please therefore forward this email to the person concerned.\n\nWhen?\nWe would request 1½ hour meeting next week (22-31 May) to discuss the subjects. As this assignment requires us to meet and coordinate similar meetings with 8-9 organisations, it would operational if you could suggest a couple of timeslots. Let us know by email your preferred time and we will call for a MS Teams meeting.\n\nTopics to cover – ref our ToR\n\n\n  *   Delivery and output so far, including the extent to which deliveries have been implemented and served their intended purposes during and after project closure.\n  *   Relevance and effectiveness, (to the extent feasible, efficiency and sustainability), on progress.\n  *   Assessment of partner’s and projects contribution to enhance local capacity.\n  *   Assessment of whether national stakeholders have been involved in decision-making, identification of needs and selection of activities.\n  *   Assessment of whether international stakeholders have been sufficiently involved and informed about Denmark’s activities to allow for prevention of undue overlap and duplication.\n\nWe are looking very much forward to meeting with you and discuss achievements and challenges relating to these important interventions.\n\nRegards\nJohnny and Anders",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000006u3z3MAA",
                        EmailAddresses = new List<string> { "arunn@unops.org", "christian@krone-controlling.com", "teresam@unops.org", "svitlanas@unops.org", "svitlanaz@unops.org", "janphilipk@unops.org", "maxmor@um.dk", "johfle@um.dk", "ate@ncg.dk", "naimoh@unops.org", "asbjornb@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arunn@unops.org".ToLower()) ? paoUserEmailMapping["arunn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 24, 6, 54, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "arunn@unops.org", "teresam@unops.org", "svitlanas@unops.org", "janphilipk@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072MBkMAM",
                    new UNOPSInteraction
                    {
                        Name = "Follow-Up Meeting Between Bank and UNOPS Finance Teams",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-13").ToUniversalTime(),
                        Subject = "Follow-Up Meeting Between Bank and UNOPS Finance Teams",
                        Description = "Agenda items:\n\n\n  *   UN Commitment\n  *   FX and Contingency Budgeting\n  *   Possible Learning Opportunities\n  *   DNP Management (if applicable)\n\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_N2Y2OTY3Y2UtMGVkYy00OTA5LThjNjAtMTE1ZmQwOWNlODY4%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22b24b5572-7221-48c1-81e0-3cc2e41e4792%22%7d>\nMeeting ID: 252 387 411 190\nPasscode: yKrY24\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com<mailto:wbg@m.webex.com>\nVideo Conference ID: 119 934 101 1\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1199341011&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,725359234#<tel:+15094080991,,725359234#>   United States, Spokane\nPhone Conference ID: 725 359 234#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=725359234> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=b24b5572-7221-48c1-81e0-3cc2e41e4792&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_N2Y2OTY3Y2UtMGVkYy00OTA5LThjNjAtMTE1ZmQwOWNlODY4@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072MBkMAM",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 15, 33, 27, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IobiAIAR" },
                    new List<int> {  },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072T1uMAE",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-WB Monthly Meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-23").ToUniversalTime(),
                        Subject = "UNOPS-WB Monthly Meeting",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NDQ5MDYzZjAtNjExNS00NjdmLTlkMTgtNDg2YzIxOTFjZWQ2%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%2233191784-efaf-48e2-a9fe-f3be8cb2e366%22%7d>\nMeeting ID: 211 962 084 617\nPasscode: QpjZHn\n________________________________\nDial-in by phone\n+1 509-408-0991,,745626136#<tel:+15094080991,,745626136> United States, Spokane\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=745626136>\nPhone conference ID: 745 626 136#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com<mailto:wbg@m.webex.com>\nVideo ID: 117 533 464 4\nMore info<https://www.webex.com/msteams?confid=1175334644&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=33191784-efaf-48e2-a9fe-f3be8cb2e366&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NDQ5MDYzZjAtNjExNS00NjdmLTlkMTgtNDg2YzIxOTFjZWQ2@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting; MC 10-572 (18) (VC) Private, MC 10-500 (16) (VC) Private, MC 10-348 (25) (VC) Private, MC 10-572 (18) (VC) Private",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072T1uMAE",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 15, 48, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072TA8MAM",
                    new UNOPSInteraction
                    {
                        Name = "Rajeev / Christine coffee",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-09").ToUniversalTime(),
                        Subject = "Rajeev / Christine coffee",
                        Description = "thanks for getting me a building pass, see you soon!",
                        Location = "MC atrium",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072TA8MAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 15, 39, 44, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BTpVYIA1" },
                    new List<int> {  },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072TJfMAM",
                    new UNOPSInteraction
                    {
                        Name = "Introduction UNOPS / Mohamed",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-16").ToUniversalTime(),
                        Subject = "Introduction UNOPS / Mohamed",
                        Description = null,
                        Location = "Tatte Bakery & Cafe | Farragut Square, 1634 I St NW, Washington, DC 20006, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072TJfMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 15, 46, 43, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IolZHIAZ" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072UncMAE",
                    new UNOPSInteraction
                    {
                        Name = "UP Forum in Sierra Leone",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-07").ToUniversalTime(),
                        Subject = "UP Forum in Sierra Leone",
                        Description = "Discussion on UP Forum run in Sierra Leone, including lessons learned and opportunities for the future\n\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NDQyY2IwYzEtMWM0MS00YzUxLWJmYWYtNWI3OWVkY2M4YjBj%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%224a5a2905-6177-478a-bc1d-64618cb688d9%22%7d>\nMeeting ID: 248 840 287 89\nPasscode: zvggsh\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com\nVideo Conference ID: 117 637 155 3\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1176371553&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,989715168#<tel:+15094080991,,989715168#>   United States, Spokane\nPhone Conference ID: 989 715 168#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=989715168> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=4a5a2905-6177-478a-bc1d-64618cb688d9&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NDQyY2IwYzEtMWM0MS00YzUxLWJmYWYtNWI3OWVkY2M4YjBj@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "MC 9-300 (20) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072UncMAE",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 15, 37, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000DUQTuIAP" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072WuFMAU",
                    new UNOPSInteraction
                    {
                        Name = "23455 Afghanistan CRL AF negotiations",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-10").ToUniversalTime(),
                        Subject = "23455 Afghanistan CRL AF negotiations",
                        Description = "This is a placeholder for the Afghanistan Community Resilience and Livelihoods AF negotiations.\n\nKaty – feel free to share with UNOPS’ colleagues as needed.\n\nMany tks, Susan\n\n\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting&lt;<a href=\"https://www.google.com/url?q=https://teams.microsoft.com/l/meetup-join/19%253ameeting_OTlkY2MwZTUtZmRjMS00YjJiLWEzYzUtYjY4MmYyMzk2Zjk2%2540thread.v2/0?context%3D%257b%2522Tid%2522%253a%252231a2fec0-266b-4c67-b56e-2796d8f59c36%2522%252c%2522Oid%2522%253a%2522b7e8a9b4-814f-44eb-bb15-9e784822610c%2522%257d&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw2iSSeT0250RUcjX5g9CzG0\" target=\"_blank\">https://teams.microsoft.com/l/meetup-join/19%3ameeting_OTlkY2MwZTUtZmRjMS00YjJiLWEzYzUtYjY4MmYyMzk2Zjk2%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22b7e8a9b4-814f-44eb-bb15-9e784822610c%22%7d</a>&gt;\nMeeting ID: 250 287 932 479\nPasscode: tiyF2h\nDownload Teams&lt;<a href=\"https://www.google.com/url?q=https://www.microsoft.com/en-us/microsoft-teams/download-app&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw37O5rB3Weyc1qP-IdFcbyH\" target=\"_blank\">https://www.microsoft.com/en-us/microsoft-teams/download-app</a>&gt; | Join on the web&lt;<a href=\"https://www.google.com/url?q=https://www.microsoft.com/microsoft-teams/join-a-meeting&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw1chgYeVoXGB9jJjRJ9D4Iz\" target=\"_blank\">https://www.microsoft.com/microsoft-teams/join-a-meeting</a>&gt;\nJoin with a video conferencing device\n<a href=\"mailto:wbg@m.webex.com\" target=\"_blank\">wbg@m.webex.com</a>&lt;mailto:<a href=\"mailto:wbg@m.webex.com\" target=\"_blank\">wbg@m.webex.com</a>&gt;\nVideo Conference ID: 118 890 368 7\nAlternate VTC instructions&lt;<a href=\"https://www.google.com/url?q=https://www.webex.com/msteams?confid%3D1188903687%26tenantkey%3Dwbg%26domain%3Dm.webex.com&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw1A_Lx7TwStpgpHbnZHXP-4\" target=\"_blank\">https://www.webex.com/msteams?confid=1188903687&amp;tenantkey=wbg&amp;domain=m.webex.com</a>&gt;\nOr call in (audio only)\n+1 509-408-0991,,220787948#&lt;tel:+15094080991,,220787948#&gt;   United States, Spokane\nPhone Conference ID: 220 787 948#\nFind a local number&lt;<a href=\"https://www.google.com/url?q=https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id%3D220787948&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw0STtfGozN-LhUTB-XPVqRS\" target=\"_blank\">https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=220787948</a>&gt; | Reset PIN&lt;<a href=\"https://www.google.com/url?q=https://dialin.teams.microsoft.com/usp/pstnconferencing&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw1hlirhWOtfPIr0ZjHaRFMG\" target=\"_blank\">https://dialin.teams.microsoft.com/usp/pstnconferencing</a>&gt;\nLearn More&lt;<a href=\"https://www.google.com/url?q=https://aka.ms/JoinTeamsMeeting&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw037AP3Nu-CvrdT3PyvBlwe\" target=\"_blank\">https://aka.ms/JoinTeamsMeeting</a>&gt; | Meeting options&lt;<a href=\"https://www.google.com/url?q=https://teams.microsoft.com/meetingOptions/?organizerId%3Db7e8a9b4-814f-44eb-bb15-9e784822610c%26tenantId%3D31a2fec0-266b-4c67-b56e-2796d8f59c36%26threadId%3D19_meeting_OTlkY2MwZTUtZmRjMS00YjJiLWEzYzUtYjY4MmYyMzk2Zjk2@thread.v2%26messageId%3D0%26language%3Den-US&amp;sa=D&amp;source=calendar&amp;ust=1704766491707314&amp;usg=AOvVaw32BZ64Lr4vkKe6vD6l1gK3\" target=\"_blank\">https://teams.microsoft.com/meetingOptions/?organizerId=b7e8a9b4-814f-44eb-bb15-9e784822610c&amp;tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&amp;threadId=19_meeting_OTlkY2MwZTUtZmRjMS00YjJiLWEzYzUtYjY4MmYyMzk2Zjk2@thread.v2&amp;messageId=0&amp;language=en-US</a>&gt;\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072WuFMAU",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 15, 26, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072aL7MAI",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS / WB framework agreement",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-29").ToUniversalTime(),
                        Subject = "UNOPS / WB framework agreement",
                        Description = "1225 Connecticut Avenue NW, Washington, DC 20433, USA<https://www.google.com/maps/search/1225+Connecticut+Avenue+NW,+Washington,+DC+20433,+USA?entry=gmail&source=g> Corporate Procurement is in a secured work area. Stop by the lobby security for a visitor badge and take any elevator to the 4th FL.\n\n________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZmQ5NmE5Y2ItNDNmNC00ZGZjLWJmZDItN2Q1ZTM1NzBlMjRl%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%224f737b84-041c-439b-8b79-d0acaa0785b3%22%7d>\nMeeting ID: 270 752 331 926\nPasscode: PfVCt8\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com\nVideo Conference ID: 119 722 247 3\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1197222473&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,75441496#<tel:+15094080991,,75441496#>   United States, Spokane\nPhone Conference ID: 754 414 96#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=75441496> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=4f737b84-041c-439b-8b79-d0acaa0785b3&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_ZmQ5NmE5Y2ItNDNmNC00ZGZjLWJmZDItN2Q1ZTM1NzBlMjRl@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "C 4-220 (10) (VC) Private",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072aL7MAI",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 15, 31, 9, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IoYahIAF" },
                    new List<int> {  },
                    new List<string> { "elizabethdu@unops.org", "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072jJbMAI",
                    new UNOPSInteraction
                    {
                        Name = "Quick chat: IDA21/UNOPS working level",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-29").ToUniversalTime(),
                        Subject = "Quick chat: IDA21/UNOPS working level",
                        Description = "https://www.worldbank.org/en/news/immersive-story/2021/06/21/heeding-the-call-of-a-country-in-crisis-world-bank-and-partners-in-yemen",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072jJbMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 17, 44, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IpIYeIAN" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072kvZMAQ",
                    new UNOPSInteraction
                    {
                        Name = "UNICEF HR Engagement",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-30").ToUniversalTime(),
                        Subject = "UNICEF HR Engagement",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072kvZMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arshmahj@unops.org".ToLower()) ? paoUserEmailMapping["arshmahj@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 17, 16, 59, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1185 },
                    new List<string> { "arshmahj@unops.org", "mirzaaamirr@unops.org", "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072ob3MAA",
                    new UNOPSInteraction
                    {
                        Name = "Lunch Margarita-Alistair",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-11").ToUniversalTime(),
                        Subject = "Lunch Margarita-Alistair",
                        Description = "Meeting with VP Mamta Murthi staffer Margarita Isaacs ahead of ED meeting with VP at Spring Meetings",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072ob3MAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 15, 19, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072qxqMAA",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/WB",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-28").ToUniversalTime(),
                        Subject = "UNOPS/WB",
                        Description = "________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZWI0OGIzZDQtYzUxMi00ODdmLWJhMDAtMTAzODRiOGQ5ZDFk%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%225d8fa2ce-0903-44fe-b6de-91f669c8c6eb%22%7d>\nMeeting ID: 244 168 579 569\nPasscode: V9Cmxe\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com\nVideo Conference ID: 117 449 886 2\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1174498862&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,560364626#<tel:+15094080991,,560364626#>   United States, Spokane\nPhone Conference ID: 560 364 626#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=560364626> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=5d8fa2ce-0903-44fe-b6de-91f669c8c6eb&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_ZWI0OGIzZDQtYzUxMi00ODdmLWJhMDAtMTAzODRiOGQ5ZDFk@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "MC 6-860 (50) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072qxqMAA",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 28, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Ipu2fIAB" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072rloMAA",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Andrew Hyde, Stimson Center",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-07").ToUniversalTime(),
                        Subject = "Meeting with Andrew Hyde, Stimson Center",
                        Description = "Discussion on renewables and peacekeeping + Sierra Leone model for UNOPS",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072rloMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 25, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Ipy2zIAB" },
                    new List<int> { 1145 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072vKkMAI",
                    new UNOPSInteraction
                    {
                        Name = "Sofia Goinhas at WB Corporate Procurement conference",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-05").ToUniversalTime(),
                        Subject = "Sofia Goinhas at WB Corporate Procurement conference",
                        Description = "Sierra Leone Country Manager Sofia Goinhas delivered remarks on gender-responsive procurement at the World Bank Corporate Procurement department conference",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072vKkMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 17, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "sofiag@unops.org", "alistairs@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072vW2MAI",
                    new UNOPSInteraction
                    {
                        Name = "Face 2 face meeting with Abeer al-Mas on gender procurement in Yemen",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-29").ToUniversalTime(),
                        Subject = "Face 2 face meeting with Abeer al-Mas on gender procurement in Yemen",
                        Description = "________________________________________________________________________________\n\nMicrosoft Teams meeting\n\nJoin on your computer, mobile app or room device\n\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NDYxYzJlMGUtMTBiOC00YzY2LWIzYjktYzRkMzE0YTk5ZDk1%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22757ebba3-d1e3-48a4-97ef-e5ba4f7970c4%22%7d>\n\nMeeting ID: 253 692 463 476\nPasscode: HCgmKs\n\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\n\nJoin with a video conferencing device\n\nwbg@m.webex.com\n\nVideo Conference ID: 119 390 025 3\n\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1193900253&tenantkey=wbg&domain=m.webex.com>\n\nOr call in (audio only)\n\n+1 509-408-0991,,143373123#   United States, Spokane\n\nPhone Conference ID: 143 373 123#\n\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=143373123> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=757ebba3-d1e3-48a4-97ef-e5ba4f7970c4&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NDYxYzJlMGUtMTBiOC00YzY2LWIzYjktYzRkMzE0YTk5ZDk1@thread.v2&messageId=0&language=en-US>\n\n________________________________________________________________________________\n\n\n\nHi All,\n\n\n\nOur colleagues at UNOPS have offered to arrange a conversation with Abeer al-Mas, who led the women-owned businesses work in Yemen. She will be at the Fragility Forum this week, delivering a \"chai chat\" presentation on the project. This face to face session for Bank procurement staff, which follows on from the chai chat, will give us the chance to ask questions and hear more about the initiative.\n\n\n\nMajed, Manjola, feel free to pass on to DC-based colleagues who may be interested.\n\nMany thanks\n\nAndy",
                        Location = "MC 9-300 (20) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072vW2MAI",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("marijab@unops.org".ToLower()) ? paoUserEmailMapping["marijab@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 29, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000DUQTuIAP" },
                    new List<int> {  },
                    new List<string> { "marijab@unops.org", "alistairs@unops.org", "christinebo@unops.org", "abeera@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072vcTMAQ",
                    new UNOPSInteraction
                    {
                        Name = "ED call with U.S. Senator Chris Van Hollen + dial in",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-07").ToUniversalTime(),
                        Subject = "ED call with U.S. Senator Chris Van Hollen + dial in",
                        Description = "<p><br></p><p>Dear Jorge, <span>please find</span><span> </span><a href=\"https://drive.google.com/drive/folders/1fGZZvN-U4OOGOf5s2-7lFEP9C9Hhc51W\" class=\"pastedDriveLink-0\">at this link</a><span> </span><span>your briefing package.</span></p><p>The dial is below:<span> </span></p><p><u></u></p><p>+16468287666,,1610753991#,,,,*<u></u>61918192# (one-click)<u></u><u></u></p><p>+1 646 828 7666<u></u><u></u></p><p>Meeting ID: 161 075 3991<u></u><u></u></p><p>Passcode: 61918192</p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072vcTMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 20, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1145 },
                    new List<string> { "christinebo@unops.org", "emiliep@unops.org", "alistairs@unops.org", "jorge.moreiradasilva@unops.org", "mikaelag@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072w8jMAA",
                    new UNOPSInteraction
                    {
                        Name = "Abeer Brown Bag Lunch, Regional Director",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-28").ToUniversalTime(),
                        Subject = "Abeer Brown Bag Lunch, Regional Director",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072w8jMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 28, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000072wALMAY",
                    new UNOPSInteraction
                    {
                        Name = "Conversation with Nick George from UNOPS on Community Resilience and Livelihoods Project in Afghanistan",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-28").ToUniversalTime(),
                        Subject = "Conversation with Nick George from UNOPS on Community Resilience and Livelihoods Project in Afghanistan",
                        Description = "________________________________________________________________________________\nMicrosoft Teams meeting\nJoin on your computer, mobile app or room device\nClick here to join the meeting<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NDc4N2M3ZDUtNTE3Ny00Y2I1LTk1MTgtMTg0MGY2ZmVmNmE4%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%224a5a2905-6177-478a-bc1d-64618cb688d9%22%7d>\nMeeting ID: 240 012 814 986\nPasscode: d6GR8k\nDownload Teams<https://www.microsoft.com/en-us/microsoft-teams/download-app> | Join on the web<https://www.microsoft.com/microsoft-teams/join-a-meeting>\nJoin with a video conferencing device\nwbg@m.webex.com\nVideo Conference ID: 111 991 705 2\nAlternate VTC instructions<https://www.webex.com/msteams?confid=1119917052&tenantkey=wbg&domain=m.webex.com>\nOr call in (audio only)\n+1 509-408-0991,,125906627#<tel:+15094080991,,125906627#>   United States, Spokane\nPhone Conference ID: 125 906 627#\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=125906627> | Reset PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nLearn More<https://aka.ms/JoinTeamsMeeting> | Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=4a5a2905-6177-478a-bc1d-64618cb688d9&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NDc4N2M3ZDUtNTE3Ny00Y2I1LTk1MTgtMTg0MGY2ZmVmNmE4@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "MC 10-300 (24) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000072wALMAY",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 5, 29, 20, 28, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000DUQTuIAP" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007AQEvMAO",
                    new UNOPSInteraction
                    {
                        Name = "Lunch - UK/UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-04").ToUniversalTime(),
                        Subject = "Lunch - UK/UNOPS",
                        Description = null,
                        Location = "Poulette Rotisserie Chicken, 304 E 49th St, New York, NY 10017, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007AQEvMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 3, 20, 41, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000099IS3IAM" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007FFNkMAO",
                    new UNOPSInteraction
                    {
                        Name = "Lunch meeting with Michele Cervone D'Urso",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "Lunch meeting with Michele Cervone D'Urso",
                        Description = "Exchange around UNOPS possible positioning within the Global Gateway",
                        Location = "Rue Froissart",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007FFNkMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 6, 8, 2, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IsHSwIAN" },
                    new List<int> { 1031 },
                    new List<string> { "mariacarmenco@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007HwGUMA0",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Serbia Ambassador to the EU",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-07").ToUniversalTime(),
                        Subject = "Meeting with Serbia Ambassador to the EU",
                        Description = "Meeting UNOPS Serbia and UNOPS Brussels on current and upcoming cooperation with the EU DG NEAR.\n\nUNOPS participants: Mariacarmen COLITTI (organizer), Michela TELATIN, Vera JOVANOVIC, Aleksandar Andrija PEJOVIC, Ermira RESHIDI",
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007HwGUMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 7, 13, 5, 9, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JTAcCIAX" },
                    new List<int> { 1026 },
                    new List<string> { "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007I1jCMAS",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Ambassador of North Macedonia to the EU",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-07").ToUniversalTime(),
                        Subject = "Meeting with Ambassador of North Macedonia to the EU",
                        Description = "Meeting UNOPS Serbia (Michela Telatin, and UNOPS Brussels on current and upcoming cooperation with the EU DG NEAR.\n\nUNOPS participants: Mariacarmen COLITTI (organizer), Michela TELATIN, Vera JOVANOVIC, Aleksandar Andrija PEJOVIC, Ermira RESHIDI\n\nTweet: https://x.com/unops_serbia/status/1799036262160212279?s=46&t=7EnDCiQl6LaCSVcxWxG97w",
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007I1jCMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 7, 13, 1, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JTKdCIAX" },
                    new List<int> { 1026 },
                    new List<string> { "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007MsgsMAC",
                    new UNOPSInteraction
                    {
                        Name = "I/O A/S Sison and UNOPS ED",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-19").ToUniversalTime(),
                        Subject = "I/O A/S Sison and UNOPS ED",
                        Description = "The ED outlined the reforms made to date and his vision for the organisation and asked what else the US would like to see before returning to a normal business relationship.  \nA/S Sison was very complimentary of the steps taken and looked for continued progress on culture and accountability.\nSison invited the ED to speak at the September meeting of the Geneva Group of UN Donors meeting on the sidelines of UNGA HL week in September 2024.  She thought UNOPS would be an inspiring case study in effective UN reform. \nED outlined the plans for the Gaza Humanitarian Access Mechanism, including routes, funding requirements, and obstacles. He described the rejected materials he witnessed on his January mission to Gaza.\nHe also outlined UXO and rubble removal capabilities and monitored fuel delivery to support the humanitarian response.  \nHe also discussed the often arbitrary and inconsistent rules for so-called dual-use goods. He asked for US help in standardizing rules and asked the US to push the Israelis to allow importation of some solar panels or solar powered equipment into the territory.\nThe ED underscored that UNOPS cannot replace UNWRA. \nA/S expressed her appreciation for UNOPS work in Gaza and elsewhere and offered US support for the Humanitarian Access Mechanism with the Israelis and other parties.\nSison also inquired about communications equipment and IT for the mechanisms. She wanted to ensure that UNOPS staff had the required communication capabilities.   \nThe A/S and UNOPS ED discussed UNOPS work in Ukraine, Haiti, Yemen, and elsewhere.",
                        Location = "US Department of State, HST",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007MsgsMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 10, 15, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HkW73IAF" },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org", "sarahdg@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007NBGUMA4",
                    new UNOPSInteraction
                    {
                        Name = "ED Meeting with NEA and PRM Bureau (State Department)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "ED Meeting with NEA and PRM Bureau (State Department)",
                        Description = "ED laid out in detail the progress made in establishing the Gaza Humanitarian Mechanism, including routes, operations, and roadblocks. \nHe also discussed the often arbitrary and inconsistent rules for so-called dual-use goods. He asked for the US to help standardize the rules and asked the US to push the Israelis to allow the importation of some solar panels or solar-powered equipment into the territory.\nHe also discussed the deconfliction and access issues in Gaza and the security issues in the territory.\nThe PDAS asked who will distribute the aid once it reaches Gaza.\nThe ED described UNOPS’s monitored fuel deliveries for humanitarian actors, UXO and rubble disposal expertise, and other work in Gaza.   \nThe PDAS and his colleagues appreciated the briefing and asked how the US could support, noting their recent demarche asking countries to fund and support the Gaza Humanitarian mechanism.\nNEA also asked about UNOPS access and relationship with Israeli civilian and military organisations like IDF Southern Command and COGAT, especially regarding de-confliction and communications in Gaza.  \nThe ED asked for US financial and political support for Gaza, including deconfliction.\nThe ED underscored the importance of UNVIM and UNOPS work in Yemen for the stability of the country and the region. \nNEA staff asked about the funding situation (how many months’ of funding are in place) and asked for a deep dive into how the US could work with like-minded countries to strengthen UNVIM’s effectiveness and reporting.    \nNEA DAS Backemeyer mentioned that the State Department had not sent up a Congressional notification to the Hill, related to UNOPS, in 18 months and expressed concern about possible extra scrutiny of CNs for UNOPS.  \nUNOPS ED and WLO staff outlined their positive engagement with Democratic and Republican staffers in both houses and suggested it was a very different environment than when Rep. McCaul objected to the Lebanese stipend project. \nWLO staff promised to update NEA and I/O on their interactions and share information on Congressional views of future funding for UNOPS.",
                        Location = "US Department of State",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007NBGUMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 12, 2, 27, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JkCJWIA3" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007NDTrMAO",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS ED Meeting with NEA PDAS and PRM A/DAS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "UNOPS ED Meeting with NEA PDAS and PRM A/DAS",
                        Description = "Meeting confirmed. Non-US citizens need a passport to enter DOS. Delegation should arrive 20 minutes ahead of scheduled meeting to clear security.",
                        Location = "U.S. Department of State, 2201 C St NW, Washington, DC 20451, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007NDTrMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 11, 52, 30, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org", "sarahdg@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007NFTlMAO",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS ED Meeting with INL A/S",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "UNOPS ED Meeting with INL A/S",
                        Description = "The ED expressed a desire to build upon their September 2023 meeting during UNGA HL week. He welcomed the openness for signing new projects.  \nA/S Robinson mentioned that INL is open to new projects pitched by their field offices, but cautioned that he could not “get ahead of the I/O Bureau,” which suggests State Dept. bureaus consider alternatives to UNOPS. He said INL would review new UNOPS projects “on a case-by-case basis”. \nRobinson said it was important for INL and UNOPS to review their standardized agreements (LOAs), after which the Bureau would consider a return to “business as usual.” \nThe ED and the A/S discussed UNOPS’ work in Haiti, Ukraine, Central America, and elsewhere. \nRegarding Guatemala, the ED mentioned UNOPS’ connection to the country’s current president and plans to mitigate any potential conflicts of interest, including moving the President’s close relative from the region.",
                        Location = "U.S. Department of State, 2201 C St NW, Washington, DC 20451, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007NFTlMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 11, 55, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000EqNQIIA3" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007NrQzMAK",
                    new UNOPSInteraction
                    {
                        Name = "Connecting with NEA and IO for a quick call this afternoon",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-30").ToUniversalTime(),
                        Subject = "Connecting with NEA and IO for a quick call this afternoon",
                        Description = "Check in regarding how US State Department can support the Gaza Mechanism. \nBureau of International Organization (I.O) Affairs is working to push like-minded countries to support mechanism. I/O wanted to know where to have other countries send funds.\nCurrent US funding focused on building pier in Gaza.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007NrQzMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 16, 33, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JkCJWIA3" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007NsJpMAK",
                    new UNOPSInteraction
                    {
                        Name = "Catch up with Layal",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-03").ToUniversalTime(),
                        Subject = "Catch up with Layal",
                        Description = "Catch up on Canadian support Haitian National Police, Executive Board preparations, and overall UNOPS-Canada relationship.  \nPromoting law and order and stability in Haiti is a priority for GAC. \nOttawa is looking forward to a very positive EB session in June. They will likely seek further information on UNOPS culture and the JIU reports on EB governance and the UN use of contractors.",
                        Location = "Virtual",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007NsJpMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 16, 39, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFQA5IAP" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007Nt2zMAC",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Briefing on the Gaza Mechanism",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-07").ToUniversalTime(),
                        Subject = "UNOPS Briefing on the Gaza Mechanism",
                        Description = "Virtual meeting with UNOPS to learn more about the Gaza Mechanism. and  the funding modalities for the Cyprus corridor.\n\nUNOPS colleagues,\n\nIO DAS Allison Lombardo, NEA DAS Christopher Backemeyer, and PRM A/DAS Deanna Abdeen will be a number of colleagues from IO, NEA, PRM, USUN, and USAID all attended.\n\nUS State Department asked for a status of three HA corridors for Gaza Humanitarian Assistance. They were concerned that the  flow of aid was not enough and could only be accomplished if the Rafah route was open. \nThey were very interested in the data base operations and how the US could help marshal support from counterparts. \nUS offered to pressure the Israelis where possible, especially in deconfliction and access to Gaza.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007Nt2zMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 16, 47, 27, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JkKLmIAN" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007OBZNMA4",
                    new UNOPSInteraction
                    {
                        Name = "Somalia UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "Somalia UNOPS",
                        Description = "The UNOPS team presented UNOPS and UNOPS operations in Somalia with focus on the Security and Rule of Law and health sector. Also potential areas of collaboration with the FGS, the WB, and WHO to solarize more than 200 health facilities across Somalia.\nINL Program officer (PO) requested clarifications on how SOCO is ensuring donor funds are protected (S3i related) and the SOCO Director explained how the reforms following the S3i are being implemented fully including in Somalia. \nINL has been very pleased with its partnerships with UNOPS in Somalia, including its support for the country’s police criminal investigative service. \nThe Bureau sees UNOPS as a future partner, especially given UNOPS's ability to work throughout the country. \nINL asked how UNOPS was planning for the transition to a new, smaller AU force this year. UNOPS Country Director replied that the situation was not entirely clear, but the CO expects to be able to continue its work across the country.\nUNOPS Somalia team shared how US assistance to the country’s security forces enables other development, especially UNOPS’s WB-funded projects to rehabilitate six regional hospitals and build the country’s first blood bank. \nINL PO requested for clarifications on whether UNOPS does monitoring after delivery to ensure assets are not falling on wrong hands and UNOPS can implement projects as per the scope of the agreement and monitoring mechanisms can be incorporated in projects with adequate resources if that is required by donors. \nINL is open to benefit from UNOPS support building on the successful delivery of previous project, conversations shaping up new engagements to continue at local level.\nINL looks forward to completing the LOA review to proceed with new projects. In the meantime, INL encouraged UNOPS Somalia to continue conversations with their counterparts in Mogadishu.\nConsider adequate Gender Mainstreaming in future peace and security engagements. \nAF/RPS opened the meeting with strong praise for the Somali team, especially for Ronel Bekker, the Project Manager of Danab Special Forces Stipends. The Africa Bureau appreciated the work of the project team being responsive, providing creative solutions, and transparent\nState Department understands the need for national authorities to fund salaries for their security forces. However, they are not aware of a successful precedent. \nQuestion on how UNOPS would operate if the  security situation deteriorates, which relate to business criticality levels, UNOPS team explained the measures in place to ensure business continuity in case of any emergency situation.\nAF/RPS asked UNOPS for an effective transition plan and examples where transition from stipend if any to be shared with the bureau  possibly with the EU-funded Joint Police Program (JPP).\nGiven these complications, AF/RPS likely to provide additional support of around $9 million later this fiscal year.  Indication of continued interest providing stipends support through UNOPS for the upcoming 1 -2 year.\nThe UNOPS team provided clarifications to a question related to UNOPS costing and pricing/fee structure which varies depending on the scope and scale of projects.\n\n\nThe UNOPS team presented UNOPS and UNOPS operations in Somalia with focus on the Security and Rule of Law and health sector. Also potential areas of collaboration with the FGS, the WB, and WHO to solarize more than 200 health facilities across Somalia.\nClarification provided on how UNOPS coordinates with other UN entities to ensure synergies delivering impactful results. \nCT Bureau saw potential for future cooperation with UNOPS in support of Somali Security forces, including a possible contribution later this fiscal year.  \nUNOPS Somali team shared how US assistance to the country’s security forces enables other development, especially UNOPS’s WB-funded projects to rehabilitate six regional hospitals and build the country’s first blood bank. \nLike INL, CT is interested in a transition plan to national funding of security forces. \nClarification provided that UNOPS can provide biometric or other tracking mechanisms of equipment provided to FSNA or Police if included in the project scope so that these are not falling in the wrong hands\nCT is concerned about how the ATMIS transition later this year will impact the security situation in the country and could lead to a resurgence of Al Shabab once the drawdown to around 8,000-10,000 happens.\nUNOPS explained that it will follow the UNDSS programme criticality should there be any security deterioration and necessary contingency plans are in place. \nClarification provided that UNOPS can provide biometric or other tracking mechanisms of equipment provided to SNA or SPF if included in the project scope.",
                        Location = "L-616",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007OBZNMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 11, 22, 25, 3, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000EqNQIIA3" },
                    new List<int> { 1788 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007Q7D8MAK",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-AF/RPS Meeting",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "UNOPS-AF/RPS Meeting",
                        Description = "AF/RPS opened the meeting with strong praise for the Somali team, especially for Ronel Bekker, the Project Manager of Danab Special Forces Stipends. The Africa Bureau appreciated the work of the project team being responsive, providing creative solutions, and transparent\nState Department understands the need for national authorities to fund salaries for their security forces. However, they are not aware of a successful precedent. \nQuestion on how UNOPS would operate if the  security situation deteriorates, which relate to business criticality levels, UNOPS team explained the measures in place to ensure business continuity in case of any emergency situation.\nAF/RPS asked UNOPS for an effective transition plan and examples where transition from stipend if any to be shared with the bureau  possibly with the EU-funded Joint Police Program (JPP).\nGiven these complications, AF/RPS likely to provide additional support of around $9 million later this fiscal year.  Indication of continued interest providing stipends support through UNOPS for the upcoming 1 -2 year.\nThe UNOPS team provided clarifications to a question related to UNOPS costing and pricing/fee structure which varies depending on the scope and scale of projects.",
                        Location = "SA-9 LL1 Conference Room",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007Q7D8MAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 12, 20, 1, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JqiRxIAJ" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007Q8IuMAK",
                    new UNOPSInteraction
                    {
                        Name = "State/Health meeting with UN Office of Project Services, health projects in fragile, violent, conflict environments",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "State/Health meeting with UN Office of Project Services, health projects in fragile, violent, conflict environments",
                        Description = "The UN Office of Project Services (UNOPS) is a UN agency that is heavily involved in the development of health projects in fragile, violent, and conflict (FCV) environments. (www.unops.org<http://www.unops.org/>)\n\nThe Somalia Country Manager, a Mogadishu-based Senior Program Officer, and two health experts from our HQ will be here in Washington from May 6-10. They have all expressed a desire to meet with the State Department's Bureau of Global Health to talk about UNOPS' health work in FCV environments with Somalia as a case study.  Our team from Somalia would also be happy to share their views on what they see there.\n\nWe will meet at the GHSD suite at 1800 G St. NW, SA-22, 10th floor. Please allow time to go through building security and we will meet you at the elevators on the 10th floor. The meeting will take place in the Latin America Room with Teams capabilities for those not based in DC.\n\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NTc1YzJkNDQtMWY2OS00OTg4LWIxODAtYTEzYTg1NTdkMjQ2%40thread.v2/0?context=%7b%22Tid%22%3a%2266cf5074-5afe-48d1-a691-a12b2121f44b%22%2c%22Oid%22%3a%22c3ef2b52-9fe6-446c-82dc-406badf61f8a%22%7d>\nMeeting ID: 294 971 065 129\nPasscode: BrWRCX\n________________________________\nDial-in by phone\n+1 509-824-1908,,489467397#<tel:+15098241908,,489467397> United States, Spokane\nFind a local number<https://dialin.teams.microsoft.com/24359e3a-e3fd-47fe-8c6b-14f6797733ce?id=489467397>\nPhone conference ID: 489 467 397#\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=c3ef2b52-9fe6-446c-82dc-406badf61f8a&tenantId=66cf5074-5afe-48d1-a691-a12b2121f44b&threadId=19_meeting_NTc1YzJkNDQtMWY2OS00OTg4LWIxODAtYTEzYTg1NTdkMjQ2@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "1800 G St NW; SA-22, 10th floor (Teams for remote participants); Latin America Room",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007Q8IuMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 12, 20, 12, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JqOgMIAV" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007S6pfMAC",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS meeting with BHA/Somalia (Room 958)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-10").ToUniversalTime(),
                        Subject = "UNOPS meeting with BHA/Somalia (Room 958)",
                        Description = "BHA opened the meeting noting that it has not traditionally funded UNOPS. Its main partners in Somalia are WFP and IOM.\nThe UNOPS team presented UNOPS and UNOPS operations in Somalia with focus on the Security and Rule of Law and health sector. Also potential areas of collaboration with the FGS, the WB, and WHO to solarize more than 200 health facilities across Somalia. UNOPS is working with FGS, WB and UN Agencies on the PDNA where UNOPS is leading in the Transport sector.\nUNOPS Somali colleagues outlined how UNOPS can support humanitarian actors in the country with project management, infrastructure, and fund management or when HA response moves into early recovery. \nBHA asked about the ATMIS transition and how this will impact the UN in the country. \nThe UNOPS Country Director noted that while the new force's exact form is unclear, UNOPS plans to keep working in the country. \nHowever, he noted that UNOPS operates under the UNDSS framework with its four tripwires requiring changes in staffing levels or withdrawals.   \nBHA looks forward to the damage assessment results from last year’s funding. \nUNOPS CO shared how UNOPS works to utilize local suppliers and build national capacity. \nUSAID supports the logistics cluster in Somalia and supports UNHAS on Airstrip rehabilitation, small interventions on emergency health portfolio and working with FAO on borehole monitoring, community early warning systems and repair works of dykes related to the flood damages",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007S6pfMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 19, 15, 9, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GsmeQIAR" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007S7k7MAC",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Somalia Team in Washington, DC",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-08").ToUniversalTime(),
                        Subject = "UNOPS Somalia Team in Washington, DC",
                        Description = "he UNOPS team presented UNOPS and UNOPS operations in Somalia with focus on the Security and Rule of Law and health sector. Also potential areas of collaboration with the FGS, the WB, and WHO to solarize more than 200 health facilities across Somalia.\nClarification provided on how UNOPS coordinates with other UN entities to ensure synergies delivering impactful results. \nCT Bureau saw potential for future cooperation with UNOPS in support of Somali Security forces, including a possible contribution later this fiscal year.  \nUNOPS Somali team shared how US assistance to the country’s security forces enables other development, especially UNOPS’s WB-funded projects to rehabilitate six regional hospitals and build the country’s first blood bank. \nLike INL, CT is interested in a transition plan to national funding of security forces. \nClarification provided that UNOPS can provide biometric or other tracking mechanisms of equipment provided to FSNA or Police if included in the project scope so that these are not falling in the wrong hands\nCT is concerned about how the ATMIS transition later this year will impact the security situation in the country and could lead to a resurgence of Al Shabab once the drawdown to around 8,000-10,000 happens.\nUNOPS explained that it will follow the UNDSS programme criticality should there be any security deterioration and necessary contingency plans are in place. \nClarification provided that UNOPS can provide biometric or other tracking mechanisms of equipment provided to SNA or SPF if included in the project scope.",
                        Location = "HST Cafeteria",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007S7k7MAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("solomong@unops.org".ToLower()) ? paoUserEmailMapping["solomong@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 19, 11, 50, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Jv8txIAB" },
                    new List<int> {  },
                    new List<string> { "solomong@unops.org", "alaan@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007S8MzMAK",
                    new UNOPSInteraction
                    {
                        Name = "State - UNOPS sync",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-30").ToUniversalTime(),
                        Subject = "State - UNOPS sync",
                        Description = "Follow up sync between State and UNOPS to discuss lates on Maritime Corridor, status of contributions to fund and in-kind, additional needs, and engagement opportunities.\nUNOPS ME Team provided an update on progress and asked for US help in deconfliction and pressing Israelis to facilitate shipments. UNOPS noted the need to get land corridors via Egypt and Jordan operating soon.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007S8MzMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 19, 41, 51, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JlJkNIAV" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007S9AoMAK",
                    new UNOPSInteraction
                    {
                        Name = "Compact Funding and Chuuk State Hospital",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-17").ToUniversalTime(),
                        Subject = "Compact Funding and Chuuk State Hospital",
                        Description = "Stephen Savage discussed the status of compact funding in the three countries: the Federated States of Micronesia, the Republic of Palau, and the Republic of the Marshall Islands. \nHe noted a great need to help these three countries implement projects. \nUNOPS can play an important role, but the governments of the three countries decide on implementers. \nOIA only provides a determination of no objection for a project. \nRegarding Chuuk State Hospital, he said the hospital contract was awarded several years ago to the US Army Corps of Engineers. When asked, he said there was no role for UNOPS in this project.  He suggested that UNOPS focus on the Republic of Palau, which is looking to build a hospital. \nHe will try to get the UNOPS ED with DOI officials at SIDS 4.",
                        Location = "Virtual Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007S9AoMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 19, 33, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JvG1tIAF" },
                    new List<int> { 1145 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007S9KbMAK",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Briefing to INL and IO",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-17").ToUniversalTime(),
                        Subject = "UNOPS Briefing to INL and IO",
                        Description = "State Department Bureau of International Organizations (I/O) and International Law Enforcement Affairs had similar questions on UNOPS's reserves, cost recovery model, and reserves. \nFG outlined the new net-zero budget process, including refunds of any reserves about the EB mandated level.  \nDiscussion supported re-negotiation of INL letters of agreement (LOA)  and addressed questions from I/O about our new budget policies.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007S9KbMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 19, 23, 50, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Gw3p7IAB" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007SBNtMAO",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Conor Savoy (USAID)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-03").ToUniversalTime(),
                        Subject = "Meeting with Conor Savoy (USAID)",
                        Description = "Conor Savoy provided his views on sunsetting the July 2022 special clauses, the May 2024 PIO language, and the overall US-UNOPS relationship. \nHe suggested UNOPS's ED talk to AA for the Bureau of Planning, Learning, and Resource Management Michele Sumilas to discuss these topics. \nHe agreed that the May 2024 PIO language regarding UNOPS was \"unfortunate\" and mischaracterized UNOPS progress. He said relations were \"on-the=right-track.\"",
                        Location = "Timgad Café, 1300 Pennsylvania Avenue NW, Washington, DC 20004, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007SBNtMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 19, 44, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hke0pIAB" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007SCi9MAG",
                    new UNOPSInteraction
                    {
                        Name = "USAID/UNOPS Touch-base-Topics for potential senior-level meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-03").ToUniversalTime(),
                        Subject = "USAID/UNOPS Touch-base-Topics for potential senior-level meeting",
                        Description = "The meeting was to review the sunsetting of UNOPS July 2022 Special Clauses, the problematic May 2024 PIO language, and the overall USAID-UNOPS relationship.  \nBoth Andrew and Liz suggested that UNOPS ED approach Michele Sumilas (PLR AA) to push for revision of the PIO language and to get a timeline for the sunsetting of the July 2022 language. They acknowledged that both sides had agreed to a review by December 31, 2023 and were apologetic that this did not happen.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007SCi9MAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 20, 7, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GstOAIAZ" },
                    new List<int> { 1112 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007SEbwMAG",
                    new UNOPSInteraction
                    {
                        Name = "USAID Asia Bureau Meeting",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-04").ToUniversalTime(),
                        Subject = "USAID Asia Bureau Meeting",
                        Description = null,
                        Location = "Location TBD.",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007SEbwMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 13, 20, 52, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GsmeQIAR" },
                    new List<int> {  },
                    new List<string> { "christinebo@unops.org", "alistairs@unops.org", "simonettas@unops.org", "sarane@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007SppxMAC",
                    new UNOPSInteraction
                    {
                        Name = "19th Meeting of the EU-UN FAFA Working Group",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "19th Meeting of the EU-UN FAFA Working Group",
                        Description = "Chairpersons: Mr Chandramouli Ramanathan, UN ASG and Controller, and\nMr Didier Versé, Director (INTPA.R), Directorate General for International Partnerships\n\nDraft agenda: https://docs.google.com/document/d/1q2q1PxQKjxIwrxpolw8bM9FV0R-gfPaw/edit?usp=drive_link&ouid=112306598079446424107&rtpof=true&sd=true",
                        Location = "4 Albert Embankment, London SE1 7SR, United Kingdom",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007SppxMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 14, 7, 50, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000JwmWqIAJ" },
                    new List<int> { 1025 },
                    new List<string> { "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007TW16MAG",
                    new UNOPSInteraction
                    {
                        Name = "NO_SUB00UQx000007TW16MAG",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-04").ToUniversalTime(),
                        Subject = "NO_SUB00UQx000007TW16MAG",
                        Description = "NO_DESC00UQx000007TW16MAG",
                        Location = "US Department of State Main Building, Office of Mainland South-East Asia",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007TW16MAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 14, 13, 8, 25, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007TYjtMAG",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Asia Region Meeting with USAID",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-06").ToUniversalTime(),
                        Subject = "UNOPS Asia Region Meeting with USAID",
                        Description = "UNOPS Senior Advisor for Asia outlined UNOPS's reach and capabilities across the region. She underscored the organization's commitment to the region and investments in SIDs. \nUSAID appreciated the update, and HQ personnel offered to help coordinate follow-up in the field.",
                        Location = "USAID HQ RRB Washington, DC",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007TYjtMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 14, 13, 56, 16, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1112 },
                    new List<string> { "patrickel@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007TbGIMA0",
                    new UNOPSInteraction
                    {
                        Name = "Myanmar Meeting with US DOS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-04").ToUniversalTime(),
                        Subject = "Myanmar Meeting with US DOS",
                        Description = "UNOPS Country Director Sara Netzer described UNOPS's current activities in Myanmar, including our ability to reach 84 percent of the country. \nShe outlined how the UN handles DFA authorities. and the rules that the UN follows in the country.\nShe also outlined the fear that Myanmar could quickly become a failed state and that the status quo could fracture the country in various ways. \nState Department Burma Unit Chief outlined US priorities and strong support for the UN in the country.\nHe noted that he was unaware of the size of UNOPS staff or project portfolio in the country. \nHe and the UNOPS Country described the Joint Peace Program and if the US might be willing to rejoin after ending its participation a few years back.",
                        Location = "Main State Department Building",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007TbGIMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 14, 13, 24, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007TdhtMAC",
                    new UNOPSInteraction
                    {
                        Name = "Asia Delegation Myanmar Meeting with USAID",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "Asia Delegation Myanmar Meeting with USAID",
                        Description = "UNOPS Myanmar Country Director shared the breadth and reach of UNOPS activities in Myanmar. \nShe noted that the LIFT program is able to reach over 84 percent of the country via CSOs and that activities are split between DFA-controlled areas and those held by various anti-regime factions. \nShe shared her concerns about how US and international support is critical to keep Myanmar from becoming a failed states.\nUSAID counterparts shared their gratitude for UNOPS's program results across the country and indicated continued US support. However, they noted that they are not expecting a \"plus up\" in this years FY 2025 budget.",
                        Location = "USAID RRB Building",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007TdhtMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 14, 13, 34, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007TfRyMAK",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Asia Meeting wtih INL East Asia Branch",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-06").ToUniversalTime(),
                        Subject = "UNOPS Asia Meeting wtih INL East Asia Branch",
                        Description = "UNOPS Asia Region Senior Advisor described UNOPS's work with INL in Laos, Pakistan, and elsewhere. She expressed her desire for UNOPS to deepen its partnership with INL with a very promising project in Laos that is close to signing. \nINL staff noted the Laos project and hoped the new standard agreements (LOAs) would be done soon so that this project and others could be signed soon. \nStephanie Greene hoped the LOAs would be done in two weeks so an INL AS could sign the Lao project during a June 24 visit.\nWLO staff updated INL on the progress on the LOAs and expressed a desire to complete them as soon as possible.",
                        Location = "INL, SA-1 Washington, DC",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007TfRyMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 14, 14, 16, 30, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000EqNQIIA3" },
                    new List<int> { 1788 },
                    new List<string> { "patrickel@unops.org", "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007WnMbMAK",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with PLR AA Michele Sumilas",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-10").ToUniversalTime(),
                        Subject = "Meeting with PLR AA Michele Sumilas",
                        Description = "UNOPS ED shared his hope, expressed in his letter to USAID Administrator Power, that the agency would revise the May 2024 PIO language and sunset the July 2022 Special Clauses. UNOPS ED noted that the two parties had agreed to a review by December 31, 2023. \nPLR AA Sumilas agreed to remove UNOPS's May 2024 funding restrictions, noting the significant progress made on reforms and the current state of the agency's relationship with UNOPS.\nSumilas said USAID was reviewing the July 2022 special clauses and hoped to make a positive decision by the fall. \nUNOPS ED and PLR AA agreed that their respective staff would coordinate for the removal of the May 2024 PIO language and support the internal review of the July 2022 language.",
                        Location = "Virtual Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007WnMbMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kirstined@unops.org".ToLower()) ? paoUserEmailMapping["kirstined@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 17, 12, 49, 43, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GsmeQIAR" },
                    new List<int> { 1112 },
                    new List<string> { "kirstined@unops.org", "jorge.moreiradasilva@unops.org", "emiliep@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007WvR9MAK",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with USAID BHA South Asia Team",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-07").ToUniversalTime(),
                        Subject = "Meeting with USAID BHA South Asia Team",
                        Description = "UNOPS AR Senior Advisor outlined UNOPS' regional capabilities and worked with humanitarian partners.\nUSAID BHA staff noted that UNOPS is not one of their traditional partners, adding that most of their assistance goes to WFP.\nBHA staff were very interested in how UNOPS can support humanitarian actors and how the organization partnered with UN agencies. \nBHA was very interested in having UNOPS staff in the Asia region talk with BHA staff in Manila and Bangkok. AR to follow up and explore additional areas of cooperation.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007WvR9MAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 17, 12, 19, 51, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KAYg7IAH" },
                    new List<int> { 1112 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007WxFRMA0",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with USAID DRB Bureau",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-07").ToUniversalTime(),
                        Subject = "Meeting with USAID DRB Bureau",
                        Description = "Meeting focused on DRG programming with the Asia Pacific Region. \nKevin Nelson was very apologetic that he was not able to fund the Nepal project during the S3i crisis and that it had to go to UN Habitat. \nHe was hopeful that in the future DRG would be able to fund UNOPS again. \nKevin Nelson did provide some names for the UNOPS AR to follow up with in the field.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007WxFRMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("simonettas@unops.org".ToLower()) ? paoUserEmailMapping["simonettas@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 17, 12, 11, 45, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KAfftIAD" },
                    new List<int> {  },
                    new List<string> { "simonettas@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007Wxc2MAC",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with SCA Assistance Coordinator",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-07").ToUniversalTime(),
                        Subject = "Meeting with SCA Assistance Coordinator",
                        Description = "UNOPS Senior Advisor for Asia outlined UNOPS work across South Asia. \nShe noted UNOPS's unique capabilities and reach across the region. She also detailed UNOPS's work with the US in Pakistan and expressed hope for greater cooperation in the future. \nMetz praised the introduction to UNOPS, noting he was unfamiliar with its work.\nHe offered to introduce  AR staff to UNOPS staff in the region.\nHe was headed to Pakistan for a week and hoped to meet the UNOPS team in Islamabad.",
                        Location = "HST, Main State Department Building",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007Wxc2MAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("simonettas@unops.org".ToLower()) ? paoUserEmailMapping["simonettas@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 17, 12, 29, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KAceqIAD" },
                    new List<int> { 1113 },
                    new List<string> { "simonettas@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007X6nNMAS",
                    new UNOPSInteraction
                    {
                        Name = "Phone Call with GAC re. June EB and UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-13").ToUniversalTime(),
                        Subject = "Phone Call with GAC re. June EB and UNOPS",
                        Description = "Sarrouh shared that Canada was pleased with the EB's outcome, noting that they were hopeful that there would soon be some progress on accountability and looked forward to seeing progress on culture. \nShe noted that Haiti was an important focus for GAC and looked forward to seeing the results of the projects there. On Gaza, GAC strongly supported the Gaza Mechanism and hoped it would be fully operational soon. \nSarrouh also shared that GAC was going through a full-scale reorganization. Staff will also rotate in August.   \nWLO staff suggested additional areas of cooperation, senior-level meetings, and a mission to Ottawa focused on Haiti. He noted these steps could help solidify the gains over the past two years. \nSarrouh suggested that these wait until after GAC's reorganization is done later this year.",
                        Location = "Virtual",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007X6nNMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 17, 13, 9, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFQA5IAP" },
                    new List<int> { 1024 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007X8pGMAS",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with INL Re. Standard Agreements",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-13").ToUniversalTime(),
                        Subject = "Meeting with INL Re. Standard Agreements",
                        Description = "UNOPS shared its initial response to INL's proposed revisions to the LOAs, which IPAS had reviewed. INL seemed satisfied with the proposals and would discuss them with their legal team and get back to UNOPS with any questions or concerns. \nLG and FG are in contact and look forward to completing these discussions as soon as possible.",
                        Location = "Virtual",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007X8pGMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 17, 13, 40, 57, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFY62IAH" },
                    new List<int> { 1788 },
                    new List<string> { "elizabethdu@unops.org", "iraklij@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007YjMuMAK",
                    new UNOPSInteraction
                    {
                        Name = "Ukraine Hybrid Generators",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-19").ToUniversalTime(),
                        Subject = "Ukraine Hybrid Generators",
                        Description = null,
                        Location = "Crowne Plaza Copenhagen Towers, an IHG Hotel, Copenhagen Towers, Ørestads Blvd. 114, 118, 2300 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007YjMuMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 9, 48, 58, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1123 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007YomMMAS",
                    new UNOPSInteraction
                    {
                        Name = "Business Case UNOPS HR Services for non-staff",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-18").ToUniversalTime(),
                        Subject = "Business Case UNOPS HR Services for non-staff",
                        Description = "<a href=\"https://docs.google.com/document/d/1wp2esUYwV6WOfvBfz2k1CBlzlUwOas6xUsxX3NEOZHk/edit#heading=h.gjdgxs\">Business Case UN Web Buy plus</a> (soft copy)",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007YomMMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("victoriac@unops.org".ToLower()) ? paoUserEmailMapping["victoriac@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 11, 25, 51, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "victoriac@unops.org", "davidc@unops.org", "raady@unops.org", "laurentium@unops.org", "lorrainea@unops.org", "alejoe@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007Z7PMMA0",
                    new UNOPSInteraction
                    {
                        Name = "Meeting UNOPS Amman MCO Director with Mr Giovanni DI GIROLAMO, ECHO, Head of Unit Middle East and North Africa",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-02").ToUniversalTime(),
                        Subject = "Meeting UNOPS Amman MCO Director with Mr Giovanni DI GIROLAMO, ECHO, Head of Unit Middle East and North Africa",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007Z7PMMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 45, 54, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KG4vOIAT" },
                    new List<int> { 1026 },
                    new List<string> { "mariacarmenco@unops.org", "usmana@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007Z7U2MAK",
                    new UNOPSInteraction
                    {
                        Name = "CODEWAY EXPO ROME",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "CODEWAY EXPO ROME",
                        Description = "UNOPS partnered with the Italian Ministry of Foreign Affairs and International Cooperation to co-host conversations on procurement and the vital role of the private sector in international cooperation. More info: https://intra.unops.org/unops-underscored-the-critical-role-of-procurement-in-driving-sustainable-development",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007Z7U2MAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 12, 33, 45, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KFPdvIAH" },
                    new List<int> { 1266 },
                    new List<string> { "mariacarmenco@unops.org", "anneclaireh@unops.org", "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZEP7MAO",
                    new UNOPSInteraction
                    {
                        Name = "6th Yemen Senior Officials Meeting (SOM)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "6th Yemen Senior Officials Meeting (SOM)",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZEP7MAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 52, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KG8FqIAL" },
                    new List<int> { 1029 },
                    new List<string> { "mariacarmenco@unops.org", "usmana@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZFoBMAW",
                    new UNOPSInteraction
                    {
                        Name = "Meeting German BMZ State Secretary Flasbarth with DEDs and Ethics Director",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "Meeting German BMZ State Secretary Flasbarth with DEDs and Ethics Director",
                        Description = null,
                        Location = "Copenhagen",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZFoBMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 14, 5, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KGAskIAH" },
                    new List<int> { 1126 },
                    new List<string> { "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZJQMMA4",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS ECR RD at Ukraine Recovery Conference in Berlin",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "UNOPS ECR RD at Ukraine Recovery Conference in Berlin",
                        Description = null,
                        Location = "Berlin",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZJQMMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("timl@unops.org".ToLower()) ? paoUserEmailMapping["timl@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 57, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1126 },
                    new List<string> { "timl@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZJdFMAW",
                    new UNOPSInteraction
                    {
                        Name = "Day of Dialogue on Syria and the region",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "Day of Dialogue on Syria and the region",
                        Description = null,
                        Location = "European Parliament",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZJdFMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 25, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1026 },
                    new List<string> { "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZJzqMAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with H.E. Saja MAJALI, Ambassador of the Hashemite Kingdom of Jordan to Belgium, the European Union and NATO",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-02").ToUniversalTime(),
                        Subject = "Meeting with H.E. Saja MAJALI, Ambassador of the Hashemite Kingdom of Jordan to Belgium, the European Union and NATO",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZJzqMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 36, 19, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KFhR0IAL" },
                    new List<int> { 1307 },
                    new List<string> { "mariacarmenco@unops.org", "usmana@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZKuHMAW",
                    new UNOPSInteraction
                    {
                        Name = "Meeting UNOPS Amman MCO Director with DG NEAR Unit Middle East (Lebanon / Jordan)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-02").ToUniversalTime(),
                        Subject = "Meeting UNOPS Amman MCO Director with DG NEAR Unit Middle East (Lebanon / Jordan)",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZKuHMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 31, 50, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KFuD3IAL" },
                    new List<int> { 1026 },
                    new List<string> { "mariacarmenco@unops.org", "usmana@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZMeLMAW",
                    new UNOPSInteraction
                    {
                        Name = "Meeting UNOPS Amman MCO Director with Xavier Camus, DG NEAR, Unit Middle East, In charge of Syria and with Mr. Lorenzo Pascotto, EU Delegation to Syria",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-02").ToUniversalTime(),
                        Subject = "Meeting UNOPS Amman MCO Director with Xavier Camus, DG NEAR, Unit Middle East, In charge of Syria and with Mr. Lorenzo Pascotto, EU Delegation to Syria",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZMeLMAW",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 41, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KG4nKIAT" },
                    new List<int> { 1026 },
                    new List<string> { "mariacarmenco@unops.org", "usmana@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZNlhMAG",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Amman MCO Director with Patrice LENORMAND",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-06").ToUniversalTime(),
                        Subject = "Meeting Amman MCO Director with Patrice LENORMAND",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZNlhMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 13, 49, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KG8FqIAL" },
                    new List<int> { 1029 },
                    new List<string> { "mariacarmenco@unops.org", "usmana@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZPSZMA4",
                    new UNOPSInteraction
                    {
                        Name = "UNEP-UNOPS meetings in Nairobi, w/o June 17th",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-18").ToUniversalTime(),
                        Subject = "UNEP-UNOPS meetings in Nairobi, w/o June 17th",
                        Description = "Dear Kajsa and Kathleen,\n\nAllow me to block arealdy in the calendar the date and a timeslot for our discussion.\n\nWill confirm the location later, but it could be Kathleen’s office.\n\nBest,\n\nJulia  R\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MTQyNmZiOTctYmI0OS00OThmLThhNmQtOTQ2YjViMDRkYTA1%40thread.v2/0?context=%7b%22Tid%22%3a%220f9e35db-544f-4f60-bdcc-5ea416e6dc70%22%2c%22Oid%22%3a%22f6589345-2bc8-40f5-a855-096ec8284f7e%22%7d>\nMeeting ID: 369 831 033 41\nPasscode: Q8yYKF\n________________________________\nJoin on a video conferencing device\nTenant key: unitevc@m.webex.com\nVideo ID: 124 992 557 3\nMore info<https://www.webex.com/msteams?confid=1249925573&tenantkey=unitevc&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=f6589345-2bc8-40f5-a855-096ec8284f7e&tenantId=0f9e35db-544f-4f60-bdcc-5ea416e6dc70&threadId=19_meeting_MTQyNmZiOTctYmI0OS00OThmLThhNmQtOTQ2YjViMDRkYTA1@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________\n\n\n_____________________________________________\nFrom: Julia Ripoll Vallcorba\nSent: 29 May 2024 14:27\nTo: Kajsa Johanna HARTMAN <kajsah@unops.org>; Kathleen Creavalle <kathleen.creavalle@un.org>\nSubject: RE: UNEP-UNOPS meetings in Nairobi, w/o June 17th\n\nDear Kajsa,\n\nWould the 18th June feasible?\n\nLet me know,\n\nJulia R\n\nFrom: Kajsa Johanna HARTMAN <kajsah@unops.org<mailto:kajsah@unops.org>>\nSent: 29 May 2024 14:19\nTo: Kathleen Creavalle <kathleen.creavalle@un.org<mailto:kathleen.creavalle@un.org>>\nCc: Julia Ripoll Vallcorba <julia.ripollvallcorba@un.org<mailto:julia.ripollvallcorba@un.org>>\nSubject: Re: UNEP-UNOPS meetings in Nairobi, w/o June 17th\n\nYou don't often get email from kajsah@unops.org<mailto:kajsah@unops.org>. Learn why this is important<https://aka.ms/LearnAboutSenderIdentification>\nDear Kathleen,\n\nExcellent, thank you for the quick response. I am happy to coordinate directly with Julia around the scheduling.\n\nI look forward to seeing you in Nairobi soon.\n\nKind regards,\nKajsa\n\nOn Wed, 29 May 2024 at 12:56, Kathleen Creavalle <kathleen.creavalle@un.org<mailto:kathleen.creavalle@un.org>> wrote:\nDear Kajsa,\nLooking forward to seeing you again in Nairobi.  I am generally around 14-21 June.  Asking @Julia Ripoll Vallcorba<mailto:julia.ripollvallcorba@un.org> to schedule time.\n\nBest Regards,\nKathleen\n======\nKathleen Creavalle\nDeputy Director\nCorporate Services Division\nUnited Nations Environment Programme\nNairobi, Kenya |  unep.org<http://www.unep.org/>\nEmail: kathleen.creavalle@un.org<mailto:kathleen.creavalle@un.org>\n\n\n\n\nFrom: Kajsa Johanna HARTMAN <kajsah@unops.org<mailto:kajsah@unops.org>>\nSent: Wednesday, May 29, 2024 12:07 PM\nTo: Kathleen Creavalle <kathleen.creavalle@un.org<mailto:kathleen.creavalle@un.org>>\nCc: Julia Ripoll Vallcorba <julia.ripollvallcorba@un.org<mailto:julia.ripollvallcorba@un.org>>\nSubject: UNEP-UNOPS meetings in Nairobi, w/o June 17th\n\nYou don't often get email from kajsah@unops.org<mailto:kajsah@unops.org>. Learn why this is important<https://aka.ms/LearnAboutSenderIdentification>\nDear Kathleen and Julia,\n\nI hope that you and the team are doing well. I am reaching out as I am planning a trip to Nairobi in June, and would like to take the opportunity to advance some of the discussions we started when I was last in Nairobi.\n\nIn particular, I would like to propose to meet with you and relevant UNEP teams to advance the following:\n1. Definition of substantive MoU collaboration areas\n2. Scoping UNOPS support to UNEP GCF/GEF projects\n\nMy planned dates are currently the 14th-21st of June. Would the proposed work for you, in view of organizing some meetings with you and relevant portfolio/task managers?\n\nLooking forward to hearing from you.\n\nKind regards,\nKajsa\n\n--\nKajsa Hartman | Partnerships Specialist | Partnerships and Liaison Group | UNOPS Headquarters | Copenhagen, Denmark | www.unops.org<http://www.unops.org/>\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official>, YouTube<https://www.youtube.com/user/UNOPSofficial>.\nSubscribe to our external newsletter in English<https://mailchi.mp/8987deaa0e61/uojskr902j>, French<https://mailchi.mp/unops.org/gu0ld93is9> or Spanish<https://mailchi.mp/edf4b84d88cf/on3qnjwzkh>.\n[Image removed by sender.]\n\n\n--\nKajsa Hartman | Partnerships Specialist | Partnerships and Liaison Group | UNOPS Headquarters | Copenhagen, Denmark | www.unops.org<http://www.unops.org/>\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official>, YouTube<https://www.youtube.com/user/UNOPSofficial>.\nSubscribe to our external newsletter in English<https://mailchi.mp/8987deaa0e61/uojskr902j>, French<https://mailchi.mp/unops.org/gu0ld93is9> or Spanish<https://mailchi.mp/edf4b84d88cf/on3qnjwzkh>.\n[Image removed by sender.]",
                        Location = "Microsoft Teams Meeting; TBD",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZPSZMA4",
                        EmailAddresses = new List<string> { "kajsah@unops.org", "kathleen.creavalle@un.org", "julia.ripollvallcorba@un.org", "unitevc@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 14, 46, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KEn7AIAT" },
                    new List<int> { 1192 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ZRvlMAG",
                    new UNOPSInteraction
                    {
                        Name = "FCLP: JHA",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-21").ToUniversalTime(),
                        Subject = "FCLP: JHA",
                        Description = "<p>Dear Francesca<u></u><u></u></p><p><u></u> <u></u></p><p>This is great. On Friday 21 June, when works for you? 2pm UK time would be great for me (or earlier, but I realise you may be in the USA). That’s 9am EST.<u></u><u></u></p><p><u></u> <u></u></p><p>UK colleagues have further consulted and been told that its non-negotiable their side that the UK bilateral funding agreement is framed as an MoU/project agreement. They say UNOPS has agreed to this for other projects, and sent you a template on 29/05. The UK would like an answer in the next 48 hours, so that they can progress their internal approvals for the £6m UNOPS contribution, and have requested a call if possible so they can explain. Can you let us know? I’ve updated the third question below to reflect this revised information.<u></u><u></u></p><p><u></u> <u></u></p><p>Many thanks, Tom<u></u><u></u></p><p><u></u> <u></u></p><p>1. <b>Purpose of the JHA</b>. The stated purpose of the JHA is to \"provide a framework among the Donors and UNOPS for cooperation in relation to the FCLP Secretariat\". The FCLP is a partnership of countries, from all regions of the world, with a representative decision-making Steering Committee. The FCLP is not a donor fund, or governed by a donor-only board. Therefore we would recommend replacing “Donors” with “Steering Committee Members” throughout the document. The purpose of the JHA should therefore be to \"provide a framework among the <b><u>Steering Committee Members</u></b> and UNOPS for cooperation in relation to the FCLP Secretariat\". Is this acceptable?<u></u><u></u></p><p> <u></u><u></u></p><p><b>2. Could UNOPS please clarify what is the intended legal status of the JHA?</b> Specifically:<u></u><u></u></p><p>- UNOPS has said by email that the JHA is legally binding. <u></u><u></u></p><p>- The UK position is that the language used in the document (e.g. use of “shall”, “must”, “parties”, etc) may be interpreted to infer the that the document is a legally binding international treaty.<u></u><u></u></p><p>- However the JHA states that it is not an international treaty (i.e. it is not intended to create a legally binding obligation in international law). See Annex III, paragraph 13 \"The JHA does not constitute an international treaty, nor is it intended to set up a legal partnership”.<u></u><u></u></p><p>- If the JHA is not a treaty but it is legally binding, could you please clarify under the law of which jurisdiction and between which parties?<u></u><u></u></p><p>Depending on the answers to these questions, it is the UK position that a non-binding MoU may be more suitable. <u></u><u></u></p><p> <u></u><u></u></p><p><b>3. Could UNOPS please confirm the intended legal status of the bilateral legal agreements</b> referenced in the JHA? Specifically for the UK, it is non-negotiable their side that the UK bilateral funding agreement is framed as an MoU/project agreement. They say UNOPS has agreed to this for other projects, and sent you a template on 29/05. The UK would like an answer in the next 48 hours, so that they can progress their internal approvals for the £6m UNOPS contribution, and have requested a call if possible so they can explain.<u></u><u></u></p><p> <u></u><u></u></p><p><b>4. Could UNOPS please clarify references in the JHA to \"the normative policy framework of the UN\"?</b> We also note that UN/UNOPS Legislative Instruments and process requirements may have a material impact on the operation of the Secretariat. Could you please provide copies of such instruments/documents?<u></u><u></u></p><p><u></u> </p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ZRvlMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("julieda@unops.org".ToLower()) ? paoUserEmailMapping["julieda@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 18, 14, 23, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1752 },
                    new List<string> { "julieda@unops.org", "harrietjo@unops.org", "francescabo@unops.org", "asbjornb@unops.org", "guillaumele@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ah5fMAA",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with EIB on Ukraine and Moldova",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-13").ToUniversalTime(),
                        Subject = "Meeting with EIB on Ukraine and Moldova",
                        Description = "Agenda:\n\nUNOPS – introduction of the team and ongoing operations on the ground, structure and presence in the region – potential services offer/initiatives or projects (Ukraine and Moldova mainly)\nEIB - introduction of the team and activities in Ukraine and Moldova, instruments and priorities\nQ&A",
                        Location = "Luxemburg",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ah5fMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 19, 9, 14, 12, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KJi9tIAD" },
                    new List<int> { 1032 },
                    new List<string> { "mariacarmenco@unops.org", "timl@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ajXFMAY",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with KfW East Africa",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-01").ToUniversalTime(),
                        Subject = "Meeting with KfW East Africa",
                        Description = null,
                        Location = "Frankfurt",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ajXFMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("rainerf@unops.org".ToLower()) ? paoUserEmailMapping["rainerf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 19, 8, 35, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BGtIPIA1" },
                    new List<int> { 1669 },
                    new List<string> { "rainerf@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007amLRMAY",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with EIB on East Africa",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-02").ToUniversalTime(),
                        Subject = "Meeting with EIB on East Africa",
                        Description = null,
                        Location = "Luxemburg",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007amLRMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 19, 8, 51, 57, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KJQKmIAP" },
                    new List<int> { 1032 },
                    new List<string> { "mariacarmenco@unops.org", "rainerf@unops.org", "alaan@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007eGYIMA2",
                    new UNOPSInteraction
                    {
                        Name = "Meeting UNOPS ED with Raffaella Iodice, Chargée d’Affaires a.i., EU Delegation to Afghanistan",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-13").ToUniversalTime(),
                        Subject = "Meeting UNOPS ED with Raffaella Iodice, Chargée d’Affaires a.i., EU Delegation to Afghanistan",
                        Description = null,
                        Location = "Afghanistan (EU Compound)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007eGYIMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("megumiu@unops.org".ToLower()) ? paoUserEmailMapping["megumiu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 20, 18, 58, 48, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1031 },
                    new List<string> { "megumiu@unops.org", "jorge.moreiradasilva@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007eIS1MAM",
                    new UNOPSInteraction
                    {
                        Name = "Kirstine Damkjær, UNOPS DED, met with the Director Federal Ministry for Economic Cooperation and Development, Dr. Helge Zeitler and Senior Policy Officer, Mr. Clemens Kapler.",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "Kirstine Damkjær, UNOPS DED, met with the Director Federal Ministry for Economic Cooperation and Development, Dr. Helge Zeitler and Senior Policy Officer, Mr. Clemens Kapler.",
                        Description = null,
                        Location = "New-York",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007eIS1MAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 20, 19, 31, 15, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1089 },
                    new List<string> { "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007eIjlMAE",
                    new UNOPSInteraction
                    {
                        Name = "Jorge Moreira da Silva, UNOPS ED, meeting with the German State Secretary for Development Mr. Flasbarth",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2023-10-15").ToUniversalTime(),
                        Subject = "Jorge Moreira da Silva, UNOPS ED, meeting with the German State Secretary for Development Mr. Flasbarth",
                        Description = null,
                        Location = "Berlin (World Health Summit)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007eIjlMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 20, 19, 35, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KGAskIAH" },
                    new List<int> { 1089 },
                    new List<string> { "mariacarmenco@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007eIrpMAE",
                    new UNOPSInteraction
                    {
                        Name = "Meeting UNOPS ED with Florian Laudi, Director UN Department, German Ministry of Foreign Affairs",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2023-10-16").ToUniversalTime(),
                        Subject = "Meeting UNOPS ED with Florian Laudi, Director UN Department, German Ministry of Foreign Affairs",
                        Description = null,
                        Location = "Berlin",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007eIrpMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 20, 19, 37, 44, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1089 },
                    new List<string> { "mariacarmenco@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007eJWAMA2",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS ED met in NY with two Parliamentary State Secretaries, Dr. Bärbel Kofler (Development) and Dr. Bettina Hoffmann (Environment).",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2023-07-18").ToUniversalTime(),
                        Subject = "UNOPS ED met in NY with two Parliamentary State Secretaries, Dr. Bärbel Kofler (Development) and Dr. Bettina Hoffmann (Environment).",
                        Description = null,
                        Location = "NY",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007eJWAMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jorge.moreiradasilva@unops.org".ToLower()) ? paoUserEmailMapping["jorge.moreiradasilva@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 20, 19, 51, 50, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1089 },
                    new List<string> { "jorge.moreiradasilva@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007eKGvMAM",
                    new UNOPSInteraction
                    {
                        Name = "Meeting UNOPS ED with Ambassador Zahneisen, DPR of Germany Perm Rep",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2023-05-24").ToUniversalTime(),
                        Subject = "Meeting UNOPS ED with Ambassador Zahneisen, DPR of Germany Perm Rep",
                        Description = null,
                        Location = "NY",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007eKGvMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jorge.moreiradasilva@unops.org".ToLower()) ? paoUserEmailMapping["jorge.moreiradasilva@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 20, 19, 52, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1089 },
                    new List<string> { "jorge.moreiradasilva@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007eKTpMAM",
                    new UNOPSInteraction
                    {
                        Name = "Jens Wandel, UNOPS ED a.i., with Jochen Flasbarth, State Secretary (BMZ)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2023-03-05").ToUniversalTime(),
                        Subject = "Jens Wandel, UNOPS ED a.i., with Jochen Flasbarth, State Secretary (BMZ)",
                        Description = null,
                        Location = "Doha, LDC5 conference.",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007eKTpMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 20, 19, 55, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KGAskIAH" },
                    new List<int> { 1089 },
                    new List<string> { "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007elkAMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between Sonja and Ole",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-24").ToUniversalTime(),
                        Subject = "Meeting between Sonja and Ole",
                        Description = null,
                        Location = "Asiatisk Plads 2; 1448 København K. Office: 6A50",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007elkAMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alexandram@unops.org".ToLower()) ? paoUserEmailMapping["alexandram@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 6, 56, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx000009faIVIAY" },
                    new List<int> { 1123 },
                    new List<string> { "alexandram@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007ep1OMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Medical needs - assistance to Mykolaiv",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-24").ToUniversalTime(),
                        Subject = "Medical needs - assistance to Mykolaiv",
                        Description = "Presentation of the verified needs is enclosed to this invitation\n\nAgenda:\n\n\n1.    Presentation of the package (Vasyl & Iryna)\n\n2.    UNOPS’s take on the package – capacity, process etc. (Asbjørn)\n\n3.    Next steps and processes (Ulrik & Anne)\n\n4.    AOB\n________________________________________________________________________________\nMicrosoft Teams Har du brug for hjælp?<https://aka.ms/JoinTeamsMeeting?omkt=da-DK>\nDeltag i mødet nu<https://teams.microsoft.com/l/meetup-join/19%3ameeting_YzI1M2IzY2UtYzVlZS00OTY5LTlkNWItZGE5ZGIzMGVmZTcx%40thread.v2/0?context=%7b%22Tid%22%3a%2248dc02d0-bd56-411d-b7c5-a814743bafc6%22%2c%22Oid%22%3a%2200c3b8bd-3ffa-44a4-a2ce-9963eca9871c%22%7d>\nMøde-id: 337 717 861 068\nAdgangskode: f3fzk8\n________________________________\nTilmeld dig på en enhed til videomøder\nLejernøgle: teams@meet.um.dk<mailto:teams@meet.um.dk>\nVideo-id: 123 041 553 3\nFlere oplysninger<https://pexip.me/teams/meet.um.dk/1230415533>\nFor arrangører: Mødeindstillinger<https://teams.microsoft.com/meetingOptions/?organizerId=00c3b8bd-3ffa-44a4-a2ce-9963eca9871c&tenantId=48dc02d0-bd56-411d-b7c5-a814743bafc6&threadId=19_meeting_YzI1M2IzY2UtYzVlZS00OTY5LTlkNWItZGE5ZGIzMGVmZTcx@thread.v2&messageId=0&language=da-DK>\n________________________________________________________________________________",
                        Location = "Microsoft Teams-møde",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007ep1OMAQ",
                        EmailAddresses = new List<string> { "teams@meet.um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 6, 56, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fSigMAE",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Tim Lardner with Alberto Oggero, Permanent Representation of Italy to the EU, Attaché - Ukraine Recovery and Reconstruction",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-14").ToUniversalTime(),
                        Subject = "Meeting Tim Lardner with Alberto Oggero, Permanent Representation of Italy to the EU, Attaché - Ukraine Recovery and Reconstruction",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fSigMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 10, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1266 },
                    new List<string> { "mariacarmenco@unops.org", "timl@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fVNDMA2",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with DG NEAR on Tunisia / North Africa",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-18").ToUniversalTime(),
                        Subject = "Meeting with DG NEAR on Tunisia / North Africa",
                        Description = "Key topics discussed: security, access to water, waste management, health & renewable energy",
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fVNDMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("nathaliea@unops.org".ToLower()) ? paoUserEmailMapping["nathaliea@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 34, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IyQbZIAV" },
                    new List<int> { 1026 },
                    new List<string> { "nathaliea@unops.org", "mariacarmenco@unops.org", "claudiar@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fXDjMAM",
                    new UNOPSInteraction
                    {
                        Name = "NO_SUB00UQx000007fXDjMAM",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-21").ToUniversalTime(),
                        Subject = "NO_SUB00UQx000007fXDjMAM",
                        Description = "NO_DESC00UQx000007fXDjMAM",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fXDjMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 12, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KUagyIAD" },
                    new List<int> { 1026 },
                    new List<string> { "mariacarmenco@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fXDkMAM",
                    new UNOPSInteraction
                    {
                        Name = "NO_SUB00UQx000007fXDkMAM",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-21").ToUniversalTime(),
                        Subject = "NO_SUB00UQx000007fXDkMAM",
                        Description = "NO_DESC00UQx000007fXDkMAM",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fXDkMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("timl@unops.org".ToLower()) ? paoUserEmailMapping["timl@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 12, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KUagyIAD" },
                    new List<int> { 1026 },
                    new List<string> { "timl@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fY9nMAE",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS at Senior Officials Meeting (SOM) Ukraine",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "UNOPS at Senior Officials Meeting (SOM) Ukraine",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fY9nMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("janphilipk@unops.org".ToLower()) ? paoUserEmailMapping["janphilipk@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 19, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000EaDwZIAV" },
                    new List<int> { 1029 },
                    new List<string> { "janphilipk@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fYswMAE",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Tim Lardner with Mr Juha AUVINEN, DG ECHO, Head of Unit Southeast Europe and Eastern Neighbourhood",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-14").ToUniversalTime(),
                        Subject = "Meeting Tim Lardner with Mr Juha AUVINEN, DG ECHO, Head of Unit Southeast Europe and Eastern Neighbourhood",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fYswMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 9, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KUMnnIAH" },
                    new List<int> { 1029 },
                    new List<string> { "mariacarmenco@unops.org", "timl@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007faRhMAI",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Tim Lardner with KFW HQ on Ukraine",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-15").ToUniversalTime(),
                        Subject = "Meeting Tim Lardner with KFW HQ on Ukraine",
                        Description = "Meeting with KFW Headquarters: \n- Lorenz Gessner (Head of Office Ukraine/Moldau)\n- Claudia Meseck (Länderbeauftragte Ukraine)\n- Christoph Isenmann (Portfolio Manager IDP projects /IOM project in Ukraine)\n- Anna-Maria Santa Cruz (Portfolio Manager Urban Development Ukraine)\n- Anna Jamal (Environment- und Social Framework Expert, responsible for Mine Action Ukraine)\n- Fabian Nguyen (Task Team Ukraine)\n\nThematics to be discussed: \n- Mine Action in Ukraine\n- Micro districts and neighborhood reconstruction, including housing and recovery of livelihoods \n- Sustainable economic development (IDP SME support)\n- Improving water supply and sanitation facilities in larger and smaller cities and municipalities\n- Ukraine Recovery Conference 2024",
                        Location = "Frankfurt",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007faRhMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 14, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1669 },
                    new List<string> { "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fbh7MAA",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS in a panel at Global Solutions Summit",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-06").ToUniversalTime(),
                        Subject = "UNOPS in a panel at Global Solutions Summit",
                        Description = null,
                        Location = "Berlin",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fbh7MAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 21, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1126 },
                    new List<string> { "laetitiak@unops.org", "ifeomacm@unops.org", "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fc0TMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Ifeoma/Laetitia with BMWK",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-06").ToUniversalTime(),
                        Subject = "Meeting Ifeoma/Laetitia with BMWK",
                        Description = "Meeting with Stefanie Schmid-Lübbert, Head of Unit VB3 (Subsahara Afrika) und Julia Dorbandt, Unit KC4 (International Climate Initiative)",
                        Location = "Berlin",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fc0TMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("ifeomacm@unops.org".ToLower()) ? paoUserEmailMapping["ifeomacm@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 23, 43, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1864 },
                    new List<string> { "ifeomacm@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fcLSMAY",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Ifeoma with KfW",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-07").ToUniversalTime(),
                        Subject = "Meeting Ifeoma with KfW",
                        Description = "Meeting with KfW Frankfurt \nDirector Amelie Heinz and Team \nSierra Leone and Gambia",
                        Location = "Frankfurt",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fcLSMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("ifeomacm@unops.org".ToLower()) ? paoUserEmailMapping["ifeomacm@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 25, 22, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1669 },
                    new List<string> { "ifeomacm@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fcYLMAY",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with BMZ on Sierra Leone and Gambia",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-07").ToUniversalTime(),
                        Subject = "Meeting with BMZ on Sierra Leone and Gambia",
                        Description = "Meeting on Sierra Leone and Gambia: \nMs Stepping (Referat 202, Gambia) Ms Lindemann (Ref. 203, Sierra Leone, Liberia) Ms Warning (Nigeria)",
                        Location = "Frankfurt",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fcYLMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("ifeomacm@unops.org".ToLower()) ? paoUserEmailMapping["ifeomacm@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 12, 26, 51, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1089 },
                    new List<string> { "ifeomacm@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007fhMjMAI",
                    new UNOPSInteraction
                    {
                        Name = "Meeting UNOPS Tunisia / BLO with Julia Ruppel, FPI",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-18").ToUniversalTime(),
                        Subject = "Meeting UNOPS Tunisia / BLO with Julia Ruppel, FPI",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007fhMjMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 21, 13, 2, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KJbDNIA1" },
                    new List<int> {  },
                    new List<string> { "mariacarmenco@unops.org", "claudiar@unops.org", "laetitiak@unops.org", "nathaliea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007lEoIMAU",
                    new UNOPSInteraction
                    {
                        Name = "UNESCO IESALC X UNOPS : Bilateral Meeting to Explore Synergies on Higher Education's Contributions to SDGs",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-26").ToUniversalTime(),
                        Subject = "UNESCO IESALC X UNOPS : Bilateral Meeting to Explore Synergies on Higher Education's Contributions to SDGs",
                        Description = "Meeting to explore potential synergies on higher education's contributions to various SDGs.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007lEoIMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 25, 13, 29, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Km7BmIAJ" },
                    new List<int> { 1256 },
                    new List<string> { "arnauds@unops.org", "nadiamo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007lwPVMAY",
                    new UNOPSInteraction
                    {
                        Name = "Discussion of ESF language in UNOPS SFA (Madagascar)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-18").ToUniversalTime(),
                        Subject = "Discussion of ESF language in UNOPS SFA (Madagascar)",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZGIyMzQwMWYtMTRmZS00MTU1LWJlN2QtMmNlNzgxYmM0NDRl%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%2288c9c3e6-fae3-4731-b4cf-58e83bf4f2d2%22%7d>\nMeeting ID: 288 610 840 278\nPasscode: vAVTsb\n________________________________\nDial in by phone\n+1 509-408-0991,,4496514#<tel:+15094080991,,4496514#> United States, Spokane\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=4496514>\nPhone conference ID: 449 651 4#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 111 618 363 7\nMore info<https://www.webex.com/msteams?confid=1116183637&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=88c9c3e6-fae3-4731-b4cf-58e83bf4f2d2&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_ZGIyMzQwMWYtMTRmZS00MTU1LWJlN2QtMmNlNzgxYmM0NDRl@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007lwPVMAY",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 25, 18, 36, 22, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007mQSSMA2",
                    new UNOPSInteraction
                    {
                        Name = "Meeting AFR RD with Manuel Müller, Head of the EU Delegation to Sierra Leone.",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-25").ToUniversalTime(),
                        Subject = "Meeting AFR RD with Manuel Müller, Head of the EU Delegation to Sierra Leone.",
                        Description = null,
                        Location = "Sierra Leone",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007mQSSMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sofiag@unops.org".ToLower()) ? paoUserEmailMapping["sofiag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 6, 26, 6, 59, 12, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1031 },
                    new List<string> { "sofiag@unops.org", "ifeomacm@unops.org", "dalilag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000007wrkgMAA",
                    new UNOPSInteraction
                    {
                        Name = "Virtual call NORAD",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-02").ToUniversalTime(),
                        Subject = "Virtual call NORAD",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000007wrkgMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 2, 8, 52, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1101 },
                    new List<string> { "asbjornb@unops.org", "jorge.moreiradasilva@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008605ZMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Lunch WB-UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-25").ToUniversalTime(),
                        Subject = "Lunch WB-UNOPS",
                        Description = "Notes:\nOPCS plans to send the SFA for output and technical assistance around the second week of July, with the ESF wording agreed between UNOPS and WB for Madagascar. Negotiations (in-person in CPH) are planned for late September or after.\nRajeev indicated support for WB-UNOPS high-level dialogue, offered to make introductions. Christine indicated WLO will start with Maria in GVA office. Rajeev suggested we speak with UNICEF, who successfully organized one recently.\nThe ESF meeting is scheduled for next week, or earlier if possible.\n\nFollow-up:\nMegumi to study and prepare for SFA negotiations, updating Vinesh's table of UNOPS concerns and desires.\nChristine to reach out to the UNICEF counterpart to understand their strategic dialogue approach, suggest meeting with Megumi.",
                        Location = "meet at main WB entrance atrium",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008605ZMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("megumiu@unops.org".ToLower()) ? paoUserEmailMapping["megumiu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 8, 4, 12, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "megumiu@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000860YbMAI",
                    new UNOPSInteraction
                    {
                        Name = "WB Education mtg Myanmar",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-03").ToUniversalTime(),
                        Subject = "WB Education mtg Myanmar",
                        Description = "Notes: \nWas less on education and more on household poverty. Suthirtha was recently in Yangon, UNOPS staff attended his presentation. Benu will move to Bangkok this summer. Rinku is changing to East Africa portfolio. Wide ranging conversation, but focused mostly on WB analytics. No real discussion of IDA for Myanmar.\n\nFollow-ups:\nSara emailed Judith at EU, who connected her to Suthirtha, to thank her and follow up on the possibility of UNOPS support to monastic education.\nSara connected Suthirtha to LIFT and A2H.\nSara to email Benu this summer (maybe around August) to see if she is in Bangkok yet.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000860YbMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 8, 4, 21, 7, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "sarane@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000861XtMAI",
                    new UNOPSInteraction
                    {
                        Name = "World Bank & UNOPS Central Asia procurement discussion",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-04").ToUniversalTime(),
                        Subject = "World Bank & UNOPS Central Asia procurement discussion",
                        Description = "Introduction to UNOPS and World Bank procurement collaboration;\nOverview and lessons learned in UNOPS health procurement support in Uzbekistan, Tajikistan, Turkmenistan & Albania;\nOverview of UNOPS procurement advisory services;\nBrief demo of UN Web Buy Plus.\n\nAttendees from WB side:\n- Quamrul Hasan, Almaty\n- Kuat Sulan, procurement specialist Almaty\n- Huai ? in Uzbekistan\n- Jamshid Umarova, Uzbekistan\n- Fazliddin Rakhimov, Uzbekistan\n- Grace, procurement in Tajikistan [key team member]\n- Katya Adianova, Tajikistan\n- Dilshod Karimova, Tajikistan\n- Irina Goncharova, senior procurement specialist, Kyrgyzstan",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000861XtMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 8, 4, 41, 59, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LcR9ZIAV" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "alexandrarb@unops.org", "konstantine@unops.org", "sylviaac@unops.org", "peteron@unops.org", "jean-vincentc@unops.org", "freyavg@unops.org", "gurelg@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000862DqMAI",
                    new UNOPSInteraction
                    {
                        Name = "Discussion of PLEASE project",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "Discussion of PLEASE project",
                        Description = "Simonetta's notes:\nDiscussion around the PLEASE project, SAMCO:\nTTL has just visited SLK; SACEP needs to demonstrate traction before talking about a project extension\nAcknowledges that SACEP has low capacity that needs to be built, SACEP needs to demonstrate that they can influence member countries decision making and elevate their capacity to make impact at the policy level in the member countries\nAcknowledges that we ramped up activities after getting additional budget.\nSACEP perceives that UNOPS is taking over their job which is not wanted - provide support and make them succeed, UNOPS to stay in the background and make SACEP grow as an institution. Reposition UNOPS within SACEP’s hierarchical structure and get SACEPs positive attention.\nIFC interested to provide funding to grantees (in countries directly, not through UNOPS or SACEP), but would need to bundle 3-4 companies to achieve total of $10-20million investment. Appreciate that there is a huge challenge to get funding into BGD and Bhutan, but through PLEASE it was possible to access NGOs. \nNew WB/IFC Director will be deployed to SLK, directing both organisations WB and IFC. Ensure to introduce UNOPS directly to the new Director; the new Director is an Energy specialist - explore positioning of UNOPS as partner in this area.\nPLEASE - prioritize institutional reform and institutional capacity building in SACEP, CB activities need to start immediately.\nAreas of focus - strategic and higher level activities such as round table, get to policy level asap. Focus should be on the future, get SACEP to think forward, i.e. on topics of Climate Action.\nNeed to address several policies such as financial, procurement or HR policy within SACEP - review policies and modernize them. For HR, add a maximum age cap for team members to address the fact that they’re led by retirees, leading to lack of innovation and change of attitude.  Mirror other organizations and modernize SACEP.\nBuild SACEP’s capacity so that they’re able to absorb $1M contributions. Consider introducing changes when the new DG comes in.\n\nEstablish mechanisms for engagement with member states!\n\nTTL mentioned one L-ICA team member who manages grants and sits with Sarat - this person is relatively senior in terms of age, and is being heard by the SACEP team. Get him to influence SACEP. \n\nAlthough disbursement is going well, the policy part is still lagging behind and needs to be stronger. Influencing of policy making in the countries is key going forward, increase engagement with member countries.\n\nWork with SACEP to have them understand that they need to change in order to receive additional contributions in the future; need to become more fit for purpose.  \n\nFollow-ups:\n- Christine and Simonetta debriefed Charlie, Lian, Constanza on the meeting",
                        Location = "MC 10-114 and Teams",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000862DqMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 8, 4, 51, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LcOBwIAN" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008638HMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Myanmar-focused discussion with FCV Group",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "Myanmar-focused discussion with FCV Group",
                        Description = "Simonetta's notes:\nBGD - WB funding for host community support has been made available\nPNG - is of particularly high interest to the WB, \nNew partnership framework is in development. WB wants to work more on access and security - how to do business, how to reach certain areas => to remain relevant we need to show that we can go to geographies where others can’t go.",
                        Location = "MC 3-500 (24) (VC), MC 4-715 (22) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008638HMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 8, 5, 0, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Hy4OjIAJ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "sarane@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000088D8LMAU",
                    new UNOPSInteraction
                    {
                        Name = "EAP INF meeting with UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "EAP INF meeting with UNOPS",
                        Description = "Notes from Simonetta:\nOngoing discussions with UNOPS in EAP - Cambodia, Philippines, Tuvalu\n\nSouth-Asia Region - disaster risk reduction, policy interpretation and quality infrastructure are key\n\nHealth & Energy transition nexus -> transformation of health sector to become more sustainable (more green E generation) needed.\nEnergy transformation agenda - ASCET ($3B) -> goal is transmission, not so much access to green energy; access to energy in Asia is satisfactory, enabling the access to renewable E is important. \n\nTTL is based in Jakarta and working on strategy alignment and compliance, additional country TTLs are taking care of other issues, not energy/infrastructure\n\nFollow-ups:\nPut Julian in touch with ETP to discuss further",
                        Location = "MC 9-300 (20) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000088D8LMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 9, 1, 23, 4, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LgfPKIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000088DI1MAM",
                    new UNOPSInteraction
                    {
                        Name = "Meeting EAP country program officers",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "Meeting EAP country program officers",
                        Description = "Notes from Simonetta:\nGet in touch with the new WB CD in Bangkok - Fabrizio ?\nNew WB CD in Philippines, more important to meet with the OPS Manager to discuss BARMM\nPNG - ensure to keep in touch and keep them informed; WB request to keep them informed about what is happening at country level and how UNOPS can support the WB with the execution of projects; interested to learn how we can help them increase delivery and how to go beyond a project by project collaboration - follow up needed with BKK based personnel\n\nDevelopment of the ASCET Program - Energy transition project that focuses on Indonesia, multi-phased approach, operationalisation is agreed with the Gov. to increase transition lines. The ASCET is similar to the ASCENT Program in Africa.\n\nTimor Leste - at the moment not much happening due to the Government reviewing all the donor funding.\n\nPacific - WB has disbursement issues due to the low capacities in the countries; time consuming and resource heavy.\nPNG, RMI, South Pacific - implementation support units are there, but more support is needed going forward.\n\nHealth - WB values partnership with UNOPS and plans to continue collaboration; for other sectors such as climate resilience and transport, the portfolios are still in development and the pipeline is now under review. This is mostly handled by the country teams, not out of DC.\nFSM - new project starting in July.\nNew permanent presence in Fiji - Stefano - was former CD in PNG.\n\nWB teams with CD, OPS Managers and TAs in Sydney, Fiji, FSM, PNG and Solomon Islands; out of these locations they cover all the Pacific SIDS. \n\nFrode - covering Malaysia, Thailand, Brunei and the Philippines - appreciates collaboration with UNOPS and is open to explore additional opportunities - important to reach out to the CD in these countries. \nCountry Framework is being updated, finalized by the end of 2024.\nWould like to see more engagement in the Health sector going forward, new opportunities coming up such as in BARMM and in Energy transition.",
                        Location = "MC 9-300 (20) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000088DI1MAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 9, 1, 26, 58, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IofDiIAJ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000088DTJMA2",
                    new UNOPSInteraction
                    {
                        Name = "Disaster Risk Management for Health Systems in WB",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-05").ToUniversalTime(),
                        Subject = "Disaster Risk Management for Health Systems in WB",
                        Description = "Notes from Simonetta:\nDRM in Health is cross cutting, embedded in various other departments, focus on emergency preparedness and response.\nNew stream coming up => Climate and Health (pandemic is seen as ‘shock’)\nMainly providing operational support to the Ministries of Health in the countries during response times, through TA (80% of their work)\nWorking closely with country TTLs on different sectors\nDRR on SIDS - TA needed, how can UNOPS provide support to countries for CB but also provide the WB with information on what is happening on the SIDS?\n\nTimor Leste - new project in Health sector has already been approved by the WB Board, but currently on hold.\n\n\nCambodia - WB provides shelter support; get in touch with WB to offer support.",
                        Location = "WB main building atrium",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000088DTJMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 9, 1, 31, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IoSDKIA3" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000088DWYMA2",
                    new UNOPSInteraction
                    {
                        Name = "60 year WB procurement anniversary",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-06").ToUniversalTime(),
                        Subject = "60 year WB procurement anniversary",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000088DWYMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 9, 1, 58, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LgnrRIAR" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000088DhpMAE",
                    new UNOPSInteraction
                    {
                        Name = "EAP Health team",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-06").ToUniversalTime(),
                        Subject = "EAP Health team",
                        Description = "Notes from Simonetta:\nAppreciate growing portfolio, now 33 projects in EAP of which 12 with UNOPS. WB is growing their collaboration with UNOPS also in countries with higher capacities such as the Phil.\n\nClimate resilient infrastructure is one focus area, particularly in the Pacific. E.g. Tonga - every building needs to mainstream climate considerations, we’re asked to look into the development of a prototype that can be adapted to local needs. Develop best practices for Phil., Indonesia and Pacific in particular.\n\nWB relies on UNOPS to build capacities for both procurement and infrastructure, but UNOPS needs to understand and follow the WB rules & regulations.\n\nREQUEST from WB - disburse faster - we are requested to become faster and more responsive when it comes to disbursement, they were mentioning delays from the day of signing to the first disbursement. Gap needs to be closed. Request to improve pre-contract mobilization. - UNOPS to deliver with speed and quality! => this specific ‘complaint’ is related to the PNG Incinerator case - slow start; advance feasibility and analysis needed to speed up. \n\nWB mentioned that fees were to high - we elaborated on how UNOPS calculates fees\n\nComment on in-country capacity of UNOPS, specifically directed to our presence in SIDS - WB team expressed concerns if it is enough to deliver on what we promised…? Countries also concerned about UNOPS capacity - they don’t see that we have enough capacity to deliver and construct what we take on, ie. PNG, Tuvalu, Tonga, Samoa - more people are needed to be deployed to the Pacific. Big expectations from the Fiji WB office. [we clarified that UNOPS is planning to increase its presence in the Pacific] \n\nPhilippines - DOH complained that they are being charged additional 17% for ProjMgmt and oversight across 6 activities. [Christine already followed up]\n\nPapua New Guinea - Streamlining procedures in PNG - once we’ve gone through direct selection, we need to go through the WB procedures -> we can’t change approach once the contract is signed.\n UNOPS to ensure that procurement specialist is experienced enough and understands processes and procedures.\n\nUNOPS to better engage with the Government - although the WB provides the funding, the contract is signed between UNOPS and the Gov. WB sees lack of stakeholder engagement; however, UNOPS to ensure to better coordinate and report back to the WB. Also, WB suggests that we explain better to countries how we procure (!!).\n\nPacific - expressed serious concerns regarding the new hospital construction. Ensure to reach out and remain in close communication with the WB.\nQuality assured drugs - in PNG, acknowledge political sensitivity; ensure to comply with quality standards.\n\nTimor Leste - we’re requested to proactively engage with WB in TimorL. Engagement is signed but no mobilization possible. Ensure climate change mainstreaming!\n.\nTonga - keep the Government better informed about how we work and how we build their capacity. Reach out to Kerry Hart regarding the design and construction of the hospital. \n\nWhile going out after the meeting, the team expressed serious concerns and requested that we follow up on the Tuvalu hospital!\n\nFollow-ups:\nNaoko requested follow up email with clarification on our CB support in O&M where UNOPS is constructing health infrastructure; + contractual obligations, i.e. during DNP for infra and equipment.\n\nPresent on procurement to BKK based team - Simonetta and Christine to follow up\n\nTonga - reach out to Kerry Hart regarding the design and construction of the hospital. \n\nEnsure follow up on Tuvalu hospital",
                        Location = "MC 9-401 and teams connection",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000088DhpMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 9, 1, 36, 1, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LgkGsIAJ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000088EE5MAM",
                    new UNOPSInteraction
                    {
                        Name = "Disaster Risk Management Unit (EAP, SAR)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-06").ToUniversalTime(),
                        Subject = "Disaster Risk Management Unit (EAP, SAR)",
                        Description = "Notes from Simonetta:\nAppreciate UNOPS role in countries where there is no Government\nDRM - Hydro-Met & Early Warning to compare urban resilience before/after the floods in Afgh\n\nHow to operationalize DRM? Portfolios are managed by TTLs, DRM comes in when TTLs are asking\nDRM in SA is stand alone, not under any other department\nSA - multi-sectoral approach in India\nGFDRR programme for emergency response and preparedness in various areas, early warning for all initiatives collaboration with ITU to bring in technical expertise.\n\nNepal - on the left over funding from the MPTF (follow up from Simonetta’s meeting in KTM)",
                        Location = "MC 10-114 (11) (VC) Private",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000088EE5MAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 9, 1, 51, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IoXKyIAN" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx0000088EPOMA2",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Asia - Small States Secretariat Meeting",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-06").ToUniversalTime(),
                        Subject = "UNOPS Asia - Small States Secretariat Meeting",
                        Description = "Notes from Simonetta:\nWill be doing more analytical work going forward, focussing on what’s going on when and why in small states\nWorking closely with IDA to help analyze how to best cater to the needs of small states\n\n\nThe Secretariat has more the role of a policy holder within the WB rather than looking outwards and working with partners\n\n\nOrganizing fora to bring all SIDS together, such as the Small States Forum which is happening annually; aim is to move away from the debt issue and towards Blue Economy\nGender also of high interest\n\nCurrently developing an internal Pacific SIDS framework for the WB to understand how they can better engage with PSIDS\nCorporate Scorecard work - this will be developed for all sectors to help identify/measure how indicators can be reached; when talking about ‘Retrofitting’ - also for active engagements, all need to incorporate these scorecard indicators. All MDBs are in the process of harmonizing these indicators.",
                        Location = "MC 11-300 (21) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000088EPOMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 9, 1, 56, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LgggUIAR" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008FtdtMAC",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-UN-Habitat meeting to discuss the UNOPS-UN-Habitat partnership",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-08").ToUniversalTime(),
                        Subject = "UNOPS-UN-Habitat meeting to discuss the UNOPS-UN-Habitat partnership",
                        Description = "Microsoft Teams\n\nNeed help? https://aka.ms/JoinTeamsMeeting?omkt=en-US\n\nJoin the meeting now: https://teams.microsoft.com/l/meetup-join/19%3ameeting_YTllMzZkOTctYTZjZC00N2Q5LTllMGEtODUzN2RiY2VlNGVm%40thread.v2/0?context=%7b%22Tid%22%3a%220f9e35db-544f-4f60-bdcc-5ea416e6dc70%22%2c%22Oid%22%3a%22fdb1c86e-aea8-4ec2-8b0b-fa38f84baca1%22%7d\n\nMeeting ID: 362 678 394 349\nPasscode: jxVa8A\n\n\n\nJoin on a video conferencing device\n\nTenant key: unitevc@m.webex.com\nVideo ID: 124 139 150 9\nMore Info: https://www.webex.com/msteams?confid=1241391509&tenantkey=unitevc&domain=m.webex.com\n\nFor organizers:\n\nMeeting options: https://teams.microsoft.com/meetingOptions/?organizerId=fdb1c86e-aea8-4ec2-8b0b-fa38f84baca1&tenantId=0f9e35db-544f-4f60-bdcc-5ea416e6dc70&threadId=19_meeting_YTllMzZkOTctYTZjZC00N2Q5LTllMGEtODUzN2RiY2VlNGVm@thread.v2&messageId=0&language=en-US",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008FtdtMAC",
                        EmailAddresses = new List<string> { "unitevc@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 12, 13, 18, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LhnHVIAZ" },
                    new List<int> { 1193 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008IUStMAO",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between GCF and UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-24").ToUniversalTime(),
                        Subject = "Meeting between GCF and UNOPS",
                        Description = "Dear both,\n\nThis email is to block your agendas for this conversation.\n\nI leave to you to suggest/decide the location of this meeting.\n\nBest,\n\n\nJúlia Ripoll Vallcorba (she/her)\nProgramme Management Officer, Director’s Office\nCorporate Services Division\nUnited Nations Environment Programme\nNairobi, Kenya |  unep.org<http://www.unep.org/>\n\n\n\n[cid:image001.png@01DAC253.853AD370]",
                        Location = "TBC",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008IUStMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 15, 8, 31, 16, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KEn7AIAT" },
                    new List<int> { 1192 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008IaMzMAK",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/UNEP - COP29 catch-up",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-20").ToUniversalTime(),
                        Subject = "UNOPS/UNEP - COP29 catch-up",
                        Description = "Hej Niklas, \n\nJag skickar en preliminär mötesinbjudan för klockan 14:15 på torsdag. Jag ser fram emot att träffas! \n\nMvh, \nKajsa",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008IaMzMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 15, 8, 32, 16, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KEZgtIAH" },
                    new List<int> { 1192 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008JJAPMA4",
                    new UNOPSInteraction
                    {
                        Name = "Meeting SPBF & UNOPs",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-09").ToUniversalTime(),
                        Subject = "Meeting SPBF & UNOPs",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NWFiZWY0YWUtOWY4MS00MDAwLTlmZTAtYTFkMmU5NWQyYjFi%40thread.v2/0?context=%7b%22Tid%22%3a%220f9e35db-544f-4f60-bdcc-5ea416e6dc70%22%2c%22Oid%22%3a%22523573bf-9f17-46c9-805b-799972fda8a4%22%7d>\nMeeting ID: 376 414 593 357\nPasscode: 8rspWx\n________________________________\nJoin on a video conferencing device\nTenant key: unitevc@m.webex.com\nVideo ID: 128 764 730 4\nMore info<https://www.webex.com/msteams?confid=1287647304&tenantkey=unitevc&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=523573bf-9f17-46c9-805b-799972fda8a4&tenantId=0f9e35db-544f-4f60-bdcc-5ea416e6dc70&threadId=19_meeting_NWFiZWY0YWUtOWY4MS00MDAwLTlmZTAtYTFkMmU5NWQyYjFi@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008JJAPMA4",
                        EmailAddresses = new List<string> { "unitevc@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 15, 13, 22, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LhmdBIAR" },
                    new List<int> { 1192 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008Kc2vMAC",
                    new UNOPSInteraction
                    {
                        Name = "Exchange w/UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-16").ToUniversalTime(),
                        Subject = "Exchange w/UNOPS",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZmQ4ZWNiYjAtMTgyYS00YmUwLTkyNjktZjExZGM1YTE4MWNl%40thread.v2/0?context=%7b%22Tid%22%3a%2277920909-8782-4efb-aaf1-44ac114d7c03%22%2c%22Oid%22%3a%220164fa18-f153-43b4-a094-24ae0b15737b%22%7d>\nMeeting ID: 276 843 637 223\nPasscode: pvCrsx\n________________________________\nDial in by phone\n+41 43 430 76 18,,433234411#<tel:+41434307618,,433234411> Switzerland, Zurich/Zürich/Zurigo (Zurich)\nFind a local number<https://dialin.teams.microsoft.com/9c0069fb-66ab-427c-ab2f-baae6789f537?id=433234411>\nPhone conference ID: 433 234 411#\nJoin on a video conferencing device\nTenant key: teams@vc.theglobalhealthcampus.org\nVideo ID: 119 194 815 2\nMore info<https://pexip.me/teams/vc.theglobalhealthcampus.org/1191948152>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=0164fa18-f153-43b4-a094-24ae0b15737b&tenantId=77920909-8782-4efb-aaf1-44ac114d7c03&threadId=19_meeting_ZmQ4ZWNiYjAtMTgyYS00YmUwLTkyNjktZjExZGM1YTE4MWNl@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\nNOTICE: This meeting may be recorded, including video and audio recording and automatic transcription. By joining the meeting you consent to the meeting being recorded and/or transcribed.\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008Kc2vMAC",
                        EmailAddresses = new List<string> { "teams@vc.theglobalhealthcampus.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("louisel@unops.org".ToLower()) ? paoUserEmailMapping["louisel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 16, 7, 58, 28, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1448 },
                    new List<string> { "louisel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008LcAXMA0",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-UN-Habitat collaboration",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-16").ToUniversalTime(),
                        Subject = "UNOPS-UN-Habitat collaboration",
                        Description = "Dear colleagues,\n\nPlease find here a meeting invitation to discuss UNOPS-UN-Habitat collaboration under the CDRI IRIS call for proposals.\n\nLooking forward to the discussion on Monday.\n\nKind regards,\nKajsa",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008LcAXMA0",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("delphineevodiek@unops.org".ToLower()) ? paoUserEmailMapping["delphineevodiek@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 16, 14, 45, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LhiHrIAJ" },
                    new List<int> { 1193 },
                    new List<string> { "delphineevodiek@unops.org", "tatianaw@unops.org", "entelas@unops.org", "kajsah@unops.org", "rodriguespaulon@unops.org", "telmasuelyd@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008Ni5PMAS",
                    new UNOPSInteraction
                    {
                        Name = "Phone Call with USAID",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-07-17").ToUniversalTime(),
                        Subject = "Phone Call with USAID",
                        Description = "Check-in with USAID Main POC on removing the problematic PIO language and sunsetting the July 2022 special clauses. \n\nStatus of PIO Language: PLR Bureau leadership has approved the removal. The ADS Team, which actually changes USAID's rules and regulations, is now implementing the change. It will take several weeks for them to do so.  \n\nSunset of July 2022 Clauses: USAID is reviewing the cost recovery methodology provided.  FG is preparing additional information for USAID on costings, cost recovery model, and USAID-related indirect costs.  \n\nNext meeting is on July 23, 2024.",
                        Location = "Virtual",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008Ni5PMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 17, 14, 31, 27, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GsmeQIAR" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008NklWMAS",
                    new UNOPSInteraction
                    {
                        Name = "Update on INL LOA Discussion",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-07-10").ToUniversalTime(),
                        Subject = "Update on INL LOA Discussion",
                        Description = "INL wanted to check in post vacation to discuss status of INL changes.  Bureau senior leadership is reviewing text.  Need to get final approval for projects in Moldova and Laos to move forward. \n\nOnce approved, INL will send out an administrative notice regarding the change.",
                        Location = "Virtual",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008NklWMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 17, 14, 38, 12, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HFY62IAH" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008UX1yMAG",
                    new UNOPSInteraction
                    {
                        Name = "Intro: UNOPS / Sweden",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-25").ToUniversalTime(),
                        Subject = "Intro: UNOPS / Sweden",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008UX1yMAG",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 22, 9, 1, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KJ6tNIAT" },
                    new List<int> { 1267 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008VOCjMAO",
                    new UNOPSInteraction
                    {
                        Name = "Meeting GAC on Funding for Office of the Quartet",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-07-19").ToUniversalTime(),
                        Subject = "Meeting GAC on Funding for Office of the Quartet",
                        Description = "A new GAC contact called regarding contract clauses for $4.5 CAD funding for the Office of the Quartet. I reached out to the ME Region and LG. A meeting is set for the week of July 22 to resolve this. We are also looking at the other agreements signed with Canada this year to see what other environmental clauses were used.",
                        Location = "Virtual Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008VOCjMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 22, 14, 52, 54, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MWwx5IAD" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008bReCMAU",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with USAID Counterpart",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-07-24").ToUniversalTime(),
                        Subject = "Meeting with USAID Counterpart",
                        Description = "Below are the significant points of our discussion:\n\n\nEB Preparation: USAID is preparing its position paper for the upcoming EB session at the end of August. USAID’s main priorities for the session are updates on implementing a speak-up culture, digitization efforts, accountability, and new risk systems. I shared updates on these topics with him but underscored again that accountability questions for S3i should be addressed to OIOS and OLA. We agreed that I would provide additional information and updates to feed into the policy paper and US talking points ahead of the EB.   \n\n\nEB Attendance: Liz Buckingham, the Multilateral Team Lead in the Planning, Learning, and Resource Management Bureau (PLR), will attend the EB sessions for USAID. If we can make it happen, a short meeting with one of the two DEDs would be super helpful. \n\n\nPIO Language: The PLR Bureau approved the removal of the requirement that they had to approve any new funding. The ADS Group, which publishes official USAID regulations, will now implement the change. It should happen in the next few weeks. We won’t get any notification. So, I will keep checking the ADS website and let you know when it is done. \n\n\nJuly 2022 Special Clauses: Andrew warned that this will likely take \"several months\" to complete. He said USAID General Counsel and the Finance Team have questions regarding our cost recovery methodology, net zero budgeting, etc. We agreed on the next step for FG to brief USAID staff sometime around August 14 and answer their questions.  (We did a similar briefing for the State Department, which was super helpful.) After this briefing, we will reassess the next steps and see if we can expedite the “sunset” of these clauses. \n\n\nRequest UNGA HL Meeting with USAID Administrator Ambassador Power: Andrew shared the official meeting request template that we need to submit. I will work on this and get it to you for your review. We should send it from the EO, and then I can double-track via my contacts. Andrew said the meeting should be focused on Gaze, Ukraine, etc., and not the ADS language or the July 2022 clauses. There are no guarantees, but we have a shot at getting this meeting at UNGA HL week. (I will also do the note verbale requested by the EO for this meeting.)\n\n\nVisit of DED August 28/29 to Washington: Andrew shared that “this is not the best time” to come to Washington as it is the Thur/Friday before the last major holiday weekend of the summer. We can make this visit happen, but most of the DED’s counterparts will likely be acting and unable to move our issues forward.",
                        Location = "Compass Coffee, 435 11th Street, NW Washington, DC",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008bReCMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 25, 15, 33, 19, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000GsmeQIAR" },
                    new List<int> { 1112 },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008bcXxMAI",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS - UNAIDS potential collaboration",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-26").ToUniversalTime(),
                        Subject = "UNOPS - UNAIDS potential collaboration",
                        Description = "Dear Stephan and Tim,\n\nThis meeting is to discuss a potential collaboration between UNOPS and UNAIDS on HR services.\n\nFrom our end, the meeting will include:\n- Arnaud SGAMBATO, Head of Unit - Partnerships Development, Partnerships and Liaison Group (PLG)\n- Lorraine ANABTAWI, HR Partnerships Specialist, Partnerships and Liaison Group (PLG)\n- Victoria CAMPBELL, Deputy Director, People & Culture Group (PCG)\n- Alejo EIRIZ, Senior HR Manager - Policy & Compliance, People & Culture Group (PCG)\n\nWe look forward to our meeting and discussion.\n\nKind regards,\n\nArnaud",
                        Location = "CPH-5-7.36-Room (12) [Cisco VC, Google Meet]",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008bcXxMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 25, 15, 36, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000M8zXwIAJ" },
                    new List<int> { 1221 },
                    new List<string> { "arnauds@unops.org", "victoriac@unops.org", "laurentium@unops.org", "lorrainea@unops.org", "alejoe@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008btXBMAY",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Greg Garramone (State/IO)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-24").ToUniversalTime(),
                        Subject = "Meeting with Greg Garramone (State/IO)",
                        Description = "I had a good meeting with Greg Garramone on July 24 covering the overall US-UNOPS relationship. Here are the main points:\n\nExecutive Board Preparation:  The United States is currently preparing its statement and position papers for the late August session. He expressed optimism about having a very constructive discussion. Similar to USAID, the U.S. priorities will include updates on fostering a speak-up culture, advancing digitization, and implementing new risk systems. Regarding accountability, he acknowledged that questions should be directed to OIOS and OLA. He also inquired about the timeline for OLA and UNOPS to consider the outstanding S3i loans as unrecoverable. Additionally, he suggested that a brief meeting between the DEDs and Greg and Nancy LaMana (USUN) on the sidelines of the EB session would be highly beneficial.\n\nNew INL Standardized Text: He was pleased that we’ve made tremendous progress on the new text for INL. He asked that we let I/O and USUN know as soon as the LOAs were complete. \n\nINL and PRM Projects: Greg was pleased to hear that potential projects are progressing. He noted that signing new projects would represent a significant step towards restoring a more normalized business relationship with the State Department.\n\nCongressional Notifications (CN):  Greg concurred that submitting a new CN to Congress is crucial for demonstrating to remaining skeptics at USAID and the State Department that partnering with UNOPS on projects is viable once more.\n\nI/O PDAS McFarland: We discussed the unusual situation where I/O Assistant Secretary Sison supports UNOPS, while I/O Principal Deputy A/S David McFarland still informally advises State Bureaus not to implement projects through UNOPS. He mentioned that he and his colleagues are working on addressing McFarland’s objections. We both agreed that a successful CN would be a crucial step in overcoming these objections. He suggested continuing to engage with McFarland and seeking opportunities for him to interact with the DEDs in the coming months.\n\nPossible DED Trip to DC on August 28/29: Greg suggested that another week would be better for the DED to comet to Washington. He noted that the I/O UNOPS team would be in New York the preceding week and unavailable to help prepare and staff the meeting. Like USAID, he expressed concern that the meeting falls on the Thursday and Friday before the long Labor Day weekend, when many DED counterparts might be unavailable. He recommended finding another time for her visit or arranging a virtual meeting with PDAS McFarland. \n\nUNGA HL Meetings: Greg mentioned that I/O A/S Sison would be very interested in meeting Jorge during UNGA HL week. He suggested we send him an email requesting the meeting, along with any information we plan to send to A/S Sison's office. He was confident that Ambassador Lu would also meet with Jorge during UNGA HL week.\n\nGeneva Group Talk: Regarding the Geneva Group talk on September 28, Greg said the State Department and USAID are just starting UNGA preparations. He is working on the agenda, speakers, and logistics of the Geneva Group meeting and promised more details soon. (I would plan on blocking September 28 on Jorge’s schedule for this talk.)",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008btXBMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 25, 20, 3, 12, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008gckIMAQ",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS - CAF Próximos pasos",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-20").ToUniversalTime(),
                        Subject = "UNOPS - CAF Próximos pasos",
                        Description = "________________________________________________________________________________\nMicrosoft Teams ¿Necesita ayuda?<https://aka.ms/JoinTeamsMeeting?omkt=es-ES>\nUnirse a la reunión ahora<https://teams.microsoft.com/l/meetup-join/19%3ameeting_M2RiZTkxNTEtMTI2MC00MGJhLWE4YjItOTk3NzUwODliYTZj%40thread.v2/0?context=%7b%22Tid%22%3a%22863e38af-aa47-45c7-a525-20465c654244%22%2c%22Oid%22%3a%22fb788c70-cecd-43ec-9f7f-0c91f151bcd0%22%7d>\nId. de reunión: 213 779 986 16\nCódigo de acceso: yLRioh\n________________________________\nUnirse en un dispositivo de videoconferencia\nClave de inquilino: cafvid@m.webex.com\nId. del vídeo: 114 371 729 8\nMás información<https://www.webex.com/msteams?confid=1143717298&tenantkey=cafvid&domain=m.webex.com>\nPara organizadores: Opciones de la reunión<https://teams.microsoft.com/meetingOptions/?organizerId=fb788c70-cecd-43ec-9f7f-0c91f151bcd0&tenantId=863e38af-aa47-45c7-a525-20465c654244&threadId=19_meeting_M2RiZTkxNTEtMTI2MC00MGJhLWE4YjItOTk3NzUwODliYTZj@thread.v2&messageId=0&language=es-ES>\n[https://www.caf.com/media/3381092/logo-teams.png]\n________________________________________________________________________________\n\n==================================================\nAntes de imprimir, piense en el medio ambiente. Before printing think about the environment. Antes de imprimir, pense no meio ambiente. Avant d´imprimer, pensez à l'environnement La información que contiene este mensaje, así como sus anexos, si los hubiere, es privilegiada, confidencial y protegida por ley. Solo es para el uso exclusivo de los destinatarios arriba mencionados. Si usted no es el destinatario, el uso, difusión, lectura o copia no autorizada de este mensaje o sus anexos, si los hubiere, está estrictamente prohibido por ley. En caso de haber recibido este mensaje por error, favor notifique inmediatamente al emisor y proceda a su destrucción. Gracias. The information in this message and the accompanying documents, if any, is confidential, privileged and protected by law. It is intended only for the use of its addressee(s) listed above. If you, the reader of this message, are not the intended recipient, you are hereby notified that you should not read, copy, further disseminate, distribute, or forward this message or its accompanying documents. If you have received this message by mistake, please notify the sender immediately and delete it. Thank you.",
                        Location = "Reunión de Microsoft Teams",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008gckIMAQ",
                        EmailAddresses = new List<string> { "cafvid@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("isabelaf@unops.org".ToLower()) ? paoUserEmailMapping["isabelaf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 29, 14, 43, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MwQwuIAF" },
                    new List<int> { 1011 },
                    new List<string> { "isabelaf@unops.org", "solv@unops.org", "leyres@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000008ge3fMAA",
                    new UNOPSInteraction
                    {
                        Name = "LICA management for UNFPA",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-31").ToUniversalTime(),
                        Subject = "LICA management for UNFPA",
                        Description = "Check-in between UNFPA and UNOPS to discuss next steps:<br><ul><li>customized LICA </li><li>SLA/Exchange of Letters  </li><li>payroll/billing (TBC)</li></ul>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008ge3fMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("lorrainea@unops.org".ToLower()) ? paoUserEmailMapping["lorrainea@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 29, 15, 41, 23, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MiuMqIAJ" },
                    new List<int> { 1195 },
                    new List<string> { "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008gekQMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Placeholder: meeting with IADB",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-19").ToUniversalTime(),
                        Subject = "Placeholder: meeting with IADB",
                        Description = "________________________________________________________________________________Microsoft Teams Need help?&lt;<a href=\"https://aka.ms/JoinTeamsMeeting?omkt=en-US\" target=\"_blank\"><u>https://aka.ms/JoinTeamsMeeting?omkt=en-US</u></a>&gt;Join the meeting now&lt;<a href=\"https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZDIyODU5OWItMjU1Ni00ZDAwLWE5NDYtY2E5NmRlNDNiZWY5%40thread.v2/0?context=%7b%22Tid%22%3a%229dfb1a05-5f1d-449a-8960-62abcb479e7d%22%2c%22Oid%22%3a%2223930f09-d5a9-4c5e-9f0d-908d8110e4e3%22%7d\" target=\"_blank\"><u>https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZDIyODU5OWItMjU1Ni00ZDAwLWE5NDYtY2E5NmRlNDNiZWY5%40thread.v2/0?context=%7b%22Tid%22%3a%229dfb1a05-5f1d-449a-8960-62abcb479e7d%22%2c%22Oid%22%3a%2223930f09-d5a9-4c5e-9f0d-908d8110e4e3%22%7d</u></a>&gt;Meeting ID: 258 226 846 307Passcode: 6JobhB________________________________Dial in by phone+1 253-343-5838,,433188680#&lt;tel:+12533435838,,433188680&gt; United States, TacomaFind a local number&lt;<a href=\"https://dialin.teams.microsoft.com/3e3e74cf-d61d-4b31-9e10-c7483ca54c4e?id=433188680\" target=\"_blank\"><u>https://dialin.teams.microsoft.com/3e3e74cf-d61d-4b31-9e10-c7483ca54c4e?id=433188680</u></a>&gt;Phone conference ID: 433 188 680#Join on a video conferencing deviceTenant key: <a href=\"mailto:iadb@m.webex.com\" target=\"_blank\"><u>iadb@m.webex.com</u></a>&lt;mailto:<a href=\"mailto:iadb@m.webex.com\" target=\"_blank\"><u>iadb@m.webex.com</u></a>&gt;Video ID: 115 776 735 3More info&lt;<a href=\"https://www.webex.com/msteams?confid=1157767353&tenantkey=iadb&domain=m.webex.com\" target=\"_blank\"><u>https://www.webex.com/msteams?confid=1157767353&amp;tenantkey=iadb&amp;domain=m.webex.com</u></a>&gt;For organizers: Meeting options&lt;<a href=\"https://teams.microsoft.com/meetingOptions/?organizerId=23930f09-d5a9-4c5e-9f0d-908d8110e4e3&tenantId=9dfb1a05-5f1d-449a-8960-62abcb479e7d&threadId=19_meeting_ZDIyODU5OWItMjU1Ni00ZDAwLWE5NDYtY2E5NmRlNDNiZWY5@thread.v2&messageId=0&language=en-US\" target=\"_blank\"><u>https://teams.microsoft.com/meetingOptions/?organizerId=23930f09-d5a9-4c5e-9f0d-908d8110e4e3&amp;tenantId=9dfb1a05-5f1d-449a-8960-62abcb479e7d&amp;threadId=19_meeting_ZDIyODU5OWItMjU1Ni00ZDAwLWE5NDYtY2E5NmRlNDNiZWY5@thread.v2&amp;messageId=0&amp;language=en-US</u></a>&gt; | Reset dial-in PIN&lt;<a href=\"https://dialin.teams.microsoft.com/usp/pstnconferencing\" target=\"_blank\"><u>https://dialin.teams.microsoft.com/usp/pstnconferencing</u></a>&gt;_________________",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008gekQMAQ",
                        EmailAddresses = new List<string> { "iadb@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("isabelaf@unops.org".ToLower()) ? paoUserEmailMapping["isabelaf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 29, 14, 41, 37, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1442 },
                    new List<string> { "isabelaf@unops.org" },
                    new List<string> {  }
                ),
                new (
                    "00UQx000008icbJMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Informal chat on Executive Board - August",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-08").ToUniversalTime(),
                        Subject = "Informal chat on Executive Board - August",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008icbJMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 30, 15, 15, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000KJ6tNIAT" },
                    new List<int> { 1267 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008jvqMMAQ",
                    new UNOPSInteraction
                    {
                        Name = "UNH UNOPS Thank you and next steps",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-25").ToUniversalTime(),
                        Subject = "UNH UNOPS Thank you and next steps",
                        Description = "Dear Kajsa and colleagues,\n\nKindly for your participation in the discussion on our collaboration.\nLet me know if the meeting time works.\n\nBest regards,\nRan\n\n________________________________\nFrom: Kajsa Johanna HARTMAN <kajsah@unops.org>\nSent: Wednesday, July 24, 2024 11:00:13 AM\nTo: Ran Wang <ran.wang@un.org>\nCc: Erfan Ali <erfan.ali@un.org>; Amrita NAUL [UNOPS] <AmritaN@unops.org>; Emilie Potvin [UNOPS] <emiliep@unops.org>; Cherrilyn Wanyangu Auma(Intern) <cherrilyn.auma@un.org>\nSubject: Re: Thank you and next steps\n\nDear Ran,\n\nThanks for your quick reply. Yes, I am available for a call either tomorrow morning or alternatively from Tuesday onwards next week. Please let me know when would work for you.\n\nIn the meantime, I would be grateful for any feedback in relation to point 1, as well as any contacts for implementing partner agreements.\n\nThanks so much in advance, and looking forward to speaking with you soon.\n\nKind regards,\nKajsa\n\nOn Wed, 24 Jul 2024 at 12:42, Ran Wang <ran.wang@un.org<mailto:ran.wang@un.org>> wrote:\nDear Kajsa,\n\nThank you so much for the update. The information has been briefed to Erfan.\nTo follow-up the agreed points, would you be available for a meeting so that we can discuss into details?\nThank you so much and looking forward to your feedback.\n\nBest regards,\nRan\n________________________________\nFrom: Kajsa Johanna HARTMAN <kajsah@unops.org<mailto:kajsah@unops.org>>\nSent: 24 July 2024 12:23\nTo: Erfan Ali <erfan.ali@un.org<mailto:erfan.ali@un.org>>; Ran Wang <ran.wang@un.org<mailto:ran.wang@un.org>>\nCc: Amrita NAUL [UNOPS] <AmritaN@unops.org<mailto:AmritaN@unops.org>>; Emilie Potvin [UNOPS] <emiliep@unops.org<mailto:emiliep@unops.org>>\nSubject: Re: Thank you and next steps\n\nYou don't often get email from kajsah@unops.org<mailto:kajsah@unops.org>. Learn why this is important<https://aka.ms/LearnAboutSenderIdentification>\nDear Erfan,\n\nI hope that you and the team are well. As promised, I am writing to get back to you on a couple of things:\n\n  1.  Bilateral meeting between UNOPS & UN-Habitat EDs: I've received a green light from our Executive Director to organize a meeting between our EDs during the UNGA/Summit of the Future (he will be in NY from 20-25 September). I will be coordinating a letter from our Executive Director on this soon. While I do so, could you please confirm whether your Executive Director's start date is August 1, 2024 and if we should send the letter to Sukhjinder with you in copy?\n  2.  Workshop dates: It would be great to organize our global workshop ahead of the meeting between our EDs, so that the workshop can also inform their conversation. I would like to suggest we find a date in the week of September 9th, if possible on your side? I will be coming to Nairobi in September and would be happy to plan my travel dates around the workshop.\n  3.  WUF12: While we are still finalizing our delegation, I can confirm that we will have a senior leader present at WUF12 representing UNOPS. I will inform you of details as I receive them, and we can then agree on practicalities with regards to a signing ceremony for the partnership action plan / other representation duties. As mentioned previously, we are also discussing joint visibility opportunities for WUF12 with your Chief of Communications.\n\nI hope that the above sounds agreeable on your side. On a final note, I was hoping to get your guidance on who to contact within UN-Habitat in relation to project implementing partner agreements. Our Infrastructure and Project Management Group are in the process of revising our grant agreements, and are seeking examples of implementing partner agreements with infrastructure clauses from other UN Agencies. Any tips on this would be much appreciated.\n\nWishing you a great day!\n\nKind regards,\nKajsa\n\nOn Mon, 15 Jul 2024 at 13:58, Kajsa Johanna HARTMAN <kajsah@unops.org<mailto:kajsah@unops.org>> wrote:\nDear Erfan,\n\nThank you for the quick response, sounds great.\n\nI look forward to working on the mapping exercise with Ran. It would indeed be great to lock down a date for the workshop - please feel free to suggest a few dates that would work on your end.\n\nThank you also for highlighting the opportunity for UNOPS attendance in the One UN roundtable at WUF12. I will get back to you on this soon, as we still need to lock down some details in relation to our presence in Cairo.\n\nWishing you a great start to the week!\n\nKind regards,\nKajsa\n\n\n\n\n\n\n\n\n\nOn Mon, 15 Jul 2024 at 11:42, Erfan Ali <erfan.ali@un.org<mailto:erfan.ali@un.org>> wrote:\n\nDear Kajsa\n\nThanks for the note and it was my pleasure to discuss with you and I’m happy to work together to advance the partnership.\n\nThe only point that I may add is the possible involvement of UNOPS in the One UN round table in the World Urban Forum in Cairo in November.\n\nI copied here <mailto:ran.wang@un.org> @Ran Wang<mailto:ran.wang@un.org> who will be liaising with regional and country offices on the mapping exercise. I’ll also suggest possible dates for the workshop in September.\n\nAll the best\n\nErfan\n\n\n\nFrom: Kajsa Johanna HARTMAN <kajsah@unops.org<mailto:kajsah@unops.org>>\nSent: Monday, July 15, 2024 10:55 AM\nTo: Erfan Ali <erfan.ali@un.org<mailto:erfan.ali@un.org>>\nCc: Amrita NAUL [UNOPS] <AmritaN@unops.org<mailto:AmritaN@unops.org>>; Emilie Potvin [UNOPS] <emiliep@unops.org<mailto:emiliep@unops.org>>\nSubject: Thank you and next steps\n\n\n\nDear Erfan,\n\n\n\nMany thanks for the meeting last week, it was a pleasure to meet with you and the team.\n\n\n\nI have attached short notes of our meeting, and summarized the agreed next steps below:\n\n  1.  Expansion of programmatic collaboration\n\n     *   July-August – UNOPS and UN-Habitat to map joint programming interests (past and potential)\n\n     *   September – Conduct global workshop in Nairobi based on identified areas of interest\n\n     *   October – Finalize joint partnership action plan\n\n     *   November – Formalize action plan (potentially at 12th World Urban Forum)\n\n  1.  High-level engagement\n\n     *   September – Organize a bilateral between UNOPS and UN-Habitat Executive Directors at the Summit of the Future.\n\nPlease feel free to add if I missed anything. I would also be grateful if you could share Ran Wang's contact so that I can advance point 1 with her.\n\n\n\nThank you again, and I look forward to our continued collaboration!\n\n\n\nKind regards,\n\nKajsa\n\n\n\n\n\n\n\n--\n\nKajsa Hartman | Partnerships Specialist | Partnerships and Liaison Group | UNOPS Headquarters | Copenhagen, Denmark | www.unops.org<http://www.unops.org/>\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official>, YouTube<https://www.youtube.com/user/UNOPSofficial>.\nSubscribe to our external newsletter in English<https://mailchi.mp/8987deaa0e61/uojskr902j>, French<https://mailchi.mp/unops.org/gu0ld93is9> or Spanish<https://mailchi.mp/edf4b84d88cf/on3qnjwzkh>.\n\n[https://lh6.googleusercontent.com/OQm_SFX56ESafTg4GciG1aePxSRRpF0vMflyA3r50Aphlnr-pT5vtTYvFlB1SNi2-nKoUMZ22vlebs9fnKCNhr3t6TR606XQVec99g9GktoM39AnMiO_nULdUTnYlRRpy2PIkpS8]\n\n\n--\nKajsa Hartman | Partnerships Specialist | Partnerships and Liaison Group | UNOPS Headquarters | Copenhagen, Denmark | www.unops.org<http://www.unops.org/>\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official>, YouTube<https://www.youtube.com/user/UNOPSofficial>.\nSubscribe to our external newsletter in English<https://mailchi.mp/8987deaa0e61/uojskr902j>, French<https://mailchi.mp/unops.org/gu0ld93is9> or Spanish<https://mailchi.mp/edf4b84d88cf/on3qnjwzkh>.\n\n[https://lh6.googleusercontent.com/OQm_SFX56ESafTg4GciG1aePxSRRpF0vMflyA3r50Aphlnr-pT5vtTYvFlB1SNi2-nKoUMZ22vlebs9fnKCNhr3t6TR606XQVec99g9GktoM39AnMiO_nULdUTnYlRRpy2PIkpS8]\n\n\n--\nKajsa Hartman | Partnerships Specialist | Partnerships and Liaison Group | UNOPS Headquarters | Copenhagen, Denmark | www.unops.org<http://www.unops.org/>\n\nKeep up-to-date with UNOPS. Follow us",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008jvqMMAQ",
                        EmailAddresses = new List<string> { "emiliep@unops.org", "erfan.ali@un.org", "ran.wang@un.org", "cherrilyn.auma@un.org", "kajsah@unops.org", "amritan@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("amritan@unops.org".ToLower()) ? paoUserEmailMapping["amritan@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 31, 9, 16, 37, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000N3GvsIAF" },
                    new List<int> { 1193 },
                    new List<string> { "amritan@unops.org", "kajsah@unops.org", "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008k1ULMAY",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/UN Habitat World Cities Day",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-21").ToUniversalTime(),
                        Subject = "UNOPS/UN Habitat World Cities Day",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008k1ULMAY",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 7, 31, 9, 23, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LhVfkIAF" },
                    new List<int> { 1193 },
                    new List<string> { "kajsah@unops.org", "paolanyiramigambom@unops.org", "elenage@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008uCujMAE",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up: UK / UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-09").ToUniversalTime(),
                        Subject = "Catch-up: UK / UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008uCujMAE",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 6, 14, 24, 58, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000099IS3IAM" },
                    new List<int> { 1752 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000008ycYtMAI",
                    new UNOPSInteraction
                    {
                        Name = "ESF Consultations with UNOPS: kick-off",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-07").ToUniversalTime(),
                        Subject = "ESF Consultations with UNOPS: kick-off",
                        Description = "Participants:\n- World Bank (WB): Andy, Rajeev, Million, Gamila (LEGEN), Brigit Kuba (LEGEN), Elaine Panter (UN Team, legal background and analysis), Julia Schipper, Nathalie Munzberg (Regional Safeguards Advisor for West Africa), Jonathan Mills Lindsay (LEGEN), Gael Gregoire.\n- UNOPS: Vinesh, Eli, Megumi, Nives (UNOPS Social and Environmental Management System), Lwanda (Senior Project Manager for HSSE), Julia Schipper (SEA/SH), Tanja Chopra (Gender and Social Development Advisor, WB ESF requirements).\n\nDiscussion Points:\n1. Objective of Consultations:\n   - Nathalie (WB): The purpose of the consultations is to better understand UNOPS ESS policies to ensure smoother project implementation and cooperation. These lessons learned will be incorporated in the guidance note. \n   - Gael (WB): Emphasized that ESS compliance in MENA has been good and expressed the desire to further facilitate ESF implementation, even for non-Bank financed projects.\n\n2. Deep Dive into Policies:\n   - Jonathan Lindsay (WB): Highlighted the process with UNICEF for mutual understanding of policies and proposed a similar approach with UNOPS. This includes introductory sessions, followed by detailed reviews of UNOPS ESS management documents and processes, risk assessments, compliance mechanisms, and reporting.\n\n3. UNOPS Flexibility and Systems:\n   - Nives (UNOPS): Stated UNOPS’ openness to adapt its systems around projects, mentioning a solid track record and hybrid solutions with other donors. Emphasized the proactive improvement of guidance and the need for a flexible system due to their diverse and smaller projects.\n\n4. Current Standards and Practices:\n   - Nathalie (WB): Requested descriptions of UNOPS' practices with other financing entities to understand the overall landscape.\n   - Rajeev (WB): Explained that these consultations aim to understand agency-wide policies, not just individual projects.\n\n5. UNOPS Environmental and Social Management:\n   - Nives (UNOPS): Described the following:\n     - ED Directive and Health and Safety as prominent due to infrastructure-heavy projects.\n     - Social and Environmental Management practices, ISO 14001 compliance without formal certification.\n     - Internal screening process and its suitability for partners.\n     - Average project size and risk categorization, with most projects being medium risk.\n     - Emphasis on continuous improvement and practical guidance for field teams.\n     - Specific focus on gender, SEA/SH response, and climate change.\n     - Active networking and collaboration with other UN agencies.\n\n6. Hybrid Solutions and Collaborations:\n   - Nives (UNOPS): Mentioned hybrid solutions with KFW and GAVI, where UNOPS' systems are supplemented with additional requirements to meet partner standards.\n\n7. Process vs. Outcome Obligations:\n   - Vinesh (UNOPS): Expressed a preference for process-oriented obligations and adherence to plans rather than fixed policies.\n\nNext Steps:\n- UNOPS to share the relevant documents (policies, guidelines, questionnaires) with WB\n-Upon reviewing the documents, WB will draft the outline of discussion and additional questions to be explored during the subsequent meetings.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008ycYtMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("megumiu@unops.org".ToLower()) ? paoUserEmailMapping["megumiu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 8, 21, 52, 29, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> {  },
                    new List<string> { "megumiu@unops.org" },
                    new List<string> { "B5101" }
                ),
                new (
                    "00UQx000008yecHMAQ",
                    new UNOPSInteraction
                    {
                        Name = "Touching Base on SFA",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-30").ToUniversalTime(),
                        Subject = "Touching Base on SFA",
                        Description = "- The World Bank clarified the background of the unilateral release of SFA outputs/TA templates for all UN agencies. This action arose from incidents in December 2023 involving another UN agency, which led to noncompliance with the World Bank's Environmental and Social Framework (ESF). The new SFA must be used for all new projects under appraisal, while projects currently in implementation will not be affected.\n- UNOPS shared a recap of the negotiation status with Dominique, highlighting that there was no formal feedback from the World Bank on our comments. Therefore, the publication of the SFA without agreement came as a surprise.\n- Both parties agreed on the need to (1) find an interim solution to address the immediate project signing needs, and (2) continue negotiating the SFA to reach a mutual agreement.\n\nNext Steps:\n1. UNOPS will share a proposal for ESS language with Gamila, to be used with the 2016 ESF template as an interim solution.\n2. UNOPS will share a comparison between the 2023 feedback and the World Bank's proposed draft ESF and the one released online. The World Bank will review the wording and provide feedback.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008yecHMAQ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 8, 21, 38, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "elizabethdu@unops.org", "alistairs@unops.org", "megumiu@unops.org", "vineshw@unops.org" },
                    new List<string> { "B5101" }
                ),
                new (
                    "00UQx000008yfRtMAI",
                    new UNOPSInteraction
                    {
                        Name = "SFA catch-up with Rajeev (confirmed)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-08").ToUniversalTime(),
                        Subject = "SFA catch-up with Rajeev (confirmed)",
                        Description = "- The World Bank (WB) and UNOPS agreed to use the 2016 template with updated ESS language as an interim solution while negotiating the SFA. These exceptions will apply to the 11 projects shared by Megumi earlier in the day; however, the list may expand over time.\n- UNOPS reiterated the challenges with the latest ESF language, including compliance with WB policies and ensuring the capacity of implementing partners, which could be interpreted as focusing on \"outcome\" rather than \"process.\"\n\nNext Steps:\n- UNOPS will draft internal guidance for colleagues, including the improvised templates, to be reviewed and approved by the WB.\n- UNOPS will continue an in-depth review of the 2024 ESF and share consolidated feedback next week.\n- The WB will review the feedback on the latest draft and hold a bilateral meeting in the week of August 20th.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000008yfRtMAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 8, 22, 2, 14, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BTpVYIA1" },
                    new List<int> { 1646 },
                    new List<string> { "elizabethdu@unops.org", "megumiu@unops.org", "vineshw@unops.org" },
                    new List<string> { "B5101" }
                ),
                new (
                    "00UQx0000093xcgMAA",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up: UNOPS/SIDA",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-11").ToUniversalTime(),
                        Subject = "Catch-up: UNOPS/SIDA",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx0000093xcgMAA",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 13, 6, 51, 15, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MiKoBIAV" },
                    new List<int> { 1108 },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009FMMPMA4",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-WB Chat (SFAs)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-20").ToUniversalTime(),
                        Subject = "UNOPS-WB Chat (SFAs)",
                        Description = "Information from OPCS:\nInterim solution was supported by E&S lawyers including Victor, the E&S counsel. It was opposed by E&S Department Director.\nA few WB Country Directors have been vocal that the interim solution is necessary, leading to the meeting happening today with the World Bank Deputy General Counsel. So it remains possible that the interim solution (E&S language inserted into the old template) will be endorsed.\nUpdated clauses in the new template are the result of the WB's internal consultation, not the result of UNICEF negotiations. \nOPCS doesn’t know if any UN agencies have signed any projects using the new template; in other words a few inconsequential projects may have been signed or perhaps none.\nUNICEF is in active negotiations with WB on the new template, covering not just E&S language but also some of the other changes. There is a high-level WB-UNICEF meeting on Sept 12, which is being used as the deadline for agreement on a new template.\n\nPoints Christine made to OPCS:\nThe sudden introduction of the template has been disruptive for us.\nWe are hopeful the interim solution will be endorsed, and in the meantime we are trying to find creative solutions for urgent projects.\nEli has been leading cross-functional internal review of the new template, noting that the changes go beyond ESS.\nI’m concerned that the inflexibility of ESF requirements for FCV may lead to some UN agencies not being able to accept the high legal risks of some projects in future.\nProgress on the UNICEF negotiations would also be helpful for us, as UNOPS leadership has a general preference to take actions which are consistent with other UN agencies. Requested to be kept informed.\nAsked that guidance to TTLs on the new template also reference the need to ensure that project budgets reflect the additional E&S responsibilities taken on by UN agencies. \nRelated, I said that we often receive E&S documents very late in the process, sometimes just before project signature is expected. Requested that UNOPS receive ESCPs and related documents earlier in the process, so that we can ensure thorough internal review and appropriate budgets.\n\nOn the last two points, Rajeev agreed and took note of these requests. However he said those instructions would need to come from the E&S Department, so he will pass along the request.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009FMMPMA4",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("megumiu@unops.org".ToLower()) ? paoUserEmailMapping["megumiu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 20, 16, 38, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "megumiu@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009GhbiMAC",
                    new UNOPSInteraction
                    {
                        Name = "Possible Collaboration at World Health Summit (Novo/UNOPS/Cities Alliance)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-22").ToUniversalTime(),
                        Subject = "Possible Collaboration at World Health Summit (Novo/UNOPS/Cities Alliance)",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009GhbiMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 21, 11, 42, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000NtUl2IAF" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org", "louisel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009HzSuMAK",
                    new UNOPSInteraction
                    {
                        Name = "Venues for CEB",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-22").ToUniversalTime(),
                        Subject = "Venues for CEB",
                        Description = "Use this link: <br><br><p>..............................<wbr />..............................<wbr />..............................<wbr />..............................<wbr />.................<b><u></u><u></u></b></p><p><a></a><a href=\"https://meet.um.dk/josibs/272M7C86\" target=\"_blank\">Deltag i Skype-møde </a>  <a> </a>  <a> </a><u></u><u></u></p><p>Problemer med at deltage? <u><a href=\"https://meet.um.dk/josibs/272M7C86?sl=1\" target=\"_blank\">Prøv Skype Web App</a></u><u></u><u></u></p><p>Deltag via telefon<u></u><u></u></p><p> +45 33 92 09 99,,7793494# (Denmark)                               <wbr />               Dansk (Danmark)  <u></u><u></u></p><p> +45 33 92 09 98,,7793494# (Denmark)                               <wbr />               Engelsk (Storbritannien)     <u></u><u></u></p><p><u><a href=\"https://dialin.um.dk/?id=7793494\" target=\"_blank\">Find et lokalt nummer </a></u> <u></u><u></u></p><p><u></u> <u></u></p><p>Møde-id: 7793494<u></u><u></u></p><p> <a href=\"https://dialin.um.dk/\" target=\"_blank\">Har du glemt pinkoden til at ringe ind? </a> |<a href=\"https://o15.officeredir.microsoft.com/r/rlidLync15?clid=1030&amp;p1=5&amp;p2=2009\" target=\"_blank\">Hjælp </a>   <u></u><u></u></p><p><u></u> </p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009HzSuMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("raady@unops.org".ToLower()) ? paoUserEmailMapping["raady@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 22, 8, 23, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "raady@unops.org", "paulom@unops.org", "cilliano@unops.org", "asbjornb@unops.org", "kerriet@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009MfYnMAK",
                    new UNOPSInteraction
                    {
                        Name = "Introductory meeting with Allm Inc.",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-07-24").ToUniversalTime(),
                        Subject = "Introductory meeting with Allm Inc.",
                        Description = null,
                        Location = "UNOPS TLO",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009MfYnMAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 26, 6, 16, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Oi2uoIAB" },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009MhabMAC",
                    new UNOPSInteraction
                    {
                        Name = "Checking venue for UNOPS Parliamentary Association meeting on 11 June",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-26").ToUniversalTime(),
                        Subject = "Checking venue for UNOPS Parliamentary Association meeting on 11 June",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009MhabMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 26, 6, 30, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OiGCvIAN" },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009MiA6MAK",
                    new UNOPSInteraction
                    {
                        Name = "Introductory meeting on UN procurement",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-26").ToUniversalTime(),
                        Subject = "Introductory meeting on UN procurement",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009MiA6MAK",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 26, 6, 57, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009MqdtMAC",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Yamaha",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-26").ToUniversalTime(),
                        Subject = "Meeting with Yamaha",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009MqdtMAC",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 26, 7, 55, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OiOqOIAV" },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009OPZlMAO",
                    new UNOPSInteraction
                    {
                        Name = "Meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2022-12-01").ToUniversalTime(),
                        Subject = "Meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009OPZlMAO",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("isabelaf@unops.org".ToLower()) ? paoUserEmailMapping["isabelaf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 27, 7, 55, 15, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MwmFjIAJ" },
                    new List<int> { 1442 },
                    new List<string> { "isabelaf@unops.org" },
                    new List<string> { "B0064" }
                ),
                new (
                    "00UQx000009Qj9CMAS",
                    new UNOPSInteraction
                    {
                        Name = "Bilateral: USG Jorge Moreira da Silva, ED of UNOPS & USG Anacláudia Rossbach, ED of UN-Habitat",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-23").ToUniversalTime(),
                        Subject = "Bilateral: USG Jorge Moreira da Silva, ED of UNOPS & USG Anacláudia Rossbach, ED of UN-Habitat",
                        Description = "*   UN Office of Project Services (UNOPS) and UN-Habitat partnership, WUF 12, etc",
                        Location = "UN Habitat NYO, 2 UN Plaza (DC2), East 44th St., bet. 1st/2nd Aves (across UNICEF /next to Millenium Hilton), 9th Flr",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009Qj9CMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 28, 9, 16, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OrK1ZIAV" },
                    new List<int> { 1193 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009R20YMAS",
                    new UNOPSInteraction
                    {
                        Name = "WCD joint comms campaign",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-08-29").ToUniversalTime(),
                        Subject = "WCD joint comms campaign",
                        Description = "Dear all,\n\nAs discussed, scheduling some time to update each other and define next steps for the WCD campaign.\n\nPlease let me know if this slot does not work.\n\nLooking forward,\n\nY",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009R20YMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("juyoungl@unops.org".ToLower()) ? paoUserEmailMapping["juyoungl@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 28, 9, 31, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LhVfkIAF" },
                    new List<int> { 1193 },
                    new List<string> { "juyoungl@unops.org", "kajsah@unops.org", "yamilafcm@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009V0bGMAS",
                    new UNOPSInteraction
                    {
                        Name = "Catch up",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-12").ToUniversalTime(),
                        Subject = "Catch up",
                        Description = null,
                        Location = "Original coffee store kongensgade",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009V0bGMAS",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 8, 30, 10, 27, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF", "003Qx00000Up4JdIAJ" },
                    new List<int> { 1089, 1086 },
                    new List<string> { "asbjornb@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009aekvMAA",
                    new UNOPSInteraction
                    {
                        Name = "Frokost",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-03").ToUniversalTime(),
                        Subject = "Frokost",
                        Description = "Kære Asbjørn,\n\nKunne dette tidsrum evt. fungere for dig? Alternativt en anden dag samme eller forudgående uge? Fra UMs side deltager Tue og jeg.\n\nBedste hilsner,\nRikke\n_____________________________________________\nFrom: Asbjorn BRINK <asbjornb@unops.org<mailto:asbjornb@unops.org>>\nSent: 14 August 2024 12:38\nTo: Rikke Enggaard Olsen <rikoln@um.dk<mailto:rikoln@um.dk>>\nCc: Naimo HASSAN HIRSI <naimoh@unops.org<mailto:naimoh@unops.org>>; Tue Kristoffer Westhoff <tuewes@um.dk<mailto:tuewes@um.dk>>; Julia Winding <julwin@um.dk<mailto:julwin@um.dk>>\nSubject: Re: Executive Board, Second Regular Session, August 26-29\n\n[CAUTION - EXTERNAL EMAIL] This email was sent from outside the MFA organisation. DO NOT reply, click on links, or open attachments unless you have verified the sender and know the content is safe.\nKære Rikke\n\nGlimrende. Hvis det kunne blive tirsdag næste uge ville det passe rigtig godt. 😅\n\nMvh\nAsbjørn\n\nOn Wed, 14 Aug 2024 at 12.36, Rikke Enggaard Olsen <rikoln@um.dk<mailto:rikoln@um.dk>> wrote:\nKære Asbjørn,\n\nVil med forsinkelse blot kvittere for det meget nyttige overblik over hvad der forventes berørt på forestående bestyrelsesmøde.\n\nJeg sender en indkaldelse til en frokost med dig og Tue, hvor vi i løse rammer kan tale om DK-UNOPS partnerskabet, UNOPS’ reviderede strategi og igangværende CRP-proces, Jorges UNCT lederskab osv.\n\nBedste hilsner,\nRikke\n\nFrom: Asbjorn BRINK <asbjornb@unops.org<mailto:asbjornb@unops.org>>\nSent: 02 August 2024 09:11\nTo: Julia Winding <julwin@um.dk<mailto:julwin@um.dk>>\nCc: Naimo HASSAN HIRSI <naimoh@unops.org<mailto:naimoh@unops.org>>; Rikke Enggaard Olsen <rikoln@um.dk<mailto:rikoln@um.dk>>; Tue Kristoffer Westhoff <tuewes@um.dk<mailto:tuewes@um.dk>>\nSubject: Re: Executive Board, Second Regular Session, August 26-29\n\n[CAUTION - EXTERNAL EMAIL] This email was sent from outside the MFA organisation. DO NOT reply, click on links, or open attachments unless you have verified the sender and know the content is safe.\nGlimrende!\n\nVelkommen til, Tue. Ser frem til samarbejdet.\n\nBh\nAsbjørn\n\nOn Thu, 1 Aug 2024 at 12.45, Julia Winding <julwin@um.dk<mailto:julwin@um.dk>> wrote:\nMange tak, Asbjørn. Looper lige vores nye teamleder Tue ind. Han starter 8. august. Ville være fint at tage et møde, når han har sat sig forud for næste session.\nVi vender lige tilbage om timing.\nBh Julia\n\nFra: Asbjorn BRINK <asbjornb@unops.org<mailto:asbjornb@unops.org>>\nSendt: 30. juli 2024 17:14\nTil: Rikke Enggaard Olsen <rikoln@um.dk<mailto:rikoln@um.dk>>; Julia Winding <julwin@um.dk<mailto:julwin@um.dk>>\nCc: Naimo HASSAN HIRSI <naimoh@unops.org<mailto:naimoh@unops.org>>\nEmne: Executive Board, Second Regular Session, August 26-29\n\n[CAUTION - EXTERNAL EMAIL] This email was sent from outside the MFA organisation. DO NOT reply, click on links, or open attachments unless you have verified the sender and know the content is safe.\nDear Rikke and Julia,\n\nI hope this email finds you well - somewhere warm and sunny!\n\nAs usual, I wanted to provide you with a brief and informal overview of the UNOPS relevant topics to be discussed at the upcoming Executive Board session on August 26-29.\n\nYou might already have noticed that all the relevant documents as well as the draft agenda<https://urldefense.com/v3/__https:/undocs.org/en/DP/2024/l.3__;!!Prj2KelAwpywYnARIQsmmHCn!KOs8RWLenpWm0n4bPD7PrB6I5Szq45c1cpnUT2afnc67yoMXFGOAhhA3kCqH3J2IZL0ZT-UD-Iz0XA$> have been uploaded to the UNOPS website and can be found here<https://urldefense.com/v3/__https:/www.unops.org/about/governance/executive-board/executive-board-documents?documentType=documents-for-sessions&year=2024__;!!Prj2KelAwpywYnARIQsmmHCn!KOs8RWLenpWm0n4bPD7PrB6I5Szq45c1cpnUT2afnc67yoMXFGOAhhA3kCqH3J2IZL0ZT-VUSxuqfw$>.\n\nOn substance the topics relevant specifically to UNOPS relates to the following:\n1.      A draft terms of reference for the review of UNOPS implementation of the Comprehensive Response Plan<https://urldefense.com/v3/__https:/undocs.org/en/DP/OPS/2024/10__;!!Prj2KelAwpywYnARIQsmmHCn!KOs8RWLenpWm0n4bPD7PrB6I5Szq45c1cpnUT2afnc67yoMXFGOAhhA3kCqH3J2IZL0ZT-XAKKgtsA$>\n2.      An information note on UNOPS strategy for the implementation of the Process Innovation and Digitalisation Programme<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/second-regular-session/item-7-unops-segment/en/UNOPS-Process-Innovation-and-Digitalization-Programme-Roadmap-July-2024.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!KOs8RWLenpWm0n4bPD7PrB6I5Szq45c1cpnUT2afnc67yoMXFGOAhhA3kCqH3J2IZL0ZT-U8lENCYw$>\n3.      Information note on the estimated amount remaining of undisbursed excess reserve funds<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/second-regular-session/item-7-unops-segment/en/Information-Note-Update-on-approximate-amount-remaining-of-undisbursed-funds.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!KOs8RWLenpWm0n4bPD7PrB6I5Szq45c1cpnUT2afnc67yoMXFGOAhhA3kCqH3J2IZL0ZT-W2dSeVtg$>\n4.      The annual report on the procurement activities of the UN system in 2023<https://urldefense.com/v3/__https:/undocs.org/en/DP/OPS/2024/9__;!!Prj2KelAwpywYnARIQsmmHCn!KOs8RWLenpWm0n4bPD7PrB6I5Szq45c1cpnUT2afnc67yoMXFGOAhhA3kCqH3J2IZL0ZT-V9_Wo5dg$> + annex<https://urldefense.com/v3/__https:/content.unops.org/documents/libraries/executive-board/documents-for-sessions/2024/second-regular-session/item-7-unops-segment/en/DP-OPS-2024-9-Annex-1_EN.pdf__;!!Prj2KelAwpywYnARIQsmmHCn!KOs8RWLenpWm0n4bPD7PrB6I5Szq45c1cpnUT2afnc67yoMXFGOAhhA3kCqH3J2IZL0ZT-Uf1fwlEQ$>\nAd 1) - ToR for review of implementation of the Comprehensive Response Plan\nAn informal session about this topic is scheduled for 19 August.\n\nUpon request from the EB, UNOPS' Internal Audit and Investigation Group (IAIG) has prepared a draft ToR for the upcoming second review of UNOPS' implementation of the Comprehensive Response Plan (CPR). This document was also presented to the EB at the June EB session.\n\nThis second review will seek to validate/dismiss that UNOPS has in fact implemented the remaining action points in the CPR and thereby adhered to the recommendations by KPMG following the S3i independent review. The review is meant to start in 2025 when all but one recommendation will have been concluded (the digitisation programme (PID) - which runs until 2025).\n\nAd 2) Implementation of the Process Innovation and Digitalisation Programme\nAn informal session about this topic is scheduled for 19 August.\n\nUpon request from the EB, UNOPS has provided an extensive information note with a roadmap for the PID's scope, progress, milestones, budget, governance and risk management, KPIs and stakeholder engagement.\n\nThe progress in implementation - as well as challenges - will be reported in the monthly briefings to the EB until the first EB session in January / February 2025.\n\nAd 3) Estimated remaining excessive reserve funds\nAn informal session about this topic is scheduled for 19 August.\n\nUNOPS estimates that out of the 123.8M USD, only 1.5M will be undisbursed by the end of 2024, which is the deadline provided by the EB. As per June 2024, only 8.5M USD remains in the reserve, which is focused on 10 government partners.\n\nThe EB will decide on the use of the remaining funds at the first session in 2025.\n\nAd 4) Estimated remaining excessive reserve funds\nAn informal session about this topic is scheduled for 23 August.\n\nThe annual procurement report shows that the UN procured goods and services worth almost 25B in 2023 - a decrease by 15.7%. 5B was procured from least developed countries.\n\nScheduled Informal Sessions\n•  19 August, 10 am - 1 pm:\no    ToR for review on UNOPS implementation of the CRP\no    Roadmap for the PID programme\no    Remaining funds in excess reserve\n•  23 August 10 am - 12 pm\no    Procurement report\n\nI hope this gives you a somewhat overview of the topics to be discussed at the August EB session.\n\nI also hope that you might be available during next week for an informal chat. where we could go through the topics, clarify potential questions bilaterally and get your initial take on the topics.\n\nIf so - please do let me know when it might suit you to mee",
                        Location = "Kanalhuset",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009aekvMAA",
                        EmailAddresses = new List<string> { "tuewes@um.dk", "julwin@um.dk", "rikoln@um.dk", "naimoh@unops.org", "asbjornb@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 3, 13, 12, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OKqawIAD" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009cZSdMAM",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS / AEF",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-04").ToUniversalTime(),
                        Subject = "UNOPS / AEF",
                        Description = "Hi Celia,\nSending already the Outlook invite so it is locked in our agenda, and happy to welcome you at our office.\nWill come back to you early next week on the additional information re our programming,\nSee you soon and have a great weekend,\nBest\nHoly",
                        Location = "AEF Office, Square de Meeus 5-6",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009cZSdMAM",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("celiaafricak@unops.org".ToLower()) ? paoUserEmailMapping["celiaafricak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 4, 12, 42, 29, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000PKhYhIAL" },
                    new List<int> {  },
                    new List<string> { "celiaafricak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009cnH3MAI",
                    new UNOPSInteraction
                    {
                        Name = "Réception Technique de l'Accord 21/00/01/09/80/2023/00003 pour l'acquisition de logistique roulante",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-23").ToUniversalTime(),
                        Subject = "Réception Technique de l'Accord 21/00/01/09/80/2023/00003 pour l'acquisition de logistique roulante",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009cnH3MAI",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("abdoulazizs@unops.org".ToLower()) ? paoUserEmailMapping["abdoulazizs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 4, 13, 31, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "abdoulazizs@unops.org", "maalecarinem@unops.org", "genevievel@unops.org" },
                    new List<string> { "B5322" }
                ),
                new (
                    "00UQx000009jLMTMA2",
                    new UNOPSInteraction
                    {
                        Name = "Briefing on UNOPS' work in Gaza",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-18").ToUniversalTime(),
                        Subject = "Briefing on UNOPS' work in Gaza",
                        Description = "Following up on the conversation between Kirstine Damkjær and Ib Petersen, please find here an invitation for an online briefing by UNOPS' Regional Director for the Middle East Region, Ms. Bana Kaloti on the situation in Gaza as well and UNOPS' work in the regions. \n\nPlease feel free to extend the invitation to other relevant colleagues at the Danish MFA. \n\nBest regards,\nAsbjørn",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009jLMTMA2",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 9, 9, 44, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org", "kirstined@unops.org", "marijab@unops.org", "jakobt@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009nZvWMAU",
                    new UNOPSInteraction
                    {
                        Name = "Revue Proposition Phase 2 Projet d'électrification des centres de santé en Côte d'Ivoire",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-11").ToUniversalTime(),
                        Subject = "Revue Proposition Phase 2 Projet d'électrification des centres de santé en Côte d'Ivoire",
                        Description = "Bonjour Madame DIENG. L'urgence du dossier nous oblige à nous rendre disponible ce mercredi pour avancer. nous attendons l'heure et le lien.\n\nCordialement\n\nTIA Yao G. Economiste de la santé\nChargé de Projet,  UCPS-Banque Mondiale\n\nAbidjan, Côte d’Ivoire\n\nMobile 1 :+225 07 08 77 37 70\n\nMobile 2 :+225 05 54 51 11 00\n\n\n\n\nLe vendredi 6 septembre 2024 à 11:34:50 UTC, Agnon N'DRI <nagnon1961@gmail.com> a écrit :\n\n\nBonjour Madame DIENG\nJe me fais disponible le mercredi 11 septembre 2024, vous voudrez générer le lien et préciser l'heure en tenant compte de...",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009nZvWMAU",
                        EmailAddresses = new List<string> { "nagnon1961@gmail.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("fatoufn@unops.org".ToLower()) ? paoUserEmailMapping["fatoufn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 11, 10, 50, 3, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Oy00vIAB" },
                    new List<int> { 1329 },
                    new List<string> { "fatoufn@unops.org" },
                    new List<string> { "B5305" }
                ),
                new (
                    "00UQx000009qOnRMAU",
                    new UNOPSInteraction
                    {
                        Name = "Mtg with UNOPS Kajsa [In-person]",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-12").ToUniversalTime(),
                        Subject = "Mtg with UNOPS Kajsa [In-person]",
                        Description = "Dear Kajsa,\n\nThanks for visiting Nairobi and co-organizing the workshop.\n\nThis is upon your request and set a meeting with Erfan to finalize the workshop and discuss about our EDs' meeting in Summit of Future.\n\nBest regards,\nRan",
                        Location = "CoS's Office",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009qOnRMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 12, 14, 19, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000N3GvsIAF" },
                    new List<int> { 1193 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009xJoTMAU",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Danish MFA HQ Africa Division",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-04").ToUniversalTime(),
                        Subject = "Meeting with Danish MFA HQ Africa Division",
                        Description = "Meeting with Director for the Danish MFA HQ Africa Division, Mr. Ketil Karlsen and Deputy Director for Multilateral Cooperation, Ms. Julia Winding. Members of their respective staff might also attend. <br><br>Attendees from UNOPs (tbc): <br><ul><li>Dalila Goncalves</li><li>Asbjørn Brink</li><li>Naimo Hassan Hirsi</li><li>Amy Niang</li></ul>",
                        Location = "Ministry of Foreign Affairs of Denmark, Asiatisk Pl. 2, 1448 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009xJoTMAU",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("amyn@unops.org".ToLower()) ? paoUserEmailMapping["amyn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 17, 9, 19, 29, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "amyn@unops.org", "naimoh@unops.org", "dalilag@unops.org", "sarahdg@unops.org", "asbjornb@unops.org", "nirangad@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx000009zanlMAA",
                    new UNOPSInteraction
                    {
                        Name = "Briefing on UNOPS' work in Gaza  - teams link",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-18").ToUniversalTime(),
                        Subject = "Briefing on UNOPS' work in Gaza  - teams link",
                        Description = "Dear all.\n\nAs per Asbjørn’s mail before please find below the new link for our call at 11.\n\nBest Lea\n________________________________________________________________________________\nMicrosoft Teams Har du brug for hjælp?<https://aka.ms/JoinTeamsMeeting?omkt=da-DK>\nDeltag i mødet nu<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NWUwOTg1ZjAtNGZhOC00ZjI5LTgxNTktODU4ZGNhNDViNGZi%40thread.v2/0?context=%7b%22Tid%22%3a%2248dc02d0-bd56-411d-b7c5-a814743bafc6%22%2c%22Oid%22%3a%22af76387b-4889-4aef-beea-769f290e709a%22%7d>\nMøde-id: 324 146 289 499\nAdgangskode: 96gouC\n________________________________\nTilmeld dig på en enhed til videomøder\nLejernøgle: teams@meet.um.dk<mailto:teams@meet.um.dk>\nVideo-id: 125 634 538 5\nFlere oplysninger<https://pexip.me/teams/meet.um.dk/1256345385>\nFor arrangører: Mødeindstillinger<https://teams.microsoft.com/meetingOptions/?organizerId=af76387b-4889-4aef-beea-769f290e709a&tenantId=48dc02d0-bd56-411d-b7c5-a814743bafc6&threadId=19_meeting_NWUwOTg1ZjAtNGZhOC00ZjI5LTgxNTktODU4ZGNhNDViNGZi@thread.v2&messageId=0&language=da-DK>\n________________________________________________________________________________",
                        Location = "Microsoft Teams-møde",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx000009zanlMAA",
                        EmailAddresses = new List<string> { "teams@meet.um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("marijab@unops.org".ToLower()) ? paoUserEmailMapping["marijab@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 18, 9, 22, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000QFn9PIAT" },
                    new List<int> { 1086 },
                    new List<string> { "marijab@unops.org", "asbjornb@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000A7dqTMAR",
                    new UNOPSInteraction
                    {
                        Name = "Møde vedr. inddragelse af kongehuset i CEB møde",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-23").ToUniversalTime(),
                        Subject = "Møde vedr. inddragelse af kongehuset i CEB møde",
                        Description = ".........................................................................................................................................\nDeltag i Skype-møde <https://meet.um.dk/josibs/C5NTPBGS>\nProblemer med at deltage? Prøv Skype Web App <https://meet.um.dk/josibs/C5NTPBGS?sl=1>\nDeltag via telefon\n +45 33 92 09 99,,3482838# (Denmark)                                              Dansk (Danmark)\n +45 33 92 09 98,,3482838# (Denmark)                                              Engelsk (Storbritannien)\nFind et lokalt nummer <https://dialin.um.dk?id=3482838>\n\nMøde-id: 3482838\n Har du glemt pinkoden til at ringe ind? <https://dialin.um.dk>  |Hjælp <https://o15.officeredir.microsoft.com/r/rlidLync15?clid=1030&p1=5&p2=2009>\n\n[!OC([1030])!]\n.........................................................................................................................................\n\n\n_____________________________________________\nFra: Asbjorn BRINK <asbjornb@unops.org>\nSendt: 19. september 2024 10:58\nTil: Josephine Helena Ibsen <josibs@um.dk>\nCc: Astrid Ruge <astrug@um.dk>; noa.valentin.katz.sogaard@undp.org; Philip Meisner <phimei@um.dk>\nEmne: Re: Møde vedr. inddragelse af kongehuset i CEB møde\n\n[CAUTION - EXTERNAL EMAIL] This email was sent from outside the MFA organisation. DO NOT reply, click on links, or open attachments unless you have verified the sender and know the content is safe.\nJeps!\n\nHåber også Noa kan. Han er desværre uden for rækkevidde i dag. Men lad os bare sige mandag kl 10.\n\nVil du sende Teams invitation?\n\nPå forhånd mange tak!\n\nBh\nAsbjørn\n\nOn Thu, 19 Sep 2024 at 10.55, Josephine Helena Ibsen <josibs@um.dk<mailto:josibs@um.dk>> wrote:\nKan det blive kl. 10? Jeg har et andet møde 11:30\n\nHilsen Josephine\n\nFra: Asbjorn BRINK <asbjornb@unops.org<mailto:asbjornb@unops.org>>\nSendt: 19. september 2024 10:51\nTil: Josephine Helena Ibsen <josibs@um.dk<mailto:josibs@um.dk>>\nCc: Astrid Ruge <astrug@um.dk<mailto:astrug@um.dk>>; noa.valentin.katz.sogaard@undp.org<mailto:noa.valentin.katz.sogaard@undp.org>; Philip Meisner <phimei@um.dk<mailto:phimei@um.dk>>\nEmne: Re: Møde vedr. inddragelse af kongehuset i CEB møde\n\n[CAUTION - EXTERNAL EMAIL] This email was sent from outside the MFA organisation. DO NOT reply, click on links, or open attachments unless you have verified the sender and know the content is safe.\nHvad siger du til mandag kl. 11?\n\nOn Thu, 19 Sept 2024 at 10:42, Josephine Helena Ibsen <josibs@um.dk<mailto:josibs@um.dk>> wrote:\nKære Asbjørn\n\nSå skal det være mandag morgen, for jeg tager på ferie på tirsdag.\n\nLad mig vide hvilket tidspunkt som passer jer bedst.\n\nHilsen Josephine\n\nFra: Asbjorn BRINK <asbjornb@unops.org<mailto:asbjornb@unops.org>>\nSendt: 18. september 2024 18:46\nTil: Josephine Helena Ibsen <josibs@um.dk<mailto:josibs@um.dk>>; Astrid Ruge <astrug@um.dk<mailto:astrug@um.dk>>; noa.valentin.katz.sogaard@undp.org<mailto:noa.valentin.katz.sogaard@undp.org>; Philip Meisner <phimei@um.dk<mailto:phimei@um.dk>>\nEmne: Møde vedr. inddragelse af kongehuset i CEB møde\n\n[CAUTION - EXTERNAL EMAIL] This email was sent from outside the MFA organisation. DO NOT reply, click on links, or open attachments unless you have verified the sender and know the content is safe.\nKære Astrid\n\nVi er desværre blevet forhindret i at deltage i mødet i morgen. Kan vi muligvis finde et tidspunkt der passer jer i starten af næste uge?\n\nJeg beklager ulejligheden!\n\nMvh\nAsbjørn\n\nAsbjørn Brink | Head of Northern Europe Liaison Office | Partnerships and Liaison Group | UNOPS HQ | Copenhagen, Denmark | Mob: +45 40 80 36 54 | www.unops.org<https://urldefense.com/v3/__https:/www.unops.org/english/Pages/Home.aspx__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QXfMmiWfg$>\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://urldefense.com/v3/__https:/www.facebook.com/unops.org/__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QX88K8cgA$>, LinkedIn<https://urldefense.com/v3/__https:/www.linkedin.com/company/unops__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QVizVe6pw$>, Twitter<https://urldefense.com/v3/__https:/twitter.com/unops__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QX2iulTNA$>, Instagram<https://urldefense.com/v3/__https:/www.instagram.com/unops_official/?hl=sv__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QWtN2a5ew$>, YouTube<https://urldefense.com/v3/__https:/www.youtube.com/user/UNOPSofficial__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QXgV0BOcg$>\nSubscribe to our external newsletter in English<https://urldefense.com/v3/__https:/mailchi.mp/8987deaa0e61/uojskr902j__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QWKNJhRNw$>, French<https://urldefense.com/v3/__https:/mailchi.mp/unops.org/gu0ld93is9__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QVmeWKs8g$> or Spanish<https://urldefense.com/v3/__https:/mailchi.mp/edf4b84d88cf/on3qnjwzkh__;!!Prj2KelAwpywYnARIQsmmHCn!I1Vu8ZHKnZ0ct2VsESA1GMiBTnSxV-H5yYhYe4TswLSAreEDhzk0_w4ZVaj_BD5gXadF8QWMDwFOzQ$>.\n\n[https://lh7-us.googleusercontent.com/nPxpbWSDHa0CL7JyQZX-4y85Mxn4y5gEFRdjC7t_CF_ptBC0_9XBSY2ZptDg9-nyWZQzuWxNqh-iqm0hCjymAwDk6KVXl7CvNjzi3PTAkCxhXNXzCWRmkUFCpULjbizB2mni1nMhp9Bzvs4_VZ_uU2I]",
                        Location = "Skype-møde",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000A7dqTMAR",
                        EmailAddresses = new List<string> { "phimei@um.dk", "astrug@um.dk", "noa.valentin.katz.sogaard@undp.org", "josibs@um.dk", "asbjornb@unops.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 23, 6, 58, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000ALSy1MAH",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-UNEP partnership",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-24").ToUniversalTime(),
                        Subject = "UNOPS-UNEP partnership",
                        Description = "Dear Hemini,\n\nPlease find here an invite to follow up on our conversation on the UNOPS-UNEP partnership that we started in Nairobi.\n\nLooking forward to our discussion!\n\nKind regards,\nKajsa",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ALSy1MAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 9, 30, 15, 11, 51, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000RX5hKIAT" },
                    new List<int> { 1192 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000AR5yoMAD",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Finnish Minister for Development",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-03").ToUniversalTime(),
                        Subject = "Meeting with Finnish Minister for Development",
                        Description = "<p>Minister Ville Tavio<u></u><u></u></p><p>Diplomatic advisor Laura Quist<u></u><u></u></p><p>Director Katja Kalamäki<u></u><u></u></p><p>Desk officer Sara Kärnä<u></u><u></u></p><p>Ambassador Harri Kämäräinen</p><p>Political advisor Mari Lankinen</p>",
                        Location = "Executive Office Meeting Room",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000AR5yoMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("emiliep@unops.org".ToLower()) ? paoUserEmailMapping["emiliep@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 3, 9, 43, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1087 },
                    new List<string> { "emiliep@unops.org", "kirstined@unops.org", "asbjornb@unops.org", "waingchita@unops.org", "jakobt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000ASSoPMAX",
                    new UNOPSInteraction
                    {
                        Name = "IDB Board decision to extend validity of our COVID-19 templates",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "IDB Board decision to extend validity of our COVID-19 templates",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ASSoPMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("isabelaf@unops.org".ToLower()) ? paoUserEmailMapping["isabelaf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 3, 19, 12, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MveYrIAJ" },
                    new List<int> { 1442 },
                    new List<string> { "isabelaf@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000ATITdMAP",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Danish MFA Africa Division Director, Mr. Ketil Karlsen",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-04").ToUniversalTime(),
                        Subject = "Meeting Danish MFA Africa Division Director, Mr. Ketil Karlsen",
                        Description = "Please find the <a href=\"https://drive.google.com/drive/folders/1cO7LNvmtRoevz1P-2xfPDoShbxGx0xZz\" class=\"pastedDriveLink-0\"><u>Briefing package</u></a>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ATITdMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("amyn@unops.org".ToLower()) ? paoUserEmailMapping["amyn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 4, 8, 26, 48, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "amyn@unops.org", "asbjornb@unops.org", "dalilag@unops.org", "sarahdg@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000AWx5fMAD",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between AFR RD and Norwegian Ambassador to Kenya",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-01").ToUniversalTime(),
                        Subject = "Meeting between AFR RD and Norwegian Ambassador to Kenya",
                        Description = null,
                        Location = "Nairobi, Kenya",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000AWx5fMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("paolanyiramigambom@unops.org".ToLower()) ? paoUserEmailMapping["paolanyiramigambom@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 7, 11, 22, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1136 },
                    new List<string> { "paolanyiramigambom@unops.org", "dalilag@unops.org", "sarahdg@unops.org" },
                    new List<string> { "B0053" }
                ),
                new (
                    "00UQx00000AX4v8MAD",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between AFR RD, KEMCO Director and Danish Ambassador to Kenya",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-24").ToUniversalTime(),
                        Subject = "Meeting between AFR RD, KEMCO Director and Danish Ambassador to Kenya",
                        Description = null,
                        Location = "Nairobi, Kenya",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000AX4v8MAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("paolanyiramigambom@unops.org".ToLower()) ? paoUserEmailMapping["paolanyiramigambom@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 7, 11, 26, 50, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1123 },
                    new List<string> { "paolanyiramigambom@unops.org", "rainerf@unops.org", "dalilag@unops.org", "sarahdg@unops.org" },
                    new List<string> { "B0053" }
                ),
                new (
                    "00UQx00000AYeJpMAL",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/Norad - Ukraine",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-08").ToUniversalTime(),
                        Subject = "UNOPS/Norad - Ukraine",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MWNjNjliMGUtMmQ3ZS00ZWVmLTg5ZDctODg3MzIwZmE4YzU3%40thread.v2/0?context=%7b%22Tid%22%3a%223977e38c-aa4b-439e-80ea-421a4d4ef891%22%2c%22Oid%22%3a%225942856a-0130-4798-9170-26ee99ce7f8b%22%7d>\nMeeting ID: 352 626 921 425\nPasscode: r5XGga\n________________________________\nDial in by phone\n+47 21 40 20 33,,161095966#<tel:+4721402033,,161095966> Norway, Oslo\nFind a local number<https://dialin.teams.microsoft.com/6608c65b-dfb5-44b7-a633-019aacd64c20?id=161095966>\nPhone conference ID: 161 095 966#\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=5942856a-0130-4798-9170-26ee99ce7f8b&tenantId=3977e38c-aa4b-439e-80ea-421a4d4ef891&threadId=19_meeting_MWNjNjliMGUtMmQ3ZS00ZWVmLTg5ZDctODg3MzIwZmE4YzU3@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000AYeJpMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arunn@unops.org".ToLower()) ? paoUserEmailMapping["arunn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 8, 6, 19, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1101 },
                    new List<string> { "arunn@unops.org", "asbjornb@unops.org", "simonp@unops.org", "eleneag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000AdYQhMAN",
                    new UNOPSInteraction
                    {
                        Name = "Kirstine Damkjær, UNOPS og Lotte Machon, Udenrigsministeriet",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-11").ToUniversalTime(),
                        Subject = "Kirstine Damkjær, UNOPS og Lotte Machon, Udenrigsministeriet",
                        Description = null,
                        Location = "Asiatisk Plads - Lottes kontor",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000AdYQhMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 10, 10, 57, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx000009fefVIAQ" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000ApFnHMAV",
                    new UNOPSInteraction
                    {
                        Name = "Meet Anders Tang Friborg (Permanent Under-Secretary of State) at the PM Office, including travel time (meeting is at 15:30)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-21").ToUniversalTime(),
                        Subject = "Meet Anders Tang Friborg (Permanent Under-Secretary of State) at the PM Office, including travel time (meeting is at 15:30)",
                        Description = null,
                        Location = "Prime Minister's Office",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ApFnHMAV",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 17, 11, 16, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1123 },
                    new List<string> { "asbjornb@unops.org", "jorge.moreiradasilva@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000ApLizMAF",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between Jorge Moreira da Silva (UNOPS) and Lotte Machon (Danish MFA)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-21").ToUniversalTime(),
                        Subject = "Meeting between Jorge Moreira da Silva (UNOPS) and Lotte Machon (Danish MFA)",
                        Description = null,
                        Location = "Asiatisk Plads 2, 1448 København K - Lottes office",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ApLizMAF",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 17, 11, 15, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx000009fefVIAQ" },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org", "kerriet@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Azu4MMAR",
                    new UNOPSInteraction
                    {
                        Name = "Negotiation of PFA & Contract Template UNOPS - IDB",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-07-19").ToUniversalTime(),
                        Subject = "Negotiation of PFA & Contract Template UNOPS - IDB",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZDIyODU5OWItMjU1Ni00ZDAwLWE5NDYtY2E5NmRlNDNiZWY5%40thread.v2/0?context=%7b%22Tid%22%3a%229dfb1a05-5f1d-449a-8960-62abcb479e7d%22%2c%22Oid%22%3a%2223930f09-d5a9-4c5e-9f0d-908d8110e4e3%22%7d>\nMeeting ID: 258 226 846 307\nPasscode: 6JobhB\n________________________________\nDial in by phone\n+1 253-343-5838,,433188680#<tel:+12533435838,,433188680> United States, Tacoma\nFind a local number<https://dialin.teams.microsoft.com/3e3e74cf-d61d-4b31-9e10-c7483ca54c4e?id=433188680>\nPhone conference ID: 433 188 680#\nJoin on a video conferencing device\nTenant key: iadb@m.webex.com<mailto:iadb@m.webex.com>\nVideo ID: 115 776 735 3\nMore info<https://www.webex.com/msteams?confid=1157767353&tenantkey=iadb&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=23930f09-d5a9-4c5e-9f0d-908d8110e4e3&tenantId=9dfb1a05-5f1d-449a-8960-62abcb479e7d&threadId=19_meeting_ZDIyODU5OWItMjU1Ni00ZDAwLWE5NDYtY2E5NmRlNDNiZWY5@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Azu4MMAR",
                        EmailAddresses = new List<string> { "iadb@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("isabelaf@unops.org".ToLower()) ? paoUserEmailMapping["isabelaf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 24, 3, 46, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MvaYQIAZ" },
                    new List<int> { 1442 },
                    new List<string> { "isabelaf@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000B0mKuMAJ",
                    new UNOPSInteraction
                    {
                        Name = "UNBT's TF on crisis and fragility meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-24").ToUniversalTime(),
                        Subject = "UNBT's TF on crisis and fragility meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000B0mKuMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("celiaafricak@unops.org".ToLower()) ? paoUserEmailMapping["celiaafricak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 24, 11, 54, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000I9WuQIAV" },
                    new List<int> { 1725 },
                    new List<string> { "celiaafricak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000B0rScMAJ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting of UNBT WG on Green Deal with DG CLIMA on COP-29",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-24").ToUniversalTime(),
                        Subject = "Meeting of UNBT WG on Green Deal with DG CLIMA on COP-29",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000B0rScMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("celiaafricak@unops.org".ToLower()) ? paoUserEmailMapping["celiaafricak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 24, 11, 52, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000T3hESIAZ" },
                    new List<int> {  },
                    new List<string> { "celiaafricak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000B2odaMAB",
                    new UNOPSInteraction
                    {
                        Name = "Opfølgning",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-25").ToUniversalTime(),
                        Subject = "Opfølgning",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MjU3ODZhZGUtMzdkYS00YzkxLWJiMDMtODdhYTYwYzVjOWY4%40thread.v2/0?context=%7b%22Tid%22%3a%2248dc02d0-bd56-411d-b7c5-a814743bafc6%22%2c%22Oid%22%3a%22de0e5b1d-3483-4723-85a6-0e0d3bb32add%22%7d>\nMeeting ID: 324 607 960 593\nPasscode: MThYjp\n________________________________\nJoin on a video conferencing device\nTenant key: teams@meet.um.dk\nVideo ID: 122 326 964 4\nMore info<https://pexip.me/teams/meet.um.dk/1223269644>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=de0e5b1d-3483-4723-85a6-0e0d3bb32add&tenantId=48dc02d0-bd56-411d-b7c5-a814743bafc6&threadId=19_meeting_MjU3ODZhZGUtMzdkYS00YzkxLWJiMDMtODdhYTYwYzVjOWY4@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000B2odaMAB",
                        EmailAddresses = new List<string> { "teams@meet.um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 25, 12, 27, 58, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BBTlLMAX",
                    new UNOPSInteraction
                    {
                        Name = "Politico Healthcare Summit",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "Politico Healthcare Summit",
                        Description = "https://www.politico.eu/healthcare-summit/",
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BBTlLMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("celiaafricak@unops.org".ToLower()) ? paoUserEmailMapping["celiaafricak@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 10, 31, 8, 19, 16, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "celiaafricak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BHyO5MAL",
                    new UNOPSInteraction
                    {
                        Name = "LEGEN-UNOPS Discussion",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-31").ToUniversalTime(),
                        Subject = "LEGEN-UNOPS Discussion",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BHyO5MAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 4, 23, 15, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Kn08QIAR" },
                    new List<int> { 1646 },
                    new List<string> { "elizabethdu@unops.org", "alistairs@unops.org", "christinebo@unops.org", "vineshw@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BIwLmMAL",
                    new UNOPSInteraction
                    {
                        Name = "UK/UNOPS catch up",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-05").ToUniversalTime(),
                        Subject = "UK/UNOPS catch up",
                        Description = "Hi – this is the time I’d suggested for us to catch up tomorrow. Does it work?\n\nAgnes\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NmZkNmVjMGUtM2M5YS00NmVmLThkNDctN2RmYjIzNGNlMzNk%40thread.v2/0?context=%7b%22Tid%22%3a%22d3a2d0d3-7cc8-4f52-bbf9-85bd43d94279%22%2c%22Oid%22%3a%2252fe111a-2571-484f-a611-55bd4f4bc92c%22%7d>\nMeeting ID: 344 034 945 149\nPasscode: ziogR9\n________________________________\nDial in by phone\n+44 20 7660 8164,,299146987#<tel:+442076608164,,299146987> United Kingdom, City of London\nFind a local number<https://dialin.teams.microsoft.com/33c0cc34-9076-4ef7-bb74-787c147b1311?id=299146987>\nPhone conference ID: 299 146 987#\nJoin on a video conferencing device\nTenant key: teams@fcdo2.onpexip.com<mailto:teams@fcdo2.onpexip.com>\nVideo ID: 121 453 535 9\nMore info<https://pexip.me/teams/fcdo2.onpexip.com/1214535359>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=52fe111a-2571-484f-a611-55bd4f4bc92c&tenantId=d3a2d0d3-7cc8-4f52-bbf9-85bd43d94279&threadId=19_meeting_NmZkNmVjMGUtM2M5YS00NmVmLThkNDctN2RmYjIzNGNlMzNk@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n[https://org871238972424.blob.core.windows.net/$web/TeamsLogo.jpg]\nYour activity on Teams may be monitored in line with relevant UK legislation.\nPrivacy and security<https://www.gov.uk/government/organisations/foreign-commonwealth-development-office/about/personal-information-charter>\n________________________________________________________________________________\n\n\n\nFollow us online: www.gov.uk/fcdo\n\n\nThis email is intended for the addressee(s) only: All messages sent and received by the Foreign, Commonwealth & Development Office may be monitored in line with relevant UK legislation<https://www.gov.uk/government/publications/fcdo-as-a-data-controller-privacy-notice/fcdo-as-a-data-controller-privacy-notice>",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BIwLmMAL",
                        EmailAddresses = new List<string> { "teams@fcdo2.onpexip.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 5, 11, 30, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000099IS3IAM" },
                    new List<int> { 1752 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BJ63OMAT",
                    new UNOPSInteraction
                    {
                        Name = "Virtual meeting with ASG Madi",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-28").ToUniversalTime(),
                        Subject = "Virtual meeting with ASG Madi",
                        Description = "ASG Madi, Assistant Secretary-General and Deputy Executive Director for Resource Management, Sustainability and Partnerships of UN Women",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BJ63OMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kirstined@unops.org".ToLower()) ? paoUserEmailMapping["kirstined@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 5, 14, 34, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1222 },
                    new List<string> { "kirstined@unops.org", "mikaelag@unops.org", "jakobt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BJFhnMAH",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up: Sweden/UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-06").ToUniversalTime(),
                        Subject = "Catch-up: Sweden/UNOPS",
                        Description = "Let’s try Skype?\n\nRoger\n.........................................................................................................................................\nAnslut till Skype-mötet <https://meet.gov.se/roger.m.karlsson/3PHN8NDT>\nAnslutningsproblem? Prova Skype Web App <https://meet.gov.se/roger.m.karlsson/3PHN8NDT?sl=1>\nAnslut via telefon\n +4684054000,,27999194# (RK Sthlm)                                                Svenska (Sverige)\nHitta ett lokalt nummer <https://dialin.regeringskansliet.se?id=27999194>\n\nKonferens-ID: 27999194\n Har du glömt din PIN-kod för inringning? <https://dialin.regeringskansliet.se>  |Hjälp <https://o15.officeredir.microsoft.com/r/rlidLync15?clid=1053&p1=5&p2=2009>\n\n[https://regeringen.se/css/img/logo-sv.png]\nAnslut via videokonferenssystem\nFör att ansluta till Skypemötet via ett videokonferenssystem anger du mötets konferens-ID följt av @regeringskansliet.se\n\nAnslut via webbläsare\nFör att ansluta till Skypemötet via din webbläsare, gå till https://join.regeringskansliet.se och ange mötets konferens-ID. (Detta fungerar endast utanför regeringskansliets nätverk)\n\n\nJoin a meeting using a Video Conference System\nIn order to connect to a Skype meeting using a Video Conference System type in the provided Conference ID followed by ”@regeringskansliet.se”\n\nJoin a meeting using a web browser\nIn order to connect to a Skype meeting through a web browser, with unrestricted internet access, log on to https://join.regeringskansliet.se and type in the provided Conference ID (Please note this will only work outside of the Regeringskansliet’s network)\n[!OC([1053])!]\n.........................................................................................................................................",
                        Location = "Skype-möte",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BJFhnMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 5, 14, 31, 27, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OobWtIAJ" },
                    new List<int> { 1267 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BJtqTMAT",
                    new UNOPSInteraction
                    {
                        Name = "Trade and Supply Chain Finance Gap Summit – Trade Finance Conference of Parties Meeting 1 (TF COP) – Washington, DC",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-28").ToUniversalTime(),
                        Subject = "Trade and Supply Chain Finance Gap Summit – Trade Finance Conference of Parties Meeting 1 (TF COP) – Washington, DC",
                        Description = "Thank you for accepting our invitation to attend the Trade and Supply Chain Finance Gap Summit | Trade Finance Conference of Parties Meeting 1 (TF COP) | 28 October 2024, IFC Headquarters in Washington, DC.\n\n\nPlease note that registration starts at 8:30am. \nThe Trade and Supply Chain Finance Gap Summit (TF COP)\n\nIn order to bring the parties together and kick-start this process, the Trade and Supply Chain Finance Gap Summit (TF COP) will be held on 28 October 2024, hosted by the IFC at its headquarters in Washington, DC. This event is invitation only and aims to bring together the trade finance ecosystem of each global region, including:\n\nDevelopment financial institutions—global, regional, and country level;\nCommercial banks—global, regional, and country level;\nInsurance underwriters and brokers;\nInvestment funds and asset managers;\nLegal firms;\nFintechs; and\nOther industry bodies.\nObjectives of the conference\n\nTF COP will have the following practical objectives:\n\nClosing the trade finance gap once and for all by leveraging public and private sector efforts to develop collaborative strategies that promote sustainable growth and development in emerging markets;\nFacilitating public-private collaboration: the event will bring together DFIs, multilateral development banks and private market players – including trade finance banks, insurance companies, fintech firms and SME borrowers – to share insights and foster much-needed coordination in the trade finance sector;\nDriving innovation in trade finance: the conference breakout sessions will focus on developing and implementing innovative financial solutions that improve trade finance accessibility and efficiency. This bottom-up approach will prioritise practical inputs from practitioners in the field; and\nEstablishing a sustainable network: the event is dedicated to building and maintaining a robust network of stakeholders who are committed to ongoing dialogue and collaboration to improve access to trade finance beyond the conference\nWe look forward to greeting you.",
                        Location = "IFC HQ, 2121 Pennsylvania Avenue NW, Washington, DC 20433, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BJtqTMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 5, 17, 46, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Tq2OPIAZ" },
                    new List<int> { 1547 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BOCZZMA5",
                    new UNOPSInteraction
                    {
                        Name = "DED - Meeting with WB MENA Vice President Ousmane Dione",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-07").ToUniversalTime(),
                        Subject = "DED - Meeting with WB MENA Vice President Ousmane Dione",
                        Description = "<p><b>Microsoft Teams</b> <a href=\"https://aka.ms/JoinTeamsMeeting?omkt=en-US\" target=\"_blank\">Need help?</a><u></u><u></u></p><p><a href=\"https://teams.microsoft.com/l/meetup-join/19%3ameeting_YjYyMmU0YTMtODQ4My00YmY5LWFhNzQtOGQ3NmI5MTBmMmUy%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%2288edcb6a-90d7-45cd-a67b-66171927e685%22%7d\" target=\"_blank\"><b>Join the meeting now</b></a><u></u><u></u></p><p>Meeting ID: 276 040 326 896<u></u><u></u></p><p>Passcode: fCdXGR<u></u><u></u></p><br><hr /><p><b>Join on a video conferencing device</b><u></u><u></u></p><p>Tenant key: <a href=\"mailto:wbg@m.webex.com\" target=\"_blank\">wbg@m.webex.com</a><u></u><u></u></p><p>Video ID: 117 833 133 5<u></u><u></u></p><p><a href=\"https://www.webex.com/msteams?confid=1178331335&amp;tenantkey=wbg&amp;domain=m.webex.com\" target=\"_blank\">More info</a><u></u><u></u></p><p>For organizers: <a href=\"https://teams.microsoft.com/meetingOptions/?organizerId=88edcb6a-90d7-45cd-a67b-66171927e685&amp;tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&amp;threadId=19_meeting_YjYyMmU0YTMtODQ4My00YmY5LWFhNzQtOGQ3NmI5MTBmMmUy@thread.v2&amp;messageId=0&amp;language=en-US\" target=\"_blank\">Meeting options</a></p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BOCZZMA5",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 7, 18, 14, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "alistairs@unops.org", "kirstined@unops.org", "marijab@unops.org", "waingchita@unops.org", "jakobt@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BYpGKMA1",
                    new UNOPSInteraction
                    {
                        Name = "Global Fund Workshop Challenging Operating Enviroments",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-27").ToUniversalTime(),
                        Subject = "Global Fund Workshop Challenging Operating Enviroments",
                        Description = null,
                        Location = "Global Health Campus / online Geneva",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BYpGKMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("louisel@unops.org".ToLower()) ? paoUserEmailMapping["louisel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 14, 12, 31, 52, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000BQj6UIAT" },
                    new List<int> { 1448 },
                    new List<string> { "louisel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BkpIxMAJ",
                    new UNOPSInteraction
                    {
                        Name = "Intro meeting: UNOPS and Swedish Embassy in Nairobi",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-12-02").ToUniversalTime(),
                        Subject = "Intro meeting: UNOPS and Swedish Embassy in Nairobi",
                        Description = "<b>From Sweden:</b><br><ul><li>Karin Snellman, <i>Deputy Permanent Representative to UNEP and UN-HABITAT</i></li><li>Amra Turcinhodzic, <i>Deputy Head of Development Cooperation</i> and <i>Operational Controller</i></li></ul><br><b>From UNOPS:</b><br><ul><li>Asbjørn Brink<i>, Head of Northern Europe </i></li><li>Fumie Arimizu<i>, Partnerships Advisor, Nairobi Office</i></li><li>Elena Georgalla<i>, Partnerships Specialists, Nairobi Office</i></li><li>Naimo Hirsi<i>, Partnerships Specialists, Northern Europe Liaison Office</i></li></ul><br><i><br></i><br><i><br></i>",
                        Location = "Embassy of Sweden, United Nations Cres, Nairobi, Kenya",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BkpIxMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 21, 9, 4, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000UsOgqIAF" },
                    new List<int> { 1267 },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org", "elenage@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BvLsiMAF",
                    new UNOPSInteraction
                    {
                        Name = "Meeting to discuss UNODC potential partner HR global agreement",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-30").ToUniversalTime(),
                        Subject = "Meeting to discuss UNODC potential partner HR global agreement",
                        Description = "Meet",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BvLsiMAF",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laurentium@unops.org".ToLower()) ? paoUserEmailMapping["laurentium@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 27, 14, 26, 32, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VJ4avIAD" },
                    new List<int> { 1194 },
                    new List<string> { "laurentium@unops.org", "arnauds@unops.org", "lorrainea@unops.org", "robertgodin@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BvOP4MAN",
                    new UNOPSInteraction
                    {
                        Name = "UNFPA alignment",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-05").ToUniversalTime(),
                        Subject = "UNFPA alignment",
                        Description = "Meeting with UNFPA to continue discussion on the harmonisation of contracts/benefits for the UNFPA personnel currently managed by UNOPS.\n\nAGENDA\n1. Finalize and agree on the communication to be sent to LICAs regarding contract changes; the who, what and when of the communication.\n2. Confirm if the salary review for January 2025 is complete.\n3. Review the changes to the ICA template, as we will need to coordinate with IT to proceed.\n4. Any other business (AOB).",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BvOP4MAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laurentium@unops.org".ToLower()) ? paoUserEmailMapping["laurentium@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 27, 13, 45, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VJ5AEIA1" },
                    new List<int> { 1195 },
                    new List<string> { "laurentium@unops.org", "arnauds@unops.org", "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BvZcDMAV",
                    new UNOPSInteraction
                    {
                        Name = "UNFPA/UNOPS, LICA to LSC Conversion",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-24").ToUniversalTime(),
                        Subject = "UNFPA/UNOPS, LICA to LSC Conversion",
                        Description = "Meeting with UNFPA to discuss the conversion of UNFPA personnel (approx 40) currently managed by UNOPS",
                        Location = "CPH-5-2.21-Room (12) [Google Meet]",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BvZcDMAV",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 27, 14, 6, 31, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VJ5AEIA1" },
                    new List<int> { 1195 },
                    new List<string> { "arnauds@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BvaYHMAZ",
                    new UNOPSInteraction
                    {
                        Name = "LICA management for UNFPA",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-30").ToUniversalTime(),
                        Subject = "LICA management for UNFPA",
                        Description = "Meeting to discuss contract & benefits alignment for the personnel that UNOPS is managing on behalf of UNOPS\n\nAGENDA\n1. New LSC entitlements for 44 LICA personnel\n2. Update from finance on the KPIs\n3. Salary mapping \n4. Introduction of UNFPA implementation team",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BvaYHMAZ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laurentium@unops.org".ToLower()) ? paoUserEmailMapping["laurentium@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 27, 14, 13, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VJ5AEIA1" },
                    new List<int> { 1195 },
                    new List<string> { "laurentium@unops.org", "arnauds@unops.org", "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Bvb1LMAR",
                    new UNOPSInteraction
                    {
                        Name = "UNODC Finance call",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-18").ToUniversalTime(),
                        Subject = "UNODC Finance call",
                        Description = "Meeting to start discussing details of what a potential HR global agreement with UNODC could look like",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Bvb1LMAR",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 27, 14, 37, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VJ432IAD" },
                    new List<int> { 1194 },
                    new List<string> { "arnauds@unops.org", "ataolo@unops.org", "laurentium@unops.org", "lorrainea@unops.org", "robertgodin@unops.org", "svene@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BvcQPMAZ",
                    new UNOPSInteraction
                    {
                        Name = "BILATERAL Rudi Roberts and Charlie Speller Co-Directors per interim,  FCLP (Forest and Climate Leaders' Partnership)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-13").ToUniversalTime(),
                        Subject = "BILATERAL Rudi Roberts and Charlie Speller Co-Directors per interim,  FCLP (Forest and Climate Leaders' Partnership)",
                        Description = "[<a href=\"https://drive.google.com/drive/folders/1l9sIbawpkqR_BnWk1tudrC0hsVoXPlh8\" target=\"_blank\"><u><u>Briefing</u></u></a>, <a href=\"https://docs.google.com/document/d/1hjIT58wVkJry6TfEoIbAvEQcVpPV0f20BcONkZPOKqk/edit?usp=drive_link\" target=\"_blank\"><u><u>Talking Points</u></u></a>]<br><br>Location is: Cafe opposite the plenary Caspian or UK Pavillion<br><br>Possibility that Dr Joanna Macrae - Head, Strategy International Forest Unit - Department for Energy Security and Net Zero (DESNZ) and Foreign, Commonwealth and Development Office (FCDO) joins the meeting.<br><br>Contact: <i> </i><br><br>Rudi Roberts - whatsapp - +44 7909 122900<br>Charlie Speller - whatsapp - +44 7927 582009",
                        Location = "Cafe opposite the plenary Caspian",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BvcQPMAZ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("djenebas@unops.org".ToLower()) ? paoUserEmailMapping["djenebas@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 11, 27, 14, 29, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1893 },
                    new List<string> { "djenebas@unops.org", "andrewk@unops.org", "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000C4HxqMAF",
                    new UNOPSInteraction
                    {
                        Name = "Centri sanitari DREAM in Africa - Possibile collaborazione tra Comunità di Sant'Egidio e UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-02").ToUniversalTime(),
                        Subject = "Centri sanitari DREAM in Africa - Possibile collaborazione tra Comunità di Sant'Egidio e UNOPS",
                        Description = null,
                        Location = "vtc online",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000C4HxqMAF",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 3, 12, 39, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VhPB2IAN" },
                    new List<int> {  },
                    new List<string> { "martina@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000C6iYyMAJ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting next week with UNOPS Washington office",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-21").ToUniversalTime(),
                        Subject = "Meeting next week with UNOPS Washington office",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NWE1NTMyMGUtMDAwNy00NmFiLWE3MDYtZDNmODljMTFlYWY0%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%223848ab21-f7f1-4477-8d27-66ce536398bc%22%7d>\nMeeting ID: 249 313 065 023\nPasscode: Jh9U2cd3\n________________________________\nDial in by phone\n+1 509-408-0991,,486490123#<tel:+15094080991,,486490123> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=486490123>\nPhone conference ID: 486 490 123#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 113 093 429 6\nMore info<https://www.webex.com/msteams?confid=1130934296&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=3848ab21-f7f1-4477-8d27-66ce536398bc&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NWE1NTMyMGUtMDAwNy00NmFiLWE3MDYtZDNmODljMTFlYWY0@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting; Nairobi WB 16-41 (15) (VC), Nairobi WB 16-95 (15) (VC), Nairobi WB 16-41 (15) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000C6iYyMAJ",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 4, 17, 15, 54, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Vn1naIAB" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "fumiea@unops.org", "elenage@unops.org", "alistairs@unops.org", "francescap@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000C70NmMAJ",
                    new UNOPSInteraction
                    {
                        Name = "WB Nairobi: Chris Oberlack, UN-WB Somalia liaison officer",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-21").ToUniversalTime(),
                        Subject = "WB Nairobi: Chris Oberlack, UN-WB Somalia liaison officer",
                        Description = null,
                        Location = "World Bank, Nairobi",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000C70NmMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("francescap@unops.org".ToLower()) ? paoUserEmailMapping["francescap@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 4, 17, 16, 25, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VnNmRIAV" },
                    new List<int> { 1646 },
                    new List<string> { "francescap@unops.org", "alistairs@unops.org", "christinebo@unops.org", "fumiea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000C7CDdMAN",
                    new UNOPSInteraction
                    {
                        Name = "FYI: UNOPS Washington in Nairobi",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-21").ToUniversalTime(),
                        Subject = "FYI: UNOPS Washington in Nairobi",
                        Description = "Looking forward to our meeting on Thursday, November 21…\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_YWMxMjAwNDctMjdiNy00M2U3LThkNWYtMTNiMmQyNmIyMGQ0%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22b23ecae1-161c-482c-9b7d-e6f7666a6da7%22%7d>\nMeeting ID: 250 762 092 002\nPasscode: YRLDNY\n________________________________\nDial in by phone\n+1 509-408-0991,,224256149#<tel:+15094080991,,224256149> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=224256149>\nPhone conference ID: 224 256 149#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 111 731 644 0\nMore info<https://www.webex.com/msteams?confid=1117316440&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=b23ecae1-161c-482c-9b7d-e6f7666a6da7&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_YWMxMjAwNDctMjdiNy00M2U3LThkNWYtMTNiMmQyNmIyMGQ0@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________\n\n\n_____________________________________________\nFrom: Alistair Peter SOMERVILLE <alistairs@unops.org>\nSent: Sunday, November 17, 2024 8:22 PM\nTo: Elmas Arisoy <earisoy@worldbank.org>\nCc: Andrew C. Kircher <akircher1@worldbank.org>; Million Fikre <mfikre@worldbank.org>; Christine BOWERS <christinebo@unops.org>; Wilma Wambui Mwai <wmwai@worldbank.org>\nSubject: Re: FYI: UNOPS Washington in Nairobi\n\n[External]\nDear Elmas,\n\nThanks for this speedy response. Andy Kircher asked if there could be a Teams connection so your DC colleagues can join. Would this be possible?\n\nBest wishes,\n\nAlistair\n\nOn Sun, Nov 17, 2024 at 7:48 PM Elmas Arisoy <earisoy@worldbank.org<mailto:earisoy@worldbank.org>> wrote:\nDear Alistair\nThursday at 2:45pm (Nairobi time) is fine with me. I have other meetings starting by 4pm so we need to finalize the meeting before 4pm.\nI guess you know the address of World Bank office in Nairobi. I am at 18th floor. We will let you know when we book a meeting room.\nThanks\n\nElmas Arisoy\nProcurement Manager\nEastern and Southern Africa Region\n[cid:image001.png@01DB392E.A05E8730]\nGovernance GP\nT\n+12024732699\nE\nearisoy@worldbank.org<mailto:earisoy@worldbank.org>\nW\nwww.worldbank.org<http://www.worldbank.org/>\n[cid:image002.png@01DB392E.A05E8730]\n[cid:image003.png@01DB392E.A05E8730]\n\n[cid:image004.png@01DB392E.A05E8730]\n\nFrom: Alistair Peter SOMERVILLE <alistairs@unops.org<mailto:alistairs@unops.org>>\nSent: Saturday, November 16, 2024 11:27 PM\nTo: Elmas Arisoy <earisoy@worldbank.org<mailto:earisoy@worldbank.org>>\nCc: Andrew C. Kircher <akircher1@worldbank.org<mailto:akircher1@worldbank.org>>; Million Fikre <mfikre@worldbank.org<mailto:mfikre@worldbank.org>>; Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>\nSubject: Fwd: FYI: UNOPS Washington in Nairobi\n\n[External]\nDear Elmas,\n\nI hope you are well. I understand from Andy Kircher you are available to meet next week — thanks for this. Would you be available either Thursday around 2:45pm or on Friday afternoon?\n\nMany thanks,\n\nAlistair\n\n---------- Forwarded message ---------\nFrom: Andrew C. Kircher <akircher1@worldbank.org<mailto:akircher1@worldbank.org>>\nDate: Sat, Nov 16, 2024 at 6:09 PM\nSubject: RE: FYI: UNOPS Washington in Nairobi\nTo: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>, Rajeev K. Swami <rswami@worldbank.org<mailto:rswami@worldbank.org>>, Million Fikre <mfikre@worldbank.org<mailto:mfikre@worldbank.org>>\nCC: Alistair Peter SOMERVILLE <alistairs@unops.org<mailto:alistairs@unops.org>>\n\nHi Christine,\n\n                Good news.  Our regional procurement manager in Nairobi, Elmas Arisoy, has agreed to meet with you while you are in Nairobi next week.  Her contact is earisoy@worldbank.org<mailto:earisoy@worldbank.org>\n\n                Please reach out to her and also I would appreciate if you set up the meeting in the afternoon so either Million or I can connect virtually from HQ and add anything needed from our side.\n\nRegards, Andy\n\n\n\n\nAndrew Kircher\nUnited Nations Program Coordinator\nOperational Policy and Country Services (OPCS)\nWorld Bank Group\nT 202-473-6313\nM 202-640-9714\nE akircher1@worldbank.org<mailto:akircher1@worldbank.org>\nW www.worldbank.org<http://www.worldbank.org/>\nA 1818 H street NW, Washington DC <https://www.google.com/maps/search/1818+H+street%0D%0A+NW,+Washington+DC++20433,+USA?entry=gmail&source=g> – 20433, USA<https://www.google.com/maps/search/1818+H+street%0D%0A+NW,+Washington+DC++20433,+USA?entry=gmail&source=g>\n\nFrom: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>\nSent: Tuesday, November 5, 2024 2:32 PM\nTo: Andrew C. Kircher <akircher1@worldbank.org<mailto:akircher1@worldbank.org>>; Rajeev K. Swami <rswami@worldbank.org<mailto:rswami@worldbank.org>>; Million Fikre <mfikre@worldbank.org<mailto:mfikre@worldbank.org>>\nCc: Alistair Peter SOMERVILLE <alistairs@unops.org<mailto:alistairs@unops.org>>\nSubject: FYI: UNOPS Washington in Nairobi\n\n[External]\nHello friends,\n\nJust to let you know that Alistair and I will be visiting Nairobi the week of November 18th. We'll be there for internal meetings as well as courtesy visits to the WB Nairobi office.\n\nOf particular interest to you, I am reaching out to George Ferreira da Silva in hopes he'll be in town. If there are other people you can think of who sit in Nairobi that we should meet, please let me know.\n\nOtherwise I will let you return to SFA wrangling! We've been working closely with Gamila and Birgit in recent days, and have recommended a joint discussion with key UN agencies and WB team on SEA/SH reporting.\n\nCheers\nChristine\n\n--\nChristine BOWERS | Head of the Washington Liaison Office | 1775 K Street NW, Washington, DC 20006<https://www.google.com/maps/search/1775+K+Street+NW,+Washington,+DC+20006?entry=gmail&source=g> | Tel: +1 202 428 9694 | www.unops.org<http://www.unops.org/>\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official>, YouTube<https://www.youtube.com/user/UNOPSofficial>.\nSubscribe to our external newsletter in English<https://mailchi.mp/8987deaa0e61/uojskr902j>, French<https://mailchi.mp/unops.org/gu0ld93is9> or Spanish<https://mailchi.mp/edf4b84d88cf/on3qnjwzkh>.\n\n[https://lh5.googleusercontent.com/74QoK81W53ZCnvrfyvmJrCsDPSw9wokiwsz8GCicF3kluBR68tKS2Aqn3tOC4fGecL2CSvq4grwFX_jC30QdImU1nGuSbddDZDfNGAC6TdaCwlktEXO4WgxgOHlLukU0ragISUiM6F1VnXZivw]",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000C7CDdMAN",
                        EmailAddresses = new List<string> { "earisoy@worldbank.org", "christinebo@unops.org", "mfikre@worldbank.org", "rswami@worldbank.org", "alistairs@unops.org", "wbg@m.webex.com", "wmwai@worldbank.org", "akircher1@worldbank.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 4, 17, 17, 43, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000C9ZKbMAN",
                    new UNOPSInteraction
                    {
                        Name = "WBG-UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-05").ToUniversalTime(),
                        Subject = "WBG-UNOPS",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ODQ5Zjg3NGItYzM2MC00MzhlLWE1OTItODQxYzEyODg1MGQ1%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%221a96cf44-cd1c-49b1-b305-d85619ee2d92%22%7d>\nMeeting ID: 278 368 315 166\nPasscode: mXfz23\n________________________________\nDial in by phone\n+1 509-408-0991,,360075089#<tel:+15094080991,,360075089#> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=360075089>\nPhone conference ID: 360 075 089#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 113 182 375 5\nMore info<https://www.webex.com/msteams?confid=1131823755&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=1a96cf44-cd1c-49b1-b305-d85619ee2d92&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_ODQ5Zjg3NGItYzM2MC00MzhlLWE1OTItODQxYzEyODg1MGQ1@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000C9ZKbMAN",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 5, 19, 57, 45, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VsJ0dIAF" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CDR9zMAH",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Pablo Vieira, Global Director NDC Partnership",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-13").ToUniversalTime(),
                        Subject = "Meeting with Pablo Vieira, Global Director NDC Partnership",
                        Description = null,
                        Location = "Baku, Azerbaijan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CDR9zMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("annag@unops.org".ToLower()) ? paoUserEmailMapping["annag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 9, 11, 11, 23, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1790 },
                    new List<string> { "annag@unops.org", "emiliep@unops.org", "jorge.moreiradasilva@unops.org", "katrinl@unops.org", "andrewk@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CDToUMAX",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Francesco La Camera, Director-General, IRENA",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-13").ToUniversalTime(),
                        Subject = "Meeting with Francesco La Camera, Director-General, IRENA",
                        Description = null,
                        Location = "Baku, Azerbaijan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CDToUMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jorge.moreiradasilva@unops.org".ToLower()) ? paoUserEmailMapping["jorge.moreiradasilva@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 9, 11, 14, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1231 },
                    new List<string> { "jorge.moreiradasilva@unops.org", "annag@unops.org", "emiliep@unops.org", "dalilag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CDWb3MAH",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Carolina Fuentes, Director, Santiago Network",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-13").ToUniversalTime(),
                        Subject = "Meeting with Carolina Fuentes, Director, Santiago Network",
                        Description = null,
                        Location = "Baku, Azerbaijan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CDWb3MAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("annag@unops.org".ToLower()) ? paoUserEmailMapping["annag@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 9, 11, 17, 14, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1857 },
                    new List<string> { "annag@unops.org", "emiliep@unops.org", "jorge.moreiradasilva@unops.org", "katrinl@unops.org", "andrewk@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CIoNhMAL",
                    new UNOPSInteraction
                    {
                        Name = "Sunset of July 2022 Clauses",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-12-11").ToUniversalTime(),
                        Subject = "Sunset of July 2022 Clauses",
                        Description = "Dear Colleagues:\n\nI am pleased to announce that UNOPS and USAID have officially agreed to sunset the July 2022 special clauses, including the three-percent fixed management fee. This change will take effect once the new provisions are formally published in USAID’s Automated Directives System (ADS) in the coming weeks. Until then, the July 2022 clauses will remain in effect.\n\nI will meet with USAID on December 12 and hope to gain a clearer timeline for when the new clauses will be incorporated into the ADS. I will share updates as soon as more information becomes available.\n\nI would like to take a moment to highlight that this step marks the restoration of a regular business relationship with the United States, a significant milestone for UNOPS. I am immensely proud of our accomplishments over the past two and a half years. Reflecting on the most challenging moments of the S3i crisis, when it often seemed like this outcome was out of reach, makes this achievement all the more meaningful.\n\nWe are already seeing positive results from our strengthened partnership with the United States. In recent months, the State Department’s Bureau of International Narcotics and Law Enforcement Affairs (INL) has committed over $10 million to new projects, with additional opportunities under discussion. Similarly, the State Department’s Bureau of Population, Refugees, and Migration (PRM) has invested $70 million to operationalize the critical Jordanian corridor for delivering humanitarian aid to Gaza. Finally, the Bureau of Oceans and International Environmental and Scientific Affairs (OES), a new UNOPS partner, has allocated approximately $1 million to support GCAP. We look forward to deepening collaboration with USAID and other U.S. government entities in the weeks ahead, and I am excited about the opportunities the coming months will bring.  \n\nI am particularly proud that this achievement represents a collective effort across UNOPS. It would not have been possible without the leadership of our Executive Director and senior leaders in Copenhagen and the regions, especially PLG Director Emilie Potvin. I want to extend my deepest gratitude to Sven Eckert, Irakli Jibladze, and the entire finance team for their tireless efforts in briefing U.S. counterparts on the complexities of net-zero budgeting, the cost recovery model, and the refund process, all while devising the creative solutions necessary to sunset the crisis. I also want to acknowledge Vinesh Winodan and Eli Duby for their dedicated work in negotiating the texts that made this milestone possible.  \n\nI am deeply grateful to all our teams worldwide who, despite the challenges posed by the S3i crisis, remained unwavering in delivering U.S.-funded projects. I especially value your dedication to maintaining an open dialogue with your American counterparts, even when the prospect of future collaboration seemed remote. These efforts were instrumental in preserving our partnerships and reestablishing our regular business relationship.\n\nAs we close the chapter on the S3i crisis with the United States, I am eager to work with you to fully leverage our renewed partnership with Washington. I am excited to see the outcomes of our collective efforts as we move forward into 2025.\n\nOnward and upward!\n\nBest regards, Patrick Ellsworth",
                        Location = "Notes",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CIoNhMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 11, 19, 36, 48, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WFuEaIAL" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CM0KEMA1",
                    new UNOPSInteraction
                    {
                        Name = "UNODC /UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-13").ToUniversalTime(),
                        Subject = "UNODC /UNOPS",
                        Description = "Key discussion points:\n1. \n2. \n3.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CM0KEMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 13, 11, 12, 54, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VJ4avIAD" },
                    new List<int> { 1194 },
                    new List<string> { "arnauds@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CN2TdMAL",
                    new UNOPSInteraction
                    {
                        Name = "Monthly Check-in Meeting w/World Bank OPCS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-12-11").ToUniversalTime(),
                        Subject = "Monthly Check-in Meeting w/World Bank OPCS",
                        Description = "Discussion on SFA renegotiations\n- Christine updated WB that she is working to find solution acceptable to IAIG on incident and accident reporting. Asked WB to provide updated ESIRT template. Concern remains around extent of personally identifiable information required. \n- WB is preparing a \"process document\" on how to comply with reporting. \n- Issue around SEAMAP project and UN-UN agreement. WLO to follow up. \n- WLO asked for update on the official French translation of the SFA.\n- Christine noted that UNOPS is trying to \"squeeze every last drop out of the interim template\" as \"quite some time\" would be needed to reach a formal agreement on the new SFA. Christine reiterated concern around the ESS applicability and/as detailed in the table in the document (Art 21). \n- Andy requested list of projects in the pipeline under interim solution and new template.\n- WLO to follow up in writing to request ESS training from WB in Africa and Asia for UNOPS teams. \n- Alistair updated in UNOPS participation in the FCV strategy review meeting. \n- Million asked WLO to note a potential issue to discuss in future meetings on the partnership in Nicaragua. \n- New FCV Group Director Shubham Chaudhuri - former Country Director in Nigeria and Afghanistan.",
                        Location = "Virtual",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CN2TdMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 13, 20, 47, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000COeGpMAL",
                    new UNOPSInteraction
                    {
                        Name = "Intro: Human Practice and UNOPS in Kenya",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-12-03").ToUniversalTime(),
                        Subject = "Intro: Human Practice and UNOPS in Kenya",
                        Description = null,
                        Location = "The United Nations Office for Project Services, QR76+V2G, United Nations Ave, Nairobi City, Kenya",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000COeGpMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 16, 7, 38, 7, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org", "fumiea@unops.org", "elenage@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000COj0KMAT",
                    new UNOPSInteraction
                    {
                        Name = "Novo Nordisk A/S - UNOPS: Introduction",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-12-05").ToUniversalTime(),
                        Subject = "Novo Nordisk A/S - UNOPS: Introduction",
                        Description = "<ul><li><b>From UNOPS:</b><br><ul><li>Asbjørn Brink<i>, Head of Northern Europe </i></li><li>Fumie Arimizu<i>, Partnerships Advisor, Nairobi Office</i></li><li>Elena Georgalla<i>, Partnerships Specialists, Nairobi Office</i></li><li>Naimo Hirsi<i>, Partnerships Specialists, Northern Europe Liaison Office<br><br></i></li></ul></li><li><b>From Novo Nordisk A/S</b><ul><li>Carolyne Olale Nordstrom, <i>Public Affairs Manager</i></li><li>Brian Ahona, <i>EA &amp; Sustainability Lead</i></li></ul></li></ul><br>Meeting location: <a href=\"https://maps.app.goo.gl/MMg93UdXQ86HswQY8\">https://maps.app.goo.gl/MMg93UdXQ86HswQY8</a> <i></i>",
                        Location = "Connect Coffee, Avenue 5 building",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000COj0KMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 16, 7, 38, 44, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1688 },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org", "fumiea@unops.org", "elenage@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000COndoMAD",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS and Finland",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-12-12").ToUniversalTime(),
                        Subject = "UNOPS and Finland",
                        Description = null,
                        Location = "Embassy of Finland, Addis Ababa, Ethiopia",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000COndoMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 16, 7, 39, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1087 },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000COpTtMAL",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS / UK in Nairobi",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-12-04").ToUniversalTime(),
                        Subject = "UNOPS / UK in Nairobi",
                        Description = null,
                        Location = "British High Commission Nairobi, Upper Hill Rd, Nairobi, Kenya",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000COpTtMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sharonle@unops.org".ToLower()) ? paoUserEmailMapping["sharonle@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 16, 7, 38, 27, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1752 },
                    new List<string> { "sharonle@unops.org", "fumiea@unops.org", "elenage@unops.org", "naimoh@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CSxBHMA1",
                    new UNOPSInteraction
                    {
                        Name = "Meeting on excess reserve refund from MHLW Japan",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-27").ToUniversalTime(),
                        Subject = "Meeting on excess reserve refund from MHLW Japan",
                        Description = "菅原様\nお世話になっております。\n厚労省国際課の藤野です。\n当省では、通常使用するウェブ会議システムがteamsとなっておりますので、こちらで実施できますと幸いです。\nよろしくお願いいたします。\n________________________________________________________________________________\nMicrosoft Teams ヘルプが必要ですか?<https://aka.ms/JoinTeamsMeeting?omkt=ja-JP>\n今すぐ会議に参加する<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NmZiYzE4Y2EtMDRkNy00YjM0LWE2OTctNDc2OGNlN2U2NTgx%40thread.v2/0?context=%7b%22Tid%22%3a%22c4f5cfcd-2672-4991-98f8-877042f5c9b3%22%2c%22Oid%22%3a%22ac397a69-a40b-476f-9810-8a2b85f58a09%22%7d>\n会議 ID: 418 208 460 406\nパスコード: SzCpeg\n[X]\n開催者向け: 会議オプション<https://teams.microsoft.com/meetingOptions/?organizerId=ac397a69-a40b-476f-9810-8a2b85f58a09&tenantId=c4f5cfcd-2672-4991-98f8-877042f5c9b3&threadId=19_meeting_NmZiYzE4Y2EtMDRkNy00YjM0LWE2OTctNDc2OGNlN2U2NTgx@thread.v2&messageId=0&language=ja-JP>\n________________________________________________________________________________",
                        Location = "Microsoft Teams 会議",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CSxBHMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 2, 52, 37, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wh3VgIAJ" },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CSylXMAT",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA on Gaza",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-30").ToUniversalTime(),
                        Subject = "Meeting with JICA on Gaza",
                        Description = "Dear Yuichi san, Dear Hitomi san, \nJICA has contacted me asking to 'exchange information' on Gaza. I consulted with Marija, our focal point for Gaza, and we agreed that someone from Sigrid Kaag's office and Marija will both be at the meeting. I asked JICA to share with us Microsoft Teams link to be used for the meeting so I will share it once ready.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CSylXMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 3, 34, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhZ0DIAV" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "marijab@unops.org", "sophien@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT0NVMA1",
                    new UNOPSInteraction
                    {
                        Name = "Meeting on potential opportunity in Palestine/Gaza",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-04").ToUniversalTime(),
                        Subject = "Meeting on potential opportunity in Palestine/Gaza",
                        Description = "UNOPS駐日事務所　前川様、菅原様　（JICA関係各位）\n　　　　　　　　　　　　　　　　　　　　　←　JICA中東・欧州部　三藤\n\nお世話になっております。\n別途メールにてやり取りをさせて頂いております標記の件、会議招集（リンク先）をお送りさせて頂きます。\nどうぞ宜しくお願い致します。\n\n________________________________________________________________________________\nMicrosoft Teams ヘルプが必要ですか?<https://aka.ms/JoinTeamsMeeting?omkt=ja-JP>\n今すぐ会議に参加する<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZWFlYWIxNzMtNWY4MC00ZTg4LWIxNzktMjRjNGM5NDY1YTE5%40thread.v2/0?context=%7b%22Tid%22%3a%22eba9fc42-5588-4d31-8a4e-6e1bf79d31c0%22%2c%22Oid%22%3a%2223315418-6c69-4627-bae5-f9dc7c8b982f%22%7d>\n会議 ID: 481 043 459 183\nパスコード: Rw33cS2L\n________________________________\n電話によるダイヤルイン\n+81 3-4567-8430,,981074825#<tel:+81345678430,,981074825> 日本, 江東区\nローカル番号を検索する<https://dialin.teams.microsoft.com/59c4cce0-8ce9-4570-93b3-6aa9b1a6be9b?id=981074825>\n電話会議 ID: 981 074 825#\n開催者向け: 会議オプション<https://teams.microsoft.com/meetingOptions/?organizerId=23315418-6c69-4627-bae5-f9dc7c8b982f&tenantId=eba9fc42-5588-4d31-8a4e-6e1bf79d31c0&threadId=19_meeting_ZWFlYWIxNzMtNWY4MC00ZTg4LWIxNzktMjRjNGM5NDY1YTE5@thread.v2&messageId=0&language=ja-JP> | ダイヤルイン PIN のリセット<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams 会議; 会議室 HQ 04階-4B会議室(10名), 会議室 HQ 04階-4B会議室(10名)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT0NVMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 2, 42, 2, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhZ0DIAV" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT0SVMA1",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with New MoFA Director Ando",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-29").ToUniversalTime(),
                        Subject = "Meeting with New MoFA Director Ando",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT0SVMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 3, 41, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WheivIAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT0qXMAT",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between ATScale CEO and MoFA Japan",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-26").ToUniversalTime(),
                        Subject = "Meeting between ATScale CEO and MoFA Japan",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT0qXMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 3, 0, 16, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whd3hIAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT1RdMAL",
                    new UNOPSInteraction
                    {
                        Name = "Courtesy Meeting with Embassy of Japan, Jakarta",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-18").ToUniversalTime(),
                        Subject = "Courtesy Meeting with Embassy of Japan, Jakarta",
                        Description = "Dear Colleagues,<br><br>Please allow me to mark your calendar for your upcoming meeting with Embassy of Japan in Jakarta.<br><br>You will meet <span>Mr. UEDA Hajime, minister in charge of economy and development cooperation. </span><br><span><br></span><br><span>For easy reference, details of our contact at the embassy for this meeting is </span><span>Taiju Sasaki,  </span><span>Second Secretary, Economic/Political Affairs Section, </span><span><a href=\"mailto:taiju.sasaki@mofa.go.jp\" target=\"_blank\">taiju.sasaki@mofa.go.jp</a> </span><span>+62 852 8179 1050</span><br><br>Correspondence related to this appointment is attached. <br><br>Thank you,<br>Dwi Riani",
                        Location = "Embassy of Japan, Jl. M.H. Thamrin No.24, Gondangdia, Kec. Menteng, Kota Jakarta Pusat, Daerah Khusus Ibukota Jakarta 10350, Indonesia",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT1RdMAL",
                        EmailAddresses = new List<string> { "taiju.sasaki@mofa.go.jp" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 3, 11, 3, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhdIGIAZ" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "sharont@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT1ZhMAL",
                    new UNOPSInteraction
                    {
                        Name = "Courtesy meeting with the Embassy of Japan in Cambodia",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-08").ToUniversalTime(),
                        Subject = "Courtesy meeting with the Embassy of Japan in Cambodia",
                        Description = null,
                        Location = "Embassy of Japan in Cambodia",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT1ZhMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 3, 16, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whdq5IAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "edak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT64kMAD",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Country Assistance Planning Division III on JSB",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-10").ToUniversalTime(),
                        Subject = "Meeting with Country Assistance Planning Division III on JSB",
                        Description = null,
                        Location = "MoFA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT64kMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 6, 31, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhlsXIAR" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT6zBMAT",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA by AR Director Sanjay",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-04").ToUniversalTime(),
                        Subject = "Meeting with JICA by AR Director Sanjay",
                        Description = "Meeting minutes: https://docs.google.com/document/d/1Bc0DQ0WRxG9I_EHCHUVUiv9wo6rkxcVqKrxSTd0jdlo/edit?tab=t.0",
                        Location = "JICA Headquarters, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT6zBMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 5, 44, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhlkQIAR" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "sanjaym@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT81iMAD",
                    new UNOPSInteraction
                    {
                        Name = "Introductory meeting with the new focal point of EoJ Denmark",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-13").ToUniversalTime(),
                        Subject = "Introductory meeting with the new focal point of EoJ Denmark",
                        Description = null,
                        Location = "UNOPS Tokyo Liaison Office at the UNU",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT81iMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 7, 15, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whze1IAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT84vMAD",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with MoFA Japan by AR Director Sanjay on Myanmar",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-04").ToUniversalTime(),
                        Subject = "Meeting with MoFA Japan by AR Director Sanjay on Myanmar",
                        Description = "Meeting minutes: https://docs.google.com/document/d/1_U6v9DRQ-xL4XpZsmx7Rl--PDsGTsTmsVF3_NZtj-7o/edit?tab=t.0",
                        Location = "MoFA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT84vMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 5, 57, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhnPeIAJ" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "sanjaym@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT9SWMA1",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with MoFA Japan by AR Director Sanjay on Afghanistan",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-04").ToUniversalTime(),
                        Subject = "Meeting with MoFA Japan by AR Director Sanjay on Afghanistan",
                        Description = "Meeting minutes: https://docs.google.com/document/d/1dcnInPyvj79fIpqtbGJ9e8mPqTaFW2aLt68h8QwQFjQ/edit?tab=t.0",
                        Location = "MoFA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT9SWMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 6, 15, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wht21IAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "sanjaym@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTA6lMAH",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with MoFA Japan by MR Director Bana on Palestine",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-11").ToUniversalTime(),
                        Subject = "Meeting with MoFA Japan by MR Director Bana on Palestine",
                        Description = null,
                        Location = "MoFA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTA6lMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 7, 53, 39, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhLfzIAF" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "usmana@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTABbMAP",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA on potential educational facility rehabilitation in Ukraine",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-19").ToUniversalTime(),
                        Subject = "Meeting with JICA on potential educational facility rehabilitation in Ukraine",
                        Description = "<p><b>Microsoft Teams</b> <a href=\"https://aka.ms/JoinTeamsMeeting?omkt=ja-JP\" target=\"_blank\">ヘルプが必要ですか?</a><u></u><u></u></p><p><a href=\"https://teams.microsoft.com/l/meetup-join/19%3ameeting_Yzk4ZGFkNWQtOGYxNC00YTgzLThkNjctYzI0ODFmNDAwMzE3%40thread.v2/0?context=%7b%22Tid%22%3a%22eba9fc42-5588-4d31-8a4e-6e1bf79d31c0%22%2c%22Oid%22%3a%225ec9df2b-bbe7-4f55-b6fc-9d9b6570c0b5%22%7d\" target=\"_blank\"><b>今すぐ会議に参加する</b></a><u></u><u></u></p><p>会議 ID: 410 253 408 875<u></u><u></u></p><p>パスコード: hVYbCW<u></u><u></u></p><br><hr /><p><b>電話によるダイヤルイン</b><u></u><u></u></p><p><a target=\"_blank\">+81 3-4567-8430,,615586660#</a> Japan, 東京 (Tokyo)<u></u><u></u></p><p><a href=\"https://dialin.teams.microsoft.com/59c4cce0-8ce9-4570-93b3-6aa9b1a6be9b?id=615586660\" target=\"_blank\">ローカル番号を検索する</a><u></u><u></u></p><p>電話会議 ID: 615 586 660#<u></u><u></u></p><p>開催者向け: <a href=\"https://teams.microsoft.com/meetingOptions/?organizerId=5ec9df2b-bbe7-4f55-b6fc-9d9b6570c0b5&amp;tenantId=eba9fc42-5588-4d31-8a4e-6e1bf79d31c0&amp;threadId=19_meeting_Yzk4ZGFkNWQtOGYxNC00YTgzLThkNjctYzI0ODFmNDAwMzE3@thread.v2&amp;messageId=0&amp;language=ja-JP\" target=\"_blank\">会議オプション</a> | <a href=\"https://dialin.teams.microsoft.com/usp/pstnconferencing\" target=\"_blank\">ダイヤルイン PIN のリセット</a></p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTABbMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 7, 24, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi1CSIAZ" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTAd4MAH",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA on new fixed management fee rate for JICA",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-03").ToUniversalTime(),
                        Subject = "Meeting with JICA on new fixed management fee rate for JICA",
                        Description = "UNOPS菅原様\n\nお世話になっております。\n別信でやり取りさせていただきました通り、下記の通り面談のお時間を頂きたく、\nTeams会議招集を送付させていただきます。末尾の参加ボタンよりご入室いただけますと幸いです。\n\n●日時：6月3日（月）15:00~16:00\n●場所：オンライン（Teamsリンクは下記参照）\n●当方参加者：企画部業務企画第一課企画役南雲、同課職員新田（関係部から数名参加するかもしれません。）\n●お話ししたい点：間接費率の算出方法、これまでの弊機構との間での間接費の調整方法、今後の調整方法\n\n当日は、どうぞ宜しくお願い致します。",
                        Location = "Online",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTAd4MAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 4, 45, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi7UnIAJ" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTE7AMAX",
                    new UNOPSInteraction
                    {
                        Name = "Courtesy visit by new director in MoFA Jinji Center to UNOPS TLO",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-22").ToUniversalTime(),
                        Subject = "Courtesy visit by new director in MoFA Jinji Center to UNOPS TLO",
                        Description = null,
                        Location = "UNOPS Tokyo Liaison Office",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTE7AMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 46, 14, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi98QIAR" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTFJKMA5",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA on cancellation of SESU potential project",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-06-13").ToUniversalTime(),
                        Subject = "Meeting with JICA on cancellation of SESU potential project",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTFJKMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 7, 27, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whv2JIAR" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTH9pMAH",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA by MR Director Bana",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-11").ToUniversalTime(),
                        Subject = "Meeting with JICA by MR Director Bana",
                        Description = null,
                        Location = "JICA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTH9pMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 7, 36, 32, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whw6PIAR" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "usmana@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTHwFMAX",
                    new UNOPSInteraction
                    {
                        Name = "Courtesy visit to EoJ Ethiopia",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-27").ToUniversalTime(),
                        Subject = "Courtesy visit to EoJ Ethiopia",
                        Description = null,
                        Location = "Embassy of Japan in Ethiopia",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTHwFMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 9, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi93ZIAR" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTI5vMAH",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA Uzbekistan",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-18").ToUniversalTime(),
                        Subject = "Meeting with JICA Uzbekistan",
                        Description = null,
                        Location = "JICA Uzbekistan Office",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTI5vMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 34, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WiI3dIAF" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "jamshidr@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTIHBMA5",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with MoFA Japan by MR Director Bana on UNOPS",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-06-11").ToUniversalTime(),
                        Subject = "Meeting with MoFA Japan by MR Director Bana on UNOPS",
                        Description = null,
                        Location = "MoFA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTIHBMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 7, 48, 13, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi5j8IAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "usmana@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTIHGMA5",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA on potential grant aid project in Myanmar",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-22").ToUniversalTime(),
                        Subject = "Meeting with JICA on potential grant aid project in Myanmar",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTIHGMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 21, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WiChNIAV" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTJRoMAP",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with EoJ Sudan on new grant project",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-04-18").ToUniversalTime(),
                        Subject = "Meeting with EoJ Sudan on new grant project",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTJRoMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("tawandaa@unops.org".ToLower()) ? paoUserEmailMapping["tawandaa@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 25, 29, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whv5VIAR" },
                    new List<int> { 1906 },
                    new List<string> { "tawandaa@unops.org", "yuichis@unops.org", "munierm@unops.org", "lydiaat@unops.org", "nadab@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTKAwMAP",
                    new UNOPSInteraction
                    {
                        Name = "Courtesy visit by Mr Utsunomiya, EoJ Iran",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-13").ToUniversalTime(),
                        Subject = "Courtesy visit by Mr Utsunomiya, EoJ Iran",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTKAwMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 54, 55, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi7Y2IAJ" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTKneMAH",
                    new UNOPSInteraction
                    {
                        Name = "Courtesy visit to EoJ Sudan (evacuated to Cairo, Egypt)",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-05-26").ToUniversalTime(),
                        Subject = "Courtesy visit to EoJ Sudan (evacuated to Cairo, Egypt)",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTKneMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 15, 6, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whv5VIAR" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTLF3MAP",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA Ethiopia",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-05-27").ToUniversalTime(),
                        Subject = "Meeting with JICA Ethiopia",
                        Description = null,
                        Location = "JICA Ethiopia",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTLF3MAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 12, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhuB4IAJ" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "lydiaat@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTLbfMAH",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA on Palestine",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-03-13").ToUniversalTime(),
                        Subject = "Meeting with JICA on Palestine",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTLbfMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 41, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi5OMIAZ" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTLtPMAX",
                    new UNOPSInteraction
                    {
                        Name = "Courtesy visit to EoJ Uzbekistan",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-03-18").ToUniversalTime(),
                        Subject = "Courtesy visit to EoJ Uzbekistan",
                        Description = null,
                        Location = "Embassy of Japan in Uzbekistan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTLtPMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 30, 22, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhmqCIAR" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "jamshidr@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTMXuMAP",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with MoFA focal point for UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-15").ToUniversalTime(),
                        Subject = "Meeting with MoFA focal point for UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTMXuMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 8, 51, 1, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OiFjKIAV" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTNVaMAP",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between PLG Director and MHLW Assistant Minister",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-07").ToUniversalTime(),
                        Subject = "Meeting between PLG Director and MHLW Assistant Minister",
                        Description = null,
                        Location = "MHLW Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTNVaMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 9, 5, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WiNcnIAF" },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTNYeMAP",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between PLG Director and MoFA Japan",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-02-05").ToUniversalTime(),
                        Subject = "Meeting between PLG Director and MoFA Japan",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTNYeMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 9, 9, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi5j8IAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTThGMAX",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between PLG Director and JICA",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-06").ToUniversalTime(),
                        Subject = "Meeting between PLG Director and JICA",
                        Description = null,
                        Location = "JICA HQ",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTThGMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 9, 8, 1, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wi5OMIAZ" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTVJFMA5",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between PLG Director and MoFA Country Assistance Planning Division III",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-02-05").ToUniversalTime(),
                        Subject = "Meeting between PLG Director and MoFA Country Assistance Planning Division III",
                        Description = null,
                        Location = "MoFA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTVJFMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 9, 15, 37, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WiQlyIAF" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org", "emiliep@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CTWlZMAX",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with EoJ Denmark",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-01-19").ToUniversalTime(),
                        Subject = "Meeting with EoJ Denmark",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CTWlZMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 9, 24, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WiOImIAN" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org", "yukom@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CUtZgMAL",
                    new UNOPSInteraction
                    {
                        Name = "Discussion UNOPS/WB",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-18").ToUniversalTime(),
                        Subject = "Discussion UNOPS/WB",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZTI4ZTE4MDAtMTI5MC00NmZkLWI5ZDAtZjM1NTY5N2ZmY2E1%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22786b72f1-28f3-43ec-9678-68ad981664bb%22%7d>\nMeeting ID: 279 148 937 422\nPasscode: H2XC3Ly6\n________________________________\nDial in by phone\n+1 509-408-0991,,492015153#<tel:+15094080991,,492015153> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=492015153>\nPhone conference ID: 492 015 153#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 118 132 199 9\nMore info<https://www.webex.com/msteams?confid=1181321999&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=786b72f1-28f3-43ec-9678-68ad981664bb&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_ZTI4ZTE4MDAtMTI5MC00NmZkLWI5ZDAtZjM1NTY5N2ZmY2E1@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting; MC 10-500 (16) (VC) Private, MC 10-500 (16) (VC) Private",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CUtZgMAL",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 18, 21, 17, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> {  },
                    new List<string> { "christinebo@unops.org", "kelleys@unops.org", "elizabethdu@unops.org", "alistairs@unops.org", "nivesc@unops.org", "juliasc@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CV5yzMAD",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between PDD Head of Secretariat and JICA",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-01-29").ToUniversalTime(),
                        Subject = "Meeting between PDD Head of Secretariat and JICA",
                        Description = null,
                        Location = "JICA HQ, Tokyo, Japan",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CV5yzMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2024, 12, 19, 2, 39, 25, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WnE12IAF" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org", "lorenzog@unops.org", "atles@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CpkOUMAZ",
                    new UNOPSInteraction
                    {
                        Name = "Letter from ED to USAID on Sunset of July 2022 Clauses",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-20").ToUniversalTime(),
                        Subject = "Letter from ED to USAID on Sunset of July 2022 Clauses",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CpkOUMAZ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 6, 18, 52, 58, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WFuEaIAL" },
                    new List<int> {  },
                    new List<string> { "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CrKeTMAV",
                    new UNOPSInteraction
                    {
                        Name = "Discussion on the agenda for the meeting: ASG Madi & ASG Damkjær - Partnership Dialogue",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-07").ToUniversalTime(),
                        Subject = "Discussion on the agenda for the meeting: ASG Madi & ASG Damkjær - Partnership Dialogue",
                        Description = null,
                        Location = "CPH-5-3.41-Room (12) [Google Meet]",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CrKeTMAV",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("waingchita@unops.org".ToLower()) ? paoUserEmailMapping["waingchita@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 7, 13, 59, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1222 },
                    new List<string> { "waingchita@unops.org", "mikaelag@unops.org", "jakobt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvOEnMAN",
                    new UNOPSInteraction
                    {
                        Name = "Status på diverse",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-09").ToUniversalTime(),
                        Subject = "Status på diverse",
                        Description = "Hermed en mødeindkaldelse (min Teams driller - håber at Skype kan fungere for dig?).\n.........................................................................................................................................\nJoin Skype Meeting<https://meet.um.dk/rikoln/W988DVW9>\nTrouble Joining? Try Skype Web App<https://meet.um.dk/rikoln/W988DVW9?sl=1>\nJoin by phone\n\n+45 33 92 09 99,,1703727# (Denmark)                         Danish (Denmark)\n+45 33 92 09 98,,1703727# (Denmark)                         English (United Kingdom)\n\nFind a local number<https://dialin.um.dk?id=1703727>\n\nConference ID: 1703727\nForgot your dial-in PIN?<https://dialin.um.dk> |Help<https://o15.officeredir.microsoft.com/r/rlidLync15?clid=1033&p1=5&p2=2009>\n\n[!OC([1033])!]\n.........................................................................................................................................",
                        Location = "Skype Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvOEnMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 12, 7, 23, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx000009fefVIAQ" },
                    new List<int> { 1123 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvjJJMAZ",
                    new UNOPSInteraction
                    {
                        Name = "Discussion on Strengthening Operational Engagements with UN Agencies",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-08").ToUniversalTime(),
                        Subject = "Discussion on Strengthening Operational Engagements with UN Agencies",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_M2M2ZjM2MmUtYWQyOS00ODg0LTk5OTktODIwMmMwYjY4ZWU1%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22863eca3b-25e6-4461-9387-398aa171590b%22%7d>\nMeeting ID: 241 340 080 455\nPasscode: TmqNDp\n________________________________\nDial in by phone\n+1 509-408-0991,,807638014#<tel:+15094080991,,807638014> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=807638014>\nPhone conference ID: 807 638 014#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com<mailto:wbg@m.webex.com>\nVideo ID: 112 706 607 2\nMore info<https://www.webex.com/msteams?confid=1127066072&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=863eca3b-25e6-4461-9387-398aa171590b&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_M2M2ZjM2MmUtYWQyOS00ODg0LTk5OTktODIwMmMwYjY4ZWU1@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Teams Connection",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvjJJMAZ",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 25, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvkgmMAB",
                    new UNOPSInteraction
                    {
                        Name = "Touch Base UNOPS/WB",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-04").ToUniversalTime(),
                        Subject = "Touch Base UNOPS/WB",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NTNmZTc1YjEtYjU5NC00OTFiLTg0OTEtZDY4MWZkZDUxYzQ0%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22786b72f1-28f3-43ec-9678-68ad981664bb%22%7d>\nMeeting ID: 253 614 730 578\nPasscode: FuNtkY\n________________________________\nDial in by phone\n+1 509-408-0991,,867337593#<tel:+15094080991,,867337593> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=867337593>\nPhone conference ID: 867 337 593#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 114 540 168 1\nMore info<https://www.webex.com/msteams?confid=1145401681&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=786b72f1-28f3-43ec-9678-68ad981664bb&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NTNmZTc1YjEtYjU5NC00OTFiLTg0OTEtZDY4MWZkZDUxYzQ0@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvkgmMAB",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 19, 52, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvl8BMAR",
                    new UNOPSInteraction
                    {
                        Name = "Christine/Andy touchbase",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-04").ToUniversalTime(),
                        Subject = "Christine/Andy touchbase",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NTIwYzA4NTItZmI2ZS00YmYwLTg5MzAtNGRiM2YwZDBmYWQ3%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22786b72f1-28f3-43ec-9678-68ad981664bb%22%7d>\nMeeting ID: 240 137 677 045\nPasscode: 9GR38G9g\n________________________________\nDial in by phone\n+1 509-408-0991,,955285883#<tel:+15094080991,,955285883> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=955285883>\nPhone conference ID: 955 285 883#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 114 651 767 5\nMore info<https://www.webex.com/msteams?confid=1146517675&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=786b72f1-28f3-43ec-9678-68ad981664bb&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NTIwYzA4NTItZmI2ZS00YmYwLTg5MzAtNGRiM2YwZDBmYWQ3@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvl8BMAR",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 35, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvmlrMAB",
                    new UNOPSInteraction
                    {
                        Name = "Meet East and Southern Africa DSO, Amit Dar",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-27").ToUniversalTime(),
                        Subject = "Meet East and Southern Africa DSO, Amit Dar",
                        Description = "Christine's notes:\n\nAmit outlined 4 key priorities:\n- Energy access. He expects about 40% grid / 60% off-grid\n- Access to water. They have a big new program launching\n- Digital connectivity, including linking public facilities (schools etc) to the internet. Also a digital ID as enabler of access to services.\n- Education, especially the quality. Largely basic education.\n\nHe said the Africa East (which is how they refer to East and Southern Africa) portfolio is 40bn, with 10-12bn new lending every year.\n\nThey are increasing their focus on outcomes and results. He mentioned also importance of understanding climate co-benefits.\n\nIn response to Kirstine's question on what they value about UNOPS, Amit said:\n- UNOPS presence on the ground in difficult contexts (i.e. South Sudan, Ethiopia)\n- procurement\n- value for money in FCV\n- project management\n- delivery of infrastructure",
                        Location = "J11-001",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvmlrMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kirstined@unops.org".ToLower()) ? paoUserEmailMapping["kirstined@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 23, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7WCUIA3" },
                    new List<int> { 1646 },
                    new List<string> { "kirstined@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvnbQMAR",
                    new UNOPSInteraction
                    {
                        Name = "SFA incident notification - urgent call",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-04").ToUniversalTime(),
                        Subject = "SFA incident notification - urgent call",
                        Description = "INCIDENTS AND ACCIDENTS\n\na) Promptly notify the Government and the Bank of any incident or accident related to the Delivery of Outputs which has, or is likely to have, a significant adverse effect on the environment, the affected communities, the public or workers, including, inter alia, cases of sexual exploitation and abuse (SEA), sexual harassment (SH), and accidents that result in death or serious or multiple injury.\n\nFor SEA/SH incidents, the notification and any follow up reporting to the Bank shall be shared with the Bank’s corporate Grievance Redress Service (GRS).  For any other incidents or accidents, the notification and any follow up reporting to the Bank shall be shared with the Bank task team.\n\nUN Partner will report alleged incidents of SH involving UN Partner Staff and Non-Staff Personnel through its Office of Internal Audit and Investigations’ annual reports to the UN Partner Executive Board, which are made publicly available.\n\nb) Subsequently, provide a report to the Government and the Bank with sufficient detail regarding the scope, severity, and possible causes of the incident or accident, indicating immediate measures taken or that are planned to be taken to address it, and any information provided by any Consultant or Contractor, as appropriate.\n\nc) In cases where protection concerns or applicable UN Partner rules may limit such notification or information that can be reported to the Government, the UN Partner shall promptly only notify the Bank. After consultations with the Bank on the modalities, extent of information to be shared and timeline of the notification to the Government of the incident or accident, notify the Government accordingly. In exceptional cases, where protection concerns outweigh the benefit of such notification, or applicable UN Partner rules prohibit any sharing of information, the Bank and the UN Partner may agree to not notify the Government of that incident or accident.\n\nd) At the Bank’s request, share any Consultant or Contractor notification and report, redacted to remove Personal Data.\n\n\n\na) Notify the Government and the Bank no later than 48 hours after learning of the incident or accident.\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nb)         Provide the report on SEA/SH incidents within 10 days of the notification, and the report on any other incident or accident within 30 days of the notification. Depending on the circumstances, this timeframe may be extended in writing.\n\n\nc)         Following consultations and agreement with the Bank, notify the Government of the incident or accident, within [xx] days, unless otherwise agreed to with the Bank.\n\n\n\n\n\nd) Upon request, share Contractors’ and Implementing Partners’ notifications and reports to the Bank within 7 days following the Bank’s request, unless otherwise agreed to with the Bank.\n\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZGIxZjQxNWYtZWJjYS00M2E5LTliYTYtYWVjZWM4MmIzYzNi%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%228e20433e-62fc-42be-aa4d-03d96a3110a0%22%7d>\nMeeting ID: 224 102 242 752\nPasscode: 9dHc49\n________________________________\nDial in by phone\n+1 509-408-0991,,538192648#<tel:+15094080991,,538192648> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=538192648>\nPhone conference ID: 538 192 648#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 112 417 161 6\nMore info<https://www.webex.com/msteams?confid=1124171616&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=8e20433e-62fc-42be-aa4d-03d96a3110a0&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_ZGIxZjQxNWYtZWJjYS00M2E5LTliYTYtYWVjZWM4MmIzYzNi@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvnbQMAR",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 30, 10, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Kn08QIAR" },
                    new List<int> { 1646 },
                    new List<string> { "elizabethdu@unops.org", "christinebo@unops.org", "vineshw@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvnhwMAB",
                    new UNOPSInteraction
                    {
                        Name = "COP29: Bilateral Meeting: South Asia Cooperative Environment Programme (SACEP) Director-General, Norbu Wangchuk with Valerie Hickey",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-11-20").ToUniversalTime(),
                        Subject = "COP29: Bilateral Meeting: South Asia Cooperative Environment Programme (SACEP) Director-General, Norbu Wangchuk with Valerie Hickey",
                        Description = "Agenda;\n\nDiscussing the following topics, in addition to anything else that may be of interest to the World bank:\n•              DGstrategic vision for SACEP and concrete next steps on the PLEASE project\n•              The SACEP institutional capacity building assessment and updates to SACEP's core governing documents.\n\nVenue: Baku WBG COP29 Offices Acacia Meeting room at  World Bank Pavilion, Blue Zone\n\nParticipants:\n\nSACEP:\nSouth Asia Cooperative Environment Programme (SACEP) has a new Director-General (DG), Norbu Wangchuk\n\nWorld Bank:\n\n  *   Global Director, Valerie Hickey\n  *   Senior External Affairs Officer, Hannah McDonald-Moniz\n\nContact:\n\n  *   Hannah hmcdonaldmoniz@worldbank.org<mailto:hmcdonaldmoniz@worldbank.org>, +1 202 250 4498 (WB)\n  *   DGs assistant, Ms Priyankari Alexander - +94 71 992 1241 (SACEP)\n  *   Simonetta number (WhatsApp) is +66 63 273 4230 (UNOP)",
                        Location = "Baku WBG COP29 Offices (B40) Acacia (16) Private; / World Bank Pavilion, Blue Zone, Baku WBG COP29 Offices Acacia (16) Private",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvnhwMAB",
                        EmailAddresses = new List<string> { "hmcdonaldmoniz@worldbank.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 33, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7CVYIA3" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "simonettas@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvnjTMAR",
                    new UNOPSInteraction
                    {
                        Name = "Re: UN Coordination - World Bank SFA and direct financing  - update from UNICEF",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-01").ToUniversalTime(),
                        Subject = "Re: UN Coordination - World Bank SFA and direct financing  - update from UNICEF",
                        Description = "Dear colleagues,\nAs promised setting up a call for next week.\nKind regards\nAndrea\n\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_Njc0YjQ3NTEtMzRhZS00YmU3LWE2ZTItZGE3MWMwNWI3N2M1%40thread.v2/0?context=%7b%22Tid%22%3a%2277410195-14e1-4fb8-904b-ab1892023667%22%2c%22Oid%22%3a%22e8fbdf38-94a7-4e7c-a1ce-2c6717bac301%22%7d>\nMeeting ID: 317 434 303 920\nPasscode: d7arMp\n________________________________\nDial in by phone\n+1 347-343-2995,,38300229#<tel:+13473432995,,38300229#> United States, New York City\nFind a local number<https://dialin.teams.microsoft.com/48734644-d9b5-4906-a15b-79fd7a5f1272?id=38300229>\nPhone conference ID: 383 002 29#\nJoin on a video conferencing device\nTenant key: 840891842@t.plcm.vc\nVideo ID: 128 463 002 6\nMore info<https://dialin.plcm.vc/teams/?key=840891842&conf=1284630026>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=e8fbdf38-94a7-4e7c-a1ce-2c6717bac301&tenantId=77410195-14e1-4fb8-904b-ab1892023667&threadId=19_meeting_Njc0YjQ3NTEtMzRhZS00YmU3LWE2ZTItZGE3MWMwNWI3N2M1@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvnjTMAR",
                        EmailAddresses = new List<string> { "840891842@t.plcm.vc" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("clothildef@unops.org".ToLower()) ? paoUserEmailMapping["clothildef@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 24, 6, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MvyhCIAR" },
                    new List<int> { 1185 },
                    new List<string> { "clothildef@unops.org", "elizabethdu@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvoajMAB",
                    new UNOPSInteraction
                    {
                        Name = "Touchbase",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-24").ToUniversalTime(),
                        Subject = "Touchbase",
                        Description = "Christine's notes:\n\n- On the French SFA version - they translated the new SFA template that went up on website into French and Spanish. They haven't updated those templates, rather they plan to do updated official versions once negotiations conclude (i.e. November). Andy believes that for Haiti we should be using the old French SFA retrofitted with ESS language in French. I asked him for official translation of the new ESS language to prevent confusion, and also indicated that we will have ongoing need for updated French version (but likely not Spanish).\n\n- The guidance note from OPCS/ESF Directors went out already; I've again asked for copy. He suggests I write him an email on this, to help him make the case internally that shared guidance is important to UN partners. He said LEGEN has been providing constant stream of updates to WB country lawyers, so I think those folks are the most updated internally.\n\n- I talked about difficulties/delays on individual projects caused by incomplete information flow. Asked that the UNOPS agreed language (based on your email chain w Gamila) be included in the template that goes centrally to WB teams. \n\n- Gave him the talking points on the ESS Table based on the chat messages, told him we are making internal guidance for our teams but asked that also the main SFA Template include clear guidance to teams that unnecessary items should be deleted and that only obligations that flow to UNOPS (or whoever) be included.\n\n- Asked him about what sorts of project amendments can expect to need retrofitting with ESS language. For instance no-cost time extensions, additional finance, small scope of work changes, larger changes, etc. He thinks that amendments shouldn't need retrofitting. I emphasized importance of clarity on this point, given that it can significantly delay and cause relationship issues if we give one version to government which then gets bounced later by WB.\n\n- UNICEF is due to provide written comments to WB this week on the final SFA text. UNOPS and WFP are next on OPCS list for bilateral discussions. I told him we are close to being able to send comments - maybe we can even send on Friday?\n\n- Rajeev hasn't gotten traction with a finance meeting, I will write to nudge again. Similarly no movement following up E&S consultation, so he asked me to send him a message he can use to nudge colleagues.",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvoajMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 22, 41, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvpepMAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS / WB FCV KM",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-12").ToUniversalTime(),
                        Subject = "UNOPS / WB FCV KM",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvpepMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 31, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvq33MAB",
                    new UNOPSInteraction
                    {
                        Name = "WB-UNOPS: Urban development discussion",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-25").ToUniversalTime(),
                        Subject = "WB-UNOPS: Urban development discussion",
                        Description = "Notes:\n- WB asked about a pipeline opportunity related to a P4R refugee program in Kenya, through the IDA window for host communities and refugees. Mentioned a $15 M component where UNOPS could support.\n- WB said the governments in region sometimes complain about perceived high costs of UN implementation.\n- WB discussed opportunities for low-income housing (i.e. slum upgrading) in Kenya. After a few decades of disengagement on this issue, discussions between the WB and GoK now have substantial momentum.\n- WB planning to launch a low-income housing initiative in Somalia from July 2025. They are beginning stakeholder consultations now. Nandya will be the TTL.\n- WB had financed the Climate Action Plan for Mombasa and might now undertake an implementing project in flood management and nature-based solutions. Possible WB would then add WASH and housing in a phase 2.\n- WB said Japan is a major financier of GFDRR activities in FCV countries. He believes there will be new Japanese money coming into technical assistance in Africa on these topics. He mentioned the Japan Quality Infrastructure (QII) which finances project preparation activities to support better quality infrastructure, with money made available after the project concept is approved by the WB Board. \n\nHemas and Lukas agreed that experience with UNOPS on ECRP1 and ERP2 has been good. Hemas asked what UNOPS could do to improve its ability to deploy and retain staff during challenging moments in FCV countries. He wondered if there is any flexibility in our staffing structures.\n\nWB brought up an issue of payment delays in Puntland and Somaliland related to a political issue with FGS, which may recur. Something about MoF recognizing CTG consultants. Suggested we might discuss with Keiko and Kristina.\n\nSoraya is based in DC, focal point for FCV knowledge management in URL. Mentioned an upcoming February Forum (on Urban?) which may have an FCV angle.",
                        Location = "The United Nations Office for Project Services, UNOPS, Nairobi, NAI-G-Meeting Room (10) [Google Meet], NAI-B-Conference room (30) [Google Meet]",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvq33MAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 34, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7fxJIAR" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "sharonle@unops.org", "fumiea@unops.org", "francescap@unops.org", "mewaelk@unops.org", "alaa@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvr3vMAB",
                    new UNOPSInteraction
                    {
                        Name = "Discussion with UNOPS on SEA/SH Reporting",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-30").ToUniversalTime(),
                        Subject = "Discussion with UNOPS on SEA/SH Reporting",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZTJhMWMzYzAtMmEzOC00OTU2LThlYjQtZjMyNTU4M2ZlZWVk%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22863eca3b-25e6-4461-9387-398aa171590b%22%7d>\nMeeting ID: 277 901 445 451\nPasscode: 8CKZSJ\n________________________________\nDial in by phone\n+1 509-408-0991,,624595485#<tel:+15094080991,,624595485> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=624595485>\nPhone conference ID: 624 595 485#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 111 521 547 4\nMore info<https://www.webex.com/msteams?confid=1115215474&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=863eca3b-25e6-4461-9387-398aa171590b&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_ZTJhMWMzYzAtMmEzOC00OTU2LThlYjQtZjMyNTU4M2ZlZWVk@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "MC 10-605 (25) VC",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvr3vMAB",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 27, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvr3wMAB",
                    new UNOPSInteraction
                    {
                        Name = "Meeting: Rainer, UNOPS - Amit, WB",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-11-14").ToUniversalTime(),
                        Subject = "Meeting: Rainer, UNOPS - Amit, WB",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_OGUwMGViOTAtMTNjNy00MzQ2LWI4YTMtMzk5NTMxNzI4ZjYw%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22211ce42b-7587-49af-a6ff-11da59216dcc%22%7d>\nMeeting ID: 297 772 240 032\nPasscode: 5CXqvw\n________________________________\nDial in by phone\n+1 509-408-0991,,787957920#<tel:+15094080991,,787957920> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=787957920>\nPhone conference ID: 787 957 920#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 114 753 994 9\nMore info<https://www.webex.com/msteams?confid=1147539949&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=211ce42b-7587-49af-a6ff-11da59216dcc&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_OGUwMGViOTAtMTNjNy00MzQ2LWI4YTMtMzk5NTMxNzI4ZjYw@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvr3wMAB",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 32, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7Md9IAF" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "rainerf@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvs4qMAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS / FAO catch up",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-10-24").ToUniversalTime(),
                        Subject = "UNOPS / FAO catch up",
                        Description = null,
                        Location = "World Bank MC Building",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvs4qMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 26, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1244 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvsb6MAB",
                    new UNOPSInteraction
                    {
                        Name = "Confirmed: WB SFAs - Joint UN approach on SEA/SH reporting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-31").ToUniversalTime(),
                        Subject = "Confirmed: WB SFAs - Joint UN approach on SEA/SH reporting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvsb6MAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 28, 52, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MvyhCIAR" },
                    new List<int> { 1244 },
                    new List<string> { "christinebo@unops.org", "kelleys@unops.org", "elizabethdu@unops.org", "alistairs@unops.org", "nivesc@unops.org", "juliasc@unops.org", "vineshw@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvu1lMAB",
                    new UNOPSInteraction
                    {
                        Name = "Re: Catch-up on WB ESS requirements on upcoming Sudan and Yemen direct financing projects",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-09-18").ToUniversalTime(),
                        Subject = "Re: Catch-up on WB ESS requirements on upcoming Sudan and Yemen direct financing projects",
                        Description = "Dear Elizabeth,\nSending invite to all on the email thread.\nKind regards\nAndrea\nFrom: Elizabeth DUBY <elizabethdu@unops.org>\nDate: Tuesday, September 17, 2024 at 08:21\nTo: Andrea Suley <asuley@unicef.org>\nCc: KEBE, Mouhamadou Amine <kebea@who.int>, Christine BOWERS <christinebo@unops.org>, Vinesh Winodan <vineshw@unops.org>, Alice Susannah Vickers <avickers@unicef.org>, Jessica Rennie <jrennie@unicef.org>\nSubject: Re: Catch-up on WB ESS requirements on upcoming Sudan and Yemen direct financing projects\nDear Andrea,\n\nMy name is Elizabeth, and I’m the UNOPS legal advisor working on WB direct financing negotiations. My colleague Vinesh Winodan and I are available to meet with you tomorrow Wednesday at 11:30 EST.\n\nPlease feel free to share the invite. We’re very pleased to have the opportunity to discuss this and ensure we are as aligned as possible.\n\nLooking forward to our conversation.\n\nBest regards,\nElizabeth\n\nElizabeth Duby | Legal Advisor and Commercial Team Lead (Americas) | Legal Group | Washington D.C. | Mob: +1 (202) 391-2817  | IPAS Solution Portal<https://tasks.unops.org/servicedesk/customer/portal/1>  | www.unops.org<https://www.unops.org/english/Pages/Home.aspx> |\n\nKeep up-to-date with UNOPS. Follow us on Facebook<https://www.facebook.com/unops.org/>, LinkedIn<https://www.linkedin.com/company/unops>, Twitter<https://twitter.com/unops>, Instagram<https://www.instagram.com/unops_official/?hl=sv>, YouTube<https://www.youtube.com/user/UNOPSofficial> Subscribe to our external newsletter in English<https://confirmsubscription.com/h/r/28CBB1F85AE31216>, French<https://confirmsubscription.com/h/r/5BC15A59F87CC82D> or Spanish<https://confirmsubscription.com/h/r/1E87E822D07D72F0>\n\n\n\nOn Tue, 17 Sep 2024 at 02:30, Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>> wrote:\nDear UNOPS and WHO colleagues,\nI am reaching out to connect with you in relation to some of the upcoming WB direct financing project negotiations we are jointly implementing.\n\nUNICEF has made some progress with WB to agree on a variety of ESS matters. I would like to brief you.\n\nWould you be available tomorrow at 11h30 EST?  Or Wednesday at 7h EST?\n\nKind regards\nAndrea\n\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZmRiZjI5Y2MtODkwYy00YzVjLTg5NzktN2NjNzA3NTg1Mjlj%40thread.v2/0?context=%7b%22Tid%22%3a%2277410195-14e1-4fb8-904b-ab1892023667%22%2c%22Oid%22%3a%22e8fbdf38-94a7-4e7c-a1ce-2c6717bac301%22%7d>\nMeeting ID: 330 910 441 341\nPasscode: uGKDwH\n________________________________\nDial in by phone\n+1 347-343-2995,,827791540#<tel:+13473432995,,827791540#> United States, New York City\nFind a local number<https://dialin.teams.microsoft.com/48734644-d9b5-4906-a15b-79fd7a5f1272?id=827791540>\nPhone conference ID: 827 791 540#\nJoin on a video conferencing device\nTenant key: 840891842@t.plcm.vc\nVideo ID: 129 963 364 0\nMore info<https://dialin.plcm.vc/teams/?key=840891842&conf=1299633640>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=e8fbdf38-94a7-4e7c-a1ce-2c6717bac301&tenantId=77410195-14e1-4fb8-904b-ab1892023667&threadId=19_meeting_ZmRiZjI5Y2MtODkwYy00YzVjLTg5NzktN2NjNzA3NTg1Mjlj@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvu1lMAB",
                        EmailAddresses = new List<string> { "christinebo@unops.org", "840891842@t.plcm.vc", "elizabethdu@unops.org", "kebea@who.int", "jrennie@unicef.org", "avickers@unicef.org", "vineshw@unops.org", "asuley@unicef.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 21, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1185 },
                    new List<string> { "elizabethdu@unops.org", "christinebo@unops.org", "vineshw@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Cvu50MAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Discussion  on Financial Terms in SFAs",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-10").ToUniversalTime(),
                        Subject = "UNOPS Discussion  on Financial Terms in SFAs",
                        Description = "Please share this with the relevant UNOPS colleagues in Copenhagen\n\n\n\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NWIzYTZkM2YtYTRiZi00OTUzLTk1ZTMtNDJhMjcwZjIzOTBl%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%2288c9c3e6-fae3-4731-b4cf-58e83bf4f2d2%22%7d>\nMeeting ID: 245 551 091 830\nPasscode: ZxhBhM\n________________________________\nDial in by phone\n+1 509-408-0991,,792818723#<tel:+15094080991,,792818723#> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=792818723>\nPhone conference ID: 792 818 723#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 113 791 992 8\nMore info<https://www.webex.com/msteams?confid=1137919928&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=88c9c3e6-fae3-4731-b4cf-58e83bf4f2d2&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NWIzYTZkM2YtYTRiZi00OTUzLTk1ZTMtNDJhMjcwZjIzOTBl@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvu50MAB",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 25, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvuEfMAJ",
                    new UNOPSInteraction
                    {
                        Name = "Coffee Kirstine / Christine / Maria",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2024-09-26").ToUniversalTime(),
                        Subject = "Coffee Kirstine / Christine / Maria",
                        Description = null,
                        Location = "Compass Coffee, 1703 H St NW, Washington, DC 20006, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvuEfMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kirstined@unops.org".ToLower()) ? paoUserEmailMapping["kirstined@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 23, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HxyhbIAB" },
                    new List<int> { 1646 },
                    new List<string> { "kirstined@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvuWPMAZ",
                    new UNOPSInteraction
                    {
                        Name = "LEGEN-UNOPS informal discussion",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-10-16").ToUniversalTime(),
                        Subject = "LEGEN-UNOPS informal discussion",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvuWPMAZ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 25, 50, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Kn08QIAR" },
                    new List<int> { 1646 },
                    new List<string> { "elizabethdu@unops.org", "christinebo@unops.org", "vineshw@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvvaYMAR",
                    new UNOPSInteraction
                    {
                        Name = "Meeting Hiba Tahboub, WB Chief Procurement Officer",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-19").ToUniversalTime(),
                        Subject = "Meeting Hiba Tahboub, WB Chief Procurement Officer",
                        Description = "WB Procurement is undergoing a change program with 4 pillars:\n1. Bringing more quality\nThe usage of rated criteria has been mandatory on all international WB tenders since Sept 2023. While the level of compliance has been good from a 'tick-the-box' perspective, it's also been very weakly implemented from a quality point of view. Limited depth in terms of usage of the rated criteria or weighting of them in tenders.\nWB is looking to tweak their internal regulations to drive more uptake of quality considerations in procurement.\n\n2. Early market engagement to promote a wider range of suppliers, particularly with respect to higher-technology and innovative approaches\nHiba asked how UNOPS does early market engagement. I told her best to speak w PG on this.\nWB is looking to diversify their supplier base, and do early market engagement at the strategic/sectoral level. She gave an example of a (hypothetical) WB program that would require steel towers for transmission lines - how can WB get better advance information to the market for something like this.\nHiba also asked whether UNOPS looks at innovations like joint ventures or whether we are able to impact the jobs agenda by how we are selecting contractors. I told her that I don't think we have a lot to do with joint ventures, and indicated that our linkage to the jobs agenda is primarily through cash-for-work.\n\n3. Aggregated procurement to achieve value for money\nHiba asked for good practices from UNOPS on aggregating procurement, as she hopes to learn from us. I briefly mentioned UN Web Buy in this context but suggested this would be good to discuss with PG.\nI also referred to the ongoing discussion with WB about the need for pooled procurements in the Pacific, and updated her on Jorge's conversation related to this with EAP RVP Manuela Ferro on the sidelines of UNGA high level week in fall 2024.\n\n4. Organizing themselves to deliver\nThis is a fresh look at the WB organizational set-up for procurement. She did mention that it would also look at coordinating capacity strengthening efforts for clients - how to better structure and monitor these.\nBuilding on the latter point, I asked Hiba about the IEG report, specifically on broader questions of how they are changing approach on procurement capacity building. I also asked about the 'PIU Academy' idea I've heard floating around. In response:\nHiba believes the finding that there is less procurement capacity building being delivered in countries with low capacity may not be accurate. She thinks there is an issue of data quality, such as poor capturing of the number of participants in lower capacity settings.\nLooking ahead, WB will expand on a structured approach to building capacity in procurement, by continuing and expanding support to existing national institutes with a procurement training program. She referred to a recent 5-day sustainable procurement training with a Ghanaian institute (targeted national procurement officials across West Africa) as an example of how they will deliver.\nHiba asked about how we measure success and monitor what we are doing when it comes to capacity building.\nShe indicated HIES (hands on implementation expanded support) will be delivered more in FCV looking ahead [this is basically when WB plays a much more direct role in PIU procurement].\nHiba said that OPCS likely to expand on its usual practice (having a deep orientation session at project launch for the PIU) by moving towards also delivering a twice a year refresher training in each country, targeting all PIUS and also conducting market engagement at the same time.\n\nI commented on how much we heard about procurement at the WB Annual Meetings, and she mentioned that WBG President Ajay Banga thinks procurement is hugely important, btw.\n\nA random question also was on China and solar panels, asking whether we have managed to source any panels elsewhere. I told her I don't know the answer, but I know this has been a topic of discussion within UNOPS (also waste from solar panels at end of life).",
                        Location = "Microsoft Teams Meeting; MC 10-348 (25) (VC) Private, MC 10-348 (25) (VC) Private",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvvaYMAR",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 37, 19, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000LgnrRIAR" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvvvVMAR",
                    new UNOPSInteraction
                    {
                        Name = "WB SFA - November Version",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-06").ToUniversalTime(),
                        Subject = "WB SFA - November Version",
                        Description = "Dear colleagues,\n\nFurther to the below, I am blocking this slot in our calendars to briefly connect next week and compare notes on the revised SFA.\nIt will be extremely useful to hear your views, thus I hope this time works for all. If not, please do feel free to suggest an alternative one.\n\nBest regards,\nIrene\n________________________________________________________________________________­­­­\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_Y2FlNzkwMTQtMDMwYS00ZDM0LTgwOTUtZDhjYzQ2NDAwM2M3%40thread.v2/0?context=%7b%22Tid%22%3a%22163ac468-abb8-44d0-81fd-d9db15e3af96%22%2c%22Oid%22%3a%22df2a60ad-26bc-4ab4-9eb1-11cde433e2d8%22%7d>\nMeeting ID: 376 357 766 136\nPasscode: XU2Uz2ou\n________________________________\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=df2a60ad-26bc-4ab4-9eb1-11cde433e2d8&tenantId=163ac468-abb8-44d0-81fd-d9db15e3af96&threadId=19_meeting_Y2FlNzkwMTQtMDMwYS00ZDM0LTgwOTUtZDhjYzQ2NDAwM2M3@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________\n\n\n_____________________________________________\nFrom: Spaziani, Irene (PSR)\nSent: Monday, November 25, 2024 2:29 PM\nTo: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>; Rawad ASSAAD <rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>>; Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>>\nCc: Lukic, Meran (PSR) <Meran.Lukic@fao.org<mailto:Meran.Lukic@fao.org>>; alistairs@unops.org<mailto:alistairs@unops.org>; Tanya Belcheva <tbelcheva@unicef.org<mailto:tbelcheva@unicef.org>>; Jessica Rennie <jrennie@unicef.org<mailto:jrennie@unicef.org>>; MPC.CONTRACTING <mpc.contracting@wfp.org<mailto:mpc.contracting@wfp.org>>; Elizabeth DUBY <elizabethdu@unops.org<mailto:elizabethdu@unops.org>>\nSubject: RE: WB SFA - November Version\n\nDear colleagues,\n\nMany thanks for your prompt and useful response to the below.\n\nIt is reassuring that we were all equally surprised by the fact that this template was published online before individual consultations with each agency were concluded (and, in our case, even started). So far, it is extremely useful to know that none of us agreed to use the November template as is. We are also making the case for continuing to use the old SFA with the retrofitted new safeguards language, but it is not clear how long OPCS will allow for this flexibility, which they have been indicating from the start that it is quite limited. Based on the below, I think it would be useful to organize a call during the second half of next week to assess whether there are any common red lines around which we can join forces or at least consult upon. If you agree, would December 5 or 6 at 3pm (Rome)/ 9am (DC) be good options? If not, please do feel free to suggest alternative slots.\n\nThanks again and best regards,\nIrene\n\nPS Happy Thanksgiving week to those who celebrate it! 😊\n\n\nFrom: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>\nSent: Monday, November 25, 2024 2:11 PM\nTo: Rawad ASSAAD <rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>>\nCc: Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>>; Spaziani, Irene (PSR) <Irene.Spaziani@fao.org<mailto:Irene.Spaziani@fao.org>>; Lukic, Meran (PSR) <Meran.Lukic@fao.org<mailto:Meran.Lukic@fao.org>>; alistairs@unops.org<mailto:alistairs@unops.org>; Tanya Belcheva <tbelcheva@unicef.org<mailto:tbelcheva@unicef.org>>; Jessica Rennie <jrennie@unicef.org<mailto:jrennie@unicef.org>>; MPC.CONTRACTING <mpc.contracting@wfp.org<mailto:mpc.contracting@wfp.org>>; Elizabeth DUBY <elizabethdu@unops.org<mailto:elizabethdu@unops.org>>\nSubject: Re: WB SFA - November Version\n\nDear colleagues,\n\nThanks for the useful information-sharing on this one. Like other agencies, UNOPS didn't receive a draft document before it was published. I had asked OPCS several times to share with us first, to prevent a repeat of the miscommunications we have had with governments since the July template was published. So I share your frustrations.\n\nWhile we had received some deliberative text for review and comment, UNOPS is not yet able to agree to all aspects of the new template. Our concerns regarding financial reporting and related topics are addressed in the new template. However, there remain a few points related to the key ESS clause which we would like to see changed, as well as continued difficulties in agreeing to the SEA/SH reporting requirements. So our guidance internally is that UNOPS cannot use the November template, and I've indicated the same to OPCS.\n\nUNOPS would be very pleased to engage in a UN family discussion and agree on the most important adjustments to the template, which we could collectively share with the Bank for negotiation. I am traveling this week (and Alistair is on leave), but in the meantime I am adding my DC-based legal colleague Elizabeth Duby who is entirely updated on the UNOPS position. She would be available this week for a discussion.\n\nIf we come together, I believe we will have much more success in moving on red lines.\n\nWarm regards\nChristine\n\nOn Mon, Nov 25, 2024 at 11:25 AM Rawad ASSAAD <rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>> wrote:\nDear Irene, Andrea and Colleagues,\n\nFor WFP, this is the first time we are seeing the new template without having the opportunity to provide feedback. We had understood from the Bank that the new template would be shared with UN agencies prior to its public release. It was therefore surprising to see it published without any formal or informal consultation.\n\nCurrently, we do not have any projects in the pipeline that would need to conform to this new template. However, we have several projects in the negotiation phase, for which we understand the old template with retrofitted ESS language would apply.\n\nWe are still consulting internally on several elements. It might be beneficial to schedule a call at some point to discuss and align on positions, particularly if there are any red lines we need to push back on.\n\nBest,\nRawad\n\nRawad Assaad\nStrategic Partnerships Officer | HG & IFI Negotiations and Contracting\nPartnerships and Innovation Department (PI)\nMultilateral & Programme Country Partnerships (MPC)\nWorld Food Programme – Rome, Italy\nP.  +39 06 6513 2832    M.  +39 347 182 1400\nEmail: rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>\n[signature_4263389815]\n\nFrom: Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>>\nSent: Friday, November 22, 2024 9:32 PM\nTo: Spaziani, Irene (PSR) <Irene.Spaziani@fao.org<mailto:Irene.Spaziani@fao.org>>; Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>; Rawad ASSAAD <rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>>\nCc: Lukic, Meran (PSR) <Meran.Lukic@fao.org<mailto:Meran.Lukic@fao.org>>; alistairs@unops.org<mailto:alistairs@unops.org>; Tanya Belcheva <tbelcheva@unicef.org<mailto:tbelcheva@unicef.org>>; Jessica Rennie <jrennie@unicef.org<mailto:jrennie@unicef.org>>\nSubject: Re: WB SFA - November Version\n\nPlease be careful when opening emails that originate from outside WFP\nDear Irene,\nWe sent through comments to WB on the July template and had a couple of follow-up discussions with them. However, we never saw the November template until it was shared was us by Andy upon it being  published.\n\nWe have not used the new SFA yet and feel we will not use them for another bit for two reasons:\n\n  1.  We need to finish internal stock take on whether we can live with the template though we see much of the feedback we sent on the July template incorporated.\n  2.  We need a couple of points of clarification from WB to inform internal guidance to our offices. There is a need for us to update orientation/training materials and guidance notes for our staff on the new template given the changes introduced.\n\nKind regards\nAndrea\n\n\n\nFrom: Spaziani, Irene (PSR) <Irene.Spaziani@fao.org<mailto:Irene.Spaziani@fao.org>>\nDate: Friday, November 22, 2024 at 14:50\nTo: Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>>, Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>, Rawad ASSAAD <rawad.assa",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvvvVMAR",
                        EmailAddresses = new List<string> { "christinebo@unops.org", "mpc.contracting@wfp.org", "elizabethdu@unops.org", "alistairs@unops.org", "rawad.assaad@wfp.org", "irene.spaziani@fao.org", "jrennie@unicef.org", "meran.lukic@fao.org", "asuley@unicef.org", "tbelcheva@unicef.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 15, 35, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7gGoIAJ" },
                    new List<int> { 1244 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CwJcsMAF",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with UNOPS Middle East Senior Regional Advisor",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-08").ToUniversalTime(),
                        Subject = "Meeting with UNOPS Middle East Senior Regional Advisor",
                        Description = "Meetings notes: https://docs.google.com/document/d/1prll0O8ntYembN3VX0lICWXhvXPKv7AMDzJ7meG6YkM/edit?tab=t.0#heading=h.3ru83nvlpyg",
                        Location = "Microsoft Teams Meeting; J 6-160 (19) (VC), J 6-133 (8) (VC) Private, J 6-160 (19) (VC)",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CwJcsMAF",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("marijab@unops.org".ToLower()) ? paoUserEmailMapping["marijab@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 9, 20, 20, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "marijab@unops.org", "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000D5ebgMAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/Norge - catch-up forud for EB",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-17").ToUniversalTime(),
                        Subject = "UNOPS/Norge - catch-up forud for EB",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000D5ebgMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 15, 13, 47, 20, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1136 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000D5nbhMAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS / Norge",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-17").ToUniversalTime(),
                        Subject = "UNOPS / Norge",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MDMxOGU1ZDQtZTRmMC00NzMwLWIwZjMtOGE5ZmI3MTQ4NTYy%40thread.v2/0?context=%7b%22Tid%22%3a%22bb0f0b4e-4525-4e4b-ba50-1e7775a8fd2e%22%2c%22Oid%22%3a%22d755301c-a813-4c56-91fe-db17670b4f48%22%7d>\nMeeting ID: 377 640 863 161\nPasscode: Pg32Rd6h\n________________________________\nDial in by phone\n+47 21 40 24 25,,331167598#<tel:+4721402425,,331167598> Norway, Oslo\nFind a local number<https://dialin.teams.microsoft.com/7995c086-b51b-4979-9262-473a18f2298f?id=331167598>\nPhone conference ID: 331 167 598#\nJoin on a video conferencing device\nTenant key: teams@vcs.mfa.no\nVideo ID: 123 254 840 4\nMore info<https://videomeet.mfa.no/teams/?conf=1232548404&ivr=teams&d=vcs.mfa.no&prefix=teams.&w>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=d755301c-a813-4c56-91fe-db17670b4f48&tenantId=bb0f0b4e-4525-4e4b-ba50-1e7775a8fd2e&threadId=19_meeting_MDMxOGU1ZDQtZTRmMC00NzMwLWIwZjMtOGE5ZmI3MTQ4NTYy@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000D5nbhMAB",
                        EmailAddresses = new List<string> { "teams@vcs.mfa.no" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 15, 13, 53, 19, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YWi0oIAD" },
                    new List<int> { 1102 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000D7OxOMAV",
                    new UNOPSInteraction
                    {
                        Name = "CEB mv.",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-01-17").ToUniversalTime(),
                        Subject = "CEB mv.",
                        Description = null,
                        Location = "Ministry of Foreign Affairs of Denmark, Asiatisk Pl. 2, 1448 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000D7OxOMAV",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 16, 10, 46, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000OKqawIAD" },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DAIDtMAP",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS & USAID Meeting on Ukraine",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-01-15").ToUniversalTime(),
                        Subject = "UNOPS & USAID Meeting on Ukraine",
                        Description = "USAID will be meeting in-person with Elene Algadze, United Nations Office for Project Services (UNOPS) partnerships lead based in Kyiv to understand UN activities in Ukraine (virtual link provided for those unable to attend in-person).<br><br>UNOPS has been actively involved in crisis response efforts in Ukraine, focusing on providing essential support in areas such as infrastructure, healthcare, and emergency response. The organization has worked on repairing critical infrastructure, including roads and schools, and has assisted in the procurement and delivery of medical supplies. Additionally, UNOPS has supported displaced populations by helping to establish temporary shelters and ensuring the continued delivery of vital services across conflict-affected regions.<br><br><b>Room: 4th floor, <span>US-RRB-ConfRm-4C.09-04-MRm-USAID</span></b>",
                        Location = "Ronald Reagan Building and International Trade Center, 1300 Pennsylvania Avenue NW, Washington, DC 20004, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DAIDtMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 17, 15, 46, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1116 },
                    new List<string> { "alistairs@unops.org", "eleneag@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DAILxMAP",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS - WB Urban discussion",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-01-16").ToUniversalTime(),
                        Subject = "UNOPS - WB Urban discussion",
                        Description = null,
                        Location = "World Bank MC Building Visitor Entrance, 752 18th St NW, Washington, DC 20006, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DAILxMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 17, 15, 47, 23, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Yght0IAB" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "eleneag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DCPxgMAH",
                    new UNOPSInteraction
                    {
                        Name = "Ingegerd Nordin - IFAD / CODEWAY2025",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-01-17").ToUniversalTime(),
                        Subject = "Ingegerd Nordin - IFAD / CODEWAY2025",
                        Description = null,
                        Location = "E207 - Food and Agriculture Organization of the United Nations, Viale delle Terme di Caracalla, 00153 Roma RM, Italy",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DCPxgMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 20, 9, 4, 12, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YVwCGIA1" },
                    new List<int> {  },
                    new List<string> { "martina@unops.org", "jeromedt@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DD5yVMAT",
                    new UNOPSInteraction
                    {
                        Name = "UN-Habitat transition from UNDP",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2024-12-06").ToUniversalTime(),
                        Subject = "UN-Habitat transition from UNDP",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MzU3ZTg1NWQtNGVjZS00ZTUxLTliZDMtYmZjYzU5NDIxNWY1%40thread.v2/0?context=%7b%22Tid%22%3a%220f9e35db-544f-4f60-bdcc-5ea416e6dc70%22%2c%22Oid%22%3a%22ecbcb7c9-38af-456a-a492-e9c443fdcd70%22%7d>\nMeeting ID: 381 488 466 810\nPasscode: DQ9Bt3cS\n________________________________\nJoin on a video conferencing device\nTenant key: unitevc@m.webex.com\nVideo ID: 128 122 078 1\nMore info<https://www.webex.com/msteams?confid=1281220781&tenantkey=unitevc&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=ecbcb7c9-38af-456a-a492-e9c443fdcd70&tenantId=0f9e35db-544f-4f60-bdcc-5ea416e6dc70&threadId=19_meeting_MzU3ZTg1NWQtNGVjZS00ZTUxLTliZDMtYmZjYzU5NDIxNWY1@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DD5yVMAT",
                        EmailAddresses = new List<string> { "unitevc@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("kajsah@unops.org".ToLower()) ? paoUserEmailMapping["kajsah@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 20, 12, 53, 22, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Yr2rOIAR" },
                    new List<int> { 1193 },
                    new List<string> { "kajsah@unops.org", "laurentium@unops.org", "lorrainea@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DFAPNMA5",
                    new UNOPSInteraction
                    {
                        Name = "Intro: Finland / UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-23").ToUniversalTime(),
                        Subject = "Intro: Finland / UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DFAPNMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 21, 12, 56, 6, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YQDnqIAH" },
                    new List<int> { 1087 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DFOSTMA5",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS - FAO UNWebBuy+ MoU",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-01-21").ToUniversalTime(),
                        Subject = "UNOPS - FAO UNWebBuy+ MoU",
                        Description = null,
                        Location = "India Room A-237",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DFOSTMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 21, 14, 43, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1244 },
                    new List<string> { "martina@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DLOUVMA5",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with ECHO on UNOPS work in Palestine",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-24").ToUniversalTime(),
                        Subject = "Meeting with ECHO on UNOPS work in Palestine",
                        Description = null,
                        Location = "Online",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DLOUVMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 24, 14, 30, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1029 },
                    new List<string> { "mariacarmenco@unops.org", "marijab@unops.org", "laetitiak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DM813MAD",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Washington <> World Bank Geneva",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-24").ToUniversalTime(),
                        Subject = "UNOPS Washington <> World Bank Geneva",
                        Description = "Notes: \n\nChristine updated on UNOPS corporate plans, including the UN-PBSO workshop in Nairobi, Client Board, Helsingor Dialogue and ED’s plan to visit Washington for the Spring Meetings.\n\nMaria updated on recent WBG External and Corporate Relations reorganisation to strengthen cooperation between the teams representing the Bank to the UN in New York and in Geneva. Geneva office hosts between 30 and 40 events and dialogues per year to launch reports; partner with think tanks; and discuss ideas from proven solutions and impact evaluations to explore operational partnerships. \n\nWB also addressed UNOPS ED’s chapter in a new Center for Global Development paper on international development architecture reform. The piece mentions the Bretton Woods Institutions and needs for reform.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DM813MAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 24, 22, 55, 16, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000HxyhbIAB" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DTv1pMAD",
                    new UNOPSInteraction
                    {
                        Name = "WB SFA - November Version",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-30").ToUniversalTime(),
                        Subject = "WB SFA - November Version",
                        Description = "Dear colleagues,\nFurther to the below, I am blocking this slot in our calendars for a call next week.\nIf this time does not work for many, please do let me know.\nMany thanks and looking forward to re-connecting soon.\nBest regards,\nIrene\n\n________________________________________________________________________________­­­­\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_Y2FlNzkwMTQtMDMwYS00ZDM0LTgwOTUtZDhjYzQ2NDAwM2M3%40thread.v2/0?context=%7b%22Tid%22%3a%22163ac468-abb8-44d0-81fd-d9db15e3af96%22%2c%22Oid%22%3a%22df2a60ad-26bc-4ab4-9eb1-11cde433e2d8%22%7d>\nMeeting ID: 376 357 766 136\nPasscode: XU2Uz2ou\n________________________________\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=df2a60ad-26bc-4ab4-9eb1-11cde433e2d8&tenantId=163ac468-abb8-44d0-81fd-d9db15e3af96&threadId=19_meeting_Y2FlNzkwMTQtMDMwYS00ZDM0LTgwOTUtZDhjYzQ2NDAwM2M3@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________\n\n_____________________________________________\nFrom: Spaziani, Irene (PSR)\nSent: Wednesday, January 22, 2025 5:18 PM\nTo: Christine BOWERS <christinebo@unops.org>; Rawad ASSAAD <rawad.assaad@wfp.org>; Andrea Suley <asuley@unicef.org>; alistairs@unops.org; Tanya Belcheva <tbelcheva@unicef.org>; Jessica Rennie <jrennie@unicef.org>; MPC.CONTRACTING <mpc.contracting@wfp.org>; Elizabeth DUBY <elizabethdu@unops.org>; Adriana BONOMO <adriana.bonomo@wfp.org>; PUENTES Andrea Carolina <apuentes@iom.int>; Micol MULON <micol.mulon@wfp.org>\nCc: Lukic, Meran (PSR) <Meran.Lukic@fao.org>; Teunissen, Jorge (LEGA) <Jorge.Teunissen@fao.org>; Ibares, Alvaro (LEGA) <Alvaro.Ibares@fao.org>; SanchezUgalde, Camila (PSR) <Camila.SanchezUgalde@fao.org>\nSubject: RE: WB SFA - November Version\n\nDear colleagues,\n\nI hope all is well on your end and that the year is off to a good start!\n\nI heard bilaterally from some of you and it looks like interactions with the Bank are continuing to move forward with some progress. I believe it is therefore timely to finalize the [​docx icon]  table of issues<https://unfao.sharepoint.com/:w:/s/PSR/EdfI75Q1HZNHnfNQsgDhkmEBFPGPnyHuQOohLFI4kYX8Iw> and to reconvene soon to agree on next steps.\n\nIn thanking those of you who have already contributed to it, I kindly ask those who haven’t or who wish to add further updates to please do so by this Friday. In terms of our next meeting, would either next Tuesday 28 or Thursday 30 January at 3pm (Rome) / 9am (DC) work for most of you?\n\nMany thanks and looking forward to re-connecting soon.\n\nBest regards,\nIrene\n\n\nIrene Spaziani\nDonor Relations and Contracts Officer\nResource Mobilization Division (PSR)\nFood and Agriculture Organization of United Nations (FAO)\nTel: (+39) 06 570 52236\nS4B: irene.spaziani@fao.org<mailto:irene.spaziani@fao.org>\nwww.fao.org<http://www.fao.org/>\n\n\nFrom: Spaziani, Irene (PSR)\nSent: Tuesday, December 17, 2024 10:38 PM\nTo: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>; Rawad ASSAAD <rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>>; Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>>; alistairs@unops.org<mailto:alistairs@unops.org>; Tanya Belcheva <tbelcheva@unicef.org<mailto:tbelcheva@unicef.org>>; Jessica Rennie <jrennie@unicef.org<mailto:jrennie@unicef.org>>; MPC.CONTRACTING <mpc.contracting@wfp.org<mailto:mpc.contracting@wfp.org>>; Elizabeth DUBY <elizabethdu@unops.org<mailto:elizabethdu@unops.org>>; Adriana BONOMO <adriana.bonomo@wfp.org<mailto:adriana.bonomo@wfp.org>>; PUENTES Andrea Carolina <apuentes@iom.int<mailto:apuentes@iom.int>>; Micol MULON <micol.mulon@wfp.org<mailto:micol.mulon@wfp.org>>\nCc: Lukic, Meran (PSR) <Meran.Lukic@fao.org<mailto:Meran.Lukic@fao.org>>; Teunissen, Jorge (LEGA) <Jorge.Teunissen@fao.org<mailto:Jorge.Teunissen@fao.org>>; Ibares, Alvaro (LEGA) <Alvaro.Ibares@fao.org<mailto:Alvaro.Ibares@fao.org>>\nSubject: RE: WB SFA - November Version\n\nDear colleagues,\n\nHaving exchanged with a few of you, I am writing to extend the deadline for feedback to after the holidays, also noting the Bank’s upcoming closure. Grateful if you could please share your comments by 4 January.\nUntil then, I take this opportunity to wish everyone a restful break.\n\nBest regards,\nIrene\n\n\nFrom: Spaziani, Irene (PSR)\nSent: Wednesday, December 11, 2024 5:31 PM\nTo: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>; Rawad ASSAAD <rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>>; Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>>; alistairs@unops.org<mailto:alistairs@unops.org>; Tanya Belcheva <tbelcheva@unicef.org<mailto:tbelcheva@unicef.org>>; Jessica Rennie <jrennie@unicef.org<mailto:jrennie@unicef.org>>; MPC.CONTRACTING <mpc.contracting@wfp.org<mailto:mpc.contracting@wfp.org>>; Elizabeth DUBY <elizabethdu@unops.org<mailto:elizabethdu@unops.org>>; Adriana BONOMO <adriana.bonomo@wfp.org<mailto:adriana.bonomo@wfp.org>>; PUENTES Andrea Carolina <apuentes@iom.int<mailto:apuentes@iom.int>>; Micol MULON <micol.mulon@wfp.org<mailto:micol.mulon@wfp.org>>\nCc: Lukic, Meran (PSR) <Meran.Lukic@fao.org<mailto:Meran.Lukic@fao.org>>; Teunissen, Jorge (LEGA) <Jorge.Teunissen@fao.org<mailto:Jorge.Teunissen@fao.org>>; Ibares, Alvaro (LEGA) <Alvaro.Ibares@fao.org<mailto:Alvaro.Ibares@fao.org>>\nSubject: RE: WB SFA - November Version\n\nDear colleagues,\n\nMany thanks once again for you time and for the constructive discussion last Friday.\n\nAs agreed, I am sharing with you a draft “[​docx icon] table of issues<https://unfao.sharepoint.com/:w:/s/PSR/EdfI75Q1HZNHnfNQsgDhkmEBFPGPnyHuQOohLFI4kYX8Iw>” that is aimed at capturing some of the ones we discussed last week. Please note that this is just an initial draft, which I kindly ask you to please populate with further information/additional issues identified on your respective ends. The main purpose of it is to identify the areas of common concern and, ideally, agreed solutions to collectively discuss with the Bank, which would be in addition to any other partner-specific issue that we will discuss/have discussed bilaterally with the Bank. Feel free to adapt the format as well, as it may be useful to associate specific additions with the respective UN Partner making them. Please do let me know if you have trouble accessing the file using the shared link.\n\nI suggest we give ourselves a week – next Wednesday - to provide our respective feedback, after which we can agree to reconvene to discuss next steps including in terms of approaching the Bank.\n\nBest regards,\nIrene\n\n\nIrene Spaziani\nDonor Relations and Contracts Officer\nResource Mobilization Division (PSR)\nFood and Agriculture Organization of United Nations (FAO)\nTel: (+39) 06 570 52236\nS4B: irene.spaziani@fao.org<mailto:irene.spaziani@fao.org>\nwww.fao.org<http://www.fao.org/>\n\n\n-----Original Appointment-----\nFrom: Spaziani, Irene (PSR)\nSent: Wednesday, November 27, 2024 5:07 PM\nTo: Spaziani, Irene (PSR); Christine BOWERS; Rawad ASSAAD; Andrea Suley\nCc: Lukic, Meran (PSR); alistairs@unops.org<mailto:alistairs@unops.org>; Tanya Belcheva; Jessica Rennie; MPC.CONTRACTING; Elizabeth DUBY; Adriana BONOMO; PUENTES Andrea Carolina; Ibares, Alvaro (LEGA); Micol MULON\nSubject: WB SFA - November Version\nWhen: Friday, December 6, 2024 3:00 PM-4:00 PM (UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna.\nWhere: Microsoft Teams Meeting\n\nDear colleagues,\n\nFurther to the below, I am blocking this slot in our calendars to briefly connect next week and compare notes on the revised SFA.\nIt will be extremely useful to hear your views, thus I hope this time works for all. If not, please do feel free to suggest an alternative one.\n\nBest regards,\nIrene\n\n_____________________________________________\nFrom: Spaziani, Irene (PSR)\nSent: Monday, November 25, 2024 2:29 PM\nTo: Christine BOWERS <christinebo@unops.org<mailto:christinebo@unops.org>>; Rawad ASSAAD <rawad.assaad@wfp.org<mailto:rawad.assaad@wfp.org>>; Andrea Suley <asuley@unicef.org<mailto:asuley@unicef.org>>\nCc: Lukic, Meran (PSR) <Meran.Lukic@fao.org<mailto:Meran.Lukic@fao.org>>; alistairs@unops.org<mailto:alistairs@unops",
                        Location = "Teams",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DTv1pMAD",
                        EmailAddresses = new List<string> { "christinebo@unops.org", "jorge.teunissen@fao.org", "mpc.contracting@wfp.org", "elizabethdu@unops.org", "alistairs@unops.org", "adriana.bonomo@wfp.org", "micol.mulon@wfp.org", "rawad.assaad@wfp.org", "irene.spaziani@fao.org", "jrennie@unicef.org", "alvaro.ibares@fao.org", "meran.lukic@fao.org", "camila.sanchezugalde@fao.org", "apuentes@iom.int", "asuley@unicef.org", "tbelcheva@unicef.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("elizabethdu@unops.org".ToLower()) ? paoUserEmailMapping["elizabethdu@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 30, 0, 16, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7VmsIAF" },
                    new List<int> {  },
                    new List<string> { "elizabethdu@unops.org", "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DVpOXMA1",
                    new UNOPSInteraction
                    {
                        Name = "World Bank Conference on Public Institutions for Development: Enabling the Private Sector.",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.UtcNow,
                        Subject = "World Bank Conference on Public Institutions for Development: Enabling the Private Sector.",
                        Description = "Notes: https://docs.google.com/document/d/158EIlCRCASmb1o7Jhv5XLpNjWQVV4flc3EcmugyMZ_4/edit?tab=t.0#heading=h.5ca1aotkz6qq",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DVpOXMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 1, 30, 18, 59, 7, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DbpC9MAJ",
                    new UNOPSInteraction
                    {
                        Name = "Re-intro kaffe: Karen/Asbjørn",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-02-07").ToUniversalTime(),
                        Subject = "Re-intro kaffe: Karen/Asbjørn",
                        Description = null,
                        Location = "Ministry of Foreign Affairs of Denmark, Asiatisk Pl. 2, 1448 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DbpC9MAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 4, 7, 50, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000ZpmgDIAR" },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Di1H3MAJ",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS - procurement",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-06").ToUniversalTime(),
                        Subject = "UNOPS - procurement",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_YzUwZjBjYTgtMmE5NC00MTcwLTg2YTgtMWNlMzk5YmRlMGRi%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22018eb159-02ce-448e-ab82-59ac4edec278%22%7d>\nMeeting ID: 277 435 342 991\nPasscode: ab94DH9C\n________________________________\nDial in by phone\n+1 509-408-0991,,287847130#<tel:+15094080991,,287847130> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=287847130>\nPhone conference ID: 287 847 130#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 112 608 362 6\nMore info<https://www.webex.com/msteams?confid=1126083626&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=018eb159-02ce-448e-ab82-59ac4edec278&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_YzUwZjBjYTgtMmE5NC00MTcwLTg2YTgtMWNlMzk5YmRlMGRi@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Di1H3MAJ",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 6, 19, 43, 0, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000A9u0RIAR" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Dk42pMAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS meeting w Michael Tirre",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-01-23").ToUniversalTime(),
                        Subject = "UNOPS meeting w Michael Tirre",
                        Description = "Discussed opportunities for UNOPS to partner with State Department on humanitarian demining ahead of submission of concept note by Ukraine country office. Discussed collaboration with other donors and implementers, including Tetra Tech.",
                        Location = "Greenberry's Coffee Co., 1805 E St NW, Washington, DC 20006, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Dk42pMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 7, 22, 31, 19, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "eleneag@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Dk5RqMAJ",
                    new UNOPSInteraction
                    {
                        Name = "Catch up with Million Fikre - WB operational matters",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-28").ToUniversalTime(),
                        Subject = "Catch up with Million Fikre - WB operational matters",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Dk5RqMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 7, 22, 28, 14, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Dk6CbMAJ",
                    new UNOPSInteraction
                    {
                        Name = "World Bank - An Introduction to Mission 300",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-01-23").ToUniversalTime(),
                        Subject = "World Bank - An Introduction to Mission 300",
                        Description = "Mission 300 is an initiative that aims to provide a reliable energy supply to 300 million people in sub-Saharan Africa by 2030. This initiative is one of the World Bank’s key priorities and will create a significant pipeline of major opportunities over the next few years in the areas of power generation, transmission, distribution and advisory support. \n\nPreparations are underway to launch various projects that fall under the Mission 300 banner and to develop supporting procurement strategies.  To support this process, we are embarking on a program of market engagement that will provide an opportunity for potential bidders and other interested parties to find out more about Mission 300 and its objectives.\n\nThis Webinar will provide interested suppliers with:\n1.\tAn introduction to Mission 300\n2.\tAn overview of upcoming projects, timelines etc\n3.\tAn overview of the Bank’s Procurement Framework\n\nFor further information on Mission 300 please see the Bank’s website https://www.worldbank.org/en/programs/energizing-africa/overview\n\n-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-\n\n\n\n\n\n\nJOIN WEBEX MEETING\nhttps://worldbankgroup.webex.com/worldbankgroup/j.php?MTID=m076da21e4add3f7be0c1bfc9bc87fb27\nMeeting number (access code): 2318 168 5129\n \nMeeting password: dMCKU6xff26 \n \n\n\n\n\nTAP TO JOIN FROM A MOBILE DEVICE (ATTENDEES ONLY)\n+1-650-479-3207,,23181685129## tel:%2B1-650-479-3207,,*01*23181685129%23%23*01* Call-in toll number (US/Canada)\n\n\nJOIN BY PHONE\n1-650-479-3207 Call-in toll number (US/Canada)\n\nGlobal call-in numbers\nhttps://worldbankgroup.webex.com/worldbankgroup/globalcallin.php?MTID=m2fd2dd0b4af007d96e4372eb197b09b8\n\n\n\nCan't join the meeting?\nhttps://collaborationhelp.cisco.com/article/WBX000029055\n\nIMPORTANT NOTICE: Please note that this Webex service allows audio and other information sent during the session to be recorded, which may be discoverable in a legal matter. By joining this session, you automatically consent to such recordings. If you do not consent to being recorded, discuss your concerns with the host or do not join the session.",
                        Location = "https://worldbankgroup.webex.com/worldbankgroup/j.php?MTID=m076da21e4add3f7be0c1bfc9bc87fb27",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Dk6CbMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 7, 22, 36, 36, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DopPeMAJ",
                    new UNOPSInteraction
                    {
                        Name = "Pernilla og Asbjørn",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-11").ToUniversalTime(),
                        Subject = "Pernilla og Asbjørn",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MGQ3OTA1YTktZTI5Yi00OTUxLThiZmQtYWVhNjNlMWMyZGFi%40thread.v2/0?context=%7b%22Tid%22%3a%2259f6ecfd-d8e9-40da-82ea-5ba5dfb8c01e%22%2c%22Oid%22%3a%22286a44f7-c86b-4b5d-a4c2-5ce0f756847f%22%7d>\nMeeting ID: 379 212 680 828\nPasscode: EQ6Yc2ij\n________________________________\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=286a44f7-c86b-4b5d-a4c2-5ce0f756847f&tenantId=59f6ecfd-d8e9-40da-82ea-5ba5dfb8c01e&threadId=19_meeting_MGQ3OTA1YTktZTI5Yi00OTUxLThiZmQtYWVhNjNlMWMyZGFi@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DopPeMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 11, 10, 19, 26, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000V8ZjWIAV" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DrbV0MAJ",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with Panama Mayor's Office",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-02-11").ToUniversalTime(),
                        Subject = "Meeting with Panama Mayor's Office",
                        Description = null,
                        Location = "Panama Mayor's Office",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DrbV0MAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("isabelaf@unops.org".ToLower()) ? paoUserEmailMapping["isabelaf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 12, 14, 36, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000aURHUIA4" },
                    new List<int> { 1322 },
                    new List<string> { "isabelaf@unops.org", "marialk@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000E0WKrMAN",
                    new UNOPSInteraction
                    {
                        Name = "Copenhagen Climate Ministerial",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-02-18").ToUniversalTime(),
                        Subject = "Copenhagen Climate Ministerial",
                        Description = null,
                        Location = "UN City",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000E0WKrMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 18, 8, 31, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Zwa5SIAR" },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EBL2fMAH",
                    new UNOPSInteraction
                    {
                        Name = "Reunion de alto nivel FF - Dante Mossi",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-04-10").ToUniversalTime(),
                        Subject = "Reunion de alto nivel FF - Dante Mossi",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EBL2fMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("isabelaf@unops.org".ToLower()) ? paoUserEmailMapping["isabelaf@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 25, 1, 56, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1568 },
                    new List<string> { "isabelaf@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EBM6nMAH",
                    new UNOPSInteraction
                    {
                        Name = "Taller UNOPS-BCIE para renovacion de acuerdo marco y plantillas",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2023-10-31").ToUniversalTime(),
                        Subject = "Taller UNOPS-BCIE para renovacion de acuerdo marco y plantillas",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EBM6nMAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("josem@unops.org".ToLower()) ? paoUserEmailMapping["josem@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 25, 2, 18, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000bJAmNIAW" },
                    new List<int> { 1568 },
                    new List<string> { "josem@unops.org", "elisabets@unops.org", "eliserp@unops.org", "williamsg@unops.org", "davidme@unops.org", "isabelaf@unops.org", "adaz@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000ECRHKMA5",
                    new UNOPSInteraction
                    {
                        Name = "Exploración alianzas CCIC_UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-24").ToUniversalTime(),
                        Subject = "Exploración alianzas CCIC_UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ECRHKMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("anav@unops.org".ToLower()) ? paoUserEmailMapping["anav@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 25, 14, 32, 34, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000bLRj6IAG" },
                    new List<int> {  },
                    new List<string> { "anav@unops.org", "lauragi@unops.org" },
                    new List<string> { "B5417" }
                ),
                new (
                    "00UQx00000ED70QMAT",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/WB Mtg",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-25").ToUniversalTime(),
                        Subject = "UNOPS/WB Mtg",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NmFlM2E4MTEtMThhYi00ZmNjLWI3ZmItZTc2MTliYmVkMWE1%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%22786b72f1-28f3-43ec-9678-68ad981664bb%22%7d>\nMeeting ID: 270 778 672 122\nPasscode: Sk3GW9Lw\n________________________________\nDial in by phone\n+1 509-408-0991,,93973193#<tel:+15094080991,,93973193> United States, Liberty Lake\nFind a local number<https://dialin.teams.microsoft.com/e272f916-d2f5-419f-ad42-73599dac03c0?id=93973193>\nPhone conference ID: 939 731 93#\nJoin on a video conferencing device\nTenant key: wbg@m.webex.com\nVideo ID: 115 464 409 2\nMore info<https://www.webex.com/msteams?confid=1154644092&tenantkey=wbg&domain=m.webex.com>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=786b72f1-28f3-43ec-9678-68ad981664bb&tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&threadId=19_meeting_NmFlM2E4MTEtMThhYi00ZmNjLWI3ZmItZTc2MTliYmVkMWE1@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting; MC 10-605 (25) VC, MC 10-605 (25) VC",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ED70QMAT",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 25, 20, 7, 48, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EEDF3MAP",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS-UN Women meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-27").ToUniversalTime(),
                        Subject = "UNOPS-UN Women meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EEDF3MAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("daniele@unops.org".ToLower()) ? paoUserEmailMapping["daniele@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 26, 10, 43, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Tsty6IAB" },
                    new List<int> { 1222 },
                    new List<string> { "daniele@unops.org", "arnauds@unops.org", "mikaelag@unops.org", "freyavg@unops.org", "robertgodin@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EEO7FMAX",
                    new UNOPSInteraction
                    {
                        Name = "Discussion (ll) on the agenda for the meeting: ASG Madi & ASG Damkjær - Partnership Dialogue",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-01-23").ToUniversalTime(),
                        Subject = "Discussion (ll) on the agenda for the meeting: ASG Madi & ASG Damkjær - Partnership Dialogue",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EEO7FMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("waingchita@unops.org".ToLower()) ? paoUserEmailMapping["waingchita@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 26, 11, 49, 4, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000B5lnxIAB" },
                    new List<int> { 1222 },
                    new List<string> { "waingchita@unops.org", "mikaelag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EGKJ6MAP",
                    new UNOPSInteraction
                    {
                        Name = "Gaza - Unops",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-27").ToUniversalTime(),
                        Subject = "Gaza - Unops",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZTcyNGQxMTAtODMzNC00YWY4LTlhYWItYzZjYjllOTFkNGU3%40thread.v2/0?context=%7b%22Tid%22%3a%223977e38c-aa4b-439e-80ea-421a4d4ef891%22%2c%22Oid%22%3a%227fc0d912-53b9-4c2e-88ee-b3fd5dcdc3b3%22%7d>\nMeeting ID: 364 012 646 159\nPasscode: fA3sB7zv\n________________________________\nDial in by phone\n+47 21 40 20 33,,887947637#<tel:+4721402033,,887947637> Norway, Oslo\nFind a local number<https://dialin.teams.microsoft.com/6608c65b-dfb5-44b7-a633-019aacd64c20?id=887947637>\nPhone conference ID: 887 947 637#\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=7fc0d912-53b9-4c2e-88ee-b3fd5dcdc3b3&tenantId=3977e38c-aa4b-439e-80ea-421a4d4ef891&threadId=19_meeting_ZTcyNGQxMTAtODMzNC00YWY4LTlhYWItYzZjYjllOTFkNGU3@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EGKJ6MAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("marijab@unops.org".ToLower()) ? paoUserEmailMapping["marijab@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 27, 12, 4, 52, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000b1faOIAQ" },
                    new List<int> { 1101 },
                    new List<string> { "marijab@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EJ1ZrMAL",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/WB sustainable procurement",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-27").ToUniversalTime(),
                        Subject = "UNOPS/WB sustainable procurement",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EJ1ZrMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 28, 22, 55, 59, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000DUQTuIAP" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "trexylcm@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EJINnMAP",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/Andrew Hyde - @ Stimson Center",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-02-27").ToUniversalTime(),
                        Subject = "UNOPS/Andrew Hyde - @ Stimson Center",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EJINnMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 2, 28, 22, 55, 46, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Ipy2zIAB" },
                    new List<int> { 1145 },
                    new List<string> { "alistairs@unops.org", "patrickel@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EQtgkMAD",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/WB OPCS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-05").ToUniversalTime(),
                        Subject = "UNOPS/WB OPCS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EQtgkMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 5, 22, 32, 57, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EaRhZMAV",
                    new UNOPSInteraction
                    {
                        Name = "World Bank MENA meeting",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-10").ToUniversalTime(),
                        Subject = "World Bank MENA meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EaRhZMAV",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 11, 17, 32, 5, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EgRJuMAN",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with NEA Gaza Reconstruction",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-13").ToUniversalTime(),
                        Subject = "Meeting with NEA Gaza Reconstruction",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EgRJuMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 14, 14, 28, 11, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "usmana@unops.org", "patrickel@unops.org", "marijab@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EgTSAMA3",
                    new UNOPSInteraction
                    {
                        Name = "PRM-UNOPS Meeting",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-03-13").ToUniversalTime(),
                        Subject = "PRM-UNOPS Meeting",
                        Description = "Notional Agenda:\n\n  *   Brief introductions\n  *   View from the field (UNOPS)\n     *   Syria and Lebanon\n        *   Q&A\n     *   Gaza\n        *   Q&A\n  *   AOB",
                        Location = "SA-09 Room NE8060",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EgTSAMA3",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 14, 14, 26, 55, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EgTjtMAF",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with NEA at Main State (HST)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-03-13").ToUniversalTime(),
                        Subject = "Meeting with NEA at Main State (HST)",
                        Description = null,
                        Location = "U.S. Department of State, 2201 C St NW, Washington, DC 20451, USA",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EgTjtMAF",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 14, 14, 27, 57, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "usmana@unops.org", "patrickel@unops.org", "marijab@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EgUUhMAN",
                    new UNOPSInteraction
                    {
                        Name = "UN IFI CAS WG",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-14").ToUniversalTime(),
                        Subject = "UN IFI CAS WG",
                        Description = "Microsoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NjIxZDZmMzUtMTU4YS00MDg2LTk0NTAtODU5MzAyYjYxYjQ3%40thread.v2/0?context=%7b%22Tid%22%3a%220f9e35db-544f-4f60-bdcc-5ea416e6dc70%22%2c%22Oid%22%3a%22551c519d-39eb-4b6d-97cb-2433a86e33c8%22%7d>\nMeeting ID: 383 965 672 533\nPasscode: c9pd9fv7",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EgUUhMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 14, 14, 56, 38, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Eh2o5MAB",
                    new UNOPSInteraction
                    {
                        Name = "WB Infrastructure, MENA Regional Director Ms Almud Weitz",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-14").ToUniversalTime(),
                        Subject = "WB Infrastructure, MENA Regional Director Ms Almud Weitz",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Eh2o5MAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 14, 20, 27, 59, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org", "alistairs@unops.org", "usmana@unops.org", "marijab@unops.org", "banak@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EwigcMAB",
                    new UNOPSInteraction
                    {
                        Name = "Sida/UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-14").ToUniversalTime(),
                        Subject = "Sida/UNOPS",
                        Description = "Testing this slot instead of today’s call.\n\nBest regards.\n\nKatarina\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MTRhNTBkYTEtZjFmNi00ZWQwLTgzNzItNDg4ZjA2MThjYTUy%40thread.v2/0?context=%7b%22Tid%22%3a%22aa88b5d0-35a6-49d6-8322-c8baa04dc712%22%2c%22Oid%22%3a%22b2b271aa-d70a-415a-9f7a-92e54bc0f227%22%7d>\nMeeting ID: 339 865 836 413\nPasscode: RU38jr2B\n________________________________\nJoin on a video conferencing device\nTenant key: teams@vmr.sida.se<mailto:teams@vmr.sida.se>\nVideo ID: 129 044 210 7\nMore info<https://vmr.sida.se/teams/?conf=1290442107&ivr=teams&d=vmr.sida.se&prefix=teams.&w>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=b2b271aa-d70a-415a-9f7a-92e54bc0f227&tenantId=aa88b5d0-35a6-49d6-8322-c8baa04dc712&threadId=19_meeting_MTRhNTBkYTEtZjFmNi00ZWQwLTgzNzItNDg4ZjA2MThjYTUy@thread.v2&messageId=0&language=en-US>\nSida’s Privacy Notice: Please, see information on Sida’s web site, www.sida.se<http://www.sida.se>. Sida’s configuration of the Teams environment has the following rules applied: Conference ID is mandatory when joining by video conference system and web browser.\nPrivacy and security<https://www.sida.se/en/about-the-website/sidas-privacy-notice>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EwigcMAB",
                        EmailAddresses = new List<string> { "teams@vmr.sida.se" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 25, 10, 1, 53, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YZz0KIAT" },
                    new List<int> { 1108 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Ewj1gMAB",
                    new UNOPSInteraction
                    {
                        Name = "Standard Template for Finland/UNOPS engagements",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-07").ToUniversalTime(),
                        Subject = "Standard Template for Finland/UNOPS engagements",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Ewj1gMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 25, 10, 4, 8, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000dC9HDIA0" },
                    new List<int> { 1087 },
                    new List<string> { "asbjornb@unops.org", "devorahfd@unops.org", "franciscoca@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EwqFtMAJ",
                    new UNOPSInteraction
                    {
                        Name = "FCDO / UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-03-13").ToUniversalTime(),
                        Subject = "FCDO / UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EwqFtMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 25, 10, 1, 45, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000099IS3IAM" },
                    new List<int> { 1752 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EwqfhMAB",
                    new UNOPSInteraction
                    {
                        Name = "Anne/Asbjørn",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-03-07").ToUniversalTime(),
                        Subject = "Anne/Asbjørn",
                        Description = null,
                        Location = "Asiatisk Plads, Asiatisk Pl., København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EwqfhMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 3, 25, 10, 4, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF" },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000FPqu6MAD",
                    new UNOPSInteraction
                    {
                        Name = "UNS @ CODEWAY2025",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-04-10").ToUniversalTime(),
                        Subject = "UNS @ CODEWAY2025",
                        Description = "As mentioned in the previous message, this meeting is to put in common the address to Private Sector participation in UN biddings, considering current challenges and situations.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000FPqu6MAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 4, 10, 13, 53, 24, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YXHRVIA5" },
                    new List<int> { 1904 },
                    new List<string> { "martina@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000FRA14MAH",
                    new UNOPSInteraction
                    {
                        Name = "Carlo Batori - Farnesina",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-04-07").ToUniversalTime(),
                        Subject = "Carlo Batori - Farnesina",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000FRA14MAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 4, 11, 7, 13, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1904 },
                    new List<string> { "martina@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GAMr1MAH",
                    new UNOPSInteraction
                    {
                        Name = "UK/UNOPS catch up",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-05-13").ToUniversalTime(),
                        Subject = "UK/UNOPS catch up",
                        Description = "Dear Asbjorn,\n\nAs discussed, putting a time for us to talk through UNOPS items at the upcoming annual session.\n\nKavoy Ashley (UNDP/UNICEF/UNOPS lead at UKMIS New York) and hopefully Emily Boyce (Team Leader, UN Partnerships Unit) will also join, as well as Sally who you know well! Hopefully this will give you a good set of contacts in the FCDO until my successor on UNOPS is appointed.\n\nAll the best,\n\nAgnes\n\n________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_YTMzNzMyNDEtYTRjMC00NmVkLWJhOTMtMzZjNzVlYmQ3MzUz%40thread.v2/0?context=%7b%22Tid%22%3a%22d3a2d0d3-7cc8-4f52-bbf9-85bd43d94279%22%2c%22Oid%22%3a%2252fe111a-2571-484f-a611-55bd4f4bc92c%22%7d>\nMeeting ID: 329 703 373 950 3\nPasscode: 7UH3Rx74\n________________________________\nDial in by phone\n+44 20 7660 8164,,598690282#<tel:+442076608164,,598690282> United Kingdom, City of London\nFind a local number<https://dialin.teams.microsoft.com/33c0cc34-9076-4ef7-bb74-787c147b1311?id=598690282>\nPhone conference ID: 598 690 282#\nJoin on a video conferencing device\nTenant key: teams@fcdo2.onpexip.com<mailto:teams@fcdo2.onpexip.com>\nVideo ID: 125 403 973 7\nMore info<https://pexip.me/teams/fcdo2.onpexip.com/1254039737>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=52fe111a-2571-484f-a611-55bd4f4bc92c&tenantId=d3a2d0d3-7cc8-4f52-bbf9-85bd43d94279&threadId=19_meeting_YTMzNzMyNDEtYTRjMC00NmVkLWJhOTMtMzZjNzVlYmQ3MzUz@thread.v2&messageId=0&language=en-US> | Reset dial-in PIN<https://dialin.teams.microsoft.com/usp/pstnconferencing>\n[https://org871238972424.blob.core.windows.net/$web/TeamsLogo.jpg]\nYour activity on Teams may be monitored in line with relevant UK legislation.\nPrivacy and security<https://www.gov.uk/government/organisations/foreign-commonwealth-development-office/about/personal-information-charter>\n________________________________________________________________________________\n\n\n\nFollow us online: www.gov.uk/fcdo\n\n\nThis email is intended for the addressee(s) only: All messages sent and received by the Foreign, Commonwealth & Development Office may be monitored in line with relevant UK legislation<https://www.gov.uk/government/publications/fcdo-as-a-data-controller-privacy-notice/fcdo-as-a-data-controller-privacy-notice>",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GAMr1MAH",
                        EmailAddresses = new List<string> { "teams@fcdo2.onpexip.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 5, 12, 15, 58, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx0000099IS3IAM" },
                    new List<int> { 1752 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GBPJdMAP",
                    new UNOPSInteraction
                    {
                        Name = "Kick off meeting - arkas school review",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-05-13").ToUniversalTime(),
                        Subject = "Kick off meeting - arkas school review",
                        Description = "Dear all,\n\nIt is our pleasure to invite you to the kick off meeting for the arkas school review project. If some of you cannot join this coming Tuesday, there will be more meetings scheduled in the coming weeks as well as site visit to Mykolaiv.\n\nShort agenda:\n\n\n  1.  Embassy/MFA DK  - introduction and presentation of the project as well as participants\n  2.  IC consulenten – introduction to the work plan, documents, time schedule etc.\n  3.  Next steps and AOB\n\nWe expect the meeting to take no longer than 30 minutes, as this is an introduction meeting.\n\nSincerely,\nVasyl\n________________________________________________________________________________\nMicrosoft Teams Har du brug for hjælp?<https://aka.ms/JoinTeamsMeeting?omkt=da-DK>\nDeltag i mødet nu<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NzE4M2RlZjQtOWViNS00NGE0LTg2ZDgtMjBlNjkyYTExY2Nk%40thread.v2/0?context=%7b%22Tid%22%3a%2248dc02d0-bd56-411d-b7c5-a814743bafc6%22%2c%22Oid%22%3a%2200c3b8bd-3ffa-44a4-a2ce-9963eca9871c%22%7d>\nMøde-id: 376 377 915 968 2\nAdgangskode: fd7gW2MQ\n________________________________\nTilmeld dig på en enhed til videomøder\nLejernøgle: teams@meet.um.dk\nVideo-id: 128 164 888 7\nFlere oplysninger<https://pexip.me/teams/meet.um.dk/1281648887>\nFor arrangører: Mødeindstillinger<https://teams.microsoft.com/meetingOptions/?organizerId=00c3b8bd-3ffa-44a4-a2ce-9963eca9871c&tenantId=48dc02d0-bd56-411d-b7c5-a814743bafc6&threadId=19_meeting_NzE4M2RlZjQtOWViNS00NGE0LTg2ZDgtMjBlNjkyYTExY2Nk@thread.v2&messageId=0&language=da-DK>\n________________________________________________________________________________",
                        Location = "Microsoft Teams-møde",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GBPJdMAP",
                        EmailAddresses = new List<string> { "teams@meet.um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 5, 13, 6, 59, 40, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000UGexxIAD" },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GYdgsMAD",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS ukraine - appraisal",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-05-16").ToUniversalTime(),
                        Subject = "UNOPS ukraine - appraisal",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_ZjY4ODM4NDUtNTFkYS00NzkwLWFhYTQtZWVjYzkyY2JkNWRm%40thread.v2/0?context=%7b%22Tid%22%3a%2248dc02d0-bd56-411d-b7c5-a814743bafc6%22%2c%22Oid%22%3a%22f6c15c47-2656-4c1c-aec4-f68abfd76838%22%7d>\nMeeting ID: 385 377 358 932 7\nPasscode: Xx2La3H9\n________________________________\nJoin on a video conferencing device\nTenant key: teams@meet.um.dk\nVideo ID: 124 605 360 1\nMore info<https://pexip.me/teams/meet.um.dk/1246053601>\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=f6c15c47-2656-4c1c-aec4-f68abfd76838&tenantId=48dc02d0-bd56-411d-b7c5-a814743bafc6&threadId=19_meeting_ZjY4ODM4NDUtNTFkYS00NzkwLWFhYTQtZWVjYzkyY2JkNWRm@thread.v2&messageId=0&language=en-US>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GYdgsMAD",
                        EmailAddresses = new List<string> { "teams@meet.um.dk" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 5, 27, 7, 28, 33, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GYeUsMAL",
                    new UNOPSInteraction
                    {
                        Name = "Finland/UNOPS: Annual EB session",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-05-15").ToUniversalTime(),
                        Subject = "Finland/UNOPS: Annual EB session",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GYeUsMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("naimoh@unops.org".ToLower()) ? paoUserEmailMapping["naimoh@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 5, 27, 7, 28, 17, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YQDnqIAH" },
                    new List<int> { 1087 },
                    new List<string> { "naimoh@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GYhEDMA1",
                    new UNOPSInteraction
                    {
                        Name = "UTP Appraisal - Finance - UNOPS",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-05-27").ToUniversalTime(),
                        Subject = "UTP Appraisal - Finance - UNOPS",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-DK>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MGYyMDM3NWQtMTU0NC00ZTE3LWJhMTAtMmM2M2NkNGM1MTQ3%40thread.v2/0?context=%7b%22Tid%22%3a%2241cdff95-bd23-4ad6-8e53-b8528f9a4259%22%2c%22Oid%22%3a%2203c3d78a-30d0-4f73-8d08-33712de85740%22%7d>\nMeeting ID: 398 934 040 418 4\nPasscode: pj9U5NB7\n________________________________\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=03c3d78a-30d0-4f73-8d08-33712de85740&tenantId=41cdff95-bd23-4ad6-8e53-b8528f9a4259&threadId=19_meeting_MGYyMDM3NWQtMTU0NC00ZTE3LWJhMTAtMmM2M2NkNGM1MTQ3@thread.v2&messageId=0&language=en-DK>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GYhEDMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arunn@unops.org".ToLower()) ? paoUserEmailMapping["arunn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 5, 27, 7, 26, 18, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "arunn@unops.org", "eleneag@unops.org", "vladyslavk@unops.org", "marysiaz@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GauiQMAR",
                    new UNOPSInteraction
                    {
                        Name = "Snihurivka and Arkas - next steps",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-06-04").ToUniversalTime(),
                        Subject = "Snihurivka and Arkas - next steps",
                        Description = "<ul><li>Intro between UNOPS and Sahil</li><li>Present <a href=\"https://drive.google.com/file/d/1L9VXI6rO2j0JP60JR3iVywPwSZAXUFgF/view?usp=sharing\">preliminary assessment</a> of reconstruction of Snihurivka High School</li><li>Discuss next steps for <a href=\"https://drive.google.com/file/d/1rCuzlYV25eJJJvJg637uQWl54pb_JMp7/view?usp=sharing\">reconstruction of Arkas High School</a></li></ul>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GauiQMAR",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arunn@unops.org".ToLower()) ? paoUserEmailMapping["arunn@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 5, 28, 8, 24, 21, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000C8DnaIAF" },
                    new List<int> { 1086 },
                    new List<string> { "arunn@unops.org", "vadyms@unops.org", "valentynp@unops.org", "eleneag@unops.org", "marysiaz@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GbK9rMAF",
                    new UNOPSInteraction
                    {
                        Name = "Hilse-på-kaffe: Sahil/Asbjørn",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-06-11").ToUniversalTime(),
                        Subject = "Hilse-på-kaffe: Sahil/Asbjørn",
                        Description = null,
                        Location = "Ministry of Foreign Affairs of Denmark, Asiatisk Pl. 2, 1448 København, Denmark",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GbK9rMAF",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 5, 28, 10, 40, 50, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000hTbYCIA0" },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GlKq5MAF",
                    new UNOPSInteraction
                    {
                        Name = "UNWebBuy - Pickup Trucks",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-06-03").ToUniversalTime(),
                        Subject = "UNWebBuy - Pickup Trucks",
                        Description = "________________________________________________________________________________\nMicrosoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_MzE4OGYyZjMtODI5ZS00YWQwLWFlM2MtZjIzOTY5MmYxNzAz%40thread.v2/0?context=%7b%22Tid%22%3a%22be4f7c42-e565-40e6-b81c-fdf01afc920c%22%2c%22Oid%22%3a%22dc7d27f3-6704-46bb-94d2-536406877e4e%22%7d>\nMeeting ID: 297 338 128 212 7\nPasscode: kA22Ba2N\n________________________________\nFor organizers: Meeting options<https://teams.microsoft.com/meetingOptions/?organizerId=dc7d27f3-6704-46bb-94d2-536406877e4e&tenantId=be4f7c42-e565-40e6-b81c-fdf01afc920c&threadId=19_meeting_MzE4OGYyZjMtODI5ZS00YWQwLWFlM2MtZjIzOTY5MmYxNzAz@thread.v2&messageId=0&language=en-US>\n[https://cdbgeneralpurpose.blob.core.windows.net/public-content/CDBLogoTeamsMeeting.png]\nhttps://www.caribank.org/general-privacy-notice\nOrg help<https://www.caribank.org/contact-us> | Privacy and security<https://www.caribank.org/legal>\n________________________________________________________________________________",
                        Location = "Microsoft Teams Meeting",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GlKq5MAF",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jean-vincentc@unops.org".ToLower()) ? paoUserEmailMapping["jean-vincentc@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 6, 3, 15, 30, 42, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000hW3V6IAK" },
                    new List<int> { 1439 },
                    new List<string> { "jean-vincentc@unops.org", "antoinel@unops.org" },
                    new List<string> { "B5416" }
                ),
                new (
                    "00UQx00000HOim5MAD",
                    new UNOPSInteraction
                    {
                        Name = "OECS - UNOPS | Advancing DIalogue on Sustainable Procurement",
                        Type = InteractionType.VirtualMeeting,
                        Date = DateTime.Parse("2025-06-26").ToUniversalTime(),
                        Subject = "OECS - UNOPS | Advancing DIalogue on Sustainable Procurement",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000HOim5MAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("sylviaac@unops.org".ToLower()) ? paoUserEmailMapping["sylviaac@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 6, 26, 14, 26, 47, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000j1ok8IAA" },
                    new List<int> { 1577 },
                    new List<string> { "sylviaac@unops.org", "williamsg@unops.org", "marcusm@unops.org", "patrickdi@unops.org", "giuseppem@unops.org", "antoinel@unops.org" },
                    new List<string> { "B5416" }
                ),
                new (
                    "00UQx00000IO6ebMAD",
                    new UNOPSInteraction
                    {
                        Name = "Invitation: Meeting with DG INTPA - Ian Hoskins, Deputy Head of Unit,... @ Fri Jun 20, 2025 11:00 - 12:00 (CEST) (Jerome DE THYSEBAERT)",
                        Type = InteractionType.InPersonMeeting,
                        Date = DateTime.Parse("2025-06-20").ToUniversalTime(),
                        Subject = "Invitation: Meeting with DG INTPA - Ian Hoskins, Deputy Head of Unit,... @ Fri Jun 20, 2025 11:00 - 12:00 (CEST) (Jerome DE THYSEBAERT)",
                        Description = null,
                        Location = "Brussels",
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000IO6ebMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : 0,
                        CreatedDate = new DateTime(2025, 7, 31, 22, 52, 49, DateTimeKind.Utc),
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1025 },
                    new List<string> { "mariacarmenco@unops.org" },
                    new List<string> { "B0047" }
                )
            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Process all interactions (create or update)
                foreach (var (gmailMessageId, interactionData, _, _, _, _) in interactionsToProcess)
                {
                    if (string.IsNullOrEmpty(gmailMessageId))
                        continue;

                    // Check if interaction already exists based on GmailMessageId
                    var existingInteraction = await context.Interactions
                        .FirstOrDefaultAsync(i => i.GmailMessageId == gmailMessageId);

                    // Check if interaction already exists based on Subject and Description
                    var existingInteractionBySubDesc = await context.Interactions
                        .FirstOrDefaultAsync(i => i.Subject == interactionData.Subject && i.Description == interactionData.Description);

                    if (existingInteraction != null)
                    {
                        // Update existing interaction - only update CreatedDate & Type fields
                        existingInteraction.CreatedDate = interactionData.CreatedDate;
                        existingInteraction.Type = interactionData.Type;
                    }
                    else if (existingInteractionBySubDesc != null)
                    {
                        //Update existing interaction
                        existingInteractionBySubDesc.CreatedDate = interactionData.CreatedDate;
                        existingInteractionBySubDesc.Type = interactionData.Type;
                        existingInteractionBySubDesc.GmailMessageId = gmailMessageId;
                    }
                    else
                    {
                        // Add new interaction to context
                        context.Interactions.Add(interactionData);
                        newInteractionsCount++;
                    }
                }

                // Save all interactions at once
                await context.SaveChangesAsync();

                // Step 2: Process all junction table records in batch
                var interactionContactsToAdd = new List<InteractionContact>();
                var interactionPartnersToAdd = new List<InteractionPartner>();
                var interactionUsersToAdd = new List<InteractionUser>();
                var orgUnitRelationshipsToAdd = new List<OrganizationUnitRelationship>();

                foreach (var (gmailMessageId, _, contactIds, partnerErpValues, ownerEmails, orgCodes) in interactionsToProcess)
                {
                    if (string.IsNullOrEmpty(gmailMessageId))
                        continue;

                    // Get the interaction (now guaranteed to exist with an ID)
                    var interaction = await context.Interactions
                        .FirstOrDefaultAsync(i => i.GmailMessageId == gmailMessageId);

                    if (interaction == null)
                        continue;

                    // Track unique relationships to avoid duplicates
                    var uniqueContactIds = new HashSet<int>();
                    var uniquePartnerIds = new HashSet<int>();
                    var uniqueUserIds = new HashSet<int>();

                    // Process Contact relationships from Who.Id (ContactNumber)
                    foreach (var contactId in contactIds)
                    {
                        if (contactMapping.ContainsKey(contactId))
                        {
                            var dbContactId = contactMapping[contactId];
                            uniqueContactIds.Add(dbContactId);

                            // Also get the parent Partner for this Contact
                            if (contactPartnerMapping.ContainsKey(dbContactId))
                            {
                                var parentPartnerId = contactPartnerMapping[dbContactId];
                                uniquePartnerIds.Add(parentPartnerId);
                            }
                        }
                    }

                    // Process Partner relationships from What.AccountNumber (ErpDimValue)
                    foreach (var erpValue in partnerErpValues)
                    {
                        if (partnerMapping.ContainsKey(erpValue))
                        {
                            uniquePartnerIds.Add(partnerMapping[erpValue]);
                        }
                    }

                    // Process email addresses from interaction.EmailAddresses
                    if (interaction.EmailAddresses != null && interaction.EmailAddresses.Any())
                    {
                        foreach (var email in interaction.EmailAddresses)
                        {
                            var emailLower = email.ToLower();

                            // Find contacts by email
                            if (contactEmailMapping.ContainsKey(emailLower))
                            {
                                var dbContactId = contactEmailMapping[emailLower];
                                uniqueContactIds.Add(dbContactId);

                                // Also get the parent Partner for this Contact
                                if (contactPartnerMapping.ContainsKey(dbContactId))
                                {
                                    var parentPartnerId = contactPartnerMapping[dbContactId];
                                    uniquePartnerIds.Add(parentPartnerId);
                                }
                            }

                            // Find users by email
                            if (paoUserEmailMapping.ContainsKey(emailLower))
                            {
                                uniqueUserIds.Add(paoUserEmailMapping[emailLower]);
                            }
                        }
                    }

                    // Process Owner.Email for User relationships
                    foreach (var ownerEmail in ownerEmails)
                    {
                        var emailLower = ownerEmail.ToLower();
                        if (paoUserEmailMapping.ContainsKey(emailLower))
                        {
                            uniqueUserIds.Add(paoUserEmailMapping[emailLower]);
                        }
                    }

                    // Create InteractionContact records
                    foreach (var contactId in uniqueContactIds)
                    {
                        // Check if relationship already exists
                        var existingRelationship = await context.Set<InteractionContact>()
                            .FirstOrDefaultAsync(ic => ic.InteractionId == interaction.Id && ic.ContactId == contactId);

                        if (existingRelationship == null)
                        {
                            interactionContactsToAdd.Add(new InteractionContact
                            {
                                InteractionId = interaction.Id,
                                ContactId = contactId
                            });
                        }
                    }

                    // Create InteractionPartner records
                    foreach (var partnerId in uniquePartnerIds)
                    {
                        // Check if relationship already exists
                        var existingRelationship = await context.Set<InteractionPartner>()
                            .FirstOrDefaultAsync(ip => ip.InteractionId == interaction.Id && ip.PartnerId == partnerId);

                        if (existingRelationship == null)
                        {
                            interactionPartnersToAdd.Add(new InteractionPartner
                            {
                                InteractionId = interaction.Id,
                                PartnerId = partnerId
                            });
                        }
                    }

                    // Create InteractionUser records
                    foreach (var userId in uniqueUserIds)
                    {
                        // Check if relationship already exists
                        var existingRelationship = await context.Set<InteractionUser>()
                            .FirstOrDefaultAsync(iu => iu.InteractionId == interaction.Id && iu.UserId == userId);

                        if (existingRelationship == null)
                        {
                            interactionUsersToAdd.Add(new InteractionUser
                            {
                                InteractionId = interaction.Id,
                                UserId = userId
                            });
                        }
                    }

                    // Process OrganizationUnitRelationship from SF_Organisation__r.SF_EntityCode__c
                    foreach (var orgCode in orgCodes)
                    {
                        if (orgHierarchyMapping.ContainsKey(orgCode))
                        {
                            var orgHierarchyId = orgHierarchyMapping[orgCode];

                            // Check if relationship already exists
                            var existingRelationship = await context.OrganizationUnitRelationships
                                .FirstOrDefaultAsync(r => r.EntityType == nameof(Interaction) && 
                                                          r.EntityId == interaction.Id && 
                                                          r.OrganizationHierarchyId == orgHierarchyId);

                            if (existingRelationship == null)
                            {
                                orgUnitRelationshipsToAdd.Add(new OrganizationUnitRelationship
                                {
                                    OrganizationHierarchyId = orgHierarchyId,
                                    EntityId = interaction.Id,
                                    EntityType = nameof(Interaction),
                                    Name = $"Interaction-{interaction.Id}-{orgHierarchyId}",
                                    Status = EntityStatus.Active,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.UtcNow,
                                    LastModifiedBy = 0,
                                    LastModifiedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                });
                            }
                        }
                    }
                }

                // Add all junction table records at once
                if (interactionContactsToAdd.Any())
                    await context.Set<InteractionContact>().AddRangeAsync(interactionContactsToAdd);
                
                if (interactionPartnersToAdd.Any())
                    await context.Set<InteractionPartner>().AddRangeAsync(interactionPartnersToAdd);
                
                if (interactionUsersToAdd.Any())
                    await context.Set<InteractionUser>().AddRangeAsync(interactionUsersToAdd);
                
                if (orgUnitRelationshipsToAdd.Any())
                    await context.OrganizationUnitRelationships.AddRangeAsync(orgUnitRelationshipsToAdd);

                // Save all junction table records at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"Successfully seeded {interactionsToProcess.Count} interactions");
                Console.WriteLine($"Created {newInteractionsCount} new Interactions");
                Console.WriteLine($"Created {interactionContactsToAdd.Count} InteractionContact relationships");
                Console.WriteLine($"Created {interactionPartnersToAdd.Count} InteractionPartner relationships");
                Console.WriteLine($"Created {interactionUsersToAdd.Count} InteractionUser relationships");
                Console.WriteLine($"Created {orgUnitRelationshipsToAdd.Count} OrganizationUnitRelationship records");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error seeding interactions: {ex.Message}");
                throw;
            }
        }
    }
}