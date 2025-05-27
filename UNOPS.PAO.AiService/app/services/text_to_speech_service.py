from google.cloud import texttospeech
from ..services.auth_service import AuthService

class TextToSpeechService:
    def __init__(self, config):
        """Initialize the text-to-speech service with configuration."""
        self.auth_service = AuthService(config)
        self.client = texttospeech.TextToSpeechClient(
            credentials=self.auth_service.get_credentials()
        )

    async def convert_text_to_audio(self, text: str) -> bytes:
        """
        Convert text to audio using Google Cloud Text-to-Speech.
        
        Args:
            text (str): The text to convert to speech
            
        Returns:
            bytes: The audio content as bytes
        """
        if not text:
            return None

        # Set the text input to be synthesized
        synthesis_input = texttospeech.SynthesisInput(text=text)

        # Build the voice request
        voice = texttospeech.VoiceSelectionParams(
            language_code="en-US",
            ssml_gender=texttospeech.SsmlVoiceGender.NEUTRAL
        )

        # Select the type of audio file to return
        audio_config = texttospeech.AudioConfig(
            audio_encoding=texttospeech.AudioEncoding.MP3
        )

        # Perform the text-to-speech request
        response = self.client.synthesize_speech(
            input=synthesis_input,
            voice=voice,
            audio_config=audio_config
        )

        # Return the audio content
        return response.audio_content 