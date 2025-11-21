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

def get_logo_url(partner_name, official_websites_dict):
    """Generate LogoUrl using Clearbit API or from official websites."""
    if partner_name in official_websites_dict:
        website = official_websites_dict[partner_name]
        # Check if website is valid (not NaN and not empty)
        if website and not pd.isna(website) and str(website).strip():
            website_str = str(website).strip()
            # Extract domain from URL
            domain_match = re.search(r'https?://(?:www\.)?([^/]+)', website_str)
            if domain_match:
                domain = domain_match.group(1)
                return f'"https://logo.clearbit.com/{domain}"'
    
    return 'null'

def map_boolean_value(value):
    """Map YES/NO string values to C# boolean."""
    if pd.isna(value) or value == '':
        return 'false'
    value_str = str(value).strip().upper()
    return 'true' if value_str == 'YES' else 'false'

def map_levy_status(value):
    """Map Partner_Levy_Potentially_Applied values to PartnerLevyStatus enum."""
    if pd.isna(value) or value == '':
        return 'null'
    
    value_str = str(value).strip().upper()
    if value_str == 'POTENT_APPLY':
        return '(PartnerLevyStatus)1'  # PotentiallyApplied
    elif value_str == 'POTENT_NOT_APPLY':
        return '(PartnerLevyStatus)2'  # PotentiallyNotApplied
    elif value_str == 'DOES_NOT_APPLY':
        return '(PartnerLevyStatus)0'  # DoesNotApply
    else:
        return 'null'

def map_approval_status(value):
    """Map Approval status values to PartnerApprovalStatus enum."""
    if pd.isna(value) or value == '':
        return '(PartnerApprovalStatus)0'  # NotApproved
    
    value_str = str(value).strip().upper()
    if value_str == 'APPROVED':
        return '(PartnerApprovalStatus)1'  # Approved
    else:
        return '(PartnerApprovalStatus)0'  # NotApproved

