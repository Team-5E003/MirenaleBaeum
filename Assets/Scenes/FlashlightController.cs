using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlightLight;
    private bool isOn = false;

    void Start()
    {
        // 시작 시 꺼진 상태
        flashlightLight.enabled = false;
    }

  void Update()
{
    if (Input.GetKeyDown(KeyCode.F))
    {
        isOn = !isOn;
        flashlightLight.enabled = isOn;
        Debug.Log("F 키 눌림, 현재 상태: " + isOn);
    }
}

}
