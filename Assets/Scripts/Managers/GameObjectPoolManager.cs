using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Mánager multicasillero. Genera y administra piscinas de memoria (ObjectPools)
/// de forma dinámica en base al Prefab que se le solicite.
/// </summary>
public class GameObjectPoolManager : MonoBehaviour
{
    public static GameObjectPoolManager Instance { get; private set; }

    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxPoolSize = 25;

    // Diccionario que vincula cada Prefab original con su piscina exclusiva en RAM
    private Dictionary<GameObject, ObjectPool<GameObject>> _poolsDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _poolsDict = new Dictionary<GameObject, ObjectPool<GameObject>>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawnea un prefab. Si es la primera vez que se pide este prefab, crea su piscina.
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        // Si no existe una piscina para este prefab, la fabricamos on-the-fly (Factory)
        if (!_poolsDict.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject instance = Instantiate(prefab, transform);
                    instance.SetActive(false);

                    // Le inyectamos al clon la "llave" para que sepa a qué piscina volver
                    PooledVFXReturner returner = instance.GetComponent<PooledVFXReturner>();
                    if (returner != null) returner.Setup(prefab);

                    return instance;
                },
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxPoolSize
            );

            _poolsDict[prefab] = pool;
        }

        // Sacamos el clon de su piscina correspondiente y lo ubicamos
        GameObject spawnedObj = pool.Get();
        spawnedObj.transform.SetPositionAndRotation(position, rotation);
        return spawnedObj;
    }

    /// <summary>
    /// Retorna una instancia a su piscina específica.
    /// </summary>
    public void ReleaseToPool(GameObject prefabKey, GameObject instance)
    {
        if (prefabKey != null && _poolsDict.TryGetValue(prefabKey, out ObjectPool<GameObject> pool))
        {
            pool.Release(instance);
        }
        else
        {
            // Fallback de seguridad por si la piscina fue destruida
            Destroy(instance);
        }
    }
}