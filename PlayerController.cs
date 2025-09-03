using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 移動スピード
    public int health = 5;              // プレイヤー体力
    public float invincibleTime = 2f;   // 無敵時間（秒）
    private float invincibleTimer = 0f;
    public float jumpForce = 3f; // ジャンプ力

    private Collider2D myCol; // 当たり判定
    private SpriteRenderer sr;

    private Rigidbody2D rb;
    private float moveGroundSpeed = 0f; // 移動床の速度
    private bool isGrounded = false;

    void Start()
    {
        myCol = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 左右移動
        float moveX = 0f;
        if (Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
            rb.linearVelocity = new Vector2(moveX * moveSpeed + moveGroundSpeed, rb.linearVelocity.y);

        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
            rb.linearVelocity = new Vector2(moveX * moveSpeed + moveGroundSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveGroundSpeed, rb.linearVelocity.y);
        }

        // ジャンプ
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }

        // 無敵時間のカウントダウン
        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;

            // 半透明にする（alpha = 0.5）
            SetTransparency(0.5f);
        }
        else
        {
            // 通常の表示（alpha = 1）
            SetTransparency(1f);
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol != null)
            {
                // OverlapColliderで重なっているかを検知
                ContactFilter2D filter = new ContactFilter2D();
                Collider2D[] results = new Collider2D[1];
                int count = myCol.Overlap(filter.NoFilter(), results);

                for (int i = 0; i < count; i++)
                {
                    if (results[i] == enemyCol && invincibleTimer <= 0f)
                    {
                        health -= enemy.GetComponent<Enemy>().damage;
                        Debug.Log("ダメージ！残り体力: " + health);
                        invincibleTimer = invincibleTime;
                    }
                }
            }
        }
    }

    // 床判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") ||
        collision.gameObject.CompareTag("Block") ||
         collision.gameObject.CompareTag("MoveGround"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveGround"))
        {
            Rigidbody2D groundRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (groundRb != null)
            {
                moveGroundSpeed = groundRb.linearVelocity.x;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveGround"))
        {
            moveGroundSpeed = 0f;
        }

    }

    // Spriteの透明度を変更する関数
    private void SetTransparency(float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}
