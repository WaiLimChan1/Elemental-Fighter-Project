using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using Unity.VisualScripting;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.UIElements;

public class Champion : NetworkBehaviour, IBeforeUpdate
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    public enum Status
    {
        IDLE, RUN,
        ROLL,
        JUMP_UP, JUMP_DOWN,
        AIR_ATTACK,
        ATTACK1, ATTACK2, ATTACK3, SPECIAL_ATTACK,
        BEGIN_DEFEND, DEFEND,
        TAKE_HIT,
        BEGIN_DEATH, FINISHED_DEATH,
        UNIQUE1, UNIQUE2, UNIQUE3
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    private ResourceBar ResourceBar;
    protected ChampionAnimationController ChampionAnimationController;
    protected Rigidbody2D Rigid;
    protected CapsuleCollider2D Collider;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Player Name
    [Networked(OnChanged = nameof(OnNicknameChanged))] private NetworkString<_8> playerName { get; set; }
    private static void OnNicknameChanged(Changed<Champion> changed) { changed.Behaviour.SetPlayerNickName(changed.Behaviour.playerName); }
    private void SetPlayerNickName(NetworkString<_8> nickName) { ResourceBar.SetPlayerNameText(nickName + " " + Object.InputAuthority.PlayerId); }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSetNickName(NetworkString<_8> nickName) { playerName = nickName; }
    //---------------------------------------------------------------------------------------------------------------------------------------------


    //---------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Champion Variables")]
    [Networked] public float healthNetworked { get; set; }
    [SerializeField] private float maxHealth = 500;

    [Networked] protected float manaNetworked { get; set; }
    [SerializeField] private float maxMana = 500;

    [Networked] public bool isFacingLeftNetworked { get; set; }
    public bool isFacingLeft;

    [Networked] public Status statusNetworked { get; set; }
    public Status status;

    [Networked] public bool tookHitNetworked { get; private set; }
    public bool tookHit;

    [SerializeField] protected bool dead;

    [SerializeField] private float moveSpeed = 15;
    [SerializeField] private float airMoveSpeed = 10;
    [SerializeField] protected float rollMoveSpeed = 25;

    [SerializeField] private float blockPercentage = 0.8f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Champion Jump Variables")]
    [SerializeField] protected float jumpForce = 20;
    [SerializeField] private LayerMask WhatIsGround;

    [Networked] private float inAirHorizontalMovementNetworked { get; set; }
    [SerializeField] private float inAirHorizontalMovement;

    [SerializeField] protected bool inAir 
    {
        get 
        {
            Vector2 position = new Vector2(transform.position.x, transform.position.y) + Collider.offset;
            return !Physics2D.Raycast(position, Vector2.down, Collider.size.y / 2 + 0.01f, WhatIsGround); 
        } 
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Champion Attack Variables")]
    [SerializeField] public Transform AttackBoxesParent;
    [SerializeField] protected string[] ListNames = { "Air Attack", "Attack1", "Attack2", "Attack3", "Special Attack" };
    [SerializeField] protected BoxCollider2D[] AttackBoxes;
    [SerializeField] protected float[] AttackDamages;

    [Header("Champion Crowd Control Variables")]
    [SerializeField] protected float[] CrowdControlStrength;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    private void SetLocalObjects()
    {
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            RpcSetNickName(GlobalManagers.Instance.NetworkRunnerController.LocalPlayerName);
        }
    }

    public override void Spawned()
    {
        ResourceBar = GetComponentInChildren<ResourceBar>();
        ChampionAnimationController = GetComponentInChildren<ChampionAnimationController>();
        Rigid = GetComponent<Rigidbody2D>();
        Collider = GetComponent<CapsuleCollider2D>();

        healthNetworked = maxHealth;
        manaNetworked = maxMana;
        tookHitNetworked = tookHit = false;
        isFacingLeftNetworked = isFacingLeft = false;
        statusNetworked = status = Status.IDLE;
        inAirHorizontalMovementNetworked = inAirHorizontalMovement = 0;

        SetLocalObjects();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public void TakeDamageNetworked(float damage, bool attackerIsFacingLeft)
    {
        if (dead) return;
        if (healthNetworked <= 0) return;

        if (statusNetworked == Status.DEFEND && isFacingLeftNetworked != attackerIsFacingLeft)
            damage -= damage * blockPercentage;
        else
            tookHitNetworked = true;

        healthNetworked -= damage;
        if (healthNetworked < 0) healthNetworked = 0;
    }

    public void AddVelocity(Vector2 velocity)
    {
        Rigid.velocity += velocity;
    }

    public void AnimationTriggerAttack()
    {
        //if (Runner.IsServer)
        {
            int index = 0;
            if (statusNetworked == Status.AIR_ATTACK) index = 0;
            else if (statusNetworked == Status.ATTACK1) index = 1;
            else if (statusNetworked == Status.ATTACK2) index = 2;
            else if (statusNetworked == Status.ATTACK3) index = 3;
            else if (statusNetworked == Status.SPECIAL_ATTACK) index = 4;

            BoxCollider2D attackBox = AttackBoxes[index];
            float damage = AttackDamages[index];

            Collider2D[] colliders = Physics2D.OverlapBoxAll(attackBox.bounds.center, attackBox.bounds.size, 0, LayerMask.GetMask("Champion"));
            foreach (Collider2D collider in colliders)
            {
                Champion enemy = collider.GetComponent<Champion>();
                if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    enemy.TakeDamageNetworked(damage, isFacingLeftNetworked);
            }
        }
    }

    public virtual void ApplyCrowdControl(Champion enemy, float crowdControlStrength) { }
    public virtual void AnimationTriggerCrowdControl()
    {
        //if (Runner.IsServer)
        {
            int index = 0;
            if (statusNetworked == Status.AIR_ATTACK) index = 0;
            else if (statusNetworked == Status.ATTACK1) index = 1;
            else if (statusNetworked == Status.ATTACK2) index = 2;
            else if (statusNetworked == Status.ATTACK3) index = 3;
            else if (statusNetworked == Status.SPECIAL_ATTACK) index = 4;

            BoxCollider2D crowdControlBox = AttackBoxes[index];
            float crowdControlStrength = CrowdControlStrength[index];

            Collider2D[] colliders = Physics2D.OverlapBoxAll(crowdControlBox.bounds.center, crowdControlBox.bounds.size, 0, LayerMask.GetMask("Champion"));
            foreach (Collider2D collider in colliders)
            {
                Champion enemy = collider.GetComponent<Champion>();
                if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    ApplyCrowdControl(enemy, crowdControlStrength);
            }
        }
    }

    public virtual void AnimationTriggerProjectileSpawn() {}
    public virtual void AnimationTriggerMobility() {}
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Different Types Of Statuses
    protected virtual bool SingleAnimationStatus()
    {
        return (status == Status.ROLL || 
            status == Status.AIR_ATTACK || 
            status == Status.ATTACK1 || status == Status.ATTACK2 || status == Status.ATTACK3 || status == Status.SPECIAL_ATTACK ||
            status == Status.TAKE_HIT);
    }

    private bool CanChangeDirectionStatus()
    {
        return (status == Status.IDLE || status == Status.RUN || 
            status == Status.JUMP_UP || status == Status.JUMP_DOWN);
    }

    protected virtual bool InterruptableStatus()
    {
        return (status == Status.IDLE || status == Status.RUN ||
            status == Status.BEGIN_DEFEND || status == Status.DEFEND);
    }

    protected bool InAirInterruptableStatus()
    {
        return (status == Status.JUMP_UP || status == Status.JUMP_DOWN) || InterruptableStatus();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    private void EndStatus()
    {
        //End single animation status
        if (SingleAnimationStatus())
            if (ChampionAnimationController.AnimationFinished())
                status = Status.IDLE;

        //End Air Status
        if (status == Status.JUMP_UP) if (Rigid.velocity.y <= 0) status = Status.JUMP_DOWN;
        if (status == Status.JUMP_DOWN) if (!inAir) status = Status.IDLE;
    }

    protected virtual void TakeInput()
    {
        if (dead)
        {
            return;
        }

        //Determine direction
        if (CanChangeDirectionStatus())
        {
            if (Input.GetKey(KeyCode.A)) { isFacingLeft = true; }
            if (Input.GetKey(KeyCode.D)) { isFacingLeft = false; }
        }

        //If Character is in an interruptable status
        if (InterruptableStatus())
        {
            status = Status.IDLE;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                status = Status.RUN;
                if (Input.GetKeyDown(KeyCode.W)) status = Status.ROLL;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                status = Status.JUMP_UP;
            }
            if (Input.GetKeyDown(KeyCode.G)) status = Status.ATTACK1;
            if (Input.GetKeyDown(KeyCode.H)) status = Status.ATTACK2;
            if (Input.GetKeyDown(KeyCode.J)) status = Status.ATTACK3;
            if (Input.GetKeyDown(KeyCode.K)) status = Status.SPECIAL_ATTACK;
            if (Input.GetKey(KeyCode.S))
            {
                //Begin_Defend, then Begin_defend into Defend, then if already Defending, continue Defending
                Status lastStatus = (Status)ChampionAnimationController.GetAnimatorStatus();
                if (lastStatus != Status.BEGIN_DEFEND && lastStatus != Status.DEFEND) status = Status.BEGIN_DEFEND;
                else if (lastStatus == Status.BEGIN_DEFEND)
                {
                    if (ChampionAnimationController.AnimationFinished()) status = Status.DEFEND;
                    else status = Status.BEGIN_DEFEND;
                }
                else if (lastStatus == Status.DEFEND) status = Status.DEFEND;

            }
        }

        //In Air Input
        if (inAir)
        {
            //In Air Movement
            inAirHorizontalMovement = 0;
            if (Input.GetKey(KeyCode.A)) inAirHorizontalMovement += -1;
            if (Input.GetKey(KeyCode.D)) inAirHorizontalMovement += 1;

            if (InAirInterruptableStatus())
            {
                //If inAir, change animation to Jump depending on y velocity.
                if (Rigid.velocity.y > 0) status = Status.JUMP_UP;
                else if (Rigid.velocity.y < 0) status = Status.JUMP_DOWN;

                //In Air Attack
                if (Input.GetKeyDown(KeyCode.G))
                    status = Status.AIR_ATTACK;
            }
        }
    }

    private void CheckDeath()
    {
        if (healthNetworked <= 0)
        {
            dead = true;

            Status lastStatus = (Status)ChampionAnimationController.GetAnimatorStatus();
            if (lastStatus != Status.BEGIN_DEATH && lastStatus != Status.FINISHED_DEATH) status = Status.BEGIN_DEATH;
            if (lastStatus == Status.BEGIN_DEATH)
            {
                if (ChampionAnimationController.AnimationFinished()) status = Status.FINISHED_DEATH;
                else status = Status.BEGIN_DEATH;
            }
            else if (lastStatus == Status.FINISHED_DEATH) status = Status.FINISHED_DEATH;
        }
        else
        {
            if (dead)
            {
                dead = false;
                status = Status.IDLE;
            }
        }
    }

    private void DetermineStatus()
    {
        if (!(Runner.LocalPlayer == Object.InputAuthority)) return;

        EndStatus();
        TakeInput();
        if (tookHitNetworked) status = Status.TAKE_HIT;
        CheckDeath();
    }

    public void BeforeUpdate()
    {
        DetermineStatus();

        //Create a buffer so that JUMP_UP doesn't transition to JUMP_DOWN too soon.
        if (status == Status.JUMP_UP && Rigid.velocity.y == 0)
        {
            Rigid.velocity = new Vector2(0, 0.01f);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------




    private void UpdateChampionVisual()
    {
        ChampionAnimationController.Flip(isFacingLeftNetworked);
        ChampionAnimationController.ChangeAnimation(statusNetworked);

        //Attack Boxes And Crowd Control Boxes Flip
        if (isFacingLeftNetworked) AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        //ChampionAnimationController.ChangeAnimation(statusNetworked);
        if (statusNetworked == Status.TAKE_HIT)
        {
            if (tookHitNetworked) ChampionAnimationController.RestartAnimation(); //Restart take hit animation
            tookHitNetworked = false;
        }
    }

    protected virtual void UpdatePosition()
    {
        if (statusNetworked == Status.RUN)
        {
            float xChange = moveSpeed * Runner.DeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }

        if (statusNetworked == Status.JUMP_UP)
        {
            //Jumping up from the ground
            if (!inAir)
            {
                Rigid.velocity = new Vector2(Rigid.velocity.x, Rigid.velocity.y + jumpForce);
            }
        }

        if (statusNetworked == Status.ROLL)
        {
            float xChange = rollMoveSpeed * Runner.DeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }

        //Moving Left or Right In Air
        if (inAir &&
            statusNetworked != Status.TAKE_HIT && statusNetworked != Status.BEGIN_DEATH && statusNetworked != Status.FINISHED_DEATH)
        {
            float xChange = inAirHorizontalMovementNetworked * airMoveSpeed * Runner.DeltaTime;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer<ChampionData>(Object.InputAuthority, out var championData))
        {
            statusNetworked = championData.status;
            isFacingLeftNetworked = championData.isFacingLeft;
            inAirHorizontalMovementNetworked = championData.inAirHorizontalMovement;
        }

        ResourceBar.UpdateResourceBarVisuals(healthNetworked, maxHealth, manaNetworked, maxMana);
        UpdateChampionVisual();
        UpdatePosition();
    }

    public void LateUpdate()
    {
        //Make sure animations don't loop
        if (ChampionAnimationController.AnimationFinished())
        {
            Status lastStatus = (Status)ChampionAnimationController.GetAnimatorStatus();
            Status temp = status; 
            status = lastStatus;
            if (SingleAnimationStatus())
            {
                ChampionAnimationController.ChangeAnimation(Status.IDLE);
            }
            status = temp;
        }
    }

    public ChampionData GetChampionData()
    {
        ChampionData data = new ChampionData();
        data.status = status;
        data.isFacingLeft = isFacingLeft;
        data.inAirHorizontalMovement = inAirHorizontalMovement;
        return data;
    }

    protected virtual void OnDrawGizmos()
    {
        if (inAir) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        Vector2 position = new Vector2(transform.position.x, transform.position.y) + Collider.offset;
        Gizmos.DrawLine(position, new Vector2(position.x, position.y - Collider.size.y / 2 + 0.01f));
    }
}
