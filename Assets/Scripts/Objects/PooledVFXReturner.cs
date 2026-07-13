using UnityEngine;

/// <summary>
/// Autogestiona el ciclo de vida del VFX y lo devuelve al diccionario correcto del Pool.
/// </summary>
public class PooledVFXReturner : MonoBehaviour
{
    [SerializeField] private float _lifetime = 1.5f;

    private GameObject _prefabKey;

    /// <summary>
    /// Llamado automáticamente por el PoolManager al momento de Instanciar (solo 1 vez).
    /// </summary>
    public void Setup(GameObject prefabKey)
    {
        _prefabKey = prefabKey;
    }

    private void OnEnable() => Invoke(nameof(Release), _lifetime);

    private void OnDisable() => CancelInvoke(nameof(Release));

    private void Release()
    {
        if (_prefabKey != null && GameObjectPoolManager.Instance != null)
        {
            GameObjectPoolManager.Instance.ReleaseToPool(_prefabKey, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public GameObject GetPrefabKey()
    {
        return _prefabKey;
    }
}