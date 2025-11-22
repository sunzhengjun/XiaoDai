using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 相机 : MonoBehaviour
{
    [Header("鼠标灵敏度设置")]
    [Tooltip("水平方向鼠标灵敏度")]
    public float horizontalSensitivity = 100f;
    [Tooltip("垂直方向鼠标灵敏度")]
    public float verticalSensitivity = 100f;

    [Header("视角限制")]
    [Tooltip("最大仰视角度（正值）")]
    public float maxLookUpAngle = 80f;
    [Tooltip("最大俯视角度（负值）")]
    public float maxLookDownAngle = -80f;

    [Header("关联角色")]
    [Tooltip("需要跟随的角色 Transform（通常是玩家的根对象）")]
    public Transform playerBody;

    private float xRotation = 0f; // 垂直旋转角度（上下看）

    void Start()
    {
        // 锁定鼠标到屏幕中心并隐藏光标（增强沉浸感）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 如果未指定角色，默认使用父对象作为角色
        if (playerBody == null)
        {
            playerBody = transform.parent;
            if (playerBody == null)
            {
                Debug.LogWarning("未指定角色对象，请在Inspector中关联玩家角色");
            }
        }
    }

    void Update()
    {
        // 获取鼠标移动输入
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.deltaTime;

        // 计算垂直旋转角度（上下看），注意Y轴输入是反向的
        xRotation -= mouseY;
        // 限制垂直旋转角度（防止过度仰头或低头）
        xRotation = Mathf.Clamp(xRotation, maxLookDownAngle, maxLookUpAngle);

        // 应用垂直旋转（上下看）
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 如果有角色对象，应用水平旋转（左右看，让角色一起旋转）
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {
            // 如果没有角色对象，直接旋转相机（左右看）
            transform.Rotate(Vector3.up * mouseX);
        }
    }
}