def generate_partner_seeder():
    """Generate the C# PartnerSeeder_v2.cs file from CSV data."""
    
    # Define paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_path = os.path.join(script_dir, 'Partner tree export 02-Oct-2025 - Sheet1.csv')
    websites_path = os.path.join(script_dir, "..", '2025-08-27', 'official_websites.csv')
    output_path = os.path.join(script_dir, "..", "..", "..", "UNOPS.PAO.UNOPSDataAccess", "Seed", "Seeders", 'PartnerSeeder_v2.cs')
    
    # Read the CSV file
    print(f"Reading CSV from: {csv_path}")
    df = pd.read_csv(csv_path, encoding='utf-8')
    
    # Read official websites
    print(f"Reading official websites from: {websites_path}")
    try:
        websites_df = pd.read_csv(websites_path, encoding='utf-8')
        official_websites = dict(zip(websites_df['Organization'], websites_df['Official Website']))
    except Exception as e:
        print(f"Warning: Could not load official websites: {e}")
        official_websites = {}
    
    print(f"Total partners to process: {len(df)}")
    
    # Start building the C# file content
    cs_content = []
    cs_content.append("using Microsoft.EntityFrameworkCore;")
    cs_content.append("using System.Collections.Generic;")
    cs_content.append("using System.Linq;")
    cs_content.append("using UNOPS.PAO.Domain.Entities;")
    cs_content.append("using UNOPS.PAO.Domain.Enums;")
    cs_content.append("using UNOPS.PAO.UNOPSDataAccess.Context;")
    cs_content.append("using UNOPS.PAO.UNOPSDomain.Entities;")
    cs_content.append("")
    cs_content.append("namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders")
    cs_content.append("{")
    cs_content.append("    public static class PartnerSeeder_v2")
    cs_content.append("    {")
    cs_content.append("        public static async Task SeedPartnersV2Async(UNOPSAppDbContext context)")
    cs_content.append("        {")
    cs_content.append("            // Create mapping from LiaisonOffice Name to Id (handle duplicates by taking first, filter out null names)")
    cs_content.append("            var liaisonOffices = await context.LiaisonOffices.ToListAsync();")
    cs_content.append("            var liaisonOfficeMapping = liaisonOffices")
    cs_content.append("                .Where(lo => !string.IsNullOrEmpty(lo.Name))")
    cs_content.append("                .GroupBy(lo => lo.Name)")
    cs_content.append("                .ToDictionary(g => g.Key, g => g.First().Id);")
    cs_content.append("")
    cs_content.append("            // Create mapping from PartnerTree Description to Id for Partner Categories (handle duplicates by taking first)")
    cs_content.append("            var partnerCategories = await context.PartnerTrees")
    cs_content.append("                .Where(pt => pt.PartnerCategoryCode != null)")
    cs_content.append("                .ToListAsync();")
    cs_content.append("            var partnerCategoryMapping = partnerCategories")
    cs_content.append("                .GroupBy(pt => pt.Description)")
    cs_content.append("                .ToDictionary(g => g.Key, g => g.First().Id);")
    cs_content.append("")
    cs_content.append("            // Create mapping from PartnerTree Description to Id for Partner Groups (handle duplicates by taking first)")
    cs_content.append("            var partnerGroups = await context.PartnerTrees")
    cs_content.append("                .Where(pt => pt.PartnerGroupCode != null)")
    cs_content.append("                .ToListAsync();")
    cs_content.append("            var partnerGroupMapping = partnerGroups")
    cs_content.append("                .GroupBy(pt => pt.Description)")
    cs_content.append("                .ToDictionary(g => g.Key, g => g.First().Id);")
    cs_content.append("")
    cs_content.append("            // Create mapping from PAOUser Name to Id (handle duplicates by taking first, filter out null names)")
    cs_content.append("            var paoUsers = await context.PAOUsers")
    cs_content.append("                .Select(u => new { u.Id, u.Name })")
    cs_content.append("                .ToListAsync();")
    cs_content.append("            var paoUserMapping = paoUsers")
    cs_content.append("                .Where(u => !string.IsNullOrEmpty(u.Name))")
    cs_content.append("                .GroupBy(u => u.Name)")
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
    cs_content.append("            // Process partners")
    cs_content.append("            var partnersToProcess = new List<(int ErpDimValue, UNOPSPartner Partner, string? OrgUnit)>");
    cs_content.append("            {")
    
    # Process each row
    partners_data = []
    for idx, row in df.iterrows():
        erp_dim_value = row['Partner']
        name = sanitize_string(row['Partner_Description'])
        short_desc = sanitize_string(row['Partner_Description_Short'])
        partner_category = sanitize_string(row['Partner_Category'])
        partner_group = sanitize_string(row['Partner Group'])
        liaison_office = sanitize_string(row['Liaison Office'])
        partner_focal_point = sanitize_string(row['Partner Focal point'])
        partner_org_unit = sanitize_string(row['Partner Org Unit'])
        un_secretariat = map_boolean_value(row['UN Secretariat'])
        pooled_fund = map_boolean_value(row['Partner_Pool_Fund_Flag'])
        levy_status = map_levy_status(row['Partner_Levy_Potentially_Applied'])
        levy_treatment = sanitize_string(row['Partner_Levy_Treatment'])
        can_create_opportunities = map_boolean_value(row['New engagement SF'])
        approval_status = map_approval_status(row['Approval status'])
        long_description = sanitize_string(row['Long description'])
        approval_reference = sanitize_string(row['Partner Approval Reference'])
        key_global_partner = map_boolean_value(row['Key Global partner'])
        
        # Get logo URL
        partner_name_raw = row['Partner_Description'] if not pd.isna(row['Partner_Description']) else ''
        logo_url = get_logo_url(partner_name_raw, official_websites)
        
        partners_data.append({
            'erp_dim_value': erp_dim_value,
            'name': name,
            'short_desc': short_desc,
            'partner_category': partner_category,
            'partner_group': partner_group,
            'liaison_office': liaison_office,
            'partner_focal_point': partner_focal_point,
            'partner_org_unit': partner_org_unit,
            'un_secretariat': un_secretariat,
            'pooled_fund': pooled_fund,
            'levy_status': levy_status,
            'levy_treatment': levy_treatment,
            'can_create_opportunities': can_create_opportunities,
            'approval_status': approval_status,
            'long_description': long_description,
            'approval_reference': approval_reference,
            'key_global_partner': key_global_partner,
            'logo_url': logo_url
        })
    
    # Generate partner objects
    for partner in partners_data:
        cs_content.append(f"                new ({partner['erp_dim_value']}, new UNOPSPartner")
        cs_content.append("                {")
        cs_content.append(f"                    Name = {partner['name']},")
        cs_content.append(f"                    PartnerShortDescription = {partner['short_desc']},")
        cs_content.append(f"                    PartnerLongDescription = {partner['long_description']},")
        cs_content.append(f"                    LogoUrl = {partner['logo_url']},")
        cs_content.append(f"                    ErpDimValue = {partner['erp_dim_value']},")
        cs_content.append("                    Status = (EntityStatus)1,")
        cs_content.append(f"                    UNSecretariatPartner = {partner['un_secretariat']},")
        cs_content.append(f"                    PooledFund = {partner['pooled_fund']},")
        cs_content.append(f"                    PartnerLevyStatus = {partner['levy_status']},")
        cs_content.append(f"                    LevyTreatment = {partner['levy_treatment']},")
        cs_content.append(f"                    CanCreateNewOpportunities = {partner['can_create_opportunities']},")
        cs_content.append(f"                    PartnerApprovalStatus = {partner['approval_status']},")
        cs_content.append(f"                    PartnerApprovalReference = {partner['approval_reference']},")
        cs_content.append(f"                    KeyGlobalPartner = {partner['key_global_partner']},")
        # Only set PartnerApprovedBy to "Data Migration" if approval status is Approved
        if partner['approval_status'] == '(PartnerApprovalStatus)1':
            cs_content.append("                    PartnerApprovedBy = \"Data Migration\",")
        else:
            cs_content.append("                    PartnerApprovedBy = null,")
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
        
        # Handle lookups with conditional checks (only if not null)
        if partner['partner_category'] != 'null':
            cs_content.append(f"                    PartnerCategoryId = partnerCategoryMapping.ContainsKey({partner['partner_category']}) ? partnerCategoryMapping[{partner['partner_category']}] : (int?)null,")
        else:
            cs_content.append("                    PartnerCategoryId = null,")
        
        if partner['partner_group'] != 'null':
            cs_content.append(f"                    PartnerGroupId = partnerGroupMapping.ContainsKey({partner['partner_group']}) ? partnerGroupMapping[{partner['partner_group']}] : (int?)null,")
        else:
            cs_content.append("                    PartnerGroupId = null,")
        
        if partner['liaison_office'] != 'null':
            cs_content.append(f"                    LiaisonOfficeId = liaisonOfficeMapping.ContainsKey({partner['liaison_office']}) ? liaisonOfficeMapping[{partner['liaison_office']}] : (int?)null,")
        else:
            cs_content.append("                    LiaisonOfficeId = null,")
        
        if partner['partner_focal_point'] != 'null':
            cs_content.append(f"                    PartnerFocalPointUserId = paoUserMapping.ContainsKey({partner['partner_focal_point']}) ? paoUserMapping[{partner['partner_focal_point']}] : (int?)null")
        else:
            cs_content.append("                    PartnerFocalPointUserId = null")
        
        cs_content.append(f"                }}, {partner['partner_org_unit']}),")
    
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
    cs_content.append("                // Step 1: Process all partners (create or update) in batch")
    cs_content.append("                foreach (var (erpDimValue, partnerData, _) in partnersToProcess)")
    cs_content.append("                {")
    cs_content.append("                    // Check if partner already exists based on ErpDimValue")
    cs_content.append("                    var existingPartner = await context.Partners")
    cs_content.append("                        .FirstOrDefaultAsync(p => p.ErpDimValue == erpDimValue);")
    cs_content.append("")
    cs_content.append("                    if (existingPartner != null)")
    cs_content.append("                    {")
    cs_content.append("                        // Update existing partner")
    cs_content.append("                        existingPartner.Name = partnerData.Name;")
    cs_content.append("                        existingPartner.PartnerShortDescription = partnerData.PartnerShortDescription;")
    cs_content.append("                        existingPartner.PartnerLongDescription = partnerData.PartnerLongDescription;")
    cs_content.append("                        existingPartner.LogoUrl = partnerData.LogoUrl;")
    cs_content.append("                        existingPartner.Status = partnerData.Status;")
    cs_content.append("                        existingPartner.PartnerCategoryId = partnerData.PartnerCategoryId;")
    cs_content.append("                        existingPartner.PartnerGroupId = partnerData.PartnerGroupId;")
    cs_content.append("                        existingPartner.LiaisonOfficeId = partnerData.LiaisonOfficeId;")
    cs_content.append("                        existingPartner.PartnerFocalPointUserId = partnerData.PartnerFocalPointUserId;")
    cs_content.append("                        existingPartner.UNSecretariatPartner = partnerData.UNSecretariatPartner;")
    cs_content.append("                        existingPartner.PooledFund = partnerData.PooledFund;")
    cs_content.append("                        existingPartner.PartnerLevyStatus = partnerData.PartnerLevyStatus;")
    cs_content.append("                        existingPartner.LevyTreatment = partnerData.LevyTreatment;")
    cs_content.append("                        existingPartner.CanCreateNewOpportunities = partnerData.CanCreateNewOpportunities;")
    cs_content.append("                        existingPartner.PartnerApprovalStatus = partnerData.PartnerApprovalStatus;")
    cs_content.append("                        existingPartner.PartnerApprovalReference = partnerData.PartnerApprovalReference;")
    cs_content.append("                        existingPartner.PartnerApprovedBy = partnerData.PartnerApprovedBy;")
    cs_content.append("                        existingPartner.KeyGlobalPartner = partnerData.KeyGlobalPartner;")
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
    cs_content.append("                foreach (var (erpDimValue, _, orgUnit) in partnersToProcess)")
    cs_content.append("                {")
    cs_content.append("                    // Skip if no org unit specified")
    cs_content.append("                    if (string.IsNullOrWhiteSpace(orgUnit) || !orgUnitMapping.ContainsKey(orgUnit))")
    cs_content.append("                        continue;")
    cs_content.append("")
    cs_content.append("                    // Get the partner (now guaranteed to exist with an ID)")
    cs_content.append("                    var partner = await context.Partners")
    cs_content.append("                        .FirstOrDefaultAsync(p => p.ErpDimValue == erpDimValue);")
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
    cs_content.append("            }")
    cs_content.append("            catch")
    cs_content.append("            {")
    cs_content.append("                // Rollback transaction if any error occurred")
    cs_content.append("                await transaction.RollbackAsync();")
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
    
    print(f"Successfully generated PartnerSeeder_v2.cs with {len(partners_data)} partners")
    print(f"Output saved to: {output_path}")

if __name__ == "__main__":
    generate_partner_seeder()

