import csv
import html
import re
from collections import defaultdict

def clean_string_for_csharp(text):
    """Clean and escape string for C# code"""
    if not text:
        return ""
    # Remove HTML tags if any
    text = re.sub('<[^<]+?>', '', text)
    # Escape quotes for C#
    text = text.replace('"', '\\"')
    # Remove extra whitespace
    text = text.strip()
    return text

def convert_status_to_entitystatus(status):
    """Convert CSV status to EntityStatus enum value"""
    if not status:
        return "1"  # Default to Active
    status_lower = status.lower()
    if status_lower == "active":
        return "1"  # EntityStatus.Active
    elif status_lower == "inactive":
        return "0"  # EntityStatus.Inactive  
    else:
        return "1"  # Default to Active

def convert_boolean_field(value):
    """Convert various boolean representations to C# bool"""
    if not value:
        return "false"
    value_lower = str(value).lower()
    if value_lower in ["true", "yes", "1", "allowed"]:
        return "true"
    elif value_lower in ["false", "no", "0", "not allowed"]:
        return "false"
    else:
        return "false"  # Default

def convert_sf_status_to_entitystatus(status):
    """Convert Salesforce SF_PRM_Status__c to EntityStatus enum value"""
    if not status:
        return "1"  # Default to Active
    status_lower = status.lower()
    if status_lower == "active":
        return "1"  # EntityStatus.Active
    elif status_lower == "inactive":
        return "0"  # EntityStatus.Inactive  
    else:
        return "1"  # Default to Active

def convert_sf_dd_required(value):
    """Convert Salesforce SF_PRM_DDRequired__c to DueDiligenceRequired enum value"""
    if not value:
        return "0"  # Default to NotRequired
    value_lower = str(value).lower()
    if value_lower == "yes":
        return "1"  # DueDiligenceRequired.Required
    elif value_lower == "no":
        return "0"  # DueDiligenceRequired.NotRequired
    else:
        return "0"  # Default to NotRequired

def convert_sf_dd_approval(value):
    """Convert Salesforce SF_PRM_DDEACDone__c to DueDiligenceApproval enum value"""
    if not value:
        return "0"  # Default to NotApproved
    value_lower = str(value).lower()
    if value_lower in ["done", "yes", "true", "approved"]:
        return "1"  # DueDiligenceApproval.Approved
    elif value_lower in ["not done", "no", "false", "not approved"]:
        return "0"  # DueDiligenceApproval.NotApproved
    else:
        return "0"  # Default to NotApproved

def convert_sf_levy_status(value):
    """Convert Salesforce SF_PRM_LevyPotentiallyApplies__c to PartnerLevyStatus enum value"""
    if not value:
        return "0"  # Default to DoesNotApply
    value_lower = str(value).lower()
    if value_lower == "potentially applies":
        return "1"  # PartnerLevyStatus.PotentiallyApplies
    elif value_lower == "does not apply":
        return "0"  # PartnerLevyStatus.DoesNotApply
    elif value_lower == "potentially does not apply":
        return "2"  # PartnerLevyStatus.PotentiallyDoesNotApply
    else:
        return "0"  # Default to DoesNotApply

