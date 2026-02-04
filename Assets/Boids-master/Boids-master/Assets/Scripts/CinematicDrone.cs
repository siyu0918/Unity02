using UnityEngine;

public class CinematicDrone : MonoBehaviour
{
    [Header("追踪目标")]
    public Transform target; // 建议拖入 Spawner

    [Header("飞行参数")]
    public float orbitSpeed = 0.15f;
    public float minHeight = 10f;       // 相机离地的最低高度
    public float heightRange = 30f;     // 在最低高度基础上往上飘多高
    public float distanceMin = 40f;
    public float distanceMax = 100f;
    public float zoomSpeed = 0.25f;

    [Header("平滑度")]
    public float smoothTime = 2.5f;

    private Vector3 currentVelocity;
    private float timer;

    void LateUpdate()
    {
        if (target == null) return;

        timer += Time.deltaTime;

        // 1. 距离计算 (拉远近景)
        float currentDistance = Mathf.Lerp(distanceMin, distanceMax, (Mathf.Sin(timer * zoomSpeed) + 1f) / 2f);

        // 2. 高度计算 (核心修改：确保始终高于地平线)
        // 使用 (Sin + 1) / 2 将波形从 [-1, 1] 转换为 [0, 1]
        float hFactor = (Mathf.Sin(timer * 0.4f) + 1f) / 2f;
        float currentHeight = target.position.y + minHeight + (hFactor * heightRange);

        // 3. 圆周位置计算
        float x = Mathf.Cos(timer * orbitSpeed) * currentDistance;
        float z = Mathf.Sin(timer * orbitSpeed) * currentDistance;
        Vector3 targetPosition = target.position + new Vector3(x, 0, z);
        targetPosition.y = currentHeight; // 强制应用计算出的高度

        // 4. 平滑移动与看向目标
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        Vector3 relativePos = target.position - transform.position;
        if (relativePos != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(relativePos);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 1.2f);
        }
    }
}