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

        if (speed < minSpeed || speed > maxSpeed)
        {
            float targetSpeed = (minSpeed + maxSpeed) * 0.5f;
            Rg.linearVelocity = Rg.linearVelocity.normalized * targetSpeed;
        }
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
            if (col.contactCount > 0)
            {
                Vector2 normal = col.GetContact(0).normal;
                Vector2 reflected = Vector2.Reflect(Rg.linearVelocity.normalized, normal);

                if (reflected.y > -0.2f)
                    reflected.y = -0.2f;

                float targetSpeed = Mathf.Clamp(Rg.linearVelocity.magnitude, minSpeed, maxSpeed);
                Rg.linearVelocity = reflected.normalized * targetSpeed;
            }

            paddle.HitBlock(
                hitObject,
                hitObject.GetComponent<SpriteRenderer>(),
                hitObject.GetComponent<Animator>()
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