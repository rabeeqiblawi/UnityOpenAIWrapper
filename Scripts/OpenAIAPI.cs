using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class OpenAIAPI : MonoBehaviour
{
    // Replace with your actual OpenAI API key
    private string openAIKey = "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";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SendRequest());
    }

    IEnumerator SendRequest()
    {
        string url = "https://api.openai.com/v1/audio/speech";
        string jsonData = "{\"model\": \"tts-1\", \"input\": \"The quick brown fox jumped over the lazy dog.\", \"voice\": \"alloy\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openAIKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Handle the received mp3 file
            // For example, save it as an MP3 file
            System.IO.File.WriteAllBytes("speech.mp3", request.downloadHandler.data);
            Debug.Log("Speech synthesized and saved as speech.mp3");
        }
    }
}
