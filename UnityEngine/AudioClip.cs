namespace UnityEngine
{
    public class AudioClip
    {
        public float[] data;

        public delegate void PCMReaderCallback(float[] data);
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream, PCMReaderCallback pcmreadercallback)
        {
            float[] data = new float[lengthSamples];
            pcmreadercallback(data);

            AudioClip clip = new AudioClip();
            clip.data = data;
            return clip;
        }
    }
}
