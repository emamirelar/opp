from flask import Flask
from app.extensions import db
from app.models.ai_prompt import AiPrompt
from app.config.settings import SQLALCHEMY_DATABASE_URI
import json

def create_app():
    app = Flask(__name__)
    app.config['SQLALCHEMY_DATABASE_URI'] = SQLALCHEMY_DATABASE_URI
    app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False
    db.init_app(app)
    return app

def insert_prompts():
    app = create_app()
    with app.app_context():
        # Define the bulk contact action prompt
        bulk_contact_prompt = {
            'Type': 'bulk_contact_action',
            'Prompt': 'You are an AI assistant helping to process bulk contact actions...',  # Your actual prompt text here
            'GenerationConfig': json.dumps({
                'temperature': 0.7,
                'top_p': 1,
                'top_k': 40,
                'max_output_tokens': 1024,
            }),
            'ContentConfig': json.dumps({
                'safety_settings': [
                    {'category': 'HARM_CATEGORY_HARASSMENT', 'threshold': 'BLOCK_MEDIUM_AND_ABOVE'},
                    {'category': 'HARM_CATEGORY_HATE_SPEECH', 'threshold': 'BLOCK_MEDIUM_AND_ABOVE'},
                    {'category': 'HARM_CATEGORY_SEXUALLY_EXPLICIT', 'threshold': 'BLOCK_MEDIUM_AND_ABOVE'},
                    {'category': 'HARM_CATEGORY_DANGEROUS_CONTENT', 'threshold': 'BLOCK_MEDIUM_AND_ABOVE'},
                ]
            }),
            'Project': 'your-project-id',
            'Location': 'your-location',
            'Model': 'gemini-pro'
        }

        # Check if prompt already exists
        existing_prompt = AiPrompt.query.filter_by(Type='bulk_contact_action').first()
        
        if existing_prompt:
            print("Updating existing prompt...")
            for key, value in bulk_contact_prompt.items():
                setattr(existing_prompt, key, value)
        else:
            print("Creating new prompt...")
            prompt = AiPrompt(**bulk_contact_prompt)
            db.session.add(prompt)
        
        db.session.commit()
        
        # Verify the prompt was saved
        saved_prompt = AiPrompt.query.filter_by(Type='bulk_contact_action').first()
        if saved_prompt:
            print("Prompt saved successfully!")
            print(f"Type: {saved_prompt.Type}")
            print(f"Project: {saved_prompt.Project}")
        else:
            print("Error: Prompt not saved!")

if __name__ == '__main__':
    insert_prompts() 