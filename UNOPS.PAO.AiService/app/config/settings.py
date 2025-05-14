import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

# Get the current environment
FLASK_ENV = os.getenv('FLASK_ENV', 'development')

class Config:
    """Base configuration."""
    # Vertex AI Settings
    PROJECT_ID = "unops-partneropportunity"
    LOCATION = "europe-west4"
    MODEL_NAME = "gemini-2.0-flash-001"

    # Model Generation Settings
    GENERATION_CONFIG = {
        "temperature": 0.2,
        "max_output_tokens": 8192,
        "top_p": 0.8,
        "top_k": 40
    }

    # Flask settings
    FLASK_DEBUG = False
    FLASK_PORT = int(os.getenv('FLASK_PORT', 8000))

    # Database settings - these will be overridden in specific configs
    SQLALCHEMY_TRACK_MODIFICATIONS = False
    
    @staticmethod
    def validate_db_config(db_user, db_pass, db_name, connection_name):
        if not all([db_user, db_pass, db_name, connection_name]):
            raise ValueError("Missing required database environment variables")

    # Database
    SQLALCHEMY_DATABASE_URI = os.getenv('DATABASE_URL')

    # Google Cloud Storage
    GOOGLE_CLOUD_PROJECT = os.getenv('GOOGLE_CLOUD_PROJECT')
    GOOGLE_CLOUD_STORAGE_BUCKET = os.getenv('GOOGLE_CLOUD_STORAGE_BUCKET')
    
    # Google Cloud credentials
    GOOGLE_APPLICATION_CREDENTIALS = os.getenv('GOOGLE_APPLICATION_CREDENTIALS')

    # AI Service settings
    AI_SERVICE_URL = os.getenv('AI_SERVICE_URL')
    AI_MODEL_LOCATION = os.getenv('AI_MODEL_LOCATION', 'us-central1')
    AI_MODEL_NAME = os.getenv('AI_MODEL_NAME', 'gemini-pro')

    # Flask settings
    DEBUG = os.getenv('FLASK_DEBUG', 'True').lower() == 'true'
    ENV = os.getenv('FLASK_ENV', 'development')

    def __init__(self):
        """Initialize configuration."""
        pass

    def get(self, key, default=None):
        """Get a configuration value by key.
        
        Args:
            key (str): The configuration key to retrieve
            default: The default value to return if the key is not found
            
        Returns:
            The configuration value if found, otherwise the default value
        """
        return getattr(self, key, default)

    def __getattr__(self, name):
        """Get attribute value, falling back to class attribute if instance attribute doesn't exist."""
        try:
            return super().__getattribute__(name)
        except AttributeError:
            return getattr(self.__class__, name, None)

class DevelopmentConfig(Config):
    """Development configuration."""
    FLASK_DEBUG = True
    
    # Local database configuration
    DB_USER = os.getenv('DB_USER')
    DB_PASS = os.getenv('DB_PASS')
    DB_NAME = os.getenv('DB_NAME')
    CLOUD_SQL_CONNECTION_NAME = os.getenv('CLOUD_SQL_CONNECTION_NAME')
    
    # Validate and construct database URI
    @classmethod
    def get_database_uri(cls):
        cls.validate_db_config(cls.DB_USER, cls.DB_PASS, cls.DB_NAME, cls.CLOUD_SQL_CONNECTION_NAME)
        return f"postgresql://{cls.DB_USER}:{cls.DB_PASS}@localhost:5433/{cls.DB_NAME}"

class ProductionConfig(Config):
    """Production configuration."""
    FLASK_DEBUG = False
    
    # Production database configuration
    DB_USER = os.getenv('PROD_DB_USER')
    DB_PASS = os.getenv('PROD_DB_PASS')
    DB_NAME = os.getenv('PROD_DB_NAME')
    CLOUD_SQL_CONNECTION_NAME = os.getenv('PROD_CLOUD_SQL_CONNECTION_NAME')
    
    # Validate and construct database URI
    @classmethod
    def get_database_uri(cls):
        cls.validate_db_config(cls.DB_USER, cls.DB_PASS, cls.DB_NAME, cls.CLOUD_SQL_CONNECTION_NAME)
        return f"postgresql://{cls.DB_USER}:{cls.DB_PASS}@localhost:5433/{cls.DB_NAME}"

class TestingConfig(Config):
    """Testing configuration."""
    TESTING = True
    FLASK_DEBUG = True
    
    # Test database configuration
    DB_USER = os.getenv('TEST_DB_USER')
    DB_PASS = os.getenv('TEST_DB_PASS')
    DB_NAME = os.getenv('TEST_DB_NAME')
    CLOUD_SQL_CONNECTION_NAME = os.getenv('TEST_CLOUD_SQL_CONNECTION_NAME')
    
    # Validate and construct database URI
    @classmethod
    def get_database_uri(cls):
        cls.validate_db_config(cls.DB_USER, cls.DB_PASS, cls.DB_NAME, cls.CLOUD_SQL_CONNECTION_NAME)
        return f"postgresql://{cls.DB_USER}:{cls.DB_PASS}@localhost:5433/{cls.DB_NAME}"

# Configuration dictionary
config = {
    'development': DevelopmentConfig,
    'production': ProductionConfig,
    'testing': TestingConfig,
    'default': DevelopmentConfig
}

# Get the active configuration
active_config = config.get(FLASK_ENV, config['default'])

# Export configuration variables
FLASK_DEBUG = active_config.FLASK_DEBUG
FLASK_PORT = active_config.FLASK_PORT
PROJECT_ID = active_config.PROJECT_ID
LOCATION = active_config.LOCATION
MODEL_NAME = active_config.MODEL_NAME
GENERATION_CONFIG = active_config.GENERATION_CONFIG
SQLALCHEMY_TRACK_MODIFICATIONS = active_config.SQLALCHEMY_TRACK_MODIFICATIONS
SQLALCHEMY_DATABASE_URI = active_config.get_database_uri()

# Print connection info (without password)
print(f"Environment: {FLASK_ENV}")
print(f"Debug mode: {FLASK_DEBUG}")
print(f"Connecting to database: postgresql://{active_config.DB_USER}:***@localhost:5433/{active_config.DB_NAME}") 