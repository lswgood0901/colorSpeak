using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHoverHandler : MonoBehaviour
{
    public LayerMask interactableLayer; // 감지할 오브젝트 레이어
    private Outline lastHoveredOutline; // 마지막으로 Hover한 오브젝트의 Outline

    void Start()
    {
        // 모든 Outline 초기화
        Outline[] outlines = FindObjectsOfType<Outline>();
        foreach (Outline outline in outlines)
        {
            outline.enabled = false; // 시작 시 모든 Outline 비활성화
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Raycast로 마우스 아래의 오브젝트 감지
        if (Physics.Raycast(ray, out hit, 100f, interactableLayer))
        {
            Outline outline = hit.collider.GetComponent<Outline>();
            if (outline != null)
            {
                // 새로운 오브젝트 Hover 시 Outline 활성화
                if (outline != lastHoveredOutline)
                {
                    RemoveOutline(); // 이전 Outline 제거
                    ApplyOutline(outline);
                }
            }
        }
        else
        {
            // Raycast가 오브젝트를 감지하지 못하면 Outline 제거
            RemoveOutline();
        }
    }

    void ApplyOutline(Outline outline)
    {
        outline.enabled = true; // Outline 활성화
        lastHoveredOutline = outline;
    }

    void RemoveOutline()
    {
        if (lastHoveredOutline != null)
        {
            lastHoveredOutline.enabled = false; // Outline 비활성화
            lastHoveredOutline = null;
        }
    }
}
