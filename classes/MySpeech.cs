using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Windows.Media.TextFormatting;
using System.Threading;

namespace TxtReader
{
    internal class MySpeech
    {
        public SpeechSynthesizer speech;
        public string text;
        public int st, charPosition;
        VoiceInfo voiceInfo;
        string voice;
        int rate=0, volume=25;



        public MySpeech(string text)
        {
            this.text = text;
            speech = new SpeechSynthesizer();
            InstalledVoice[] vs = speech.GetInstalledVoices().ToArray();
            voice = vs[0].VoiceInfo.Name;
            st = 0;
        }

        public MySpeech(string text, string voice, int rate, int volume)
        {
            this.text = text;
            speech= new SpeechSynthesizer();
            setSpeech(voice, rate, volume);
            st = 0;
        }

        public static void speakOne(string text)
        {
            var speech = new SpeechSynthesizer();
            speech.SpeakAsync(text);
        }

        public static void speakOne(string text, int rate, int volume, string voice)
        {
            var speech = new SpeechSynthesizer();
            speech.Rate = rate;
            speech.Volume = volume;
            speech.SelectVoice(voice);
            speech.SpeakAsync(text);

        }

        public void setSpeech(string voice, int rate, int volume)
        {
            speech.Rate = this.rate = rate;
            speech.Volume = this.volume = volume;
            this.voice = voice;
            speech.SelectVoice(voice);
        }



        public void Speak()
        {
            speech.SpeakAsync(text[st..]);
        }

        public void ctrlTask()
        {
            switch (speech.State.ToString())
            {
                case "Paused": speech.Resume(); break;
                case "Speaking": speech.Pause(); break;
                case "Ready": Speak(); break;
            }
        }

        public bool IsSpeaking()
        {
            return speech.State.ToString() == "Speaking";
        }

        public void Cancel()
        {
            speech.SpeakAsyncCancelAll();
        }

        public void Jump(int L)
        {
            st += L + charPosition;
            st = Math.Max(st, 0);
            st = Math.Min(st, text.Length);
            speech.SpeakAsyncCancelAll(); 
            Speak();
        }


        public void setProp(string voice = null, int rate=-100, int volume = -100)
        {
            if(voice!=null)
                this.voice = voice;
            if (rate != -100)
                this.rate = rate;
            if (volume != -100)
                this.volume = volume;

            bool speaking = speech.State.ToString() == "Speaking";
            if (speaking)
            {
                st += charPosition;
                speech.SpeakAsyncCancelAll();
            }            
            setSpeech(this.voice, this.rate, this.volume);
            if(speaking)
                Speak();
        }



        public void setVoice(string v)
        {
            voice = v;
            if (speech.State.ToString() == "Speaking")
            {
                st += charPosition;
                speech.SpeakAsyncCancelAll();
                setSpeech(v, rate, volume);
                Speak();

            }
        }

        public void setVolume(int volume)
        {
            if (speech.State.ToString() == "Speaking")
            {
                this.volume = volume;
                st += charPosition;
                speech.SpeakAsyncCancelAll();
                setSpeech(voice, rate, volume);
                Speak();
            }
            else
            {
                this.volume = volume;
                setSpeech(voice, rate, volume);
            }

        }

        public void setRate(int rate)
        {
            this.rate = rate;
            st += charPosition;
            speech.SpeakAsyncCancelAll();
            setSpeech(voice, rate, volume);
            Speak();
        }

    }
}
