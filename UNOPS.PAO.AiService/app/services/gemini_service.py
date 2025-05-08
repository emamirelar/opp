import vertexai
from vertexai.generative_models import GenerativeModel, GenerationConfig
from app.config.settings import PROJECT_ID, LOCATION
import json
from typing import Optional, Dict, Any
import logging
from app.models.ai_prompt import AiPrompt
from app.extensions import db

logger = logging.getLogger(__name__)

def init_vertex_ai():
    """Initialize Vertex AI."""
    vertexai.init(project=PROJECT_ID, location=LOCATION)

def get_gemini_response(prompt: str, prompt_template: AiPrompt) -> str:
    """
    Get response from Gemini model using configuration from AiPrompt.
    
    Args:
        prompt: The prompt to send to the model
        prompt_template: AiPrompt instance containing configuration
    """
    try:
        # Parse configurations from JSON strings
        generation_config = json.loads(prompt_template.GenerationConfig) if prompt_template.GenerationConfig else {}
        content_config = json.loads(prompt_template.ContentConfig) if prompt_template.ContentConfig else {}
        tools_config = json.loads(prompt_template.ToolsConfig) if prompt_template.ToolsConfig else []
        safety_settings = json.loads(prompt_template.SafetySettings) if prompt_template.SafetySettings else []

        # Create generation config
        generation_config = GenerationConfig(**generation_config)
        
        # Initialize Vertex AI with template's project and location
        vertexai.init(
            project=prompt_template.Project,
            location=prompt_template.Location
        )
        
        # Create model instance
        model = GenerativeModel(prompt_template.Model)

        # Create content structure
        if isinstance(content_config, dict):
            # If it's a single content object, create a list with one item
            content = [{
                "role": content_config.get("role", "user"),
                "parts": [{"text": prompt}]
            }]
        elif isinstance(content_config, list):
            # If it's a list, update the last item or append new one
            content = content_config
            content[-1]["parts"].append({"text": prompt})
        else:
            # Default content structure
            content = [{
                "role": "user",
                "parts": [{"text": prompt}]
            }]

        # Generate response using all configurations
        response = model.generate_content(
            contents=content,
            generation_config=generation_config,
            tools=tools_config if tools_config else None,
            safety_settings=safety_settings if safety_settings else None
        )
        return response.text
        
    except Exception as e:
        logger.error(f"Error getting Gemini response: {str(e)}")
        raise

def process_with_gemini(prompt_type: str, prompt_data: Any, additional_replacements: Optional[Dict[str, str]] = None) -> str:
    """
    Generic function to process data with Gemini using a prompt template.
    
    Args:
        prompt_type: Type of prompt template to use
        prompt_data: Data to replace {promptData} in template
        additional_replacements: Optional dict of additional placeholder replacements
        
    Returns:
        Processed response from Gemini
    """
    try:
        # Get the prompt template with configuration
        prompt_template = db.session.query(AiPrompt).filter_by(Type=prompt_type).first()
        if not prompt_template:
            raise ValueError(f"No prompt template found for type: {prompt_type}")

        # Convert prompt_data to string if it's not already
        if isinstance(prompt_data, (dict, list)):
            prompt_data_str = json.dumps(prompt_data)
        else:
            prompt_data_str = str(prompt_data)

        # Replace the promptData placeholder
        final_prompt = prompt_template.Prompt.replace('{promptData}', prompt_data_str)

        # Handle any additional replacements
        if additional_replacements:
            for key, value in additional_replacements.items():
                placeholder = '{' + key + '}'
                final_prompt = final_prompt.replace(placeholder, str(value))

        # Get response from Gemini using template's configurations
        response = get_gemini_response(
            prompt=final_prompt,
            prompt_template=prompt_template
        )
        return response

    except Exception as e:
        logger.error(f"Error in process_with_gemini: {str(e)}")
        raise 