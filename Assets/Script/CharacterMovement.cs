using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

public class CharacterMovement : MonoBehaviour
{
    private string serverUrl = "http://127.0.0.1:5001/observe_user_behavior";
    public float moveSpeed = 5f; // 이동 속도
    public float rotationSpeed = 180f; // 회전 속도

    private Vector2 moveInput;
    private Rigidbody rb; // Rigidbody 추가

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody 가져오기
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            moveInput = context.ReadValue<Vector2>(); // 입력 값 가져오기
        }
        else if (context.canceled) // 입력이 멈추면 0으로 초기화
        {
            moveInput = Vector2.zero;
        }
    }
    public void OnVoice(InputAction.CallbackContext context)
    {
        // 키를 눌렀을 때만 실행
        if (context.performed)
        {
            Debug.Log("Voice Action Triggered!");
            StartCoroutine(SendOptimizationRequest(5));  // 서버 요청
        }
    }

    void FixedUpdate()
    {
        // "앞뒤" 이동 처리 (W/S 키)
        Vector3 moveDirection = transform.forward * moveInput.y * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection); // Rigidbody를 통한 이동 처리

        // "좌우" 회전 처리 (A/D 키)
        float rotateAmount = moveInput.x * rotationSpeed * Time.fixedDeltaTime;
        Quaternion rotation = Quaternion.Euler(0f, rotateAmount, 0f);
        rb.MoveRotation(rb.rotation * rotation); // Rigidbody를 통한 회전 처리
    }

    IEnumerator SendOptimizationRequest(float x)
    {
        // JSON 데이터 생성
        string jsonData = JsonUtility.ToJson(new OptimizationRequest(x));

        // UnityWebRequest 설정
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(serverUrl, jsonData))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 요청 보내기
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
                // JSON 파싱
                OptimizationResponse response = JsonUtility.FromJson<OptimizationResponse>(request.downloadHandler.text);
                Debug.Log("Optimized Value: " + response.optimized_value);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    // 요청 데이터 클래스
    [System.Serializable]
    public class OptimizationRequest
    {
        public float x;

        public OptimizationRequest(float x)
        {
            this.x = x;
        }
    }

    // 응답 데이터 클래스
    [System.Serializable]
    public class OptimizationResponse
    {
        public float optimized_value;
    }
}
