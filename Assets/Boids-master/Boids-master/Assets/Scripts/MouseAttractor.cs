using UnityEngine;

public class MouseAttractor : MonoBehaviour
{
    public Transform targetObject; // 拖入你作为目标的那个空物体
    public LayerMask clickMask;    // 设置为你服务器阵列所在的 Layer，或者地面的 Layer
    public float defaultDistance = 50f; // 如果没点到物体，目标点移动到距离相机多远的地方

    void Update()
    {
        if (Input.GetMouseButton(0))
        { // 鼠标左键按住时吸引
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 如果射线点到了服务器或者障碍物
            if (Physics.Raycast(ray, out hit, 500f, clickMask))
            {
                targetObject.position = hit.point;
            }
            else
            {
                // 如果没点到东西（比如点到了深海虚空），就让目标点停在相机前方固定距离
                targetObject.position = ray.GetPoint(defaultDistance);
            }
        }
    }
}