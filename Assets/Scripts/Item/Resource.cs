using UnityEngine;

public class Resource : MonoBehaviour
{
    public ItemData itemToGive; // 자원 채집 보상
    public int quantityPerHit = 1;  // 한번에 얻는 보상 수량
    public int capacity;    // 최대 보상 수량

    public void Gather(Vector3 hitPoint, Vector3 hitNormal)
    {
        // 한번에 얻는 보상 수량만큼 보상 아이템을 생성
        for(int i = 0; i < quantityPerHit; i++)
        {
            if (capacity <= 0) break;

            capacity -= 1;  // 최대 보상 수량 차감
            // 타격 지점보다 위에서 아이템 생성
            Instantiate(itemToGive.dropPrefab, hitPoint + Vector3.up, Quaternion.LookRotation(hitNormal, Vector3.up));
        }
        // 최대 보상 수량이 0보다 작거나 같으면 오브젝트 삭제
        if(capacity <= 0)
        {
            Destroy(gameObject);
        }
    }
}