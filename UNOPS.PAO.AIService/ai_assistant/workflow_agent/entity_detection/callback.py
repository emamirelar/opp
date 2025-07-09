"""
Enhanced Entity Detection Callback
Based on patterns from UNOPS.PAO.AgenticAi for better tools.json integration
"""

from typing import Optional
from google.adk.agents.callback_context import CallbackContext
from google.genai import types
from ...config_manager import config_manager

# Add Vertex AI imports for Gemini function
import vertexai
from vertexai.generative_models import GenerativeModel, GenerationConfig, HarmBlockThreshold, HarmCategory
import os
import logging

logger = logging.getLogger(__name__)


def extract_entity_intent_before_model(callback_context: CallbackContext, llm_request=None) -> None:
    """
    Enhanced entity detection callback that dynamically injects entity and intent 
    detection configuration from tools.json.
    
    This function follows the UNOPS.PAO.AgenticAi pattern of dynamic instruction injection.
    """
    ctx = callback_context  # Use the correct parameter name
    print(f"🔍 [Callback] extract_entity_intent_before_model triggered for {ctx.agent_name}")
    
    # Only apply to entity detection agent
    if ctx.agent_name != "entity_detection_agent":
        print(f"ℹ️ Skipping callback - not entity detection agent (current: {ctx.agent_name})")
        return
    
    print("🔧 Injecting dynamic entity detection configuration...")
    
    # Use the llm_request parameter if provided, otherwise try to get it from context
    if not llm_request:
        llm_request = getattr(ctx, 'llm_request', None)
    
    if not llm_request:
        print("⚠️ No LLM request found in context")
        return
    
    # Get dynamic entity detection configuration
    detection_config = config_manager.get_entity_detection_config()
    
    # Build enhanced instruction with dynamic configuration
    enhanced_instruction = f"""**🎯 ENHANCED ENTITY DETECTION AGENT**
You are an advanced entity detection agent that analyzes user requests to extract:
1. **Target Entity**: What data type the user wants to work with
2. **Intent**: What action they want to perform  
3. **Parameters**: Specific values mentioned

{detection_config}

**🔍 DETECTION PROCESS:**

**Step 1: Entity Detection**
- Look for mentions of entity names or their synonyms
- Consider context clues (e.g., "contact info", "partner details", "interaction history")
- Default to most relevant entity if multiple possibilities exist

**Step 2: Intent Detection** 
- Analyze action words to determine user intent:
  - "find", "search", "get", "show", "list", "details", "detals" → **search** intent
  - "create", "add", "new" → **create** intent
  - "update", "change", "modify", "edit", "set" → **update** intent (requires ID)
  - "delete", "remove" → **delete** intent (requires ID)

**🚨 CRITICAL INTENT RULES:**
- **"get details"**, **"get detals"**, **"show details"** → ALWAYS **search** intent, never update
- **"find contact X"** → ALWAYS **search** intent to locate the contact
- Only use **update** intent when explicitly changing a field AND have the record ID
- Only use **delete** intent when explicitly removing AND have the record ID

**Step 3: Parameter Extraction**
- Extract specific values mentioned (names, IDs, filters, limits)
- Note any constraints or conditions
- Handle parameter mappings (e.g., "top 5" → "pageSize": 5)

**📊 RESPONSE FORMAT:**
Always return a JSON array with detected entities:

```json
[
  {{
    "entity": "Contact",
    "intent": "search", 
    "extracted_params": {{
      "search": "john smith",
      "pageSize": 5
    }},
    "confidence": 0.95,
    "reasoning": "User wants to search for contacts named john smith, limited to 5 results"
  }}
]
```

**🎯 EXAMPLES:**

**Input:** "Find top 5 contacts"
```json
[{{"entity": "Contact", "intent": "search", "extracted_params": {{"pageSize": 5}}}}]
```

**Input:** "Get detals of conatct Madeline" (with typos)
```json
[{{"entity": "Contact", "intent": "search", "extracted_params": {{"name": "Madeline", "search": "Madeline"}}}}]
```

**Input:** "Create a new partner named Tech Corp"  
```json
[{{"entity": "Partner", "intent": "create", "extracted_params": {{"name": "Tech Corp"}}}}]
```

**Input:** "Update contact John Smith's email to john@newcompany.com"
```json
[{{"entity": "Contact", "intent": "update", "extracted_params": {{"name": "John Smith", "email": "john@newcompany.com"}}}}]
```

**Input:** "Show me all interactions from last month"
```json
[{{"entity": "Interaction", "intent": "search", "extracted_params": {{"timeframe": "last month"}}}}]
```

**⚠️ CRITICAL REQUIREMENTS:**
- ALWAYS return valid JSON array format - no explanations outside JSON
- ALWAYS detect at least one entity per request  
- Use exact entity names from configuration (case-sensitive)
- Include confidence score (0.0 to 1.0)
- Provide brief reasoning for detection decisions

**🔧 PARAMETER MAPPING:**
- "top X", "first X", "limit X" → "pageSize": X
- "search for X", "find X" → "search": "X"  
- Names, emails, IDs → include as-is with appropriate keys
"""

    # Update the LLM request instruction
    if llm_request.config and hasattr(llm_request.config, 'system_instruction'):
        llm_request.config.system_instruction = enhanced_instruction
    else:
        # Create config if it doesn't exist
        if not llm_request.config:
            llm_request.config = types.GenerateContentConfig()
        llm_request.config.system_instruction = enhanced_instruction
    
    print("✅ Dynamic entity detection configuration injected successfully")
    print(f"📊 Loaded {len(config_manager.get_entities())} entities from tools.json")
    
    return None


