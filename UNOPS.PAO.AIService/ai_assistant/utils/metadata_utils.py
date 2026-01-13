"""
Metadata utilities for loading and formatting entity metadata
"""

import json
import logging
from pathlib import Path

logger = logging.getLogger(__name__)


class MetadataError(Exception):
    """Raised when metadata loading fails"""
    pass


def _find_entities_metadata_file() -> str:
    """
    Find the entities-metadata.json file by checking multiple possible locations.
    
    Returns:
        str: Path to the entities-metadata.json file
        
    Raises:
        MetadataError: If entities-metadata.json file is not found
    """
    current_dir = Path(__file__).parent.absolute()
    
    # Possible locations to check for entities-metadata.json
    possible_paths = [
        # Jenkins deployment: AIService copied into UNOPS.PAO.AIService directory
        current_dir.parent.parent / "AIService" / "metadata" / "entities-metadata.json",  # /app/AIService/metadata/entities-metadata.json
        # Same level as current directory (for deployment scenarios)
        current_dir / "AIService" / "metadata" / "entities-metadata.json",
        # One level up from current directory (development scenario)
        current_dir.parent / "AIService" / "metadata" / "entities-metadata.json",
        # Two levels up (from utils -> ai_assistant -> UNOPS.PAO.AIService -> root)
        current_dir.parent.parent.parent / "AIService" / "metadata" / "entities-metadata.json",
        # Three levels up (alternative structure)
        current_dir.parent.parent.parent.parent / "AIService" / "metadata" / "entities-metadata.json"
    ]
    
    for path in possible_paths:
        if path.exists() and path.is_file():
            logger.debug(f"Found entities-metadata.json at: {path}")
            return str(path)
    
    # If not found, raise an error with helpful information
    searched_paths = [str(p) for p in possible_paths]
    raise MetadataError(
        f"entities-metadata.json file not found. Searched locations:\n" + 
        "\n".join(f"  - {p}" for p in searched_paths)
    )


def load_entities_metadata():
    """Load the entities metadata JSON file"""
    try:
        metadata_path = _find_entities_metadata_file()
        
        with open(metadata_path, 'r', encoding='utf-8') as f:
            metadata = json.load(f)
        
        logger.info(f"Entities metadata loaded successfully from: {metadata_path}")
        return metadata
        
    except MetadataError as e:
        logger.error(f"Metadata file not found: {e}")
        print(f"Warning: {e}")
        return {}
    except json.JSONDecodeError as e:
        logger.error(f"Error parsing entities-metadata.json: {e}")
        print(f"Warning: Error parsing entities-metadata.json: {e}")
        return {}
    except Exception as e:
        logger.error(f"Unexpected error loading entities metadata: {e}")
        print(f"Warning: Unexpected error loading entities metadata: {e}")
        return {}
