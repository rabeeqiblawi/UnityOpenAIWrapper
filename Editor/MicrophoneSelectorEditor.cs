using UnityEditor;
using UnityEngine;

namespace Rabeeqiblawi.OpenAI.APIWrapper
{
    [CustomEditor(typeof(MicrophoneSelector))]
    public class MicrophoneSelectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MicrophoneSelector microphoneSelector = (MicrophoneSelector)target;

            string[] microphoneOptions = Microphone.devices;
            int selectedIndex = microphoneSelector.SelectedMicrophoneIndex;

            selectedIndex = EditorGUILayout.Popup("Microphone", selectedIndex, microphoneOptions);

            if (selectedIndex != microphoneSelector.SelectedMicrophoneIndex)
            {
                Undo.RecordObject(microphoneSelector, "Changed Selected Microphone");
                microphoneSelector.SelectedMicrophoneIndex = selectedIndex;
            }
        }
    }
}