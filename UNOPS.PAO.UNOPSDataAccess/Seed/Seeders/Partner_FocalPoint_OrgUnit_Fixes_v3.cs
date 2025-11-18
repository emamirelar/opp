using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Partner_FocalPoint_OrgUnit_Fixes_v3
    {
        public static async Task UpdatePartnerFocalPointAndOrgUnitAsync(UNOPSAppDbContext context)
        {
            // Create mapping from PAOUser Email to Id (handle duplicates by taking first, filter out null emails)
            // Convert emails to lowercase for case-insensitive matching
            var paoUsers = await context.PAOUsers
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();
            var paoUserMapping = paoUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .GroupBy(u => u.Email!.ToLower())
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Create mapping from OrganizationHierarchy Description to Id (only OrgUnit type)
            var orgUnits = await context.OrganizationHierarchies
                .Where(o => o.Type == OrganizationUnitType.OrgUnit)
                .Select(o => new { o.Id, o.Description })
                .ToListAsync();
            var orgUnitMapping = orgUnits
                .Where(o => !string.IsNullOrEmpty(o.Description))
                .GroupBy(o => o.Description)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Define partner updates data structure
            var partnerUpdates = new List<PartnerUpdateData>
            {
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "UN Integrated Strategy for the Sahel (UNISS)",
                    LegacyFocalPointUser = "abdoulazizs@unops.org",
                    SuggestedFocalPoint = "abdoulazizs@unops.org",
                    LegacyOrgUnit = "AFR, WAMCO, West Africa MCO",
                    SuggestedOrgUnit = "AFR, WAMCO, West Africa MCO"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1105,
                    Name = "KOICA Korea International Cooperation Agency",
                    LegacyFocalPointUser = "arnauds@unops.org",
                    SuggestedFocalPoint = "arnauds@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1139,
                    Name = "Republic of Korea",
                    LegacyFocalPointUser = "arnauds@unops.org",
                    SuggestedFocalPoint = "arnauds@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1908,
                    Name = "South Korea Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "arnauds@unops.org",
                    SuggestedFocalPoint = "arnauds@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1086,
                    Name = "Ministry of Foreign Affairs of Denmark",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1087,
                    Name = "Ministry for Foreign Affairs of Finland",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1101,
                    Name = "NORAD Norwegian Agency for Development Cooperation",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1102,
                    Name = "Ministry of Foreign Affairs of Norway",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1108,
                    Name = "SIDA Swedish International Development Cooperation Agency",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1111,
                    Name = "DFID Department For International Development",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1123,
                    Name = "Denmark",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1124,
                    Name = "Finland",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1136,
                    Name = "Norway",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1144,
                    Name = "United Kingdom of Great Britain and Northern Ireland",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1267,
                    Name = "Sweden",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1610,
                    Name = "British overseas territory of Anguilla",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1613,
                    Name = "British overseas territory of Cayman Islands",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1618,
                    Name = "British overseas territory of Montserrat",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1711,
                    Name = "Fleming Fund",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1752,
                    Name = "FCDO Foreign, Commonwealth & Development Office",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1753,
                    Name = "Ministry of Justice of Norway",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1754,
                    Name = "SEPA Swedish Environmental Protection Agency",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1837,
                    Name = "Norwegian Refugee Council",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1909,
                    Name = "Sweden Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1910,
                    Name = "DESNZ Department for Energy Security and Net Zero",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Ministry of Climate, Energy and Utilities of Denmark",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Carlsberg Group A/S",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "IFU - Impact Fund Denmark",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Human Practice Foundation",
                    LegacyFocalPointUser = "asbjornb@unops.org",
                    SuggestedFocalPoint = "asbjornb@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1261,
                    Name = "WHO World Health Organization",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = "daniele@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1445,
                    Name = "Gavi The Vaccine Alliance",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = "daniele@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1448,
                    Name = "GFATM Global Fund to Fight AIDS, Tuberculosis and Malaria",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = "daniele@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1679,
                    Name = "GFATM-AID Global Fund to fight AIDS",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = "daniele@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1680,
                    Name = "GFATM-TUB Global Fund to fight Tuberculosis",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = "daniele@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1681,
                    Name = "GFATM-MAL Global Fund to fight Malaria",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = "daniele@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1312,
                    Name = "Kuwait",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1319,
                    Name = "Oman",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1371,
                    Name = "Qatar",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1395,
                    Name = "Saudi Arabia",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1425,
                    Name = "United Arab Emirates",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1456,
                    Name = "Mohammed bin Rashid Al Maktoum Foundation",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1571,
                    Name = "IsDB Islamic Development Bank",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1723,
                    Name = "SFD Saudi Fund for Development",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1761,
                    Name = "KSRelief King Salman Humanitarian Aid and Relief Centre",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1818,
                    Name = "QFFD Qatar Fund for Development",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1911,
                    Name = "Saudi Arabia Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1912,
                    Name = "Qatar Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1913,
                    Name = "United Arab Emirates Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1914,
                    Name = "Kuwait Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1915,
                    Name = "KFAED Kuwait Fund for Arab Economic Development",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1916,
                    Name = "Oman Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1918,
                    Name = "SDRPY Saudi Development and Reconstruction Program for Yemen",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1919,
                    Name = "Qatar Charity",
                    LegacyFocalPointUser = "halas@unops.org",
                    SuggestedFocalPoint = "halas@unops.org",
                    LegacyOrgUnit = "MR, MROD, Office of the RD",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1903,
                    Name = "France Ministry for Europe and Foreign Affairs",
                    LegacyFocalPointUser = "laetitiak@unops.org",
                    SuggestedFocalPoint = "laetitiak@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Camara de Comercio de Cortes",
                    LegacyFocalPointUser = "lauragi@unops.org",
                    SuggestedFocalPoint = "lauragi@unops.org",
                    LegacyOrgUnit = "LCR, CEMCO, Honduras",
                    SuggestedOrgUnit = "LCR, CEMCO, Honduras"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Secretaría de Relaciones Exteriores y Cooperación Internacional",
                    LegacyFocalPointUser = "lauragi@unops.org",
                    SuggestedFocalPoint = "lauragi@unops.org",
                    LegacyOrgUnit = "LCR, CEMCO, Honduras",
                    SuggestedOrgUnit = "LCR, CEMCO, Honduras"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1183,
                    Name = "UNHCR Office of the United Nations High Commissioner for Refugees",
                    LegacyFocalPointUser = "laurentium@unops.org",
                    SuggestedFocalPoint = "laurentium@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1182,
                    Name = "ITC International Trade Centre",
                    LegacyFocalPointUser = "lorrainea@unops.org",
                    SuggestedFocalPoint = "lorrainea@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "HR services for partners",
                    LegacyFocalPointUser = "lorrainea@unops.org",
                    SuggestedFocalPoint = "lorrainea@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "MONUSCO",
                    LegacyFocalPointUser = "lorrainea@unops.org",
                    SuggestedFocalPoint = "lorrainea@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "AEF Africa-Europe Foundation",
                    LegacyFocalPointUser = "celiaafricak@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "EU DG CLIMA Directorate-General for Climate Action - Prospect",
                    LegacyFocalPointUser = "celiaafricak@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1025,
                    Name = "EU DG INTPA Directorate-General for International Partnerships",
                    LegacyFocalPointUser = "mariacarmenco@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1026,
                    Name = "EU DG ENEST, Directorate-General for Enlargement and Eastern Neighbourhood",
                    LegacyFocalPointUser = "mariacarmenco@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1029,
                    Name = "EU DG ECHO Directorate-General for European Civil Protection and Humanitarian Aid Operations",
                    LegacyFocalPointUser = "mariacarmenco@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1864,
                    Name = "BMWE German Federal Ministry for Economic Affairs and Energy",
                    LegacyFocalPointUser = "mariacarmenco@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "FPI - European Commission",
                    LegacyFocalPointUser = "mariacarmenco@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Ministry of Foreign Affairs of Italy",
                    LegacyFocalPointUser = "mariacarmenco@unops.org",
                    SuggestedFocalPoint = "mariacarmenco@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1247,
                    Name = "IFAD International Fund for Agricultural Development",
                    LegacyFocalPointUser = "martina@unops.org",
                    SuggestedFocalPoint = "martina@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1902,
                    Name = "AICS Italian Agency for Development Cooperation",
                    LegacyFocalPointUser = "martina@unops.org",
                    SuggestedFocalPoint = "martina@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1904,
                    Name = "MAECI Italian Ministry of Foreign Affairs and International Cooperation",
                    LegacyFocalPointUser = "martina@unops.org",
                    SuggestedFocalPoint = "martina@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1905,
                    Name = "Italy Ministry of Environment",
                    LegacyFocalPointUser = "martina@unops.org",
                    SuggestedFocalPoint = "martina@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "UN in Rome",
                    LegacyFocalPointUser = "martina@unops.org",
                    SuggestedFocalPoint = "martina@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Comunità Sant'Egidio",
                    LegacyFocalPointUser = "martina@unops.org",
                    SuggestedFocalPoint = "martina@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Extra Supports",
                    LegacyFocalPointUser = "martina@unops.org",
                    SuggestedFocalPoint = "martina@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "RCO Mali",
                    LegacyFocalPointUser = "michaeld@unops.org",
                    SuggestedFocalPoint = "michaeld@unops.org",
                    LegacyOrgUnit = "AFR, WAMCO, Mali",
                    SuggestedOrgUnit = "AFR, WAMCO, Mali"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1222,
                    Name = "UN WOMEN United Nations Entity for Gender Equality and the Empowerment of Women",
                    LegacyFocalPointUser = "mikaelag@unops.org",
                    SuggestedFocalPoint = "mikaelag@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "JICA Japón",
                    LegacyFocalPointUser = "mildredt@unops.org",
                    SuggestedFocalPoint = "mildredt@unops.org",
                    LegacyOrgUnit = "LCR, CEMCO, Honduras",
                    SuggestedOrgUnit = "LCR, CEMCO, Honduras"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1192,
                    Name = "UNEP United Nations Environment Programme",
                    LegacyFocalPointUser = "kajsah@unops.org",
                    SuggestedFocalPoint = "norikok@unops.org",
                    LegacyOrgUnit = "GPO, GVA, Water, Environment & Climate (WEC)",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1193,
                    Name = "UN-HABITAT United Nations Human Settlements Programme",
                    LegacyFocalPointUser = "kajsah@unops.org",
                    SuggestedFocalPoint = "norikok@unops.org",
                    LegacyOrgUnit = "GPO, GVA, Water, Environment & Climate (WEC)",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1443,
                    Name = "IBRD International Bank of Reconstruction and Development",
                    LegacyFocalPointUser = "christinebo@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1628,
                    Name = "ICSID International Centre for Settlement of Investment Disputes",
                    LegacyFocalPointUser = "christinebo@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1646,
                    Name = "The World Bank",
                    LegacyFocalPointUser = "christinebo@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1112,
                    Name = "USAID United States Agency for International Development",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1113,
                    Name = "USDOS United States Department of State",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1114,
                    Name = "MCC Millennium Challenge Corporation",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1115,
                    Name = "USDA United States Department of Agriculture",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1116,
                    Name = "USAID Bureau of Humanitarian Assistance",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1641,
                    Name = "USAID and Affiliated U.S. Agency for International Development and Affiliated",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1788,
                    Name = "INL U.S. Department of State’s Bureau of International Narcotics and Law Enforcement Affairs",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1898,
                    Name = "USMBHC The United States-Mexico Border Health Commission",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1917,
                    Name = "DND Department of National Defence",
                    LegacyFocalPointUser = "patrickel@unops.org",
                    SuggestedFocalPoint = "patrickel@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Japan Embassy Conakry",
                    LegacyFocalPointUser = "seynaboud@unops.org",
                    SuggestedFocalPoint = "seynaboud@unops.org",
                    LegacyOrgUnit = "AFR, WAMCO, Senegal",
                    SuggestedOrgUnit = "AFR, WAMCO, Senegal"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Japan Embassy Guinea",
                    LegacyFocalPointUser = "seynaboud@unops.org",
                    SuggestedFocalPoint = "seynaboud@unops.org",
                    LegacyOrgUnit = "AFR, WAMCO, Senegal",
                    SuggestedOrgUnit = "AFR, WAMCO, Senegal"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "British Virgin Islands",
                    LegacyFocalPointUser = "williamsg@unops.org",
                    SuggestedFocalPoint = "williamsg@unops.org",
                    LegacyOrgUnit = "LCR, ICMCO, Costa Rica",
                    SuggestedOrgUnit = "LCR, ICMCO, Costa Rica"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Ministry of Health, Labour and Welfare (MHLW) Japan",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Ministry of Economy, Trade and Industry (METI) Japan",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "(To delete) Japanese private companies",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "AAIC Japan Co., Ltd.",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Hotel New Otani Tokyo",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "NEC Corporation",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Allm Inc.",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Nomura Research Institute (NRI)",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Twinbird Corporation",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Yamaha Motor Co., Ltd.",
                    LegacyFocalPointUser = "yuichis@unops.org",
                    SuggestedFocalPoint = "yuichis@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1095,
                    Name = "JICA Japan International Cooperation Agency",
                    LegacyFocalPointUser = "yukom@unops.org",
                    SuggestedFocalPoint = "yukom@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1131,
                    Name = "Japan",
                    LegacyFocalPointUser = "yukom@unops.org",
                    SuggestedFocalPoint = "yukom@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1906,
                    Name = "Japan Ministry of Foreign Affairs",
                    LegacyFocalPointUser = "yukom@unops.org",
                    SuggestedFocalPoint = "yukom@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1907,
                    Name = "Japan Ministry of Health, Labour and Welfare",
                    LegacyFocalPointUser = "yukom@unops.org",
                    SuggestedFocalPoint = "yukom@unops.org",
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1928,
                    Name = "The Energy Foundation",
                    LegacyFocalPointUser = "isabelaf@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1931,
                    Name = "SED Fund",
                    LegacyFocalPointUser = "isabelaf@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = null,
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1943,
                    Name = "EU DG MENA, Directorate-General for the Middle East, North Africa and the Gulf",
                    LegacyFocalPointUser = "isabelaf@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1944,
                    Name = "EU DG CLIMA, Directorate-General for Climate Action",
                    LegacyFocalPointUser = "isabelaf@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1949,
                    Name = "Water PNG Limited",
                    LegacyFocalPointUser = "isabelaf@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1959,
                    Name = "Ministry of Economic Affairs of the Netherlands",
                    LegacyFocalPointUser = "isabelaf@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1960,
                    Name = "Windward Fund",
                    LegacyFocalPointUser = "isabelaf@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1876,
                    Name = "Clinton Health Access Initiative",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1877,
                    Name = "Instituto Costarricense de Ferrocarriles",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1878,
                    Name = "Comisión Nacional de Prevención de Riesgos y Atención de Emergencias",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1879,
                    Name = "Global Alliance for a Sustainable Planet",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1880,
                    Name = "Fund for Reconstruction and Development of the Republic of Uzbekistan",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1881,
                    Name = "The Climate and Society Institute (ICS)",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1882,
                    Name = "Plan International Liberia",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1883,
                    Name = "Minderoo Foundation",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1885,
                    Name = "Global Climate Action Partnership (GCAP) Multi-Donor Fund",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1886,
                    Name = "International Medical Corps UK",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1887,
                    Name = "Sergey Brin Family Foundation",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1888,
                    Name = "International Medical Corps US",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1889,
                    Name = "Global Institute For Disease Elimination",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1890,
                    Name = "Empresa Publica Metropolitana de Movilidad y Obras Publicas",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1891,
                    Name = "Kharkiv City Council",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1892,
                    Name = "CPP - Companhia Paulista de Parcerias",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1893,
                    Name = "The Forest and Climate Leaders’ Partnership",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1894,
                    Name = "Baylor College of Medicine Children’s Foundation Eswatini",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1895,
                    Name = "Rockefeller Brothers Fund",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1896,
                    Name = "NATO North Atlantic Treaty Organisation",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1897,
                    Name = "PSI Population Services International Madagascar",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1899,
                    Name = "Swaniti Initiative",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1900,
                    Name = "CONAB Companhia Nacional de Abastecimento",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1901,
                    Name = "Plan International Mali",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1920,
                    Name = "APRA Accelerated Partnerships for Renewables in Africa",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1921,
                    Name = "DRC Danish Refugee Council",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1922,
                    Name = "ZESCO Limited",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1923,
                    Name = "RF Catalytic Capital",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1924,
                    Name = "Fuel Distribution Gaza",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1925,
                    Name = "West African Development Bank",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1926,
                    Name = "ANSES Agencia Nacional de la Seguridad Social",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1927,
                    Name = "IGSS Instituto Guatemalteco de la Seguridad Social",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1929,
                    Name = "Sow & Reap Agro Private Limited",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1930,
                    Name = "Loughborough University",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1933,
                    Name = "Nehemia Christliches Hilfswerk e. V.",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1934,
                    Name = "The Regents of the University of California, on behalf of its Davis campus (UC Davis)",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1936,
                    Name = "Caixa Econômica Federal",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1937,
                    Name = "Stellenbosh University",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1938,
                    Name = "EIF-3 Enhanced Integrated Framework Phase 3",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1939,
                    Name = "Google LLC",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1940,
                    Name = "Ville de Nice",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1941,
                    Name = "The University of Sydney",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1942,
                    Name = "COG Comite Olimpico Guatemalteco",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1945,
                    Name = "Cygnum Capital Asset Management Ltd.",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1946,
                    Name = "ENEE Empresa Nacional de Energía Eléctrica",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1947,
                    Name = "Parexel International LLC",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1948,
                    Name = "Maisha Healthcare Inc.",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1950,
                    Name = "Microbac SPA",
                    LegacyFocalPointUser = "joseme@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, NYPO, UN Technology Support Services",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1932,
                    Name = "Federación Red NicaSalud",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1935,
                    Name = "UNDP Multi-Partner Trust Fund Office",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1951,
                    Name = "EMPORNAC Empresa Portuaria Nacional Santo Tomás de Castilla",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1952,
                    Name = "Chernihiv City Council",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1953,
                    Name = "Kremenchuk City Council",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1954,
                    Name = "Kryvyi Rih City Council",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1955,
                    Name = "Mykolaiv City Council",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1956,
                    Name = "Slavuta City Council",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1957,
                    Name = "Sumy City Council",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1958,
                    Name = "AFEF",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1961,
                    Name = "Empresa Electricidad Del Peru - Electroperu S A",
                    LegacyFocalPointUser = "michaelri@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = null,
                    Name = "Asian Institute of Technology (AIT)",
                    LegacyFocalPointUser = "mohammedameers@unops.org",
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "GPO, GVA, Geneva",
                    SuggestedOrgUnit = null
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1109,
                    Name = "SDC Swiss Agency for Development and Cooperation",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
                new PartnerUpdateData
                {
                    ErpDimValue = 1142,
                    Name = "Switzerland",
                    LegacyFocalPointUser = null,
                    SuggestedFocalPoint = null,
                    LegacyOrgUnit = "DP, PLG, Partnerships and Liaison Group",
                    SuggestedOrgUnit = "DP, PLG, Partnerships and Liaison Group"
                },
            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                int partnersProcessed = 0;
                int focalPointUpdates = 0;
                int orgUnitUpdates = 0;
                int orgUnitDeletions = 0;

                foreach (var data in partnerUpdates)
                {
                    // Find the partner
                    Partner? partner = null;
                    
                    if (data.ErpDimValue.HasValue)
                    {
                        // Lookup by ErpDimValue
                        partner = await context.Partners
                            .Include(p => p.PartnerFocalPointUser)
                            .FirstOrDefaultAsync(p => p.ErpDimValue == data.ErpDimValue);
                    }
                    else if (!string.IsNullOrEmpty(data.Name))
                    {
                        // Lookup by Name where ErpDimValue is null
                        partner = await context.Partners
                            .Include(p => p.PartnerFocalPointUser)
                            .FirstOrDefaultAsync(p => p.Name == data.Name && p.ErpDimValue == null);
                    }

                    // Load organization unit relationships separately (polymorphic relationship)
                    if (partner != null)
                    {
                        var orgUnitRelationships = await context.OrganizationUnitRelationships
                            .Include(our => our.OrganizationHierarchy)
                            .Where(our => our.EntityId == partner.Id && our.EntityType == nameof(Partner) && !our.IsDeleted)
                            .ToListAsync();
                        
                        partner.OrganizationUnitRelationships = orgUnitRelationships;
                    }

                    if (partner == null)
                    {
                        Console.WriteLine($"Warning: Partner not found - ErpDimValue: {data.ErpDimValue}, Name: {data.Name}");
                        continue;
                    }

                    bool partnerModified = false;
                    string partnerIdentifier = $"{partner.Name} (ErpDimValue: {partner.ErpDimValue ?? 0})";

                    // ========== FOCAL POINT LOGIC ==========
                    
                    // If SuggestedFocalPoint is null, set FocalPointUserId to null
                    if (string.IsNullOrEmpty(data.SuggestedFocalPoint))
                    {
                        if (partner.PartnerFocalPointUserId != null)
                        {
                            partner.PartnerFocalPointUserId = null;
                            partner.LastModifiedBy = -1; // System user
                            partner.LastModifiedDate = DateTime.UtcNow;
                            partnerModified = true;
                            focalPointUpdates++;
                            Console.WriteLine($"Cleared FocalPoint for Partner: {partnerIdentifier}");
                        }
                    }
                    // If SuggestedFocalPoint is not null, update only if the FocalPoint in database is null OR matches LegacyFocalPoint
                    else
                    {
                        // Check if current focal point matches legacy
                        int? legacyUserId = data.LegacyFocalPointUser != null ? paoUserMapping.ContainsKey(data.LegacyFocalPointUser) 
                            ? paoUserMapping[data.LegacyFocalPointUser] 
                            : (int?)null : (int?)null;

                        if (partner.PartnerFocalPointUserId == null || partner.PartnerFocalPointUserId == legacyUserId)
                        {
                            // Get the suggested focal point user ID
                            if (paoUserMapping.ContainsKey(data.SuggestedFocalPoint))
                            {
                                int suggestedUserId = paoUserMapping[data.SuggestedFocalPoint];
                                partner.PartnerFocalPointUserId = suggestedUserId;
                                partner.LastModifiedBy = -1; // System user
                                partner.LastModifiedDate = DateTime.UtcNow;
                                partnerModified = true;
                                focalPointUpdates++;
                                Console.WriteLine($"Updated FocalPoint for Partner: {partnerIdentifier} from '{data.LegacyFocalPointUser}' to '{data.SuggestedFocalPoint}'");
                            }
                            else
                            {
                                Console.WriteLine($"Warning: SuggestedFocalPoint user '{data.SuggestedFocalPoint}' not found for Partner: {partnerIdentifier}");
                            }
                        }
                    }

                    // ========== ORGANIZATION UNIT LOGIC ==========

                    // If SuggestedOrgUnit is null, delete the relationship
                    if (string.IsNullOrEmpty(data.SuggestedOrgUnit))
                    {
                        if (!string.IsNullOrEmpty(data.LegacyOrgUnit) && orgUnitMapping.ContainsKey(data.LegacyOrgUnit))
                        {
                            int legacyOrgUnitId = orgUnitMapping[data.LegacyOrgUnit];
                            var relationshipsToRemove = partner.OrganizationUnitRelationships
                                .Where(r => r.OrganizationHierarchyId == legacyOrgUnitId && !r.IsDeleted)
                                .ToList();

                            foreach (var relationship in relationshipsToRemove)
                            {
                                relationship.IsDeleted = true;
                                relationship.DeletedBy = -1;
                                relationship.DeletedDate = DateTime.UtcNow;
                                orgUnitDeletions++;
                                Console.WriteLine($"Deleted OrgUnit relationship '{data.LegacyOrgUnit}' for Partner: {partnerIdentifier}");
                            }

                            if (relationshipsToRemove.Any())
                            {
                                partnerModified = true;
                            }
                        }
                    }
                    // If SuggestedOrgUnit is not null update or create OrganizationUnitRelationship
                    else
                    // && data.SuggestedOrgUnit != data.LegacyOrgUnit)
                    {
                        if (!orgUnitMapping.ContainsKey(data.SuggestedOrgUnit))
                        {
                            Console.WriteLine($"Warning: SuggestedOrgUnit '{data.SuggestedOrgUnit}' not found for Partner: {partnerIdentifier}");
                            continue;
                        }

                        int suggestedOrgUnitId = orgUnitMapping[data.SuggestedOrgUnit];

                        // Check if there's an existing relationship to update
                        OrganizationUnitRelationship? existingRelationship = null;
                        
                        if (!string.IsNullOrEmpty(data.LegacyOrgUnit) && orgUnitMapping.ContainsKey(data.LegacyOrgUnit))
                        {
                            int legacyOrgUnitId = orgUnitMapping[data.LegacyOrgUnit];
                            existingRelationship = partner.OrganizationUnitRelationships
                                .FirstOrDefault(r => r.OrganizationHierarchyId == legacyOrgUnitId && !r.IsDeleted);
                        }

                        if (existingRelationship != null)
                        {
                            // Update existing relationship
                            existingRelationship.OrganizationHierarchyId = suggestedOrgUnitId;
                            existingRelationship.Name = $"Partner-{partner.Id}-{suggestedOrgUnitId}";
                            existingRelationship.LastModifiedBy = -1;
                            existingRelationship.LastModifiedDate = DateTime.UtcNow;
                            orgUnitUpdates++;
                            Console.WriteLine($"Updated OrgUnit for Partner: {partnerIdentifier} from '{data.LegacyOrgUnit}' to '{data.SuggestedOrgUnit}'");
                        }
                        else
                        {
                            // Check if relationship already exists for the suggested org unit
                            var alreadyExists = partner.OrganizationUnitRelationships
                                .Any(r => r.OrganizationHierarchyId == suggestedOrgUnitId && !r.IsDeleted);

                            if (!alreadyExists)
                            {
                                // Create new relationship
                                var newRelationship = new OrganizationUnitRelationship
                                {
                                    OrganizationHierarchyId = suggestedOrgUnitId,
                                    EntityId = partner.Id,
                                    EntityType = nameof(Partner),
                                    Name = $"Partner-{partner.Id}-{suggestedOrgUnitId}",
                                    Status = EntityStatus.Active,
                                    CreatedBy = -1,
                                    CreatedDate = DateTime.UtcNow,
                                    LastModifiedBy = -1,
                                    LastModifiedDate = DateTime.UtcNow
                                };
                                context.OrganizationUnitRelationships.Add(newRelationship);
                                orgUnitUpdates++;
                                Console.WriteLine($"Created new OrgUnit relationship '{data.SuggestedOrgUnit}' for Partner: {partnerIdentifier}");
                            }
                        }
                        
                        partnerModified = true;
                    }

                    if (partnerModified)
                    {
                        partnersProcessed++;
                    }
                }

                // Save all changes at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"\nPartner FocalPoint and OrgUnit updates completed successfully.");
                Console.WriteLine($"Partners processed: {partnersProcessed}");
                Console.WriteLine($"FocalPoint updates: {focalPointUpdates}");
                Console.WriteLine($"OrgUnit updates/creations: {orgUnitUpdates}");
                Console.WriteLine($"OrgUnit deletions: {orgUnitDeletions}");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating Partner FocalPoints and OrgUnits: {ex.Message}");
                throw;
            }
        }

        private class PartnerUpdateData
        {
            public int? ErpDimValue { get; set; }
            public string? Name { get; set; }
            public string? LegacyFocalPointUser { get; set; }
            public string? SuggestedFocalPoint { get; set; }
            public string? LegacyOrgUnit { get; set; }
            public string? SuggestedOrgUnit { get; set; }
        }
    }
}
