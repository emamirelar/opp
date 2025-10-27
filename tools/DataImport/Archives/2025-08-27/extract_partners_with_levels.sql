-- Requête SQL pour extraire les Partners avec leurs niveaux de partner code
-- Cette requête joint les Partners avec la hiérarchie complète des PartnerTrees

WITH RECURSIVE partner_hierarchy AS (
    -- Niveau de base : récupérer le niveau direct du partner
    SELECT 
        pt."Code",
        pt."Name",
        pt."Description",
        pt."Type",
        pt."Parent",
        pt."PartnerCategoryCode",
        pt."PartnerGroupCode",
        1 as level_depth,
        pt."Code" as original_code
    FROM public."PartnerTrees" pt
    
    UNION ALL
    
    -- Récursion : remonter la hiérarchie
    SELECT 
        parent_pt."Code",
        parent_pt."Name", 
        parent_pt."Description",
        parent_pt."Type",
        parent_pt."Parent",
        parent_pt."PartnerCategoryCode",
        parent_pt."PartnerGroupCode",
        ph.level_depth + 1,
        ph.original_code
    FROM public."PartnerTrees" parent_pt
    INNER JOIN partner_hierarchy ph ON parent_pt."Code" = ph."Parent"
    WHERE ph."Parent" IS NOT NULL
),
partner_levels AS (
    -- Organiser les niveaux hiérarchiques
    SELECT 
        ph.original_code,
        MAX(CASE WHEN ph."Type" = 'Level_1' THEN ph."Name" END) as level_1_name,
        MAX(CASE WHEN ph."Type" = 'Level_1' THEN ph."Code" END) as level_1_code,
        MAX(CASE WHEN ph."Type" = 'Level_2' THEN ph."Name" END) as level_2_name,
        MAX(CASE WHEN ph."Type" = 'Level_2' THEN ph."Code" END) as level_2_code,
        MAX(CASE WHEN ph."Type" = 'Level_3' THEN ph."Name" END) as level_3_name,
        MAX(CASE WHEN ph."Type" = 'Level_3' THEN ph."Code" END) as level_3_code,
        MAX(CASE WHEN ph."Type" = 'Level_4' THEN ph."Name" END) as level_4_name,
        MAX(CASE WHEN ph."Type" = 'Level_4' THEN ph."Code" END) as level_4_code
    FROM partner_hierarchy ph
    GROUP BY ph.original_code
)

-- Requête principale : Partners avec leurs niveaux
SELECT 
    p."ErpDimValue" as account_number,
    p."Name" as partner_name,
    p."PartnerShortDescription" as partner_short_description,
    p."PartnerLongDescription" as partner_long_description,
    p."Status" as partner_status,
    p."PartnerGroupCode" as direct_partner_group_code,
    pt."Name" as direct_partner_group_name,
    pt."Type" as parent_level_type,
    pt."Description" as direct_partner_group_description,
    
    -- Hiérarchie complète
    pl.level_1_name,
    pl.level_1_code,
    pl.level_2_name,
    pl.level_2_code,
    pl.level_3_name,
    pl.level_3_code,
    pl.level_4_name,
    pl.level_4_code,
    
    -- Chemin hiérarchique complet
    CONCAT_WS(' → ', 
        pl.level_1_name, 
        pl.level_2_name, 
        pl.level_3_name, 
        pl.level_4_name
    ) as full_hierarchy_path,
    
    -- Informations additionnelles
    p."ErpDimValue" as erp_dimension_value,
    p."LiaisonOfficeId" as liaison_office_id,
    p."CreatedBy" as created_by,
    p."CreatedDate" as created_date,
    p."LastModifiedBy" as last_modified_by,
    p."LastModifiedDate" as last_modified_date

FROM public."Partners" p
LEFT JOIN public."PartnerTrees" pt ON p."PartnerGroupCode" = pt."Code"
LEFT JOIN partner_levels pl ON p."PartnerGroupCode" = pl.original_code

-- Filtres optionnels (décommenter selon besoin)
-- WHERE p."Status" = 'Active'  -- Seulement les partners actifs
-- WHERE pt."Type" IS NOT NULL   -- Seulement les partners avec classification

ORDER BY 
    pl.level_1_name,
    pl.level_2_name,
    pl.level_3_name,
    pl.level_4_name,
    p."Name";