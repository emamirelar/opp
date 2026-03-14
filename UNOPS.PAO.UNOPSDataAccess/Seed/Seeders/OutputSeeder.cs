using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds the Output table with UNOPS Products and Services List
/// Auto-generated from productsandservices.csv
/// Note: Embedding generation is done separately via OutputEmbeddingSeeder to avoid circular dependencies
/// </summary>
public static class OutputSeeder
{
    public static async Task SeedOutputsAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("📦 Seeding Outputs from static data...");

        try
        {
            var outputsToSeed = GetOutputsToSeed();
            
            Console.WriteLine($"   Processing {outputsToSeed.Count} outputs");

            // Get existing outputs
            var existingOutputs = await context.Set<Output>().ToListAsync();

            // Create a set of unique keys for new outputs
            var newOutputKeys = outputsToSeed
                .Select(o => GetOutputUniqueKey(o))
                .ToHashSet();

            // Process each output
            int insertedCount = 0;
            int updatedCount = 0;

            foreach (var outputData in outputsToSeed)
            {
                var uniqueKey = GetOutputUniqueKey(outputData);
                var existingOutput = existingOutputs.FirstOrDefault(o => GetOutputUniqueKey(o) == uniqueKey);

                if (existingOutput == null)
                {
                    context.Set<Output>().Add(outputData);
                    insertedCount++;
                }
                else
                {
                    bool hasChanges = false;

                    if (existingOutput.Name != outputData.Name) { existingOutput.Name = outputData.Name; hasChanges = true; }
                    if (existingOutput.Level0 != outputData.Level0) { existingOutput.Level0 = outputData.Level0; hasChanges = true; }
                    if (existingOutput.Level1 != outputData.Level1) { existingOutput.Level1 = outputData.Level1; hasChanges = true; }
                    if (existingOutput.DefinitionLevel1 != outputData.DefinitionLevel1) { existingOutput.DefinitionLevel1 = outputData.DefinitionLevel1; hasChanges = true; }
                    if (existingOutput.Level2 != outputData.Level2) { existingOutput.Level2 = outputData.Level2; hasChanges = true; }
                    if (existingOutput.DefinitionLevel2 != outputData.DefinitionLevel2) { existingOutput.DefinitionLevel2 = outputData.DefinitionLevel2; hasChanges = true; }
                    if (existingOutput.Level3 != outputData.Level3) { existingOutput.Level3 = outputData.Level3; hasChanges = true; }
                    if (existingOutput.DefinitionLevel3 != outputData.DefinitionLevel3) { existingOutput.DefinitionLevel3 = outputData.DefinitionLevel3; hasChanges = true; }
                    if (existingOutput.Level4 != outputData.Level4) { existingOutput.Level4 = outputData.Level4; hasChanges = true; }
                    if (existingOutput.DefinitionLevel4 != outputData.DefinitionLevel4) { existingOutput.DefinitionLevel4 = outputData.DefinitionLevel4; hasChanges = true; }
                    if (existingOutput.ServiceLine != outputData.ServiceLine) { existingOutput.ServiceLine = outputData.ServiceLine; hasChanges = true; }
                    if (existingOutput.GrantSupportImplementingModality != outputData.GrantSupportImplementingModality) { existingOutput.GrantSupportImplementingModality = outputData.GrantSupportImplementingModality; hasChanges = true; }
                    if (existingOutput.GrantSupportComponent != outputData.GrantSupportComponent) { existingOutput.GrantSupportComponent = outputData.GrantSupportComponent; hasChanges = true; }
                    if (existingOutput.ProcurementComponent != outputData.ProcurementComponent) { existingOutput.ProcurementComponent = outputData.ProcurementComponent; hasChanges = true; }
                    if (existingOutput.ProcurementInstallationComponent != outputData.ProcurementInstallationComponent) { existingOutput.ProcurementInstallationComponent = outputData.ProcurementInstallationComponent; hasChanges = true; }
                    if (existingOutput.InfrastructureComponent != outputData.InfrastructureComponent) { existingOutput.InfrastructureComponent = outputData.InfrastructureComponent; hasChanges = true; }
                    if (existingOutput.Status != outputData.Status) { existingOutput.Status = outputData.Status; hasChanges = true; }
                    if (existingOutput.IsDeleted) { existingOutput.IsDeleted = false; hasChanges = true; }

                    if (hasChanges) { updatedCount++; }
                }
            }

            // Mark old outputs as deleted (soft delete) if they're no longer in the list
            int deletedCount = 0;
            foreach (var existingOutput in existingOutputs)
            {
                var existingKey = GetOutputUniqueKey(existingOutput);
                if (!string.IsNullOrWhiteSpace(existingKey) && !newOutputKeys.Contains(existingKey) && !existingOutput.IsDeleted)
                {
                    existingOutput.IsDeleted = true;
                    deletedCount++;
                }
            }

            await context.SaveChangesAsync();
            
            Console.WriteLine($"   ✅ Inserted: {insertedCount}, Updated: {updatedCount}, Deleted: {deletedCount}");
            Console.WriteLine($"   Outputs seeding completed - Total active: {outputsToSeed.Count}");
            Console.WriteLine($"   ℹ️  Run OutputEmbeddingSeeder separately to generate embeddings\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error seeding outputs: {ex.Message}");
            Console.WriteLine($"   {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Generate a unique key for an output based on its hierarchical levels
    /// </summary>
    private static string GetOutputUniqueKey(Output output)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(output.Level0)) parts.Add(output.Level0.ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(output.Level1)) parts.Add(output.Level1.ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(output.Level2)) parts.Add(output.Level2.ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(output.Level3)) parts.Add(output.Level3.ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(output.Level4)) parts.Add(output.Level4.ToLowerInvariant());
        
        return string.Join("|", parts);
    }


