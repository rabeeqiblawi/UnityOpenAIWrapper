using System;
using System.Collections;
using System.IO; // Include System.IO for file operations
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Rabeeqiblawi.OpenAI.APIWrapper
{
    public class OpenAIDalleAPIWrapper : MonoBehaviour
    {
        private string dalleUrl = "https://api.openai.com/v1/images/generations";
        private string apiKey;

        void Start()
        {
            apiKey = OpenAIManager.Instance.ApiKey;
        }

        public void SendDalleRequest(string prompt, Action<Texture2D> onResponse, string model= "dall-e-3", string size = "1024x1024", int n = 1)
        {
            StartCoroutine(SendDalleRequestCoroutine(prompt, n, size, onResponse));
        }

        private IEnumerator SendDalleRequestCoroutine(string prompt, int n, string size, Action<Texture2D> onResponse)
        {
            JObject requestBodyJson = new JObject
            {
                { "model", "dall-e-3" },
                { "prompt", prompt },
                { "n", n },
                { "size", size }
            };

            UnityWebRequest webRequest = CreateRequestBody(requestBodyJson);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                onResponse?.Invoke(null);
            }
            else
            {
                // Parse the response and get the image URL
                JObject jsonResponse = JObject.Parse(webRequest.downloadHandler.text);
                string imageUrl = jsonResponse["data"][0]["url"].ToString();

                StartCoroutine(GetTextureFromURL(imageUrl, texture =>
                {
                    onResponse?.Invoke(texture);
                    SaveTextureToFile(texture, "GeneratedImage"); // Save image to file
                }));
            }
        }

        private UnityWebRequest CreateRequestBody(JObject requestBodyJson)
        {
            string requestBody = requestBodyJson.ToString();
            UnityWebRequest webRequest = new UnityWebRequest(dalleUrl, "POST");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            return webRequest;
        }

        private IEnumerator GetTextureFromURL(string url, Action<Texture2D> onLoaded)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error downloading image: " + www.error);
                    onLoaded?.Invoke(null);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    onLoaded?.Invoke(texture);
                }
            }
        }

        private void SaveTextureToFile(Texture2D texture, string fileName)
        {
            byte[] bytes = texture.EncodeToPNG();
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".png");
            File.WriteAllBytes(filePath, bytes);
            Debug.Log("Saved image to " + filePath);
        }
    }
}
