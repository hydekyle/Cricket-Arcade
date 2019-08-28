using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bate : MonoBehaviour
{
    public float repeatCD;
    public Material afterHitMaterial;
    public Text debugText;
    public LayerMask afterHitMask;
    public float lastTime_moved;
    public SpriteRenderer bate_sprite;
    Animator animator_brazo;
    Camera myCamera;

    private void Start()
    {
        animator_brazo = GetComponent<Animator>();
        myCamera = Camera.main;
    }

    private void Update()
    {
        PlayerInputs();
    }

    private void PlayerInputs()
    {
        if (!Application.isMobilePlatform)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) Touched(true);
            if (Input.GetKeyDown(KeyCode.Mouse1)) Touched(false);
            if (GameManager.Instance.mode2H) MoveToRaton();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (GameManager.Instance.mode2H) Touched(transform.position.x < 0 ? false : true); //Gira según la posición en pantalla.
                else Touched(Camera.main.ScreenToViewportPoint(Input.mousePosition).x < 0.5f ? true : false); //Gira según donde pulses.
            }
            if (GameManager.Instance.mode2H) MoveWithAccelerometer();
        }
    }

    void MoveToRaton()
    {
        float xDestiny = Mathf.Clamp(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, -2f, 2f);
        transform.position = Vector3.Lerp(transform.position, new Vector3(xDestiny, transform.position.y, 0), Time.deltaTime * 6);
    }

    void MoveWithAccelerometer()
    {
        if (Mathf.Abs(transform.position.x - Mathf.Clamp(Input.acceleration.normalized.x * 10, -2f, 2f)) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(Mathf.Clamp(Input.acceleration.normalized.x * 10, -2f, 2f), -4, 0), Time.deltaTime * 8);
        }
    }

    void Touched(bool goRight)
    {
        GolpearBate(goRight);
    }

    void GolpearBate(bool inversed)
    {
        if (Time.time > lastTime_moved + repeatCD && !GameManager.Instance.gameIsStop)
        {
            animator_brazo.Play(inversed ? "BateGirarInv" : "BateGirar");
            lastTime_moved = Time.time;
            bate_sprite.flipX = !inversed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector3 viewport_col = myCamera.ScreenToViewportPoint(myCamera.WorldToScreenPoint(collision.transform.position));
        if (!animator_brazo.GetCurrentAnimatorStateInfo(0).IsName("Parado") &&  viewport_col.y > 0.1f)
        {
            var rb = collision.GetComponent<Rigidbody2D>();
            var trail_renderer = collision.GetComponent<TrailRenderer>();
            Vector3 salirDisparado;
            salirDisparado = Vector3.Reflect(transform.up.normalized, rb.velocity.normalized) * (70 - Mathf.Clamp(Mathf.Abs(collision.transform.position.y), -4, 4) * 13);
            if (salirDisparado.y <= 0) salirDisparado = new Vector3(salirDisparado.x, salirDisparado.y * -1, 0); //Asegura que la bola vaya hacia arriba.
            if (salirDisparado.y < 2) salirDisparado = new Vector3(salirDisparado.x, salirDisparado.y + 4, 0); //Si la fuerza de rebote es pequeña, dar un impulso.
            rb.velocity = new Vector2(salirDisparado.x * - 1, salirDisparado.y) * 1.4f;
            trail_renderer.time = Mathf.Clamp(0.40f + collision.transform.position.y / 10, 0.1f, 0.3f);
            trail_renderer.material = afterHitMaterial;
            collision.gameObject.layer = afterHitMask - 1;
            GameManager.Instance.BallStrike(rb.velocity.magnitude);
        }
    }
}