    private static List<Output> GetOutputsToSeed()
    {
        var outputs = new List<Output>();

        outputs.Add(new Output
        {
            Name = @"Project management-related services",
            Level0 = @"Project management-related services",
            ServiceLine = @"Project Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Programme or Portfolio Management Office",
            Level0 = @"Project management-related services",            Level1 = @"Programme or Portfolio Management Office",
            DefinitionLevel1 = @"A specialized unit providing comprehensive oversight and delivery services for a partner's initiative. It functions as a central hub that integrates two key roles: the strategic ""Office"" function, which provides a centre of excellence, governance frameworks, risk management, and consolidated reporting; and the hands-on ""Team"" function, which manages the direct implementation of projects, including procurement, finance, and daily operations.

Programme and Portfolio exclusive. ",
            ServiceLine = @"Project Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical advisory services - project management",
            Level0 = @"Project management-related services",            Level1 = @"Technical advisory services - project management",
            DefinitionLevel1 = @"The provision of high-level strategic expertise to expand the capacity of partners, specifically targeting their people and systems. This service involves offering expert support, advice, recommendations, or strategic guidance to define the path forward on the conceptual and design phase, diagnosing complex challenges and determining the optimal course of action without UNOPS direct execution.",
            ServiceLine = @"Project Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical assistance services - project management",
            Level0 = @"Project management-related services",            Level2 = @"Technical assistance services - project management",
            DefinitionLevel2 = @"The provision of specialized operational expertise to guide, review, and support the partner's implementation efforts. This service provides technical guidance for complex processes without UNOPS taking over direct execution.",
            ServiceLine = @"Project Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Training services - project management",
            Level0 = @"Project management-related services",            Level3 = @"Training services - project management",
            DefinitionLevel3 = @"A targeted educational intervention designed strictly to enhance the knowledge, skills, and competencies of individual personnel. It is delivered through finite learning events, such as workshops, seminars, or certification courses, aimed at closing immediate skill gaps to empower individuals to perform their roles effectively.",
            ServiceLine = @"Project Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Capacity building - project management",
            Level0 = @"Project management-related services",            Level3 = @"Capacity building - project management",
            DefinitionLevel3 = @"A broad, systemic intervention designed to strengthen institutional infrastructure and the enabling environment. It focuses on advising on and developing organizational assets, such as governance structures, operational systems, and frameworks, that are sustained nationally or institutionally.",
            ServiceLine = @"Project Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Infrastructure-related services",
            Level0 = @"Infrastructure-related services",
            ServiceLine = @"Infrastructure",
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        outputs.Add(new Output
        {
            Name = @"Infrastructure physical assets",
            Level0 = @"Infrastructure-related services",
            Level1 = @"Infrastructure physical assets",
            DefinitionLevel1 = @"Infrastructure-related works implemented on a physical asset (Design / Construction / Rehabilitation / Installations / Others).",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });


        outputs.Add(new Output
        {
            Name = @"Building",
            Level0 = @"Infrastructure-related services",
            Level1 = @"Infrastructure physical assets",            
            Level2 = @"Building",
            DefinitionLevel2 = @"A temporary or permanent structure with a roof and walls designed to enclose space and provide shelter or house activities, such as an office, school, or terminal.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });


        outputs.Add(new Output
        {
            Name = @"Educational facilities",
            Level0 = @"Infrastructure-related services",            
            Level1 = @"Infrastructure physical assets",            
            Level2 = @"Building",            
            Level3 = @"Educational facilities",
            DefinitionLevel3 = @"Facilities designed and equipped to provide spaces for learning, instruction, and related activities, such as schools, universities, libraries, or research labs.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"School (primary / secondary)",
            Level0 = @"Infrastructure-related services",            
            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Educational facilities",            Level4 = @"School (primary / secondary)",
            DefinitionLevel4 = @"Institution for development of a child's education generally from 5 yrs old to 16 yrs old.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Teacher training center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Educational facilities",            Level4 = @"Teacher training center",
            DefinitionLevel4 = @"Facility for the purpose of training teachers for specific educational subjects.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"University",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Educational facilities",            Level4 = @"University",
            DefinitionLevel4 = @"Institution of learning of the highest level, having a program of graduate studies together with several professional schools such as law, medicine, and engineering, and authorized to confer both undergraduate and graduate degrees.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Vocational training center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Educational facilities",            Level4 = @"Vocational training center",
            DefinitionLevel4 = @"Facility for the purpose of training and preparing people for a specific technical trade, career or service industry.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Training center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Educational facilities",            Level4 = @"Training center",
            DefinitionLevel4 = @"Facility for the purpose to provide training in the aim to enhance skills, knowledge and competencies. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Languages laboratory",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Educational facilities",            Level4 = @"Languages laboratory",
            DefinitionLevel4 = @"Facility equipped with technological tools designed to assist in language learning and teaching.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Residential facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Residential facilities",
            DefinitionLevel3 = @"Facilities designed to provide living accommodations and facilities for people, such as dormitories, camps, and kitchens.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Housing and accommodation facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Residential facilities",            Level4 = @"Housing and accommodation facilities",
            DefinitionLevel4 = @"Room or group of related rooms, designed for use as a dwelling. Normally counted singly but may be combined within a single larger building.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Shelter",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Residential facilities",            Level4 = @"Shelter",
            DefinitionLevel4 = @"Collection of self-built shelters or simple planned facilities provided for emergency use.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Emergency center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Residential facilities",            Level4 = @"Emergency center",
            DefinitionLevel4 = @"A temporary facility or location established to provide refuge, safety, and basic necessities for individuals or families who have been displaced or affected by disasters, emergencies, or crises.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Camp",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Residential facilities",            Level4 = @"Camp",
            DefinitionLevel4 = @"Facility intended for short term non-permanent use and may be comprised of tents, containers, rudimentary shelters. It usually contains accommodations, ablutions, administrative and sometimes security facilities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Dining halls",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Residential facilities",            Level4 = @"Dining halls",
            DefinitionLevel4 = @"A designated area within a building or institution where individuals gather to eat meals together.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Kitchen",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Residential facilities",            Level4 = @"Kitchen",
            DefinitionLevel4 = @"A facility that contains equipment and systems for the operation of a kitchen whether it be in a residential, commercial or institutional setting.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Commercial facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Commercial facilities",
            DefinitionLevel3 = @"Facilities designed and used primarily for business activities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Market",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Commercial facilities",            Level4 = @"Market",
            DefinitionLevel4 = @"Open place or a covered building where buyers and sellers convene for the sale of goods.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Retail",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Commercial facilities",            Level4 = @"Retail",
            DefinitionLevel4 = @"A facility that accommodates the systems to support the buying and selling of goods or services to consumers.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Office",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Commercial facilities",            Level4 = @"Office",
            DefinitionLevel4 = @"A designed space within a building or facility where administrative and professional work is conducted.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Fishery",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Commercial facilities",            Level4 = @"Fishery",
            DefinitionLevel4 = @"A facility used in the capture, processing, storage, and distribution of fish and other aquatic organisms.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Institutional facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",
            DefinitionLevel3 = @"Facilities designed to serve a public, social, or governmental function, such as police stations, administrative offices, and detention facilities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Ministries",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Ministries",
            DefinitionLevel4 = @"A governmental facility intended to provide planning, development, management and maintenance of projects and services in a specific sector.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Parliament",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Parliament",
            DefinitionLevel4 = @"A facility that represents the national legislative body for the development, regulation and oversight of policies and laws in a country.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Judiciary facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Judiciary facilities",
            DefinitionLevel4 = @"A facility that supports the functioning of the judicial branch of a country. It is intended to provide the environment for legal proceedings, the administration of justice and the support services required by the judiciary.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Courthouse",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Courthouse",
            DefinitionLevel4 = @"Building in which rule of law is administered in one or more courtrooms. May contain prisoner facilities, offices.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Police station",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Police station",
            DefinitionLevel4 = @"Facility from which police officers are dispatched and to which persons under arrest are brought. May contain staff accommodation and training facilities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Correctional facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Correctional facilities",
            DefinitionLevel4 = @"A facility designed to safely house individuals who have been convicted of crimes and sentenced to serve time in confinement.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Prisons / detention facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Prisons / detention facilities",
            DefinitionLevel4 = @"Building for the confinement of persons held while awaiting trial, persons sentenced after conviction, and associated staff and administration facilities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Custom / border control facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Custom / border control facility",
            DefinitionLevel4 = @"Facility on the border between countries, states, provinces, or in the section of an airport, station, etc., where goods, vehicles and/or people passing through are monitored.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Administrative office",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Administrative office",
            DefinitionLevel4 = @"Facility used by civil servants for the delivery of administrative support to government activity.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Forensic lab",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Institutional facilities",            Level4 = @"Forensic lab",
            DefinitionLevel4 = @"A specialized facility equipped with advanced tools and technologies to support scientific analysis and investigation of crime-related evidence.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Industrial and logistical facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Industrial and logistical facilities",
            DefinitionLevel3 = @"Facilities designed for the manufacturing, processing, storage, and distribution of goods, such as factories, warehouses, and distribution centers.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Warehouse",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Industrial and logistical facilities",            Level4 = @"Warehouse",
            DefinitionLevel4 = @"A facility designed for storage, handling, and distribution of goods and materials.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Manufacturing unit",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Industrial and logistical facilities",            Level4 = @"Manufacturing unit",
            DefinitionLevel4 = @"A facility designed to accommodate activities where raw materials or components are transformed into finished products through various industrial processes. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Hangar",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Industrial and logistical facilities",            Level4 = @"Hangar",
            DefinitionLevel4 = @"A large and enclosed structure designed to provide shelter, maintenance, and storage for for aircraft.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Industrial zone",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Industrial and logistical facilities",            Level4 = @"Industrial zone",
            DefinitionLevel4 = @"An area specifically designated and developed for industrial activities and manufacturing operations. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Logistics hub",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Industrial and logistical facilities",            Level4 = @"Logistics hub",
            DefinitionLevel4 = @"A centralized structure equipped with facilities and infrastructure to efficiently manage the storage, handling, and distribution of goods and commodities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Cargo facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Industrial and logistical facilities",            Level4 = @"Cargo facility",
            DefinitionLevel4 = @"A specialised site designed for the handling, storage, and transportation of goods and commodities. It is intended to facilitate the movement of cargo between different modes of transportation, such as ships, airplanes, trucks, and trains. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Cultural and recreational facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Cultural and recreational facilities",
            DefinitionLevel3 = @"Facilities designed for public leisure, arts, entertainment, or physical activity, such as museums, theaters, and community centers.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Theater",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Cultural and recreational facilities",            Level4 = @"Theater",
            DefinitionLevel4 = @"A facility designed for hosting live performances, concerts, musical events and other cultural events. Its architectural shape and equipment consider the quality of the sonority in the facility. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Museum",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Cultural and recreational facilities",            Level4 = @"Museum",
            DefinitionLevel4 = @"Facility that cares for collections and other objects of artistic, cultural, historical, or scientific importance and makes them available for public viewing.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Sport facility / stadium",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Cultural and recreational facilities",            Level4 = @"Sport facility / stadium",
            DefinitionLevel4 = @"Place or venue for (mostly) outdoor sports and consists of a field or stage either partly or completely surrounded by a tiered structure designed to allow spectators to stand or sit and view the event.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Community center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Cultural and recreational facilities",            Level4 = @"Community center",
            DefinitionLevel4 = @"Building used by members of a community for social gatherings, educational activities, etc.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"UN buildings",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"UN buildings",
            DefinitionLevel3 = @"Facilities designed for the use by the UN.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Non-lethal support facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Non-lethal support facilities",
            DefinitionLevel3 = @"Facilities designed to house the training, administration, and storage functions for military, police, or security forces, distinct from active combat zones.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Military training facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Non-lethal support facilities",            Level4 = @"Military training facility",
            DefinitionLevel4 = @"A specialized site designed to provide realistic training environments for military personnel across various branches and specialities in the aim to improve the preparedness and the capabilities of the soldiers. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Police training facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Non-lethal support facilities",            Level4 = @"Police training facility",
            DefinitionLevel4 = @"Facility in which police can be trained in theoretical and/or practical subjects.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Bunker",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Non-lethal support facilities",            Level4 = @"Bunker",
            DefinitionLevel4 = @"A fortified structure designed to provide protection against threats, such as military attaches, natural disasters, or other emergencies. It is intended to withstand extreme conditions and ensure the safety of the occupants and critical assets. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"FIBUA (fighting in built-up areas) units",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Non-lethal support facilities",            Level4 = @"FIBUA (fighting in built-up areas) units",
            DefinitionLevel4 = @"A specialized training facility designed to simulate urban combat environments.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Shooting range",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Non-lethal support facilities",            Level4 = @"Shooting range",
            DefinitionLevel4 = @"A facility designed for the safe practice and training in the use of firearms.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Ammunition and weapons storage facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Non-lethal support facilities",            Level4 = @"Ammunition and weapons storage facilities",
            DefinitionLevel4 = @"A highly secure facility designed for the safe storage, management and handling of various types of weapons, ammunition, and related materials.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Healthcare facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",
            DefinitionLevel3 = @"Facilities designed and equipped for providing diagnosis, treatment, and medical care to patients.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Hospital",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Hospital",
            DefinitionLevel4 = @"Institution generally divided into multiple departments in which sick or injured persons are given significant medical or surgical treatment.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Clinic",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Clinic",
            DefinitionLevel4 = @"Health care facility that is primarily devoted to the care of ""outpatients"" and short term medical appointments. These may be more comprehensive in situations where hospitals are not available to support health.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Dialysis center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Dialysis center",
            DefinitionLevel4 = @"Facility that provides dialysis treatment to patients with kidney failure or impaired kidney function.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Birth center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Birth center",
            DefinitionLevel4 = @"Facility that provides childbirth assistance and perform surgeries related to delivery.  ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Cancer treatment center / oncology center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Cancer treatment center / oncology center",
            DefinitionLevel4 = @"Facility focused on diagnosing and treating various types of cancer.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Radiology center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Radiology center",
            DefinitionLevel4 = @"Facility focused on diagnosing and treating diseases and injuries using imaging technologies.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Specialist facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Specialist facility",
            DefinitionLevel4 = @"Health care facility which is specialized in certain practices.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Medical laboratory",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Medical laboratory",
            DefinitionLevel4 = @"Facility that provides controlled conditions in which scientific testing, research, experiments may be performed.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Blood bank",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Blood bank",
            DefinitionLevel4 = @"Facility that collects, processes, stores and distributes blood and blood components for transfusions.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Medical warehouse",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Medical warehouse",
            DefinitionLevel4 = @"Specialized building for storage of vaccines, medicines and other types of medical supplies.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Medical gas systems",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Medical gas systems",
            DefinitionLevel4 = @"Facility that houses various medical gases such as oxygen, nitrous oxide, medical air, and vacuum to different parts of the hospital/clinic.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Isolation units",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Building",            Level3 = @"Healthcare facilities",            Level4 = @"Isolation units",
            DefinitionLevel4 = @"Facility designed to prevent the spread of infectious diseases by isolating patients who are infected or suspected to be infected.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Sitework and ancillary facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Sitework and ancillary facilities",
            DefinitionLevel2 = @"External works on sites and properties, including landscaping, access routes, perimeter boundaries, and parking.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Parks / garden",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Sitework and ancillary facilities",            Level3 = @"Parks / garden",
            DefinitionLevel3 = @"Area of land for the enjoyment of the public, having facilities for rest and recreation, often owned, set apart, and managed by a city, state, or nation.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Perimeter walls",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Sitework and ancillary facilities",            Level3 = @"Perimeter walls",
            DefinitionLevel3 = @"A boundary wall that encloses / defines the outer edge of a property or specific area.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Pavement",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Sitework and ancillary facilities",            Level3 = @"Pavement",
            DefinitionLevel3 = @"Surface material laid down on an area intended to sustain pedestrian or vehicular traffic.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Pedestrian walkways",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Sitework and ancillary facilities",            Level3 = @"Pedestrian walkways",
            DefinitionLevel3 = @"Area specifically designed for people to walk on, separate from vehicular traffic.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Public lighting",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Sitework and ancillary facilities",            Level3 = @"Public lighting",
            DefinitionLevel3 = @"Elevated source of light on the edge of, or suspended over a road or walkway.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Parking",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Sitework and ancillary facilities",            Level3 = @"Parking",
            DefinitionLevel3 = @"Facility designed to accommodate the parking of vehicles.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Testing Facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Testing Facilities",
            DefinitionLevel2 = @"A specialized facility, such as a laboratory, equipped to conduct experiments, analysis, or performance evaluations on materials, products, or biological samples.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Construction and civil engineering laboratories",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Testing Facilities",            Level3 = @"Construction and civil engineering laboratories",
            DefinitionLevel3 = @"A testing facility used to analyze the properties and performance of building materials, structural components, and soils.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Healthcare and medical laboratories",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Testing Facilities",            Level3 = @"Healthcare and medical laboratories",
            DefinitionLevel3 = @"Specialized facility for the testing and analysis of biological samples or pathogens to diagnose disease, monitor patient health, and support medical treatment.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Manufacturing and industrial laboratories",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Testing Facilities",            Level3 = @"Manufacturing and industrial laboratories",
            DefinitionLevel3 = @"A facility for research, quality control, and process improvement for the development and production of goods.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Environmental laboratories",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Testing Facilities",            Level3 = @"Environmental laboratories",
            DefinitionLevel3 = @"A facility equipped to analyze samples of air, water, soil, and biological matter to detect contaminants, assess pollution levels, and monitor ecological health.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Energy laboratories",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Testing Facilities",            Level3 = @"Energy laboratories",
            DefinitionLevel3 = @"A facility used to research, test, and develop technologies related to power generation, energy conversion, storage, and efficiency.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Food and agricultural laboratories",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Testing Facilities",            Level3 = @"Food and agricultural laboratories",
            DefinitionLevel3 = @"A facility used to analyze food products for safety and quality, and to test soil, crop, and livestock samples to support agricultural production.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Agricultural facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Agricultural facilities",
            DefinitionLevel2 = @"Facilities, such as barns, silos, or refineries, designed for farming operations. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Silo",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Agricultural facilities",            Level3 = @"Silo",
            DefinitionLevel3 = @"A structure designed for the bulk storage of flowable materials, such as grain, animal feed, or industrial powders.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Refinery",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Agricultural facilities",            Level3 = @"Refinery",
            DefinitionLevel3 = @"A facility designed to process raw crops into more refined products such as sugar, biofuels, or edible oils.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Farm",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Agricultural facilities",            Level3 = @"Farm",
            DefinitionLevel3 = @"Land used primarily for growing crops or other agricultural activities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Agricultural administrative building",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Agricultural facilities",            Level3 = @"Agricultural administrative building",
            DefinitionLevel3 = @"Facility used for managing the business operations, planning, and record-keeping associated with a farm or agricultural enterprise.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Transportation",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",
            DefinitionLevel2 = @"Physical assets, such as roads, bridges, railways, ports, or airports, built to facilitate the movement of people and goods.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Harbor infrastructure",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",
            DefinitionLevel3 = @"Facilities to support maritime vessel operations, cargo handling, and passenger access.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Wharf / quay",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",            Level4 = @"Wharf / quay",
            DefinitionLevel4 = @"It is a structure on the shore of a harbour or  on the bank of a river/canal where ships ships may dock to load and unload cargo or passengers. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Quay / docking facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",            Level4 = @"Quay / docking facility",
            DefinitionLevel4 = @"Structure which does not qualify as a port, built on the land next to a river, lake, or ocean that is used as a place for boats to stop for loading and unloading freight and passengers.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Commercial port / cargo",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",            Level4 = @"Commercial port / cargo",
            DefinitionLevel4 = @"Large port where ships can dock and transfer commercial or containerised cargo, which has to be loaded and unloaded by significant mechanical equipment.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Fishery port",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",            Level4 = @"Fishery port",
            DefinitionLevel4 = @"Fishing port is a smaller port or harbour for landing, processing and distributing fish. It may be a recreational facility, but it is usually small scale commercial.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Tetrapods",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",            Level4 = @"Tetrapods",
            DefinitionLevel4 = @"A type of concrete structure used to protect shorelines and breakwaters from the erosive force of waves. They have a tetrahedral shape with four legs which helps dissipate the wave energy and reduce the impact of water on coastal areas.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Canal",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",            Level4 = @"Canal",
            DefinitionLevel4 = @"Artificial waterway constructed to allow the passage of boats or ships inland or to convey water for irrigation.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Passenger port",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Harbor infrastructure",            Level4 = @"Passenger port",
            DefinitionLevel4 = @"A facility designed to accommodate and serve passengers travelling by sea using maritime transportation means. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Aviation infrastructure",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",
            DefinitionLevel3 = @"Facilities such as airports, runways, terminals, and air traffic control systems to facilitate aircraft operations.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Landing runway",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Landing runway",
            DefinitionLevel4 = @"Sealed or unsealed surface laid on graded base material specifically prepared for the landing and take-off of aircraft.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Landing strip",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Landing strip",
            DefinitionLevel4 = @"Sealed or unsealed aircraft landing zone normally with basic capacity and minimal facilities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Apron",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Apron",
            DefinitionLevel4 = @"An area of the airfield where aircraft are parked, loaded, unloaded, refueled, and boarded or deboarded by passengers.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Taxiway",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Taxiway",
            DefinitionLevel4 = @"A pathway on an airport surface that connects runways, aprons, terminals and other facilities to allow the aircraft to move between these areas.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"HLS (helicopter landing site)",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"HLS (helicopter landing site)",
            DefinitionLevel4 = @"A designated area where helicopters can safely take off, land, and operate.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Control tower",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Control tower",
            DefinitionLevel4 = @"A tall structure located at an airport that serves as the central command center for air traffic control operations.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Aviation intelligent system",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Aviation intelligent system",
            DefinitionLevel4 = @"Flight traffic control, information and lighting systems designed to provide increased safety for aircraft movement in the air and on the ground.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Airfield lighting",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Airfield lighting",
            DefinitionLevel4 = @"Facilities include runway lights, taxiway lights, approach lights, and other visual aids that help pilots navigate and operate aircraft safely during takeoff, landing, and taxiing, especially during low visibility conditions or at night.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Terminal services and amenities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Aviation infrastructure",            Level4 = @"Terminal services and amenities",
            DefinitionLevel4 = @"A structures, amenities, and services designed to support the operation of an airport and provide for the needs of passengers, airlines, and airport personnel.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Horizontal infrastructure",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",
            DefinitionLevel3 = @"Infrastructure built along or through the land to facilitate transportation, utility distribution, or water management, such as roads, bridges, tunnels, and culverts.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Paved road",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Paved road",
            DefinitionLevel4 = @"Durable sealed surface laid over graded base material for vehicular or foot traffic.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Highways",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Highways",
            DefinitionLevel4 = @"Main road, a significant arterial connection, normally sealed and designed for higher speed and load capacity connecting towns and/or cities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Bridges",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Bridges",
            DefinitionLevel4 = @"Structure which carries vehicle traffic over a river, road, or other obstacle.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Tunnels",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Tunnels",
            DefinitionLevel4 = @"Enclosed passageway for vehicle traffic, which may be dug through the surrounding soil/earth/rock.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Culverts",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Culverts",
            DefinitionLevel4 = @"Discharge channel that transfers water from one side of the road to the other normally connected to roadside drains removing water from the road surface.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Retaining wall",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Retaining wall",
            DefinitionLevel4 = @"A structure built to hold back or retain soil, rock, or other materials in a sloped or uneven area to prevent erosion, control movement, and create usable spaces on sloping terrain.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Roundabout",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Roundabout",
            DefinitionLevel4 = @"A circular intersection or junction where traffic flows continuously in one direction around a central island.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Pedestrian bridge",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Pedestrian bridge",
            DefinitionLevel4 = @"Structure which carries pedestrian traffic over a river, road, or other obstacle.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Unpaved road",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Unpaved road",
            DefinitionLevel4 = @"An unsealed surface laid over graded base material for vehicular or foot traffic.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Road safety barrier / structure",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Transportation",            Level3 = @"Horizontal infrastructure",            Level4 = @"Road safety barrier / structure",
            DefinitionLevel4 = @"Protective barrier, speed bumps or similar that act as protective traffic control/ calming mechanisms.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water supply",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",
            DefinitionLevel2 = @"Physical systems, such as treatment plants, reservoirs, pipelines, and sewer networks, built to supply clean water and manage wastewater.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water supply treatment and distribution facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",
            DefinitionLevel3 = @"Acces to source, purify, and convey water, such as boreholes, aquaducts, irrigation networks, and water treatment plants.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water borehole",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Water borehole",
            DefinitionLevel4 = @"Narrow hole which is drilled significantly deeper compared to a well, and later lined with pipes and supplied with a pumping mechanism, for the purpose of obtaining water from the earth.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Groundwater well",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Groundwater well",
            DefinitionLevel4 = @"Wider hole dug, at times by hand, usually somewhere between 5 to 10 meters deep, for the purpose of obtaining water from the earth.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Aqueduct",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Aqueduct",
            DefinitionLevel4 = @"It is a structure/system that is designed to transport water from a location to another, typically over long distances, to provide reliable water supply for drinking, irrigation, and industrial purposes.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Irrigation system",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Irrigation system",
            DefinitionLevel4 = @"It is a network of structures, channels, pipes, pumps, and control mechanisms designed to deliver water to agricultural field or landscapes for the purpose of crop irrigation.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Surface irrigation",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Surface irrigation",
            DefinitionLevel4 = @"System which enables the surface distribution of water to the land surfaces via channels or spray systems.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Retention pond",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Retention pond",
            DefinitionLevel4 = @"Stormwater management facility providing retention of water runoff for future use in irrigation or water supply.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Head works",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Head works",
            DefinitionLevel4 = @"Apparatus for controlling the flow and direction of water to be diverted for irrigation purposes.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Groundwater recharge",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Groundwater recharge",
            DefinitionLevel4 = @"Structure and/or process established for management of water discharge into subsurface aquifer.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Aquifer management system",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Aquifer management system",
            DefinitionLevel4 = @"Set of plans, structures and/or process established for management of an underground layer of water-bearing permeable rock or unconsolidated materials from which groundwater can be extracted using a well or bore.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water treatment plant",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Water treatment plant",
            DefinitionLevel4 = @"Facility which filters and decontaminates water to an approved standard suitable for an end-use.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water supply pipeline / network",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water supply treatment and distribution facilities",            Level4 = @"Water supply pipeline / network",
            DefinitionLevel4 = @"Secondary system of hydraulic pipework, usually smaller pipes, and control components which enables broad distribution of water to individual premises and facilities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water storage facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water storage facilities",
            DefinitionLevel3 = @"Faciilties designed to impound and hold volumes of water for future supply, irrigation, or flow management.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Reservoir",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water storage facilities",            Level4 = @"Reservoir",
            DefinitionLevel4 = @"Bulk storage structure or significant water retention facility for storing water for the purposes of later distribution.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water tower",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water storage facilities",            Level4 = @"Water tower",
            DefinitionLevel4 = @"A large elevated structure designed to store and distribute potable water to a local community.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Storage tank",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water storage facilities",            Level4 = @"Storage tank",
            DefinitionLevel4 = @"A large container or vessel designed to store liquids, gases, or bulk materials for industrial, commercial, or municipal purposes.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Rain catchment system",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water storage facilities",            Level4 = @"Rain catchment system",
            DefinitionLevel4 = @"System used for the accumulation and deposition of rainwater for reuse on-site, rather than allowing it to run off.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Dam",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Water supply",            Level3 = @"Water storage facilities",            Level4 = @"Dam",
            DefinitionLevel4 = @"Over ground, large scale water storage facility comprising a barrier that impounds water from natural catchment zones, rivers or streams.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Wastewater and sanitation",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",
            DefinitionLevel2 = @"Facilities and systems to collect, convey, and process water contaminated by human activities or stormwater runoff.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Sanitation facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sanitation facilities",
            DefinitionLevel3 = @"Facilities for the safe and hygienic disposal of human waste.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Ablution unit",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sanitation facilities",            Level4 = @"Ablution unit",
            DefinitionLevel4 = @"A facility designed to provide washing and hygiene amenities, typically for public or communal use. It can be found in public restrooms, temporary accommodations, religious buildings, and outdoor events.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Latrine",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sanitation facilities",            Level4 = @"Latrine",
            DefinitionLevel4 = @"A facility within a sanitation system. It can be a communal trench in the earth in a camp, a hole in the ground (pit), or similar non mechanical systems.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Toilet",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sanitation facilities",            Level4 = @"Toilet",
            DefinitionLevel4 = @"A sanitation facility designed for the disposal of human waste. It is an essential component of buildings, residences, and public facilities, providing a hygienic and convenient method for waste management.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Wastewater treatment facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Wastewater treatment facilities",
            DefinitionLevel3 = @"Facility to remove contaminants before the water is safely discharged back into the environment or reclaimed for reuse.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Wastewater treatment plant",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Wastewater treatment facilities",            Level4 = @"Wastewater treatment plant",
            DefinitionLevel4 = @"Facility to treat wastewater so that can be either returned to the water cycle with minimal environmental issues or reused.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Packaged wastewater treatment plant",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Wastewater treatment facilities",            Level4 = @"Packaged wastewater treatment plant",
            DefinitionLevel4 = @"Self-contained smaller scale plant providing a process of treating waste, (e.g. from household sewage). It includes physical, chemical, and biological processes to produce environmentally safe treated waste.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Sewage",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sewage",
            DefinitionLevel3 = @"Infrastructure components, such as pipelines, sewer mains, and septic tanks, built to collect or convey wastewater to a point of treatment or disposal.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Sewer pipeline",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sewage",            Level4 = @"Sewer pipeline",
            DefinitionLevel4 = @"An underground network of pipes designed to collect and convey wastewater, including sewage and stormwater runoff from homes, businesses, and industrial facilities to wastewater treatment plants or disposal points.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Septic tank",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sewage",            Level4 = @"Septic tank",
            DefinitionLevel4 = @"A waterproof underground container designed to treat and dispose of household or small-scale sewage.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Soak pit / infiltration trench",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Sewage",            Level4 = @"Soak pit / infiltration trench",
            DefinitionLevel4 = @"A shallow, excavated pit or trench filled with porous materials such as gravel, crushed stone, or coarse sand. It serves to control surface water runoff and prevent flooding or soil erosion.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Drainage",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Drainage",
            DefinitionLevel3 = @"A system and assets to collect, convey, and manage water or groundwater away from a specific area.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Trench drain / channel drain",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Drainage",            Level4 = @"Trench drain / channel drain",
            DefinitionLevel4 = @"It is a linear drainage system designed to collect and convey surface water runoff, stormwater, or other liquids from paved or impervious surfaces, such as roadways, parking lots, sidewalks, and industrial areas. It consists of a narrow, elongated channel or trough with a grated or slotted cover that allows water to enter while preventing debris, sediment, or other contaminants from clogging the drain.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Side drain / roadside drain",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Drainage",            Level4 = @"Side drain / roadside drain",
            DefinitionLevel4 = @"It is a linear drainage feature typically located along the side of roads, highways, or other transportation corridors. It is designed to collect and convey surface water runoff, stormwater, or other liquids from the roadway and adjacent areas to prevent flooding, erosion and pavement damage. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Catch water drain",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Drainage",            Level4 = @"Catch water drain",
            DefinitionLevel4 = @"A drainage system designed to collect and channel surface water runoff from hillsides, slopes, or elevated terrain to prevent erosion, landslides, and flooding in lower-lying areas. They are used in hilly and mountainous regions to manage the flow of rainwater and reduce soil erosion and sedimentation. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Manhole",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Drainage",            Level4 = @"Manhole",
            DefinitionLevel4 = @"It is a vertical access shaft or chamber constructed underground to provide entry points for personnel and equipment inco sewer, stormwater, or utility networks. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Drain pipeline",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Wastewater and sanitation",            Level3 = @"Drainage",            Level4 = @"Drain pipeline",
            DefinitionLevel4 = @"It is a component of a drainage system designed to collect and convey surface water runoff, stormwater, or wastewater from various sources to a discharge point or treatment facility.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Solid waste management",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",
            DefinitionLevel2 = @"Infrastructure assets and systems to safely collect, transport, process, and dispose of various types of waste.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Waste management",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Waste management",
            DefinitionLevel3 = @"Assets and systems to safely collect, transport, process, and dispose of general, non-hazardous waste.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Waste collection and separation facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Waste management",            Level4 = @"Waste collection and separation facility",
            DefinitionLevel4 = @"System for managing waste streams such as chemicals, oils, electrical goods, paints, paper etc. Associated planning to establish system includes to determine collection methods, capacity of systems, recycling methodologies.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Waste disposal facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Waste management",            Level4 = @"Waste disposal facility",
            DefinitionLevel4 = @"A site designed for the collection, treatment, and disposal of various types of waste generated by residential, commercial, industrial, and institutional sources.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Waste processing facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Waste management",            Level4 = @"Waste processing facility",
            DefinitionLevel4 = @"A site designed to receive, sort, treat, and manage various types of waste materials before disposal, recycling, or reuse. It plays a crucial role in waste management systems, helping to reduce the volume of waste sent to landfills, recover valuable resources, and minimize environmental impacts.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Landfills",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Waste management",            Level4 = @"Landfills",
            DefinitionLevel4 = @"Site for the disposal of waste materials by burial which can also be used for waste management purposes, such as the temporary storage, consolidation and transfer, or processing of waste material (sorting, treatment, or recycling).",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Recycling facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Waste management",            Level4 = @"Recycling facility",
            DefinitionLevel4 = @"Place where different types of sorted waste can be stockpiled and treated for partial or complete re-use.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Composting facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Waste management",            Level4 = @"Composting facility",
            DefinitionLevel4 = @"Facility to stockpile organic matter for decomposition and recycling into fertilizer and soil products.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Hazardous waste management",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Hazardous waste management",
            DefinitionLevel3 = @"Assets and systems to safely collect, transport, process, and dispose of hazardous waste.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Hazardous waste facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Hazardous waste management",            Level4 = @"Hazardous waste facility",
            DefinitionLevel4 = @"A facility designed for the safe storage, treatment, and disposal of hazardous waste materials that pose risks for human health and environment due to their toxic, flammable, reactive, or infectious properties. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Leachate and gas emission system",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Hazardous waste management",            Level4 = @"Leachate and gas emission system",
            DefinitionLevel4 = @"Set of plans, structures and/or process established for management or recycling/ cleansing of contaminated liquid or gas emissions that contain harmful substances that may enter the environment.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Incineration plant",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Solid waste management",            Level3 = @"Hazardous waste management",            Level4 = @"Incineration plant",
            DefinitionLevel4 = @"Facility where waste is destroyed using high temperature furnaces. The heat from the combustion process may be used to generate electricity or provide large scale heating where feasible. Use waste to energy in this instance.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Environment",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",
            DefinitionLevel2 = @"Constructed assets and engineered works to protect natural resources or mitigate human impact on the natural world.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Coastal protection / defensive works",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Coastal protection / defensive works",
            DefinitionLevel3 = @"Barrier built out into the sea, with the explicit purpose of breaking waves, to protect a coast or harbour from the force of waves.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Flood control and drainage facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Flood control and drainage facilities",
            DefinitionLevel3 = @"A system of structures, channels, and mechanisms designed to manage and mitigate the risk of flooding by controlling the flow of water and drainage excess water away from populated areas.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Sedimentation tank / settling tank / clarifier",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Sedimentation tank / settling tank / clarifier",
            DefinitionLevel3 = @"It is a component of wastewater treatment plants designed to remove suspended solids and particles from sewage or industrial wastewater before it is discharged back into the environment or reused for other purposes. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Gauge station",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Gauge station",
            DefinitionLevel3 = @"A facility or a location along a pipeline where measurements and monitoring of the pipeline's operational parameters are conducted to ensure the safe and efficient transportation of the liquids or gases in the pipeline.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Levee / barrier",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Levee / barrier",
            DefinitionLevel3 = @"Embankment built to prevent the overflow of a body of water or contain its encroachment into other areas, e.g. a river.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Riverbank stabilization system",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Riverbank stabilization system",
            DefinitionLevel3 = @"Planned structure and/or process established for stabilization of land alongside a body of water such as a river or canal particularly those affected by erosion.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Landslide recovery",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Landslide recovery",
            DefinitionLevel3 = @"Removal of debris and slip material, stabilisation, remediation and revegetation of affected areas.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Pumping station",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Pumping station",
            DefinitionLevel3 = @"Facility including pumps and equipment for pumping utilized for flood control and drainage purpose.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Surface and sub-surface drainage system (flood control, drainage channel)",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Environment",            Level3 = @"Surface and sub-surface drainage system (flood control, drainage channel)",
            DefinitionLevel3 = @"System which provides removal of subsurface water from within an area or stops encroachment of water from adjacent areas.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Energy / power facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",
            DefinitionLevel2 = @"Infrastructure assets and integrated systems, such as power plants, substations, and transmission lines, built for the generation, conversion, and distribution of electricity or other forms of energy.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Power generation facilities (renewable)",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities (renewable)",
            DefinitionLevel3 = @"Facilities to produce electricity from naturally replenishing resources.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Wind energy farm",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities (renewable)",            Level4 = @"Wind energy farm",
            DefinitionLevel4 = @"Large or small scale facility which produces electrical power through the use of the wind energy source.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Solar energy farm",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities (renewable)",            Level4 = @"Solar energy farm",
            DefinitionLevel4 = @"Large or small scale facility which produces electrical power through the use of the solar energy source.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Hydroelectric power plant",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities (renewable)",            Level4 = @"Hydroelectric power plant",
            DefinitionLevel4 = @"Large or small scale facility which produces electrical power through the use of the gravitational force of falling or flowing water.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Tidal energy plant",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities (renewable)",            Level4 = @"Tidal energy plant",
            DefinitionLevel4 = @"A facility designed to harness the energy of tidal movements to general electricity. This facility is located in coastal areas where there are significant tidal ranges to utilise the kinetic energy from the rising and falling tides to drive turbines and generate electricity.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Mini-grid",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities (renewable)",            Level4 = @"Mini-grid",
            DefinitionLevel4 = @"An independant energy system which produces, stores and distributes electrical power through the use of renewable energy sources, e.g. wind, sunlight, biomass or water to a localised area",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Waste-to-energy facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities (renewable)",            Level4 = @"Waste-to-energy facility",
            DefinitionLevel4 = @"Waste management facility which combusts wastes to produce electricity.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Power generation facilities / power plant",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power generation facilities / power plant",
            DefinitionLevel3 = @"A facility where electricity is generated through the conversion of various energy sources into electrical energy.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Power transmission facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power transmission facilities",
            DefinitionLevel3 = @"Assets to transport bulk electricity over long distances from generation sources to local distribution networks.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"HV power line",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power transmission facilities",            Level4 = @"HV power line",
            DefinitionLevel4 = @"High voltage electrical cables used to transmit electrical energy for long distances.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Transformer / substation",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power transmission facilities",            Level4 = @"Transformer / substation",
            DefinitionLevel4 = @"An electrical device that transfers electrical energy between two or more circuits through electromagnetic induction. Transformers increase or decrease the voltages of alternating current in electric power applications.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Mast for energy transmission",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power transmission facilities",            Level4 = @"Mast for energy transmission",
            DefinitionLevel4 = @"A tall vertical structure used to support overhead power lines or electrical transmission cables.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Power distribution facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power distribution facilities",
            DefinitionLevel3 = @"Assets to deliver electricity from transmission systems to end-users.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Power distribution center / distribution substation",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power distribution facilities",            Level4 = @"Power distribution center / distribution substation",
            DefinitionLevel4 = @"A facility designed to receive high-voltage electricity from transmission lines and distributing it safely, reliably, and efficiently to users (homes, businesses, industries) at lower voltages.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"LV / MV distribution network",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Power distribution facilities",            Level4 = @"LV / MV distribution network",
            DefinitionLevel4 = @"Final stage in the delivery of electric power; it carries electricity from the generation or transmission system to individual consumers.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Oil and gas transmission facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas transmission facilities",
            DefinitionLevel3 = @"Assets to transport and deliver refined petroleum products and natural gas to local networks or final consumers.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Gas pipeline",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas transmission facilities",            Level4 = @"Gas pipeline",
            DefinitionLevel4 = @"It is a long-distance transportation system consisting of pipes or conduits used to convey gases from one location to another.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Oil pipeline",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas transmission facilities",            Level4 = @"Oil pipeline",
            DefinitionLevel4 = @"It is a long-distance transportation system consisting of pipes or conduits used to convey oil from one location to another.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Oil and gas storage facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas storage facilities",
            DefinitionLevel3 = @"Assets to safely contain and hold reserves of crude oil, natural gas, or refined petroleum products.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"LNG (liquified natural gas) reservoir",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas storage facilities",            Level4 = @"LNG (liquified natural gas) reservoir",
            DefinitionLevel4 = @"A facility designed to store liquified natural gas at very low temperatures and high pressure due to its insulation systems. The reservoir is used to store LNG before its transportation via tankers, ships, trucks or pipelines to end-users or distribution points.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Gas reservoir",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas storage facilities",            Level4 = @"Gas reservoir",
            DefinitionLevel4 = @"An underground facility designed to store natural gas in gaseous form for later use or distribution. It is equipped with compression and injection equipment in addition to monitoring and control systems.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Oil reservoir",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas storage facilities",            Level4 = @"Oil reservoir",
            DefinitionLevel4 = @"A storage facility or tank designed to store crude oil before transportation to refineries or distribution centers. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Fuel reservoir",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas storage facilities",            Level4 = @"Fuel reservoir",
            DefinitionLevel4 = @"A storage facility or tank designed to store various types of fuels, such as gasoline, diesel or jet fuel, for later use or distribution.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Diesel generator support structure",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas storage facilities",            Level4 = @"Diesel generator support structure",
            DefinitionLevel4 = @"Structures intended to house or support medium to small scale diesel engines which produces electrical power for small areas or specific facilities.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Bulk fuel storage facility",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"Energy / power facilities",            Level3 = @"Oil and gas storage facilities",            Level4 = @"Bulk fuel storage facility",
            DefinitionLevel4 = @"Large scale storage tanks in tank farms that hold liquids, compressed gases for the short- or long-term storage of fuel.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"ICT",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",
            DefinitionLevel2 = @"Physical infrastructure and installed systems to enable digital data processing, storage, and transmission.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Internet and telephone facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",            Level3 = @"Internet and telephone facilities",
            DefinitionLevel3 = @"Physical networks to to provide public voice communication and data connectivity.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Optical fiber network",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",            Level3 = @"Internet and telephone facilities",            Level4 = @"Optical fiber network",
            DefinitionLevel4 = @"A telecommunication network composed of optical fibers capable of transmitting data pulses of light. They are used for high-speed data transmission over long distances.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Data center",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",            Level3 = @"Internet and telephone facilities",            Level4 = @"Data center",
            DefinitionLevel4 = @"Facility where ICT infrastructure is located and/or managed.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"ICT systems facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",            Level3 = @"Internet and telephone facilities",            Level4 = @"ICT systems facilities",
            DefinitionLevel4 = @"Refers to an assemblage of communication devices or applications, including, but not limited to: radio, television, cellular phones, computer and network hardware and software or satellite systems. Including associated cabling. The number refers to an entire system, including any sub-components.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Mast for ICT infrastructure",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",            Level3 = @"Internet and telephone facilities",            Level4 = @"Mast for ICT infrastructure",
            DefinitionLevel4 = @"A tall structure designed to support antennas and other equipment for transmitting and receiving radio waves, including cellular, radio, television, and other forms of wireless communication.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"TV and radio facilities",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",            Level3 = @"TV and radio facilities",
            DefinitionLevel3 = @"Assets to produce and transmit television and radio signals.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Mast and antenna",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure physical assets",            Level2 = @"ICT",            Level3 = @"TV and radio facilities",            Level4 = @"Mast and antenna",
            DefinitionLevel4 = @"A tall vertical structure used to support antennas and broadcasting equipment for transmitting TV or radio signals.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Infrastructure Technical Services",
            Level0 = @"Infrastructure-related services",            Level1 = @"Infrastructure Technical Services",
            DefinitionLevel1 = @"Encompasses the comprehensive engineering and technical expertise required to guide infrastructure projects through their full lifecycle, from initial concept to physical realization. This category integrates the critical preparatory phases, assessing viability and site conditions through feasibility and technical studies, with the development of detailed, buildable design packages suitable for procurement.

Beyond planning and design, this service line extends to the execution phase through construction supervision. This ensures that physical works are monitored and controlled on-site to guarantee strict compliance with design specifications, quality standards, schedules, and HSSE requirements, ultimately delivering safe and functional infrastructure assets.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Feasibility studies",
            Level0 = @"Infrastructure-related services",            Level2 = @"Feasibility studies",
            DefinitionLevel2 = @"An early-stage assessment of the viability, the practicality, and the economic justification of a proposed infrastructure project.  ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical studies",
            Level0 = @"Infrastructure-related services",            Level2 = @"Technical studies",
            DefinitionLevel2 = @"A deep investigation into the engineering, environemental, and operational aspects of a project once its general feasibility is established. It may include site investigations, utility mapping, load assessment, traffic modeling, and preliminary engineering caluculations and simulations. It precedes the subsequent design developement. ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Design development",
            Level0 = @"Infrastructure-related services",            Level2 = @"Design development",
            DefinitionLevel2 = @"A design package suitable for procurement that matures the preliminary and conceptual design into detailed, buildable engineering drawings, technical specifications, and bills of quantities.  ",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Construction Supervision",
            Level0 = @"Infrastructure-related services",            Level2 = @"Construction Supervision",
            DefinitionLevel2 = @"The monitoring, control of the implementation of works on sute to ensure the compliance with design, quality, schedule, and HSSE requirements.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical advisory services - infrastructure",
            Level0 = @"Infrastructure-related services",            Level1 = @"Technical advisory services - infrastructure",
            DefinitionLevel1 = @"The provision of high-level strategic expertise to expand the capacity of partners, specifically targeting their people and systems. This service involves offering expert support, advice, recommendations, or strategic guidance to define the path forward on the conceptual and design phase, diagnosing complex challenges and determining the optimal course of action without UNOPS direct execution.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical assistance services - infrastructure",
            Level0 = @"Infrastructure-related services",            Level2 = @"Technical assistance services - infrastructure",
            DefinitionLevel2 = @"The provision of specialized operational expertise to guide, review, and support the partner's implementation efforts. This service provides technical guidance for complex processes without UNOPS taking over direct execution.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Training services - infrastructure",
            Level0 = @"Infrastructure-related services",            Level3 = @"Training services - infrastructure",
            DefinitionLevel3 = @"A targeted educational intervention designed strictly to enhance the knowledge, skills, and competencies of individual personnel. It is delivered through finite learning events, such as workshops, seminars, or certification courses, aimed at closing immediate skill gaps to empower individuals to perform their roles effectively.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Capacity building - infrastructure",
            Level0 = @"Infrastructure-related services",            Level3 = @"Capacity building - infrastructure",
            DefinitionLevel3 = @"A broad, systemic intervention designed to strengthen institutional infrastructure and the enabling environment. It focuses on advising on and developing organizational assets, such as governance structures, operational systems, and frameworks, that are sustained nationally or institutionally.",
            ServiceLine = @"Infrastructure",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Procurement-related services",
            Level0 = @"Procurement-related services",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Goods",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            DefinitionLevel1 = @"Design and implementation of procurement processes that primarily result in the supply and delivery of goods. This includes needs analysis, technical requirement definition, market engagement, solicitation, evaluation, contract award, and management through delivery and acceptance of the goods, as well as, where relevant, associated installation or start-up services.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Accommodation / shelter",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Accommodation / shelter",
            DefinitionLevel2 = @"Prefabricated structures and related items used for shelter and accommodation purposes.",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Prefabricated residential buildings and structures",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Accommodation / shelter",
            Level3 = @"Prefabricated residential buildings and structures",
            DefinitionLevel3 = @"Includes single family residential structures that were pre-constructed at one location and erected at another location. Covers houses, mobile homes, cabins, garages, gazebos and home kitchens (items within UNSPSC class 95141600 - Prefabricated residential buildings and structures).",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Agriculture",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Agriculture",
            DefinitionLevel2 = @"Machinery, equipment and related items used for agriculture and forestry.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Agricultural, forestry and landscape equipment",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Agriculture",
            Level3 = @"Agricultural, forestry and landscape equipment",
            DefinitionLevel3 = @"Includes ploughs, balers and weeders and all other items within UNSPSC family 21100000 - Agricultural and forestry and landscape machinery and equipment.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Construction materials",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Construction materials",
            DefinitionLevel2 = @"Construction materials and supplies.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Construction material supplies (others)",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Construction materials",
            Level3 = @"Construction material supplies (others)",
            DefinitionLevel3 = @"Includes construction materials supplies (all items within UNSPSC segment 30000000 - Structures and Building and Construction and Manufacturing Components and Supplies).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Container and storage",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Container and storage",
            DefinitionLevel2 = @"Containarization and storage items.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Container and storage (with accessories)",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Container and storage",
            Level3 = @"Container and storage (with accessories)",
            DefinitionLevel3 = @"Includes containerization or storage items and their accessories for the purpose of storing or transporting products for future use, such as containers and their scanning accessories (UNSPSC codes: 24111501-141700).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Domestic furnishing and appliances",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Domestic furnishing and appliances",
            DefinitionLevel2 = @"Domestic furnishing and appliances items.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Domestic furnishing and appliances",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Domestic furnishing and appliances",
            Level3 = @"Domestic furnishing and appliances",
            DefinitionLevel3 = @"Includes domestic appliances and electronics (UNSPSC codes: 52000000-161600), domestic furniture (UNSPSC codes: 56100000-1900).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Educational items",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Educational items",
            DefinitionLevel2 = @"Educational items.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Teaching aids, materials and supplies",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Educational items",
            Level3 = @"Teaching aids, materials and supplies",
            DefinitionLevel3 = @"Includes developmental and professional teaching aids and materials and accessories and supplies within UNSPSC family 60100000 - Developmental and professional teaching aids and materials and accessories and supplies.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Explosive threat management",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Explosive threat management",
            DefinitionLevel2 = @"Explosive threat management equipment and supplies.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Explosive threat management",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Explosive threat management",
            Level3 = @"Explosive threat management",
            DefinitionLevel3 = @"Includes explosives threat management specialized equipment, supplies and related products (UNSPSC codes: 46151800-02), (UNSPSC codes: 46221500-13).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Fuel",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Fuel",
            DefinitionLevel2 = @"Combustible materials used to generate heat or power.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Fuel for vehicles or machinery",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Fuel",
            Level3 = @"Fuel for vehicles or machinery",
            DefinitionLevel3 = @"Includes liquid fuel (UNSPSC codes: 15100000-1513).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Health",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            DefinitionLevel2 = @"Prefabricated structures, equipment, tools, machines, accessories, medicines and consumable supplies used in the practice of medicine and related fields.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Medical equipment",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            Level3 = @"Medical equipment",
            DefinitionLevel3 = @"Includes all medical and facilities equipment, and its accesories within UNSPSC segment 42000000 - Medical Equipment and Accessories and Supplies except UNSPSC codes 42190000-809.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Medical facility products",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            Level3 = @"Medical facility products",
            DefinitionLevel3 = @"Includes patient care beds or accessories for general use, wheelchairs and gurneys or scissor lifts (UNSPSC codes 42190000-809).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Medical laboratory and testing equipment",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            Level3 = @"Medical laboratory and testing equipment",
            DefinitionLevel3 = @"Includes medical measuring, observing and testing equipment or other diagnostic kits used in the diagnosis of patients (UNSPSC codes 41115800-6225).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Medical supplies",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            Level3 = @"Medical supplies",
            DefinitionLevel3 = @"Includes mosquito nets (UNSPSC code: 49121508); first aid-, response- and search, rescue kits (UNSPSC codes: 42172001-17); syringes, hypodermic needles and needle protectors (UNSPSC codes: 42142601-20); condoms (UNSPSC code: 53131622) and all other medical supplies not included in the previous UNSPSCs. The count is per unit, not per package of units.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Prefabricated medical buildings and structures",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            Level3 = @"Prefabricated medical buildings and structures",
            DefinitionLevel3 = @"Includes buildings and structures used for medical purposes that were pre-constructed at one location and erected at another location (UNSPSCs 95141900-04).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Vials / tablets / capsules of medicines",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            Level3 = @"Vials / tablets / capsules of medicines",
            DefinitionLevel3 = @"Includes all medicines within UNSPSC segment: 51000000 - Drugs and Pharmaceutical Products.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Health waste management",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Health",
            Level3 = @"Health waste management",
            DefinitionLevel3 = @"Includes health waste incinerators.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Humanitarian",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Humanitarian",
            DefinitionLevel2 = @"Prefabricated emergency structures, items and supplies used in emergency preparedness and response.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Emergency relief items",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Humanitarian",
            Level3 = @"Emergency relief items",
            DefinitionLevel3 = @"Includes  emergency relief items within UNSPSC segment (57000000 - Humanitarian Relief Items, Kits, or Accessories), tents (UNSPSC code: 49121503) and vital supplies as  blankets, foldable mattresses, pillows, bedsheets and plastic mats",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Prefabricated emergency relief buildings and structures",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Humanitarian",
            Level3 = @"Prefabricated emergency relief buildings and structures",
            DefinitionLevel3 = @"Includes structures providing shelter that were pre-constructed at one location and erected at another location. Covers shelters, halls and container units (UNSPSC codes: 95141801-03).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"ICT",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"ICT",
            DefinitionLevel2 = @"Information and communications technology hardware, software and related equipment.",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"ICT equipment and software",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"ICT",
            Level3 = @"ICT equipment and software",
            DefinitionLevel3 = @"Includes all information and telecommunication equipment and software within the UNSPSC segment 43000000 - Information Technology Broadcasting and Telecommunications.",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Industry",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Industry",
            DefinitionLevel2 = @"Machinery, equipment and related items used for industrial processes.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Industrial manufacturing and processing equipment",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Industry",
            Level3 = @"Industrial manufacturing and processing equipment",
            DefinitionLevel3 = @"Includes industrial manufacturing equipment within UNSPSC segment 23000000 - Industrial Manufacturing and Processing Machinery and Accessories.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Motorized land vehicles",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            DefinitionLevel2 = @"Motorised land vehicles, their accessories and components.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Ambulances",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Ambulances",
            DefinitionLevel3 = @"Includes motor vehicles explicitly used for medical/health purposes (UNSPSC codes: 25101703, 25101713, 25101715).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Armored motor vehicles",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Armored motor vehicles",
            DefinitionLevel3 = @"Includes passenger type vehicle equipped with armor reinforced body and frame (UNSPSC code: 25101510).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Cars, wagons, light trucks and SUVs",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Cars, wagons, light trucks and SUVs",
            DefinitionLevel3 = @"Includesroad vehicles (UNSPSC codes: 25101503-04), light trucks, sport utility vehicles (SUV) (UNSPSC code: 25101507).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Construction machinery",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Construction machinery",
            DefinitionLevel3 = @"Includes motor vehicles explicitly used for infrastructure construction purposes (UNSPSC codes: 22000000-102000).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Electric vehicles (cars, buses, motorcycles)",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Electric vehicles (cars, buses, motorcycles)",
            DefinitionLevel3 = @"Includes electric vehices (UNSPSC codes: 25101509, 25101918), and electric buses.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Mobile healthcare center",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Mobile healthcare center",
            DefinitionLevel3 = @"Includes motor vehicles with clinics and/or laboratory facilities which are utilized for direct medical attention to patients (UNSPSC code: 85101508).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Motorized cycles",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Motorized cycles",
            DefinitionLevel3 = @"Includes motorcycles, scooters and mopeds (UNSPSC codes: 25101801-04).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Vehicle spare parts kits",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Motorized land vehicles",
            Level3 = @"Vehicle spare parts kits",
            DefinitionLevel3 = @"Includes spare part kits and accessories for all motorized vehicle types.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Non-medical laboratory and measuring equipment",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Non-medical laboratory and measuring equipment",
            DefinitionLevel2 = @"Laboratory and measuring equipment not used for medical purposes.",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Non-medical laboratory and measuring equipment",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Non-medical laboratory and measuring equipment",
            Level3 = @"Non-medical laboratory and measuring equipment",
            DefinitionLevel3 = @"Includes measuring, observing and testing equipment for non-medical purposes (UNSPSC codes: 41110000-6502 [excluding codes 41115800-6225 for medical measuring]).",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Office",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Office",
            DefinitionLevel2 = @"Furniture, equipment, and machinery for office spaces.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Office furniture and accessories",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Office",
            Level3 = @"Office furniture and accessories",
            DefinitionLevel3 = @"Includes movable articles that are used to make a room or building suitable for working in, such as tables, chairs, desks, cabinets and shelving (UNSPSC: 56101701-19).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Office machines, supplies and accessories",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Office",
            Level3 = @"Office machines, supplies and accessories",
            DefinitionLevel3 = @"Includes photocopiers, dry erase boards or accessories and adding machines (UNSPSC codes: 44101500-904).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Other vehicles",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Other vehicles",
            DefinitionLevel2 = @"All other vehicles not included under motorised land vehicles.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Other vehicles",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Other vehicles",
            Level3 = @"Other vehicles",
            DefinitionLevel3 = @"Includes bicycles (UNSPSC codes: 25161505-07); rescue ships/boats (UNSPSC code: 25111603); and all other vehicles not included under motorized land vehicles.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Power generators",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Power generators",
            DefinitionLevel2 = @"Power generation equipment and accessories.",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Fuel generators",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Power generators",
            Level3 = @"Fuel generators",
            DefinitionLevel3 = @"Includes diesel and petrol generators (UNSPSC codes: 26111601 and 26111614).",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Renewable energy generators",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Power generators",
            Level3 = @"Renewable energy generators",
            DefinitionLevel3 = @"Includes solar generators, hydro-electric generators, tidal wave generators, wind generators and thermal generators (UNSPSC codes: 26111602-03, 26111605, 07-08, 13).",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Other generators",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Power generators",
            Level3 = @"Other generators",
            DefinitionLevel3 = @"Includes hydraulic generators and others.",
            ServiceLine = @"Procurement",
            ProcurementInstallationComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Safety and security",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Safety and security",
            DefinitionLevel2 = @"Safety and security equipment, devices and accessories.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Safety and security equipment and supplies",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Safety and security",
            Level3 = @"Safety and security equipment and supplies",
            DefinitionLevel3 = @"Includes metal detectors (UNSPSC code: 41111903); crowd control equipment, security and control equipment, forensic equipment, explosives control equipment (UNSPSC codes: 46150000-46151802); public safety and control equipment (UNSPSC code: 46160000-1715); security, surveillance and detection equipment (UNSPSC code: 46170000-1701); personal and fire protection equipment and accesories (UNSPSC codes: 46180000-191625); uniforms (UNSPSC codes: 53102700-18).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water and wastewater",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Water and wastewater",
            DefinitionLevel2 = @"Water and wastewater treatment equipment, goods and accessories.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Water and wastewater treatment items",
            Level0 = @"Procurement-related services",            Level1 = @"Goods",
            Level2 = @"Water and wastewater",
            Level3 = @"Water and wastewater treatment items",
            DefinitionLevel3 = @"Includes collection tanks, septic tanks and carbon filtration equipment, water treatment incinerators (UNSPSC codes within family 47100000 - Water and wastewater treatment supply and disposal).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services",
            Level0 = @"Procurement-related services",            Level1 = @"Services",
            DefinitionLevel1 = @"Design and implementation of procurement processes that primarily result in the provision of services. This includes needs analysis, technical requirement definition, market engagement, solicitation, evaluation, contract award and management through the provision and acceptance of the services.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services - explosive threat management",
            Level0 = @"Procurement-related services",            Level2 = @"Services - explosive threat management",
            DefinitionLevel2 = @"Includes explosive threat management services, including survey, demining, explosive ordnance clearance, training and other related activities (UNSPSC class 81102800 - Minefield and demining services and UNSPSCs 81111709, 92111611, 92111612).",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services - engineering",
            Level0 = @"Procurement-related services",            Level2 = @"Services - engineering",
            DefinitionLevel2 = @"Includes engineering related services within UNSPSC segment 81000000 - Engineering and Research and Technology Based Service [excluding UNSPSC family 81110000 - Computer services, and UNSPSC class 81102800 - Minefield and demining services].",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services - health",
            Level0 = @"Procurement-related services",            Level2 = @"Services - health",
            DefinitionLevel2 = @"Includes health services within UNSPSC segment 85000000 - Healthcare Services.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services - ICT",
            Level0 = @"Procurement-related services",            Level2 = @"Services - ICT",
            DefinitionLevel2 = @"Includes services within UNSPSC family 81110000 - Computer services.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services - management consultancy",
            Level0 = @"Procurement-related services",            Level2 = @"Services - management consultancy",
            DefinitionLevel2 = @"Includes management consultancy services within UNSPSC segment 80000000 - Management and Business Professionals and Administrative Services.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services - other",
            Level0 = @"Procurement-related services",            Level2 = @"Services - other",
            DefinitionLevel2 = @"Includes all other services not considered in other output classifications.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Services - safety and security",
            Level0 = @"Procurement-related services",            Level2 = @"Services - safety and security",
            DefinitionLevel2 = @"Includes safety and security services within UNSPSC segment 92000000 - National Defense and Public Order and Security and Safety Services.",
            ServiceLine = @"Procurement",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical advisory services - procurement",
            Level0 = @"Procurement-related services",            Level1 = @"Technical advisory services - procurement",
            DefinitionLevel1 = @"The provision of high-level strategic expertise to expand the capacity of partners, specifically targeting their people and systems. This service involves offering expert support, advice, recommendations, or strategic guidance to define the path forward on the conceptual and design phase, diagnosing complex challenges and determining the optimal course of action without UNOPS direct execution.",
            ServiceLine = @"Procurement",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical assistance services - procurement",
            Level0 = @"Procurement-related services",            Level2 = @"Technical assistance services - procurement",
            DefinitionLevel2 = @"The provision of specialized operational expertise to guide, review, and support the partner's implementation efforts. This service provides technical guidance for complex processes without UNOPS taking over direct execution.",
            ServiceLine = @"Procurement",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Training services - procurement",
            Level0 = @"Procurement-related services",            Level3 = @"Training services - procurement",
            DefinitionLevel3 = @"A targeted educational intervention designed strictly to enhance the knowledge, skills, and competencies of individual personnel. It is delivered through finite learning events, such as workshops, seminars, or certification courses, aimed at closing immediate skill gaps to empower individuals to perform their roles effectively.",
            ServiceLine = @"Procurement",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Capacity building - procurement",
            Level0 = @"Procurement-related services",            Level3 = @"Capacity building - procurement",
            DefinitionLevel3 = @"A broad, systemic intervention designed to strengthen institutional infrastructure and the enabling environment. It focuses on advising on and developing organizational assets, such as governance structures, operational systems, and frameworks, that are sustained nationally or institutionally.",
            ServiceLine = @"Procurement",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Financial management-related services",
            Level0 = @"Financial management-related services",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Financial transfers-related services",
            Level0 = @"Financial management-related services",            Level1 = @"Financial transfers-related services",
            DefinitionLevel1 = @"In the context of UNOPS projects involving Grant Support, Cash and Voucher Assistance (CVA), Cash for Work (CfW), and the utilization of Pay Agents / payment on behalf of (POBO), Financial transfers-related services encompass the specialized processes and controls required to ensure the secure, transparent, and accountable disbursement of funds to Implementing Partners, beneficiaries, or other designated payees.",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Grant support",
            Level0 = @"Financial management-related services",            Level2 = @"Grant support",
            DefinitionLevel2 = @"A UNOPS project activity that is outside the framework of procurement and is undertaken by way of grants, credits or loans to an Implementing Partner (IP). ",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Grant support (under competitive selection)",
            Level0 = @"Financial management-related services",            Level3 = @"Grant support (under competitive selection)",
            DefinitionLevel3 = @"Grant Support under Competitive Process is a selection methodology where UNOPS selects a Grantee through an open or limited competitive process via the distribution of a Call for Proposals. UNOPS is accountable for ensuring fairness, transparency, and equity in the comparison of Substantially Compliant Proposals to maximize grant-funded project outputs. UNOPS is responsible for monitoring the performance of the selected Grantee and reporting any challenges or concerns. UNOPS remains in control of the management of the Grantee.",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Grant support (under preselection)",
            Level0 = @"Financial management-related services",            Level3 = @"Grant support (under preselection)",
            DefinitionLevel3 = @"Preselection is a selection methodology where the Funding Source pre-selects a specific Implementing Partner (IP) via its own applicable internal rules, regulations, and procedures. The Funding Source agrees that UNOPS shall not be accountable or carry any liability for the performance of the pre-selected IP(s).UNOPS is still responsible for monitoring the performance of the IP and reporting any challenges or concerns. UNOPS remains in control of the management of the IP.",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Grant support (under tailored selection - ad hoc)",
            Level0 = @"Financial management-related services",            Level3 = @"Grant support (under tailored selection - ad hoc)",
            DefinitionLevel3 = @"Ad hoc tailored slection methodology is a uniquely designed selection methodology agreed between UNOPS and the funding source. The selection is distinct from the standard competitive and pre-selection methods. UNOPS remains responsible and liable for the selection and performance of the IP(s) under this method.",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Grant support (under exceptions to the competitive process - ad hoc)",
            Level0 = @"Financial management-related services",            Level3 = @"Grant support (under exceptions to the competitive process - ad hoc)",
            DefinitionLevel3 = @"Ad hoc exceptions to the competative process applies when the grantee selection process is conducted without following the standard competitive selection process. UNOPS remains responsible and liable for the selection and performance of the IP(s) under this method.",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Cash and voucher assistance",
            Level0 = @"Financial management-related services",            Level2 = @"Cash and voucher assistance",
            DefinitionLevel2 = @"Within UNOPS projects, Cash and Voucher Assistance (CVA) refers to all activities where cash transfers or vouchers for goods/services are provided directly to beneficiaries (individuals, households, or communities).

This assistance is distinct from aid given to governments or other state actors and excludes other financial aid like microfinance or personal remittances, even if money transfer institutions are used for the delivery.",
            ServiceLine = @"Financial Management",
            InfrastructureComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Cash for work",
            Level0 = @"Financial management-related services",            Level3 = @"Cash for work",
            DefinitionLevel3 = @"Within UNOPS projects, Cash for Work (CfW) is a conditional transfer providing payments (cash, voucher, or electronic) to beneficiaries after they complete designated work.

Participation is often in time-bound cycles, with payments based on time worked (e.g., a daily rate) or outputs (e.g., items produced). Beneficiaries may use these funds without restriction. The work can be unskilled or skilled, often on public assets or community projects, but may also include home-based activities.

This differs from 'cash for training,' where payment is conditional on attending training, not performing work.",
            ServiceLine = @"Financial Management",
            InfrastructureComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Cash for work (under preselection)",
            Level0 = @"Financial management-related services",            Level4 = @"Cash for work (under preselection)",
            DefinitionLevel4 = @"Work days generated via the cash payments provided to beneficiaries which are preselected by the partner on the condition that they undertake designated work other than infrastructure nature. Beneficiary participation is usually restricted to time-bound cycles (e.g., four to six weeks) and payment for work on a cash for work (CfW) project can be made in the form of cash, vouchers (if necessary) or e-transfers. The number of days can be calculated by multiplying the number of beneficiaries by the days of work each of them generated via the cash payments.  ",
            ServiceLine = @"Financial Management",
            InfrastructureComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical advisory services - financial management",
            Level0 = @"Financial management-related services",            Level1 = @"Technical advisory services - financial management",
            DefinitionLevel1 = @"The provision of high-level strategic expertise to expand the capacity of partners, specifically targeting their people and systems. This service involves offering expert support, advice, recommendations, or strategic guidance to define the path forward on the conceptual and design phase, diagnosing complex challenges and determining the optimal course of action without UNOPS direct execution.",
            ServiceLine = @"Financial Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical assistance services - financial management",
            Level0 = @"Financial management-related services",            Level2 = @"Technical assistance services - financial management",
            DefinitionLevel2 = @"The provision of specialized operational expertise to guide, review, and support the partner's implementation efforts. This service provides technical guidance for complex processes without UNOPS taking over direct execution.",
            ServiceLine = @"Financial Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Training services - financial management",
            Level0 = @"Financial management-related services",            Level3 = @"Training services - financial management",
            DefinitionLevel3 = @"A targeted educational intervention designed strictly to enhance the knowledge, skills, and competencies of individual personnel. It is delivered through finite learning events, such as workshops, seminars, or certification courses, aimed at closing immediate skill gaps to empower individuals to perform their roles effectively.",
            ServiceLine = @"Financial Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Capacity building - financial management",
            Level0 = @"Financial management-related services",            Level3 = @"Capacity building - financial management",
            DefinitionLevel3 = @"A broad, systemic intervention designed to strengthen institutional infrastructure and the enabling environment. It focuses on advising on and developing organizational assets, such as governance structures, operational systems, and frameworks, that are sustained nationally or institutionally.",
            ServiceLine = @"Financial Management",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Human resources-related services",
            Level0 = @"Human resources-related services",
            ServiceLine = @"Human Resources",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"HR contract management services",
            Level0 = @"Human resources-related services",            Level1 = @"HR contract management services",
            DefinitionLevel1 = @"HR contract management services cover the end-to-end administration of partner contracts, including benefits, entitlements, and payroll. This also includes managing agreements of services and Memoranda of Understanding (MOUs), supporting partner discussions, drafting and reviewing detailed MOUs, and ensuring proper execution and compliance.",
            ServiceLine = @"Human Resources",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"HR contract management services (under competitive selection)",
            Level0 = @"Human resources-related services",            Level2 = @"HR contract management services (under competitive selection)",
            DefinitionLevel2 = @"Competitive recruitment under UNOPS rules and regulations covering the issuance of contracts, including benefits, entitlements, and payroll.",
            ServiceLine = @"Human Resources",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"HR contract management services (under preselection)",
            Level0 = @"Human resources-related services",            Level2 = @"HR contract management services (under preselection)",
            DefinitionLevel2 = @"HR contract management services cover the issuance of partner contracts, including benefits, entitlements, and payroll following the preselection of the candidate from the partner side.",
            ServiceLine = @"Human Resources",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Contract administration",
            Level0 = @"Human resources-related services",            Level2 = @"Contract administration",
            DefinitionLevel2 = @"Issuance of new contracts, renewals and terminations.",
            ServiceLine = @"Human Resources",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Benefits and entitlements and payroll",
            Level0 = @"Human resources-related services",            Level2 = @"Benefits and entitlements and payroll",
            DefinitionLevel2 = @"Leave management (off line), Insurance management (registration of dependents, claims management).",
            ServiceLine = @"Human Resources",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"HR GA stakeholder management",
            Level0 = @"Human resources-related services",            Level2 = @"HR GA stakeholder management",
            DefinitionLevel2 = @"Strategic progress involves managing stakeholder relationships, advancing new service opportunities, and improving internal capabilities.",
            ServiceLine = @"Human Resources",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical advisory services - HR",
            Level0 = @"Human resources-related services",            Level1 = @"Technical advisory services - HR",
            DefinitionLevel1 = @"The provision of high-level strategic expertise to expand the capacity of partners, specifically targeting their people and systems. This service involves offering expert support, advice, recommendations, or strategic guidance to define the path forward on the conceptual and design phase, diagnosing complex challenges and determining the optimal course of action without UNOPS direct execution.",
            ServiceLine = @"Human Resources",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Technical assistance services - HR",
            Level0 = @"Human resources-related services",            Level2 = @"Technical assistance services - HR",
            DefinitionLevel2 = @"The provision of specialized operational expertise to guide, review, and support the partner's implementation efforts. This service provides technical guidance for complex processes without UNOPS taking over direct execution.",
            ServiceLine = @"Human Resources",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Training services - HR",
            Level0 = @"Human resources-related services",            Level3 = @"Training services - HR",
            DefinitionLevel3 = @"A targeted educational intervention designed strictly to enhance the knowledge, skills, and competencies of individual personnel. It is delivered through finite learning events, such as workshops, seminars, or certification courses, aimed at closing immediate skill gaps to empower individuals to perform their roles effectively.",
            ServiceLine = @"Human Resources",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Capacity building - HR",
            Level0 = @"Human resources-related services",            Level3 = @"Capacity building - HR",
            DefinitionLevel3 = @"A broad, systemic intervention designed to strengthen institutional infrastructure and the enabling environment. It focuses on advising on and developing organizational assets, such as governance structures, operational systems, and frameworks, that are sustained nationally or institutionally.",
            ServiceLine = @"Human Resources",
            GrantSupportImplementingModality = true,
            GrantSupportComponent = true,
            ProcurementComponent = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        // ===== HQ financial transfers-related services (POBO) — SUP-61017 =====

        outputs.Add(new Output
        {
            Name = @"HQ financial transfers-related services",
            Level0 = @"HQ financial transfers-related services",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"In-house bank (HQ exclusive)",
            Level0 = @"HQ financial transfers-related services",            Level1 = @"In-house bank (HQ exclusive)",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        outputs.Add(new Output
        {
            Name = @"Payments on behalf of (POBO) (HQ exclusive)",
            Level0 = @"HQ financial transfers-related services",            Level1 = @"In-house bank (HQ exclusive)",            Level2 = @"Payments on behalf of (POBO) (HQ exclusive)",
            DefinitionLevel2 = @"The execution of payments on behalf of partners based on structured instructions using UNOPS bank accounts.",
            ServiceLine = @"Financial Management",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        return outputs;
    }
}
