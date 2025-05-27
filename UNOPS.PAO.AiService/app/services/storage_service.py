from google.cloud import storage
import uuid
from io import BytesIO
from ..services.auth_service import AuthService

class GoogleCloudStorageService:
    def __init__(self, config):
        """Initialize the storage service with configuration."""
        self.auth_service = AuthService(config)
        self.storage_client = storage.Client(
            credentials=self.auth_service.get_credentials(),
            project=self.auth_service.get_project_id()
        )
        self.bucket_name = config.get('GOOGLE_CLOUD_STORAGE_BUCKET')
        self.bucket = self.storage_client.bucket(self.bucket_name)

    async def upload_audio_to_gcs(self, audio_bytes: bytes) -> str:
        """
        Upload audio bytes to Google Cloud Storage.
        
        Args:
            audio_bytes (bytes): The audio content to upload
            
        Returns:
            str: The public URL of the uploaded file
        """
        # Generate a unique filename
        object_name = f"tts_audio_{uuid.uuid4()}.mp3"
        
        # Create a blob and upload the file
        blob = self.bucket.blob(object_name)
        
        # Upload from memory
        blob.upload_from_string(
            audio_bytes,
            content_type='audio/mpeg'
        )
        
        # Return the public URL
        return blob.public_url

    async def upload_file_to_gcs(self, file, content_type: str = None) -> str:
        """
        Upload a file to Google Cloud Storage.
        
        Args:
            file: The file to upload (can be bytes or file-like object)
            content_type: The content type of the file
            
        Returns:
            str: The public URL of the uploaded file
        """
        # Generate a unique filename
        object_name = f"{uuid.uuid4()}_{file.filename}"
        
        # Create a blob and upload the file
        blob = self.bucket.blob(object_name)
        
        # Upload the file
        if isinstance(file, bytes):
            blob.upload_from_string(file, content_type=content_type)
        else:
            blob.upload_from_file(file, content_type=content_type)
        
        # Return the public URL
        return blob.public_url 