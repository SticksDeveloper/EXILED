// -----------------------------------------------------------------------
// <copyright file="Speaker.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features.Toys
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using AdminToys;
    using Enums;
    using Interfaces;
    using MEC;
    using NAudio.Wave;
    using NVorbis;
    using UnityEngine;
    using Utils.Networking;
    using VoiceChat.Networking;

    using Object = UnityEngine.Object;

    /// <summary>
    /// A wrapper class for <see cref="SpeakerToy"/>.
    /// </summary>
    public class Speaker : AdminToy, IWrapper<SpeakerToy>
    {
        private const int SamplesPerChunk = 480;
        private readonly PlaybackBuffer playbackBuffer = new();
        private readonly Queue<float> streamBuffer = new();

        private float allowedSamples;
        private int samplesPerSecond;

        private bool stopPlayback;
        private bool isPlaying;

        /// <summary>
        /// Initializes a new instance of the <see cref="Speaker"/> class.
        /// </summary>
        /// <param name="speakerToy">The <see cref="SpeakerToy"/> of the toy.</param>
        internal Speaker(SpeakerToy speakerToy)
            : base(speakerToy, AdminToyType.Speaker) => Base = speakerToy;

        /// <summary>
        /// Gets the base <see cref="SpeakerToy"/>.
        /// </summary>
        public SpeakerToy Base { get; }

        /// <summary>
        /// Gets or sets the controller ID of the SpeakerToy.
        /// </summary>
        public byte ControllerID
        {
            get => Base.NetworkControllerId;
            set => Base.NetworkControllerId = value;
        }

        /// <summary>
        /// Gets or sets the volume of the audio source.
        /// </summary>
        public float Volume
        {
            get => Base.NetworkVolume;
            set => Base.NetworkVolume = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source is spatialized.
        /// </summary>
        public bool IsSpatial
        {
            get => Base.NetworkIsSpatial;
            set => Base.NetworkIsSpatial = value;
        }

        /// <summary>
        /// Gets or sets the maximum distance at which the audio source can be heard.
        /// </summary>
        public float MaxDistance
        {
            get => Base.NetworkMaxDistance;
            set => Base.NetworkMaxDistance = value;
        }

        /// <summary>
        /// Gets or sets the minimum distance at which the audio source reaches full volume.
        /// </summary>
        public float MinDistance
        {
            get => Base.NetworkMinDistance;
            set => Base.NetworkMinDistance = value;
        }

        /// <summary>
        /// Creates a new Speaker instance.
        /// </summary>
        /// <param name="controllerId">The controller ID for the SpeakerToy.</param>
        /// <param name="position">The position to place the SpeakerToy.</param>
        /// <param name="isSpatial">Indicates whether the audio is spatialized.</param>
        /// <param name="spawn">Determines if the speaker should be spawned immediately.</param>
        /// <returns>A new <see cref="Speaker"/> instance.</returns>
        public static Speaker Create(byte controllerId, Vector3 position, bool isSpatial = false, bool spawn = true)
        {
            Speaker speaker = new(Object.Instantiate(ToysHelper.SpeakerBaseObject))
            {
                Position = position,
                IsSpatial = isSpatial,
                Base = { ControllerId = controllerId },
            };

            if (spawn)
                speaker.Spawn();

            return speaker;
        }

        /// <summary>
        /// Gets the <see cref="Speaker"/> associated with a given <see cref="SpeakerToy"/>.
        /// </summary>
        /// <param name="speakerToy">The SpeakerToy instance.</param>
        /// <returns>The corresponding Speaker instance.</returns>
        public static Speaker Get(SpeakerToy speakerToy)
        {
            AdminToy adminToy = Map.Toys.FirstOrDefault(x => x.AdminToyBase == speakerToy);
            return adminToy is not null ? adminToy as Speaker : new Speaker(speakerToy);
        }

        /// <summary>
        /// Plays a single audio file through the speaker system. (No Arguments given (assuming you already preset those)).
        /// </summary>
        /// <param name="path">Path to the audio file to play.</param>
        /// <returns>A boolean indicating if playback was successful.</returns>
        public bool Play(string path) => Play(path, Volume, MinDistance, MaxDistance);

        /// <summary>
        /// Plays an audio file using FFmpeg to decode it into raw audio data.
        /// </summary>
        /// <param name="path">The file path of the audio file.</param>
        /// <param name="volume">The desired playback volume.</param>
        /// <param name="minDistance">The minimum distance at which the audio is audible.</param>
        /// <param name="maxDistance">The maximum distance at which the audio is audible.</param>
        /// <returns>A boolean indicating if playback was successful.</returns>
        public bool Play(string path, float volume, float minDistance, float maxDistance)
        {
            if (isPlaying)
                Stop();

            if (!File.Exists(path))
            {
                Log.Warn($"Tried playing audio from {path} but no file was found.");
                return false;
            }

            Volume = volume;
            MinDistance = minDistance;
            MaxDistance = maxDistance;

            isPlaying = true;
            Timing.RunCoroutine(PlaybackRoutine(path));
            return true;
        }

        /// <summary>
        /// Stops the current playback.
        /// </summary>
        public void Stop()
        {
            stopPlayback = true;
            isPlaying = false;
        }

        private IEnumerator<float> PlaybackRoutine(string filePath)
        {
            stopPlayback = false;

            // Determine file format based on extension
            string fileExtension = Path.GetExtension(filePath).ToLower();
            Log.Info($"File detected: {filePath}, Extension: {fileExtension}");

            int sampleRate; // Default to 48kHz
            int channels; // Default to mono

            // Variables for data processing
            float[] floatBuffer = new float[SamplesPerChunk];
            byte[] byteBuffer;

            switch (fileExtension)
            {
                case ".ogg":
                {
                    Log.Info("Detected OGG file. Using NVorbis for playback.");
                    using VorbisReader vorbisReader = new(filePath);
                    sampleRate = vorbisReader.SampleRate;
                    channels = vorbisReader.Channels;
                    samplesPerSecond = sampleRate * channels;

                    Log.Info($"Playing OGG file with Sample Rate: {sampleRate}, Channels: {channels}");

                    while (!stopPlayback)
                    {
                        int samplesRead = vorbisReader.ReadSamples(floatBuffer, 0, SamplesPerChunk);
                        if (samplesRead <= 0)
                            break;

                        foreach (float sample in floatBuffer.Take(samplesRead))
                            streamBuffer.Enqueue(sample);

                        while (streamBuffer.Count >= SamplesPerChunk)
                        {
                            allowedSamples += Time.deltaTime * samplesPerSecond;
                            int samplesToCopy = Mathf.Min(Mathf.FloorToInt(allowedSamples), streamBuffer.Count);

                            for (int i = 0; i < samplesToCopy; i++)
                                playbackBuffer.Write(streamBuffer.Dequeue());

                            allowedSamples -= samplesToCopy;

                            if (playbackBuffer.Length >= SamplesPerChunk)
                            {
                                playbackBuffer.ReadTo(floatBuffer, SamplesPerChunk, 0);
                                byte[] encodedData = EncodeSamples(floatBuffer);
                                AudioMessage audioMessage = new(ControllerID, encodedData, SamplesPerChunk);
                                audioMessage.SendToAuthenticated();

                                Log.Info($"Sent audio data: {SamplesPerChunk} bytes for Controller ID: {ControllerID}");
                            }

                            yield return Timing.WaitForOneFrame;
                        }
                    }

                    break;
                }

                case ".mp3":
                case ".wav":
                {
                    Log.Info($"Detected {fileExtension.ToUpper()} file. Using NAudio for playback.");

                    using WaveStream audioReader = fileExtension == ".mp3" ? new Mp3FileReader(filePath) : new WaveFileReader(filePath);
                    sampleRate = audioReader.WaveFormat.SampleRate;
                    channels = audioReader.WaveFormat.Channels;
                    samplesPerSecond = sampleRate * channels;

                    Log.Info($"Playing {fileExtension.ToUpper()} file with Sample Rate: {sampleRate}, Channels: {channels}");

                    byteBuffer = new byte[SamplesPerChunk * audioReader.WaveFormat.BlockAlign];

                    while (!stopPlayback)
                    {
                        int bytesRead = audioReader.Read(byteBuffer, 0, byteBuffer.Length);
                        if (bytesRead <= 0)
                            break;

                        int samplesRead = ConvertBytesToFloats(byteBuffer, floatBuffer, bytesRead, audioReader.WaveFormat);

                        foreach (float sample in floatBuffer.Take(samplesRead))
                            streamBuffer.Enqueue(sample);

                        while (streamBuffer.Count >= SamplesPerChunk)
                        {
                            allowedSamples += Time.deltaTime * samplesPerSecond;
                            int samplesToCopy = Mathf.Min(Mathf.FloorToInt(allowedSamples), streamBuffer.Count);

                            for (int i = 0; i < samplesToCopy; i++)
                                playbackBuffer.Write(streamBuffer.Dequeue());

                            allowedSamples -= samplesToCopy;

                            if (playbackBuffer.Length >= SamplesPerChunk)
                            {
                                playbackBuffer.ReadTo(floatBuffer, SamplesPerChunk, 0);
                                byte[] encodedData = EncodeSamples(floatBuffer);
                                AudioMessage audioMessage = new(ControllerID, encodedData, SamplesPerChunk);
                                audioMessage.SendToAuthenticated();

                                Log.Info($"Sent audio data: {SamplesPerChunk} bytes for Controller ID: {ControllerID}");
                            }

                            yield return Timing.WaitForOneFrame;
                        }
                    }

                    break;
                }

                default:
                    Log.Error($"Unsupported file format: {fileExtension}");
                    yield break;
            }

            Log.Info($"Playback completed for file '{filePath}'.");
            isPlaying = false;
        }

        private int ConvertBytesToFloats(byte[] byteBuffer, float[] floatBuffer, int bytesRead, WaveFormat waveFormat)
        {
            int bytesPerSample = waveFormat.BitsPerSample / 8;
            int sampleCount = bytesRead / bytesPerSample;

            for (int i = 0; i < sampleCount; i++)
            {
                // Assuming 16-bit PCM data
                short sample = BitConverter.ToInt16(byteBuffer, i * bytesPerSample);
                floatBuffer[i] = sample / 32768f; // Normalize to -1.0 to 1.0 range
            }

            return sampleCount;
        }

        private byte[] EncodeSamples(float[] samples)
        {
            byte[] encodedData = new byte[samples.Length * sizeof(float)];
            Buffer.BlockCopy(samples, 0, encodedData, 0, encodedData.Length);
            return encodedData;
        }
    }
}
