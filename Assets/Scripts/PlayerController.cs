using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D collider;
    private GameObject bottomCollider;
    private LayerMask levelEnviromentMask;

    private bool isFlying = false;
    private bool blockStateChanger = false;
    private float colliderNormalHeight;

    public float jumpPower = 12.0f;
    public float movementSpeed = 20.0f;
    public TrunksStates state = TrunksStates.PowerUp;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();
        colliderNormalHeight = collider.size.y;
        bottomCollider = this.transform.FindChild("FeetCollider").gameObject;
        levelEnviromentMask = LayerMask.NameToLayer("Level");
        TryChangeState(TrunksStates.PowerUp);
        blockStateChanger = true;
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        if (this.state == TrunksStates.PowerUp)
        {
            return;
        }

        isFlying = IsPlayerFlying();

        float horizontalMovement = Input.GetAxis("Horizontal");
        if (horizontalMovement != 0)
        {
            MoveOverX(horizontalMovement);
        }

        float verticalMovement = Input.GetAxis("Vertical");
        if (verticalMovement > 0 && !isFlying)
        {
            MoveOverY();
        }

        if (isFlying)
        {
            TryChangeState(TrunksStates.Fly);
        }

        bool isAttacking = Input.GetButton("Fire1");
        if (isAttacking && !blockStateChanger)
        {
            TryChangeState(TrunksStates.Attack);
            blockStateChanger = true;
        }

        if (!isFlying && horizontalMovement == 0 && !isAttacking)
        {
            var verticalVelocity = rb.velocity.y;
            rb.velocity = new Vector2(0, verticalVelocity);
            TryChangeState(TrunksStates.Stand);
        }
    }

    public void Attack()
    {
        // TODO: Perform attack
        blockStateChanger = false;
        TryChangeState(TrunksStates.Stand);
    }

    public void PoweredUp()
    {
        blockStateChanger = false;
        TryChangeState(TrunksStates.Stand);
    }

    public void TryChangeState(TrunksStates state)
    {
        // Prevent change of state.
        if (this.state == state || blockStateChanger)
        {
            return;
        }

        // Restore collider orientation.
        if (this.state == TrunksStates.Fly)
        {
            ShrinkCollider();
        }

        // Conditions for changing from current state to new state.
        bool canChangeState = true;
        switch (state)
        {
            case TrunksStates.Stand:
                canChangeState = this.state != TrunksStates.MakeBlock;
                SetTriggers(false, true, true);
                break;
            case TrunksStates.Fly:
                ShrinkCollider();
                canChangeState = this.state != TrunksStates.MakeBlock;
                SetTriggers(true, false, true);
                break;
            case TrunksStates.Attack:
                canChangeState = this.state != TrunksStates.MakeBlock;
                SetTriggers(true, true, false);
                break;
            case TrunksStates.Move:
                canChangeState = (this.state == TrunksStates.Fly
                                    || this.state == TrunksStates.Stand
                                    || this.state == TrunksStates.Attack);
                SetAllTriggers();
                break;
            case TrunksStates.MakeBlock:
                canChangeState = (this.state == TrunksStates.Attack
                                    || this.state == TrunksStates.Stand);
                SetAllTriggers();
                break;
            case TrunksStates.DismissBlock:
                canChangeState = this.state == TrunksStates.MakeBlock;
                break;
            case TrunksStates.PowerUp:
                SetAllTriggers();
                break;
        }
        
        if (canChangeState)
        {
            this.state = state;
            anim.SetInteger("State", (int)state);
        }
    }

    private void HandleMovementDirection(float horizontalMovement)
    {
        bool isMovingLeft = horizontalMovement < 0;
        if ((isMovingLeft && this.transform.localScale.x > 0)
            || (!isMovingLeft && this.transform.localScale.x < 0))
        {
            Flip();
        }
    }

    private bool IsPlayerFlying()
    {
        return !(bottomCollider.GetComponent<BoxCollider2D>()
                                        .IsTouchingLayers(levelEnviromentMask));
    }

    private void MoveOverY()
    {
        isFlying = true;
        var velocity = rb.velocity;
        velocity.y = jumpPower;
        rb.velocity = velocity;
    }

    private void MoveOverX(float horizontalMovement)
    {
        var velocity = rb.velocity;
        velocity.x = horizontalMovement * movementSpeed;
        rb.velocity = velocity;
        if (!isFlying)
        {
            TryChangeState(TrunksStates.Move);
        }

        HandleMovementDirection(horizontalMovement);
    }

    private void Flip()
    {
        var scale = this.transform.localScale;
        scale.x *= -1;
        this.transform.localScale = scale;
    }

    private void ShrinkCollider()
    {
        float x = collider.size.x;
        float y = collider.size.y;
        if (x == y)
        {
            y = colliderNormalHeight;
        }
        else
        {
            y = x;
        }

        collider.size = new Vector2(x, y);
    }

    private void SetAllTriggers()
    {
        SetTriggers(true, true, true);
    }

    private void SetTriggers(bool canStand, bool canFly, bool canAttack)
    {
        if (canStand)
        {
            anim.SetTrigger("Standing");
        }

        if (canFly)
        {
            anim.SetTrigger("Flying");
        }

        if (canAttack)
        {
            anim.SetTrigger("Attacking");
        }
    }
}
