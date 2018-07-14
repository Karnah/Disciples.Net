using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NLayer.NAudioSupport;

using Inftastructure;
using Inftastructure.Interfaces;

namespace Animation.Implementation
{
    public class AudioPlaybackEngine : IAudioService, IDisposable
    {
        private readonly IWavePlayer _outputDevice;
        private readonly MixingSampleProvider _mixer;

        private readonly SortedDictionary<string, IList<CachedSound>> _musics;
        private readonly SortedDictionary<string, CachedSound> _sounds;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            _musics = new SortedDictionary<string, IList<CachedSound>>();
            _sounds = new SortedDictionary<string, CachedSound>();

            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            _mixer.ReadFully = true;

            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_mixer);
            _outputDevice.Play();

            //LoadMusic();
            //LoadSounds();
        }


        private void LoadMusic()
        {
            var cd = Environment.CurrentDirectory;
            foreach (var musicFile in Directory.GetFiles($"{cd}\\Music"))
            {
                var fileName = Path.GetFileNameWithoutExtension(musicFile);
                var match = Regex.Match(fileName, @"(?<key>\D+)(?<numbers>\d+)");
                var key = match.Success
                    ? match.Groups["key"].Value
                    : fileName;

                if (_musics.ContainsKey(key) == false)
                    _musics.Add(key, new List<CachedSound>());

                _musics[key].Add(CreateCachedSound(musicFile, true));
            }
        }

        private void LoadSounds()
        {
            var cd = Environment.CurrentDirectory;
            foreach (var musicFile in Directory.GetFiles($"{cd}\\Sounds"))
            {
                var fileName = Path.GetFileNameWithoutExtension(musicFile);
                _sounds.Add(fileName, CreateCachedSound(musicFile, false));
            }
        }

        // todo крайне низкая скорость загрузки
        private static CachedSound CreateCachedSound(string fileName, bool repeat)
        {
            var builder = new Mp3FileReader.FrameDecompressorBuilder(wf => new Mp3FrameDecompressor(wf));
            using (var audioFileReader = new Mp3FileReader(fileName, builder))
            {
                var sampleChannel = new SampleChannel(audioFileReader);
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[sampleChannel.WaveFormat.SampleRate * sampleChannel.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = sampleChannel.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }

                return new CachedSound(wholeFile.ToArray(), audioFileReader.WaveFormat, repeat);
            }
        }


        public void PlayBackground(string name)
        {
            if (_musics.ContainsKey(name) == false)
                return;

            var musics = _musics[name];
            var sound = musics[RandomGenerator.Next(0, musics.Count)];
            PlaySound(sound);
        }

        public void PlaySound(string name)
        {
            if (_sounds.ContainsKey(name) == false)
                return;

            var sound = _sounds[name];
            PlaySound(sound);
        }

        private void PlaySound(CachedSound sound)
        {
            var provider = new CachedSoundSampleProvider(sound);
            _mixer.AddMixerInput(ConvertToRightChannelCount(provider));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
            {
                return input;
            }

            if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }

            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }


        public void Dispose()
        {
            _outputDevice.Dispose();
        }
    }

    public class CachedSound
    {
        public CachedSound(float[] audioData, WaveFormat waveFormat, bool repeat)
        {
            AudioData = audioData;
            WaveFormat = waveFormat;
            Repeat = repeat;
        }


        public float[] AudioData { get; }

        public WaveFormat WaveFormat { get; }

        public bool Repeat { get; }
    }

    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound _cachedSound;
        private long _position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this._cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = _cachedSound.AudioData.Length - _position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);
            _position += samplesToCopy;

            if (_position == _cachedSound.AudioData.Length && _cachedSound.Repeat)
            {
                //AudioPlaybackEngine.Instance.PlaySound(cachedSound);
            }
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat => _cachedSound.WaveFormat;
    }
}
