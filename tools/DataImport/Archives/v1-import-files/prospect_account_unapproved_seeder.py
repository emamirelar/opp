import pandas as pd
import os
import re
from datetime import datetime

def sanitize_string(value):
    """Sanitize string values for C# code generation."""
    if pd.isna(value) or value == '' or str(value).strip() == '':
        return 'null'
    
    # Convert to string and escape special characters
    value = str(value).strip()
    value = value.replace('\\', '\\\\')
    value = value.replace('"', '\\"')
    value = value.replace('\n', '\\n')
    value = value.replace('\r', '\\r')
    value = value.replace('\t', '\\t')
    
    return f'"{value}"'

def get_logo_url_from_website(website):
    """Generate LogoUrl using Clearbit API from website URL."""
    if pd.isna(website) or not website or str(website).strip() == '':
        return 'null'
    
    website_str = str(website).strip()
    # Extract domain from URL
    domain_match = re.search(r'https?://(?:www\.)?([^/]+)', website_str)
    if domain_match:
        domain = domain_match.group(1)
        return f'"https://logo.clearbit.com/{domain}"'
    
    return 'null'

def map_boolean_value(value):
    """Map TRUE/FALSE string values to C# boolean."""
    if pd.isna(value) or value == '':
        return 'false'
    value_str = str(value).strip().upper()
    return 'true' if value_str == 'TRUE' else 'false'

