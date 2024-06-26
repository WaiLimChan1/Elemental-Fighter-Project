using UnityEngine;
using Fusion;
using System;
using Unity.VisualScripting;

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
        AlwaysBlockable, //Block by blocking. Direction does not matter
        BlockByFacingAttack, //Block by facing in the opposite direction of the attacker
        BlockByFacingAttacker, //Block by facing towards the attack's direction
        Unblockable //Can't Block
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Componets
    [Header("Champion Components")]
    [Networked] public NetworkedPlayer NetworkedPlayer { get; set; }
    protected ResourceBar ResourceBar;
    protected ChampionAnimationController ChampionAnimationController;
    protected Rigidbody2D Rigid;
    public CapsuleCollider2D Collider;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Player Name
    public void SetPlayerName(NetworkString<_8> nickName) { ResourceBar.SetPlayerNameText(nickName + " " + Object.InputAuthority.PlayerId); }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Dynamic Variables
    [Header("Champion Dynamic Variables")]

    [Networked] public float healthNetworked { get; set; }

    [Networked] public float manaNetworked { get; set; }

    [Networked] public float ultimateMeterNetworked { get; set; }

    [Networked] public bool isFacingLeftNetworked { get; set; }
    public bool isFacingLeft;

    [Networked] public Status statusNetworked { get; set; }
    public Status status;

    [SerializeField] protected bool dead;
    //---------------------------------------------------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Dynamic Variable Functions
    public void setHealthNetworked(float health)
    {
        healthNetworked = health;
        healthNetworked = Mathf.Clamp(healthNetworked, 0.0f, maxHealth);
    }
    public void setManaNetworked(float mana)
    {
        manaNetworked = mana;
        manaNetworked = Mathf.Clamp(manaNetworked, 0.0f, maxMana);
    }
    public void setUltimateMeterNetworked(float ultimateMeter, bool applyGainMultiplier = true)
    {
        //If gaining ultimateMeter value, increase the gain by ultimateMeterGainMutliplier
        if (ultimateMeter > ultimateMeterNetworked)
        {
            float ultimateMeterGain = (ultimateMeter - ultimateMeterNetworked);
            if (applyGainMultiplier) ultimateMeterGain *= ultimateMeterGainMultipier;

            ultimateMeter = ultimateMeterNetworked + ultimateMeterGain;
        }

        ultimateMeterNetworked = ultimateMeter;
        if (ultimateMeterNetworked < 0) ultimateMeterNetworked = 0;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Air Variables & Functions
    [Header("Champion Air Variables")]
    [SerializeField] private LayerMask WhatIsGround;

    [Networked] private float inAirHorizontalMovementNetworked { get; set; }
    [SerializeField] private float inAirHorizontalMovement;

    private const float inAirRayCastBuffer = 0.01f;
    private const float sideRayCastOffSetModifier = 1f / 4f;
    [SerializeField]
    protected bool inAir
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
    //Champion Take Hit Recovery
    [Header("Champion Take Hit Recovery")]
    [Networked] [SerializeField] protected float TakeHitRecoveryPercentage { get; set; }
    private float TakeHitRecoveryPercentageGain = 0.1f;
    private float TakeHitRecoveryPercentageDecay = 0.25f;
    private float SkipTakeHitAnimationCutOff = 0.9f;

    private Vector2 TakeHitAnimationSpeedRange = new Vector2(1, 10);

    private void SetTakeHitRecoveryValue(float value)
    {
        TakeHitRecoveryPercentage = Mathf.Clamp01(value);
    }

    private void IncreaseTakeHitRecoveryPercentage()
    {
        SetTakeHitRecoveryValue(TakeHitRecoveryPercentage + TakeHitRecoveryPercentageGain + TakeHitRecoveryPercentageDecay * Runner.DeltaTime);
    }

    private void SetTakeHitAnimationSpeed()
    {
        ChampionAnimationController.SetSpeed(Mathf.Lerp(TakeHitAnimationSpeedRange.x, TakeHitAnimationSpeedRange.y, TakeHitRecoveryPercentage));
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Base Stats
    [Header("Champion Static Stats")]
    [SerializeField] protected float ultimateMeterDefendCost = 1f;

    [SerializeField] protected float baseMoveSpeed = 15f;
    [SerializeField] protected Attack roll;
    [SerializeField] protected float baseRollMoveSpeed = 25f;

    [SerializeField] protected float baseJumpForce = 20f;
    [SerializeField] protected float maxJumpForce = 25f;
    //[SerializeField] protected float jumpMobilityModifierScale

    [SerializeField] protected float baseAirMoveSpeed = 10f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Base Stats
    [Header("Champion Base Stats")]
    [SerializeField] protected float baseMaxHealth = 500;
    [SerializeField] protected float baseMaxMana = 500;

    [SerializeField] protected float baseHealthRegenPercentage = 0.005f;
    [SerializeField] protected float baseManaRegenPercentage = 0.01f;

    [SerializeField] protected float baseUltimateMeterRegen = 0;
    [SerializeField] protected float baseUltimateMeterGainMultipier = 1;

    [SerializeField] protected float baseArmor = 0;
    [SerializeField] protected float baseCrowdControlIgnorePercentage = 0;

    [SerializeField] protected float baseBlockPercentage = 0.5f;
    [SerializeField] protected float baseCrowdControlBlockPercentage = 0.3f;

    [SerializeField] protected float basePhysicalDamage = 0;
    [SerializeField] protected float baseAttackSpeed = 1;
    [SerializeField] protected float baseCoolDownReduction = 0;
    [SerializeField] protected float baseOmnivamp = 0;

    [SerializeField] protected float baseMobilityModifer = 1f;
    //---------------------------------------------------------------------------------------------------------------------------------------------


    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Stats
    [Header("Champion Stats")]
    [Networked] [SerializeField] protected float maxHealth { get; set; }
    [Networked] [SerializeField] protected float maxMana { get; set; }

    [Networked] [SerializeField] protected float healthRegenPercentage { get; set; }
    [Networked] [SerializeField] protected float manaRegenPercentage { get; set; }

    [Networked] [SerializeField] protected float ultimateMeterRegen { get; set; }
    [Networked] [SerializeField] protected float ultimateMeterGainMultipier { get; set; }

    [Networked] [SerializeField] protected float armor { get; set; }
    [Networked] [SerializeField] protected float crowdControlIgnorePercentage { get; set; }

    [Networked] [SerializeField] protected float blockPercentage { get; set; }
    [Networked] [SerializeField] protected float crowdControlBlockPercentage { get; set; }

    [Networked] [SerializeField] protected float physicalDamage { get; set; }
    [Networked] [SerializeField] protected float attackSpeed { get; set; }
    [Networked] [SerializeField] protected float coolDownReduction { get; set; }
    [Networked] [SerializeField] protected float omnivamp { get; set; }

    [Networked] [SerializeField] protected float mobilityModifier { get; set; } //0.5 to 1.5

    private void CalculateBaseStats()
    {
        maxHealth = baseMaxHealth;
        maxMana = baseMaxMana;

        healthRegenPercentage = baseHealthRegenPercentage;
        manaRegenPercentage = baseManaRegenPercentage;

        ultimateMeterRegen = baseUltimateMeterRegen;
        ultimateMeterGainMultipier = baseUltimateMeterGainMultipier;

        armor = baseArmor;
        crowdControlIgnorePercentage = baseCrowdControlIgnorePercentage;

        blockPercentage = baseBlockPercentage;
        crowdControlBlockPercentage = baseCrowdControlBlockPercentage;

        physicalDamage = basePhysicalDamage;
        attackSpeed = baseAttackSpeed;
        coolDownReduction = baseCoolDownReduction;
        omnivamp = baseOmnivamp;

        mobilityModifier = baseMobilityModifer;
    }

    private void CalculateItemStats()
    {
        if (!NetworkedPlayer.CanUseNetworkedPlayer(NetworkedPlayer)) return;
        ItemManager.Item combinedItem = NetworkedPlayer.GetCombinedItemStats();

        maxHealth += combinedItem.maxHealthBonus;
        maxMana += combinedItem.maxManaBonus;

        healthRegenPercentage += combinedItem.healthRegenPercentageBonus;
        manaRegenPercentage += combinedItem.manaRegenPercentageBonus;

        ultimateMeterRegen += combinedItem.ultimateMeterRegenBonus;
        ultimateMeterGainMultipier += combinedItem.ultimateMeterGainMultipierBonus;

        armor += combinedItem.armorBonus;
        crowdControlIgnorePercentage += combinedItem.crowdControlIgnorePercentageBonus;

        blockPercentage += combinedItem.blockPercentageBonus;
        crowdControlBlockPercentage += combinedItem.crowdControlBlockPercentageBonus;

        physicalDamage += combinedItem.physicalDamageBonus;
        attackSpeed += combinedItem.attackSpeedBonus;
        coolDownReduction += combinedItem.coolDownReductionBonus;
        omnivamp += combinedItem.omnivampBonus;

        mobilityModifier += combinedItem.mobilityModifierBonus;
    }

    private void HandleStatsRange()
    {
        blockPercentage = Mathf.Clamp01(blockPercentage);
        coolDownReduction = Mathf.Clamp01(coolDownReduction);
    }

    protected void CalculateStats()
    {
        if (Object == default) return;
        if (!Runner.IsServer) return;   

        CalculateBaseStats();
        CalculateItemStats();
        HandleStatsRange();
    }

    public ItemManager.Item GetCalculatedStatsAsAnItem()
    {
        ItemManager.Item calculatedStats = new ItemManager.Item();
        if (Object == default) return calculatedStats;

        CalculateStats();

        calculatedStats.maxHealthBonus = maxHealth;
        calculatedStats.maxManaBonus = maxMana;

        calculatedStats.healthRegenPercentageBonus = healthRegenPercentage;
        calculatedStats.manaRegenPercentageBonus = manaRegenPercentage;

        calculatedStats.ultimateMeterRegenBonus = ultimateMeterRegen;
        calculatedStats.ultimateMeterGainMultipierBonus = ultimateMeterGainMultipier;

        calculatedStats.armorBonus = armor;
        calculatedStats.crowdControlIgnorePercentageBonus = crowdControlIgnorePercentage;

        calculatedStats.blockPercentageBonus = blockPercentage;
        calculatedStats.crowdControlBlockPercentageBonus = crowdControlBlockPercentage;

        calculatedStats.physicalDamageBonus = physicalDamage;
        calculatedStats.attackSpeedBonus = attackSpeed;
        calculatedStats.coolDownReductionBonus = coolDownReduction;
        calculatedStats.omnivampBonus = omnivamp;

        calculatedStats.mobilityModifierBonus = mobilityModifier;

        return calculatedStats;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Class
    [Serializable] public class Attack
    {
        public string attackName;
        public BoxCollider2D hitBox;
        public float damage;
        public float physicalDamageScaling;
        public float numOfAttacks;
        public float crowdControlStrength;
        public float manaCost;
        public float coolDownDuration;
        public TickTimer coolDownTimer;

        public float getCoolDownRemainingTime() { return coolDownTimer.RemainingTime(GlobalManagers.Instance.NetworkRunnerController.networkRunnerInstance) ?? 0.0f; }
    }

    protected float getCalculatedDamage(Attack attack)
    {
        return (attack.damage + (physicalDamage * attack.physicalDamageScaling)) / attack.numOfAttacks;
    }

    //Attack UI
    public virtual void SetAttack_ChampionUI(AllAttacks_ChampionUI ChampionUI)
    {
        ChampionUI.SetActiveAllAttack_ChampionUI(false);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.AirAttack, Attacks[0], "Space + G");
        ChampionUI.SetAttack_ChampionUI(ChampionUI.Attack1, Attacks[1], "G");
        ChampionUI.SetAttack_ChampionUI(ChampionUI.Attack2, Attacks[2], "H");
        ChampionUI.SetAttack_ChampionUI(ChampionUI.Attack3, Attacks[3], "J");
        ChampionUI.SetAttack_ChampionUI(ChampionUI.SpecialAttak, Attacks[4], "K");
        ChampionUI.SetAttack_ChampionUI(ChampionUI.Roll, roll, "A/D + W");
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
        else if (status == Status.ROLL) return roll;
        return null;
    }

    protected float getManaCost(Status status)
    {
        Attack attack = getAttack(status);
        if (attack != null) return attack.manaCost;
        else return 0;
    }

    public float getCoolDownDuration(Attack attack)
    {
        if (attack != null) return attack.coolDownDuration - (attack.coolDownDuration * coolDownReduction);
        else return 0;
    }

    protected float getCoolDownDuration(Status status)
    {
        return getCoolDownDuration(getAttack(status));
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    protected void RPC_setCoolDownDuration(Status status, float cooldown)
    {
        Attack attack = getAttack(status);
        if (attack != null) attack.coolDownTimer = TickTimer.CreateFromSeconds(Runner, cooldown); //Use host data
    }

    protected bool CanUseAttack(Status status)
    {
        Attack attack = getAttack(status);
        if (attack != null) return manaNetworked >= attack.manaCost && attack.coolDownTimer.ExpiredOrNotRunning(Runner);
        else return false;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Transform Variables & Functions
    [Header("Champion Transform Variables")]
    [SerializeField] protected NetworkPrefabRef TransformChampion;
    [SerializeField] protected float TransformHealthGainPercentage = 0.5f;
    [SerializeField] protected float TransformManaGainPercentage = 0.5f;
    [SerializeField] protected float ultimateMeterCost = 1000;
    [SerializeField] protected float ultimateMeterKillBonus = 300;

    private bool CanUseTransform()
    {
        return ultimateMeterNetworked >= ultimateMeterCost;
    }

    //Elemental to Default
    protected virtual void HostSetUpTransformChampion(float healthRatio, float TransformHealthGainAmount, float manaRatio, float TransformManaGainAmount, float ultimateMeter, float ultimateMeterCost)
    {
        CalculateStats();
        setHealthNetworked(maxHealth * healthRatio + maxHealth * TransformHealthGainAmount);
        setManaNetworked(maxMana * manaRatio + maxMana * TransformManaGainAmount);
        setUltimateMeterNetworked(ultimateMeter, false);
    }

    //Elemental to Default
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    protected virtual void RPC_ClientSetUpTransformChampion(bool isFacingLeft)
    {
        this.isFacingLeft = isFacingLeft;
    }

    //Default to Elemental
    protected virtual void SpawnTransformChampion(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;

        var transformChampionObject = Runner.Spawn(TransformChampion, transform.position, Quaternion.identity, playerRef);
        NetworkedPlayer.OwnedChampion = transformChampionObject;
        Champion transformChampion = transformChampionObject.GetComponent<Champion>();

        transformChampion.NetworkedPlayer = NetworkedPlayer;
        transformChampion.HostSetUpTransformChampion(healthNetworked / maxHealth, TransformHealthGainPercentage, manaNetworked / maxMana, TransformManaGainPercentage, ultimateMeterNetworked, ultimateMeterCost);
        transformChampion.RPC_ClientSetUpTransformChampion(isFacingLeftNetworked);

        Runner.Despawn(this.Object);
    }


    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SpawnTransformChampion(PlayerRef playerRef)
    {
        SpawnTransformChampion(playerRef);
    }

    public virtual void AnimationTriggerTransform()
    {
        SpawnTransformChampion(Object.InputAuthority);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Initialization
    public override void Spawned()
    {
        ResourceBar = GetComponentInChildren<ResourceBar>();
        ChampionAnimationController = GetComponentInChildren<ChampionAnimationController>();
        Rigid = GetComponent<Rigidbody2D>();
        Collider = GetComponent<CapsuleCollider2D>();

        isFacingLeft = false;
        status = Status.IDLE;
        inAirHorizontalMovement = 0;

        CalculateStats();
        if (Runner.IsServer)
        {
            healthNetworked = maxHealth;
            manaNetworked = maxMana;
            ultimateMeterNetworked = 0;

            isFacingLeftNetworked = isFacingLeft;
            statusNetworked = status;
            inAirHorizontalMovementNetworked = 0;
        }
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
        return (statusNetworked == Status.ROLL || statusNetworked == Status.SPECIAL_ATTACK);
    }

    protected virtual bool DefensiveStatusNetworked()
    {
        return (statusNetworked == Status.BEGIN_DEFEND || statusNetworked == Status.DEFEND);
    }

    //Statuses that are influenced by AttackSpeed
    protected virtual bool AttackSpeedStatus(Status status)
    {
        return (status == Status.AIR_ATTACK || status == Status.ATTACK1 || status == Status.ATTACK2);
    }

    //Statuses that are influenced by MobilityModifier
    protected virtual bool MobilityStatus(Status status)
    {
        return (status == Status.RUN || status == Status.ROLL ||
            status == Status.JUMP_UP || status == Status.JUMP_DOWN);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    protected virtual void EndStatus()
    {
        //End single animation status
        if (SingleAnimationStatus())
            if (ChampionAnimationController.AnimationFinished())
                status = Status.IDLE;

        //End Air Status
        if (status == Status.JUMP_UP) if (Rigid.velocity.y <= 0) status = Status.JUMP_DOWN;
        if (status == Status.JUMP_DOWN) if (!inAir) status = Status.IDLE;
    }

    protected virtual void TransformTakeInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && CanUseTransform()) RPC_SpawnTransformChampion(Runner.LocalPlayer);
    }

    protected virtual void OnGroundTakeInput()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            status = Status.RUN;
            if (Input.GetKeyDown(KeyCode.W) && CanUseAttack(Status.ROLL)) status = Status.ROLL;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            status = Status.JUMP_UP;
        }
        if ((Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.Mouse0)) && CanUseAttack(Status.ATTACK1)) status = Status.ATTACK1;
        if ((Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Mouse1)) && CanUseAttack(Status.ATTACK2)) status = Status.ATTACK2;
        if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Mouse4)) && CanUseAttack(Status.ATTACK3)) status = Status.ATTACK3;
        if ((Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Mouse3)) && CanUseAttack(Status.SPECIAL_ATTACK)) status = Status.SPECIAL_ATTACK;
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
        TransformTakeInput();
    }

    protected virtual void InAirTakeInput()
    {
        if ((Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.Mouse0)) && CanUseAttack(Status.AIR_ATTACK)) status = Status.AIR_ATTACK;
    }

    protected virtual void CancelTakeInput() { }

    protected virtual void TakeInput()
    {
        if (dead)
        {
            return;
        }

        //If GameManager exists and it stops champion control
        if (GameManager.CanUseGameManager() && GameManager.Instance.StopChampionTakeInput) return;

        //Determine direction
        if (CanChangeDirectionStatus())
        {
            if (Input.GetKey(KeyCode.A)) { isFacingLeft = true; }
            if (Input.GetKey(KeyCode.D)) { isFacingLeft = false; }
        }

        //If Character is in an interruptable status
        if (!inAir && InterruptableStatus())
        {
            status = Status.IDLE;
            OnGroundTakeInput();
        }

        //In Air Input
        if (inAir)
        {
            //In Air Horizontal Movement
            inAirHorizontalMovement = 0;
            if (Input.GetKey(KeyCode.A)) inAirHorizontalMovement += -1;
            if (Input.GetKey(KeyCode.D)) inAirHorizontalMovement += 1;

            if (InAirInterruptableStatus())
            {
                //If inAir, change animation to Jump depending on y velocity.
                if (Rigid.velocity.y > 0) status = Status.JUMP_UP;
                else if (Rigid.velocity.y < 0) status = Status.JUMP_DOWN;

                InAirTakeInput();
            }
        }

        CancelTakeInput();
    }

    private void CheckDeath()
    {
        if (Object == default) return;
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

        CalculateStats();
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

    public float TakeDamageNetworked(Champion attacker, float damage, float numOfAttacks, bool attackerIsFacingLeft, AttackType attackType = AttackType.BlockByFacingAttack, Vector2 attackerPosition = default(Vector2))
    {
        if (dead) return 0;
        if (healthNetworked <= 0) return 0;

        float originalDamage = damage;
        damage -= armor / numOfAttacks;

        //Blocked
        if (DefensiveStatusNetworked() && attackType == AttackType.AlwaysBlockable)
            damage -= damage * blockPercentage;
        else if (DefensiveStatusNetworked() && attackType == AttackType.BlockByFacingAttack && isFacingLeftNetworked != attackerIsFacingLeft)
            damage -= damage * blockPercentage;
        else if (DefensiveStatusNetworked()
            && attackType == AttackType.BlockByFacingAttacker
            && ((isFacingLeftNetworked && transform.position.x > attackerPosition.x) || (!isFacingLeftNetworked && transform.position.x < attackerPosition.x)))
            damage -= damage * blockPercentage;
        else
        {
            if (!UnstoppableStatusNetworked())
            {
                if (Runner.IsServer) 
                    if (TakeHitRecoveryPercentage < SkipTakeHitAnimationCutOff)
                        RPC_BeginTakeHitAnimation();
                IncreaseTakeHitRecoveryPercentage();
            }
        }

        if (damage < 1) damage = 1;

        //Defender Effects
        setHealthNetworked(healthNetworked - damage);
        setUltimateMeterNetworked(ultimateMeterNetworked + originalDamage); //Defender gain UltimateMeter for taking and blocking damage

        //Attacker Deal Damage Effects
        if (attacker != null)
        {
            attacker.setHealthNetworked(attacker.healthNetworked + damage * attacker.omnivamp); //Omnivamp
            attacker.setUltimateMeterNetworked(attacker.ultimateMeterNetworked + damage); //Attack gain UltimateMeter for dealing damage
            if (healthNetworked <= 0) attacker.setUltimateMeterNetworked(attacker.ultimateMeterNetworked + ultimateMeterKillBonus); //Attack gain UltimateMeter for killing enemy
        }

        //LifeTime Data Update
        if (NetworkedPlayer.CanUseNetworkedPlayer(this.NetworkedPlayer)) this.NetworkedPlayer.TotalDamageTaken += damage;
        if (NetworkedPlayer.CanUseNetworkedPlayer(attacker.NetworkedPlayer))
        {
            attacker.NetworkedPlayer.TotalDamageDealt += damage;
            if (healthNetworked <= 0) attacker.NetworkedPlayer.TotalKills++;
        }

        return damage;
    }

    public Vector2 CalculateVelocity(Vector2 velocity)
    {
        float totalCCReductionPercentage = crowdControlIgnorePercentage;
        if (DefensiveStatusNetworked() || UnstoppableStatusNetworked()) totalCCReductionPercentage += crowdControlBlockPercentage;

        return velocity * Mathf.Clamp01(1 - totalCCReductionPercentage);
    }

    public void AddVelocity(Vector2 velocity)
    {
        Rigid.velocity += CalculateVelocity(velocity);
    }

    public void SetVelocity(Vector2 velocity)
    {
        Rigid.velocity = CalculateVelocity(velocity);
    }

    public virtual void DealDamageToVictim(Champion enemy, float damage, float numOfAttacks)
    {
        enemy.TakeDamageNetworked(this, damage, numOfAttacks, isFacingLeftNetworked);
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

    public virtual void GetAttackBoxAndDamage(ref BoxCollider2D attackBox, ref float damage, ref float numOfAttacks, int index)
    {
        attackBox = Attacks[index].hitBox;
        damage = getCalculatedDamage(Attacks[index]);
        numOfAttacks = Attacks[index].numOfAttacks;
    }

    public virtual void AnimationTriggerAttack()
    {
        int index = GetAttackBoxIndex();
        if (index == -1) return;

        BoxCollider2D attackBox = default;
        float damage = 0;
        float numOfAttacks = 0;

        GetAttackBoxAndDamage(ref attackBox, ref damage, ref numOfAttacks, index);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackBox.bounds.center, attackBox.bounds.size, 0, LayerMask.GetMask("Champion"));
        foreach (Collider2D collider in colliders)
        {
            Champion enemy = collider.GetComponent<Champion>();
            if (enemy != null && enemy.CanBeAttacked(this))
                DealDamageToVictim(enemy, damage, numOfAttacks);
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
    //Apply Effects Logic
    private void ApplyManaCost()
    {
        if (!Runner.IsServer) return;

        float manaCost = 0;
        if (ChampionAnimationController.GetAnimatorStatus() != (int)statusNetworked) //Animation Changed
        {
            //The if statement fixes mana cost being taken multiple times after having an animation cancelled by another attack
            if (getAttack(statusNetworked) != null && getAttack(statusNetworked).getCoolDownRemainingTime() == 0)
                manaCost = getManaCost(statusNetworked);
        }
        setManaNetworked(manaNetworked - manaCost);
    }

    private void ApplyCoolDownDuration()
    {
        if (!Runner.IsServer) return;

        if (ChampionAnimationController.GetAnimatorStatus() != (int)statusNetworked) //Animation Changed
        {
            if (getAttack(statusNetworked) != null) RPC_setCoolDownDuration(statusNetworked, getCoolDownDuration(statusNetworked));
        }
    }

    private void ApplyResourceRegen()
    {
        if (!Runner.IsServer) return;
        if (healthNetworked <= 0) return;
        if (GameManager.CanUseGameManager() && GameManager.Instance.StopResourceRegenAndDecay) return;

        setHealthNetworked(healthNetworked + maxHealth * healthRegenPercentage * Runner.DeltaTime);
        setManaNetworked(manaNetworked + maxMana * manaRegenPercentage * Runner.DeltaTime);
        setUltimateMeterNetworked(ultimateMeterNetworked + ultimateMeterRegen * Runner.DeltaTime);
    }

    private void ApplyDefendCost()
    {
        if (!Runner.IsServer) return;
        if (statusNetworked == Status.DEFEND) ultimateMeterNetworked -= ultimateMeterDefendCost * Runner.DeltaTime;
    }

    private void ApplyTakeHitRecoveryPercentageDecay()
    {
        if (!Runner.IsServer) return;
        SetTakeHitRecoveryValue(TakeHitRecoveryPercentage - TakeHitRecoveryPercentageDecay * Runner.DeltaTime);
    }

    protected virtual void ApplyEffects()
    {
        ApplyManaCost();
        ApplyCoolDownDuration();
        ApplyResourceRegen();
        ApplyDefendCost();
        ApplyTakeHitRecoveryPercentageDecay();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    protected virtual void UpdatePosition()
    {
        if (statusNetworked == Status.RUN)
        {
            float xChange = baseMoveSpeed * mobilityModifier * Runner.DeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }

        if (statusNetworked == Status.JUMP_UP)
        {
            //Jumping up from the ground
            if (!inAir && Rigid.velocity.y < 0.011)
            {
                float jumpForce = baseJumpForce * (1 + (mobilityModifier - 1) * 0.5f);
                Rigid.velocity = new Vector2(Rigid.velocity.x, Rigid.velocity.y + Mathf.Clamp(jumpForce, 0, maxJumpForce));
            }
        }

        if (statusNetworked == Status.ROLL)
        {
            float xChange = baseRollMoveSpeed * mobilityModifier * Runner.DeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }

        //Moving Left or Right In Air
        if (inAir &&
            statusNetworked != Status.TAKE_HIT && statusNetworked != Status.BEGIN_DEATH && statusNetworked != Status.FINISHED_DEATH)
        {
            float xChange = inAirHorizontalMovementNetworked * baseAirMoveSpeed * mobilityModifier * Runner.DeltaTime;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }

    private void ChangeAnimationSpeed()
    {
        if (MobilityStatus(statusNetworked)) ChampionAnimationController.SetSpeed(mobilityModifier);
        else if (AttackSpeedStatus(statusNetworked)) ChampionAnimationController.SetSpeed(attackSpeed);
        else if (statusNetworked == Status.TAKE_HIT) SetTakeHitAnimationSpeed();
        else ChampionAnimationController.SetSpeed(1);
    }

    protected virtual void UpdateResourceBarVisuals()
    {
        ResourceBar.UpdateResourceBarVisuals(healthNetworked, maxHealth, manaNetworked, maxMana, TakeHitRecoveryPercentage, ultimateMeterNetworked, ultimateMeterCost);
    }

    private void UpdateChampionVisual()
    {
        ChampionAnimationController.Flip(isFacingLeftNetworked);
        ChampionAnimationController.ChangeAnimation((int)statusNetworked);
        ChangeAnimationSpeed();

        //Attack Boxes And Crowd Control Boxes Flip
        if (isFacingLeftNetworked) AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void UpdateGameManagerRoundRankingHost()
    {
        if (healthNetworked > 0) return;
        if (!Runner.IsServer) return;
        if (!GameManager.CanUseGameManager()) return;
        if (GameManager.Instance.GameStateNetworked != GameManager.GameState.ROUND) return;
        if (GameManager.Instance.RoundRankingReversedHost.Contains(this.NetworkedPlayer)) return;

        GameManager.Instance.RoundRankingReversedHost_Add(this.NetworkedPlayer);
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
        ApplyEffects();
        UpdateResourceBarVisuals();
        UpdateChampionVisual();
        UpdateGameManagerRoundRankingHost();
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



    //---------------------------------------------------------------------------------------------------------------------------------------------
    public void HostResetChampion()
    {
        setHealthNetworked(maxHealth);
        setManaNetworked(maxMana);
    }

    public void HostReposition(Transform newPosition)
    {
        transform.position = newPosition.position;
    }

    //Testing
    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RestoreHealthAndMana()
    {
        setHealthNetworked(maxHealth);
        setManaNetworked(maxMana);
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ClearHealthAndMana()
    {
        setHealthNetworked(0);
        setManaNetworked(0);
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_FillUltimateMeter()
    {
        setUltimateMeterNetworked(ultimateMeterCost * 3.0f);
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ClearUltimateMeter()
    {
        setUltimateMeterNetworked(0);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
