using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ObjectClickHandler : MonoBehaviour
{
    public LayerMask interactableLayer; // 감지할 오브젝트 레이어
    public UIManager uiManager; // UIManager 참조
    private string filePath;
    void Start()
    {
        // JSON 파일 저장 경로 설정
        filePath = Path.Combine(Application.persistentDataPath, "ClickedObjectData.json");
        Debug.Log("JSON file path: " + filePath);
    }

    void Update()
    {
        if (uiManager != null && uiManager.isPanelActive)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, interactableLayer))
            {
                GameObject clickedObject = hit.collider.gameObject;
                Debug.Log("Clicked on: " + clickedObject.name);
                Renderer renderer = null;

                // 클릭한 오브젝트가 "sofa"일 경우 첫 번째 자식의 Renderer 가져오기
                if (clickedObject.name.ToLower() == "sofa")
                {
                    if (clickedObject.transform.childCount > 0)
                    {
                        Transform firstChild = clickedObject.transform.GetChild(0);
                        renderer = firstChild.GetComponent<Renderer>();
                    }
                }
                else
                {
                    // 다른 경우 클릭된 오브젝트의 Renderer 가져오기
                    renderer = clickedObject.GetComponent<Renderer>();
                }
                
                if (renderer != null && renderer.material != null)
                {
                    Material material = renderer.material;
                    string materialName = material.name.Replace(" (Instance)", "");
                    Color color = material.color;

                    // JSON 데이터 생성
                    ClickedObjectData data = new ClickedObjectData
                    {
                        objectName = clickedObject.name,
                        materialName = materialName,
                        materialColor = new float[] { color.r, color.g, color.b, color.a }
                    };

                    // JSON 데이터 저장
                    SaveDataToJson(data);

                    // UIManager를 통해 Panel 띄우기
                    if (uiManager != null)
                    {
                        uiManager.ShowPanel(clickedObject);
                    }
                }
                else
                {
                    Debug.LogWarning("Clicked object has no material!");
                }
            }
        }
    }

    // JSON 파일에 데이터 저장
    void SaveDataToJson(ClickedObjectData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Data saved to JSON: " + json);
    }

    // JSON 데이터 로드
    // public ClickedObjectData LoadDataFromJson()
    // {
    //     if (File.Exists(filePath))
    //     {
    //         string json = File.ReadAllText(filePath);
    //         return JsonUtility.FromJson<ClickedObjectData>(json);
    //     }
    //     Debug.LogWarning("JSON file not found!");
    //     return null;
    // }
    
}
// 클릭된 오브젝트 데이터를 저장할 클래스
[System.Serializable]
public class ClickedObjectData
{
    public string objectName;
    public string materialName;
    public float[] materialColor; // RGBA 값
}