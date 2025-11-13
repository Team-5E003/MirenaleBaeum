using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections; 

public class Player : MonoBehaviour
{
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private LayerMask itemLayerMask; //상호 작용가능하게 할 레이어 설정

    //연결할 텍스트 UI
    [SerializeField] private TextMeshProUGUI pickupPromptText;

    private Inventory inventory;
    // 현재 바라보고 있는 아이템을 저장할 변수
    private ItemPickup currentTargetItem;
    private DestroyableObject currentTargetDestroyable;
    public Camera playerCamera;

    private Coroutine messageCoroutine;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        inventory = GetComponent<Inventory>();
        if (pickupPromptText != null)
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (messageCoroutine == null)
        {
            CheckForInteraction();
        }

        // E키를 눌렀는지 확인
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // 1. E키를 눌렀고, 아이템을 보고 있었다면 줍기
            if (currentTargetItem != null)
            {
                AttemptPickup();
            }
            // 2. E키를 눌렀고, 파괴 가능 물체를 보고 있었다면 파괴 시도
            else if (currentTargetDestroyable != null)
            {
                AttemptDestroy();
            }
        }
    }

    private void CheckForInteraction()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;


        currentTargetItem = null;
        currentTargetDestroyable = null;


        if (Physics.Raycast(ray, out hit, pickupDistance, itemLayerMask))
        {
            // Raycast에 맞은 것이 ItemPickup인지 확인
            ItemPickup item = hit.collider.GetComponent<ItemPickup>();

            if (item != null)
            {
                currentTargetItem = item;
                ShowInteractionPrompt(true, $"E키를 눌러 {item.itemToPickup.itemName} 획득"); // UI 켜기
                return;
            }

            DestroyableObject destroyable = hit.collider.GetComponent<DestroyableObject>();
            if (destroyable != null)
            {
                currentTargetDestroyable = destroyable;
                // DestructibleObject의 설정값을 가져와 UI 표시
                ShowInteractionPrompt(true, $"E키를 눌러 {destroyable.ObjectName} {destroyable.ActionName}");
                return; // 파괴 가능 물체를 찾았으므로 함수 종료
            }
        }

        // Raycast에 아이템이 감지되지 않았다면
        currentTargetItem = null;
        ShowInteractionPrompt(false); // UI 끄기
    }

    // 아이템 줍기
    private void AttemptPickup()
    {
        if (currentTargetItem == null) return;

        bool success = inventory.AddItem(currentTargetItem.itemToPickup);

        if (success)
        {
            Destroy(currentTargetItem.gameObject);
            ShowInteractionPrompt(false);
        }
    }

    private void AttemptDestroy()
    {
        if (currentTargetDestroyable == null) return;

        // 실제 파괴 시도는 DestructibleObject가 스스로 하도록 호출
        // (인벤토리 정보를 넘겨줌)
        string message = currentTargetDestroyable.AttemptDestroy(inventory);
        if (message != null)
        {
            // 3. 메시지를 2초간 띄우는 코루틴을 시작
            ShowTemporaryMessage(message, 2.0f);
        }
        currentTargetDestroyable.AttemptDestroy(inventory);
    }

    private void ShowInteractionPrompt(bool show, string message = "")
    {
        if (pickupPromptText == null) return;

        if (show)
        {
            // 텍스트를 설정하고 UI를 켜기
            pickupPromptText.text = message;
            pickupPromptText.gameObject.SetActive(true);
        }
        else
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    //메시지를 N초간 띄우는 함수
    private void ShowTemporaryMessage(string message, float duration)
    {
        // 혹시 이전에 실행 중인 메시지가 있다면 중지
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        // 새 코루틴 시작
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        // 1. UI 텍스트 설정 및 활성화
        pickupPromptText.text = message;
        pickupPromptText.gameObject.SetActive(true);

        // 2. 지정된 시간(duration)만큼 대기
        yield return new WaitForSeconds(duration);

        // 3. UI 비활성화
        pickupPromptText.gameObject.SetActive(false);

        // 4. 코루틴이 끝났음을 표시 (다시 상호작용 UI가 나올 수 있게)
        messageCoroutine = null;
    }
}