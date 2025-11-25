using UnityEngine;

namespace Rabeeqiblawi.OpenAI.Runtime
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                OpenAIManager.Instance.VoiceServices.SendSTTRequest(model: "gpt-4o-transcribe", response: HandleResponseFromSTT);
            if (Input.GetKeyUp(KeyCode.Return))
                OpenAIManager.Instance.VoiceServices.StopRecoding();
        }

        private void HandleResponseFromSTT(WhisperResponse response)
        {
            if (response == null) return;

            string newRequset = response.text;
            Debug.Log($"Transcription: {newRequset}");
            
            if (response.usage != null)
            {
                Debug.Log($"Input Tokens: {response.usage.input_tokens}");
                Debug.Log($"Output Tokens: {response.usage.output_tokens}");
            }

            OpenAIManager.Instance.VoiceServices.SendTTSRequest(inputText: newRequset, model: "gpt-4o-mini-tts", response: HandleResponseFromTTS);
        }

        private void HandleResponseFromTTS(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
