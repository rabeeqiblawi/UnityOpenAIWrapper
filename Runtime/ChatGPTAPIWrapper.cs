using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Rabeeqiblawi.OpenAI.Runtime
{
    public class Conversation
    {
        private List<JObject> messages = new List<JObject>();

        public void AddMessage(JObject message)
        {
            messages.Add(message);
        }

        public List<JObject> GetMessages()
        {
            return messages;
        }
    }

    public class ToolCallResult
    {
        public string FunctionName;
        public Dictionary<string, string> Parameters;
    }

    public enum APIMode
    {
        ChatCompletion,
        Responses
    }

    public class ChatGPTAPIWrapper : MonoBehaviour
    {
        private string _chatCompletionUrl = "https://api.openai.com/v1/chat/completions";
        private string _responsesUrl = "https://api.openai.com/v1/responses";
        
        public APIMode Mode = APIMode.Responses;

        private string _apiKey;
        public string _modelName = "gpt-5.1-chat-latest";
        private JObject _systemMessage = null;

        [SerializeField] private float _temperature = 0.7f;
        public float Temperature
        {
            get => _temperature;
            set => _temperature = Mathf.Clamp(value, 0.0f, 1.0f);
        }

        void Start()
        {
            _apiKey = OpenAIManager.Instance.ApiKey; // Make sure this manager is set up to provide the API key
        }

        public void SendRequest(string message, Action<string> response = null, List<OpenAITool> functions = null, Action<string> jsonResponse = null, Action<List<ToolCallResult>> toolsResponse = null, Action<string> onError = null, JObject response_format = null, AudioClip audioInput = null, Action<AudioClip> audioResponse = null)
        {
            StartCoroutine(SendRequestToChatGPT(message, functions: functions, conversationHistory: null, on_response_text: response, on_response_json: jsonResponse, on_response_function: toolsResponse, onError: onError, response_format: response_format, audioInput: audioInput, on_response_audio: audioResponse));
        }

        public void AddMessageToConversation(string userMessage, Conversation conversationHistory, Action<string> onresponce = null, Action<string> onjsonResponce = null, float? frequency_penalty = null, int? max_tokens = null, int? n = null, int? seed = null, float? top_p = null, string user = null, JObject response_format = null, AudioClip audioInput = null, Action<AudioClip> audioResponse = null)
        {
            StartCoroutine(SendRequestToChatGPT(userMessage, conversationHistory, on_response_text: onresponce, on_response_json: onjsonResponce, frequency_penalty: frequency_penalty, max_tokens: max_tokens, n: n, seed: seed, top_p: top_p, user: user, response_format: response_format, audioInput: audioInput, on_response_audio: audioResponse));
        }

        private JObject CreateRequestBody(
    string prompt,
    Conversation conversationHistory,
    List<OpenAITool> functions,
    float? frequency_penalty = null,
    JObject logit_bias = null,
    int? max_tokens = null,
    int? n = null,
    int? seed = null,
    float? top_p = null,
    string user = null,
    JObject response_format = null,
    AudioClip audioInput = null)
        {
            JArray messages;
            JObject userMessage;

            if (audioInput != null)
            {
                string base64Audio = ConvertAudioClipToBase64(audioInput);
                userMessage = new JObject(
                    new JProperty("role", "user"),
                    new JProperty("content", new JArray(
                        new JObject(new JProperty("type", "text"), new JProperty("text", prompt)),
                        new JObject(
                            new JProperty("type", "input_audio"), 
                            new JProperty("input_audio", new JObject(
                                new JProperty("data", base64Audio),
                                new JProperty("format", "wav")
                            ))
                        )
                    ))
                );
            }
            else
            {
                userMessage = new JObject(new JProperty("role", "user"), new JProperty("content", prompt));
            }

            if (conversationHistory != null)
            {
                conversationHistory.AddMessage(userMessage);
                messages = new JArray(conversationHistory.GetMessages());
            }
            else
            {
                messages = new JArray { userMessage };
            }

            if (_systemMessage != null)
            {
                messages.Add(_systemMessage);
            }

            JObject requestBody = new JObject(
                new JProperty("model", _modelName),
                new JProperty("temperature", Temperature)
            );

            if (Mode == APIMode.ChatCompletion)
            {
                requestBody.Add(new JProperty("messages", messages));
            }
            else
            {
                requestBody.Add(new JProperty("input", messages));
            }

            if (frequency_penalty != null)
                requestBody.Add(new JProperty("frequency_penalty", frequency_penalty));
            if (logit_bias != null)
                requestBody.Add(new JProperty("logit_bias", logit_bias));
            if (max_tokens != null)
                requestBody.Add(new JProperty("max_tokens", max_tokens));
            if (n != null)
                requestBody.Add(new JProperty("n", n));
            if (seed != null)
                requestBody.Add(new JProperty("seed", seed));
            if (top_p != null)
                requestBody.Add(new JProperty("top_p", top_p));
            if (user != null)
                requestBody.Add(new JProperty("user", user));
            if (response_format != null)
                requestBody.Add(new JProperty("response_format", response_format));

            if (functions != null && functions.Any())
            {
                JArray functionArray = new JArray();
                foreach (var function in functions)
                {
                    JObject functionObject = function.ToJson();
                    functionArray.Add(functionObject);
                }

                requestBody.Add(new JProperty("tools", functionArray));
                requestBody.Add(new JProperty("tool_choice", "auto"));
            }

            print(requestBody.ToString());
            return requestBody;
        }
        private IEnumerator SendRequestToChatGPT(string prompt, Conversation conversationHistory, float? frequency_penalty = null,
    JObject logit_bias = null,
    int? max_tokens = null,
    int? n = null,
    int? seed = null,
    float? top_p = null,
    string user = null, List<OpenAITool> functions = null, Action<string> on_response_text = null, Action<string> on_response_json = null, Action<List<ToolCallResult>> on_response_function = null, Action<string> onError = null, JObject response_format = null, AudioClip audioInput = null, Action<AudioClip> on_response_audio = null)
        {
            JObject requestBodyJson = CreateRequestBody(prompt, conversationHistory, functions, frequency_penalty, logit_bias, max_tokens, n, seed, top_p, user, response_format, audioInput);
            
            // If audio output is requested, add modalities
            if (on_response_audio != null)
            {
                requestBodyJson["modalities"] = new JArray("text", "audio");
                requestBodyJson["audio"] = new JObject(new JProperty("voice", "alloy"), new JProperty("format", "wav"));
            }

            string requestBody = requestBodyJson.ToString(Formatting.None);
            string url = Mode == APIMode.ChatCompletion ? _chatCompletionUrl : _responsesUrl;

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestBody);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", "Bearer " + _apiKey);

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onError.Invoke(webRequest.error);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    JObject jsonObject = JObject.Parse(jsonResponse);
                    string response = "";
                    if (Mode == APIMode.ChatCompletion)
                    {
                        response = jsonObject["choices"][0]["message"]["content"].ToString();
                    }
                    else
                    {
                        // Assuming Responses API returns 'output' object with 'content'
                        // Adjust this based on actual API response if different
                        if (jsonObject["output"] != null && jsonObject["output"]["content"] != null)
                        {
                            response = jsonObject["output"]["content"].ToString();
                        }
                        else if (jsonObject["choices"] != null) // Fallback if it still uses choices
                        {
                            response = jsonObject["choices"][0]["message"]["content"].ToString();
                        }
                        else
                        {
                            Debug.LogError("Unknown response structure: " + jsonResponse);
                        }
                    }

                    if (conversationHistory != null)
                        conversationHistory.AddMessage(new JObject(new JProperty("role", "assistant"), new JProperty("content", response)));
                    
                    if (on_response_function != null)
                    {
                        List<ToolCallResult> responseFunctions = ExtractToolCalls(jsonObject);
                        on_response_function.Invoke(responseFunctions);
                    }

                    if (on_response_audio != null)
                    {
                        string audioData = null;
                        if (Mode == APIMode.ChatCompletion)
                        {
                            audioData = jsonObject["choices"]?[0]?["message"]?["audio"]?["data"]?.ToString();
                        }
                        else
                        {
                            audioData = jsonObject["output"]?["audio"]?["data"]?.ToString();
                        }

                        if (!string.IsNullOrEmpty(audioData))
                        {
                            AudioClip clip = ConvertBase64ToAudioClip(audioData);
                            on_response_audio.Invoke(clip);
                        }
                    }

                    on_response_json?.Invoke(jsonResponse);
                    on_response_text?.Invoke(response);
                }
            }
        }

        private string ConvertAudioClipToBase64(AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            byte[] bytes = new byte[samples.Length * 2];
            int rescaleFactor = 32767;
            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(samples[i] * rescaleFactor);
                BitConverter.GetBytes(value).CopyTo(bytes, i * 2);
            }
            return Convert.ToBase64String(bytes);
        }

        private AudioClip ConvertBase64ToAudioClip(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            float[] samples = new float[bytes.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short value = BitConverter.ToInt16(bytes, i * 2);
                samples[i] = value / 32767f;
            }
            AudioClip clip = AudioClip.Create("GeneratedAudio", samples.Length, 1, 24000, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private List<ToolCallResult> ExtractToolCalls(JObject jsonObject)
        {
            JToken toolCalls = null;
            if (Mode == APIMode.ChatCompletion)
            {
                toolCalls = jsonObject["choices"]?[0]?["message"]?["tool_calls"];
            }
            else
            {
                // Assuming Responses API returns tool_calls in output
                toolCalls = jsonObject["output"]?["tool_calls"];
                if (toolCalls == null) // Fallback
                     toolCalls = jsonObject["choices"]?[0]?["message"]?["tool_calls"];
            }

            List<ToolCallResult> toolCallResults = new List<ToolCallResult>();

            if (toolCalls != null)
            {
                foreach (var toolCall in toolCalls)
                {
                    var functionName = toolCall["function"]["name"].ToString();
                    var arguments = JObject.Parse(toolCall["function"]["arguments"].ToString());

                    var parameters = new Dictionary<string, string>();
                    foreach (var arg in arguments)
                    {
                        parameters.Add(arg.Key, arg.Value.ToString());
                    }

                    toolCallResults.Add(new ToolCallResult
                    {
                        FunctionName = functionName,
                        Parameters = parameters
                    });
                }
            }

            return toolCallResults;
        }
    }
}