using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // Keyboard.current 사용

public class PhoneBellPolling : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AudioSource ringSound;     // 루프 체크된 벨소리
    [SerializeField] private TextMeshProUGUI promptUI;  // "E 꾹 눌러서 끄기" 안내 (선택)

    [Header("Who & Where")]
    [SerializeField] private Transform player;          // 비우면 자동으로 찾음
    [SerializeField] private Camera playerCamera;       // 비우면 Camera.main
    [SerializeField] private float interactDistance = 2.0f;   // 상호작용 거리
    [SerializeField, Range(0f,1f)] private float lookDotThreshold = 0.6f; // 바라보는 각도 판정

    [Header("Ringing")]
    [SerializeField] private float minDelay = 15f; // 다음 울림 최소 대기
    [SerializeField] private float maxDelay = 45f; // 다음 울림 최대 대기
    [SerializeField] private float holdTime = 2.0f; // E키 꾹 누르는 시간

    private bool isRinging = false;
    private float nextRingTime = 0f;
    private float holdTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            var p = FindFirstObjectByType<Player>(); // 네가 쓰는 Player 스크립트
            if (p != null) player = p.transform;
            if (player == null && Camera.main != null) player = Camera.main.transform;
        }
        if (playerCamera == null) playerCamera = Camera.main;

        ScheduleNextRing();
        HidePrompt();
    }

    void Update()
    {
        // 1) 타이머로 랜덤 울림 시작 (폴링)
        if (!isRinging && Time.time >= nextRingTime)
            StartRinging();

        // 2) 울릴 때만 상호작용 처리 (폴링)
        if (isRinging && player != null)
        {
            bool inRange = Vector3.Distance(player.position, transform.position) <= interactDistance;
            bool looking = IsPlayerLookingAtPhone();

            if (inRange && looking)
            {
                // 프롬프트 & 남은 시간 표시
                ShowPrompt(true, Mathf.Max(0f, holdTime - holdTimer));

                // E키를 "누르고 있는 동안" 홀드 누적
                if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
                {
                    holdTimer += Time.deltaTime;
                    if (holdTimer >= holdTime)
                    {
                        StopRinging();
                    }
                }

                // 키에서 손 떼면 초기화
                if (Keyboard.current != null && Keyboard.current.eKey.wasReleasedThisFrame)
                {
                    holdTimer = 0f;
                }
            }
            else
            {
                // 시야/거리 벗어나면 홀드 취소 & 프롬프트 숨김
                holdTimer = 0f;
                HidePrompt();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    // ========== 내부 로직 ==========

    private void ScheduleNextRing()
    {
        float delay = Random.Range(minDelay, maxDelay);
        nextRingTime = Time.time + delay;
    }

    private void StartRinging()
    {
        isRinging = true;
        holdTimer = 0f;
        if (ringSound != null)
        {
            if (!ringSound.loop) ringSound.loop = true;
            ringSound.Play();
        }
        else
        {
            Debug.LogWarning("[Phone] AudioSource가 비었습니다.");
        }
    }

    private void StopRinging()
    {
        isRinging = false;
        holdTimer = 0f;
        if (ringSound != null) ringSound.Stop();
        HidePrompt();
        ScheduleNextRing(); // 다음 벨 예약
    }

    private bool IsPlayerLookingAtPhone()
    {
        if (playerCamera == null) return true; // 카메라 없으면 각도 체크 생략
        Vector3 toPhone = (transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(playerCamera.transform.forward, toPhone);
        return dot >= lookDotThreshold;
    }

    private void ShowPrompt(bool show, float remain = 0f)
    {
        if (promptUI == null) return;
        if (show)
        {
            promptUI.gameObject.SetActive(true);
            promptUI.text = $"Hold E to turn off ({remain:0.0}s)";
        }
        else HidePrompt();
    }

    private void HidePrompt()
    {
        if (promptUI != null) promptUI.gameObject.SetActive(false);
    }

    // 에디터 테스트용(원하면 버튼으로 연결)
    public void ForceRingNow()
    {
        nextRingTime = Time.time;
    }
}
