using UnityEngine;

namespace Rabeeqiblawi.OpenAI.APIWrapper
{
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
                Debug.LogError("AudioSource component not found on the GameObject.");
            }

            // Check if the specified microphone index is valid
            if (MicIndex < 0 || MicIndex >= Microphone.devices.Length)
            {
                Debug.LogError("Invalid microphone index.");
                return;
            }

            // Optional: Log the name of the selected microphone
            Debug.Log("Selected Microphone: " + Microphone.devices[MicIndex]);
        }



        public void StartRecording()
        {
            // Start recording from the selected microphone, with a maximum duration of 10 seconds
            recordedClip = Microphone.Start(Microphone.devices[MicIndex], false, 10, 44100);
        }

        public void StopRecording(System.Action onSaved = null, bool play = false)
        {
            // Stop the microphone
            Microphone.End(Microphone.devices[MicIndex]);

            // Play the recorded clip
            if (recordedClip != null)
            {
                audioSource.clip = recordedClip;
                if (play)
                    audioSource.Play();

                // Save the recorded clip
                SaveRecordedAudio(onSaved);
            }
        }

        private void SaveRecordedAudio(System.Action onSaved = null)
        {
            // Modify the saving method accordingly
            audioSource.Save("record.wav", onSaved);
            Debug.Log("Audio saved to " + "/" + "record.wav");

        }
    }
}
