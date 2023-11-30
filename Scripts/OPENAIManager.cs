using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
namespace Rabeeqiblawi.OpenAI.APIWrapper
{
    public class OpenAI: MonoBehaviour
    {
        public static OpenAI Instance { get; private set; }
        private ChatGPTAPIWrapper _chatGPT;
        public ChatGPTAPIWrapper ChatGPT
        {
            get
            {
                if (_chatGPT == null)
                {
                    _chatGPT = GetComponent<ChatGPTAPIWrapper>();
                    Assert.IsNotNull(_chatGPT, "OpenAICHatGPTAPIController not found");
                }
                return _chatGPT;
            }
        }
        private OpenAIVoiceAPIWrapper _voiceServices;
        public OpenAIVoiceAPIWrapper VoiceServices
        {
            get
            {
                if (_voiceServices == null)
                {
                    _voiceServices = GetComponent<OpenAIVoiceAPIWrapper>();
                    Assert.IsNotNull(_voiceServices, "OPenAIVoiceAPIController not found");
                }
                return _voiceServices;
            }
        }

        string filePath = Application.dataPath + "/openai_key.txt";
        [Header("Put your key in Assets/openai_key.txt")]
        public bool LoadKeyFromFile = false;
        public string ApiKey;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            if (LoadKeyFromFile == true)
                LoadKey();
        }
        private void LoadKey()
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                ApiKey = content;
            }
        }
    }
}