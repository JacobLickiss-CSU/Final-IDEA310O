using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EventHeal()
    {
        PlayerManager.Instance.EventHeal();
    }

    public void EventFinishHealing()
    {
        PlayerManager.Instance.EventFinishHealing();
    }

    public void EventFinishResting()
    {
        PlayerManager.Instance.EventFinishResting();
    }

    public void EventStartReseting()
    {
        PlayerManager.Instance.EventStartReseting();
    }

    public void EventGameOver()
    {
        PlayerManager.Instance.EventGameOver();
    }

    public void EventFootstep()
    {
        PlayerManager.Instance.PlayFootstepSound();
    }

    public void EventAttackSwing()
    {
        PlayerManager.Instance.PlayAttackSwingSound();
    }

    public void EventEndingJump()
    {
        PlayerManager.Instance.EventEndingJump();
    }

    public void EventOpenFully()
    {
        PlayerManager.Instance.EventOpenFully();
    }

    public void EventFinishOpen()
    {
        PlayerManager.Instance.EventFinishOpen();
    }

    public void EventStartWeapon()
    {
        PlayerManager.Instance.EventStartWeapon();
    }

    public void EventEndWeapon()
    {
        PlayerManager.Instance.EventEndWeapon();
    }

    public void EventAttackMove()
    {
        PlayerManager.Instance.EventAttackMove();
    }

    public void EventAttackHalt()
    {
        PlayerManager.Instance.EventAttackHalt();
    }

    public void EventAttackStartHolding()
    {
        PlayerManager.Instance.EventAttackStartHolding();
    }

    public void EventAttackStopHolding()
    {
        PlayerManager.Instance.EventAttackStopHolding();
    }

    public void EventAttackFinish()
    {
        PlayerManager.Instance.EventAttackFinish();
    }

    public void EventRollStartMoving()
    {
        PlayerManager.Instance.EventRollStartMoving();
    }

    public void EventRollStopMoving()
    {
        PlayerManager.Instance.EventRollStopMoving();
    }

    public void EventRollStartInv()
    {
        PlayerManager.Instance.EventRollStartInv();
    }

    public void EventRollStopInv()
    {
        PlayerManager.Instance.EventRollStopInv();
    }

    public void EventRollFinish()
    {
        PlayerManager.Instance.EventRollFinish();
    }
}
