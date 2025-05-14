from google.cloud import secretmanager
from google.oauth2 import service_account
from google.auth import default
import json
import os

class AuthService:
    def __init__(self, config):
        """Initialize the auth service with configuration."""
        self.project_id = config.get('GOOGLE_CLOUD_PROJECT')
        self._credentials = None

    def get_credentials(self):
        """
        Get Google Cloud credentials using application default credentials.
        
        Returns:
            google.oauth2.credentials.Credentials: The credentials object
        """
        if self._credentials is None:
            try:
                # Use application default credentials
                self._credentials, project_id = default()
            except Exception as e:
                raise Exception(f"Failed to get credentials: {str(e)}")
            
        return self._credentials

    def get_project_id(self):
        """Get the Google Cloud project ID."""
        return self.project_id 