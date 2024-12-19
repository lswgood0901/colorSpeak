using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using Samples.Whisper;
using OpenAI;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System;
public class VoiceRecorder : MonoBehaviour
{
    private string serverUrl = "http://127.0.0.1:5001/upload_text";
    private AudioClip recordedClip;
    private bool isRecording = false;
    private readonly int recordDuration = 10; // 최대 녹음 시간 (초)
    private string resText = ""; // 변환된 텍스트 저장
    private string selectedDevice; // 선택된 오디오 장치
    private Vector3 currentColor = new Vector3(1, 1, 1);
    private List<Vector3> rgbList = new List<Vector3>(); // 서버에서 받은 RGB 리스트
    private string jsonFilePath;
    private int currentIndex = 0; // 현재 RGB 리스트 인덱스
    private GameObject currentObject; // 현재 JSON으로 읽어온 오브젝트
    private void Start()
    {
        // 사용 가능한 오디오 장치 확인 및 기본 장치 설정
        InitializeAudioDevice();
        jsonFilePath = Path.Combine(Application.persistentDataPath, "ClickedObjectData.json");
        Debug.Log("JSON file path: " + jsonFilePath);
    }
    private void InitializeAudioDevice()
    {
        if (Microphone.devices.Length > 0)
        {
            // 첫 번째 오디오 장치를 선택
            selectedDevice = Microphone.devices[0];
            Debug.Log("Default audio device selected: " + selectedDevice);

            // PlayerPrefs에 저장 (다음 실행 시 유지)
            PlayerPrefs.SetString("default-audio-device", selectedDevice);
        }
        else
        {
            Debug.LogError("No audio devices available!");
        }
    }
    public void OnVoice(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // V 키를 누르기 시작했을 때 녹음 시작
            StartRecording();
        }
        else if (context.canceled)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            Debug.Log("Recording started...");
            recordedClip = Microphone.Start(null, false, recordDuration, 44100);
        }
    }

    private async void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Debug.Log("Recording stopped.");

            // 녹음 종료
            Microphone.End(null);

            // WAV 데이터 생성
            byte[] wavData = SaveWav.Save("recording.wav", recordedClip);

            // OpenAI Whisper API로 변환 요청
            var openai = new OpenAIApi();
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData { Data = wavData, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);
            resText = res.Text;

            Debug.Log("Transcription result: " + resText);
            ClickedObjectData data = LoadDataFromJson();
            if (data != null)
            {
                Debug.Log("Loaded Object Name: " + data.objectName);
                Debug.Log("Loaded Material Name: " + data.materialName);
                Debug.Log($"Loaded Material Color: RGBA({data.materialColor[0]}, {data.materialColor[1]}, {data.materialColor[2]}, {data.materialColor[3]})");
                currentColor = new Vector3(data.materialColor[0], data.materialColor[1], data.materialColor[2]);
            }
            else
            {
                Debug.LogWarning("No JSON data found.");
            }
            
            // 서버로 텍스트 전송
            StartCoroutine(SendTextToServer(resText, currentColor));
        }
    }

    private IEnumerator SendTextToServer(string userText, Vector3 currentRGB)
{
    int[] intRGB = new int[]
    {
        Mathf.Clamp((int)(currentRGB.x * 255), 0, 255), // R
        Mathf.Clamp((int)(currentRGB.y * 255), 0, 255), // G
        Mathf.Clamp((int)(currentRGB.z * 255), 0, 255)  // B
    };


    var jsonData = new
    {
        text = userText,
        current_rgb = intRGB
    };
    string jsonString = JsonConvert.SerializeObject(jsonData);
    Debug.Log("Serialized JSON: " + jsonString);

    using (UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:5001/upload_text", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json"); // JSON 형식 명시

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response from server: " + request.downloadHandler.text);
                var responseData = JsonConvert.DeserializeObject<ResponseData>(request.downloadHandler.text);

                // suggested_rgb 리스트를 0~1 범위로 변환
                rgbList.Clear();
                foreach (var rgb in responseData.suggested_rgb)
                {
                    rgbList.Add(new Vector3(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f));
                }

                currentIndex = 0; // 리스트의 첫 번째 값을 초기화
                ApplyColorFromJson(); // 첫 번째 값을 적용

                }
            else
            {
                Debug.LogError("Failed to send text to server: " + request.error);
            }
        }
    }
    private void ApplyColorFromJson()
{
    // JSON 데이터 로드
    ClickedObjectData data = LoadDataFromJson();
    if (data == null)
    {
        Debug.LogWarning("Failed to load JSON data.");
        return;
    }

    // Scene에서 오브젝트 찾기
    GameObject targetObject = GameObject.Find(data.objectName);
    if (targetObject == null)
    {
        Debug.LogWarning($"Object with name {data.objectName} not found in the Scene.");
        return;
    }

    
    Vector3 rgbVector = rgbList[currentIndex];
    Color colorToApply = new Color(rgbVector.x, rgbVector.y, rgbVector.z);

    // 조건문: objectName이 "sofa"일 경우
    if (data.objectName.ToLower() == "sofa")
    {
        // 자식 오브젝트들까지 색상 변경
        ApplyColorToChildren(targetObject, colorToApply);
        Debug.Log($"Applied color {colorToApply} to {data.objectName} and its children.");
    }
    else
    {
        // 해당 오브젝트의 색상만 변경
        ApplyColorToSingleObject(targetObject, colorToApply);
        Debug.Log($"Applied color {colorToApply} to {data.objectName} only.");
    }
}
    private void ApplyColorToChildren(GameObject parentObject, Color color)
    {
        Renderer[] childRenderers = parentObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = color;
                Debug.Log($"Applied color {color} to {renderer.gameObject.name}");
            }
        }
    }
    private void ApplyColorToSingleObject(GameObject targetObject, Color color)
{
    if (targetObject.TryGetComponent<Renderer>(out Renderer renderer) && renderer.material != null)
    {
        renderer.material.color = color;
        Debug.Log($"Applied color {color} to {targetObject.name}");
    }
    else
    {
        Debug.LogWarning($"No Renderer or Material found on {targetObject.name}");
    }
}
    public void OnLeftButtonClick()
    {
        if (rgbList.Count > 0)
        {
            currentIndex = (currentIndex - 1 + rgbList.Count) % rgbList.Count; // 이전 색상으로 이동
            ApplyColorFromJson();
        }
    }

    public void OnRightButtonClick()
    {
        if (rgbList.Count > 0)
        {
            currentIndex = (currentIndex + 1) % rgbList.Count; // 다음 색상으로 이동
            ApplyColorFromJson();
        }
    }
    public void OnButton2Click()
    {
        Debug.Log("Optimize clicked!");

        // 현재 RGB 리스트 데이터를 서버로 전송하고 서버에서 최적화 결과 받기
        StartCoroutine(SendRGBListToServerAndUpdate());
    }
    
