using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class UnapprovedPartner_Update_With_CategoryGroup_Seeder_v3
    {
        public static async Task UpdateUnapprovedPartnersWithCategoryGroupAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting Unapproved Partner Category/Group update process (v3)...");
            
            int skippedCount = 0;
            int categoryUpdatedCount = 0;
            int groupUpdatedCount = 0;
            int notFoundCount = 0;
            
            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Record 1: Partner Name=AAIC Japan Co., Ltd.
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "AAIC Japan Co., Ltd." && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'AAIC Japan Co., Ltd.' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'AAIC Japan Co., Ltd.'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "AAIC");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'AAIC' not found for Partner Name 'AAIC Japan Co., Ltd.'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'AAIC Japan Co., Ltd.' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'AAIC Japan Co., Ltd.' - PartnerGroupId set to PartnerTree Code 'AAIC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'AAIC Japan Co., Ltd.' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 2: Partner Name=AEF Africa-Europe Foundation
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "AEF Africa-Europe Foundation" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'AEF Africa-Europe Foundation' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "FOUNDATION");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'FOUNDATION' not found for Partner Name 'AEF Africa-Europe Foundation'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "AEF");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'AEF' not found for Partner Name 'AEF Africa-Europe Foundation'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'AEF Africa-Europe Foundation' - PartnerCategoryId set to PartnerTree Code 'FOUNDATION'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'AEF Africa-Europe Foundation' - PartnerGroupId set to PartnerTree Code 'AEF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'AEF Africa-Europe Foundation' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 3: Partner Name=Allm Inc.
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Allm Inc." && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Allm Inc.' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'Allm Inc.'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ALLM");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ALLM' not found for Partner Name 'Allm Inc.'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Allm Inc.' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Allm Inc.' - PartnerGroupId set to PartnerTree Code 'Allm'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Allm Inc.' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 4: Partner Name=British Virgin Islands
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "British Virgin Islands" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'British Virgin Islands' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'British Virgin Islands'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UK");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UK' not found for Partner Name 'British Virgin Islands'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'British Virgin Islands' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'British Virgin Islands' - PartnerGroupId set to PartnerTree Code 'UK'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'British Virgin Islands' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 5: Partner Name=Camara de Comercio de Cortes
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Camara de Comercio de Cortes" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Camara de Comercio de Cortes' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'Camara de Comercio de Cortes'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CAMARA_DE_COMERCIO_DE_CO");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CAMARA_DE_COMERCIO_DE_CO' not found for Partner Name 'Camara de Comercio de Cortes'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Camara de Comercio de Cortes' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Camara de Comercio de Cortes' - PartnerGroupId set to PartnerTree Code 'CAMARA_DE_COMERCIO_DE_CO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Camara de Comercio de Cortes' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 6: Partner Name=Carlsberg Group A/S
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Carlsberg Group A/S" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Carlsberg Group A/S' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'Carlsberg Group A/S'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CARLSBERG");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CARLSBERG' not found for Partner Name 'Carlsberg Group A/S'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Carlsberg Group A/S' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Carlsberg Group A/S' - PartnerGroupId set to PartnerTree Code 'CARLSBERG'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Carlsberg Group A/S' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 7: Partner Name=Comunità Sant\'Egidio
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Comunità Sant\'Egidio" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Comunità Sant\'Egidio' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OTHER");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OTHER' not found for Partner Name 'Comunità Sant\'Egidio'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CAMARA_DE_COMERCIO_DE_CO");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CAMARA_DE_COMERCIO_DE_CO' not found for Partner Name 'Comunità Sant\'Egidio'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Comunità Sant\'Egidio' - PartnerCategoryId set to PartnerTree Code 'OTHER'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Comunità Sant\'Egidio' - PartnerGroupId set to PartnerTree Code 'CAMARA_DE_COMERCIO_DE_CO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Comunità Sant\'Egidio' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 8: Partner Name=FPI - European Commission
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "FPI - European Commission" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'FPI - European Commission' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_INGO");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner Name 'FPI - European Commission'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EU");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU' not found for Partner Name 'FPI - European Commission'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'FPI - European Commission' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'FPI - European Commission' - PartnerGroupId set to PartnerTree Code 'EU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'FPI - European Commission' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 9: Partner Name=Hotel New Otani Tokyo
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Hotel New Otani Tokyo" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Hotel New Otani Tokyo' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'Hotel New Otani Tokyo'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "HOTEL_NEW_OTANI_TOKYO");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'HOTEL_NEW_OTANI_TOKYO' not found for Partner Name 'Hotel New Otani Tokyo'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Hotel New Otani Tokyo' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Hotel New Otani Tokyo' - PartnerGroupId set to PartnerTree Code 'HOTEL_NEW_OTANI_TOKYO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Hotel New Otani Tokyo' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 10: Partner Name=Human Practice Foundation
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Human Practice Foundation" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Human Practice Foundation' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "FOUNDATION");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'FOUNDATION' not found for Partner Name 'Human Practice Foundation'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "HUMAN_PRACTICE_FDN");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'HUMAN_PRACTICE_FDN' not found for Partner Name 'Human Practice Foundation'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Human Practice Foundation' - PartnerCategoryId set to PartnerTree Code 'FOUNDATION'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Human Practice Foundation' - PartnerGroupId set to PartnerTree Code 'HUMAN_PRACTICE_FDN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Human Practice Foundation' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 11: Partner Name=IFU - Impact Fund Denmark
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "IFU - Impact Fund Denmark" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'IFU - Impact Fund Denmark' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'IFU - Impact Fund Denmark'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IFU");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IFU' not found for Partner Name 'IFU - Impact Fund Denmark'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'IFU - Impact Fund Denmark' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'IFU - Impact Fund Denmark' - PartnerGroupId set to PartnerTree Code 'IFU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'IFU - Impact Fund Denmark' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 12: Partner Name=Japan Embassy Conakry
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Japan Embassy Conakry" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Japan Embassy Conakry' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'Japan Embassy Conakry'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JAPAN");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JAPAN' not found for Partner Name 'Japan Embassy Conakry'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Japan Embassy Conakry' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Japan Embassy Conakry' - PartnerGroupId set to PartnerTree Code 'JAPAN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Japan Embassy Conakry' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 13: Partner Name=Japan Embassy Guinea
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Japan Embassy Guinea" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Japan Embassy Guinea' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'Japan Embassy Guinea'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JAPAN");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JAPAN' not found for Partner Name 'Japan Embassy Guinea'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Japan Embassy Guinea' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Japan Embassy Guinea' - PartnerGroupId set to PartnerTree Code 'JAPAN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Japan Embassy Guinea' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 14: Partner Name=Ministry of Climate, Energy and Utilities of Denmark
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Ministry of Climate, Energy and Utilities of Denmark" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Ministry of Climate, Energy and Utilities of Denmark' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'Ministry of Climate, Energy and Utilities of Denmark'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "DENMARK");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'DENMARK' not found for Partner Name 'Ministry of Climate, Energy and Utilities of Denmark'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Climate, Energy and Utilities of Denmark' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Climate, Energy and Utilities of Denmark' - PartnerGroupId set to PartnerTree Code 'DENMARK'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Ministry of Climate, Energy and Utilities of Denmark' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 15: Partner Name=Ministry of Economy, Trade and Industry (METI) Japan
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Ministry of Economy, Trade and Industry (METI) Japan" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Ministry of Economy, Trade and Industry (METI) Japan' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'Ministry of Economy, Trade and Industry (METI) Japan'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JAPAN");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JAPAN' not found for Partner Name 'Ministry of Economy, Trade and Industry (METI) Japan'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Economy, Trade and Industry (METI) Japan' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Economy, Trade and Industry (METI) Japan' - PartnerGroupId set to PartnerTree Code 'JAPAN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Ministry of Economy, Trade and Industry (METI) Japan' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 16: Partner Name=Ministry of Foreign Affairs of Italy
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Ministry of Foreign Affairs of Italy" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Ministry of Foreign Affairs of Italy' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'Ministry of Foreign Affairs of Italy'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ITA001");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ITA001' not found for Partner Name 'Ministry of Foreign Affairs of Italy'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Foreign Affairs of Italy' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Foreign Affairs of Italy' - PartnerGroupId set to PartnerTree Code 'ITA001'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Ministry of Foreign Affairs of Italy' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 17: Partner Name=Ministry of Health, Labour and Welfare (MHLW) Japan
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Ministry of Health, Labour and Welfare (MHLW) Japan" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Ministry of Health, Labour and Welfare (MHLW) Japan' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'Ministry of Health, Labour and Welfare (MHLW) Japan'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JAPAN");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JAPAN' not found for Partner Name 'Ministry of Health, Labour and Welfare (MHLW) Japan'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Health, Labour and Welfare (MHLW) Japan' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Ministry of Health, Labour and Welfare (MHLW) Japan' - PartnerGroupId set to PartnerTree Code 'JAPAN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Ministry of Health, Labour and Welfare (MHLW) Japan' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 18: Partner Name=NEC Corporation
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "NEC Corporation" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'NEC Corporation' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'NEC Corporation'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "NEC");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'NEC' not found for Partner Name 'NEC Corporation'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'NEC Corporation' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'NEC Corporation' - PartnerGroupId set to PartnerTree Code 'NEC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'NEC Corporation' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 19: Partner Name=Nomura Research Institute (NRI)
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Nomura Research Institute (NRI)" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Nomura Research Institute (NRI)' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ACADEMIC_TRAINING_RESEARC");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ACADEMIC_TRAINING_RESEARC' not found for Partner Name 'Nomura Research Institute (NRI)'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "NRI");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'NRI' not found for Partner Name 'Nomura Research Institute (NRI)'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Nomura Research Institute (NRI)' - PartnerCategoryId set to PartnerTree Code 'ACADEMIC_TRAINING_RESEARC'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Nomura Research Institute (NRI)' - PartnerGroupId set to PartnerTree Code 'NRI'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Nomura Research Institute (NRI)' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 20: Partner Name=RCO Mali
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "RCO Mali" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'RCO Mali' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNITED_NATIONS");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner Name 'RCO Mali'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_DCO");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_DCO' not found for Partner Name 'RCO Mali'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'RCO Mali' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'RCO Mali' - PartnerGroupId set to PartnerTree Code 'UN_DCO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'RCO Mali' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 21: Partner Name=Secretaría de Relaciones Exteriores y Cooperación Internacional
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Secretaría de Relaciones Exteriores y Cooperación Internacional" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Secretaría de Relaciones Exteriores y Cooperación Internacional' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "GOVERNMENT");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner Name 'Secretaría de Relaciones Exteriores y Cooperación Internacional'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "HONDURAS");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'HONDURAS' not found for Partner Name 'Secretaría de Relaciones Exteriores y Cooperación Internacional'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Secretaría de Relaciones Exteriores y Cooperación Internacional' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Secretaría de Relaciones Exteriores y Cooperación Internacional' - PartnerGroupId set to PartnerTree Code 'HONDURAS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Secretaría de Relaciones Exteriores y Cooperación Internacional' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 22: Partner Name=Twinbird Corporation
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Twinbird Corporation" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Twinbird Corporation' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'Twinbird Corporation'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "TWINBIRD");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'TWINBIRD' not found for Partner Name 'Twinbird Corporation'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Twinbird Corporation' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Twinbird Corporation' - PartnerGroupId set to PartnerTree Code 'TWINBIRD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Twinbird Corporation' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 23: Partner Name=UN in Rome
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "UN in Rome" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'UN in Rome' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNITED_NATIONS");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner Name 'UN in Rome'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_-_ROME");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_-_ROME' not found for Partner Name 'UN in Rome'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'UN in Rome' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'UN in Rome' - PartnerGroupId set to PartnerTree Code 'UN_-_ROME'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'UN in Rome' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 24: Partner Name=UN Integrated Strategy for the Sahel (UNISS)
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "UN Integrated Strategy for the Sahel (UNISS)" && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'UN Integrated Strategy for the Sahel (UNISS)' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNITED_NATIONS");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner Name 'UN Integrated Strategy for the Sahel (UNISS)'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_DCO");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_DCO' not found for Partner Name 'UN Integrated Strategy for the Sahel (UNISS)'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'UN Integrated Strategy for the Sahel (UNISS)' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'UN Integrated Strategy for the Sahel (UNISS)' - PartnerGroupId set to PartnerTree Code 'UN_DCO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'UN Integrated Strategy for the Sahel (UNISS)' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Record 25: Partner Name=Yamaha Motor Co., Ltd.
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == "Yamaha Motor Co., Ltd." && p.ErpDimValue == null);
                    
                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with Name 'Yamaha Motor Co., Ltd.' and ErpDimValue == null does not exist.");
                        notFoundCount++;
                    }
                    else
                    {
                        bool categoryUpdated = false;
                        bool groupUpdated = false;

                        // Update PartnerCategoryId if null and category code is provided
                        if (partner.PartnerCategoryId == null)
                        {
                            var categoryTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PRIVATE_SECTOR");
                            
                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner Name 'Yamaha Motor Co., Ltd.'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "YAMAHA");
                            
                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'YAMAHA' not found for Partner Name 'Yamaha Motor Co., Ltd.'");
                            }
                        }
                        
                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated) 
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Yamaha Motor Co., Ltd.' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner 'Yamaha Motor Co., Ltd.' - PartnerGroupId set to PartnerTree Code 'YAMAHA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner 'Yamaha Motor Co., Ltd.' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }
                
                // Commit transaction
                await transaction.CommitAsync();
                
                Console.WriteLine($"\nUnapproved Partner Category/Group update completed successfully.");
                Console.WriteLine($"Total partners processed: {notFoundCount + skippedCount + Math.Max(categoryUpdatedCount, groupUpdatedCount)}");
                Console.WriteLine($"Partners not found: {notFoundCount}");
                Console.WriteLine($"Partners skipped (already populated): {skippedCount}");
                Console.WriteLine($"Partners with PartnerCategoryId updated: {categoryUpdatedCount}");
                Console.WriteLine($"Partners with PartnerGroupId updated: {groupUpdatedCount}");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during Unapproved Partner Category/Group update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
