using UnityEngine;
using UnityEngine.Pool;

public class PooledObject : MonoBehaviour
{
    // 나를 관리하는 풀에 대한 참조 (출생 증명서)
    private IObjectPool<GameObject> _myPool;

    // 풀 매니저가 생성 시점에 이 함수를 통해 풀을 등록해줍니다.
    public void SetPool(IObjectPool<GameObject> pool)
    {
        _myPool = pool;
    }

    // 스스로 풀로 돌아가는 함수
    public void ReturnToPool()
    {
        if (_myPool != null)
        {
            _myPool.Release(gameObject);
        }
        else
        {
            // 혹시라도 풀 소속이 아니면 그냥 파괴 (방어적 코드)
            Destroy(gameObject);
        }
    }
}
