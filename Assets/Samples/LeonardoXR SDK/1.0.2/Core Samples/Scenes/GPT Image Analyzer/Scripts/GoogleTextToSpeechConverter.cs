using UnityEngine.Networking;
using System.Collections;
using UnityEngine;
using System;

namespace LeonardoXR.Samples.DualCamera
{
    // テキストから音声に変換する
    [RequireComponent(typeof(AudioSource))]
    public class GoogleTextToSpeechConverter : MonoBehaviour
    {
        // Google Cloud PlatformのAPIキー
        [SerializeField] private string gcp_APIKey; // 「gcp_APIKey_Text」を設定済みの場合は必要無し

        [SerializeField] private TextAsset gcp_APIKey_Text;//　「gcp_APIKey」を設定済みの場合は必要無し
        private string URL;
        private AudioSource audioSource;// 音声出力用

        [System.Serializable]
        private class SynthesisInput
        {
            public string text;
        }

        [System.Serializable]
        private class VoiceSelectionParams
        {
            public string languageCode = "ja-JP";// 日本語の指定
            public string name;
        }

        [System.Serializable]
        private class AudioConfig
        {
            public string audioEncoding = "LINEAR16";
            public int speakingRate = 1;
            public int pitch = 0;
            public int sampleRateHertz = 16000;
        }

        [System.Serializable]
        private class SynthesisRequest
        {
            public SynthesisInput input;
            public VoiceSelectionParams voice;
            public AudioConfig audioConfig;
        }

        [System.Serializable]
        private class SynthesisResponse
        {
            public string audioContent;
        }


        private void Start()
        {
            audioSource = GetComponent<AudioSource>();

            if (gcp_APIKey_Text)// APIキーのファイルが設定されている場合は、そのAPIキーを使用する
            {
                gcp_APIKey = gcp_APIKey_Text.text;
            }
            if (gcp_APIKey == null)
            {
                return;
            }

            URL = "https://texttospeech.googleapis.com/v1/text:synthesize?key=" + gcp_APIKey;

            SynthesizeAndPlay("音声の出力機能は有効です");
        }

        public void SynthesizeAndPlay(string text)
        {
            StartCoroutine(Synthesize(text));
        }

        private IEnumerator Synthesize(string text)
        {
            SynthesisRequest requestData = new SynthesisRequest
            {
                input = new SynthesisInput { text = text },
                voice = new VoiceSelectionParams { languageCode = "ja-JP", name = "ja-JP-Neural2-B" },
                audioConfig = new AudioConfig { audioEncoding = "LINEAR16", speakingRate = 1, pitch = 0, sampleRateHertz = 16000 }
            };

            using (UnityWebRequest www = new UnityWebRequest(URL, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestData));
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string response = www.downloadHandler.text;
                    SynthesisResponse synthesisResponse = JsonUtility.FromJson<SynthesisResponse>(response);
                    PlayAudioFromBase64(synthesisResponse.audioContent);
                }
                else
                {
                    Debug.LogError("Google Text-to-Speech Error: " + www.error);
                }
            }
        }

        private void PlayAudioFromBase64(string base64AudioData)
        {
            byte[] audioBytes = System.Convert.FromBase64String(base64AudioData);
            LoadAudioClipAndPlay(audioBytes);
        }

        private void LoadAudioClipAndPlay(byte[] audioData)
        {
            int sampleRate = 16000;
            int channels = 1;

            int samplesCount = audioData.Length / 2;
            float[] audioFloatData = new float[samplesCount];

            for (int i = 0; i < samplesCount; i++)
            {
                short sampleInt = BitConverter.ToInt16(audioData, i * 2);
                audioFloatData[i] = sampleInt / 32768.0f;
            }

            AudioClip clip = AudioClip.Create("SynthesizedSpeech", samplesCount, channels, sampleRate, false);
            clip.SetData(audioFloatData, 0);

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
