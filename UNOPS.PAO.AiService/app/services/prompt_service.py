from typing import Optional, Any
from app.models import AiPrompt
from app.extensions import db
import logging

logger = logging.getLogger(__name__)

def get_prompt_data(prompt_type: str) -> Optional[Any]:
    """
    Get prompt data from the database.
    Args:
        prompt_type: The type of prompt to retrieve
    Returns:
        The prompt data or None if not found
    """
    try:
        prompt = db.session.query(AiPrompt).filter_by(Type=prompt_type).first()
        return prompt
    except Exception as e:
        logger.error(f"Error getting prompt data: {str(e)}")
        db.session.rollback()
        return None

def get_prompt_template(prompt_type: str) -> Optional[str]:
    """
    Get a prompt template by type.
    
    Args:
        prompt_type (str): The type of prompt to retrieve
        
    Returns:
        Optional[str]: The prompt template if found, None otherwise
    """
    try:
        prompt = db.session.query(AiPrompt).filter_by(Type=prompt_type).first()
        return prompt.Prompt if prompt else None
    except Exception as e:
        logger.error(f"Error getting prompt template: {str(e)}")
        db.session.rollback()
        return None 