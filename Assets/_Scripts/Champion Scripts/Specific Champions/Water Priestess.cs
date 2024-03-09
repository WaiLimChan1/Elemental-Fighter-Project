using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPriestess : Champion
{
    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength * 1 / 5, crowdControlStrength));
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status.UNIQUE1 : Begin_Meditation
    //Status.UNIQUE2 : Meditation
    //Status.UNIQUE3 : Slide

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE3);
    }

    protected override bool InterruptableStatus()
    {
        return (base.InterruptableStatus() ||
            status == Status.UNIQUE1 || status == Status.UNIQUE2);
    }

    protected override void TakeInput()
    {
        base.TakeInput();

        if (dead)
        {
            return;
        }

        if (!inAir && InterruptableStatus())
        {
            if (Input.GetKey(KeyCode.Q))
            {
                //Begin_Meditation, then Begin_Meditation into Meditation, then if already Meditation, continue Meditation
                Status lastStatus = (Status)ChampionAnimationController.GetAnimatorStatus();
                if (lastStatus != Status.UNIQUE1 && lastStatus != Status.UNIQUE2) status = Status.UNIQUE1;
                else if (lastStatus == Status.UNIQUE1)
                {
                    if (ChampionAnimationController.AnimationFinished()) status = Status.UNIQUE2;
                    else status = Status.UNIQUE1;
                }
                else if (lastStatus == Status.UNIQUE2) status = Status.UNIQUE2;

            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.Q)) status = Status.UNIQUE3;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        if (statusNetworked == Status.UNIQUE3)
        {
            float xChange = rollMoveSpeed * Time.fixedDeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
