using UnityEngine;
using Rabeeqiblawi.OpenAI.APIWrapper;

public class Demo : MonoBehaviour
{
    OpenAIVoiceAPIWrapper openAICore;
    public AudioSource audioSource;
    AudioRecorder recorder;
    public string text = "Hello, ChatGPT!";
    Conversation conversation;

    public void Start()
    {
        openAICore = GetComponent<OpenAIVoiceAPIWrapper>();
        recorder = GetComponent<AudioRecorder>();
        conversation = new Conversation();
    }

    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
            recorder.StartRecording();
        if (Input.GetKeyUp(KeyCode.Space))
            recorder.StopRecording(() => Send());

        if (Input.GetKeyDown(KeyCode.T))
            Test();
    }

    void Send()
    {
        string filePath = Application.persistentDataPath + "/record.wav";
        string model = "whisper-1";

        openAICore.SendWhisperRequest(filePath, model, response =>
        {
            print(response);
            openAICore.SendTTSRequest(response, "alloy", audioClip =>
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            });
        });
    }

    void Test()
    {
        var chatGPTAPIWrapper = OPENAIManager.Instance.OpenAICHatGPTAPIWrapper;
        chatGPTAPIWrapper.AddMessageToConversation(text, conversation, responce =>
        {
            print(responce);
        });
    }
}




