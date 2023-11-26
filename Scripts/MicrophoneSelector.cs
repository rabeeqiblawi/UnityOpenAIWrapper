using UnityEngine;

public class MicrophoneSelector : MonoBehaviour
{
    [SerializeField, HideInInspector] private int selectedMicrophoneIndex = 0;

    public int SelectedMicrophoneIndex
    {
        get => selectedMicrophoneIndex;
        set => selectedMicrophoneIndex = value;
    }
}
