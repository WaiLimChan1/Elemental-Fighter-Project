using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;
using Fusion;
using UnityEngine.Windows;
using NanoSockets;

public class ChampionAnimationController : MonoBehaviour
{
    private Champion Champion;

    private void AnimationTriggerAttack() { Champion.AnimationTriggerAttack(); }
    private void AnimationTriggerCrowdControl() { Champion.AnimationTriggerCrowdControl(); }
    private void AnimationTriggerProjectileSpawn() { Champion.AnimationTriggerProjectileSpawn(); }
    private void AnimationTriggerMobility() { Champion.AnimationTriggerMobility(); }

    private Animator Animator;

    public void Awake()
    {
        Champion = GetComponentInParent<Champion>();
        Animator = GetComponent<Animator>();
    }

    public int GetAnimatorStatus() { return Animator.GetInteger("Status"); }

    public float GetNormalizedTime() { return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime; }
    public bool AnimationFinished() { return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f; }
    public void Flip(bool isFacingLeft) { GetComponent<SpriteRenderer>().flipX = isFacingLeft; }
    public void ChangeAnimation(Champion.Status status) { Animator.SetInteger("Status", (int)status); }
    public void GoToAnimationLastFrame() { Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0.98f); }
    public void RestartAnimation() { Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0); }
}
