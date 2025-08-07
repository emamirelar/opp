# Speech-to-Text (STT) Tools

This document describes the Speech-to-Text tools available in the AI Assistant for transcribing audio artifacts.

## Overview

The AI Assistant includes two Speech-to-Text tools that use Google Cloud Speech-to-Text API to provide precise audio transcription:

- `transcribe_audio_from_artifact` - For short audio files (under 60 seconds)
- `transcribe_audio_long_running` - For longer audio files (over 60 seconds)

## Tools

### 1. transcribe_audio_from_artifact

**Purpose**: Transcribes short audio files (under 60 seconds) using synchronous recognition.

**Parameters**:
- `artifact_filename` (str): The filename of the audio artifact saved by `context.save_artifact`

**Returns**:
```json
{
  "transcribed_text": "Hello world.",
  "status": "success",
  "source_filename": "audio.mp3",
  "mime_type": "audio/mp3", 
  "confidence": 0.95,
  "character_count": 12
}
```

**Features**:
- Auto-detects audio encoding and sample rate
- Provides confidence scores for transcription quality
- Supports automatic punctuation
- Uses latest Google Speech model for best accuracy

### 2. transcribe_audio_long_running

**Purpose**: Transcribes longer audio files (over 60 seconds) using asynchronous long-running operations.

**Parameters**:
- `artifact_filename` (str): The filename of the audio artifact
- `language_code` (str, optional): Language code for transcription (default: "en-US")

**Returns**:
```json
{
  "transcribed_text": "This is a longer transcription...",
  "status": "success", 
  "source_filename": "long_meeting.wav",
  "mime_type": "audio/wav",
  "confidence": 0.92,
  "character_count": 1250,
  "word_count": 245,
  "language_code": "en-US"
}
```

**Features**:
- Handles audio files longer than 60 seconds
- Provides word-level timestamps
- Supports multiple languages
- 5-minute timeout for processing
- Enhanced accuracy for long-form content

## Supported Audio Formats

Both tools support these audio formats:
- WAV (`audio/wav`)
- MP3 (`audio/mp3`) 
- AIFF (`audio/aiff`)
- AAC (`audio/aac`)
- OGG Vorbis (`audio/ogg`)
- FLAC (`audio/flac`)

## Usage Examples

### Agent Usage

The AI agent automatically selects the appropriate tool based on user requests:

**Short Audio Transcription**:
```
User: "Transcribe this audio file" + [short_audio.mp3]
→ Agent uses transcribe_audio_from_artifact
```

**Long Audio Transcription**:
```
User: "Transcribe this meeting recording" + [long_meeting.wav] 
→ Agent uses transcribe_audio_long_running
```

**Audio Analysis vs Transcription**:
```
User: "What's in this recording?" + [audio.mp3]
→ Agent uses native Gemini audio analysis (faster, general analysis)

User: "Give me the exact transcript" + [audio.mp3]
→ Agent uses STT tools (precise transcription with confidence scores)
```

## Configuration

### Google Cloud Setup

1. **Enable Google Cloud Speech-to-Text API** in your Google Cloud Project
2. **Set up authentication** using one of these methods:
   - Service Account Key file (set `GOOGLE_APPLICATION_CREDENTIALS` environment variable)
   - Application Default Credentials (ADC)
   - Google Cloud SDK authentication

3. **Install dependencies**:
   ```bash
   pip install google-cloud-speech>=2.21.0
   ```

### Language Support

The tools support Google Cloud Speech-to-Text's language codes including:
- `en-US` (English - US)
- `en-GB` (English - UK) 
- `es-ES` (Spanish - Spain)
- `fr-FR` (French - France)
- `de-DE` (German - Germany)
- And many more...

## Error Handling

Both tools include comprehensive error handling:

```json
{
  "error": "Failed to transcribe audio: [error details]",
  "status": "failure"
}
```

Common error scenarios:
- Audio artifact not found or invalid
- Unsupported audio format
- Google Cloud API authentication issues
- Network connectivity problems
- Audio file too large or corrupted

## Best Practices

1. **Choose the Right Tool**:
   - Use `transcribe_audio_from_artifact` for short clips, voice messages, quick audio
   - Use `transcribe_audio_long_running` for meetings, interviews, long recordings

2. **Audio Quality**:
   - Higher quality audio produces better transcription results
   - Clear speech with minimal background noise works best
   - Supported sample rates: 8kHz to 48kHz

3. **Language Detection**:
   - Specify the correct language code for best results
   - The default is `en-US` but can be changed per request

4. **File Size Limits**:
   - Maximum file size: depends on Google Cloud Speech limits
   - For very large files, consider chunking the audio

## Integration with Gemini

The STT tools complement Gemini's native audio capabilities:

- **Use STT tools** when you need:
  - Precise word-for-word transcription
  - Confidence scores
  - Word-level timestamps
  - Language-specific transcription

- **Use Gemini native audio** when you need:
  - General audio understanding
  - Audio content analysis
  - Music or sound identification
  - Faster processing for simple queries

## Troubleshooting

### Common Issues

1. **"Audio artifact not found"**
   - Ensure the audio file was properly saved as an artifact
   - Check that the artifact filename is correct

2. **Google Cloud authentication errors**
   - Verify `GOOGLE_APPLICATION_CREDENTIALS` is set
   - Check that the service account has Speech-to-Text API permissions

3. **Unsupported audio format**
   - Convert audio to supported formats (WAV, MP3, FLAC, etc.)
   - Check that the MIME type is correctly detected

4. **Long-running operation timeout**
   - For very long audio files, the default 5-minute timeout may not be sufficient
   - Consider splitting large audio files into smaller chunks

### Debugging

Enable debug logging to troubleshoot issues:

```python
import logging
logging.getLogger('ai_assistant.tools.speech_to_text_tool').setLevel(logging.DEBUG)
```

This will show detailed logs of the transcription process including file loading, API calls, and response processing. 