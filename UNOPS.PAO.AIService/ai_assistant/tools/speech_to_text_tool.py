import logging
from typing import Dict, Any

from google.adk.tools.tool_context import ToolContext
from google.genai import types

# Import Google Cloud Speech client
from google.cloud import speech

logger = logging.getLogger(__name__)

# --- SPEECH-TO-TEXT TOOL USING GOOGLE CLOUD SPEECH-TO-TEXT API ---

async def save_audio_as_artifact(tool_context: ToolContext, filename: str = "audio_file") -> Dict[str, Any]:
    """
    Helper tool to save audio from the current message as an artifact.
    This should be called first before transcription tools.

    Args:
        tool_context: The tool context provided by the ADK framework.
        filename: The filename to use when saving as artifact.

    Returns:
        A dictionary containing the save result and filename.
    """
    logger.info(f"Tool: Request to save audio as artifact '{filename}'.")

    try:
        # This is a placeholder - in practice, the ADK framework should handle
        # artifact saving automatically for message parts.
        # For now, we'll assume the audio is already available as an artifact
        # since it was uploaded as part of the message.
        
        audio_metadata = tool_context.state.get('available_audio_files', [])
        
        if not audio_metadata:
            return {
                "error": "No audio files found in the current message",
                "status": "failure",
                "suggestion": "Please attach an audio file to your message"
            }
        
        first_audio = audio_metadata[0]
        actual_filename = first_audio.get('filename', filename)
        
        logger.info(f"📄 Audio file should be available as artifact: {actual_filename}")
        
        return {
            "status": "success",
            "filename": actual_filename,
            "message": f"Audio file '{actual_filename}' is ready for transcription"
        }

    except Exception as e:
        logger.error(f"❌ Error preparing audio artifact: {e}", exc_info=True)
        return {
            "error": f"Failed to prepare audio artifact: {str(e)}", 
            "status": "failure"
        }

async def transcribe_audio_from_message(tool_context: ToolContext, filename: str = "audio_file") -> Dict[str, Any]:
    """
    Transcribes audio from the most recent message based on available metadata.
    This tool guides the user to use the artifact-based approach since direct message access isn't available.

    Args:
        tool_context: The tool context provided by the ADK framework.
        filename: Optional filename to use when saving as artifact (default: "audio_file").

    Returns:
        A dictionary containing the transcribed text and status.
    """
    logger.info(f"Tool: Request to transcribe audio from message (will look for '{filename}').")

    try:
        # Check if there are audio files available in session metadata
        audio_metadata = tool_context.state.get('available_audio_files', [])
        
        if not audio_metadata:
            logger.warning("No audio files found in session metadata.")
            return {
                "error": "No audio files found in the current message. Please attach an audio file and try again.",
                "status": "failure",
                "suggestion": "Make sure to attach an audio file (.mp3, .wav, .aac, etc.) to your message before requesting transcription."
            }

        # Use the first available audio file
        first_audio = audio_metadata[0]
        audio_filename = first_audio.get('filename', filename)
        
        logger.info(f"📄 Found audio file metadata: {audio_filename} ({first_audio.get('mime_type', 'unknown type')})")
        
        # The audio should have been automatically saved as an artifact during message processing
        # Try to transcribe using the artifact-based approach
        return await transcribe_audio_from_artifact(tool_context, audio_filename)

    except Exception as e:
        logger.error(f"❌ Error transcribing audio from message: {e}", exc_info=True)
        return {
            "error": f"Failed to transcribe audio: {str(e)}", 
            "status": "failure",
            "suggestion": "Please try uploading the audio file again or use a different audio format."
        }

