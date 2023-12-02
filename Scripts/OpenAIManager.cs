using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
namespace Rabeeqiblawi.OpenAI.APIWrapper
{
    public class OpenAIManager : MonoBehaviour
    {
        public static OpenAIManager Instance { get; private set; }
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
                    Assert.IsNotNull(_voiceServices, "OPenAIVoiceAPIWrapper not found");
                }
                return _voiceServices;
            }
        }
        private OpenAIDalleAPIWrapper _AIDalle;
        public OpenAIDalleAPIWrapper AIDalle
        {
            get
            {
                if (_AIDalle == null)
                {
                    _AIDalle = GetComponent<OpenAIDalleAPIWrapper>();
                    Assert.IsNotNull(_AIDalle, "DalleAPI Wrapper not Found");
                }
                return _AIDalle;
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