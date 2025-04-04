using Strobotnik.Klattersynth;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace KlattersynthAPI
{
    class Program
    {
        const bool USE_STREAMING_MODE = false;
        const int SAMPLE_RATE = 11025;
        const int MS_PER_SPEECH_FRAME = 10;
        const int FLUTTER = 10;
        const float FLUTTER_SPEED = 1f;

        const int VOICE_FUNDAMENTAL_FREQ = 220;
        const SpeechSynth.VoicingSource VOICING_SOURCE = SpeechSynth.VoicingSource.natural;
        const bool BRACKETS_AS_PHONEMES = false;
        const bool WITH_LOUDNESS = true;

        static byte[] GetWAV(string text, int voiceFundamentalFreq, SpeechSynth.VoicingSource voicingSource, bool bracketsAsPhonemes, bool withLoudness)
        {
            float[] samples = GetSamples(text, voiceFundamentalFreq, voicingSource, bracketsAsPhonemes, withLoudness);
            int chunkSize = 36 + samples.Length * sizeof(float);
            int subchunk1Size = 16;
            short audioFormat = 3;
            short numChannels = 1;
            int sampleRate = SAMPLE_RATE;
            int byteRate = SAMPLE_RATE * numChannels * sizeof(float);
            short blockAlign = (short)(numChannels * sizeof(float));
            short bitsPerSample = sizeof(float) * 8;
            int subchunk2Size = samples.Length * sizeof(float);

            byte[] wav;
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(chunkSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(subchunk1Size);
                writer.Write(audioFormat);
                writer.Write(numChannels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write(blockAlign);
                writer.Write(bitsPerSample);

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(subchunk2Size);

                foreach (float sample in samples)
                    writer.Write(sample);

                writer.Flush();
                wav = stream.ToArray();
            }

            return wav;
        }

        static float[] GetSamples(string text, int voiceFundamentalFreq, SpeechSynth.VoicingSource voicingSource, bool bracketsAsPhonemes, bool withLoudness)
        {
            SpeechClip clip = synth.pregenerate(new StringBuilder(text), voiceFundamentalFreq, voicingSource, bracketsAsPhonemes, withLoudness);
            return clip.pregenAudio.data;
        }

        static readonly AudioSource source = new AudioSource();
        static readonly SpeechSynth synth = new SpeechSynth();

        static void Main(string[] args)
        {
            synth.init(source, USE_STREAMING_MODE, SAMPLE_RATE, MS_PER_SPEECH_FRAME, FLUTTER, FLUTTER_SPEED);

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();

            while (true) {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string text = request.QueryString["text"];
                if (string.IsNullOrEmpty(text))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Close();
                    continue;
                }

                int voiceFundamentalFreq = int.TryParse(request.QueryString["voiceFundamentalFreq"], out int v) ? v : VOICE_FUNDAMENTAL_FREQ;
                SpeechSynth.VoicingSource voicingSource = Enum.TryParse(request.QueryString["voicingSource"], out SpeechSynth.VoicingSource vs) ? vs : VOICING_SOURCE;
                bool bracketsAsPhonemes = bool.TryParse(request.QueryString["bracketsAsPhonemes"], out bool b) ? b : BRACKETS_AS_PHONEMES;
                bool withLoudness = bool.TryParse(request.QueryString["withLoudness"], out bool wl) ? wl : WITH_LOUDNESS;

                byte[] wav = GetWAV(text, voiceFundamentalFreq, voicingSource, bracketsAsPhonemes, withLoudness);

                response.ContentType = "audio/wav";
                response.ContentLength64 = wav.Length;
                using (Stream output = response.OutputStream)
                    output.Write(wav, 0, wav.Length);

                response.Close();
            }
        }
    }
}
