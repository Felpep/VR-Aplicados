using UnityEngine;
using TMPro;


public class VRFPSCounter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text fpsText;

    [Header("Configuración")]
    [SerializeField] private float refreshRate = 0.5f;

    private float _timer;
    private int _frameCount;

   
    private static readonly string[] CachedFPSStrings = new string[151];

    private void Awake()
    {
        if (fpsText == null)
        {
            enabled = false;
            return;
        }

        
        fpsText.color = Color.green;

       
        for (int i = 0; i < CachedFPSStrings.Length; i++)
        {
            CachedFPSStrings[i] = $"FPS: {i}";
        }
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        _frameCount++;

        if (_timer >= refreshRate)
        {
            
            int fps = Mathf.RoundToInt(_frameCount / _timer);
            fps = Mathf.Clamp(fps, 0, 150); 

           
            fpsText.text = CachedFPSStrings[fps];

            
            _timer = 0f;
            _frameCount = 0;
        }
    }
}