def dynamic_instruction_callback(callback_context: CallbackContext, llm_request=None) -> str:
    """
    Dynamic instruction callback that builds instruction from tools.json
    """
    ctx = callback_context  # Use the correct parameter name
    print(f"🔧 [Callback] dynamic_instruction_callback for {ctx.agent_name}")
    
    # Get dynamic configuration
    detection_config = config_manager.get_entity_detection_config()
    
    return f"""**🎯 ENTITY DETECTION AGENT**

Analyze the current user message and return JSON with detected entities.

{detection_config}

**🚨 CRITICAL RULES:**
- "get details", "get detals", "show details" → ALWAYS **search** intent
- "find contact X" → ALWAYS **search** intent  
- Only "update" when explicitly changing AND have record ID
- Only "delete" when explicitly removing AND have record ID

**📊 REQUIRED OUTPUT FORMAT:**
Return ONLY a JSON array, no other text or explanations:

```json
[{{"entity": "Contact", "intent": "search", "extracted_params": {{"search": "Madeline"}}, "confidence": 0.95}}]
```

**IMPORTANT:** Analyze the user's current message immediately and respond with the JSON array. Do not wait for further instructions.""" 


def call_gemini_direct(prompt: str, model_name: str = "gemini-1.5-flash", max_tokens: int = 100, temperature: float = 0.7) -> str:
    """
    Make a direct call to Gemini via Vertex AI.
    
    Args:
        prompt (str): The prompt to send to Gemini
        model_name (str): The Gemini model to use (default: "gemini-1.5-flash")
        max_tokens (int): Maximum output tokens (default: 100)
        temperature (float): Temperature for generation (0.0 to 1.0, default: 0.7)
    
    Returns:
        str: The generated response from Gemini
        
    Raises:
        Exception: If there's an error with the Gemini call
    """
    try:
        # Get configuration from config manager
        config = config_manager.framework_config
        google_cloud_config = config.get('google_cloud', {})
        
        # Get project ID and location from config
        project_id = google_cloud_config.get('project', os.getenv("GOOGLE_CLOUD_PROJECT_ID"))
        location = google_cloud_config.get('location', os.getenv("GOOGLE_CLOUD_LOCATION", "us-central1"))

        
        if not project_id:
            raise ValueError("Google Cloud Project ID not found in configuration or environment variables")
        
        logger.info(f"🔧 Initializing Vertex AI for project: {project_id}, location: {location}")
        
        # Initialize Vertex AI
        vertexai.init(project=project_id, location=location)

        safety_settings = {
            HarmCategory.HARM_CATEGORY_HARASSMENT: HarmBlockThreshold.BLOCK_NONE,
            HarmCategory.HARM_CATEGORY_HATE_SPEECH: HarmBlockThreshold.BLOCK_NONE,
            HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT: HarmBlockThreshold.BLOCK_NONE,
            HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT: HarmBlockThreshold.BLOCK_NONE,
        }
        
        # Create generation config
        generation_config = GenerationConfig(
            temperature=temperature,
            max_output_tokens=max_tokens,
            top_p=0.8,
            top_k=40
        )
        
        # Create model and generate content
        model = GenerativeModel(model_name)
        logger.info(f"🤖 Sending prompt to Gemini model: {model_name}")
        
        response = model.generate_content(
            prompt,
            generation_config=generation_config,
            safety_settings=safety_settings
        )

        print(response)
        
        # Extract the generated text
        if response.candidates and response.candidates[0].content.parts:
            generated_text = response.candidates[0].content.parts[0].text.strip()
            logger.info(f"✅ Gemini response generated successfully: {generated_text[:50]}...")
            return generated_text
        else:
            # Handle cases where no valid response was generated
            print(f"🔍 DEBUG: Response object details:")
            print(f"  - Response type: {type(response)}")
            print(f"  - Has candidates: {hasattr(response, 'candidates')}")
            print(f"  - Candidates: {getattr(response, 'candidates', 'N/A')}")
            print(f"  - Response attributes: {dir(response)}")
            
            if response.prompt_feedback and response.prompt_feedback.block_reason:
                error_msg = f"Gemini blocked the response due to: {response.prompt_feedback.block_reason}"
                logger.warning(f"⚠️ {error_msg}")
                print(f"🔍 DEBUG: Prompt feedback details:")
                print(f"  - Block reason: {response.prompt_feedback.block_reason}")
                print(f"  - Safety ratings: {getattr(response.prompt_feedback, 'safety_ratings', 'N/A')}")
                raise Exception(error_msg)
            else:
                error_msg = "No valid response could be generated from Gemini"
                logger.warning(f"⚠️ {error_msg}")
                print(f"🔍 DEBUG: No prompt feedback available")
                print(f"  - Response text: {getattr(response, 'text', 'N/A')}")
                print(f"  - Response content: {getattr(response, 'content', 'N/A')}")
                raise Exception(error_msg)
                
    except Exception as e:
        logger.error(f"❌ Error calling Gemini: {str(e)}")
        raise Exception(f"Gemini API call failed: {str(e)}")


