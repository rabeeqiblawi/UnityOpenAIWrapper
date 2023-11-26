using UnityEngine;
using UnityEngine.Networking;
using Unity.Plastic.Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

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

    public class ToolCallResult
    {
        public string FunctionName;
        public Dictionary<string, string> Parameters;
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

        public void SendRequest(string message, List<OpenAITool> functions = null, Action<string> on_response_text = null, Action<string> on_response_josn = null, Action<List<ToolCallResult>> on_response_function = null)
        {
            StartCoroutine(SendRequestToChatGPT(message, functions: functions, conversationHistory: null, on_response_text: on_response_text, on_response_json: on_response_josn, on_response_function: on_response_function));
        }

        public void AddMessageToConversation(string userMessage, Conversation conversationHistory, Action<string> onresponce = null, Action<string> onjsonResponce = null)
        {
            StartCoroutine(SendRequestToChatGPT(userMessage, conversationHistory, on_response_text: onresponce, on_response_json: onjsonResponce));
        }

        private JObject CreateRequestBody(string prompt, Conversation conversationHistory, List<OpenAITool> functions)
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
                messages = new JArray { userMessage };
            }

            // Assuming 'systemMessage' and 'modelName' are defined elsewhere in your class
            if (systemMessage != null)
            {
                messages.Add(systemMessage);
            }

            JObject requestBody = new JObject(
                new JProperty("model", modelName),
                new JProperty("messages", messages),
                new JProperty("temperature", Temperature)
            );

            // Add functions if they are provided
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
        private IEnumerator SendRequestToChatGPT(string prompt, Conversation conversationHistory, List<OpenAITool> functions = null, Action<string> on_response_text = null, Action<string> on_response_json = null, Action<List<ToolCallResult>> on_response_function = null)
        {
            JObject requestBodyJson = CreateRequestBody(prompt, conversationHistory, functions);
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
                    if (on_response_function != null)
                    {
                        List<ToolCallResult> responseFunctions = ExtractToolCalls(jsonObject);
                        on_response_function.Invoke(responseFunctions);
                    }

                    on_response_json?.Invoke(jsonResponse);
                    on_response_text?.Invoke(response);
                }
            }
        }

        private List<ToolCallResult> ExtractToolCalls(JObject jsonObject)
        {
            var toolCalls = jsonObject["choices"][0]["message"]["tool_calls"];
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