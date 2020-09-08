using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float rotate_speed;
    public float sprint_factor;
    public bool hasEnded;

    public GameObject body, bullet;
    public Text message, sprint_meter, rock_available;

    public Camera thirdPerson,
        firstPerson,
        birdEye;
    public Animator anim;

    private int currentCamera;
    private Rigidbody rb;
    private float energy,
        projectile_cooldown,
        projectile_timer;
    private bool can_sprint,
        is_sprinting;
    private Vector3 last_position;

    private float back_to_menu_delay;

    // Start is called before the first frame update
    void Start()
    {
        speed = 1250f;
        rotate_speed = 125f;
        sprint_factor = 1.75f;
        rb = gameObject.GetComponent<Rigidbody>();
        hasEnded = false;

        currentCamera = 0;
        firstPerson.enabled = false;
        birdEye.enabled = false;

        energy = 10f;
        can_sprint = true;
        is_sprinting = false;
        last_position = gameObject.transform.position;

        sprint_meter.color = Color.green;
        sprint_meter.text = "Sprint: " + (int)(energy / 10) * 100 + "%";

        rock_available.color = Color.white;
        rock_available.text = "Rock Ready!";

        projectile_cooldown = 10f;
        projectile_timer = projectile_cooldown;
        anim = body.GetComponent<Animator>();

        back_to_menu_delay = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!hasEnded)
        {
            float mouseMove = Input.GetAxis("Mouse X");
            rb.MoveRotation(Quaternion.Euler(gameObject.transform.rotation.eulerAngles + new Vector3(0, mouseMove * rotate_speed, 0) * Time.deltaTime));
        }
    }

    void Update()
    {
        // Continue game if it has not ended
        if (!hasEnded)
        {
            /******************************************************************
             * SPRINTING BEHAVIOR
             ******************************************************************/
            is_sprinting = false;

            // Activate is_sprinting flag if energy is left
            if (Input.GetKey(KeyCode.LeftShift) && can_sprint)
            {
                // Activate is_sprinting flag if energy is left


                // If not energy remains, prevent player from continuing to Sprint
                if (energy <= 0f)
                {
                    can_sprint = false;
                    energy = 0f;
                    sprint_meter.color = Color.red;
                }

                else
                {
                    is_sprinting = true;
                }
            }

            /******************************************************************
            * PLAYER MOVEMENT
            ******************************************************************/

            float inputx = Input.GetAxis("Horizontal"),
                inputy = Input.GetAxis("Vertical");

            // Only remove sprint energy if player is moving
            if (is_sprinting && gameObject.transform.position != last_position)
            {
                energy -= 1 * Time.deltaTime;
            }

            // Recharge if player is not sprinting
            else
            {
                if (energy >= 10f)
                {
                    can_sprint = can_sprint || !(Input.GetKey(KeyCode.LeftShift)); // Assuming player cannot sprint, allow them to once they release shift
                    energy = 10f;
                    sprint_meter.color = Color.green;
                }

                else
                {
                    energy += 1 * Time.deltaTime;
                }
            }

            sprint_meter.text = "Sprint: " + (energy < 0 ? 0 : (int)((energy / 10f) * 100)) + "%";

            Vector3 movement = new Vector3(inputx, 0, inputy) * (is_sprinting ? sprint_factor : 1);

            anim.SetFloat("BlendX", movement.x);
            anim.SetFloat("BlendY", movement.z);

            // Determine which set of animations to play for player

            // Not moving (idle)
            if (Mathf.Abs(movement.x) > 1 || Mathf.Abs(movement.z) > 1)
            {
                anim.SetInteger("MovementState", 2);
            }

            // Walking
            else if (Mathf.Abs(movement.x) > 0 || Mathf.Abs(movement.z) > 0)
            {
                anim.SetInteger("MovementState", 1);
            }

            else
            {
                anim.SetInteger("MovementState", 0);
            }

            //Debug.Log(movement.x + " " + movement.y + " -> " + anim.GetInteger("MovementState"));

            rb.AddRelativeForce(movement * speed);

            /******************************************************************
            * CAMERA CONTROLS
            ******************************************************************/
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentCamera = (currentCamera + 1) % 3;

                if (currentCamera == 0)
                {
                    thirdPerson.enabled = true;
                    birdEye.enabled = false;
                }

                else if (currentCamera == 1)
                {
                    firstPerson.enabled = true;
                    thirdPerson.enabled = false;
                }

                else
                {
                    birdEye.enabled = true;
                    firstPerson.enabled = false;
                }
            }

            /******************************************************************
            * PROJECTILE CONTROLS
            ******************************************************************/

            if (projectile_timer > projectile_cooldown)
            {
                projectile_timer = projectile_cooldown;
                rock_available.color = Color.white;
                rock_available.text = "Rock Ready!";
            }

            else
            {
                projectile_timer += Time.deltaTime;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (projectile_cooldown == projectile_timer)
                {
                    Vector3 pos = gameObject.transform.position;

                    anim.Play("Throw");
                    Instantiate(bullet, gameObject.transform.TransformPoint(Vector3.forward * 0.5f + Vector3.up), transform.rotation);
                    //anim.SetInteger("MovementState", temp);
                    //Instantiate(bullet, gameObject.transform.TransformPoint(Vector3.forward * 0.5f + Vector3.up), transform.rotation);
                    projectile_timer = 0;
                    rock_available.color = Color.black;
                    rock_available.text = "Looking for Rock...";
                }
            }

            last_position = gameObject.transform.position;
        }

        else
        {
            if (back_to_menu_delay < 5f)
            {
                back_to_menu_delay += Time.deltaTime;
                //Debug.Log(back_to_menu_delay);
            }

            else
            {
                //Debug.Log("Loading Menu");
                StartCoroutine(loadMenu());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("enemy") && other.GetComponent<EnemyLogic>().state != 3)
        {
            other.GetComponent<EnemyLogic>().stopEnemy(true);
            playerLoses();
        }
    }

    public void playerLoses()
    {
        hasEnded = true;
        anim.Play("Dies");
        message.color = Color.red;
        message.text = "You Lose...";
    }

    public void playerWins()
    {
        hasEnded = true;
        anim.Play("Wins");
        message.color = Color.green;
        message.text = "You Win!";
    }

    public IEnumerator loadMenu()
    {
        //Debug.Log("Begin Menu Load");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
