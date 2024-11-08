using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    public string GetInteractPrompt();
    public void OnInteract();
}

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f; // 상호작용 중인지 체크하는 빈도
    private float lastCheckTime;
    public float maxCheckDistance;
    public LayerMask layerMask;

    public GameObject curInteractGameObject;
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;
    private Camera camera;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        if(Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;
            
            // 스크린 기준 정중앙에서 ray 발사
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            // maxCheckDistance 내에 layerMask와 일치하여 raycast된 hit 정보 가져오기
            if(Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                // 현재 상호작용 중인 객체가 아니면(새로운 객체를 상호작용하는 경우)
                if(hit.collider.gameObject != curInteractGameObject)
                {
                    // 현재 상호작용 중인 객체 정보 갱신하고 프롬프트 텍스트 설정
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText();
                }
            }
            // raycast된 객체가 없으면
            else
            {
                // 현재 상호작용 중인 객체 null로 만들고 프롬프트의 텍스트 비활성화
                curInteractGameObject = null;
                curInteractable = null;
                promptText.gameObject.SetActive(false);
            }
        }
    }

    private void SetPromptText()
    {
        // 상호작용 텍스트 설정하고 활성화
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetInteractPrompt();
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        // E키가 눌렸고 상호작용 가능한 객체가 있을 때
        if(context.phase == InputActionPhase.Started && curInteractable != null)
        {
            // 상호작용하는 함수 호출(상호작용 실행) 후
            curInteractable.OnInteract();
            // 상호작용 중인 객체가 없는 상태로 reset
            curInteractGameObject = null;
            curInteractable = null;
            promptText.gameObject.SetActive(false);
        }
    }
}