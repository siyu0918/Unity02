using UnityEngine;
using System.Collections.Generic; // 必须引用

public class ServerFlicker : MonoBehaviour
{
    private List<Material> serverMaterials = new List<Material>(); // 改为列表
    private int boidCount = 0;

    [Header("发光设置")]
    public Color emissionColor = Color.white;
    public float baseIntensity = 0.5f;
    public float activeIntensity = 5.0f;
    public float flickerSpeed = 20f;
    [Header("平滑设置")]
    public float fadeSpeed = 2f; // 熄灭的速度，越小越慢
    private float smoothedIntensity; // 用于记录当前的平滑亮度

    void Start()
    {
        // 抓取所有子物体的 Renderer
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            serverMaterials.Add(r.material);
        }

        if (serverMaterials.Count == 0)
        {
            Debug.LogError("没找到任何 Renderer！");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Boid>() != null)
        {
            boidCount++;
            Debug.Log("有一只鸟进来了！当前数量：" + boidCount); // 添加这行
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Boid>() != null)
        {
            boidCount--;
        }
    }

    // 在 ServerFlicker.cs 中添加
    public bool IsActive()
    {
        return boidCount > 0; // 如果有鸟在范围内，就认为处于激活尝试状态
    }

    void Update()
    {
        float targetIntensity;

        if (boidCount > 0)
        {
            // 1. 呼吸逻辑：在激活范围内产生平滑的正弦波
            float breath = (Mathf.Sin(Time.time * flickerSpeed) + 1f) / 2f;
            targetIntensity = Mathf.Lerp(baseIntensity, activeIntensity, breath);

            // 加入一点柏林噪声模拟电流不稳
            targetIntensity *= Mathf.PerlinNoise(Time.time * 0.5f, 0f) * 0.2f + 0.9f;
        }
        else
        {
            // 2. 鸟离开后，目标亮度回到基础值
            targetIntensity = baseIntensity;
        }

        // 核心：使用 Lerp 实现“缓动”效果
        // 无论从亮变暗，还是从暗变亮，都会有一个平滑过渡的过程
        smoothedIntensity = Mathf.Lerp(smoothedIntensity, targetIntensity, Time.deltaTime * fadeSpeed);

        // 应用到所有材质
        foreach (Material mat in serverMaterials)
        {
            // 使用白色乘以平滑后的强度
            mat.SetColor("_EmissionColor", emissionColor * smoothedIntensity);
            mat.EnableKeyword("_EMISSION");
        }
    }
}