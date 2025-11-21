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
    public static class InteractionFromEventSeeder
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

            // Process interactions
            var interactionsToProcess = new List<(string GmailMessageId, UNOPSInteraction Interaction, List<string> ContactIds, List<int> PartnerErpValues, List<string> OwnerEmails, List<string> OrgCodes)>
            {
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1101 },
                    new List<string> { "asbjornb@unops.org", "eleneag@unops.org", "simonp@unops.org", "arunn@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx000009fefVIAQ" },
                    new List<int> { 1086 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000B0mKuMAJ",
                    new UNOPSInteraction
                    {
                        Name = "UNBT's TF on crisis and fragility meeting",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-24").ToUniversalTime(),
                        Subject = "UNBT's TF on crisis and fragility meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000B0mKuMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("celiaafricak@unops.org".ToLower()) ? paoUserEmailMapping["celiaafricak@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-24").ToUniversalTime(),
                        Subject = "Meeting of UNBT WG on Green Deal with DG CLIMA on COP-29",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000B0rScMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("celiaafricak@unops.org".ToLower()) ? paoUserEmailMapping["celiaafricak@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                    "00UQx00000BHyO5MAL",
                    new UNOPSInteraction
                    {
                        Name = "LEGEN-UNOPS Discussion",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-31").ToUniversalTime(),
                        Subject = "LEGEN-UNOPS Discussion",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BHyO5MAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Kn08QIAR" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-28").ToUniversalTime(),
                        Subject = "Virtual meeting with ASG Madi",
                        Description = "ASG Madi, Assistant Secretary-General and Deputy Executive Director for Resource Management, Sustainability and Partnerships of UN Women",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BJ63OMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("jakobt@unops.org".ToLower()) ? paoUserEmailMapping["jakobt@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1222 },
                    new List<string> { "jakobt@unops.org", "kirstined@unops.org", "mikaelag@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-11-07").ToUniversalTime(),
                        Subject = "DED - Meeting with WB MENA Vice President Ousmane Dione",
                        Description = "<p><b>Microsoft Teams</b> <a href=\"https://aka.ms/JoinTeamsMeeting?omkt=en-US\" target=\"_blank\">Need help?</a><u></u><u></u></p><p><a href=\"https://teams.microsoft.com/l/meetup-join/19%3ameeting_YjYyMmU0YTMtODQ4My00YmY5LWFhNzQtOGQ3NmI5MTBmMmUy%40thread.v2/0?context=%7b%22Tid%22%3a%2231a2fec0-266b-4c67-b56e-2796d8f59c36%22%2c%22Oid%22%3a%2288edcb6a-90d7-45cd-a67b-66171927e685%22%7d\" target=\"_blank\"><b>Join the meeting now</b></a><u></u><u></u></p><p>Meeting ID: 276 040 326 896<u></u><u></u></p><p>Passcode: fCdXGR<u></u><u></u></p><br><hr /><p><b>Join on a video conferencing device</b><u></u><u></u></p><p>Tenant key: <a href=\"mailto:wbg@m.webex.com\" target=\"_blank\">wbg@m.webex.com</a><u></u><u></u></p><p>Video ID: 117 833 133 5<u></u><u></u></p><p><a href=\"https://www.webex.com/msteams?confid=1178331335&amp;tenantkey=wbg&amp;domain=m.webex.com\" target=\"_blank\">More info</a><u></u><u></u></p><p>For organizers: <a href=\"https://teams.microsoft.com/meetingOptions/?organizerId=88edcb6a-90d7-45cd-a67b-66171927e685&amp;tenantId=31a2fec0-266b-4c67-b56e-2796d8f59c36&amp;threadId=19_meeting_YjYyMmU0YTMtODQ4My00YmY5LWFhNzQtOGQ3NmI5MTBmMmUy@thread.v2&amp;messageId=0&amp;language=en-US\" target=\"_blank\">Meeting options</a></p>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BOCZZMA5",
                        EmailAddresses = new List<string> { "wbg@m.webex.com" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "jakobt@unops.org", "banak@unops.org", "christinebo@unops.org", "waingchita@unops.org", "marijab@unops.org", "kirstined@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                    "00UQx00000BjKFaMAN",
                    new UNOPSInteraction
                    {
                        Name = "Catch up",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-12-02").ToUniversalTime(),
                        Subject = "Catch up",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BjKFaMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Up4JdIAJ" },
                    new List<int> { 1089 },
                    new List<string> { "laetitiak@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000UsOgqIAF" },
                    new List<int> { 1267 },
                    new List<string> { "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000BvOP4MAN",
                    new UNOPSInteraction
                    {
                        Name = "UNFPA alignment",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-11-05").ToUniversalTime(),
                        Subject = "UNFPA alignment",
                        Description = "Meeting with UNFPA to continue discussion on the harmonisation of contracts/benefits for the UNFPA personnel currently managed by UNOPS.\n\nAGENDA\n1. Finalize and agree on the communication to be sent to LICAs regarding contract changes; the who, what and when of the communication.\n2. Confirm if the salary review for January 2025 is complete.\n3. Review the changes to the ICA template, as we will need to coordinate with IT to proceed.\n4. Any other business (AOB).",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000BvOP4MAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                    "00UQx00000Bvb1LMAR",
                    new UNOPSInteraction
                    {
                        Name = "UNODC Finance call",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-11-18").ToUniversalTime(),
                        Subject = "UNODC Finance call",
                        Description = "Meeting to start discussing details of what a potential HR global agreement with UNODC could look like",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Bvb1LMAR",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VJ432IAD" },
                    new List<int> { 1194 },
                    new List<string> { "arnauds@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Bvc2DMAR",
                    new UNOPSInteraction
                    {
                        Name = "Meeting to discuss UNODC potential partner HR global agreement",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-30").ToUniversalTime(),
                        Subject = "Meeting to discuss UNODC potential partner HR global agreement",
                        Description = "Meet",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Bvc2DMAR",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1893 },
                    new List<string> { "djenebas@unops.org", "emiliep@unops.org", "andrewk@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Vn1naIAB" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "elenage@unops.org", "christinebo@unops.org", "fumiea@unops.org", "francescap@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000VnNmRIAV" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "fumiea@unops.org", "francescap@unops.org", "christinebo@unops.org" },
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
                        EmailAddresses = new List<string> { "alistairs@unops.org", "christinebo@unops.org", "earisoy@worldbank.org", "mfikre@worldbank.org", "wmwai@worldbank.org", "akircher1@worldbank.org", "wbg@m.webex.com", "rswami@worldbank.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("emiliep@unops.org".ToLower()) ? paoUserEmailMapping["emiliep@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1790 },
                    new List<string> { "emiliep@unops.org", "andrewk@unops.org", "annag@unops.org", "katrinl@unops.org", "jorge.moreiradasilva@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("dalilag@unops.org".ToLower()) ? paoUserEmailMapping["dalilag@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1231 },
                    new List<string> { "dalilag@unops.org", "emiliep@unops.org", "annag@unops.org", "jorge.moreiradasilva@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("emiliep@unops.org".ToLower()) ? paoUserEmailMapping["emiliep@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1857 },
                    new List<string> { "emiliep@unops.org", "andrewk@unops.org", "annag@unops.org", "katrinl@unops.org", "jorge.moreiradasilva@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-12-13").ToUniversalTime(),
                        Subject = "UNODC /UNOPS",
                        Description = "Key discussion points:\n1. \n2. \n3.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CM0KEMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("arnauds@unops.org".ToLower()) ? paoUserEmailMapping["arnauds@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> {  },
                    new List<string> { "asbjornb@unops.org", "fumiea@unops.org", "elenage@unops.org", "naimoh@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1688 },
                    new List<string> { "asbjornb@unops.org", "fumiea@unops.org", "elenage@unops.org", "naimoh@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1087 },
                    new List<string> { "asbjornb@unops.org", "naimoh@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("elenage@unops.org".ToLower()) ? paoUserEmailMapping["elenage@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1752 },
                    new List<string> { "elenage@unops.org", "fumiea@unops.org", "naimoh@unops.org", "asbjornb@unops.org", "sharonle@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Wh3VgIAJ" },
                    new List<int> {  },
                    new List<string> { "yuichis@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhZ0DIAV" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT0qXMAT",
                    new UNOPSInteraction
                    {
                        Name = "Meeting between ATScale CEO and MoFA Japan",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-11-26").ToUniversalTime(),
                        Subject = "Meeting between ATScale CEO and MoFA Japan",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT0qXMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhdIGIAZ" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Whdq5IAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT1zVMAT",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with JICA on Gaza",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-30").ToUniversalTime(),
                        Subject = "Meeting with JICA on Gaza",
                        Description = "Dear Yuichi san, Dear Hitomi san, \nJICA has contacted me asking to 'exchange information' on Gaza. I consulted with Marija, our focal point for Gaza, and we agreed that someone from Sigrid Kaag's office and Marija will both be at the meeting. I asked JICA to share with us Microsoft Teams link to be used for the meeting so I will share it once ready.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT1zVMAT",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WhZ0DIAV" },
                    new List<int> { 1095 },
                    new List<string> { "yuichis@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CT25xMAD",
                    new UNOPSInteraction
                    {
                        Name = "Meeting with New MoFA Director Ando",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-29").ToUniversalTime(),
                        Subject = "Meeting with New MoFA Director Ando",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CT25xMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000WheivIAB" },
                    new List<int> { 1906 },
                    new List<string> { "yuichis@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000IooKDIAZ" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CpkOUMAZ",
                    new UNOPSInteraction
                    {
                        Name = "Letter from ED to USAID on Sunset of July 2022 Clauses",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-12-20").ToUniversalTime(),
                        Subject = "Letter from ED to USAID on Sunset of July 2022 Clauses",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CpkOUMAZ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("patrickel@unops.org".ToLower()) ? paoUserEmailMapping["patrickel@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("jakobt@unops.org".ToLower()) ? paoUserEmailMapping["jakobt@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1222 },
                    new List<string> { "jakobt@unops.org", "waingchita@unops.org", "mikaelag@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Kn08QIAR" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7CVYIA3" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000CvpepMAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS / WB FCV KM",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-11-12").ToUniversalTime(),
                        Subject = "UNOPS / WB FCV KM",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvpepMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7fxJIAR" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7Md9IAF" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-31").ToUniversalTime(),
                        Subject = "Confirmed: WB SFAs - Joint UN approach on SEA/SH reporting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Cvsb6MAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000MvyhCIAR" },
                    new List<int> { 1244 },
                    new List<string> { "alistairs@unops.org", "christinebo@unops.org", "nivesc@unops.org", "kelleys@unops.org", "juliasc@unops.org", "vineshw@unops.org", "elizabethdu@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                    "00UQx00000CvuWPMAZ",
                    new UNOPSInteraction
                    {
                        Name = "LEGEN-UNOPS informal discussion",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2024-10-16").ToUniversalTime(),
                        Subject = "LEGEN-UNOPS informal discussion",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000CvuWPMAZ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Kn08QIAR" },
                    new List<int> { 1646 },
                    new List<string> { "christinebo@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        EmailAddresses = new List<string> { "alistairs@unops.org", "tbelcheva@unicef.org", "christinebo@unops.org", "asuley@unicef.org", "jrennie@unicef.org", "rawad.assaad@wfp.org", "mpc.contracting@wfp.org", "irene.spaziani@fao.org", "elizabethdu@unops.org", "meran.lukic@fao.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("christinebo@unops.org".ToLower()) ? paoUserEmailMapping["christinebo@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "marijab@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000D5ebgMAB",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/Norge - catch-up forud for EB",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-01-17").ToUniversalTime(),
                        Subject = "UNOPS/Norge - catch-up forud for EB",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000D5ebgMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                    "00UQx00000D5kqkMAB",
                    new UNOPSInteraction
                    {
                        Name = "Catch-up: UNOPS/Sweden",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-01-20").ToUniversalTime(),
                        Subject = "Catch-up: UNOPS/Sweden",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000D5kqkMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1116 },
                    new List<string> { "alistairs@unops.org", "patrickel@unops.org", "eleneag@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Yght0IAB" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "eleneag@unops.org", "christinebo@unops.org" },
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("jeromedt@unops.org".ToLower()) ? paoUserEmailMapping["jeromedt@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YVwCGIA1" },
                    new List<int> {  },
                    new List<string> { "jeromedt@unops.org", "martina@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Yr2rOIAR" },
                    new List<int> { 1193 },
                    new List<string> { "kajsah@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DFAPNMA5",
                    new UNOPSInteraction
                    {
                        Name = "Intro: Finland / UNOPS",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-01-23").ToUniversalTime(),
                        Subject = "Intro: Finland / UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DFAPNMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("laetitiak@unops.org".ToLower()) ? paoUserEmailMapping["laetitiak@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1029 },
                    new List<string> { "laetitiak@unops.org", "marijab@unops.org", "mariacarmenco@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DM813MAD",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS Washington <> World Bank Geneva",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-01-24").ToUniversalTime(),
                        Subject = "UNOPS Washington <> World Bank Geneva",
                        Description = "Notes: \n\nChristine updated on UNOPS corporate plans, including the UN-PBSO workshop in Nairobi, Client Board, Helsingor Dialogue and ED’s plan to visit Washington for the Spring Meetings.\n\nMaria updated on recent WBG External and Corporate Relations reorganisation to strengthen cooperation between the teams representing the Bank to the UN in New York and in Geneva. Geneva office hosts between 30 and 40 events and dialogues per year to launch reports; partner with think tanks; and discuss ideas from proven solutions and impact evaluations to explore operational partnerships. \n\nWB also addressed UNOPS ED’s chapter in a new Center for Global Development paper on international development architecture reform. The piece mentions the Bretton Woods Institutions and needs for reform.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DM813MAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        EmailAddresses = new List<string> { "alistairs@unops.org", "adriana.bonomo@wfp.org", "meran.lukic@fao.org", "christinebo@unops.org", "asuley@unicef.org", "apuentes@iom.int", "camila.sanchezugalde@fao.org", "jrennie@unicef.org", "rawad.assaad@wfp.org", "mpc.contracting@wfp.org", "irene.spaziani@fao.org", "micol.mulon@wfp.org", "elizabethdu@unops.org", "tbelcheva@unicef.org", "alvaro.ibares@fao.org", "jorge.teunissen@fao.org" },
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Y7VmsIAF" },
                    new List<int> {  },
                    new List<string> { "alistairs@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000DVpOXMA1",
                    new UNOPSInteraction
                    {
                        Name = "World Bank Conference on Public Institutions for Development: Enabling the Private Sector.",
                        Type = InteractionType.Other,
                        Date = DateTime.UtcNow,
                        Subject = "World Bank Conference on Public Institutions for Development: Enabling the Private Sector.",
                        Description = "Notes: https://docs.google.com/document/d/158EIlCRCASmb1o7Jhv5XLpNjWQVV4flc3EcmugyMZ_4/edit?tab=t.0#heading=h.5ca1aotkz6qq",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000DVpOXMA1",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000A9u0RIAR" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "patrickel@unops.org", "eleneag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000Dk5RqMAJ",
                    new UNOPSInteraction
                    {
                        Name = "Catch up with Million Fikre - WB operational matters",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-01-28").ToUniversalTime(),
                        Subject = "Catch up with Million Fikre - WB operational matters",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Dk5RqMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("marialk@unops.org".ToLower()) ? paoUserEmailMapping["marialk@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000aURHUIA4" },
                    new List<int> { 1322 },
                    new List<string> { "marialk@unops.org", "isabelaf@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                    "00UQx00000ECRHKMA5",
                    new UNOPSInteraction
                    {
                        Name = "Exploración alianzas CCIC_UNOPS",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-02-24").ToUniversalTime(),
                        Subject = "Exploración alianzas CCIC_UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000ECRHKMA5",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("anav@unops.org".ToLower()) ? paoUserEmailMapping["anav@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-02-27").ToUniversalTime(),
                        Subject = "UNOPS-UN Women meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EEDF3MAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("freyavg@unops.org".ToLower()) ? paoUserEmailMapping["freyavg@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000Tsty6IAB" },
                    new List<int> { 1222 },
                    new List<string> { "freyavg@unops.org", "robertgodin@unops.org", "arnauds@unops.org", "daniele@unops.org", "mikaelag@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EEO7FMAX",
                    new UNOPSInteraction
                    {
                        Name = "Discussion (ll) on the agenda for the meeting: ASG Madi & ASG Damkjær - Partnership Dialogue",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-01-23").ToUniversalTime(),
                        Subject = "Discussion (ll) on the agenda for the meeting: ASG Madi & ASG Damkjær - Partnership Dialogue",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EEO7FMAX",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("mikaelag@unops.org".ToLower()) ? paoUserEmailMapping["mikaelag@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000b1faOIAQ" },
                    new List<int> { 1101 },
                    new List<string> { "asbjornb@unops.org", "marijab@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EJ1ZrMAL",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/WB sustainable procurement",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-02-27").ToUniversalTime(),
                        Subject = "UNOPS/WB sustainable procurement",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EJ1ZrMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000DUQTuIAP" },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "trexylcm@unops.org", "christinebo@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EJINnMAP",
                    new UNOPSInteraction
                    {
                        Name = "UNOPS/Andrew Hyde - @ Stimson Center",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-02-27").ToUniversalTime(),
                        Subject = "UNOPS/Andrew Hyde - @ Stimson Center",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EJINnMAP",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-03-05").ToUniversalTime(),
                        Subject = "UNOPS/WB OPCS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EQtgkMAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                    "00UQx00000EaRhZMAV",
                    new UNOPSInteraction
                    {
                        Name = "World Bank MENA meeting",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-03-10").ToUniversalTime(),
                        Subject = "World Bank MENA meeting",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EaRhZMAV",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-03-13").ToUniversalTime(),
                        Subject = "Meeting with NEA Gaza Reconstruction",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EgRJuMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "banak@unops.org", "usmana@unops.org", "patrickel@unops.org", "marijab@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1113 },
                    new List<string> { "alistairs@unops.org", "banak@unops.org", "usmana@unops.org", "patrickel@unops.org", "marijab@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EgUUhMAN",
                    new UNOPSInteraction
                    {
                        Name = "UN IFI CAS WG",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-03-14").ToUniversalTime(),
                        Subject = "UN IFI CAS WG",
                        Description = "Microsoft Teams Need help?<https://aka.ms/JoinTeamsMeeting?omkt=en-US>\nJoin the meeting now<https://teams.microsoft.com/l/meetup-join/19%3ameeting_NjIxZDZmMzUtMTU4YS00MDg2LTk0NTAtODU5MzAyYjYxYjQ3%40thread.v2/0?context=%7b%22Tid%22%3a%220f9e35db-544f-4f60-bdcc-5ea416e6dc70%22%2c%22Oid%22%3a%22551c519d-39eb-4b6d-97cb-2433a86e33c8%22%7d>\nMeeting ID: 383 965 672 533\nPasscode: c9pd9fv7",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EgUUhMAN",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-03-14").ToUniversalTime(),
                        Subject = "WB Infrastructure, MENA Regional Director Ms Almud Weitz",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Eh2o5MAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("alistairs@unops.org".ToLower()) ? paoUserEmailMapping["alistairs@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1646 },
                    new List<string> { "alistairs@unops.org", "banak@unops.org", "usmana@unops.org", "christinebo@unops.org", "marijab@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-03-07").ToUniversalTime(),
                        Subject = "Standard Template for Finland/UNOPS engagements",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000Ewj1gMAB",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000dC9HDIA0" },
                    new List<int> { 1087 },
                    new List<string> { "asbjornb@unops.org", "franciscoca@unops.org", "devorahfd@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000EwqFtMAJ",
                    new UNOPSInteraction
                    {
                        Name = "FCDO / UNOPS",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-03-13").ToUniversalTime(),
                        Subject = "FCDO / UNOPS",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000EwqFtMAJ",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-04-10").ToUniversalTime(),
                        Subject = "UNS @ CODEWAY2025",
                        Description = "As mentioned in the previous message, this meeting is to put in common the address to Private Sector participation in UN biddings, considering current challenges and situations.",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000FPqu6MAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-04-07").ToUniversalTime(),
                        Subject = "Carlo Batori - Farnesina",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000FRA14MAH",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-05-15").ToUniversalTime(),
                        Subject = "Finland/UNOPS: Annual EB session",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GYeUsMAL",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000YQDnqIAH" },
                    new List<int> { 1087 },
                    new List<string> { "asbjornb@unops.org", "naimoh@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> {  },
                    new List<int> { 1086 },
                    new List<string> { "arunn@unops.org", "vladyslavk@unops.org", "eleneag@unops.org", "marysiaz@unops.org", "asbjornb@unops.org" },
                    new List<string> { "B0047" }
                ),
                new (
                    "00UQx00000GauiQMAR",
                    new UNOPSInteraction
                    {
                        Name = "Snihurivka and Arkas - next steps",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-06-04").ToUniversalTime(),
                        Subject = "Snihurivka and Arkas - next steps",
                        Description = "<ul><li>Intro between UNOPS and Sahil</li><li>Present <a href=\"https://drive.google.com/file/d/1L9VXI6rO2j0JP60JR3iVywPwSZAXUFgF/view?usp=sharing\">preliminary assessment</a> of reconstruction of Snihurivka High School</li><li>Discuss next steps for <a href=\"https://drive.google.com/file/d/1rCuzlYV25eJJJvJg637uQWl54pb_JMp7/view?usp=sharing\">reconstruction of Arkas High School</a></li></ul>",
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000GauiQMAR",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedDate = DateTime.UtcNow,
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
                        CreatedBy = paoUserEmailMapping.ContainsKey("antoinel@unops.org".ToLower()) ? paoUserEmailMapping["antoinel@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000hW3V6IAK" },
                    new List<int> { 1439 },
                    new List<string> { "antoinel@unops.org", "jean-vincentc@unops.org" },
                    new List<string> { "B5416" }
                ),
                new (
                    "00UQx00000HOim5MAD",
                    new UNOPSInteraction
                    {
                        Name = "OECS - UNOPS | Advancing DIalogue on Sustainable Procurement",
                        Type = InteractionType.Other,
                        Date = DateTime.Parse("2025-06-26").ToUniversalTime(),
                        Subject = "OECS - UNOPS | Advancing DIalogue on Sustainable Procurement",
                        Description = null,
                        Location = null,
                        GmailThreadId = null,
                        GmailMessageId = "00UQx00000HOim5MAD",
                        EmailAddresses = new List<string>(),
                        Status = (EntityStatus)1,
                        CreatedBy = paoUserEmailMapping.ContainsKey("antoinel@unops.org".ToLower()) ? paoUserEmailMapping["antoinel@unops.org".ToLower()] : 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    },
                    new List<string> { "003Qx00000j1ok8IAA" },
                    new List<int> { 1577 },
                    new List<string> { "antoinel@unops.org" },
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
                        CreatedDate = DateTime.UtcNow,
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

                    if (existingInteraction != null)
                    {
                        // Update existing interaction
                        existingInteraction.Name = interactionData.Name;
                        existingInteraction.Type = interactionData.Type;
                        existingInteraction.Date = interactionData.Date;
                        existingInteraction.Subject = interactionData.Subject;
                        existingInteraction.Description = interactionData.Description;
                        existingInteraction.Location = interactionData.Location;
                        existingInteraction.GmailThreadId = interactionData.GmailThreadId;
                        existingInteraction.EmailAddresses = interactionData.EmailAddresses;
                        existingInteraction.Status = interactionData.Status;
                        existingInteraction.LastModifiedBy = 0;
                        existingInteraction.LastModifiedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add new interaction to context
                        context.Interactions.Add(interactionData);
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