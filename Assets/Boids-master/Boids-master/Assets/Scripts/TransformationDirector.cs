using UnityEngine;
using System.Collections;

public class TransformationDirector : MonoBehaviour
{
    [Header("引用组件")]
    public GameObject serverGroup;      // 包含服务器和骨骼的父物体
    public GameObject explosionFX;      // 爆炸粒子预制体
    public GameObject finalWhale;       // 初始隐藏的鲸鱼物体
    public ServerFlicker serverScript;  // 父物体上的闪烁脚本
    public Transform whaleSpawnPoint;   // 自定义的鲸鱼出生点

    [Header("时间与渐入设置")]
    public float requiredTime = 5f;      // 长按时长
    public float appearanceDelay = 2.0f; // 爆炸后等待多久才开始显现
    public float fadeDuration = 3.0f;    // 渐入时长

    [Header("爆炸位置标记点")]
    public Transform serverPoint; // 服务器位置
    public Transform headPoint;   // 头部位置
    public Transform tailPoint;   // 尾部位置

    private float timer = 0f;
    private bool hasExploded = false;

    void Update()
    {
        // 只有没爆炸过、且按住鼠标、且鸟群在范围内才计时
        if (!hasExploded && serverScript != null && Input.GetMouseButton(0) && serverScript.IsActive())
        {
            timer += Time.deltaTime;
            if (timer >= requiredTime)
            {
                StartCoroutine(PerformFullTransformation());
            }
        }
        else
        {
            timer = Mathf.Max(0, timer - Time.deltaTime);
        }
    }

    IEnumerator PerformFullTransformation()
    {
        hasExploded = true;

        // 1. 【立即执行】触发三重爆炸
        if (explosionFX != null)
        {
            Instantiate(explosionFX, serverPoint.position, Quaternion.identity);
            Instantiate(explosionFX, headPoint.position, Quaternion.identity);
            Instantiate(explosionFX, tailPoint.position, Quaternion.identity);
        }

        // 2. 【关键修改：立即消失】不再等待，爆炸瞬间就关掉旧物体
        // 这样在火光四溅的时候，服务器和残骸就已经不见了
        if (serverGroup != null) serverGroup.SetActive(false);

        // 3. 【进入留白期】这时场景里只有飘散的爆炸粒子
        yield return new WaitForSeconds(appearanceDelay);

        // 4. 【开始渐入】鲸鱼开始显影
        yield return StartCoroutine(FadeInWhaleEffect());
    }

    IEnumerator FadeInWhaleEffect()
    {
        finalWhale.SetActive(true);
        finalWhale.transform.position = whaleSpawnPoint.position;
        finalWhale.transform.rotation = whaleSpawnPoint.rotation;

        // 获取材质并设置为 0 透明度
        Renderer whaleRenderer = finalWhale.GetComponentInChildren<Renderer>();
        if (whaleRenderer != null)
        {
            Material mat = whaleRenderer.material;
            // 注意：URP 材质通常使用 _BaseColor
            Color baseCol = mat.GetColor("_BaseColor");

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

                // 应用透明度
                mat.SetColor("_BaseColor", new Color(baseCol.r, baseCol.g, baseCol.b, alpha));
                yield return null;
            }
        }

        // 5. 显现完成后，赋予游泳动力
        if (finalWhale.GetComponent<WhaleFinalSwim>() == null)
        {
            finalWhale.AddComponent<WhaleFinalSwim>();
        }
    }
}