using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("行走参数")]
    [Tooltip("行走速度（单位：米/秒）")]
    public float walkSpeed = 4.5f; // 推荐值：4-6，模拟正常步行速度

    [Tooltip("转向平滑时间（数值越小转向越灵敏）")]
    public float turnSmoothTime = 0.1f; // 转向缓冲时间，使转向更自然

    [Header("引用设置")]
    [Tooltip("主相机（用于确定移动方向与视角一致）")]
    public Camera mainCamera;

    // 组件引用
    private CharacterController characterController;

    // 转向平滑相关变量
    private float turnSmoothVelocity;
    private Vector3 moveDirection;

    private void Start()
    {
        // 获取角色控制器组件（必选，用于处理移动和碰撞）
        characterController = GetComponent<CharacterController>();

        // 自动获取主相机（如果未手动指定）
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("场景中未找到主相机，移动方向将基于世界坐标系");
            }
        }
    }

    private void Update()
    {
        // 处理行走逻辑
        HandleWalking();
    }

    /// <summary>
    /// 处理WASD输入并实现行走功能
    /// </summary>
    private void HandleWalking()
    {
        // 获取WASD输入：
        // Horizontal对应A(左)和D(右)，值范围-1到1
        // Vertical对应W(前)和S(后)，值范围-1到1
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 构建输入方向向量（忽略Y轴，确保只有水平移动）
        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // 当有有效输入时（输入向量长度大于0.1，避免微小输入触发）
        if (inputDirection.magnitude >= 0.1f)
        {
            // 计算移动方向（与相机视角同步）
            CalculateMoveDirection(inputDirection);

            // 计算目标转向角度（使角色面向移动方向）
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            // 平滑转向（使用插值实现缓冲效果，避免瞬间转向）
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 执行移动（乘以速度和时间增量，确保不同帧率下移动速度一致）
            characterController.Move(moveDirection * walkSpeed * Time.deltaTime);
        }
        else
        {
            // 无输入时停止移动
            moveDirection = Vector3.zero;
        }
    }

    /// <summary>
    /// 根据输入方向和相机视角计算最终移动方向
    /// </summary>
    private void CalculateMoveDirection(Vector3 inputDir)
    {
        if (mainCamera != null)
        {
            // 1. 获取相机的前向和右向向量（用于计算基于视角的移动方向）
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            // 2. 忽略Y轴分量（防止移动时上下偏移）
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            // 3. 归一化向量（确保斜向移动速度与正向移动一致）
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 4. 计算最终移动方向（结合输入和相机方向）
            moveDirection = (cameraForward * inputDir.z + cameraRight * inputDir.x).normalized;
        }
        else
        {
            // 无相机时使用世界坐标系方向（前=Z轴正方向）
            moveDirection = inputDir;
        }
    }

    // 在Scene视图中绘制角色控制器范围（方便调试碰撞体大小）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        // 绘制角色控制器的碰撞范围
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * characterController.height / 2,
            new Vector3(
                characterController.radius * 2,
                characterController.height,
                characterController.radius * 2
            )
        );
    }
}
