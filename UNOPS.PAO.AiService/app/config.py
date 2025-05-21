import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    # Database
    SQLALCHEMY_DATABASE_URI = os.getenv('DATABASE_URL')
    SQLALCHEMY_TRACK_MODIFICATIONS = False

    # Google Cloud Storage
    GOOGLE_CLOUD_PROJECT = os.getenv('GOOGLE_CLOUD_PROJECT')
    GOOGLE_CLOUD_STORAGE_BUCKET = os.getenv('GOOGLE_CLOUD_STORAGE_BUCKET')
    
    # Google Cloud credentials
    GOOGLE_APPLICATION_CREDENTIALS = os.getenv('GOOGLE_APPLICATION_CREDENTIALS')

    # AI Service settings
    AI_SERVICE_URL = os.getenv('AI_SERVICE_URL')
    AI_MODEL_LOCATION = os.getenv('AI_MODEL_LOCATION', 'us-central1')
    AI_MODEL_NAME = os.getenv('AI_MODEL_NAME', 'gemini-pro') 