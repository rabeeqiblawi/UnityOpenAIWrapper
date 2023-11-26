using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
namespace Rabeeqiblawi.OpenAI.APIWrapper
{
    public class OPENAIManager : MonoBehaviour
    {
        public static OPENAIManager Instance { get; private set; }
        private ChatGPTAPIWrapper _openAICHatGPTAPIController;
        public ChatGPTAPIWrapper OpenAICHatGPTAPIWrapper
        {
            get
            {
                if (_openAICHatGPTAPIController == null)
                {
                    _openAICHatGPTAPIController = GetComponent<ChatGPTAPIWrapper>();
                    Assert.IsNotNull(_openAICHatGPTAPIController, "OpenAICHatGPTAPIController not found");
                }
                return _openAICHatGPTAPIController;
            }
        }
        private OpenAIVoiceAPIWrapper _oPenAIVoiceAPIController;
        public OpenAIVoiceAPIWrapper OPenAIVoiceAPIController
        {
            get
            {
                if (_oPenAIVoiceAPIController == null)
                {
                    _oPenAIVoiceAPIController = GetComponent<OpenAIVoiceAPIWrapper>();
                    Assert.IsNotNull(_oPenAIVoiceAPIController, "OPenAIVoiceAPIController not found");
                }
                return _oPenAIVoiceAPIController;
            }
        }

        string filePath = Application.dataPath + "/openai_key.txt";
        [Header("Put your key in Assets/openai_key.txt")]
        public bool loadKeyFromFile = false;
        public string apiKey;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (loadKeyFromFile == true)
                LoadKey();
        }

        private void LoadKey()
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                apiKey = content;
            }
        }
    }
}