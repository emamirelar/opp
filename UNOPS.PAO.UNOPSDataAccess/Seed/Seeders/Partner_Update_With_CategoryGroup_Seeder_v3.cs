using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class Partner_Update_With_CategoryGroup_Seeder_v3
    {
        public static async Task UpdatePartnersWithCategoryGroupAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("Starting Partner Category/Group update process (v3)...");

            int skippedCount = 0;
            int categoryUpdatedCount = 0;
            int groupUpdatedCount = 0;
            int notFoundCount = 0;

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Record 1: Partner ErpDimValue=1945
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1945);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1945' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner ErpDimValue '1945'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CC001");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CC001' not found for Partner ErpDimValue '1945'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1945' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1945' - PartnerGroupId set to PartnerTree Code 'CC001'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1945' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 2: Partner ErpDimValue=1942
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1942);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1942' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'OTHER' not found for Partner ErpDimValue '1942'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "COG01");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'COG01' not found for Partner ErpDimValue '1942'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1942' - PartnerCategoryId set to PartnerTree Code 'OTHER'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1942' - PartnerGroupId set to PartnerTree Code 'COG01'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1942' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 3: Partner ErpDimValue=1011
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1011);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1011' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1011'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CAF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CAF' not found for Partner ErpDimValue '1011'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1011' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1011' - PartnerGroupId set to PartnerTree Code 'CAF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1011' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 4: Partner ErpDimValue=1250
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1250);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1250' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1250'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IMF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IMF' not found for Partner ErpDimValue '1250'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1250' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1250' - PartnerGroupId set to PartnerTree Code 'IMF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1250' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 5: Partner ErpDimValue=1437
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1437);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1437' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1437'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "AFDB");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'AFDB' not found for Partner ErpDimValue '1437'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1437' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1437' - PartnerGroupId set to PartnerTree Code 'AFDB'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1437' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 6: Partner ErpDimValue=1438
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1438);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1438' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1438'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ADB");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ADB' not found for Partner ErpDimValue '1438'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1438' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1438' - PartnerGroupId set to PartnerTree Code 'ADB'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1438' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 7: Partner ErpDimValue=1439
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1439);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1439' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1439'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CDB");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CDB' not found for Partner ErpDimValue '1439'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1439' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1439' - PartnerGroupId set to PartnerTree Code 'CDB'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1439' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 8: Partner ErpDimValue=1440
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1440);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1440' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1440'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CFC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CFC' not found for Partner ErpDimValue '1440'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1440' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1440' - PartnerGroupId set to PartnerTree Code 'CFC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1440' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 9: Partner ErpDimValue=1441
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1441);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1441' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1441'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EBRD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EBRD' not found for Partner ErpDimValue '1441'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1441' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1441' - PartnerGroupId set to PartnerTree Code 'EBRD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1441' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 10: Partner ErpDimValue=1571
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1571);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1571' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1571'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IsDB");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IsDB' not found for Partner ErpDimValue '1571'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1571' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1571' - PartnerGroupId set to PartnerTree Code 'IsDB'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1571' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 11: Partner ErpDimValue=1572
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1572);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1572' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1572'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "AFESD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'AFESD' not found for Partner ErpDimValue '1572'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1572' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1572' - PartnerGroupId set to PartnerTree Code 'AFESD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1572' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 12: Partner ErpDimValue=1793
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1793);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1793' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1793'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "AIIB");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'AIIB' not found for Partner ErpDimValue '1793'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1793' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1793' - PartnerGroupId set to PartnerTree Code 'AIIB'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1793' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 13: Partner ErpDimValue=1817
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1817);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1817' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1817'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OFID");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OFID' not found for Partner ErpDimValue '1817'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1817' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1817' - PartnerGroupId set to PartnerTree Code 'OFID'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1817' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 14: Partner ErpDimValue=1925
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1925);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1925' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "REG_OTH_FI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_FI' not found for Partner ErpDimValue '1925'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "BOAD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'BOAD' not found for Partner ErpDimValue '1925'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1925' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_FI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1925' - PartnerGroupId set to PartnerTree Code 'BOAD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1925' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 15: Partner ErpDimValue=1948
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1948);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1948' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner ErpDimValue '1948'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MAI001");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MAI001' not found for Partner ErpDimValue '1948'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1948' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1948' - PartnerGroupId set to PartnerTree Code 'MAI001'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1948' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 16: Partner ErpDimValue=1938
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1938);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1938' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "MPI");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MPI' not found for Partner ErpDimValue '1938'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EIF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EIF' not found for Partner ErpDimValue '1938'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1938' - PartnerCategoryId set to PartnerTree Code 'MPI'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1938' - PartnerGroupId set to PartnerTree Code 'EIF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1938' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 17: Partner ErpDimValue=1933
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1933);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1933' does not exist.");
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
                                .FirstOrDefaultAsync(pt => pt.Code == "NGO");

                            if (categoryTree != null)
                            {
                                partner.PartnerCategoryId = categoryTree.Id;
                                categoryUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'NGO' not found for Partner ErpDimValue '1933'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "NEH001");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'NEH001' not found for Partner ErpDimValue '1933'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1933' - PartnerCategoryId set to PartnerTree Code 'NGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1933' - PartnerGroupId set to PartnerTree Code 'NEH001'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1933' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 18: Partner ErpDimValue=1949
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1949);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1949' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'GOVERNMENT' not found for Partner ErpDimValue '1949'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PNG001");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PNG001' not found for Partner ErpDimValue '1949'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1949' - PartnerCategoryId set to PartnerTree Code 'GOVERNMENT'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1949' - PartnerGroupId set to PartnerTree Code 'PNG001'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1949' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 19: Partner ErpDimValue=1947
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1947);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1947' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner ErpDimValue '1947'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PAR001");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PAR001' not found for Partner ErpDimValue '1947'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1947' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1947' - PartnerGroupId set to PartnerTree Code 'PAR001'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1947' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 20: Partner ErpDimValue=1025
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1025);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1025' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1025'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EC' not found for Partner ErpDimValue '1025'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1025' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1025' - PartnerGroupId set to PartnerTree Code 'EC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1025' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 21: Partner ErpDimValue=1026
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1026);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1026' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1026'");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU' not found for Partner ErpDimValue '1026'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1026' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1026' - PartnerGroupId set to PartnerTree Code 'EU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1026' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 22: Partner ErpDimValue=1029
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1029);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1029' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1029'");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU' not found for Partner ErpDimValue '1029'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1029' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1029' - PartnerGroupId set to PartnerTree Code 'EU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1029' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 23: Partner ErpDimValue=1032
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1032);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1032' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1032'");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU' not found for Partner ErpDimValue '1032'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1032' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1032' - PartnerGroupId set to PartnerTree Code 'EU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1032' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 24: Partner ErpDimValue=1165
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1165);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1165' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1165'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNAMID");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNAMID' not found for Partner ErpDimValue '1165'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1165' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1165' - PartnerGroupId set to PartnerTree Code 'UNAMID'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1165' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 25: Partner ErpDimValue=1649
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1649);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1649' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1649'");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU' not found for Partner ErpDimValue '1649'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1649' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1649' - PartnerGroupId set to PartnerTree Code 'EU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1649' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 26: Partner ErpDimValue=1739
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1739);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1739' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1739'");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU' not found for Partner ErpDimValue '1739'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1739' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1739' - PartnerGroupId set to PartnerTree Code 'EU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1739' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 27: Partner ErpDimValue=1807
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1807);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1807' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1807'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EBY");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EBY' not found for Partner ErpDimValue '1807'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1807' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1807' - PartnerGroupId set to PartnerTree Code 'EBY'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1807' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 28: Partner ErpDimValue=1943
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1943);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1943' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1943'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EU_DG_MENA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU_DG_MENA' not found for Partner ErpDimValue '1943'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1943' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1943' - PartnerGroupId set to PartnerTree Code 'EU_DG_MENA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1943' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 29: Partner ErpDimValue=1944
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1944);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1944' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'REG_OTH_INGO' not found for Partner ErpDimValue '1944'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EU_DG_CLIMA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EU_DG_CLIMA' not found for Partner ErpDimValue '1944'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1944' - PartnerCategoryId set to PartnerTree Code 'REG_OTH_INGO'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1944' - PartnerGroupId set to PartnerTree Code 'EU_DG_CLIMA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1944' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 30: Partner ErpDimValue=1934
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1934);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1934' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'ACADEMIC_TRAINING_RESEARC' not found for Partner ErpDimValue '1934'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UCD001");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UCD001' not found for Partner ErpDimValue '1934'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1934' - PartnerCategoryId set to PartnerTree Code 'ACADEMIC_TRAINING_RESEARC'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1934' - PartnerGroupId set to PartnerTree Code 'UCD001'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1934' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 31: Partner ErpDimValue=1015
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1015);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1015' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1015'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SSHF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SSHF' not found for Partner ErpDimValue '1015'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1015' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1015' - PartnerGroupId set to PartnerTree Code 'SSHF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1015' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 32: Partner ErpDimValue=1027
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1027);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1027' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1027'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "EBOLA_RESPONSE_MPTF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'EBOLA_RESPONSE_MPTF' not found for Partner ErpDimValue '1027'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1027' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1027' - PartnerGroupId set to PartnerTree Code 'EBOLA_RESPONSE_MPTF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1027' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 33: Partner ErpDimValue=1151
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1151);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1151' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1151'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SYRIA_EMERGENCY_RESPONSE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SYRIA_EMERGENCY_RESPONSE' not found for Partner ErpDimValue '1151'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1151' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1151' - PartnerGroupId set to PartnerTree Code 'SYRIA_EMERGENCY_RESPONSE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1151' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 34: Partner ErpDimValue=1154
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1154);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1154' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1154'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SOMALIA_UN_MPTF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SOMALIA_UN_MPTF' not found for Partner ErpDimValue '1154'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1154' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1154' - PartnerGroupId set to PartnerTree Code 'SOMALIA_UN_MPTF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1154' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 35: Partner ErpDimValue=1166
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1166);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1166' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1166'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDF' not found for Partner ErpDimValue '1166'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1166' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1166' - PartnerGroupId set to PartnerTree Code 'UNDF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1166' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 36: Partner ErpDimValue=1168
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1168);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1168' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1168'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_GENERAL_TRUST_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_GENERAL_TRUST_FUND' not found for Partner ErpDimValue '1168'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1168' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1168' - PartnerGroupId set to PartnerTree Code 'UN_GENERAL_TRUST_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1168' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 37: Partner ErpDimValue=1226
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1226);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1226' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1226'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CERF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CERF' not found for Partner ErpDimValue '1226'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1226' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1226' - PartnerGroupId set to PartnerTree Code 'CERF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1226' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 38: Partner ErpDimValue=1237
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1237);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1237' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1237'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNPBF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNPBF' not found for Partner ErpDimValue '1237'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1237' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1237' - PartnerGroupId set to PartnerTree Code 'UNPBF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1237' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 39: Partner ErpDimValue=1239
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1239);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1239' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1239'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNVFTC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNVFTC' not found for Partner ErpDimValue '1239'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1239' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1239' - PartnerGroupId set to PartnerTree Code 'UNVFTC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1239' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 40: Partner ErpDimValue=1240
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1240);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1240' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1240'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNVFVT");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNVFVT' not found for Partner ErpDimValue '1240'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1240' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1240' - PartnerGroupId set to PartnerTree Code 'UNVFVT'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1240' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 41: Partner ErpDimValue=1241
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1241);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1241' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1241'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNVFD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNVFD' not found for Partner ErpDimValue '1241'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1241' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1241' - PartnerGroupId set to PartnerTree Code 'UNVFD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1241' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 42: Partner ErpDimValue=1255
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1255);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1255' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1255'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDEF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDEF' not found for Partner ErpDimValue '1255'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1255' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1255' - PartnerGroupId set to PartnerTree Code 'UNDEF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1255' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 43: Partner ErpDimValue=1258
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1258);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1258' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1258'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNFIP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNFIP' not found for Partner ErpDimValue '1258'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1258' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1258' - PartnerGroupId set to PartnerTree Code 'UNFIP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1258' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 44: Partner ErpDimValue=1463
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1463);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1463' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1463'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN-WATER");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN-WATER' not found for Partner ErpDimValue '1463'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1463' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1463' - PartnerGroupId set to PartnerTree Code 'UN-WATER'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1463' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 45: Partner ErpDimValue=1464
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1464);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1464' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1464'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ALBANIA_ONE_UNCF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ALBANIA_ONE_UNCF' not found for Partner ErpDimValue '1464'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1464' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1464' - PartnerGroupId set to PartnerTree Code 'ALBANIA_ONE_UNCF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1464' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 46: Partner ErpDimValue=1465
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1465);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1465' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1465'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "BHUTAN_UNCF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'BHUTAN_UNCF' not found for Partner ErpDimValue '1465'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1465' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1465' - PartnerGroupId set to PartnerTree Code 'BHUTAN_UNCF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1465' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 47: Partner ErpDimValue=1466
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1466);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1466' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1466'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "BOTSWANA_UNCF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'BOTSWANA_UNCF' not found for Partner ErpDimValue '1466'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1466' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1466' - PartnerGroupId set to PartnerTree Code 'BOTSWANA_UNCF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1466' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 48: Partner ErpDimValue=1467
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1467);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1467' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1467'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CAPE_VERDE_TRANSITION_FU");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CAPE_VERDE_TRANSITION_FU' not found for Partner ErpDimValue '1467'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1467' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1467' - PartnerGroupId set to PartnerTree Code 'CAPE_VERDE_TRANSITION_FU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1467' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 49: Partner ErpDimValue=1468
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1468);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1468' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1468'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CAR_HF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CAR_HF' not found for Partner ErpDimValue '1468'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1468' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1468' - PartnerGroupId set to PartnerTree Code 'CAR_HF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1468' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 50: Partner ErpDimValue=1469
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1469);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1469' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1469'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CFIA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CFIA' not found for Partner ErpDimValue '1469'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1469' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1469' - PartnerGroupId set to PartnerTree Code 'CFIA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1469' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 51: Partner ErpDimValue=1470
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1470);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1470' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1470'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CBA_CC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CBA_CC' not found for Partner ErpDimValue '1470'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1470' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1470' - PartnerGroupId set to PartnerTree Code 'CBA_CC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1470' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 52: Partner ErpDimValue=1471
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1471);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1471' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1471'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "COMOROS_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'COMOROS_ONE_UN_FUND' not found for Partner ErpDimValue '1471'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1471' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1471' - PartnerGroupId set to PartnerTree Code 'COMOROS_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1471' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 53: Partner ErpDimValue=1472
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1472);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1472' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1472'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "DCPSF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'DCPSF' not found for Partner ErpDimValue '1472'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1472' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1472' - PartnerGroupId set to PartnerTree Code 'DCPSF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1472' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 54: Partner ErpDimValue=1473
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1473);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1473' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1473'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "DRC_POOLED_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'DRC_POOLED_FUND' not found for Partner ErpDimValue '1473'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1473' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1473' - PartnerGroupId set to PartnerTree Code 'DRC_POOLED_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1473' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 55: Partner ErpDimValue=1474
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1474);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1474' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1474'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "DRC_STABILIZATION_AND_RE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'DRC_STABILIZATION_AND_RE' not found for Partner ErpDimValue '1474'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1474' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1474' - PartnerGroupId set to PartnerTree Code 'DRC_STABILIZATION_AND_RE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1474' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 56: Partner ErpDimValue=1475
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1475);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1475' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1475'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ETHIOPIA_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ETHIOPIA_ONE_UN_FUND' not found for Partner ErpDimValue '1475'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1475' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1475' - PartnerGroupId set to PartnerTree Code 'ETHIOPIA_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1475' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 57: Partner ErpDimValue=1476
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1476);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1476' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1476'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "HRM_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'HRM_FUND' not found for Partner ErpDimValue '1476'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1476' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1476' - PartnerGroupId set to PartnerTree Code 'HRM_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1476' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 58: Partner ErpDimValue=1477
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1477);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1477' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1477'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "INDONESIA_DR_TF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'INDONESIA_DR_TF' not found for Partner ErpDimValue '1477'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1477' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1477' - PartnerGroupId set to PartnerTree Code 'INDONESIA_DR_TF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1477' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 59: Partner ErpDimValue=1478
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1478);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1478' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1478'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IRAQ_UNDAF_TRUST_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IRAQ_UNDAF_TRUST_FUND' not found for Partner ErpDimValue '1478'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1478' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1478' - PartnerGroupId set to PartnerTree Code 'IRAQ_UNDAF_TRUST_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1478' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 60: Partner ErpDimValue=1479
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1479);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1479' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1479'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_ARMED_VIOLENCE_PREVEN");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_ARMED_VIOLENCE_PREVEN' not found for Partner ErpDimValue '1479'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1479' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1479' - PartnerGroupId set to PartnerTree Code 'JP_ARMED_VIOLENCE_PREVEN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1479' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 61: Partner ErpDimValue=1480
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1480);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1480' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1480'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_BANGLADESH_LGSP–LIC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_BANGLADESH_LGSP–LIC' not found for Partner ErpDimValue '1480'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1480' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1480' - PartnerGroupId set to PartnerTree Code 'JP_BANGLADESH_LGSP–LIC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1480' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 62: Partner ErpDimValue=1481
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1481);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1481' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1481'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_CHAD_DIS_SECURITY");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_CHAD_DIS_SECURITY' not found for Partner ErpDimValue '1481'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1481' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1481' - PartnerGroupId set to PartnerTree Code 'JP_CHAD_DIS_SECURITY'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1481' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 63: Partner ErpDimValue=1482
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1482);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1482' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1482'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_DRC_MICROFINANCE_II");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_DRC_MICROFINANCE_II' not found for Partner ErpDimValue '1482'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1482' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1482' - PartnerGroupId set to PartnerTree Code 'JP_DRC_MICROFINANCE_II'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1482' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 64: Partner ErpDimValue=1483
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1483);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1483' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1483'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_DRC_SECURITY_SECT_REF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_DRC_SECURITY_SECT_REF' not found for Partner ErpDimValue '1483'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1483' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1483' - PartnerGroupId set to PartnerTree Code 'JP_DRC_SECURITY_SECT_REF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1483' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 65: Partner ErpDimValue=1484
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1484);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1484' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1484'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_GUATEMALA_MAYA_PROGRA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_GUATEMALA_MAYA_PROGRA' not found for Partner ErpDimValue '1484'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1484' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1484' - PartnerGroupId set to PartnerTree Code 'JP_GUATEMALA_MAYA_PROGRA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1484' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 66: Partner ErpDimValue=1485
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1485);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1485' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1485'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_GUATEMALA_RURAL_DEV");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_GUATEMALA_RURAL_DEV' not found for Partner ErpDimValue '1485'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1485' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1485' - PartnerGroupId set to PartnerTree Code 'JP_GUATEMALA_RURAL_DEV'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1485' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 67: Partner ErpDimValue=1486
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1486);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1486' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1486'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_KAZAKHSTAN_INNOV_APRC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_KAZAKHSTAN_INNOV_APRC' not found for Partner ErpDimValue '1486'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1486' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1486' - PartnerGroupId set to PartnerTree Code 'JP_KAZAKHSTAN_INNOV_APRC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1486' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 68: Partner ErpDimValue=1487
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1487);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1487' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1487'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_KENYA_HIV_AND_AIDS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_KENYA_HIV_AND_AIDS' not found for Partner ErpDimValue '1487'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1487' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1487' - PartnerGroupId set to PartnerTree Code 'JP_KENYA_HIV_AND_AIDS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1487' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 69: Partner ErpDimValue=1488
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1488);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1488' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1488'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_KOSOVO_DOMESTIC_VIOLE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_KOSOVO_DOMESTIC_VIOLE' not found for Partner ErpDimValue '1488'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1488' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1488' - PartnerGroupId set to PartnerTree Code 'JP_KOSOVO_DOMESTIC_VIOLE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1488' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 70: Partner ErpDimValue=1489
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1489);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1489' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1489'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_LAO_GOVERN/PUBLIC_ADM");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_LAO_GOVERN/PUBLIC_ADM' not found for Partner ErpDimValue '1489'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1489' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1489' - PartnerGroupId set to PartnerTree Code 'JP_LAO_GOVERN/PUBLIC_ADM'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1489' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 71: Partner ErpDimValue=1490
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1490);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1490' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1490'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_LIBERIA_FOOD_SECURITY");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_LIBERIA_FOOD_SECURITY' not found for Partner ErpDimValue '1490'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1490' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1490' - PartnerGroupId set to PartnerTree Code 'JP_LIBERIA_FOOD_SECURITY'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1490' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 72: Partner ErpDimValue=1491
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1491);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1491' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1491'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_LIBERIA_GENDER_EQUALI");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_LIBERIA_GENDER_EQUALI' not found for Partner ErpDimValue '1491'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1491' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1491' - PartnerGroupId set to PartnerTree Code 'JP_LIBERIA_GENDER_EQUALI'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1491' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 73: Partner ErpDimValue=1492
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1492);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1492' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1492'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_MALI_AGRO_PASTORAL_PR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_MALI_AGRO_PASTORAL_PR' not found for Partner ErpDimValue '1492'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1492' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1492' - PartnerGroupId set to PartnerTree Code 'JP_MALI_AGRO_PASTORAL_PR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1492' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 74: Partner ErpDimValue=1493
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1493);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1493' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1493'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_MOLDOVA_JILDP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_MOLDOVA_JILDP' not found for Partner ErpDimValue '1493'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1493' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1493' - PartnerGroupId set to PartnerTree Code 'JP_MOLDOVA_JILDP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1493' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 75: Partner ErpDimValue=1494
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1494);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1494' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1494'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_NEPAL_LGCDP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_NEPAL_LGCDP' not found for Partner ErpDimValue '1494'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1494' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1494' - PartnerGroupId set to PartnerTree Code 'JP_NEPAL_LGCDP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1494' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 76: Partner ErpDimValue=1495
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1495);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1495' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1495'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_SERBIA_SCILD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_SERBIA_SCILD' not found for Partner ErpDimValue '1495'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1495' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1495' - PartnerGroupId set to PartnerTree Code 'JP_SERBIA_SCILD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1495' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 77: Partner ErpDimValue=1496
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1496);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1496' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1496'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_SOLOMON_ISLANDS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_SOLOMON_ISLANDS' not found for Partner ErpDimValue '1496'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1496' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1496' - PartnerGroupId set to PartnerTree Code 'JP_SOLOMON_ISLANDS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1496' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 78: Partner ErpDimValue=1497
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1497);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1497' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1497'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_SOMALIA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_SOMALIA' not found for Partner ErpDimValue '1497'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1497' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1497' - PartnerGroupId set to PartnerTree Code 'JP_SOMALIA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1497' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 79: Partner ErpDimValue=1498
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1498);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1498' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1498'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_MACEDONIA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_MACEDONIA' not found for Partner ErpDimValue '1498'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1498' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1498' - PartnerGroupId set to PartnerTree Code 'JP_MACEDONIA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1498' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 80: Partner ErpDimValue=1499
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1499);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1499' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1499'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_TIMOR-LESTE_INFUSE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_TIMOR-LESTE_INFUSE' not found for Partner ErpDimValue '1499'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1499' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1499' - PartnerGroupId set to PartnerTree Code 'JP_TIMOR-LESTE_INFUSE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1499' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 81: Partner ErpDimValue=1500
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1500);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1500' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1500'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_TIMOR-LESTE_LGSP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_TIMOR-LESTE_LGSP' not found for Partner ErpDimValue '1500'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1500' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1500' - PartnerGroupId set to PartnerTree Code 'JP_TIMOR-LESTE_LGSP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1500' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 82: Partner ErpDimValue=1501
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1501);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1501' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1501'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_UGANDA_GENDER_EQUALIT");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_UGANDA_GENDER_EQUALIT' not found for Partner ErpDimValue '1501'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1501' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1501' - PartnerGroupId set to PartnerTree Code 'JP_UGANDA_GENDER_EQUALIT'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1501' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 83: Partner ErpDimValue=1502
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1502);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1502' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1502'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "JP_UGANDA_SUPPORT_FOR_AI");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'JP_UGANDA_SUPPORT_FOR_AI' not found for Partner ErpDimValue '1502'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1502' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1502' - PartnerGroupId set to PartnerTree Code 'JP_UGANDA_SUPPORT_FOR_AI'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1502' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 84: Partner ErpDimValue=1503
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1503);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1503' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1503'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "KIRIBATI_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'KIRIBATI_ONE_UN_FUND' not found for Partner ErpDimValue '1503'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1503' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1503' - PartnerGroupId set to PartnerTree Code 'KIRIBATI_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1503' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 85: Partner ErpDimValue=1504
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1504);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1504' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1504'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "KYRGYZSTAN_ONE_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'KYRGYZSTAN_ONE_FUND' not found for Partner ErpDimValue '1504'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1504' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1504' - PartnerGroupId set to PartnerTree Code 'KYRGYZSTAN_ONE_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1504' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 86: Partner ErpDimValue=1505
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1505);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1505' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1505'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "LEBANON_RECOVERY_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'LEBANON_RECOVERY_FUND' not found for Partner ErpDimValue '1505'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1505' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1505' - PartnerGroupId set to PartnerTree Code 'LEBANON_RECOVERY_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1505' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 87: Partner ErpDimValue=1506
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1506);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1506' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1506'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "LESOTHO_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'LESOTHO_ONE_UN_FUND' not found for Partner ErpDimValue '1506'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1506' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1506' - PartnerGroupId set to PartnerTree Code 'LESOTHO_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1506' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 88: Partner ErpDimValue=1507
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1507);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1507' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1507'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MALAWI_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MALAWI_ONE_UN_FUND' not found for Partner ErpDimValue '1507'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1507' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1507' - PartnerGroupId set to PartnerTree Code 'MALAWI_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1507' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 89: Partner ErpDimValue=1508
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1508);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1508' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1508'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MALDIVES_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MALDIVES_ONE_UN_FUND' not found for Partner ErpDimValue '1508'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1508' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1508' - PartnerGroupId set to PartnerTree Code 'MALDIVES_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1508' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 90: Partner ErpDimValue=1509
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1509);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1509' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1509'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MDG_ACHIEVEMENT_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MDG_ACHIEVEMENT_FUND' not found for Partner ErpDimValue '1509'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1509' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1509' - PartnerGroupId set to PartnerTree Code 'MDG_ACHIEVEMENT_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1509' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 91: Partner ErpDimValue=1510
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1510);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1510' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1510'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MONTENEGRO_UN_COUNTRY_FU");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MONTENEGRO_UN_COUNTRY_FU' not found for Partner ErpDimValue '1510'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1510' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1510' - PartnerGroupId set to PartnerTree Code 'MONTENEGRO_UN_COUNTRY_FU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1510' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 92: Partner ErpDimValue=1511
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1511);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1511' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1511'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MOZAMBIQUE_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MOZAMBIQUE_ONE_UN_FUND' not found for Partner ErpDimValue '1511'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1511' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1511' - PartnerGroupId set to PartnerTree Code 'MOZAMBIQUE_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1511' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 93: Partner ErpDimValue=1512
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1512);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1512' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1512'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "NEPAL_-_UN_PEACE_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'NEPAL_-_UN_PEACE_FUND' not found for Partner ErpDimValue '1512'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1512' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1512' - PartnerGroupId set to PartnerTree Code 'NEPAL_-_UN_PEACE_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1512' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 94: Partner ErpDimValue=1513
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1513);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1513' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1513'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PAKISTAN_ONE_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PAKISTAN_ONE_FUND' not found for Partner ErpDimValue '1513'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1513' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1513' - PartnerGroupId set to PartnerTree Code 'PAKISTAN_ONE_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1513' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 95: Partner ErpDimValue=1514
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1514);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1514' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1514'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PBF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PBF' not found for Partner ErpDimValue '1514'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1514' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1514' - PartnerGroupId set to PartnerTree Code 'PBF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1514' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 96: Partner ErpDimValue=1515
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1515);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1515' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1515'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "PNG_UN_COUNTRY_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'PNG_UN_COUNTRY_FUND' not found for Partner ErpDimValue '1515'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1515' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1515' - PartnerGroupId set to PartnerTree Code 'PNG_UN_COUNTRY_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1515' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 97: Partner ErpDimValue=1516
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1516);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1516' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1516'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "REDD+_JP_PARTNERSHIP_SUP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'REDD+_JP_PARTNERSHIP_SUP' not found for Partner ErpDimValue '1516'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1516' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1516' - PartnerGroupId set to PartnerTree Code 'REDD+_JP_PARTNERSHIP_SUP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1516' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 98: Partner ErpDimValue=1517
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1517);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1517' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1517'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "RWANDA_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'RWANDA_ONE_UN_FUND' not found for Partner ErpDimValue '1517'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1517' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1517' - PartnerGroupId set to PartnerTree Code 'RWANDA_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1517' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 99: Partner ErpDimValue=1518
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1518);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1518' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1518'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SIERRA_LEONE_MDTF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SIERRA_LEONE_MDTF' not found for Partner ErpDimValue '1518'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1518' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1518' - PartnerGroupId set to PartnerTree Code 'SIERRA_LEONE_MDTF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1518' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 100: Partner ErpDimValue=1519
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1519);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1519' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1519'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SOMALIA_COMMON_HUMANITAR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SOMALIA_COMMON_HUMANITAR' not found for Partner ErpDimValue '1519'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1519' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1519' - PartnerGroupId set to PartnerTree Code 'SOMALIA_COMMON_HUMANITAR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1519' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 101: Partner ErpDimValue=1520
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1520);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1520' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1520'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SSRF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SSRF' not found for Partner ErpDimValue '1520'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1520' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1520' - PartnerGroupId set to PartnerTree Code 'SSRF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1520' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 102: Partner ErpDimValue=1521
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1521);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1521' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1521'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SUDAN_COMMON_HUMANITARIA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SUDAN_COMMON_HUMANITARIA' not found for Partner ErpDimValue '1521'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1521' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1521' - PartnerGroupId set to PartnerTree Code 'SUDAN_COMMON_HUMANITARIA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1521' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 103: Partner ErpDimValue=1522
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1522);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1522' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1522'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "TANZANIA_ONE_UN_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'TANZANIA_ONE_UN_FUND' not found for Partner ErpDimValue '1522'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1522' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1522' - PartnerGroupId set to PartnerTree Code 'TANZANIA_ONE_UN_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1522' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 104: Partner ErpDimValue=1523
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1523);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1523' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1523'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_ACTION");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_ACTION' not found for Partner ErpDimValue '1523'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1523' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1523' - PartnerGroupId set to PartnerTree Code 'UN_ACTION'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1523' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 105: Partner ErpDimValue=1524
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1524);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1524' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1524'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_CIVIL_SOCIETY_TRUST_F");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_CIVIL_SOCIETY_TRUST_F' not found for Partner ErpDimValue '1524'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1524' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1524' - PartnerGroupId set to PartnerTree Code 'UN_CIVIL_SOCIETY_TRUST_F'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1524' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 106: Partner ErpDimValue=1525
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1525);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1525' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1525'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIPP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIPP' not found for Partner ErpDimValue '1525'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1525' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1525' - PartnerGroupId set to PartnerTree Code 'UNIPP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1525' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 107: Partner ErpDimValue=1526
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1526);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1526' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1526'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNTFHS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNTFHS' not found for Partner ErpDimValue '1526'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1526' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1526' - PartnerGroupId set to PartnerTree Code 'UNTFHS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1526' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 108: Partner ErpDimValue=1527
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1527);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1527' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1527'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_TRUST_FUND");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_TRUST_FUND' not found for Partner ErpDimValue '1527'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1527' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1527' - PartnerGroupId set to PartnerTree Code 'UN_TRUST_FUND'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1527' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 109: Partner ErpDimValue=1528
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1528);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1528' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1528'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDG_HRF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDG_HRF' not found for Partner ErpDimValue '1528'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1528' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1528' - PartnerGroupId set to PartnerTree Code 'UNDG_HRF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1528' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 110: Partner ErpDimValue=1529
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1529);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1529' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1529'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDG_ITF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDG_ITF' not found for Partner ErpDimValue '1529'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1529' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1529' - PartnerGroupId set to PartnerTree Code 'UNDG_ITF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1529' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 111: Partner ErpDimValue=1530
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1530);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1530' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1530'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN-REDD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN-REDD' not found for Partner ErpDimValue '1530'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1530' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1530' - PartnerGroupId set to PartnerTree Code 'UN-REDD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1530' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 112: Partner ErpDimValue=1531
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1531);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1531' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1531'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "URUGUAY_ONE_UN_COHERENCE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'URUGUAY_ONE_UN_COHERENCE' not found for Partner ErpDimValue '1531'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1531' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1531' - PartnerGroupId set to PartnerTree Code 'URUGUAY_ONE_UN_COHERENCE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1531' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 113: Partner ErpDimValue=1532
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1532);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1532' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1532'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "VIET_NAM_ONE_FUND_I");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'VIET_NAM_ONE_FUND_I' not found for Partner ErpDimValue '1532'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1532' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1532' - PartnerGroupId set to PartnerTree Code 'VIET_NAM_ONE_FUND_I'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1532' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 114: Partner ErpDimValue=1533
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1533);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1533' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1533'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "VIET_NAM_ONE_FUND_II");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'VIET_NAM_ONE_FUND_II' not found for Partner ErpDimValue '1533'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1533' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1533' - PartnerGroupId set to PartnerTree Code 'VIET_NAM_ONE_FUND_II'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1533' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 115: Partner ErpDimValue=1538
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1538);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1538' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1538'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OTHER_UNDP_MDTF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OTHER_UNDP_MDTF' not found for Partner ErpDimValue '1538'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1538' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1538' - PartnerGroupId set to PartnerTree Code 'OTHER_UNDP_MDTF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1538' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 116: Partner ErpDimValue=1539
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1539);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1539' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1539'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OTHER_UNDP_JP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OTHER_UNDP_JP' not found for Partner ErpDimValue '1539'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1539' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1539' - PartnerGroupId set to PartnerTree Code 'OTHER_UNDP_JP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1539' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 117: Partner ErpDimValue=1545
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1545);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1545' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1545'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNSO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNSO' not found for Partner ErpDimValue '1545'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1545' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1545' - PartnerGroupId set to PartnerTree Code 'UNSO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1545' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 118: Partner ErpDimValue=1643
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1643);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1643' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1643'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_VTF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_VTF' not found for Partner ErpDimValue '1643'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1643' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1643' - PartnerGroupId set to PartnerTree Code 'UN_VTF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1643' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 119: Partner ErpDimValue=1705
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1705);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1705' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1705'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_HAITI_CHOLERA_MPTF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_HAITI_CHOLERA_MPTF' not found for Partner ErpDimValue '1705'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1705' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1705' - PartnerGroupId set to PartnerTree Code 'UN_HAITI_CHOLERA_MPTF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1705' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 120: Partner ErpDimValue=1718
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1718);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1718' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1718'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNTFHS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNTFHS' not found for Partner ErpDimValue '1718'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1718' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1718' - PartnerGroupId set to PartnerTree Code 'UNTFHS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1718' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 121: Partner ErpDimValue=1760
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1760);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1760' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1760'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNITLIFE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITLIFE' not found for Partner ErpDimValue '1760'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1760' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1760' - PartnerGroupId set to PartnerTree Code 'UNITLIFE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1760' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 122: Partner ErpDimValue=1765
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1765);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1765' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1765'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_MPTF_OFFICE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_MPTF_OFFICE' not found for Partner ErpDimValue '1765'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1765' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1765' - PartnerGroupId set to PartnerTree Code 'UN_MPTF_OFFICE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1765' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 123: Partner ErpDimValue=1779
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1779);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1779' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1779'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_SRI_LANKA_SDG_MPTF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_SRI_LANKA_SDG_MPTF' not found for Partner ErpDimValue '1779'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1779' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1779' - PartnerGroupId set to PartnerTree Code 'UN_SRI_LANKA_SDG_MPTF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1779' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 124: Partner ErpDimValue=1941
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1941);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1941' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'ACADEMIC_TRAINING_RESEARC' not found for Partner ErpDimValue '1941'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNISID1");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNISID1' not found for Partner ErpDimValue '1941'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1941' - PartnerCategoryId set to PartnerTree Code 'ACADEMIC_TRAINING_RESEARC'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1941' - PartnerGroupId set to PartnerTree Code 'UNISID1'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1941' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 125: Partner ErpDimValue=1009
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1009);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1009' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1009'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "BINUCA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'BINUCA' not found for Partner ErpDimValue '1009'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1009' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1009' - PartnerGroupId set to PartnerTree Code 'BINUCA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1009' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 126: Partner ErpDimValue=1014
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1014);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1014' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1014'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CEB");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CEB' not found for Partner ErpDimValue '1014'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1014' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1014' - PartnerGroupId set to PartnerTree Code 'CEB'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1014' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 127: Partner ErpDimValue=1058
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1058);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1058' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1058'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MENUB");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MENUB' not found for Partner ErpDimValue '1058'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1058' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1058' - PartnerGroupId set to PartnerTree Code 'MENUB'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1058' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 128: Partner ErpDimValue=1061
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1061);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1061' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1061'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MINURSO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MINURSO' not found for Partner ErpDimValue '1061'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1061' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1061' - PartnerGroupId set to PartnerTree Code 'MINURSO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1061' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 129: Partner ErpDimValue=1062
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1062);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1062' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1062'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MINUSCA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MINUSCA' not found for Partner ErpDimValue '1062'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1062' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1062' - PartnerGroupId set to PartnerTree Code 'MINUSCA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1062' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 130: Partner ErpDimValue=1063
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1063);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1063' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1063'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MINUSMA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MINUSMA' not found for Partner ErpDimValue '1063'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1063' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1063' - PartnerGroupId set to PartnerTree Code 'MINUSMA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1063' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 131: Partner ErpDimValue=1064
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1064);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1064' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1064'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MINUSTAH");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MINUSTAH' not found for Partner ErpDimValue '1064'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1064' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1064' - PartnerGroupId set to PartnerTree Code 'MINUSTAH'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1064' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 132: Partner ErpDimValue=1066
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1066);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1066' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1066'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MONUSCO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MONUSCO' not found for Partner ErpDimValue '1066'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1066' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1066' - PartnerGroupId set to PartnerTree Code 'MONUSCO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1066' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 133: Partner ErpDimValue=1162
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1162);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1162' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1162'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNAKRT");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNAKRT' not found for Partner ErpDimValue '1162'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1162' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1162' - PartnerGroupId set to PartnerTree Code 'UNAKRT'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1162' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 134: Partner ErpDimValue=1163
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1163);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1163' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1163'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNAMA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNAMA' not found for Partner ErpDimValue '1163'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1163' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1163' - PartnerGroupId set to PartnerTree Code 'UNAMA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1163' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 135: Partner ErpDimValue=1164
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1164);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1164' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1164'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNAMI");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNAMI' not found for Partner ErpDimValue '1164'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1164' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1164' - PartnerGroupId set to PartnerTree Code 'UNAMI'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1164' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 136: Partner ErpDimValue=1167
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1167);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1167' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1167'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNFICYP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNFICYP' not found for Partner ErpDimValue '1167'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1167' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1167' - PartnerGroupId set to PartnerTree Code 'UNFICYP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1167' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 137: Partner ErpDimValue=1169
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1169);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1169' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1169'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIFIL");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIFIL' not found for Partner ErpDimValue '1169'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1169' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1169' - PartnerGroupId set to PartnerTree Code 'UNIFIL'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1169' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 138: Partner ErpDimValue=1170
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1170);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1170' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1170'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIPSIL");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIPSIL' not found for Partner ErpDimValue '1170'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1170' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1170' - PartnerGroupId set to PartnerTree Code 'UNIPSIL'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1170' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 139: Partner ErpDimValue=1171
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1171);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1171' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1171'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNISFA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNISFA' not found for Partner ErpDimValue '1171'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1171' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1171' - PartnerGroupId set to PartnerTree Code 'UNISFA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1171' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 140: Partner ErpDimValue=1175
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1175);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1175' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1175'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNMIL");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNMIL' not found for Partner ErpDimValue '1175'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1175' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1175' - PartnerGroupId set to PartnerTree Code 'UNMIL'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1175' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 141: Partner ErpDimValue=1176
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1176);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1176' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1176'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNMISS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNMISS' not found for Partner ErpDimValue '1176'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1176' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1176' - PartnerGroupId set to PartnerTree Code 'UNMISS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1176' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 142: Partner ErpDimValue=1177
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1177);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1177' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1177'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNMIT");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNMIT' not found for Partner ErpDimValue '1177'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1177' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1177' - PartnerGroupId set to PartnerTree Code 'UNMIT'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1177' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 143: Partner ErpDimValue=1178
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1178);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1178' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1178'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNMOGIP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNMOGIP' not found for Partner ErpDimValue '1178'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1178' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1178' - PartnerGroupId set to PartnerTree Code 'UNMOGIP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1178' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 144: Partner ErpDimValue=1179
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1179);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1179' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1179'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOAU");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOAU' not found for Partner ErpDimValue '1179'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1179' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1179' - PartnerGroupId set to PartnerTree Code 'UNOAU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1179' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 145: Partner ErpDimValue=1180
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1180);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1180' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1180'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOCA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOCA' not found for Partner ErpDimValue '1180'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1180' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1180' - PartnerGroupId set to PartnerTree Code 'UNOCA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1180' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 146: Partner ErpDimValue=1181
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1181);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1181' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1181'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOCI");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOCI' not found for Partner ErpDimValue '1181'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1181' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1181' - PartnerGroupId set to PartnerTree Code 'UNOCI'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1181' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 147: Partner ErpDimValue=1182
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1182);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1182' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1182'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ITC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ITC' not found for Partner ErpDimValue '1182'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1182' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1182' - PartnerGroupId set to PartnerTree Code 'ITC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1182' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 148: Partner ErpDimValue=1183
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1183);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1183' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1183'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNHCR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNHCR' not found for Partner ErpDimValue '1183'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1183' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1183' - PartnerGroupId set to PartnerTree Code 'UNHCR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1183' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 149: Partner ErpDimValue=1184
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1184);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1184' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1184'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNCDF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNCDF' not found for Partner ErpDimValue '1184'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1184' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1184' - PartnerGroupId set to PartnerTree Code 'UNCDF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1184' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 150: Partner ErpDimValue=1185
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1185);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1185' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1185'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNICEF");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNICEF' not found for Partner ErpDimValue '1185'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1185' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1185' - PartnerGroupId set to PartnerTree Code 'UNICEF'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1185' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 151: Partner ErpDimValue=1186
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1186);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1186' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1186'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNCTAD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNCTAD' not found for Partner ErpDimValue '1186'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1186' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1186' - PartnerGroupId set to PartnerTree Code 'UNCTAD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1186' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 152: Partner ErpDimValue=1192
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1192);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1192' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1192'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNEP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNEP' not found for Partner ErpDimValue '1192'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1192' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1192' - PartnerGroupId set to PartnerTree Code 'UNEP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1192' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 153: Partner ErpDimValue=1193
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1193);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1193' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1193'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN-HABITAT");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN-HABITAT' not found for Partner ErpDimValue '1193'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1193' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1193' - PartnerGroupId set to PartnerTree Code 'UN-HABITAT'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1193' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 154: Partner ErpDimValue=1194
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1194);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1194' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1194'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNODC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNODC' not found for Partner ErpDimValue '1194'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1194' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1194' - PartnerGroupId set to PartnerTree Code 'UNODC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1194' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 155: Partner ErpDimValue=1195
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1195);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1195' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1195'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNFPA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNFPA' not found for Partner ErpDimValue '1195'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1195' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1195' - PartnerGroupId set to PartnerTree Code 'UNFPA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1195' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 156: Partner ErpDimValue=1196
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1196);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1196' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1196'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNRWA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNRWA' not found for Partner ErpDimValue '1196'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1196' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1196' - PartnerGroupId set to PartnerTree Code 'UNRWA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1196' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 157: Partner ErpDimValue=1197
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1197);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1197' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1197'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNV");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNV' not found for Partner ErpDimValue '1197'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1197' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1197' - PartnerGroupId set to PartnerTree Code 'UNV'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1197' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 158: Partner ErpDimValue=1198
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1198);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1198' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1198'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "WFP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'WFP' not found for Partner ErpDimValue '1198'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1198' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1198' - PartnerGroupId set to PartnerTree Code 'WFP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1198' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 159: Partner ErpDimValue=1200
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1200);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1200' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1200'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_DESA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_DESA' not found for Partner ErpDimValue '1200'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1200' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1200' - PartnerGroupId set to PartnerTree Code 'UN_DESA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1200' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 160: Partner ErpDimValue=1202
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1202);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1202' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1202'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_DGACM");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_DGACM' not found for Partner ErpDimValue '1202'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1202' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1202' - PartnerGroupId set to PartnerTree Code 'UN_DGACM'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1202' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 161: Partner ErpDimValue=1203
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1203);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1203' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1203'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_DMSPC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_DMSPC' not found for Partner ErpDimValue '1203'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1203' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1203' - PartnerGroupId set to PartnerTree Code 'UN_DMSPC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1203' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 162: Partner ErpDimValue=1205
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1205);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1205' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1205'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_DGC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_DGC' not found for Partner ErpDimValue '1205'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1205' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1205' - PartnerGroupId set to PartnerTree Code 'UN_DGC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1205' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 163: Partner ErpDimValue=1206
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1206);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1206' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1206'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDSS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDSS' not found for Partner ErpDimValue '1206'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1206' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1206' - PartnerGroupId set to PartnerTree Code 'UNDSS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1206' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 164: Partner ErpDimValue=1207
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1207);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1207' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1207'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_OCHA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_OCHA' not found for Partner ErpDimValue '1207'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1207' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1207' - PartnerGroupId set to PartnerTree Code 'UN_OCHA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1207' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 165: Partner ErpDimValue=1208
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1208);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1208' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1208'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_OHCHR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_OHCHR' not found for Partner ErpDimValue '1208'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1208' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1208' - PartnerGroupId set to PartnerTree Code 'UN_OHCHR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1208' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 166: Partner ErpDimValue=1209
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1209);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1209' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1209'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OIOS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OIOS' not found for Partner ErpDimValue '1209'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1209' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1209' - PartnerGroupId set to PartnerTree Code 'OIOS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1209' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 167: Partner ErpDimValue=1210
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1210);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1210' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1210'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_OLA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_OLA' not found for Partner ErpDimValue '1210'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1210' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1210' - PartnerGroupId set to PartnerTree Code 'UN_OLA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1210' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 168: Partner ErpDimValue=1211
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1211);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1211' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1211'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OSAA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OSAA' not found for Partner ErpDimValue '1211'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1211' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1211' - PartnerGroupId set to PartnerTree Code 'OSAA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1211' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 169: Partner ErpDimValue=1212
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1212);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1212' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1212'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "SRSG_CAAC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'SRSG_CAAC' not found for Partner ErpDimValue '1212'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1212' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1212' - PartnerGroupId set to PartnerTree Code 'SRSG_CAAC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1212' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 170: Partner ErpDimValue=1213
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1213);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1213' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1213'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNODA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNODA' not found for Partner ErpDimValue '1213'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1213' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1213' - PartnerGroupId set to PartnerTree Code 'UNODA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1213' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 171: Partner ErpDimValue=1214
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1214);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1214' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1214'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOG");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOG' not found for Partner ErpDimValue '1214'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1214' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1214' - PartnerGroupId set to PartnerTree Code 'UNOG'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1214' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 172: Partner ErpDimValue=1215
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1215);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1215' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1215'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN-OHRLLS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN-OHRLLS' not found for Partner ErpDimValue '1215'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1215' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1215' - PartnerGroupId set to PartnerTree Code 'UN-OHRLLS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1215' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 173: Partner ErpDimValue=1216
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1216);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1216' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1216'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNON");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNON' not found for Partner ErpDimValue '1216'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1216' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1216' - PartnerGroupId set to PartnerTree Code 'UNON'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1216' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 174: Partner ErpDimValue=1217
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1217);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1217' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1217'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOV");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOV' not found for Partner ErpDimValue '1217'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1217' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1217' - PartnerGroupId set to PartnerTree Code 'UNOV'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1217' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 175: Partner ErpDimValue=1220
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1220);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1220' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1220'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_ICC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_ICC' not found for Partner ErpDimValue '1220'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1220' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1220' - PartnerGroupId set to PartnerTree Code 'UN_ICC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1220' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 176: Partner ErpDimValue=1221
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1221);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1221' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1221'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNAIDS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNAIDS' not found for Partner ErpDimValue '1221'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1221' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1221' - PartnerGroupId set to PartnerTree Code 'UNAIDS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1221' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 177: Partner ErpDimValue=1222
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1222);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1222' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1222'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_WOMEN");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_WOMEN' not found for Partner ErpDimValue '1222'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1222' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1222' - PartnerGroupId set to PartnerTree Code 'UN_WOMEN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1222' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 178: Partner ErpDimValue=1223
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1223);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1223' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1223'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDRR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDRR' not found for Partner ErpDimValue '1223'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1223' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1223' - PartnerGroupId set to PartnerTree Code 'UNDRR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1223' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 179: Partner ErpDimValue=1224
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1224);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1224' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1224'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNSSC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNSSC' not found for Partner ErpDimValue '1224'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1224' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1224' - PartnerGroupId set to PartnerTree Code 'UNSSC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1224' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 180: Partner ErpDimValue=1225
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1225);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1225' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1225'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNU");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNU' not found for Partner ErpDimValue '1225'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1225' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1225' - PartnerGroupId set to PartnerTree Code 'UNU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1225' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 181: Partner ErpDimValue=1227
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1227);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1227' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1227'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_ESCAP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_ESCAP' not found for Partner ErpDimValue '1227'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1227' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1227' - PartnerGroupId set to PartnerTree Code 'UN_ESCAP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1227' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 182: Partner ErpDimValue=1228
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1228);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1228' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1228'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_ESCWA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_ESCWA' not found for Partner ErpDimValue '1228'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1228' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1228' - PartnerGroupId set to PartnerTree Code 'UN_ESCWA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1228' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 183: Partner ErpDimValue=1229
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1229);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1229' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1229'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_ECA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_ECA' not found for Partner ErpDimValue '1229'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1229' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1229' - PartnerGroupId set to PartnerTree Code 'UN_ECA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1229' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 184: Partner ErpDimValue=1230
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1230);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1230' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1230'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_ECLAC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_ECLAC' not found for Partner ErpDimValue '1230'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1230' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1230' - PartnerGroupId set to PartnerTree Code 'UN_ECLAC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1230' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 185: Partner ErpDimValue=1234
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1234);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1234' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1234'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDG");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDG' not found for Partner ErpDimValue '1234'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1234' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1234' - PartnerGroupId set to PartnerTree Code 'UNDG'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1234' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 186: Partner ErpDimValue=1235
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1235);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1235' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1235'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_ECE");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_ECE' not found for Partner ErpDimValue '1235'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1235' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1235' - PartnerGroupId set to PartnerTree Code 'UN_ECE'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1235' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 187: Partner ErpDimValue=1236
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1236);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1236' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1236'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIOGBIS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIOGBIS' not found for Partner ErpDimValue '1236'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1236' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1236' - PartnerGroupId set to PartnerTree Code 'UNIOGBIS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1236' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 188: Partner ErpDimValue=1238
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1238);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1238' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1238'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNSCN");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNSCN' not found for Partner ErpDimValue '1238'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1238' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1238' - PartnerGroupId set to PartnerTree Code 'UNSCN'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1238' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 189: Partner ErpDimValue=1243
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1243);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1243' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1243'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "CRPD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'CRPD' not found for Partner ErpDimValue '1243'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1243' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1243' - PartnerGroupId set to PartnerTree Code 'CRPD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1243' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 190: Partner ErpDimValue=1244
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1244);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1244' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1244'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "FAO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'FAO' not found for Partner ErpDimValue '1244'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1244' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1244' - PartnerGroupId set to PartnerTree Code 'FAO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1244' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 191: Partner ErpDimValue=1245
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1245);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1245' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1245'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IAEA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IAEA' not found for Partner ErpDimValue '1245'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1245' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1245' - PartnerGroupId set to PartnerTree Code 'IAEA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1245' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 192: Partner ErpDimValue=1246
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1246);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1246' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1246'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ICAO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ICAO' not found for Partner ErpDimValue '1246'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1246' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1246' - PartnerGroupId set to PartnerTree Code 'ICAO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1246' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 193: Partner ErpDimValue=1247
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1247);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1247' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1247'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IFAD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IFAD' not found for Partner ErpDimValue '1247'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1247' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1247' - PartnerGroupId set to PartnerTree Code 'IFAD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1247' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 194: Partner ErpDimValue=1248
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1248);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1248' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1248'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ILO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ILO' not found for Partner ErpDimValue '1248'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1248' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1248' - PartnerGroupId set to PartnerTree Code 'ILO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1248' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 195: Partner ErpDimValue=1249
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1249);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1249' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1249'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IMO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IMO' not found for Partner ErpDimValue '1249'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1249' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1249' - PartnerGroupId set to PartnerTree Code 'IMO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1249' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 196: Partner ErpDimValue=1251
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1251);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1251' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1251'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "ITU");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'ITU' not found for Partner ErpDimValue '1251'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1251' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1251' - PartnerGroupId set to PartnerTree Code 'ITU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1251' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 197: Partner ErpDimValue=1252
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1252);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1252' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1252'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OPCW");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OPCW' not found for Partner ErpDimValue '1252'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1252' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1252' - PartnerGroupId set to PartnerTree Code 'OPCW'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1252' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 198: Partner ErpDimValue=1254
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1254);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1254' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1254'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNCCD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNCCD' not found for Partner ErpDimValue '1254'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1254' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1254' - PartnerGroupId set to PartnerTree Code 'UNCCD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1254' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 199: Partner ErpDimValue=1256
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1256);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1256' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1256'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNESCO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNESCO' not found for Partner ErpDimValue '1256'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1256' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1256' - PartnerGroupId set to PartnerTree Code 'UNESCO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1256' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 200: Partner ErpDimValue=1257
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1257);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1257' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1257'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNFCCC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNFCCC' not found for Partner ErpDimValue '1257'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1257' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1257' - PartnerGroupId set to PartnerTree Code 'UNFCCC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1257' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 201: Partner ErpDimValue=1259
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1259);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1259' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1259'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIDO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIDO' not found for Partner ErpDimValue '1259'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1259' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1259' - PartnerGroupId set to PartnerTree Code 'UNIDO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1259' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 202: Partner ErpDimValue=1260
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1260);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1260' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1260'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UPU");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UPU' not found for Partner ErpDimValue '1260'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1260' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1260' - PartnerGroupId set to PartnerTree Code 'UPU'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1260' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 203: Partner ErpDimValue=1262
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1262);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1262' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1262'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "WIPO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'WIPO' not found for Partner ErpDimValue '1262'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1262' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1262' - PartnerGroupId set to PartnerTree Code 'WIPO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1262' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 204: Partner ErpDimValue=1263
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1263);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1263' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1263'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "WMO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'WMO' not found for Partner ErpDimValue '1263'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1263' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1263' - PartnerGroupId set to PartnerTree Code 'WMO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1263' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 205: Partner ErpDimValue=1264
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1264);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1264' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1264'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNWTO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNWTO' not found for Partner ErpDimValue '1264'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1264' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1264' - PartnerGroupId set to PartnerTree Code 'UNWTO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1264' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 206: Partner ErpDimValue=1265
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1265);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1265' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1265'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "WTO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'WTO' not found for Partner ErpDimValue '1265'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1265' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1265' - PartnerGroupId set to PartnerTree Code 'WTO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1265' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 207: Partner ErpDimValue=1534
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1534);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1534' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1534'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIDIR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIDIR' not found for Partner ErpDimValue '1534'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1534' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1534' - PartnerGroupId set to PartnerTree Code 'UNIDIR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1534' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 208: Partner ErpDimValue=1535
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1535);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1535' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1535'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNITAR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITAR' not found for Partner ErpDimValue '1535'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1535' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1535' - PartnerGroupId set to PartnerTree Code 'UNITAR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1535' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 209: Partner ErpDimValue=1536
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1536);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1536' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1536'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNICRI");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNICRI' not found for Partner ErpDimValue '1536'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1536' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1536' - PartnerGroupId set to PartnerTree Code 'UNICRI'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1536' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 210: Partner ErpDimValue=1537
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1537);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1537' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1537'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNRISD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNRISD' not found for Partner ErpDimValue '1537'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1537' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1537' - PartnerGroupId set to PartnerTree Code 'UNRISD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1537' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 211: Partner ErpDimValue=1542
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1542);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1542' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1542'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOIP");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOIP' not found for Partner ErpDimValue '1542'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1542' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1542' - PartnerGroupId set to PartnerTree Code 'UNOIP'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1542' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 212: Partner ErpDimValue=1543
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1543);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1543' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1543'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNROD");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNROD' not found for Partner ErpDimValue '1543'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1543' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1543' - PartnerGroupId set to PartnerTree Code 'UNROD'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1543' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 213: Partner ErpDimValue=1567
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1567);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1567' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1567'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNMIS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNMIS' not found for Partner ErpDimValue '1567'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1567' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1567' - PartnerGroupId set to PartnerTree Code 'UNMIS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1567' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 214: Partner ErpDimValue=1576
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1576);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1576' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1576'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IOM");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IOM' not found for Partner ErpDimValue '1576'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1576' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1576' - PartnerGroupId set to PartnerTree Code 'IOM'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1576' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 215: Partner ErpDimValue=1590
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1590);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1590' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1590'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNMIK");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNMIK' not found for Partner ErpDimValue '1590'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1590' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1590' - PartnerGroupId set to PartnerTree Code 'UNMIK'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1590' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 216: Partner ErpDimValue=1593
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1593);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1593' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1593'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOCI");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOCI' not found for Partner ErpDimValue '1593'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1593' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1593' - PartnerGroupId set to PartnerTree Code 'UNOCI'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1593' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 217: Partner ErpDimValue=1608
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1608);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1608' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1608'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_UNITED_NATIONS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_UNITED_NATIONS' not found for Partner ErpDimValue '1608'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1608' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1608' - PartnerGroupId set to PartnerTree Code 'UN_UNITED_NATIONS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1608' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 218: Partner ErpDimValue=1629
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1629);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1629' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1629'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIFEM");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIFEM' not found for Partner ErpDimValue '1629'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1629' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1629' - PartnerGroupId set to PartnerTree Code 'UNIFEM'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1629' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 219: Partner ErpDimValue=1630
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1630);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1630' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1630'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNORCID");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNORCID' not found for Partner ErpDimValue '1630'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1630' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1630' - PartnerGroupId set to PartnerTree Code 'UNORCID'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1630' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 220: Partner ErpDimValue=1631
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1631);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1631' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1631'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOWA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOWA' not found for Partner ErpDimValue '1631'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1631' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1631' - PartnerGroupId set to PartnerTree Code 'UNOWA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1631' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 221: Partner ErpDimValue=1633
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1633);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1633' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1633'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNSCEAR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNSCEAR' not found for Partner ErpDimValue '1633'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1633' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1633' - PartnerGroupId set to PartnerTree Code 'UNSCEAR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1633' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 222: Partner ErpDimValue=1636
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1636);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1636' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1636'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNSMIL");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNSMIL' not found for Partner ErpDimValue '1636'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1636' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1636' - PartnerGroupId set to PartnerTree Code 'UNSMIL'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1636' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 223: Partner ErpDimValue=1637
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1637);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1637' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1637'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNSOS");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNSOS' not found for Partner ErpDimValue '1637'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1637' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1637' - PartnerGroupId set to PartnerTree Code 'UNSOS'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1637' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 224: Partner ErpDimValue=1638
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1638);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1638' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1638'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNSOM");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNSOM' not found for Partner ErpDimValue '1638'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1638' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1638' - PartnerGroupId set to PartnerTree Code 'UNSOM'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1638' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 225: Partner ErpDimValue=1639
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1639);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1639' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1639'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNTSO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNTSO' not found for Partner ErpDimValue '1639'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1639' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1639' - PartnerGroupId set to PartnerTree Code 'UNTSO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1639' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 226: Partner ErpDimValue=1685
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1685);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1685' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1685'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "MINUJUSTH");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'MINUJUSTH' not found for Partner ErpDimValue '1685'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1685' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1685' - PartnerGroupId set to PartnerTree Code 'MINUJUSTH'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1685' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 227: Partner ErpDimValue=1725
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1725);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1725' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1725'");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_DCO' not found for Partner ErpDimValue '1725'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1725' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1725' - PartnerGroupId set to PartnerTree Code 'UN_DCO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1725' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 228: Partner ErpDimValue=1758
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1758);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1758' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1758'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNGM");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNGM' not found for Partner ErpDimValue '1758'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1758' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1758' - PartnerGroupId set to PartnerTree Code 'UNGM'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1758' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 229: Partner ErpDimValue=1762
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1762);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1762' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1762'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UN_TBLDC");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UN_TBLDC' not found for Partner ErpDimValue '1762'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1762' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1762' - PartnerGroupId set to PartnerTree Code 'UN_TBLDC'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1762' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 230: Partner ErpDimValue=1764
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1764);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1764' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1764'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNRCO_-_SRI_LANKA");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNRCO_-_SRI_LANKA' not found for Partner ErpDimValue '1764'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1764' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1764' - PartnerGroupId set to PartnerTree Code 'UNRCO_-_SRI_LANKA'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1764' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 231: Partner ErpDimValue=1769
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1769);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1769' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1769'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNOCT");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNOCT' not found for Partner ErpDimValue '1769'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1769' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1769' - PartnerGroupId set to PartnerTree Code 'UNOCT'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1769' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 232: Partner ErpDimValue=1848
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1848);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1848' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1848'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OSGEY");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OSGEY' not found for Partner ErpDimValue '1848'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1848' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1848' - PartnerGroupId set to PartnerTree Code 'OSGEY'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1848' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 233: Partner ErpDimValue=1866
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1866);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1866' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1866'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNIRMCT");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNIRMCT' not found for Partner ErpDimValue '1866'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1866' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1866' - PartnerGroupId set to PartnerTree Code 'UNIRMCT'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1866' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 234: Partner ErpDimValue=1935
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1935);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1935' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '1935'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "UNDP_MPTFO");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNDP_MPTFO' not found for Partner ErpDimValue '1935'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1935' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1935' - PartnerGroupId set to PartnerTree Code 'UNDP_MPTFO'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1935' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 235: Partner ErpDimValue=9012
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 9012);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '9012' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'UNITED_NATIONS' not found for Partner ErpDimValue '9012'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "IPSAS_ACCOUNTING");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'IPSAS_ACCOUNTING' not found for Partner ErpDimValue '9012'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '9012' - PartnerCategoryId set to PartnerTree Code 'UNITED_NATIONS'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '9012' - PartnerGroupId set to PartnerTree Code 'IPSAS_ACCOUNTING'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '9012' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Record 236: Partner ErpDimValue=1581
                {
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.ErpDimValue == 1581);

                    if (partner == null)
                    {
                        Console.WriteLine($"Not Found: Partner with ErpDimValue '1581' does not exist.");
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
                                Console.WriteLine($"Warning: PartnerTree with Code 'PRIVATE_SECTOR' not found for Partner ErpDimValue '1581'");
                            }
                        }

                        // Update PartnerGroupId if null and group code is provided
                        if (partner.PartnerGroupId == null)
                        {
                            var groupTree = await context.PartnerTrees
                                .FirstOrDefaultAsync(pt => pt.Code == "OTHER_PRIVATE_SECTOR");

                            if (groupTree != null)
                            {
                                partner.PartnerGroupId = groupTree.Id;
                                groupUpdated = true;
                            }
                            else
                            {
                                Console.WriteLine($"Warning: PartnerTree with Code 'OTHER_PRIVATE_SECTOR' not found for Partner ErpDimValue '1581'");
                            }
                        }

                        // Save changes if any updates were made
                        if (categoryUpdated || groupUpdated)
                        {
                            await context.SaveChangesAsync();

                            if (categoryUpdated)
                            {
                                categoryUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1581' - PartnerCategoryId set to PartnerTree Code 'PRIVATE_SECTOR'");
                            }
                            if (groupUpdated)
                            {
                                groupUpdatedCount++;
                                Console.WriteLine($"Updated: Partner '1581' - PartnerGroupId set to PartnerTree Code 'OTHER_PRIVATE_SECTOR'");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: Partner '1581' - PartnerCategoryId and PartnerGroupId already populated or no update values provided.");
                            skippedCount++;
                        }
                    }
                }

                // Commit transaction
                await transaction.CommitAsync();

                Console.WriteLine($"\nPartner Category/Group update completed successfully.");
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
                Console.WriteLine($"Error during Partner Category/Group update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
