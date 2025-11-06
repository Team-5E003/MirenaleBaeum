using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HorrorScreenFX : MonoBehaviour
{
    [Header("References")]
    public Transform player;      // 비워두면 자동으로 이 컴포넌트의 transform 사용
    public Volume volume;         // Global Volume 드래그
    public LayerMask enemyMask;   // Enemy 레이어 체크

    [Header("Effect Distance (m)")]
    public float dangerDistance = 30f;   // 이하면 최대효과
    public float maxDistance = 100f;     // 이상이면 효과 0
    public float smooth = 5f;           // 변화 부드러움

    [Header("Max Effect Strength")]
    [Range(0,1)] public float maxVignette = 0.6f;
    [Range(0,1)] public float maxRedTint  = 0.3f;

    // PP refs
    Vignette vignette;
    ColorAdjustments colorAdj;
    float k; // 0~1 현재 강도

    void Start()
    {
        // Player 지정 (비어있으면 자기 자신 transform 사용)
        if (!player) player = transform;

        // Volume에서 필요한 override 가져오기
        if (volume && volume.profile)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out colorAdj);
        }

        // 초기값은 항상 0/white
        if (vignette)  vignette.intensity.value   = 0f;
        if (colorAdj)  colorAdj.colorFilter.value = Color.white;
    }

    void Update()
    {
        Debug.Log("Enemy Distance: " + GetNearestEnemyDistance());

        float nearest = GetNearestEnemyDistance();

        // ✅ 임계값 방식: dangerDistance 바깥이면 완전 0, 안으로 들어오면 그때부터 0→1
        float target = 0f;
        if (nearest <= dangerDistance)
        {
            // 가장자리(dangerDistance)에서 0, 완전 근접(0m)에서 1
            target = Mathf.InverseLerp(maxDistance, dangerDistance, nearest);
        }

        // 부드럽게 전환
        k = Mathf.Lerp(k, target, Time.deltaTime * smooth);

        ApplyFX(k);
    }

    float GetNearestEnemyDistance()
    {
        Collider[] hits = Physics.OverlapSphere(player.position, maxDistance, enemyMask, QueryTriggerInteraction.Ignore);
        float nearest = Mathf.Infinity;
        foreach (var h in hits)
        {
            float d = Vector3.Distance(player.position, h.transform.position);
            if (d < nearest) nearest = d;
        }
        return nearest == Mathf.Infinity ? maxDistance + 1f : nearest;
    }


    void ApplyFX(float v)
    {
        if (vignette) vignette.intensity.value = maxVignette * v;
        if (colorAdj) colorAdj.colorFilter.value = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), maxRedTint * v);
    }

    void OnDrawGizmosSelected()
    {
        var p = player ? player.position : transform.position;
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(p, dangerDistance);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(p, maxDistance);
    }
}
