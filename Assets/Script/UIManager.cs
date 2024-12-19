using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 위한 네임스페이스

public class UIManager : MonoBehaviour
{
    public GameObject colorOptionPanel; // UI 창 (colorOptionPanel)
    public TextMeshProUGUI titleText; // Panel의 제목 (제목 TextMeshPro)

    void Start()
    {
        colorOptionPanel.SetActive(false); // 시작 시 Panel 비활성화
    }

    public void ShowPanel(GameObject clickedObject)
    {
        if (clickedObject != null)
        {
            // ObjectInfo 스크립트를 통해 오브젝트 제목 가져오기
            ObjectInfo objectInfo = clickedObject.GetComponent<ObjectInfo>();
            if (objectInfo != null)
            {
                titleText.text = objectInfo.objectTitle; // 패널 제목 설정
            }
            else
            {
                titleText.text = "Unknown Object"; // ObjectInfo가 없을 경우 기본값
            }
        }

        colorOptionPanel.SetActive(true); // 패널 활성화
    }

    public void HidePanel()
    {
        colorOptionPanel.SetActive(false); // Panel 비활성화
    }
}