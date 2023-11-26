using Rabeeqiblawi.OpenAI.APIWrapper;
using UnityEngine;

public class Demo : MonoBehaviour
{
    public AudioRecorder recorder;
    public AudioSource audioSource;

    public ChatGPTAPIWrapper ChatGPT;
    public OpenAIVoiceAPIWrapper VoiceAPI;

    private void Start()
    {
        if (ChatGPT == null)
            ChatGPT = OPENAIManager.Instance.ChatGPT;
        recorder = GetComponent<AudioRecorder>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            recorder.StartRecording();
        if (Input.GetKeyUp(KeyCode.Space))
            recorder.StopRecording(play: false, onSaved: SendSTT);
    }

    void SendSTT()
    {
        OPENAIManager.Instance.OPenAIVoiceAPIController.SendWhisperRequest(Application.persistentDataPath + "/record.wav", "whisper-1", OnRecivedFromSpeachToText);
    }

    void OnRecivedFromSpeachToText(string response)
    {
        print(response);
        ChatGPT.SendMessage(response, onresponce: OnRecivedFormChatGPT);
    }

    void OnRecivedFormChatGPT(string response)
    {
        print(response);
        VoiceAPI.SendTTSRequest(response, "alloy", OnRecivedFromTextToSpeach);
    }

    void OnRecivedFromTextToSpeach(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}