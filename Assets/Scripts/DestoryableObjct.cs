using UnityEngine;

public class DestroyableObject: MonoBehaviour
{
    [Header("파괴 조건")]
    [SerializeField] private ItemData requiredItem; // 파괴하는 데 필요한 아이템 

    [Header("상호작용 텍스트")]
    [SerializeField] private string actionName = "자르기"; // UI에 표시될 행동 텍스트

    public string ObjectName => gameObject.name;
    public string ActionName => actionName;

    /// <param name="playerInventory">플레이어의 인벤토리</param>
    public string AttemptDestroy(Inventory playerInventory)
    {
        // 1. 플레이어 인벤토리에 필요한 아이템이 있는지 확인
        if (playerInventory.HasItem(requiredItem))
        {
            // 2. 성공
            Debug.Log(requiredItem.itemName + " 로 " + gameObject.name + " 를 잘랐다.");
            Destroy(gameObject);
            return null; // 성공했으므로 null 반환
        }
        else
        {
            // 3. 실패
            // Debug.Log 대신 실패 메시지를 반환
            return $"{requiredItem.itemName} (이)가 필요합니다.";
        }
    }
}