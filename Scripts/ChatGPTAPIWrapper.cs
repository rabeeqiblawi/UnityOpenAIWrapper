using UnityEngine;
using UnityEngine.Networking;
using Unity.Plastic.Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
namespace Rabeeqiblawi.OpenAI.APIWrapper
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

    public class ChatGPTAPIWrapper : MonoBehaviour
    {
        private string baseOpenAIUrl = "https://api.openai.com/v1/chat/completions";
        private string apiKey;
        public string modelName = "gpt-3.5-turbo";
        private JObject systemMessage = null;
        [SerializeField] private float temperature = 0.7f;
        public float Temperature
        {
            get
            {
                return temperature;
            }
            set
            {
                temperature = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        void Start()
        {
            apiKey = OPENAIManager.Instance.apiKey; // Make sure this manager is set up to provide the API key
        }

        public void SendMessage(string message, Action<string> onresponce = null, Action<string> onjsonResponce = null)
        {
            StartCoroutine(SendRequestToChatGPT(message, conversationHistory: null, onresponse: onresponce, onjsonResponse: onjsonResponce));
        }

        public void AddMessageToConversation(string userMessage, Conversation conversationHistory, Action<string> onresponce = null, Action<string> onjsonResponce = null)
        {
            StartCoroutine(SendRequestToChatGPT(userMessage, conversationHistory, onresponse: onresponce, onjsonResponse: onjsonResponce));
        }

        private JObject CreateRequestBody(string prompt, Conversation conversationHistory)
        {
            JArray messages;
            JObject userMessage = new JObject(new JProperty("role", "user"), new JProperty("content", prompt));
            if (conversationHistory != null)
            {
                conversationHistory.AddMessage(userMessage);
                messages = new JArray(conversationHistory.GetMessages());
            }
            else
            {
                messages = new JArray
                {
                    userMessage
                };
            }


            if (systemMessage != null)
            {
                messages.Add(systemMessage);
            }

            return new JObject(new JProperty("model", modelName), new JProperty("messages", messages), new JProperty("temperature", Temperature));
        }

        private IEnumerator SendRequestToChatGPT(string prompt, Conversation conversationHistory, Action<string> onresponse = null, Action<string> onjsonResponse = null)
        {
            JObject requestBodyJson = CreateRequestBody(prompt, conversationHistory);
            string requestBody = requestBodyJson.ToString(Unity.Plastic.Newtonsoft.Json.Formatting.None);

            using (UnityWebRequest webRequest = new UnityWebRequest(baseOpenAIUrl, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestBody);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + webRequest.error);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    JObject jsonObject = JObject.Parse(jsonResponse);
                    string response = jsonObject["choices"][0]["message"]["content"].ToString();

                    if (conversationHistory != null)
                        conversationHistory.AddMessage(new JObject(new JProperty("role", "assistant"), new JProperty("content", response)));

                    onjsonResponse?.Invoke(jsonResponse);
                    onresponse?.Invoke(response);
                }
            }
        }
    }
}