def generate_prospect_account_seeder():
    """Generate the C# ProspectAccountsSeeder.cs file from CSV data."""
    
    # Define paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_path = os.path.join(script_dir, 'Prospect_Accounts_Unapproved_20251008 - Sheet1.csv')
    output_path = os.path.join(script_dir, "..", "..", "..", "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", 'ProspectAccountsSeeder.cs')
    
    # Read the CSV file
    print(f"Reading CSV from: {csv_path}")
    df = pd.read_csv(csv_path, encoding='utf-8')
    
    print(f"Total prospect accounts to process: {len(df)}")
    
    # Start building the C# file content
    cs_content = []
    cs_content.append("using Microsoft.EntityFrameworkCore;")
    cs_content.append("using System;")
    cs_content.append("using System.Collections.Generic;")
    cs_content.append("using System.Linq;")
    cs_content.append("using System.Threading.Tasks;")
    cs_content.append("using UNOPS.PAO.Domain.Entities;")
    cs_content.append("using UNOPS.PAO.Domain.Enums;")
    cs_content.append("using UNOPS.PAO.UNOPSDataAccess.Context;")
    cs_content.append("using UNOPS.PAO.UNOPSDomain.Entities;")
    cs_content.append("")
    cs_content.append("namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders")
    cs_content.append("{")
    cs_content.append("    public static class ProspectAccountsSeeder")
    cs_content.append("    {")
    cs_content.append("        public static async Task SeedProspectAccountsAsync(UNOPSAppDbContext context)")
    cs_content.append("        {")
    cs_content.append("            // Create mapping from PAOUser Email to Id (handle duplicates by taking first, filter out null emails)")
    cs_content.append("            var paoUsers = await context.PAOUsers")
    cs_content.append("                .Select(u => new { u.Id, u.Email })")
    cs_content.append("                .ToListAsync();")
    cs_content.append("            var paoUserEmailMapping = paoUsers")
    cs_content.append("                .Where(u => !string.IsNullOrEmpty(u.Email))")
    cs_content.append("                .GroupBy(u => u.Email.ToLower())")
    cs_content.append("                .ToDictionary(g => g.Key, g => g.First().Id);")
    cs_content.append("")
    cs_content.append("            // Create mapping from OrganizationHierarchy Name to Id (handle duplicates by taking first, filter out null names)")
    cs_content.append("            var orgUnits = await context.OrganizationHierarchies")
    cs_content.append("                .Where(oh => oh.Type == Domain.Enums.OrganizationUnitType.OrgUnit)")
    cs_content.append("                .ToListAsync();")
    cs_content.append("            var orgUnitMapping = orgUnits")
    cs_content.append("                .Where(oh => !string.IsNullOrEmpty(oh.Name))")
    cs_content.append("                .GroupBy(oh => oh.Name)")
    cs_content.append("                .ToDictionary(g => g.Key, g => g.First().Id);")
    cs_content.append("")
    cs_content.append("            // Process prospect accounts")
    cs_content.append("            var partnersToProcess = new List<(string Name, UNOPSPartner Partner, string? OrgUnit)>")
    cs_content.append("            {")
    
    # Process each row
    partners_data = []
    for idx, row in df.iterrows():
        name = sanitize_string(row['Name'])
        short_name = sanitize_string(row['SF_PRM_ShortName__c'])
        website = sanitize_string(row['Website'])
        phone = sanitize_string(row['Phone'])
        billing_street = sanitize_string(row['BillingStreet'])
        billing_city = sanitize_string(row['BillingCity'])
        billing_state = sanitize_string(row['BillingState'])
        billing_postal_code = sanitize_string(row['BillingPostalCode'])
        billing_country = sanitize_string(row['BillingCountry'])
        owner_email = sanitize_string(row['Owner.Email'])
        owner_department = sanitize_string(row['Owner.Department'])
        un_secretariat = map_boolean_value(row['SF_PRM_UNSecretariatEntity__c'])
        pooled_fund = map_boolean_value(row['SF_PRM_PooledFund__c'])
        key_global_partner = map_boolean_value(row['SF_PRM_GlobalKeyAccountPartner__c'])
        can_create_opportunities = map_boolean_value(row['SF_PRM_NewEngagement__c'])
        
        # Get logo URL from website
        logo_url = get_logo_url_from_website(row['Website'])
        
        # Build address if any address fields are present
        address_parts = []
        if billing_street != 'null':
            address_parts.append(billing_street.strip('"'))
        if billing_city != 'null':
            address_parts.append(billing_city.strip('"'))
        if billing_state != 'null':
            address_parts.append(billing_state.strip('"'))
        if billing_postal_code != 'null':
            address_parts.append(billing_postal_code.strip('"'))
        if billing_country != 'null':
            address_parts.append(billing_country.strip('"'))
        
        long_description_value = ', '.join(address_parts) if address_parts else ''
        long_description = sanitize_string(long_description_value) if long_description_value else 'null'
        
        partners_data.append({
            'name': name,
            'name_raw': row['Name'] if not pd.isna(row['Name']) else '',
            'short_name': short_name,
            'long_description': long_description,
            'logo_url': logo_url,
            'website': website,
            'phone': phone,
            'owner_email': owner_email,
            'owner_department': owner_department,
            'un_secretariat': un_secretariat,
            'pooled_fund': pooled_fund,
            'key_global_partner': key_global_partner,
            'can_create_opportunities': can_create_opportunities
        })
    
    # Generate partner objects
    for partner in partners_data:
        cs_content.append(f"                new ({partner['name']}, new UNOPSPartner")
        cs_content.append("                {")
        cs_content.append(f"                    Name = {partner['name']},")
        cs_content.append(f"                    PartnerShortDescription = {partner['short_name']},")
        cs_content.append(f"                    PartnerLongDescription = {partner['long_description']},")
        cs_content.append(f"                    LogoUrl = {partner['logo_url']},")
        cs_content.append("                    ErpDimValue = null,")
        cs_content.append("                    Status = (EntityStatus)1,")
        cs_content.append(f"                    UNSecretariatPartner = {partner['un_secretariat']},")
        cs_content.append(f"                    PooledFund = {partner['pooled_fund']},")
        cs_content.append(f"                    KeyGlobalPartner = {partner['key_global_partner']},")
        cs_content.append(f"                    CanCreateNewOpportunities = {partner['can_create_opportunities']},")
        cs_content.append("                    PartnerApprovalStatus = (PartnerApprovalStatus)0,")
        cs_content.append("                    CreatedBy = 0,")
        cs_content.append("                    CreatedDate = DateTime.UtcNow,")
        cs_content.append("                    LastModifiedBy = 0,")
        cs_content.append("                    LastModifiedDate = DateTime.UtcNow,")
        cs_content.append("                    IsDeleted = false,")
        cs_content.append("                    DeletedBy = 0,")
        cs_content.append("                    DueDiligenceRequired = null,")
        cs_content.append("                    DueDiligenceApproval = null,")
        cs_content.append("                    DueDiligenceApprovalDate = null,")
        cs_content.append("                    DueDiligenceExpiryDate = null,")
        cs_content.append("                    PartnerApprovalDate = null,")
        cs_content.append("                    PartnerApprovedBy = null,")
        
        # Handle PartnerFocalPointUserId lookup by email
        if partner['owner_email'] != 'null':
            cs_content.append(f"                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey({partner['owner_email']}.ToLower()) ? paoUserEmailMapping[{partner['owner_email']}.ToLower()] : (int?)null,")
        else:
            cs_content.append("                    PartnerFocalPointUserId = null,")
        
        # Add placeholder nulls for optional fields
        cs_content.append("                    PartnerCategoryId = null,")
        cs_content.append("                    PartnerGroupId = null,")
        cs_content.append("                    LiaisonOfficeId = null,")
        cs_content.append("                    PartnerLevyStatus = null,")
        cs_content.append("                    LevyTreatment = null,")
        cs_content.append("                    ReasonForLevy = null,")
        cs_content.append("                    ReasonForNoNewOpportunity = null,")
        cs_content.append("                    PartnerApprovalReference = null")
        
        cs_content.append(f"                }}, {partner['owner_department']}),")
    
    # Remove last comma
    if cs_content[-1].endswith(','):
        cs_content[-1] = cs_content[-1][:-1]
    
    cs_content.append("            };")
    cs_content.append("")
    cs_content.append("            // Begin transaction to ensure atomicity")
    cs_content.append("            await using var transaction = await context.Database.BeginTransactionAsync();")
    cs_content.append("")
    cs_content.append("            try")
    cs_content.append("            {")
    cs_content.append("                // Step 1: Process all partners (create or update)")
    cs_content.append("                foreach (var (partnerName, partnerData, _) in partnersToProcess)")
    cs_content.append("                {")
    cs_content.append("                    // Check if partner already exists based on Name where ErpDimValue is null")
    cs_content.append("                    var existingPartner = await context.Partners")
    cs_content.append("                        .FirstOrDefaultAsync(p => p.Name == partnerName && p.ErpDimValue == null);")
    cs_content.append("")
    cs_content.append("                    if (existingPartner != null)")
    cs_content.append("                    {")
    cs_content.append("                        // Update existing partner")
    cs_content.append("                        existingPartner.Name = partnerData.Name;")
    cs_content.append("                        existingPartner.PartnerShortDescription = partnerData.PartnerShortDescription;")
    cs_content.append("                        existingPartner.PartnerLongDescription = partnerData.PartnerLongDescription;")
    cs_content.append("                        existingPartner.LogoUrl = partnerData.LogoUrl;")
    cs_content.append("                        existingPartner.Status = partnerData.Status;")
    cs_content.append("                        existingPartner.UNSecretariatPartner = partnerData.UNSecretariatPartner;")
    cs_content.append("                        existingPartner.PooledFund = partnerData.PooledFund;")
    cs_content.append("                        existingPartner.KeyGlobalPartner = partnerData.KeyGlobalPartner;")
    cs_content.append("                        existingPartner.CanCreateNewOpportunities = partnerData.CanCreateNewOpportunities;")
    cs_content.append("                        existingPartner.PartnerApprovalStatus = partnerData.PartnerApprovalStatus;")
    cs_content.append("                        existingPartner.PartnerFocalPointUserId = partnerData.PartnerFocalPointUserId;")
    cs_content.append("                        existingPartner.DueDiligenceRequired = partnerData.DueDiligenceRequired;")
    cs_content.append("                        existingPartner.DueDiligenceApproval = partnerData.DueDiligenceApproval;")
    cs_content.append("                        existingPartner.DueDiligenceApprovalDate = partnerData.DueDiligenceApprovalDate;")
    cs_content.append("                        existingPartner.DueDiligenceExpiryDate = partnerData.DueDiligenceExpiryDate;")
    cs_content.append("                        existingPartner.PartnerApprovalDate = partnerData.PartnerApprovalDate;")
    cs_content.append("                        existingPartner.PartnerApprovedBy = partnerData.PartnerApprovedBy;")
    cs_content.append("                        existingPartner.PartnerCategoryId = partnerData.PartnerCategoryId;")
    cs_content.append("                        existingPartner.PartnerGroupId = partnerData.PartnerGroupId;")
    cs_content.append("                        existingPartner.LiaisonOfficeId = partnerData.LiaisonOfficeId;")
    cs_content.append("                        existingPartner.PartnerLevyStatus = partnerData.PartnerLevyStatus;")
    cs_content.append("                        existingPartner.LevyTreatment = partnerData.LevyTreatment;")
    cs_content.append("                        existingPartner.ReasonForLevy = partnerData.ReasonForLevy;")
    cs_content.append("                        existingPartner.ReasonForNoNewOpportunity = partnerData.ReasonForNoNewOpportunity;")
    cs_content.append("                        existingPartner.PartnerApprovalReference = partnerData.PartnerApprovalReference;")
    cs_content.append("                        existingPartner.LastModifiedBy = 0;")
    cs_content.append("                        existingPartner.LastModifiedDate = DateTime.UtcNow;")
    cs_content.append("                    }")
    cs_content.append("                    else")
    cs_content.append("                    {")
    cs_content.append("                        // Add new partner to context")
    cs_content.append("                        context.Partners.Add(partnerData);")
    cs_content.append("                    }")
    cs_content.append("                }")
    cs_content.append("")
    cs_content.append("                // Save all partners at once")
    cs_content.append("                await context.SaveChangesAsync();")
    cs_content.append("")
    cs_content.append("                // Step 2: Process all organization unit relationships in batch")
    cs_content.append("                foreach (var (partnerName, _, orgUnit) in partnersToProcess)")
    cs_content.append("                {")
    cs_content.append("                    // Skip if no org unit specified")
    cs_content.append("                    if (string.IsNullOrWhiteSpace(orgUnit) || !orgUnitMapping.ContainsKey(orgUnit))")
    cs_content.append("                        continue;")
    cs_content.append("")
    cs_content.append("                    // Get the partner (now guaranteed to exist with an ID)")
    cs_content.append("                    var partner = await context.Partners")
    cs_content.append("                        .FirstOrDefaultAsync(p => p.Name == partnerName && p.ErpDimValue == null);")
    cs_content.append("")
    cs_content.append("                    if (partner == null)")
    cs_content.append("                        continue;")
    cs_content.append("")
    cs_content.append("                    var orgHierarchyId = orgUnitMapping[orgUnit];")
    cs_content.append("")
    cs_content.append("                    // Check if relationship already exists")
    cs_content.append("                    var existingRelationship = await context.OrganizationUnitRelationships")
    cs_content.append("                        .FirstOrDefaultAsync(r => r.EntityType == nameof(Partner) && ")
    cs_content.append("                                                  r.EntityId == partner.Id && ")
    cs_content.append("                                                  r.OrganizationHierarchyId == orgHierarchyId);")
    cs_content.append("")
    cs_content.append("                    if (existingRelationship == null)")
    cs_content.append("                    {")
    cs_content.append("                        // Create new relationship")
    cs_content.append("                        var newRelationship = new OrganizationUnitRelationship")
    cs_content.append("                        {")
    cs_content.append("                            OrganizationHierarchyId = orgHierarchyId,")
    cs_content.append("                            EntityId = partner.Id,")
    cs_content.append("                            EntityType = nameof(Partner),")
    cs_content.append("                            Name = $\"Partner-{partner.Id}-{orgHierarchyId}\",")
    cs_content.append("                            Status = EntityStatus.Active,")
    cs_content.append("                            CreatedBy = 0,")
    cs_content.append("                            CreatedDate = DateTime.UtcNow,")
    cs_content.append("                            LastModifiedBy = 0,")
    cs_content.append("                            LastModifiedDate = DateTime.UtcNow,")
    cs_content.append("                            IsDeleted = false")
    cs_content.append("                        };")
    cs_content.append("                        context.OrganizationUnitRelationships.Add(newRelationship);")
    cs_content.append("                    }")
    cs_content.append("                }")
    cs_content.append("")
    cs_content.append("                // Save all organization unit relationships at once")
    cs_content.append("                await context.SaveChangesAsync();")
    cs_content.append("")
    cs_content.append("                // Commit transaction if everything succeeded")
    cs_content.append("                await transaction.CommitAsync();")
    cs_content.append("")
    cs_content.append("                Console.WriteLine($\"Successfully seeded prospect accounts\");")
    cs_content.append("            }")
    cs_content.append("            catch (Exception ex)")
    cs_content.append("            {")
    cs_content.append("                // Rollback transaction if any error occurred")
    cs_content.append("                await transaction.RollbackAsync();")
    cs_content.append("                Console.WriteLine($\"Error seeding prospect accounts: {ex.Message}\");")
    cs_content.append("                throw;")
    cs_content.append("            }")
    cs_content.append("        }")
    cs_content.append("    }")
    cs_content.append("}")
    
    # Write to file
    print(f"Writing output to: {output_path}")
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(cs_content))
    
    print(f"Successfully generated ProspectAccountsSeeder.cs with {len(partners_data)} prospect accounts")
    print(f"Output saved to: {output_path}")

if __name__ == "__main__":
    generate_prospect_account_seeder()

