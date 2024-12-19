using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public void OnButton1Click()
    {
        Debug.Log("Go left clicked!");
    }

    public void OnButton2Click()
    {
        Debug.Log("Like clicked!");
    }

    public void OnButton3Click()
    {
        Debug.Log("Go right clicked!");
    }
}