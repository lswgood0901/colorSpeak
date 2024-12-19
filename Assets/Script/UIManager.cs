using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 위한 네임스페이스

public class UIManager : MonoBehaviour
{
    public GameObject colorOptionPanel; // UI 창 (colorOptionPanel)
    public TextMeshProUGUI titleText; // Panel의 제목 (제목 TextMeshPro)
    public bool isPanelActive = false; // UI 창 활성화 상태

    void Start()
    {
        colorOptionPanel.SetActive(false); // 시작 시 Panel 비활성화
    }

    public void ShowPanel(GameObject clickedObject)
    {
        if (clickedObject != null)
        {
            ObjectInfo objectInfo = clickedObject.GetComponent<ObjectInfo>();
            if (objectInfo != null)
            {
                Debug.Log("Object Title: " + objectInfo.objectTitle); // 디버그 메시지
                titleText.text = objectInfo.objectTitle; // Title Text에 오브젝트 이름 설정
            }
            else
            {
                Debug.LogWarning("ObjectInfo is missing on: " + clickedObject.name);
                titleText.text = "Unknown Object"; // 기본값 설정
            }
        }

        colorOptionPanel.SetActive(true); // Canvas 패널 활성화
        isPanelActive = true; // Panel 활성화 상태 업데이트
    }
    public void HidePanel()
    {
        colorOptionPanel.SetActive(false); // Panel 비활성화
        isPanelActive = false; // Panel 활성화 상태 업데이트
    }
}