from app.services.text_to_speech_service import TextToSpeechService
from app.services.storage_service import GoogleCloudStorageService
from app.config.settings import config, FLASK_ENV
import asyncio

async def main():
    try:
        # Get the appropriate config class
        config_class = config.get(FLASK_ENV, config['default'])
        print(f"Using config class: {config_class.__name__}")
        
        # Create config instance
        config_instance = config_class()
        
        # Initialize services
        tts = TextToSpeechService(config_instance)
        storage = GoogleCloudStorageService(config_instance)
        print("Services initialized")
        
        # Test text-to-speech conversion
        test_text = "Hello, this is a test of the text-to-speech service."
        print(f"Converting text: {test_text}")
        
        # Convert text to audio
        audio = await tts.convert_text_to_audio(test_text)
        print(f"Audio bytes length: {len(audio) if audio else 'None'}")
        
        # Upload audio to cloud storage
        if audio:
            file_url = await storage.upload_audio_to_gcs(audio)
            print(f"Audio file uploaded to: {file_url}")
        
    except Exception as e:
        print(f"Error: {str(e)}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    asyncio.run(main()) 