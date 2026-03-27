using UnityEngine;

public class Ball : MonoBehaviour
{
    public Paddle paddle;
    public Rigidbody2D Rg;

    [SerializeField] private float minSpeed = 5.7f;
    [SerializeField] private float maxSpeed = 6.0f;

    void Reset()
    {
        Rg = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        KeepBallSpeedStable();
    }

    void KeepBallSpeedStable()
    {
        if (paddle == null || Rg == null)
            return;

        float speed = Rg.linearVelocity.magnitude;

        if (speed <= 0.01f)
            return;

        Vector2 dir = Rg.linearVelocity.normalized;
        float targetSpeed = Mathf.Clamp(speed, minSpeed, maxSpeed);

        Rg.linearVelocity = dir * targetSpeed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (paddle == null || col == null || Rg == null)
            return;

        GameObject hitObject = col.gameObject;

        if (hitObject.CompareTag("Paddle"))
        {
            paddle.BallHitPaddle(transform, Rg);
            return;
        }

        if (hitObject.CompareTag("Block"))
        {
            paddle.HitBlock(
                hitObject,
                hitObject.GetComponent<SpriteRenderer>(),
                null
            );
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (paddle == null || col == null)
            return;

        if (col.CompareTag("DeadLine"))
        {
            paddle.BallOut(gameObject);
        }
    }
}