using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClickHandler : MonoBehaviour
{
    public LayerMask interactableLayer; // 감지할 오브젝트 레이어
    public UIManager uiManager; // UIManager 참조

    void Update()
    {
        if (uiManager != null && uiManager.isPanelActive)
        {
            return; // 아무 작업도 하지 않음
        }
        
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 위치에서 Ray 발사
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, interactableLayer))
            {
                GameObject clickedObject = hit.collider.gameObject; // 클릭된 오브젝트 가져오기
                Debug.Log("Clicked on: " + clickedObject.name);

                // UIManager를 통해 Panel 띄우기
                if (uiManager != null)
                {
                    uiManager.ShowPanel(clickedObject);
                }
            }
        }
    }
}