using System;
using System.Collections;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Rabeeqiblawi.OpenAI.Runtime
{
    public class OpenAIDalleAPIWrapper : MonoBehaviour
    {
        private string dalleUrl = "https://api.openai.com/v1/images/generations";
        private string editsUrl = "https://api.openai.com/v1/images/edits";
        private string variationsUrl = "https://api.openai.com/v1/images/variations";
        private string apiKey;

        void Start()
        {
            apiKey = OpenAIManager.Instance.ApiKey;
        }

        public void SendDalleRequest(string prompt, Action<Texture2D> onResponse, string model= "gpt-image-1", string size = "1024x1024", int n = 1)
        {
            StartCoroutine(SendDalleRequestCoroutine(prompt, n, size, model, onResponse));
        }

        public void SendEditRequest(Texture2D image, string prompt, Action<Texture2D> onResponse, string model = "dall-e-2", string size = "1024x1024", int n = 1)
        {
            StartCoroutine(SendEditRequestCoroutine(image, prompt, n, size, model, onResponse));
        }

        public void SendVariationRequest(Texture2D image, Action<Texture2D> onResponse, string model = "dall-e-2", string size = "1024x1024", int n = 1)
        {
            StartCoroutine(SendVariationRequestCoroutine(image, n, size, model, onResponse));
        }

        private IEnumerator SendDalleRequestCoroutine(string prompt, int n, string size, string model, Action<Texture2D> onResponse)
        {
            JObject requestBodyJson = new JObject
            {
                { "model", model },
                { "prompt", prompt },
                { "n", n },
                { "size", size }
            };

            UnityWebRequest webRequest = CreateRequestBody(requestBodyJson);
            yield return webRequest.SendWebRequest();

            HandleResponse(webRequest, onResponse);
        }

        private IEnumerator SendEditRequestCoroutine(Texture2D image, string prompt, int n, string size, string model, Action<Texture2D> onResponse)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData("image", image.EncodeToPNG(), "image.png", "image/png");
            form.AddField("prompt", prompt);
            form.AddField("n", n);
            form.AddField("size", size);
            form.AddField("model", model);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(editsUrl, form))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
                yield return webRequest.SendWebRequest();
                HandleResponse(webRequest, onResponse);
            }
        }

        private IEnumerator SendVariationRequestCoroutine(Texture2D image, int n, string size, string model, Action<Texture2D> onResponse)
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData("image", image.EncodeToPNG(), "image.png", "image/png");
            form.AddField("n", n);
            form.AddField("size", size);
            form.AddField("model", model);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(variationsUrl, form))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
                yield return webRequest.SendWebRequest();
                HandleResponse(webRequest, onResponse);
            }
        }

        private void HandleResponse(UnityWebRequest webRequest, Action<Texture2D> onResponse)
        {
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                onResponse?.Invoke(null);
            }
            else
            {
                JObject jsonResponse = JObject.Parse(webRequest.downloadHandler.text);
                
                if (jsonResponse["data"][0]["b64_json"] != null)
                {
                    string b64Json = jsonResponse["data"][0]["b64_json"].ToString();
                    Texture2D texture = Base64ToTexture(b64Json);
                    onResponse?.Invoke(texture);
                    SaveTextureToFile(texture, "GeneratedImage_" + DateTime.Now.Ticks);
                }
                else if (jsonResponse["data"][0]["url"] != null)
                {
                    string imageUrl = jsonResponse["data"][0]["url"].ToString();

                    StartCoroutine(GetTextureFromURL(imageUrl, texture =>
                    {
                        onResponse?.Invoke(texture);
                        SaveTextureToFile(texture, "GeneratedImage_" + DateTime.Now.Ticks);
                    }));
                }
                else
                {
                    Debug.LogError("No image data found in response.");
                    onResponse?.Invoke(null);
                }
            }
        }

        private Texture2D Base64ToTexture(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(bytes))
            {
                return texture;
            }
            else
            {
                Debug.LogError("Failed to load texture from base64.");
                return null;
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
