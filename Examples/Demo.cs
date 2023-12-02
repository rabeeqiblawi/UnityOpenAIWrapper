using UnityEngine;

namespace Rabeeqiblawi.OpenAI.APIWrapper
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                OpenAIManager.Instance.VoiceServices.SendSTTRequest(onResponse: HandleResponseFromTTS);
            if (Input.GetKeyUp(KeyCode.Return))
                OpenAIManager.Instance.VoiceServices.StopRecoding();
        }

        private void HandleResponseFromTTS(string tts_response)
        {
            string newRequset = tts_response;
            OpenAIManager.Instance.VoiceServices.SendTTSRequest(inputText: newRequset, onResponse: HandleResponseFromTTS);
        }

        private void HandleResponseFromTTS(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
