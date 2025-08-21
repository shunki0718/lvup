using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;        // 通常の移動速度
    public float dashSpeed = 10f;       // Shiftキー長押し中の移動速度

    [Header("Jump")]
    public float jumpForce = 7f;        // ジャンプの推進力

    [Header("Crouch")]
    public float crouchSpeed = 2.5f;    // しゃがみ中の移動速度
    [Range(0.2f, 1f)]
    public float crouchHeightRatio = 0.5f; // しゃがみ中のコライダーの高さ比率 (0.2~1.0)

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private bool isGrounded = false;
    private bool isCrouching = false;

    private Vector2 originalColSize;
    private Vector2 originalColOffset;

    [SerializeField] private Transform visual;       // 見た目用の子(Visual)を割り当てる
    private SpriteRenderer sr;                       // Visual上のSpriteRenderer
    private Vector3 visualOrigScale;                 // Visualの元のスケール
    private Vector3 visualOrigLocalPos;              // Visualの元のローカル位置
    private float visualOrigWorldHeight;             // Visualの元のワールド高さ(足元補正用)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        // Visualを取得（インスペクタで未設定なら子から探す）
        if (visual == null)
            visual = transform.Find("Visual");

        if (visual != null)
        {
            sr = visual.GetComponent<SpriteRenderer>();
            visualOrigScale = visual.localScale;
            visualOrigLocalPos = visual.localPosition;

            // 元のワールド高さを記録（後で縮小時の足元補正に使用）
            if (sr != null) visualOrigWorldHeight = sr.bounds.size.y;
        }
        else
        {
            Debug.LogWarning("Visual(見た目用の子)が見つかりません。足元補正が無効になります。");
        }

        originalColSize = col.size;
        originalColOffset = col.offset;
    }

    void Update()
    {
        // 水平方向の入力 (-1, 0, 1)
        float move = Input.GetAxisRaw("Horizontal");

        // しゃがみは地面にいるときのみ可能
        bool wantCrouch = (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) && isGrounded;
        if (wantCrouch && !isCrouching) EnterCrouch();
        if (!wantCrouch && isCrouching) ExitCrouch();

        // 速度の選択（しゃがみ中はダッシュは無視される）
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? dashSpeed : moveSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;

        // --- 移動処理 ---
        if (isCrouching && isGrounded)
        {
            // しゃがみ中 & 地面にいる → 移動禁止
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            // 通常移動
            rb.linearVelocity = new Vector2(move * currentSpeed, rb.linearVelocity.y);
        }

        // --- ジャンプ処理（しゃがみ中は無効） ---
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.W)) && isGrounded && !isCrouching)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }


    // --- しゃがみ開始 ---
    private void EnterCrouch()
    {
        if (col == null) return;
        isCrouching = true;

        // 当たり判定は「足を地面に残したまま」縮める
        float newHeight = originalColSize.y * crouchHeightRatio;
        float deltaH = originalColSize.y - newHeight;
        col.size = new Vector2(originalColSize.x, newHeight);
        col.offset = new Vector2(originalColOffset.x, originalColOffset.y - deltaH * 0.5f);

        // 見た目だけ縦に縮小（横幅はそのまま）
        if (visual != null)
        {
            visual.localScale = new Vector3(visualOrigScale.x,
                                             visualOrigScale.y * crouchHeightRatio,
                                             visualOrigScale.z);

            // Pivotが中央などで足が浮く場合に備えて足元を補正
            if (sr != null && visualOrigWorldHeight > 0f)
            {
                float lost = visualOrigWorldHeight * (1f - crouchHeightRatio); // 失われた高さ（ワールド空間）
                                                                               // 見た目を半分だけ下げて足元を地面に合わせる（PivotがBottomなら移動量は0になる）
                visual.localPosition = visualOrigLocalPos + new Vector3(0f, -lost * 0.5f, 0f);
            }
        }
    }


    // --- しゃがみ終了 ---
    private void ExitCrouch()
    {
        if (col == null) return;
        isCrouching = false;

        // 当たり判定を元のサイズに戻す
        col.size = originalColSize;
        col.offset = originalColOffset;

        // 見た目も元の状態に戻す
        if (visual != null)
        {
            visual.localScale = visualOrigScale;
            visual.localPosition = visualOrigLocalPos;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // "Ground"タグの床が必要
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}
