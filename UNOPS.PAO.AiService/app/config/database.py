# This file is kept as a placeholder for future database needs
# Currently, the application doesn't require any database functionality 

from app.extensions import db

def init_db(app):
    """Initialize the database with the Flask app"""
    with app.app_context():
        # Import models here to avoid circular imports
        from app.models.ai_prompt import AiPrompt
        
        # Create all tables
        db.create_all() 