using System.Collections.Generic;
using UnityEngine;

public class EnemyProximityAnimator : MonoBehaviour
{
    [Header("玩家检测")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string playerObjectName = "玩家";
    [SerializeField] private float attackRange = 3f;

    [Header("动画配置")]
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private RuntimeAnimatorController attackController;

    private Animator animator;
    private AnimatorOverrideController idleController;
    private bool isAttacking;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{name} 缺少 Animator 组件，无法切换动画。");
            enabled = false;
            return;
        }

        if (attackController == null)
        {
            attackController = animator.runtimeAnimatorController;
        }

        BuildIdleController();
    }

    private void OnValidate()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (attackController == null && animator != null)
        {
            attackController = animator.runtimeAnimatorController;
        }

        BuildIdleController();
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindPlayer();
            if (player == null)
            {
                return;
            }
        }

        if (idleController == null && !BuildIdleController())
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool shouldAttack = distance <= attackRange;

        if (shouldAttack != isAttacking)
        {
            isAttacking = shouldAttack;
            animator.runtimeAnimatorController = isAttacking ? attackController : idleController;
        }
    }

    private Transform FindPlayer()
    {
        GameObject target = null;
        if (!string.IsNullOrEmpty(playerTag))
        {
            target = GameObject.FindWithTag(playerTag);
        }

        if (target == null && !string.IsNullOrEmpty(playerObjectName))
        {
            var found = GameObject.Find(playerObjectName);
            if (found != null)
            {
                target = found;
            }
        }

        return target != null ? target.transform : null;
    }

    private bool BuildIdleController()
    {
        if (attackController == null || idleClip == null)
        {
            return false;
        }

        idleController = new AnimatorOverrideController(attackController);
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        idleController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, idleClip);
        }

        idleController.ApplyOverrides(overrides);
        return true;
    }
}
