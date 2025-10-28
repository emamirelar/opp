DELETE FROM public."AiScreenMapping"
WHERE "Type" = 'partner_risk_profile';
INSERT INTO public."AiScreenMapping" ("Type", "TableName", "ComparisonKey", "RelatedEntity", "RelatedEntityKey", "QueryConditions", "Order", "CreatedAt", "Name", "Status") VALUES 
('partner_risk_profile', 'Partners', 'Id', 'PartnerProjects', 'PartnersId', NULL, 1, NOW(), 'Partners', 1),
('partner_risk_profile', 'PartnerProjects', 'ProjectsId', 'Projects', 'Id', NULL, 2, NOW(), 'Partners', 1);

DELETE FROM public."AiPrompt"
WHERE "Type" = 'partner_risk_profile';
INSERT INTO public."AiPrompt" ("Type", "Prompt", "CreatedAt", "Name", "Status", "GenerationConfig", "ContentConfig", "Project", "Location", "Model") VALUES
('partner_risk_profile', 'I need to determine the risk profile of a partner based on their involvement in one or more projects. I will provide a JSON object containing partner details in the partners array and project details in the projects array. A partner may be involved in multiple projects, linked by the PartnerId field in the projects array matching the Id field in the partners array.

**Risk Factors to Consider:**

  **Financial Risk:** Consider financial risk based on budget overruns (ExpenditureAmount exceeding BudgetAmount), project delays potentially leading to increased costs.
  **Project Execution Risk:** Assess risk related to project delays (StartDate vs. current date, EndDate in the future but project Status is "Delayed"), project status (Cancelled projects are high risk), and project stage (projects in early stages may have higher uncertainty).  Projects with a status of "Cancelled" should be considered high risk.
  **Reputational Risk:** Consider the potential for reputational damage based on project failures or delays.
  **Compliance Risk:** Consider the compliance risk based on DDRequired and NewEngagement fields.

**Desired Output Format:**

**Name:** [Partner Name] (Get this from the partners property from the JSON) (give 2 line breaks)

Provide a Markdown-formatted summary of risk profile of partner, including:

  Overall Risk Assessment (High, Medium, Low).
  A brief explanation of the factors contributing to the risk assessment.
  Specific examples from the provided data that support the assessment (e.g., "Project X has a significant budget overrun, with expenditure exceeding the budget by Y amount").
  Recommendations for mitigating the identified risks.
  Table summarizing the risk with impact and likelihood (add more tab spaces in this table to avoid overwriting of text)

**Constraints/Assumptions:**

  The partner data does not include information on their financial performance beyond the project level. Therefore, financial risk assessment will be limited to project-specific budget overruns.
  If EndDate of a Project is missing, assume it is ongoing and calculate the duration from the StartDate to the current date.
  Prioritize risks from projects with larger budget amounts.
  If DDRequired is "Yes" and NewEngagement is "Not Allowed" this would indicate higher risk for the partner.

Now, here is the JSON data:

{jsonData}

Please provide the generated Markdown summary based on these instructions. Add additional line space after each detail. If any detail that you are instructed to provide is unavailable, do not include that in the response. Do not assume any detail.', NOW(), 'Partners', 1, '{ "temperature": 0.1, "top_p": 0.2, "max_output_tokens": 2048 }', '{ "role": "user", "parts": [ { "text": "{promptData}" } ] }', 'unops-partneropportunity', 'europe-west3', 'gemini-1.5-flash-001');