def load_liaison_office_codes():
    """Load valid liaison office codes from the liaison office CSV if it exists"""
    valid_liaison_office_codes = set()
    try:
        with open('sf_prod_liaison_office_export - Sheet1.csv', 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            for row in reader:
                liaison_code = row['Id'].strip() if 'Id' in row and row['Id'] else ""
                if liaison_code:
                    valid_liaison_office_codes.add(liaison_code)
        print(f"Loaded {len(valid_liaison_office_codes)} liaison office codes from CSV")
    except FileNotFoundError:
        print("Liaison office CSV not found - will use default null values for LiaisonOfficeId")
    
    return valid_liaison_office_codes

def load_partner_liaison_mapping():
    """Load mapping from AccountNumber to various partner fields from partners export CSV"""
    account_to_partner_data_mapping = {}
    try:
        with open('sf_prod_partners_export - Sheet1.csv', 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            for row in reader:
                account_number = row['AccountNumber'].strip() if 'AccountNumber' in row and row['AccountNumber'] else ""
                
                if account_number:
                    # Extract all relevant SF fields
                    partner_data = {
                        'liaison_office_code': row['SF_PRM_LiaisonOffice__c'].strip() if 'SF_PRM_LiaisonOffice__c' in row and row['SF_PRM_LiaisonOffice__c'] else "",
                        'sf_status': row['SF_PRM_Status__c'].strip() if 'SF_PRM_Status__c' in row and row['SF_PRM_Status__c'] else "",
                        'sf_new_engagement': row['SF_PRM_NewEngagement__c'].strip() if 'SF_PRM_NewEngagement__c' in row and row['SF_PRM_NewEngagement__c'] else "",
                        'sf_reason_no_engagement': clean_string_for_csharp(row['SF_ReasonForNoNewEngagement__c']) if 'SF_ReasonForNoNewEngagement__c' in row and row['SF_ReasonForNoNewEngagement__c'] else "",
                        'sf_pooled_fund': row['SF_PRM_PooledFund__c'].strip() if 'SF_PRM_PooledFund__c' in row and row['SF_PRM_PooledFund__c'] else "",
                        'sf_global_key_partner': row['SF_PRM_GlobalKeyAccountPartner__c'].strip() if 'SF_PRM_GlobalKeyAccountPartner__c' in row and row['SF_PRM_GlobalKeyAccountPartner__c'] else "",
                        'sf_un_secretariat': row['SF_PRM_UNSecretariatEntity__c'].strip() if 'SF_PRM_UNSecretariatEntity__c' in row and row['SF_PRM_UNSecretariatEntity__c'] else "",
                        'sf_dd_required': row['SF_PRM_DDRequired__c'].strip() if 'SF_PRM_DDRequired__c' in row and row['SF_PRM_DDRequired__c'] else "",
                        'sf_dd_approval': row['SF_PRM_DDEACDone__c'].strip() if 'SF_PRM_DDEACDone__c' in row and row['SF_PRM_DDEACDone__c'] else "",
                        'sf_levy_status': row['SF_PRM_LevyPotentiallyApplies__c'].strip() if 'SF_PRM_LevyPotentiallyApplies__c' in row and row['SF_PRM_LevyPotentiallyApplies__c'] else "",
                        'sf_reason_levy': clean_string_for_csharp(row['SF_PRM_ReasonForLevyNotApplying__c']) if 'SF_PRM_ReasonForLevyNotApplying__c' in row and row['SF_PRM_ReasonForLevyNotApplying__c'] else "",
                        'sf_levy_treatment': clean_string_for_csharp(row['SF_PRM_LevyTreatment__c']) if 'SF_PRM_LevyTreatment__c' in row and row['SF_PRM_LevyTreatment__c'] else "",
                        'sf_eac_reference': clean_string_for_csharp(row['SF_PRM_EACReference__c']) if 'SF_PRM_EACReference__c' in row and row['SF_PRM_EACReference__c'] else ""
                    }
                    account_to_partner_data_mapping[account_number] = partner_data
        
        print(f"Loaded {len(account_to_partner_data_mapping)} AccountNumber -> Partner SF data mappings from partners CSV")
    except FileNotFoundError:
        print("Partners export CSV not found - will use default values for all SF fields")
    
    return account_to_partner_data_mapping

def load_partner_logo_and_flag_mapping():
    """Load mapping from partner_id to logo_url (with flag_url fallback) from the partner logos CSV"""
    partner_to_logo_mapping = {}
    logo_count = 0
    flag_fallback_count = 0
    
    try:
        with open('partner_logos_and_flags_results.csv', 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            for row in reader:
                partner_id = row['partner_id'].strip() if 'partner_id' in row and row['partner_id'] else ""
                logo_url = row['logo_url'].strip() if 'logo_url' in row and row['logo_url'] else ""
                flag_url = row['flag_url'].strip() if 'flag_url' in row and row['flag_url'] else ""
                
                if partner_id:
                    # Prioritize logo_url, fallback to flag_url
                    if logo_url:
                        partner_to_logo_mapping[partner_id] = logo_url
                        logo_count += 1
                    elif flag_url:
                        partner_to_logo_mapping[partner_id] = flag_url
                        flag_fallback_count += 1
        
        print(f"Loaded {len(partner_to_logo_mapping)} partner -> image URL mappings from logos CSV")
        print(f"  - {logo_count} with logo URLs")
        print(f"  - {flag_fallback_count} with flag URLs as fallback")
    except FileNotFoundError:
        print("Partner logos CSV not found - will use default null values for LogoUrl")
    
    return partner_to_logo_mapping

def load_account_owner_userid_mapping():
    """Load mapping from AccountNumber to UserID via SF_PRM_AccountOwner__c from both CSV files"""
    account_to_userid_mapping = {}
    
    # First, load the account owner name mapping: AccountNumber -> SF_PRM_AccountOwner__c
    account_to_owner_mapping = {}
    try:
        with open('sf_prod_account_owner_account_number_export.csv', 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            for row in reader:
                account_number = row['AccountNumber'].strip() if 'AccountNumber' in row and row['AccountNumber'] else ""
                account_owner = row['SF_PRM_AccountOwner__c'].strip() if 'SF_PRM_AccountOwner__c' in row and row['SF_PRM_AccountOwner__c'] else ""
                
                if account_number and account_owner:
                    account_to_owner_mapping[account_number] = account_owner
        
        print(f"Loaded {len(account_to_owner_mapping)} AccountNumber -> AccountOwner mappings")
    except FileNotFoundError:
        print("Account owner account number CSV not found - will use null values for PartnerFocalPointUserId")
        return account_to_userid_mapping
    
    # Second, load the owner to UserID mapping: SF_PRM_AccountOwner__c -> UserId
    owner_to_userid_mapping = {}
    try:
        with open('sf_prod_account_owner_userid_export.csv', 'r', encoding='utf-8') as file:
            reader = csv.DictReader(file)
            for row in reader:
                account_owner = row['SF_PRM_AccountOwner__c'].strip() if 'SF_PRM_AccountOwner__c' in row and row['SF_PRM_AccountOwner__c'] else ""
                user_id = row['UserId'].strip() if 'UserId' in row and row['UserId'] else ""
                
                if account_owner and user_id:
                    try:
                        # Ensure UserID is a valid integer
                        user_id_int = int(user_id)
                        owner_to_userid_mapping[account_owner] = user_id_int
                    except ValueError:
                        print(f"Invalid UserID format: {user_id} for owner: {account_owner}")
        
        print(f"Loaded {len(owner_to_userid_mapping)} AccountOwner -> UserId mappings")
    except FileNotFoundError:
        print("Account owner userid CSV not found - will use null values for PartnerFocalPointUserId")
        return account_to_userid_mapping
    
    # Combine the mappings: AccountNumber -> SF_PRM_AccountOwner__c -> UserId
    for account_number, account_owner in account_to_owner_mapping.items():
        if account_owner in owner_to_userid_mapping:
            account_to_userid_mapping[account_number] = owner_to_userid_mapping[account_owner]
    
    print(f"Created {len(account_to_userid_mapping)} AccountNumber -> UserId mappings")
    return account_to_userid_mapping

def is_circular_reference(level_code, partner_id):
    """Check if a level code creates a circular reference with the partner ID"""
    if not level_code or not partner_id:
        return False
    
    try:
        # Check if both can be converted to integers and are equal
        return int(level_code) == int(partner_id)
    except ValueError:
        # If level_code is not a number, it's not a circular reference
        return False

def parse_combined_hierarchy_csv():
    """Parse the Combined Hierarchy CSV and extract both partners and tree structure"""
    partners = []
    partner_tree_entries = set()  # Use set to avoid duplicates
    level_hierarchy = defaultdict(set)  # Track parent-child relationships
    circular_references_count = 0
    
    # Load valid liaison office codes and partner mapping
    valid_liaison_office_codes = load_liaison_office_codes()
    account_to_partner_data_mapping = load_partner_liaison_mapping()
    partner_to_logo_mapping = load_partner_logo_and_flag_mapping()
    account_to_userid_mapping = load_account_owner_userid_mapping()
    
    print("Reading Combined Hierarchy CSV...")
    
    with open('PartnerTreeExport - TEST Combined Hierarchy 28 Aug 2025.csv', 'r', encoding='utf-8') as file:
        reader = csv.DictReader(file)
        headers = reader.fieldnames
        print(f"CSV Headers: {headers}")
        
        for row in reader:
            # Extract partner data
            partner_id = row['Partner'].strip() if row['Partner'] else ""
            partner_name = clean_string_for_csharp(row['Partner_Description']) if row['Partner_Description'] else ""
            partner_short = clean_string_for_csharp(row['Partner_Description_Short']) if row['Partner_Description_Short'] else ""
            
            # Extract hierarchical levels
            level1_code = row['Partner_Level1'].strip() if row['Partner_Level1'] else ""
            level1_desc = clean_string_for_csharp(row['Partner_Level1_Description']) if row['Partner_Level1_Description'] else ""
            level1_short = clean_string_for_csharp(row['Partner_Level1_Description_Short']) if row['Partner_Level1_Description_Short'] else ""
            
            level2_code = row['Partner_Level2'].strip() if row['Partner_Level2'] else ""
            level2_desc = clean_string_for_csharp(row['Partner_Level2_Description']) if row['Partner_Level2_Description'] else ""
            level2_short = clean_string_for_csharp(row['Partner_Level2_Description_Short']) if row['Partner_Level2_Description_Short'] else ""
            
            # Check for additional levels (3, 4, 5)
            level3_code = row['Partner_Level3'].strip() if row['Partner_Level3'] else ""
            level3_desc = clean_string_for_csharp(row['Partner_Level3_Description']) if row['Partner_Level3_Description'] else ""
            
            level4_code = row['Partner_Level4'].strip() if row['Partner_Level4'] else ""
            level4_desc = clean_string_for_csharp(row['Partner_Level4_Description']) if row['Partner_Level4_Description'] else ""
            
            level5_code = row['Partner_Level5'].strip() if row['Partner_Level5'] else ""
            level5_desc = clean_string_for_csharp(row['Partner_Level5_Description']) if row['Partner_Level5_Description'] else ""
            
            reporting_category = clean_string_for_csharp(row['Partner_Reporting_Category']) if row['Partner_Reporting_Category'] else ""
            internal_code = row['Internal_Report_Level_Code'].strip() if row['Internal_Report_Level_Code'] else ""
            internal_desc = clean_string_for_csharp(row['Internal_Report_Level_Description']) if row['Internal_Report_Level_Description'] else ""
            
            # Add Level 1 entry (main categories like FOUNDATION, GOVERNMENT, etc.)
            if level1_code and level1_desc:
                partner_tree_entries.add((level1_code, level1_desc, level1_short or level1_desc, "Level_1", ""))
            
            # Determine the hierarchy and partner group code
            current_level = 1
            parent_code = level1_code
            
            # Add Level 2 if exists and is not a circular reference
            if level2_code and level2_desc and level2_code != level1_code and not is_circular_reference(level2_code, partner_id):
                partner_tree_entries.add((level2_code, level2_desc, level2_short or level2_desc, "Level_2", level1_code))
                parent_code = level2_code
                current_level = 2
            elif is_circular_reference(level2_code, partner_id):
                circular_references_count += 1
            
            # Add Level 3 if exists and is not a circular reference
            if level3_code and level3_desc and not is_circular_reference(level3_code, partner_id):
                partner_tree_entries.add((level3_code, level3_desc, level3_desc, "Level_3", parent_code))
                parent_code = level3_code
                current_level = 3
            elif is_circular_reference(level3_code, partner_id):
                circular_references_count += 1
            
            # Add Level 4 if exists and is not a circular reference
            if level4_code and level4_desc and not is_circular_reference(level4_code, partner_id):
                partner_tree_entries.add((level4_code, level4_desc, level4_desc, "Level_4", parent_code))
                parent_code = level4_code
                current_level = 4
            elif is_circular_reference(level4_code, partner_id):
                circular_references_count += 1
            
            # Add Level 5 if exists and is not a circular reference
            if level5_code and level5_desc and not is_circular_reference(level5_code, partner_id):
                partner_tree_entries.add((level5_code, level5_desc, level5_desc, "Level_5", parent_code))
                parent_code = level5_code
                current_level = 5
            elif is_circular_reference(level5_code, partner_id):
                circular_references_count += 1
            
            # Determine PartnerGroupCode using hierarchical PartnerTree codes (not partner ID)
            partner_group_code = ""
            if level5_code and level5_desc and not is_circular_reference(level5_code, partner_id):
                partner_group_code = level5_code
            elif level4_code and level4_desc and not is_circular_reference(level4_code, partner_id):
                partner_group_code = level4_code
            elif level3_code and level3_desc and not is_circular_reference(level3_code, partner_id):
                partner_group_code = level3_code
            elif level2_code and level2_desc and level2_code != level1_code and not is_circular_reference(level2_code, partner_id):
                partner_group_code = level2_code
            else:
                partner_group_code = level1_code
            
            # Extract SF partner data using AccountNumber lookup
            sf_data = account_to_partner_data_mapping.get(partner_id, {})
            liaison_office_code = sf_data.get('liaison_office_code', '')
            
            # Extract logo URL using partner_id lookup
            logo_url = ""
            if partner_id in partner_to_logo_mapping:
                logo_url = partner_to_logo_mapping[partner_id]
            
            # Extract PartnerFocalPointUserId using account number lookup
            partner_focal_point_user_id = None
            if partner_id in account_to_userid_mapping:
                partner_focal_point_user_id = account_to_userid_mapping[partner_id]
            
            # Extract and map SF fields to partner fields
            sf_status = sf_data.get('sf_status', 'Active')  # Default to Active
            sf_new_engagement = sf_data.get('sf_new_engagement', 'Allowed')  # Default to Allowed
            sf_reason_no_engagement = sf_data.get('sf_reason_no_engagement', '')
            sf_pooled_fund = sf_data.get('sf_pooled_fund', 'No')
            sf_global_key_partner = sf_data.get('sf_global_key_partner', 'FALSE')
            sf_un_secretariat = sf_data.get('sf_un_secretariat', 'FALSE')
            sf_dd_required = sf_data.get('sf_dd_required', 'No')  # Default to No
            sf_dd_approval = sf_data.get('sf_dd_approval', 'Not Done')  # Default to Not Done
            sf_levy_status = sf_data.get('sf_levy_status', 'Does not apply')  # Default to Does not apply
            sf_reason_levy = sf_data.get('sf_reason_levy', '')
            sf_levy_treatment = sf_data.get('sf_levy_treatment', '')
            sf_eac_reference = sf_data.get('sf_eac_reference', '')
            
            # Create partner entry
            if partner_id and partner_name:
                partner_data = {
                    'partner_code': partner_id,
                    'name': partner_name,
                    'short_description': partner_short,
                    'status': convert_sf_status_to_entitystatus(sf_status),
                    'partner_group_code': partner_group_code,
                    'liaison_office_code': liaison_office_code,
                    'logo_url': logo_url,
                    'can_create_new_opportunities': convert_boolean_field(sf_new_engagement),
                    'pooled_fund': convert_boolean_field(sf_pooled_fund),
                    'key_global_partner': convert_boolean_field(sf_global_key_partner),
                    'un_secretariat_partner': convert_boolean_field(sf_un_secretariat),
                    'reason_no_opportunity': sf_reason_no_engagement,
                    'due_diligence_required': convert_sf_dd_required(sf_dd_required),
                    'due_diligence_approval': convert_sf_dd_approval(sf_dd_approval),
                    'partner_levy_status': convert_sf_levy_status(sf_levy_status),
                    'reason_for_levy': sf_reason_levy,
                    'levy_treatment': sf_levy_treatment,
                    # New approval fields - all partners are approved from Salesforce
                    'partner_approval_status': '1',  # Approved
                    'partner_approval_date': 'DateTime.UtcNow',
                    'partner_approval_reference': sf_eac_reference if sf_eac_reference else None,
                    'partner_approved_by': 'Salesforce Migration',
                    'partner_focal_point_user_id': partner_focal_point_user_id
                }
                partners.append(partner_data)
    
    print(f"Detected and skipped {circular_references_count} circular references")
    return partners, list(partner_tree_entries)

def generate_partner_tree_seeder(tree_entries):
    """Generate PartnerTreeSeeder.cs"""
    print(f"Generating PartnerTree seeder with {len(tree_entries)} entries...")
    
    # Sort entries by level to ensure proper hierarchy
    level_order = {'Level_1': 1, 'Level_2': 2, 'Level_3': 3, 'Level_4': 4, 'Level_5': 5}
    tree_entries.sort(key=lambda x: (level_order.get(x[3], 6), x[0]))  # Sort by level, then by code
    
    # Create mapping from Code to ID for use in partner seeder
    partner_tree_mapping = {}
    for i, entry in enumerate(tree_entries):
        code = entry[0]
        id_value = i + 1
        partner_tree_mapping[code] = id_value
    
    seeder_code = '''using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class PartnerTreeSeeder
    {
        public static async Task SeedPartnerTreesAsync(UNOPSAppDbContext context)
        {
            if (await context.PartnerTrees.AnyAsync())
            {
                return;
            }

            var partnerTrees = new List<UNOPSPartnerTree>
            {
'''

    # Generate entries
    for i, entry in enumerate(tree_entries):
        code, name, description, level_type, parent = entry
        id_value = i + 1  # Auto-increment ID starting from 1
        
        seeder_code += f'''                new UNOPSPartnerTree
                {{
                    Code = "{code}",
                    Name = "{name}",
                    Description = "{description}",
                    Type = "{level_type}",
                    Parent = "{parent}",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }}'''
        
        # Add comma if not the last item
        if i < len(tree_entries) - 1:
            seeder_code += ','
        
        seeder_code += '\n'

    # Close the seeder class
    seeder_code += '''            };

            await context.PartnerTrees.AddRangeAsync(partnerTrees);
            await context.SaveChangesAsync();
        }
    }
}'''

    # Save to file
    output_file = '../../Seed/PartnerTreeSeeder.cs'
    with open(output_file, 'w', encoding='utf-8') as cs_file:
        cs_file.write(seeder_code)

    print(f"PartnerTree seeder code generated and saved to {output_file}")
    return output_file, partner_tree_mapping

def generate_partner_seeder(partners, partner_tree_mapping):
    """Generate PartnerSeeder.cs"""
    print(f"Generating Partner seeder with {len(partners)} entries...")
    
    seeder_code = '''using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class PartnerSeeder
    {
        public static async Task SeedPartnersAsync(UNOPSAppDbContext context)
        {
            if (await context.Partners.AnyAsync())
            {
                return;
            }

            // Create mapping from LiaisonOffice Code to Id
            var liaisonOfficeMapping = await context.LiaisonOffices
                .ToDictionaryAsync(lo => lo.Code, lo => lo.Id);

            // Create mapping from PartnerTree Code to Id
            var partnerTreeMapping = await context.PartnerTrees
                .ToDictionaryAsync(pt => pt.Code, pt => pt.Id);

            var partners = new List<UNOPSPartner>
            {
'''

    # Generate partner entries
    for i, partner in enumerate(partners):
        # Handle null values for nullable fields
        partner_group_code = f'"{partner["partner_group_code"]}"' if partner['partner_group_code'] else "null"
        
        # Generate PartnerGroupId lookup using PartnerTree mapping
        if partner['partner_group_code']:
            partner_group_id_lookup = f'partnerTreeMapping.ContainsKey("{partner["partner_group_code"]}") ? partnerTreeMapping["{partner["partner_group_code"]}"] : (int?)null'
        else:
            partner_group_id_lookup = "null"
        
        # Generate liaison office lookup code (using the same pattern as original seeder)
        if partner['liaison_office_code']:
            liaison_office_lookup = f'liaisonOfficeMapping.ContainsKey("{partner["liaison_office_code"]}") ? liaisonOfficeMapping["{partner["liaison_office_code"]}"] : (int?)null'
        else:
            liaison_office_lookup = "null"
        
        # Handle logo URL - use null if empty
        logo_url_value = f'"{partner["logo_url"]}"' if partner['logo_url'] else 'null'
        
        # Handle PartnerFocalPointUserId - use null if None
        partner_focal_point_user_id_value = partner['partner_focal_point_user_id'] if partner['partner_focal_point_user_id'] is not None else 'null'
        
        seeder_code += f'''                new UNOPSPartner
                {{
                    Name = "{partner['name']}",
                    PartnerShortDescription = "{partner['short_description']}",
                    Status = (EntityStatus){partner['status']},
                    PartnerGroupId = {partner_group_id_lookup},
                    LiaisonOfficeId = {liaison_office_lookup},
                    LogoUrl = {logo_url_value},
                    CanCreateNewOpportunities = {partner['can_create_new_opportunities']},
                    PooledFund = {partner['pooled_fund']},
                    KeyGlobalPartner = {partner['key_global_partner']},
                    UNSecretariatPartner = {partner['un_secretariat_partner']},
                    ReasonForNoNewOpportunity = "{partner['reason_no_opportunity']}",
                    DueDiligenceRequired = (DueDiligenceRequired){partner['due_diligence_required']},
                    DueDiligenceApproval = (DueDiligenceApproval){partner['due_diligence_approval']},
                    PartnerLevyStatus = (PartnerLevyStatus){partner['partner_levy_status']},
                    ReasonForLevy = "{partner['reason_for_levy']}",
                    LevyTreatment = "{partner['levy_treatment']}",
                    PartnerApprovalStatus = (PartnerApprovalStatus){partner['partner_approval_status']},
                    PartnerApprovalDate = {partner['partner_approval_date']},
                    PartnerApprovalReference = {f'"{partner["partner_approval_reference"]}"' if partner['partner_approval_reference'] else 'null'},
                    PartnerApprovedBy = "{partner['partner_approved_by']}",
                    PartnerFocalPointUserId = {partner_focal_point_user_id_value},
                    ErpDimValue = {f'int.Parse("{partner["partner_code"]}")' if partner['partner_code'] and partner['partner_code'].isdigit() else 'null'},
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }}'''
        
        # Add comma if not the last item
        if i < len(partners) - 1:
            seeder_code += ','
        
        seeder_code += '\n'

    # Close the seeder class
    seeder_code += '''            };

            await context.Partners.AddRangeAsync(partners);
            await context.SaveChangesAsync();
        }
    }
}'''

    # Save to file
    output_file = '../../Seed/PartnerSeeder.cs'
    with open(output_file, 'w', encoding='utf-8') as cs_file:
        cs_file.write(seeder_code)

    print(f"Partner seeder code generated and saved to {output_file}")
    return output_file

def main():
    """Main execution function"""
    print("=== Combined Hierarchy Seeder Generator ===")
    print("Using: PartnerTreeExport - Combined Hierarchy 28 Aug 2025.csv")
    
    # Parse the CSV
    partners, tree_entries = parse_combined_hierarchy_csv()
    
    print(f"\nExtracted {len(partners)} partners")
    print(f"Extracted {len(tree_entries)} partner tree entries")
    
    # Show distribution by level
    level_counts = defaultdict(int)
    for entry in tree_entries:
        level_counts[entry[3]] += 1
    
    print("\nPartner Tree Level Distribution:")
    for level in ['Level_1', 'Level_2', 'Level_3', 'Level_4', 'Level_5']:
        if level in level_counts:
            print(f"  - {level}: {level_counts[level]}")
    
    # Generate seeders
    tree_file, partner_tree_mapping = generate_partner_tree_seeder(tree_entries)
    partner_file = generate_partner_seeder(partners, partner_tree_mapping)
    
    print(f"\n=== Generation Complete ===")
    print(f"Generated: {tree_file}")
    print(f"Generated: {partner_file}")
    
    # Show some examples
    if partners:
        print(f"\nFirst 5 partners:")
        for i, p in enumerate(partners[:5]):
            print(f"  {i+1}. {p['partner_code']}: {p['name']} (Group: {p['partner_group_code']})")
    
    if tree_entries:
        print(f"\nFirst 5 tree entries:")
        for i, entry in enumerate(tree_entries[:5]):
            code, name, desc, level, parent = entry
            parent_info = f" (Parent: {parent})" if parent else ""
            print(f"  {i+1}. [{level}] {code}: {name}{parent_info}")

if __name__ == "__main__":
    main()