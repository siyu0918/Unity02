using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    BoidSettings settings;
    float individualSpeedOffset; // 个体速度偏移量

    Animator anim;
    float individualAnimSpeedOffset; // 个体动画速度偏移

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;

    void Awake () {
        //material = transform.GetComponentInChildren<MeshRenderer> ().material;
        //cachedTransform = transform;
        // 将 MeshRenderer 改为 Renderer，这样它就能同时识别普通网格和带皮网格了
        Renderer rend = transform.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            material = rend.material;

            // 2. 核心技巧：获取基础贴图 (_BaseMap 是 URP 的标准命名)
            Texture baseTexture = material.GetTexture("_BaseMap");

            if (baseTexture != null)
            {
                // 3. 将基础贴图也赋值给自发光贴图
                material.SetTexture("_EmissionMap", baseTexture);

                // 4. 设置发光强度 (这里建议用 Color.white 作为倍数)
                // 你可以根据需要调整这个系数 (比如 1.5f 到 3.0f)
                float intensity = 2.0f;
                material.SetColor("_EmissionColor", Color.white * intensity);

                // 5. 必须开启自发光关键字
                material.EnableKeyword("_EMISSION");
            }
        }
        cachedTransform = transform;
        anim = GetComponentInChildren<Animator>();
    }

    public void Initialize (BoidSettings settings, Transform target) {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = Random.Range(settings.minSpeed, settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;

        individualSpeedOffset = Random.Range(-0.5f, 0.5f);

        if (anim != null)
        {
            // 2. 关键：绕过名字！获取当前正在播放的状态（不管它叫 Idle 还是 Run）
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // 3. 随机化起步时间，让 350 只鸟动作错开
            // 使用 fullPathHash 就不需要知道动画的名字叫什么
            anim.Play(stateInfo.fullPathHash, 0, Random.value);

            // 4. 给每只鸟一个独特的翅膀频率系数
            individualAnimSpeedOffset = Random.Range(0.8f, 1.2f);
            anim.speed = individualAnimSpeedOffset;
        }
    }

    public void SetColour (Color col) {
        if (material != null)
        {
            // 1. 设置基础颜色 (URP 里的属性名通常是 _BaseColor)
            material.SetColor("_BaseColor", col);

            // 2. 设置自发光颜色 (Emission)
            // 我们将原始颜色乘以一个强度系数（比如 5.0f），使其在 HDR 下产生发光效果
            float intensity = 1.0f;
            Color emissionColor = col * intensity;
            material.SetColor("_EmissionColor", emissionColor);

            // 3. 必须确保材质开启了自发光关键字，否则设置了颜色也不会亮
            material.EnableKeyword("_EMISSION");
        }
    }

    public void UpdateBoid () {
        Vector3 acceleration = Vector3.zero;

        if (target != null) {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        // --- 修改：限速时加入个体差异 ---
        float boidMinSpeed = settings.minSpeed + individualSpeedOffset;
        float boidMaxSpeed = settings.maxSpeed + individualSpeedOffset;
        speed = Mathf.Clamp(speed, boidMinSpeed, boidMaxSpeed);
        // ------------------------------
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;

        if (anim != null)
        {
            float currentSpeed = velocity.magnitude;

            // 5. 动画速度 = (当前飞行速度 / 最大速度) * 个体频率系数
            // 这样即使 44 种鸟 Controller 不同，它们也都会遵循这个物理规律
            float speedPercent = currentSpeed / settings.maxSpeed;

            // 使用 Lerp 平滑动画速度，范围在 0.7倍 到 1.5倍 之间
            anim.speed = Mathf.Lerp(0.7f, 1.5f, speedPercent) * individualAnimSpeedOffset;
        }
    }

    bool IsHeadingForCollision () {
        RaycastHit hit;
        if (Physics.SphereCast (position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask)) {
            return true;
        } else { }
        return false;
    }

    Vector3 ObstacleRays () {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = cachedTransform.TransformDirection (rayDirections[i]);
            Ray ray = new Ray (position, dir);
            if (!Physics.SphereCast (ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask)) {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards (Vector3 vector) {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

}