using UnityEngine;

/// <summary>
/// Feedback de hover por emisión. Enciende el brillo del material cuando la mano
/// se acerca y lo apaga al alejarse. Sin shaders custom y sin instanciar materiales.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class ButtonHoverHighlight : MonoBehaviour
{
    [Header("Color del brillo en hover")]
    [SerializeField] private Color highlightColor = Color.cyan;
    [SerializeField] private float intensity = 2f; // qué tan fuerte brilla

    private Renderer cachedRenderer;
    private MaterialPropertyBlock block;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    /// <summary>Enchufar a WhenHover: la mano se acercó.</summary>
    public void Highlight()
    {
        cachedRenderer.GetPropertyBlock(block);
        block.SetColor(EmissionColorID, highlightColor * intensity);
        cachedRenderer.SetPropertyBlock(block);
    }

    /// <summary>Enchufar a WhenUnhover: la mano se alejó.</summary>
    public void Unhighlight()
    {
        cachedRenderer.GetPropertyBlock(block);
        block.SetColor(EmissionColorID, Color.black); // negro = sin emisión
        cachedRenderer.SetPropertyBlock(block);
    }
}