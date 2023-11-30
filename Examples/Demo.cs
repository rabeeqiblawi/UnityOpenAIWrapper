using Rabeeqiblawi.OpenAI.APIWrapper;
using System.Collections.Generic;
using UnityEngine;

namespace Rabeeqiblawi.OpenAI.Demo
{
    public class Demo : MonoBehaviour
    {
        public AudioRecorder recorder;
        public AudioSource audioSource;

        public ChatGPTAPIWrapper ChatGPT;
        public OpenAIVoiceAPIWrapper VoiceAPI;
        public List<OpenAITool> functions = new List<OpenAITool>();

        private void Start()
        {
            if (ChatGPT == null)
                ChatGPT = APIWrapper.OpenAI.Instance.ChatGPT;
            recorder = GetComponent<AudioRecorder>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Boom();
            }
        }

        public void Boom()
        {
            ChatGPT.SendRequest("Explode with force of 100", on_response_function: OnRecivedFormChatGPT, functions: functions);
        }

        void OnRecivedFormChatGPT(List<ToolCallResult> results)
        {
            foreach (var result in results)
            {
                print(result.FunctionName);
                foreach (var param in result.Parameters)
                {
                    print(param.Key + " : " + param.Value);
                }

            }
        }

        void OnRecivedFromTextToSpeach(AudioClip audioClip)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}