async def transcribe_audio_from_artifact(tool_context: ToolContext, artifact_filename: str) -> Dict[str, Any]:
    """
    Transcribes audio from an audio artifact (e.g., MP3, WAV) into text using Google Cloud Speech-to-Text API.

    Args:
        tool_context: The tool context provided by the ADK framework.
        artifact_filename: The filename of the audio artifact (as saved by context.save_artifact).
                           This filename is used to load the artifact from the ArtifactService.

    Returns:
        A dictionary containing the transcribed text and status.
        Example: {"transcribed_text": "Hello world.", "status": "success"}
    """
    logger.info(f"Tool: Request to transcribe audio from artifact '{artifact_filename}'.")

    try:
        # 1. Load the artifact from the ArtifactService
        audio_part = await tool_context.load_artifact(filename=artifact_filename)

        if not audio_part or not audio_part.inline_data:
            logger.warning(f"Audio artifact '{artifact_filename}' not found or has no inline data.")
            return {"error": f"Audio artifact '{artifact_filename}' not found or invalid.", "status": "failure"}

        audio_bytes = audio_part.inline_data.data
        mime_type = audio_part.inline_data.mime_type

        # 2. Initialize Google Cloud Speech-to-Text client
        client = speech.SpeechClient()

        # 3. Configure the audio for transcription
        # You might need to adjust encoding and sample_rate_hertz based on expected input.
        # For common formats like MP3, FLAC, OGG, use AUTO_DETECT and let the API figure it out.
        audio = speech.RecognitionAudio(content=audio_bytes)
        config = speech.RecognitionConfig(
            encoding=speech.RecognitionConfig.AudioEncoding.ENCODING_UNSPECIFIED, # Auto-detect encoding
            sample_rate_hertz=0, # Auto-detect sample rate
            language_code="en-US", # Adjust as needed
            # You can add features like enable_automatic_punctuation, model="default" or "latest"
            enable_automatic_punctuation=True,
            model="latest_long",  # Use the latest long model for better accuracy
        )

        # 4. Perform synchronous speech recognition
        # For longer audio (over 60 seconds), you'd use client.long_running_recognize
        response = client.recognize(config=config, audio=audio)

        transcribed_text = ""
        confidence_scores = []
        
        for result in response.results:
            # The first alternative is generally the most confident one
            alternative = result.alternatives[0]
            transcribed_text += alternative.transcript + " "
            confidence_scores.append(alternative.confidence)

        transcribed_text = transcribed_text.strip()
        avg_confidence = sum(confidence_scores) / len(confidence_scores) if confidence_scores else 0.0
        
        logger.info(f"Successfully transcribed audio from '{artifact_filename}'. Text length: {len(transcribed_text)} characters. Avg confidence: {avg_confidence:.2f}")
        # logger.debug(f"Transcribed text: \n{transcribed_text[:500]}...")

        return {
            "transcribed_text": transcribed_text,
            "status": "success",
            "source_filename": artifact_filename,
            "mime_type": mime_type,
            "confidence": avg_confidence,
            "character_count": len(transcribed_text)
        }

    except Exception as e:
        logger.error(f"❌ Error transcribing audio from '{artifact_filename}': {e}", exc_info=True)
        return {"error": f"Failed to transcribe audio: {str(e)}", "status": "failure"}


async def transcribe_audio_long_running(tool_context: ToolContext, artifact_filename: str, language_code: str = "en-US") -> Dict[str, Any]:
    """
    Transcribes longer audio files (over 60 seconds) using Google Cloud Speech-to-Text long running operation.
    
    Args:
        tool_context: The tool context provided by the ADK framework.
        artifact_filename: The filename of the audio artifact.
        language_code: Language code for transcription (default: "en-US").
        
    Returns:
        A dictionary containing the transcribed text and status.
    """
    logger.info(f"Tool: Request to transcribe long audio from artifact '{artifact_filename}' with language '{language_code}'.")
    
    try:
        # 1. Load the artifact from the ArtifactService
        audio_part = await tool_context.load_artifact(filename=artifact_filename)

        if not audio_part or not audio_part.inline_data:
            logger.warning(f"Audio artifact '{artifact_filename}' not found or has no inline data.")
            return {"error": f"Audio artifact '{artifact_filename}' not found or invalid.", "status": "failure"}

        audio_bytes = audio_part.inline_data.data
        mime_type = audio_part.inline_data.mime_type

        # 2. Initialize Google Cloud Speech-to-Text client
        client = speech.SpeechClient()

        # 3. Configure the audio for long-running transcription
        audio = speech.RecognitionAudio(content=audio_bytes)
        config = speech.RecognitionConfig(
            encoding=speech.RecognitionConfig.AudioEncoding.ENCODING_UNSPECIFIED,
            sample_rate_hertz=0, 
            language_code=language_code,
            enable_automatic_punctuation=True,
            enable_word_time_offsets=True,  # Get timestamps for each word
            model="latest_long",
        )

        # 4. Perform long-running speech recognition
        operation = client.long_running_recognize(config=config, audio=audio)
        logger.info("Waiting for long-running operation to complete...")
        
        response = operation.result(timeout=300)  # 5 minute timeout

        transcribed_text = ""
        confidence_scores = []
        word_count = 0
        
        for result in response.results:
            alternative = result.alternatives[0]
            transcribed_text += alternative.transcript + " "
            confidence_scores.append(alternative.confidence)
            word_count += len(alternative.words)

        transcribed_text = transcribed_text.strip()
        avg_confidence = sum(confidence_scores) / len(confidence_scores) if confidence_scores else 0.0
        
        logger.info(f"Successfully transcribed long audio from '{artifact_filename}'. Text length: {len(transcribed_text)} characters, Words: {word_count}, Avg confidence: {avg_confidence:.2f}")

        return {
            "transcribed_text": transcribed_text,
            "status": "success",
            "source_filename": artifact_filename,
            "mime_type": mime_type,
            "confidence": avg_confidence,
            "character_count": len(transcribed_text),
            "word_count": word_count,
            "language_code": language_code
        }

    except Exception as e:
        logger.error(f"❌ Error transcribing long audio from '{artifact_filename}': {e}", exc_info=True)
        return {"error": f"Failed to transcribe long audio: {str(e)}", "status": "failure"} 