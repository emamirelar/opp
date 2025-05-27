from flask import Blueprint, jsonify, request
from app.services.gemini_service import process_with_gemini
from app.services.db_service import retrieve_similarity_results
from app.services.vertex_service import get_text_embedding
from app.models import AiPrompt
from app.services.bulk_import_service import BulkImportService
from app.extensions import db
import json
import re
import uuid
import logging

# Configure logging
logger = logging.getLogger(__name__)

# Create blueprint
api_bp = Blueprint('api', __name__)

@api_bp.route('/')
def index():
    """Root endpoint."""
    return jsonify({"message": "Hello, World!"})

@api_bp.route('/embeddings', methods=['POST'])
def create_embedding():
    """Generate text embeddings using Vertex AI."""
    try:
        data = request.get_json()
        if not data or 'text' not in data:
            return jsonify({'error': 'Text is required'}), 400

        text = data['text']
        
        # Generate embedding
        embedding = get_text_embedding(text)
        
        return jsonify({
            'embedding': embedding,
            'dimension': len(embedding)
        })

    except Exception as e:
        print(f"Error in create_embedding: {str(e)}")
        return jsonify({'error': str(e)}), 500

@api_bp.route('/embeddings', methods=['GET'])
def generate_and_store_embedding():
    """Generate RAG text and store embeddings for an entity."""
    try:
        # Get and validate query parameters
        entity_name = request.args.get('entityName')
        entity_id_str = request.args.get('entityId')
        
        if not entity_name or not entity_id_str:
            return jsonify({
                'error': 'Required parameters missing. Please provide entityName and entityId.'
            }), 400
            
        try:
            entity_id = int(entity_id_str)
        except (ValueError, TypeError):
            return jsonify({
                'error': 'entityId must be a valid integer'
            }), 400

        from app.services.entity_service import (
            generate_rag_text,
            store_entity_embedding,
            format_entity_name
        )

        # Generate RAG text
        rag_text = generate_rag_text(entity_name, entity_id)
        
        # Store the embedding
        store_entity_embedding(format_entity_name(entity_name), entity_id, rag_text)

        # Return success response
        return jsonify({
            'success': True,
            'message': f'Successfully generated and stored embedding for {entity_name} (ID: {entity_id})'
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/rag-text', methods=['GET'])
def get_rag_text():
    """Get RAG text for an entity without storing embeddings."""
    try:
        # Get and validate query parameters
        entity_name = request.args.get('entityName')
        entity_id_str = request.args.get('entityId')
        
        if not entity_name or not entity_id_str:
            return jsonify({
                'error': 'Required parameters missing. Please provide entityName and entityId.'
            }), 400
            
        try:
            entity_id = int(entity_id_str)
        except (ValueError, TypeError):
            return jsonify({
                'error': 'entityId must be a valid integer'
            }), 400

        from app.services.entity_service import generate_rag_text

        # Generate and return RAG text
        rag_text = generate_rag_text(entity_name, entity_id)
        return rag_text

    except Exception as e:
        return jsonify({'error': str(e)}), 500

def validate_session_id(session_id):
    """Validate if the string is a valid UUID."""
    try:
        uuid_obj = uuid.UUID(session_id)
        return str(uuid_obj)
    except ValueError:
        return None

def validate_user_id(user_id):
    """Validate if the value is a valid integer."""
    try:
        return int(user_id)
    except (ValueError, TypeError):
        return None

def process_gemini_response(response_text):
    """Extract and parse JSON from Gemini response."""
    try:
        # Extract JSON from code block if present
        json_match = re.search(r'```json\s*(.*?)\s*```', response_text, re.DOTALL)
        if json_match:
            json_str = json_match.group(1)
        else:
            json_str = response_text

        # Parse the JSON string
        return json.loads(json_str)
    except json.JSONDecodeError:
        raise ValueError('Invalid JSON response from model')

def get_entity_name(type_value):
    """Extract and format entity name from type value."""
    # Extract the entity name (e.g., 'retrieve_contact_information' -> 'contact')
    entity_name = type_value.split('_')[1]
    
    # Capitalize and pluralize
    # Add special cases here if needed for irregular plurals
    if entity_name.endswith('y'):
        entity_name = entity_name[:-1] + 'ies'
    else:
        entity_name = entity_name + 's'
    
    return entity_name.capitalize()

def handle_retrieval(parsed_response):
    """Handle retrieval types with similarity search."""
    print("\n=== Debug: Starting handle_retrieval ===")
    print(f"Received response: {json.dumps(parsed_response, indent=2)}")
    
    type_value = parsed_response.get('Type', '')
    similarity_criteria = parsed_response.get('SimilarityCriteria')
    
    print(f"Type: {type_value}")
    print(f"SimilarityCriteria: {similarity_criteria}")
    
    if type_value.startswith('retrieve_') and similarity_criteria:
        print(f"\nFound retrieval type with similarity criteria")
        
        # Get properly formatted entity name
        entity_name = get_entity_name(type_value)
        print(f"Formatted entity name: {entity_name}")
        
        try:
            # Get similarity results
            print(f"\nCalling retrieve_similarity_results with:")
            print(f"entity_name: {entity_name}")
            print(f"search_text: {similarity_criteria}")
            
            results = retrieve_similarity_results(
                entity_name=entity_name,
                search_text=similarity_criteria
            )
            
            if results:
                print(f"\nFound {len(results)} matching entities")
                parsed_response['SearchResults'] = results
            else:
                print("\nNo matching entities found")
                parsed_response['SearchResults'] = []
            
        except Exception as e:
            print(f"\nError during similarity search: {str(e)}")
            print(f"Error type: {type(e)}")
            print(f"Error details: {e.__dict__}")
            raise
    else:
        print("\nSkipping similarity search - criteria not met")
    
    print("\n=== Debug: Finishing handle_retrieval ===")
    return parsed_response

@api_bp.route("/analyse-file", methods=['POST'])
def process_file_analysis():
    """
    Process a bulk import request.
    """
    try:
        data = request.get_json()
        if not data:
            return jsonify({'error': 'No data provided'}), 400

        service = BulkImportService()
        result = service.process_file_analysis(
            batch_data=data.get('batch_data'),
            prompt_type=data.get('prompt_type'),
            user_id=data.get('user_id'),
            entity_name=data.get('entity_name')
        )
        
        return jsonify(result)
    except Exception as e:
        return jsonify({'error': str(e)}), 500

# Chat Session Endpoints
@api_bp.route('/get-user-sessions', methods=['GET'])
def get_user_sessions():
    """Get all chat sessions for a user."""
    try:
        user_id = request.args.get('userId')
        if not user_id:
            return jsonify({'error': 'userId is required'}), 400
            
        try:
            user_id = int(user_id)
        except ValueError:
            return jsonify({'error': 'userId must be an integer'}), 400
            
        from app.services.chat_service import get_user_sessions
        sessions = get_user_sessions(user_id)
        return jsonify(sessions)
        
    except Exception as e:
        logger.error(f"Error in get-user-sessions: {str(e)}")
        return jsonify({'error': str(e)}), 500

@api_bp.route('/get-session', methods=['GET'])
def get_session():
    """Get chat history for a specific session."""
    try:
        session_id = request.args.get('sessionId')
        user_id = request.args.get('userId')
        
        if not session_id or not user_id:
            return jsonify({'error': 'sessionId and userId are required'}), 400
            
        try:
            user_id = int(user_id)
        except ValueError:
            return jsonify({'error': 'userId must be an integer'}), 400
            
        from app.services.chat_service import get_session_history
        history = get_session_history(session_id, user_id)
        return jsonify(history)
        
    except ValueError as e:
        return jsonify({'error': str(e)}), 404
    except Exception as e:
        logger.error(f"Error in get-session: {str(e)}")
        return jsonify({'error': str(e)}), 500

@api_bp.route('/create-session', methods=['GET'])
def create_session():
    """Create a new chat session."""
    try:
        user_id = request.args.get('userId')
        if not user_id:
            return jsonify({'error': 'userId is required'}), 400
            
        try:
            user_id = int(user_id)
        except ValueError:
            return jsonify({'error': 'userId must be an integer'}), 400
            
        from app.services.chat_service import create_session
        new_session = create_session(user_id)
        return jsonify(new_session)
        
    except Exception as e:
        logger.error(f"Error in create-session: {str(e)}")
        return jsonify({'error': str(e)}), 500

@api_bp.route('/end-session', methods=['GET'])
def end_session():
    """End a specific chat session."""
    try:
        session_id = request.args.get('sessionId')
        if not session_id:
            return jsonify({'error': 'sessionId is required'}), 400
            
        from app.services.chat_service import end_session
        ended_session = end_session(session_id)
        return jsonify(ended_session)
        
    except ValueError as e:
        return jsonify({'error': str(e)}), 404
    except Exception as e:
        logger.error(f"Error in end-session: {str(e)}")
        return jsonify({'error': str(e)}), 500 