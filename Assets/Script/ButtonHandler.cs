using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public VoiceRecorder voiceRecorder; // VoiceRecorder 스크립트 참조

    public void OnButton1Click()
    {
        if (voiceRecorder != null)
        {
            voiceRecorder.OnLeftButtonClick(); // VoiceRecorder의 메서드 호출
        }
        else
        {
            Debug.LogWarning("VoiceRecorder reference is not set in ButtonHandler.");
        }
    }

    public void OnButton2Click()
    {
        Debug.Log("Like clicked!");
        voiceRecorder.OnButton2Click();
    }

    public void OnButton3Click()
    {
        if (voiceRecorder != null)
        {
            voiceRecorder.OnRightButtonClick(); // VoiceRecorder의 메서드 호출
        }
        else
        {
            Debug.LogWarning("VoiceRecorder reference is not set in ButtonHandler.");
        }
    }
}