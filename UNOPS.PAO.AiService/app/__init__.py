from flask import Flask
from app.extensions import db
import logging
from .config.settings import config, FLASK_ENV

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def create_app():
    """Initialize and configure the Flask application."""
    # Get the appropriate config class based on environment
    config_class = config.get(FLASK_ENV, config['default'])
    app = Flask(__name__)
    
    # Create config instance and configure the Flask app
    config_instance = config_class()
    app.config['SQLALCHEMY_DATABASE_URI'] = config_instance.get_database_uri()
    app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False
    app.config['DEBUG'] = config_instance.get('DEBUG', False)
    app.config['FLASK_PORT'] = config_instance.get('FLASK_PORT', 8000)
    
    # Initialize extensions
    db.init_app(app)
    
    # Initialize AI service
    from .controllers.gemini_controller import gemini_bp, init_ai_service
    from .api.routes import api_bp
    init_ai_service(app.config)
    
    @app.before_request
    def before_request():
        """Ensure clean database state before each request"""
        try:
            # If there's any pending transaction, roll it back
            db.session.rollback()
        except Exception as e:
            logger.error(f"Error in before_request: {str(e)}")

    @app.teardown_request
    def teardown_request(exception=None):
        """Clean up after each request"""
        try:
            if exception:
                db.session.rollback()
            db.session.remove()
        except Exception as e:
            logger.error(f"Error in teardown_request: {str(e)}")
    
    # Register blueprints
    app.register_blueprint(gemini_bp, url_prefix='/gemini')
    app.register_blueprint(api_bp, url_prefix='/api')
    
    return app