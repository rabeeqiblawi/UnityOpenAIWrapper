using UnityEngine;

namespace Rabeeqiblawi.OpenAI.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioRecorder : MonoBehaviour
    {
        private AudioSource audioSource;
        private bool isRecording = false;
        private AudioClip recordedClip;
        [SerializeField, HideInInspector] private int selectedMicrophoneIndex = 0;

        public int MicIndex
        {
            get => selectedMicrophoneIndex;
            set => selectedMicrophoneIndex = value;
        }

        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogError("Missing Audio Source");
                return;
            }

            if (MicIndex < 0 || MicIndex >= Microphone.devices.Length)
            {
                Debug.LogError("Invalid microphone index.");
                return;
            }
        }
        public void StartRecording()
        {
            recordedClip = Microphone.Start(Microphone.devices[MicIndex], false, 10, 44100);
        }

        public void StopRecording(System.Action onSaved = null, bool play = false)
        {
            Microphone.End(Microphone.devices[MicIndex]);
            if (recordedClip != null)
            {
                audioSource.clip = recordedClip;
                if (play)
                    audioSource.Play();
                SaveRecordedAudio(onSaved);
            }
        }

        private void SaveRecordedAudio(System.Action onSaved = null)
        {
            audioSource.Save("record.wav", onSaved);
            Debug.Log("Audio saved to " + "/" + "record.wav");
        }
    }
}