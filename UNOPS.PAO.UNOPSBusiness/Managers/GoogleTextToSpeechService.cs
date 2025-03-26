using Google.Cloud.TextToSpeech.V1;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class GoogleTextToSpeechService
{
    private readonly TextToSpeechClient _ttsClient;

    public GoogleTextToSpeechService() 
    {
        _ttsClient = TextToSpeechClient.Create();
    }

    public async Task<byte[]> ConvertTextToAudio(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var client = TextToSpeechClient.Create();

        // 1. Prepare the request
        var request = new SynthesizeSpeechRequest
        {
            Input = new SynthesisInput { Text = text },
            Voice = new VoiceSelectionParams
            {
                LanguageCode = "en-US", // Change if needed
                SsmlGender = SsmlVoiceGender.Neutral
            },
            AudioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            }
        };

        // 2. Call Google TTS API
        var response = client.SynthesizeSpeech(request);

        byte[] audioBytes = response.AudioContent.ToByteArray();

        return audioBytes;
    }
}