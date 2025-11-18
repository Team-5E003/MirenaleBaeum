using StarterAssets;
using UnityEngine;
using UnityEngine.AI;

public class Robot : MonoBehaviour
{
    FirstPersonController player;
    NavMeshAgent agent;

    // 플레이어 근처에서 멈출 거리
    public float stopDistance = 1.5f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        player = FindFirstObjectByType<FirstPersonController>();
    }

    void Update()
    {
        if (player == null) return;

        // 로봇이랑 플레이어 사이 거리
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= stopDistance)
        {
            // 충분히 가까우면 멈춤
            agent.isStopped = true;
        }
        else
        {
            // 멀면 다시 쫓아감
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
    }
}
