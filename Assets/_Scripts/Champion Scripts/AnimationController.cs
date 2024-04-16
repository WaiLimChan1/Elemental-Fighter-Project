using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    protected Animator Animator;

    public void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    public void ChangeAnimation(int statusNum) { Animator.SetInteger("Status", statusNum); }
    public int GetAnimatorStatus() { return Animator.GetInteger("Status"); }

    public float GetNormalizedTime() { return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime; }
    public void SetNormalizedTime(float normalizedTime) { Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, normalizedTime);  }

    public float GetSpeed() { return Animator.speed; }
    public void SetSpeed(float speed) { Animator.speed = speed; }

    public bool AnimationFinished() { return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f; }
    public void Flip(bool isFacingLeft) { GetComponent<SpriteRenderer>().flipX = isFacingLeft; }
    public void RestartAnimation() { Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0); }
}
