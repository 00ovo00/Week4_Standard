using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Vector2 curMovementInput;
    public LayerMask groundLayerMask;

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;  // 회전 범위 최소값
    public float maxXLook;  // 회전 범위 최대값
    private float camCurXRot;   // 현재 회전값
    public float lookSensitivity;   // 민감도

    private Vector2 mouseDelta;

    [HideInInspector]
    public bool canLook = true; // 인벤토리용 커서 활성화 여부
    // true면 카메라 회전, 인벤토리 비활성화, 커서 비활성화
    // false면 카메라 고정, 인벤토리 활성화, 커서 활성화

    private Rigidbody rigidbody;
    
    public Action inventory;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // 마우스 커서 보이지 않게 설정
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        // 인벤토리 비활성 상태일 때만 카메라 움직이도록 제한
        if (canLook)
        {
            CameraLook();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        // 키 처음 눌린 상태
        // if (context.phase == InputActionPhase.Started)
        // 계속해서 키 입력되므로 Performed 사용
        // 키 눌린 상태
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
        }
        // 키 눌리지 않은 상태
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;    // 이동 중지
        }
    }
    
    private void Move()
    {
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= moveSpeed;
        dir.y = rigidbody.velocity.y;   // 점프

        rigidbody.velocity = dir;
    }

    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    bool IsGrounded()
    {
        // 플레이어 기준 4방향 아래로 향하는 ray(책상 다리 형태)
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) +(transform.up * 0.01f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            // 바닥에 Raycast되면 
            if (Physics.Raycast(rays[i], 0.1f, groundLayerMask))
            {
                // 땅에 붙어있는 상태로 인식
                return true;
            }
        }
        // 공중에 떠있는 상태로 인식
        return false;
    }
    
    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        // 탭 키 눌렸으면
        if (callbackContext.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    public void ToggleCursor()
    {
        // 커서 상태 확인
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        // 락 상태이면(인벤토리 안열리고 커서 비활성화) 락 해제
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;  // 커서 활성화 여부 토글
    }
}