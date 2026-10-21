using System;
using UnityEngine;

namespace JahNomad.Reutov
{
    /// <summary>Deterministic original synthwave loop; bundled by code so WebGL has no missing OGG assets.</summary>
    public static class ReutovSynthwave
    {
        private const int Rate = 22050;
        private const float Beat = 60f / 104f;
        private static AudioClip menuLoop;

        public static AudioClip GetMenuLoop()
        {
            if (menuLoop != null) return menuLoop;
            const int bars = 8;
            int frames = Mathf.RoundToInt(bars * 4f * Beat * Rate);
            float[] pcm = new float[frames];

            // Original chord progression: Cm - Ab - Eb - Bb, repeated with a melodic variation.
            int[][] chords = {
                new []{48, 51, 55}, new []{44, 48, 51},
                new []{51, 55, 58}, new []{46, 50, 53}
            };
            int[] bass = {36, 32, 39, 34};
            int[] melody = {72, 75, 79, 75, 70, 72, 75, 79,
                            68, 72, 75, 72, 67, 70, 72, 75,
                            70, 72, 79, 75, 77, 75, 72, 70,
                            74, 72, 70, 67, 70, 72, 75, 79};

            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4f * Beat;
                int progression = bar % 4;
                // Soft analog-like sustained chords behind the melody.
                for (int voice = 0; voice < 3; voice++)
                    AddTone(pcm, barStart, 4f * Beat - .06f,
                        chords[progression][voice] + 12, .030f, 2, .065f);

                for (int step = 0; step < 8; step++)
                {
                    float t = barStart + step * (Beat / 2f);
                    if (step % 2 == 0)
                        AddTone(pcm, t, Beat * .86f, bass[progression], .115f, 1, .015f);
                    int arpeggio = chords[progression][(step + bar) % 3] + 24;
                    AddTone(pcm, t, Beat * .39f, arpeggio, .041f, 2, .005f);
                    AddHat(pcm, t, .028f, step % 2 == 0 ? .024f : .017f);
                }

                for (int beat = 0; beat < 4; beat++)
                {
                    float at = barStart + beat * Beat;
                    if (beat == 0 || beat == 2) AddKick(pcm, at, .15f, .18f);
                    else AddSnare(pcm, at, .16f, .075f);
                    int leadNote = melody[bar * 4 + beat];
                    AddTone(pcm, at, Beat * .71f, leadNote, .061f, 3, .014f);
                }
            }

            // Soft saturation and safe output headroom; seam-free loop envelope.
            float max = 0f;
            for (int i = 0; i < frames; i++)
            {
                float signal = pcm[i];
                pcm[i] = signal / (1f + Mathf.Abs(signal) * .4f);
                max = Mathf.Max(max, Mathf.Abs(pcm[i]));
            }
            float level = max > .85f ? .85f / max : 1f;
            int fade = Mathf.Min(frames / 10, Rate / 14);
            for (int i = 0; i < frames; i++)
            {
                float envelope = 1f;
                if (i < fade) envelope = i / (float)fade;
                if (frames - i < fade) envelope = Mathf.Min(envelope, (frames - i) / (float)fade);
                pcm[i] *= envelope * level;
            }

            menuLoop = AudioClip.Create("Reutov 1996 | Synthwave Main Theme", frames, 1, Rate, false);
            menuLoop.SetData(pcm, 0);
            return menuLoop;
        }

        private static float Frequency(int midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69f) / 12f);
        }

        private static void AddTone(float[] pcm, float start, float duration, int note,
                                    float amp, int wave, float attack)
        {
            int startSample = Mathf.Max(0, Mathf.RoundToInt(start * Rate));
            int count = Mathf.Min(pcm.Length - startSample, Mathf.RoundToInt(duration * Rate));
            if (count <= 0) return;
            float f = Frequency(note);
            for (int n = 0; n < count; n++)
            {
                float t = n / (float)Rate;
                float phase = t * f;
                float x;
                if (wave == 1)
                    x = Mathf.Sin(6.2831853f * phase) * .8f +
                        Mathf.Sin(12.5663706f * phase) * .18f;
                else if (wave == 2)
                    x = Mathf.Sin(6.2831853f * phase) * .75f +
                        Mathf.Sin(18.849556f * phase) * .13f;
                else
                    x = Mathf.Sin(6.2831853f * phase) * .61f +
                        Mathf.Sin(12.5663706f * phase) * .24f +
                        Mathf.Sin(25.132741f * phase) * .10f;

                float env = Mathf.Min(1f, t / Mathf.Max(.001f, attack));
                env *= Mathf.Min(1f, (duration - t) / .065f);
                if (wave == 2) env *= Mathf.Exp(-1.3f * t);
                pcm[startSample + n] += amp * x * Mathf.Max(0f, env);
            }
        }

        private static void AddKick(float[] pcm, float start, float duration, float amp)
        {
            int idx = Mathf.RoundToInt(start * Rate);
            int count = Mathf.Min(pcm.Length - idx, Mathf.RoundToInt(duration * Rate));
            for (int n = 0; n < count; n++)
            {
                float t = n / (float)Rate;
                float phase = 2f * Mathf.PI * (52f * t + 22f * .038f *
                    (1f - Mathf.Exp(-t / .038f)));
                pcm[idx + n] += amp * Mathf.Sin(phase) * Mathf.Exp(-t * 24f);
            }
        }

        private static float Noise(int index)
        {
            unchecked
            {
                uint x = (uint)index * 747796405u + 2891336453u;
                x = ((x >> (int)((x >> 28) + 4)) ^ x) * 277803737u;
                return ((x >> 22) ^ x) * (1f / 2147483648f) - 1f;
            }
        }

        private static void AddSnare(float[] pcm, float start, float duration, float amp)
        {
            int idx = Mathf.RoundToInt(start * Rate);
            int count = Mathf.Min(pcm.Length - idx, Mathf.RoundToInt(duration * Rate));
            for (int n = 0; n < count; n++)
            {
                float t = n / (float)Rate;
                pcm[idx + n] += (Noise(idx + n) * .8f +
                    Mathf.Sin(t * 1200f) * .2f) * Mathf.Exp(-t * 28f) * amp;
            }
        }

        private static void AddHat(float[] pcm, float start, float duration, float amp)
        {
            int idx = Mathf.RoundToInt(start * Rate);
            int count = Mathf.Min(pcm.Length - idx, Mathf.RoundToInt(duration * Rate));
            for (int n = 0; n < count; n++)
            {
                float t = n / (float)Rate;
                float whitened = Noise(idx + n) - Noise(idx + n - 1) * .8f;
                pcm[idx + n] += whitened * Mathf.Exp(-t * 92f) * amp;
            }
        }
    }
}
