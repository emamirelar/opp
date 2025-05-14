from typing import Dict, Any, Optional, List
from ..services.storage_service import GoogleCloudStorageService
from ..services.gemini_service import process_with_gemini, get_gemini_response
from ..services.db_service import retrieve_similarity_results
from ..models.ai_chat import AiChatSession, AiChatHistory
from ..extensions import db
from werkzeug.datastructures import FileStorage
import json
import logging
import re

logger = logging.getLogger(__name__)

class AiService:
    def __init__(self, config):
        """Initialize the AI service with required dependencies."""
        self.storage_service = GoogleCloudStorageService(config)
        self.config = config

    def _parse_gemini_response(self, response: str) -> dict:
        """Parse a response from Gemini into a dictionary.
        
        Args:
            response: Raw response string from Gemini
            
        Returns:
            Parsed dictionary from the response
        """
        print(f"\n=== Parsing Gemini response ===")
        print(f"Raw response: {repr(response)}")
        
        # Clean the response string
        cleaned_response = response.strip()
        if cleaned_response.startswith('```json'):
            cleaned_response = cleaned_response[7:]  # Remove ```json prefix
        if cleaned_response.endswith('```'):
            cleaned_response = cleaned_response[:-3]  # Remove ``` suffix
        cleaned_response = cleaned_response.strip()
        
        print(f"Cleaned response: {repr(cleaned_response)}")
        
        try:
            # Try parsing the cleaned response
            return json.loads(cleaned_response)
        except json.JSONDecodeError as e:
            print(f"JSON parsing failed: {str(e)}")
            # If parsing fails, wrap the text in a basic response
            print("Wrapping in basic response")
            return {
                "Message": response,
                "Type": "general_information",
                "Forward": "No"
            }

    def process_chat(
        self,
        session: AiChatSession,
        message: str,
        extracted_text: Optional[str] = None
    ) -> Dict[str, Any]:
        """
        Process a chat message and return the response.
        
        Args:
            session: The chat session
            message: The user's message
            extracted_text: Optional text extracted from uploaded files
            
        Returns:
            Dict containing the response data including:
            - Message: The model's response
            - Entity: Detected entity type
            - Intent: Detected intent
            - Type: Response type
            - Forward: Whether to forward to another service
            - MediaUrl: URL of any generated media (e.g. audio)
            - MediaType: Type of media generated
            - SearchResults: Results from similarity search if applicable
        """
        try:
            print("\n=== Starting process_chat ===")
            print(f"Input message parameter: {repr(message)}")
            print(f"Input extracted_text parameter: {repr(extracted_text)}")
            print(f"Session ID: {session.Id}")
            print(f"Session TextToSpeech: {session.TextToSpeech}")
            
            if not message and not extracted_text:
                print("WARNING: Both message and extracted_text are empty!")
                raise ValueError("No message content provided - both message and extracted_text are empty")

            # Get chat history for context
            chat_history = (
                db.session.query(AiChatHistory)
                .filter_by(SessionId=session.Id)
                .order_by(AiChatHistory.Id)
                .all()
            )
            print(f"Found {len(chat_history)} chat history items")
            
            # Format chat history for Gemini
            formatted_history = []
            for chat in chat_history:
                formatted_history.append({
                    "role": chat.Sender,
                    "parts": [{"text": chat.Message}]
                })
            print(f"Formatted chat history: {json.dumps(formatted_history, indent=2)}")

            # Use extracted text if available, otherwise use message
            print("\n=== Setting final prompt ===")
            print(f"Current message: {repr(message)}")
            print(f"Current extracted_text: {repr(extracted_text)}")
            final_prompt = extracted_text if extracted_text else message
            print(f"Set final_prompt to: {repr(final_prompt)}")
            
            if not final_prompt:
                print("WARNING: final_prompt is empty!")
                raise ValueError("final_prompt is empty - no content to process")

            # First detect entity and intent
            print("\n=== Detecting entity and intent ===")
            print("Calling process_with_gemini for entity detection...")
            entity_response = process_with_gemini(
                prompt_type="entity_intent_detection",
                prompt_data=final_prompt
            )
            print(f"Raw entity_response: {repr(entity_response)}")
            
            # Parse the entity response
            entity_data = self._parse_gemini_response(entity_response)
            print(f"Parsed entity_data: {json.dumps(entity_data, indent=2)}")
            
            # Get the appropriate prompt type based on entity/intent
            prompt_type = entity_data.get("Type", "general_information")
            print(f"Determined prompt type: {prompt_type}")
            
            # Process with appropriate prompt
            print("\n=== Processing with main prompt ===")
            response = process_with_gemini(
                prompt_type=prompt_type,
                prompt_data=final_prompt,
                additional_replacements={"history": json.dumps(formatted_history)}
            )
            print(f"Initial response: {response}")
            
            # Parse response
            response_data = self._parse_gemini_response(response)
            print(f"Parsed response data: {json.dumps(response_data, indent=2)}")
            
            # Handle retrieval if needed
            if prompt_type.startswith('retrieve_') and response_data.get('SimilarityCriteria'):
                print("\n=== Handling retrieval ===")
                entity_name = prompt_type.replace('retrieve_', '').replace('_information', '')
                print(f"Entity name for retrieval: {entity_name}")
                print(f"Similarity criteria: {response_data['SimilarityCriteria']}")
                
                # Get search results
                search_results = retrieve_similarity_results(
                    entity_name=entity_name,
                    search_text=response_data['SimilarityCriteria']
                )
                print(f"Search results: {json.dumps(search_results, indent=2)}")
                
                # If we found search results, send them to another prompt
                if search_results:
                    print("\n=== Processing search results with final prompt ===")
                    # Prepare data for the next prompt
                    context_data = {
                        "user_query": final_prompt,
                        "search_results": search_results,
                        "chat_history": formatted_history,
                        "similarity_criteria": response_data['SimilarityCriteria'],
                        "entity_type": entity_name
                    }
                    print(f"Context data for final prompt: {json.dumps(context_data, indent=2)}")
                    
                    # Get response using the search results
                    final_response = process_with_gemini(
                        prompt_type=f"{entity_name}_information",
                        prompt_data=json.dumps(context_data)
                    )
                    print(f"Final response: {final_response}")
                    
                    # Update response data with the final response
                    print("\n=== Updating response data ===")
                    print(f"Original response_data: {json.dumps(response_data, indent=2)}")
                    final_response_data = self._parse_gemini_response(final_response)
                    print(f"Final response data before merge: {json.dumps(final_response_data, indent=2)}")
                    
                    # Instead of update, explicitly set fields
                    response_data['Message'] = final_response_data.get('Message', response_data.get('Message', ''))
                    response_data['Entity'] = final_response_data.get('Entity', response_data.get('Entity'))
                    response_data['Intent'] = final_response_data.get('Intent', response_data.get('Intent'))
                    response_data['Type'] = final_response_data.get('Type', response_data.get('Type'))
                    response_data['Summary'] = final_response_data.get('Summary', response_data.get('Summary', ''))
                    response_data['Forward'] = final_response_data.get('Forward', response_data.get('Forward', 'No'))
                    response_data['SearchResults'] = search_results
                    response_data['URL'] = final_response_data.get('URL', response_data.get('URL', ''))
                    
                    print(f"Final merged response_data: {json.dumps(response_data, indent=2)}")
                else:
                    print("No search results found")
                    response_data['Message'] = f"I couldn't find any matching {entity_name} records for '{response_data['SimilarityCriteria']}'"
                    response_data['Forward'] = "No"
            
            # Save user message to history
            print("\n=== Saving chat history ===")
            user_history = AiChatHistory(
                SessionId=session.Id,
                Sender="user",
                Message=message,
                RawMessage=final_prompt,
                EntityType=entity_data.get("Entity"),
                RequestType=entity_data.get("Intent"),
                Type=prompt_type
            )
            db.session.add(user_history)
            print("Saved user message to history")
            
            # Handle text-to-speech if enabled
            media_url = None
            media_type = None
            if session.TextToSpeech and response_data.get("Message"):
                print("\n=== Processing text-to-speech ===")
                from ..services.text_to_speech_service import TextToSpeechService
                tts_service = TextToSpeechService(self.config)
                audio_bytes = tts_service.convert_text_to_audio(response_data["Message"])
                media_url = self.storage_service.upload_audio_to_gcs(audio_bytes)
                media_type = "audio"
                print(f"Generated audio URL: {media_url}")
            
            # Save model response to history
            model_history = AiChatHistory(
                SessionId=session.Id,
                Sender="model",
                Message=response_data.get("Message", ""),
                RawMessage=response,
                EntityType=response_data.get("Entity"),
                RequestType=response_data.get("Intent"),
                Type=prompt_type,
                MediaUrl=media_url,
                MediaType=media_type
            )
            db.session.add(model_history)
            db.session.commit()
            print("Saved model response to history")
            
            # Prepare final response
            final_result = {
                "Message": response_data.get("Message", ""),
                "Entity": response_data.get("Entity"),
                "Intent": response_data.get("Intent"),
                "Type": response_data.get("Type", prompt_type),
                "Summary": response_data.get("Summary", ""),
                "Forward": response_data.get("Forward", "No"),
                "MediaUrl": media_url,
                "MediaType": media_type,
                "Files": [{"MediaUrl": media_url, "MediaType": media_type}] if media_url else [],
                "SearchResults": response_data.get("SearchResults", [])
            }
            print(f"\n=== Final response: {json.dumps(final_result, indent=2)} ===")
            return final_result
            
        except Exception as e:
            print(f"\nERROR in process_chat: {str(e)}")
            logger.error(f"Error in process_chat: {str(e)}", exc_info=True)
            db.session.rollback()
            raise Exception(f"Error processing chat: {str(e)}")

    def extract_text_from_file(self, file: FileStorage) -> str:
        """
        Extract text from an uploaded file.
        
        Args:
            file: The uploaded file
            
        Returns:
            str: The extracted text
        """
        # TODO: Implement file text extraction
        return "Text extracted from file: " + file.filename 