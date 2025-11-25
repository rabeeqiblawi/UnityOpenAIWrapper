

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.Profiling;

namespace Rabeeqiblawi.OpenAI.Runtime
{
    [RequireComponent(typeof(AudioRecorder))]
    // Note: For conversational audio (chatting with audio), consider using ChatGPTAPIWrapper with Multimodal Audio support.
    // This wrapper is best for standalone TTS/STT tasks.
    public class OpenAIVoiceAPIWrapper : MonoBehaviour
    {
        private string whisper_url = "https://api.openai.com/v1/audio/transcriptions";
        private string speach_url = "https://api.openai.com/v1/audio/speech";
        private string apiKey;
        AudioRecorder recorder;
        bool stoppedRecording = false;

        void Start()
        {
            apiKey = OpenAIManager.Instance.ApiKey;
            recorder = GetComponent<AudioRecorder>();
        }

        public void StopRecoding()
        {
            stoppedRecording = true;
        }

        public void SendSTTRequest(string model = "gpt-4o-transcribe", Action<string> response = null, bool autoRec = true, float recordTime = 5, string language="en")
        {
            string filePath = Application.persistentDataPath + "/record.wav";
            if (autoRec == false)
            {
                StartCoroutine(RecordingCoroutine(recordTime, autoRec, () => StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model, language, response))));
            }
            else
            {
                StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model,language, response));
            }
        }

        public void SendSTTRequest(string filePath, string model = "gpt-4o-transcribe", Action<string> onResponse = null, bool autoRec = true, float recordTime = 5, string language = "en")
        {
            if (autoRec == true)
            {
                StartCoroutine(RecordingCoroutine(recordTime, autoRecord: autoRec, () => StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model, language, onResponse))));
            }
            else
            {
                StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model,language, onResponse));
            }
        }

        public void SendSTTRequest(string model = "gpt-4o-transcribe", Action<WhisperResponse> response = null, bool autoRec = true, float recordTime = 5, string language = "en")
        {
            string filePath = Application.persistentDataPath + "/record.wav";
            if (autoRec == false)
            {
                StartCoroutine(RecordingCoroutine(recordTime, autoRec, () => StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model, language, response))));
            }
            else
            {
                StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model, language, response));
            }
        }

        public void SendSTTRequest(string filePath, string model = "gpt-4o-transcribe", Action<WhisperResponse> onResponse = null, bool autoRec = true, float recordTime = 5, string language = "en")
        {
            if (autoRec == true)
            {
                StartCoroutine(RecordingCoroutine(recordTime, autoRecord: autoRec, () => StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model, language, onResponse))));
            }
            else
            {
                StartCoroutine(SendSTTRequestRequestCoroutine(filePath, model, language, onResponse));
            }
        }

        public void SendTTSRequest(string inputText, string voice = "alloy", string model = "gpt-4o-mini-tts", Action<AudioClip> response = null)
        {
            StartCoroutine(SendTTSRequestCoroutine(inputText, voice, model, response));
        }

        private IEnumerator SendTTSRequestCoroutine(string inputText, string voice, string model, Action<AudioClip> onResponse)
        {
            JObject requestBodyJson = new JObject
            {
                { "model", model },
                { "voice", voice },
                { "input", inputText }
            };
            UnityWebRequest webRequest = CreateRequestBody(requestBodyJson);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
                Debug.LogError("Error: " + webRequest.error);
            else
            {
                byte[] audioData = webRequest.downloadHandler.data;
                string filePath = Application.persistentDataPath + "/speech.mp3"; // Assuming WAV format
                System.IO.File.WriteAllBytes(filePath, audioData);
                StartCoroutine(GetAudioClip((clip) =>
                {
                    onResponse?.Invoke(clip);
                }));
            }
        }

        private UnityWebRequest CreateRequestBody(JObject requestBodyJson)
        {
            string requestBody = requestBodyJson.ToString();
            print(requestBody);
            UnityWebRequest webRequest = new UnityWebRequest(speach_url, "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            return webRequest;
        }

        private IEnumerator SendSTTRequestRequestCoroutine(string filePath, string model,string language, Action<string> onResponse)
        {
            WWWForm form = new WWWForm();
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            form.AddBinaryData("file", fileData, System.IO.Path.GetFileName(filePath), "audio/mpeg");
            form.AddField("model", model);
            form.AddField("language", language);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(whisper_url, form))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onResponse?.Invoke(null);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    var textresp = JsonConvert.DeserializeObject<WhisperResponse>(jsonResponse);

                    onResponse?.Invoke(textresp.text);
                }
            }
        }

        private IEnumerator SendSTTRequestRequestCoroutine(string filePath, string model, string language, Action<WhisperResponse> onResponse)
        {
            WWWForm form = new WWWForm();
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            form.AddBinaryData("file", fileData, System.IO.Path.GetFileName(filePath), "audio/mpeg");
            form.AddField("model", model);
            form.AddField("language", language);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(whisper_url, form))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onResponse?.Invoke(null);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    var textresp = JsonConvert.DeserializeObject<WhisperResponse>(jsonResponse);

                    onResponse?.Invoke(textresp);
                }
            }
        }

        public static float[] ConvertByteToFloat(byte[] byteArray)
        {
            if (byteArray.Length % 4 != 0)
            {
                throw new ArgumentException("Byte array length must be a multiple of 4");
            }

            int floatCount = byteArray.Length / 4;
            float[] floatArray = new float[floatCount];

            for (int i = 0; i < floatCount; i++)
            {
                float value = BitConverter.ToSingle(byteArray, i * 4);
                floatArray[i] = value;
            }
            return floatArray;
        }

        IEnumerator GetAudioClip(Action<AudioClip> onLoaded)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + Application.persistentDataPath + "/speech.mp3", AudioType.MPEG))
            {
                yield return www.SendWebRequest();
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                onLoaded.Invoke(myClip);
            }
        }

        IEnumerator RecordingCoroutine(float time, bool autoRecord, Action onDoneRecording)
        {
            recorder.StartRecording();
            stoppedRecording = false;

            if (time > 0 && autoRecord == true)
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return new WaitUntil(() => stoppedRecording);
            }
            print("Stopped Recording");
            recorder.StopRecording(onSaved: () => { onDoneRecording.Invoke(); });
        }
    }


    public class WhisperResponse
    {
        public string text;
        public Usage usage;
    }

    public class Usage
    {
        public string type;
        public int input_tokens;
        public InputTokenDetails input_token_details;
        public int output_tokens;
        public int total_tokens;
    }

    public class InputTokenDetails
    {
        public int text_tokens;
        public int audio_tokens;
    }
}