using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    public float rotationSpeed = 180f; // 회전 속도

    private Vector2 moveInput;

    void OnMove(InputValue value)
    {
        // Input System에서 Move 액션 입력 가져오기
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // "앞뒤" 이동 처리 (W/S 키)
        float moveForward = moveInput.y * moveSpeed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveForward);

        // "좌우" 회전 처리 (A/D 키)
        float rotateAmount = moveInput.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotateAmount);
    }
}

