

using UnityEngine;

public class PerfOverlay : MonoBehaviour
{
    private float _deltaTime;
    private GUIStyle _style;

    void Start()
    {
        _style = new GUIStyle();
        _style.fontSize = 28;
        _style.normal.textColor = Color.yellow;
    }

    void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        float ms = _deltaTime * 1000f;
        float fps = 1f / _deltaTime;
        int count = GameManager.Instance?.BulletManager?.ActiveBulletCount ?? 0;

        GUI.Label(new Rect(10, 10, 400, 120),
            $"FPS: {fps:0.}\nms: {ms:0.00}\nBullets: {count}",
            _style);
    }
}