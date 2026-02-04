using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never, SelectedOnly, Always }

    //public Boid prefab;
    public Boid[] prefabs;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color colour;
    //public Gradient birdGradient;
    public GizmoType showSpawnRegion;
    public float minScale = 0.5f; // 最小缩放比例
    public float maxScale = 1.5f; // 最大缩放比例

    void Awake () {
        for (int i = 0; i < spawnCount; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            //Boid boid = Instantiate (prefab);
            Boid boid = Instantiate(prefabs[Random.Range(0, prefabs.Length)]);

            // --- 新增：随机大小 ---
            float randomScale = Random.Range(minScale, maxScale);
            boid.transform.localScale = Vector3.one * randomScale;
            // --------------------

            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;

            // --- 修改这里：不再使用 colour，而是使用随机 HSV 颜色 ---
            // 参数含义：色相(0-1循环全彩), 饱和度(高), 亮度(高)
            //if (birdGradient != null)
            //{
                // 从渐变色中随机抽取一个颜色
                //Color randomCol = birdGradient.Evaluate(Random.value);
                //boid.SetColour(randomCol);
            //}
            // ----------------------------------------------------
        }
    }

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {

        Gizmos.color = new Color (colour.r, colour.g, colour.b, 0.3f);
        Gizmos.DrawSphere (transform.position, spawnRadius);
    }

}