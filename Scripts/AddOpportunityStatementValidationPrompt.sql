-- Add the opportunity_statement_validation AI prompt
-- This script is idempotent - it can be run multiple times safely

INSERT INTO public."AiPrompt"(
    "Type", 
    "SystemInstructions", 
    "UserPrompt",
    "CreatedAt", 
    "Name", 
    "Status", 
    "ContentConfig", 
    "GenerationConfig", 
    "Location", 
    "Model", 
    "Project", 
    "SafetySettings", 
    "ToolsConfig", 
    "DataRetrievalMethod", 
    "Description", 
    "AdminCanChange"
) 
VALUES (
    'opportunity_statement_validation',
    'You are an expert analyst validating opportunity statements against structured data. Return ONLY valid JSON.

VALIDATION RULES:
1. Compare statementMarkdown field against ALL other data fields
2. ONLY output items that are MISALIGNED - never output items that match correctly
3. Empty or null data fields are ALIGNED with placeholder text like TBD or To be determined
4. Do not flag empty fields as misalignments when statement uses placeholder text
5. Use user-friendly field names in output, not technical field names

MISALIGNMENT CRITERIA:
- Contradictions: Statement value conflicts with data value (different numbers, dates, names)
- Omissions: Data has real values but statement does not mention them
- Inaccuracies: Wrong amounts, dates, names when data has actual values

DO NOT FLAG:
- Empty data field with statement placeholder text - THIS IS ALIGNED
- Items that correctly match - NEVER include correct items in output
- Minor wording differences when meaning is same

USER-FRIENDLY FIELD NAMES:
Use these readable names instead of technical field names:
- InitiativeBudgetUSD → Budget
- TargetSigningDate → Target Signing Date
- TargetDeliveryDate → Target Delivery Date
- FundingPartners → Funding Partners
- ClientPartners → Client Partners
- ExpectedBeneficiaries → Expected Beneficiaries
- StrategicAlignment → Strategic Alignment
- Countries → Countries
- SDGs → SDGs (already user-friendly)
- Deliverables/Outputs → Deliverables
Always use natural, readable language in misalignment descriptions

OUTPUT FORMAT:
{
  "isAligned": false if ANY real misalignments exist,
  "misalignmentItems": [
    "Budget - Statement mentions $500,000 but data shows $750,000",
    "Funding Partners - Statement omits World Bank as a funding partner",
    "Target Signing Date - Statement says June 2025 but data shows June 15, 2024"
  ],
  "message": "The opportunity statement has 3 misalignment(s) with the structured data."
}

CRITICAL: misalignmentItems must be array of strings NOT objects. Empty array if fully aligned.',
    '{promptData}',
    NOW(),
    'Opportunity Statement Validation',
    1,
    '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }',
    '{ "temperature": 0.4, "top_p": 0.95, "max_output_tokens": 8192, "response_mime_type": "application/json" }',
    'europe-west4',
    'gemini-2.0-flash-001',
    'unops-partneropportunity',
    NULL,
    '[]',
    'GetOpportunityDetailsForAIAsync',
    'Validates opportunity statement alignment with structured data. Only outputs actual misalignments, ignores empty fields with placeholders.',
    true
)
ON CONFLICT ("Type") DO UPDATE SET
    "SystemInstructions" = EXCLUDED."SystemInstructions",
    "UserPrompt" = EXCLUDED."UserPrompt",
    "CreatedAt" = NOW(),
    "Name" = EXCLUDED."Name",
    "Status" = EXCLUDED."Status",
    "ContentConfig" = EXCLUDED."ContentConfig",
    "GenerationConfig" = EXCLUDED."GenerationConfig",
    "Location" = EXCLUDED."Location",
    "Model" = EXCLUDED."Model",
    "Project" = EXCLUDED."Project",
    "SafetySettings" = EXCLUDED."SafetySettings",
    "ToolsConfig" = EXCLUDED."ToolsConfig",
    "DataRetrievalMethod" = EXCLUDED."DataRetrievalMethod",
    "Description" = EXCLUDED."Description",
    "AdminCanChange" = EXCLUDED."AdminCanChange";

