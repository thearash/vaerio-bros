using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event Action OnDeath;
    CapsuleCollider2D playerCollider;


    public Rigidbody2D thisRigidbody;
    [SerializeField] LayerMask floorCheckLayers;

    private bool dead = false;

    public float walkInput;
    public float jumpInput;
    private bool bounce;

    public bool IsOnFloor { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        walkInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetAxisRaw("Vertical");
        jumpInput = Mathf.Clamp01(jumpInput);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.gameObject.GetComponent<WalkerEnemy>();
        if (enemy != null)
        {
            var disp = thisRigidbody.position - (Vector2)enemy.transform.position;
            var ang = Vector2.Angle(disp, Vector2.up);
            if (ang > 50)
            {
                Die();
            }
            else
            {
                enemy.Squished();
                bounce = true;
            }
        }
    }

    private void Die()
    {
        if (!dead)
        {
            dead = true;
            if (OnDeath != null)
            {
                OnDeath();
            }
        }
    }

    void FixedUpdate()
    {
        if (Physics2D.simulationMode == SimulationMode2D.FixedUpdate)
        {
            PhysiscsStep();
        }
    }

    /// <summary>
    /// do one physics step. This is outsourced since we need it for the A* simulation too.
    /// </summary>
    public void PhysiscsStep()
    {
        var currentVel = thisRigidbody.velocity;

        IsOnFloor = ForcePhysCheckGround();
        if (!IsOnFloor) {
            jumpInput = 0;
        }

        currentVel = SharedData.ComputePlayerVelocity(currentVel, walkInput, jumpInput, Time.deltaTime);
        if(bounce)
        {
            currentVel.y = SharedData.bouncePower;
            bounce = false;
        }

        thisRigidbody.velocity = currentVel;

        // check fallen out the map
        if (thisRigidbody.position.y < GameManager.KillHeight)
        {
            Die();
        }
    }

    public bool ForcePhysCheckGround()
    {
        var colliderHeight = (playerCollider.size.y * 0.5f) + 0.01f;
        var checkPoint = thisRigidbody.position + (Vector2.down * colliderHeight);
        var col = Physics2D.OverlapPoint(checkPoint, floorCheckLayers);
        return col != null;
    }
}
