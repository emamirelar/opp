import json
from typing import List, Optional, Any, Dict
import logging
from app.services.gemini_service import process_with_gemini
from app.extensions import db

logger = logging.getLogger(__name__)

class BulkImportService:
    def process_file_analysis(self, batch_data: str, prompt_type: str, user_id: str, entity_name: str) -> dict:
        """
        Process bulk data import by sending entire dataset to Gemini.
        
        Args:
            batch_data: JSON string containing array of records
            prompt_type: Type of prompt to use for processing
            user_id: User ID for the request
            entity_name: Name of the entity being processed
            
        Returns:
            Dictionary containing success status, message, and results
        """
        try:
            # Parse the batch data
            unescaped_json = batch_data.replace('\\"', '"').strip('"')
            data_list = json.loads(unescaped_json)
            
            # Validate data format
            if not isinstance(data_list, list):
                raise ValueError("batch_data must be an array")
            
            # Validate prompt type
            if not prompt_type:
                raise ValueError("prompt_type is required for batch processing")
            
            # Process with Gemini using the generic function
            additional_replacements = {
                'entityName': entity_name,
                'userId': user_id
            }
            
            response = process_with_gemini(
                prompt_type=prompt_type,
                prompt_data=data_list,
                additional_replacements=additional_replacements
            )
            
            try:
                # Parse the response
                parsed_response = json.loads(response)
                return {
                    'success': True,
                    'message': f"Successfully processed {len(data_list)} records",
                    'results': parsed_response
                }
            except json.JSONDecodeError:
                return {
                    'success': True,
                    'message': f"Successfully processed {len(data_list)} records",
                    'results': response
                }

        except ValueError as e:
            error_message = f"Invalid data format or configuration: {str(e)}"
            logger.error(error_message)
            return {
                'success': False,
                'message': error_message,
                'results': None
            }
        except Exception as e:
            error_message = f"Error processing bulk import: {str(e)}"
            logger.error(error_message)
            return {
                'success': False,
                'message': error_message,
                'results': None
            }