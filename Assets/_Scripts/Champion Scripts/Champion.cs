using UnityEngine;
using Fusion;
using System;

public class Champion : NetworkBehaviour, IBeforeUpdate
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Enums
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

    public enum AttackType
    {
        BlockByFacingAttack, //Block by facing in the opposite direction of the attacker
        BlockByFacingAttacker, //Block by facing towards the attack's direction
        Unblockable //Can't Block
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Componets
    [Header("Components")]
    private ResourceBar ResourceBar;
    protected ChampionAnimationController ChampionAnimationController;
    protected Rigidbody2D Rigid;
    public CapsuleCollider2D Collider;
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
    //Champion Variables
    [Header("Champion Variables")]
    [Networked] public float healthNetworked { get; set; }
    [SerializeField] private float maxHealth = 500;

    [Networked] protected float manaNetworked { get; set; }
    [SerializeField] private float maxMana = 500;

    [Networked] public bool isFacingLeftNetworked { get; set; }
    public bool isFacingLeft;

    [Networked] public Status statusNetworked { get; set; }
    public Status status;

    [SerializeField] protected bool dead;

    [SerializeField] private float moveSpeed = 15;
    [SerializeField] private float airMoveSpeed = 10;
    [SerializeField] protected float rollMoveSpeed = 25;

    [SerializeField] private float blockPercentage = 0.8f;
    [SerializeField] private float crowdControlBlockPercentage = 0f; 
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Air Variables
    [Header("Champion Air Variables")]
    [SerializeField] protected float jumpForce = 20;
    [SerializeField] private LayerMask WhatIsGround;

    [Networked] private float inAirHorizontalMovementNetworked { get; set; }
    [SerializeField] private float inAirHorizontalMovement;


    private const float inAirRayCastBuffer = 0.01f;
    private const float sideRayCastOffSetModifier = 1f/4f;
    [SerializeField] protected bool inAir 
    {
        get
        {
            float sideRayCastOffSet = Collider.size.x * sideRayCastOffSetModifier;
            float rayCastDistance = Collider.size.y / 2 + inAirRayCastBuffer;

            Vector2 centerPosition = new Vector2(transform.position.x, transform.position.y) + Collider.offset;
            Vector2 leftPosition = new Vector2(transform.position.x - sideRayCastOffSet, transform.position.y) + Collider.offset;
            Vector2 rightPosition = new Vector2(transform.position.x + sideRayCastOffSet, transform.position.y) + Collider.offset;


            return !Physics2D.Raycast(centerPosition, Vector2.down, rayCastDistance, WhatIsGround) && 
                   !Physics2D.Raycast(leftPosition, Vector2.down, rayCastDistance, WhatIsGround) &&
                   !Physics2D.Raycast(rightPosition, Vector2.down, rayCastDistance, WhatIsGround); 
        } 
    }

    private void OnDrawGizmosInAirRayCast()
    {
        if (inAir) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;

        float sideRayCastOffSet = Collider.size.x * sideRayCastOffSetModifier;
        float rayCastDistance = Collider.size.y / 2 + inAirRayCastBuffer;

        Vector2 centerPosition = new Vector2(transform.position.x, transform.position.y) + Collider.offset;
        Vector2 leftPosition = new Vector2(transform.position.x - sideRayCastOffSet, transform.position.y) + Collider.offset;
        Vector2 rightPosition = new Vector2(transform.position.x + sideRayCastOffSet, transform.position.y) + Collider.offset;


        if (!Physics2D.Raycast(centerPosition, Vector2.down, rayCastDistance, WhatIsGround)) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        Gizmos.DrawLine(centerPosition, new Vector2(centerPosition.x, centerPosition.y - rayCastDistance));

        if (!Physics2D.Raycast(leftPosition, Vector2.down, rayCastDistance, WhatIsGround)) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        Gizmos.DrawLine(leftPosition, new Vector2(leftPosition.x, leftPosition.y - rayCastDistance));

        if (!Physics2D.Raycast(rightPosition, Vector2.down, rayCastDistance, WhatIsGround)) Gizmos.color = Color.red;
        else Gizmos.color = Color.green;
        Gizmos.DrawLine(rightPosition, new Vector2(rightPosition.x, rightPosition.y - rayCastDistance));
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Class
    [Serializable] public class Attack
    {
        public string attackName;
        public BoxCollider2D hitBox;
        public float damage;
        public float crowdControlStrength;
        public float manaCost;
        public float coolDownDuration;
        public TickTimer coolDownTimer;

        public float getCoolDownRemainingTime() { return coolDownTimer.RemainingTime(GlobalManagers.Instance.NetworkRunnerController.networkRunnerInstance) ?? 0.0f; }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    [Header("Champion Attack Variables")]
    [SerializeField] public Transform AttackBoxesParent;
    [SerializeField] public Attack[] Attacks;

    protected virtual Attack getAttack(Status status)
    {
        if (status == Status.AIR_ATTACK) return Attacks[0];
        else if (status == Status.ATTACK1) return Attacks[1];
        else if (status == Status.ATTACK2) return Attacks[2];
        else if (status == Status.ATTACK3) return Attacks[3];
        else if (status == Status.SPECIAL_ATTACK) return Attacks[4];
        return null;
    }

    protected virtual float getManaCost(Status status)
    {
        Attack attack = getAttack(status);
        if (attack != null) return attack.manaCost;
        else return 0;
    }

    protected float getCoolDownDuration(Status status)
    {
        Attack attack = getAttack(status);
        if (attack != null) return attack.coolDownDuration;
        else return 0;
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    protected void RPC_setCoolDownDuration(Status status)
    {
        Attack attack = getAttack(status);
        if (attack != null) attack.coolDownTimer = TickTimer.CreateFromSeconds(Runner, attack.coolDownDuration);
    }

    protected bool canUseAttack(Status status)
    {
        Attack attack = getAttack(status);
        if (attack != null) return manaNetworked >= attack.manaCost && attack.coolDownTimer.ExpiredOrNotRunning(Runner);
        else return false;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Initialization
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
        isFacingLeftNetworked = isFacingLeft = false;
        statusNetworked = status = Status.IDLE;
        inAirHorizontalMovementNetworked = inAirHorizontalMovement = 0;

        SetLocalObjects();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Different Types Of Statuses
    protected virtual bool LoopingAnimationStatus(Status status)
    {
        return (status == Status.IDLE || status == Status.RUN || 
            status == Status.JUMP_UP || status == Status.JUMP_DOWN || 
            status == Status.DEFEND);
    }

    protected virtual bool SingleAnimationStatus()
    {
        return (status == Status.ROLL ||
            status == Status.AIR_ATTACK ||
            status == Status.ATTACK1 || status == Status.ATTACK2 || status == Status.ATTACK3 || status == Status.SPECIAL_ATTACK ||
            status == Status.TAKE_HIT);
    }

    protected bool CanChangeDirectionStatus()
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

    protected virtual bool UnstoppableStatusNetworked()
    {
        return (statusNetworked == Status.SPECIAL_ATTACK);
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
            if (Input.GetKeyDown(KeyCode.G) && canUseAttack(Status.ATTACK1)) status = Status.ATTACK1;
            if (Input.GetKeyDown(KeyCode.H) && canUseAttack(Status.ATTACK2)) status = Status.ATTACK2;
            if (Input.GetKeyDown(KeyCode.J) && canUseAttack(Status.ATTACK3)) status = Status.ATTACK3;
            if (Input.GetKeyDown(KeyCode.K) && canUseAttack(Status.SPECIAL_ATTACK)) status = Status.SPECIAL_ATTACK;
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
                if (Input.GetKeyDown(KeyCode.G) && canUseAttack(Status.AIR_ATTACK))
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



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BeginTakeHitAnimation()
    {
        if (Runner.LocalPlayer == Object.InputAuthority) status = Status.TAKE_HIT;
        statusNetworked = Status.TAKE_HIT;
        ChampionAnimationController.ChangeAnimation((int)statusNetworked);
        ChampionAnimationController.RestartAnimation();
    }

    public void TakeDamageNetworked(float damage, bool attackerIsFacingLeft, AttackType attackType = AttackType.BlockByFacingAttack, Vector2 attackerPosition = default(Vector2))
    {
        if (dead) return;
        if (healthNetworked <= 0) return;

        //Blocked
        if ((statusNetworked == Status.BEGIN_DEFEND || statusNetworked == Status.DEFEND) 
            && attackType == AttackType.BlockByFacingAttack && isFacingLeftNetworked != attackerIsFacingLeft)
            damage -= damage * blockPercentage;
        else if ((statusNetworked == Status.BEGIN_DEFEND || statusNetworked == Status.DEFEND)
            && attackType == AttackType.BlockByFacingAttacker
            && ((isFacingLeftNetworked && transform.position.x > attackerPosition.x) || (!isFacingLeftNetworked && transform.position.x < attackerPosition.x)))
            damage -= damage * blockPercentage;
        else
        {
            if (!UnstoppableStatusNetworked())
            {
                if (Runner.IsServer) RPC_BeginTakeHitAnimation();
            }
        }

        healthNetworked -= damage;
        if (healthNetworked < 0) healthNetworked = 0;
    }

    public void AddVelocity(Vector2 velocity)
    {
        Rigid.velocity += velocity * (1 - crowdControlBlockPercentage);
    }

    public void SetVelocity(Vector2 velocity)
    {
        Rigid.velocity = velocity * (1 - crowdControlBlockPercentage);
    }

    public virtual void DealDamageToVictim(Champion enemy, float damage)
    {
        enemy.TakeDamageNetworked(damage, isFacingLeftNetworked);
    }

    public virtual bool CanBeAttacked(Champion attacker)
    {
        return (this != attacker && this.healthNetworked > 0);
    }

    public virtual int GetAttackBoxIndex()
    {
        if (statusNetworked == Status.AIR_ATTACK) return 0;
        else if (statusNetworked == Status.ATTACK1) return 1;
        else if (statusNetworked == Status.ATTACK2) return 2;
        else if (statusNetworked == Status.ATTACK3) return 3;
        else if (statusNetworked == Status.SPECIAL_ATTACK) return 4;
        else return -1;
    }

    public virtual void GetAttackBoxAndDamage(ref BoxCollider2D attackBox, ref float damage, int index)
    {
        attackBox = Attacks[index].hitBox;
        damage = Attacks[index].damage;
    }
    public virtual void AnimationTriggerAttack()
    {
        int index = GetAttackBoxIndex();
        if (index == -1) return;

        BoxCollider2D attackBox = default;
        float damage = 0;

        GetAttackBoxAndDamage(ref attackBox, ref damage, index);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackBox.bounds.center, attackBox.bounds.size, 0, LayerMask.GetMask("Champion"));
        foreach (Collider2D collider in colliders)
        {
            Champion enemy = collider.GetComponent<Champion>();
            if (enemy != null && enemy.CanBeAttacked(this))
                DealDamageToVictim(enemy, damage);
        }
    }

    public virtual void ApplyCrowdControl(Champion enemy, float crowdControlStrength) {}

    public virtual void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        crowdControlBox = Attacks[index].hitBox;
        crowdControlStrength = Attacks[index].crowdControlStrength;
    }
    public virtual void AnimationTriggerCrowdControl()
    {
        int index = GetAttackBoxIndex();
        if (index == -1) return;

        BoxCollider2D crowdControlBox = default;
        float crowdControlStrength = 0;

        GetControlBoxAndStrength(ref crowdControlBox, ref crowdControlStrength, index);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(crowdControlBox.bounds.center, crowdControlBox.bounds.size, 0, LayerMask.GetMask("Champion"));
        foreach (Collider2D collider in colliders)
        {
            Champion enemy = collider.GetComponent<Champion>();
            if (enemy != null && enemy.CanBeAttacked(this))
                ApplyCrowdControl(enemy, crowdControlStrength);
        }
    }

    public virtual void AnimationTriggerAbilitySpawn() {}
    public virtual void AnimationTriggerProjectileSpawn() {}
    public virtual void AnimationTriggerMobility() {}
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
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
            if (!inAir && Rigid.velocity.y < 0.011)
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

    private void ApplyManaCost()
    {
        if (!Runner.IsServer) return;

        float manaCost = 0;
        if (ChampionAnimationController.GetAnimatorStatus() != (int) statusNetworked) //Animation Changed
            manaCost = getManaCost(statusNetworked);
        manaNetworked -= manaCost;
    }

    private void ApplyCoolDownDuration()
    {
        if (!Runner.IsServer) return;

        if (ChampionAnimationController.GetAnimatorStatus() != (int)statusNetworked) //Animation Changed
        {
            RPC_setCoolDownDuration(statusNetworked);
        }
    }

    private void UpdateChampionVisual()
    {
        ChampionAnimationController.Flip(isFacingLeftNetworked);
        ChampionAnimationController.ChangeAnimation((int)statusNetworked);

        //Attack Boxes And Crowd Control Boxes Flip
        if (isFacingLeftNetworked) AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer<ChampionData>(Object.InputAuthority, out var championData))
        {
            statusNetworked = championData.status;
            isFacingLeftNetworked = championData.isFacingLeft;
            inAirHorizontalMovementNetworked = championData.inAirHorizontalMovement;
        }
        UpdatePosition();
        ApplyManaCost();
        ApplyCoolDownDuration();

        ResourceBar.UpdateResourceBarVisuals(healthNetworked, maxHealth, manaNetworked, maxMana);
        UpdateChampionVisual();
    }

    public void LateUpdate()
    {
        //Make sure looping animations do not go over normalizedtime > 1. Because if that happens, Unity animator will fail to swap states (Unity Bug?).
        if (LoopingAnimationStatus((Champion.Status)ChampionAnimationController.GetAnimatorStatus()) && ChampionAnimationController.AnimationFinished())
        {
            ChampionAnimationController.RestartAnimation();
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



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
        OnDrawGizmosInAirRayCast();
    }
}
