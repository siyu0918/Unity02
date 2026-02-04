using UnityEngine;

public class WhaleFinalSwim : MonoBehaviour
{
    public float swimSpeed = 10f;
    public float lifeTime = 15f; // 15秒后彻底消失

    void Start()
    {
        // 确保动画已经播放
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. 从右往左移动（沿着世界坐标负X轴）
        transform.Translate(Vector3.left * swimSpeed * Time.deltaTime, Space.World);

        // 2. 让它面向左边
        transform.LookAt(transform.position + Vector3.left);

        // 3. 加一点点起伏感（12宫的深海律动）
        float yOffset = Mathf.Sin(Time.time) * 0.1f;
        transform.position += new Vector3(0, yOffset, 0);
    }
}