private IEnumerator SendRGBListToServerAndUpdate()
{
    if (rgbList.Count == 0)
    {
        Debug.LogWarning("RGB list is empty, nothing to optimize.");
        yield break;
    }

    List<int> chosen = new List<int>
    {
        Mathf.Clamp((int)(rgbList[currentIndex].x * 255), 0, 255), // R
        Mathf.Clamp((int)(rgbList[currentIndex].y * 255), 0, 255), // G
        Mathf.Clamp((int)(rgbList[currentIndex].z * 255), 0, 255)  // B
    };

    List<List<int>> others = new List<List<int>>();
    for (int i = 0; i < rgbList.Count; i++)
    {
        if (i == currentIndex) continue; // 현재 인덱스 제외
        others.Add(new List<int>
        {
            Mathf.Clamp((int)(rgbList[i].x * 255), 0, 255), // R
            Mathf.Clamp((int)(rgbList[i].y * 255), 0, 255), // G
            Mathf.Clamp((int)(rgbList[i].z * 255), 0, 255)  // B
        });
    }

    // JSON 데이터 생성
    var jsonData = new
    {
        chosen = chosen,
        others = others
    };
    string jsonString = JsonConvert.SerializeObject(jsonData);
    Debug.Log("Serialized RGB List JSON: " + jsonString);

    // 서버 요청
    using (UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:5001/observe_user_behavior", "POST"))
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response from server: " + request.downloadHandler.text);

            // 서버 응답 데이터 파싱
            try
            {
                var responseData = JsonConvert.DeserializeObject<ResponseData>(request.downloadHandler.text);

                // rgbList 업데이트
                rgbList.Clear();
                foreach (var rgb in responseData.suggested_rgb)
                {
                    rgbList.Add(new Vector3(
                        rgb[0], // R
                        rgb[1], // G
                        rgb[2]  // B
                    ));
                }

                Debug.Log("RGB list updated from server response.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse server response: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Failed to send RGB list to server: {request.error}");
        }
    }
}
    private ClickedObjectData LoadDataFromJson()
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            return JsonConvert.DeserializeObject<ClickedObjectData>(json);
        }
        else
        {
            Debug.LogWarning("JSON file not found: " + jsonFilePath);
            return null;
        }
    }
}

// 서버 응답 데이터 구조
[System.Serializable]
public class ResponseData
{
    public List<List<int>> suggested_rgb; // 0~255 범위의 RGB 값 리스트
}