def generate_conversation_title(formatted_conversation: str) -> str:
    """
    Generate a concise title for a conversation using Gemini.
    
    Args:
        formatted_conversation (str): The formatted conversation text
        
    Returns:
        str: A concise title (3-5 words) for the conversation
    """
    try:
        # Create the prompt for title generation
        title_prompt = f"""
Please generate a concise and descriptive title for the following conversation, limited to 3-5 words. The title should capture the core topic or outcome of the interaction. Ensure the title is neutral or positive in tone, avoiding any negative connotations.

{formatted_conversation}

The response should just be the title, no explanations.
"""
        
        # Call Gemini with specific parameters for title generation
        from ...config_manager import config_manager
        gemini_model = config_manager.get_gemini_adhoc_model()
        title = call_gemini_direct(
            prompt=title_prompt,
            model_name=gemini_model,
            max_tokens=20,  # Keep it small for a 3-5 word title
            temperature=0.3  # Lower temperature for more consistent titles
        )
        
        logger.info(f"📝 Generated conversation title: {title}")
        return title
        
    except Exception as e:
        logger.error(f"❌ Error generating conversation title: {str(e)}")
        print(f"🔍 DEBUG: Full exception details:")
        print(f"  - Exception type: {type(e)}")
        print(f"  - Exception message: {str(e)}")
        import traceback
        print(f"  - Full traceback:")
        traceback.print_exc()
        # Return a fallback title if Gemini fails
        return "Conversation" 