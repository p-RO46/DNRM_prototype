using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<GameObject, IObjectPool<GameObject>> _pools =
        new Dictionary<GameObject, IObjectPool<GameObject>>();

    private void Awake() => Instance = this;

    public GameObject Get(GameObject prefab)
    {
        if (!_pools.ContainsKey(prefab))
        {
            _pools[prefab] = CreatePool(prefab);
        }

        return _pools[prefab].Get();
    }

    public void Release(GameObject prefab, GameObject instance)
    {
        if (_pools.ContainsKey(prefab))
        {
            _pools[prefab].Release(instance);
        }
    }

    private IObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        IObjectPool<GameObject> pool = null; // 아래에서 할당하기 위해 미리 선언

        pool = new ObjectPool<GameObject>(
        createFunc: () => {
            GameObject obj = Instantiate(prefab);
            // 생성 시점에 PooledObject 컴포넌트를 찾아서 풀 참조를 넣어줌
            var pooledScript = obj.GetComponent<PooledObject>();
            if (pooledScript == null) pooledScript = obj.AddComponent<PooledObject>();

            pooledScript.SetPool(pool); // "너의 주인은 이 풀이다"라고 지정
            return obj;
        },
        actionOnGet: (obj) => obj.SetActive(true),
        actionOnRelease: (obj) => obj.SetActive(false),
        actionOnDestroy: (obj) => Destroy(obj),
        maxSize: 50
    );

        return pool;
    }
}
