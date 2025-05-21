from flask import Blueprint, request, jsonify
from ..services.ai_service import AiService
from ..models.ai_chat import AiChatSession
from ..extensions import db
from typing import Dict, Any
import json

gemini_bp = Blueprint('gemini', __name__)
ai_service = None

def init_ai_service(config):
    global ai_service
    ai_service = AiService(config)

@gemini_bp.route('/chat', methods=['POST'])
def chat():
    """
    Process chat messages.
    
    Expected request body:
    {
        "sessionId": "guid",
        "prompt": "string",
        "extractedText": "string"
    }
    """
    try:
        data = request.get_json()
        session_id = data.get('sessionId')
        message = data.get('prompt', '')
        extracted_text = data.get('extractedText')
        
        # Get the session
        session = db.session.query(AiChatSession).filter_by(Id=session_id).first()
        if not session:
            return jsonify({'error': 'Session not found'}), 404

        # Process the chat
        response = ai_service.process_chat(
            session=session,
            message=message,
            extracted_text=extracted_text
        )

        return jsonify(response)

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@gemini_bp.route('/accessibility', methods=['POST'])
async def update_accessibility():
    """
    Update text-to-speech settings for a session.
    
    Expected request body:
    {
        "sessionId": "guid",
        "textToSpeech": boolean
    }
    """
    try:
        data = request.get_json()
        session_id = data.get('sessionId')
        text_to_speech = data.get('textToSpeech', False)

        session = db.session.query(AiChatSession).filter_by(Id=session_id).first()
        if not session:
            return jsonify({'error': 'Session not found'}), 404

        session.TextToSpeech = text_to_speech
        db.session.commit()

        return jsonify({'success': True})

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@gemini_bp.route('/process-file', methods=['POST'])
def process_file():
    """
    Process uploaded files for chat.
    
    Expected request body:
    {
        "file": file (multipart/form-data),
        "type": "string" (optional)
    }
    """
    try:
        if 'file' not in request.files:
            return jsonify({'error': 'No file provided'}), 400

        file = request.files['file']
        file_type = request.form.get('type')

        # Process the file
        extracted_text = ai_service.extract_text_from_file(file)
        file_url = ai_service.storage_service.upload_file_to_gcs(file)

        return jsonify({
            'extractedText': extracted_text,
            'fileUrl': file_url,
            'fileType': file_type or file.content_type
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500 