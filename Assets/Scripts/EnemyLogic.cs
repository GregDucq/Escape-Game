using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO: HAVE MAPGENERATOR SPAWN ENEMY AND PLAYER IN TO ENSURE THAT MAP IS LOADED BEFORE BOTH START REFERENCING THEM!!!

public class EnemyLogic : MonoBehaviour
{
    public GameObject body,
        player,
        maze;
    public GameObject lose;
    public float IDLE_TURN_SPEED = 350f,
        HUNT_TURN_SPEED = 120f,
        CHASE_TURN_SPEED = 300f,
        IDLE_MOVE_SPEED = 3f,
        HUNT_MOVE_SPEED = 7f,
        CHASE_MOVE_SPEED = 9f;
    public int search_distance,
        state;   // 0 = idle (patrols by going from random cell), 
                 // 1 = hunting (will track players location)
                 // 2 = chasing (will active go after player in sight)
                 // 3 = stunned (will remain in place for a few seconds)
    private float turn_speed,
        move_speed,
        stun_duration,
        stun_timer,
        idle_duration,
        idle_timer;
    private float[] last_known_position;
    private float[] idle_target;
    private int maze_height, maze_length, maze_width;
    private float wall_width;
    private Vector3 width;
    private bool hasEnded;

    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        move_speed = IDLE_MOVE_SPEED;
        turn_speed = IDLE_TURN_SPEED;

        search_distance = 5;
        state = 0;
        idle_target = null;
        last_known_position = null;
        maze = null;

        stun_duration = 5f;
        stun_timer = 0f;

        idle_duration = 3f;
        idle_timer = 0f;

        maze = GameObject.Find("MazeManager");
        maze_height = maze.GetComponent<MazeGenerator>().MAZE_HEIGHT;
        maze_length = maze.GetComponent<MazeGenerator>().MAZE_LENGTH;
        maze_width = maze.GetComponent<MazeGenerator>().MAZE_WIDTH;
        wall_width = maze.GetComponent<MazeGenerator>().WALL_WIDTH;

        anim = body.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasEnded)
        {
            int[] indices = maze.GetComponent<MazeGenerator>().coorsToIndices(gameObject.transform.position);
            //Debug.Log("(" + indices[0] + "," + indices[1] + ")");
            // Wait to receive information about maze
            if (maze != null)
            {
                // Don't do anything except wait to be unstunned if stunned
                if (state == 3)
                {
                    stunBehavior();
                }

                // Chase after the player if there is a direct line of sight
                else if (!Physics.Linecast(body.transform.position, player.transform.position, LayerMask.GetMask("Wall")))
                {
                    chasePlayer();
                }

                // otherwise, hunt/wait accordingly
                else
                {
                    float[] direction = maze.GetComponent<MazeGenerator>().getDirection(gameObject.transform.position, player.transform.position, search_distance);

                    // Hunt if player is out of sight but is in range
                    if (direction != null)
                    {
                        huntPlayer(direction);
                    }

                    else
                    {
                        idleBehavior();
                    }
                }
            }
        }
    }

    // Behavior for when enemy is stunned by projectile
    private void stunBehavior()
    {
        if (stun_timer >= stun_duration)
        {
            state = 1;
            anim.SetInteger("EnemyState", 1);
        }

        else
        {
            stun_timer += Time.deltaTime;
        }
    }


    // Chase the player by moving directly towards them if they are in site
    private void chasePlayer()
    {
        anim.SetInteger("EnemyState", 2);
        state = 2;
        turn_speed = CHASE_TURN_SPEED;
        move_speed = CHASE_MOVE_SPEED;
        idle_target = null;
        last_known_position = new float[] { player.transform.position.x, player.transform.position.z };
        
        Vector3 pos = gameObject.transform.position,
            player_pos = player.transform.position;

        width = new Vector3(0.625f, 0, 0);
        LayerMask mask = LayerMask.GetMask("Wall");

        // Move Directly to player if there is enough room
        if (!Physics.Linecast(gameObject.transform.TransformPoint(new Vector3(3f, 1f, 0)), 
            player.transform.TransformPoint(width * 1), 
            mask)
            && !Physics.Linecast(gameObject.transform.TransformPoint(new Vector3(-3f, 1f, 0)),
            player.transform.TransformPoint(width * -1), 
            mask))
        {
            //Debug.Log("Enough room");
            moveTowards(player.transform.position.x, player.transform.position.z);
        } 

        // Otherwise, quickly approach cell where player is
        else
        {
            //Debug.Log("NOT ENOUGH ROOM!");
            float[] direction = maze.GetComponent<MazeGenerator>().getDirection(gameObject.transform.position, player.transform.position);
            if (direction != null) {
                moveTowards(direction[0], direction[1]);
            }

            // Default behavior for if enemy and player are in same cell
            else{
                moveTowards(player.transform.position.x, player.transform.position.z);
            }
        }
    }

    // Behavior for when player is out of sight. Will hunt down player (navigate to their position) if they are
    // close enough (is within a certain number of cells) to track.
    private void huntPlayer(float[] direction)
    {
        // Speed up if player has just entered hunt range
        if (state == 0)
        {
            turn_speed = HUNT_TURN_SPEED;
            move_speed = HUNT_MOVE_SPEED;
        }

        state = 1;
        anim.SetInteger("EnemyState", 1);
        idle_target = null;
        last_known_position = new float[]{player.transform.position.x, player.transform.position.z};
        moveTowards(direction[0], direction[1]);
    }

    // Behavior for when the player is outside of search range. If player just exited out of their sight/range,
    // they will go to the last spot they were suspected to be while maintaining their current speed.
    // Otherwise, they will slowly navigate to a random spot
    private void idleBehavior()
    {
        if (state != 0)
        {
            idle_target = last_known_position;
            state = 0;
            anim.SetInteger("EnemyState", 0);
        }

        else if (idle_timer < idle_duration)
        {
            anim.SetFloat("BlendY", 0);
            idle_timer += Time.deltaTime;
            if (idle_timer >= idle_duration)
            {
                idle_timer = idle_duration;
                System.Random rand = new System.Random();
                int[] randPoint = new int[]{rand.Next(maze_height), rand.Next(maze_length)};
                idle_target = maze.GetComponent<MazeGenerator>().indicesToCoors(randPoint[0], randPoint[1]);
            }
        }

        else {
            Vector3 target = new Vector3(idle_target[0], 0f, idle_target[1]);

            int[] enemy_indices = maze.GetComponent<MazeGenerator>().coorsToIndices(gameObject.transform.position),
                target_indices = maze.GetComponent<MazeGenerator>().coorsToIndices(target);

            if(enemy_indices[0] != target_indices[0] || enemy_indices[1] != target_indices[1])
            {
                float[] direction = maze.GetComponent<MazeGenerator>().getDirection(gameObject.transform.position, target);
                moveTowards(direction[0], direction[1]);
            }

            else
            {
                idle_timer = 0;
                move_speed = IDLE_MOVE_SPEED;
                turn_speed = IDLE_TURN_SPEED;
            }
        }
    }

    private void moveTowards(float x, float z)
    {
        x -= gameObject.transform.position.x;
        z -= gameObject.transform.position.z;

        float angle = gameObject.transform.rotation.eulerAngles.y,
            target_angle = Mathf.Atan2(x, z) * Mathf.Rad2Deg;

        // Preprocess Mathf.Atan2 result so it's range is [0,360)
        target_angle += (target_angle < 0 ? 360 : 0);

        // Get left and right angles between enemy and player
        float right = (target_angle > angle ? target_angle - angle : 360 + (target_angle - angle)),
            left = 360 - right;

        // Determine degree of rotation if difference between enemy's and player's angle is smaller than the default angle rotation speed
        float rot = Mathf.Abs(angle - target_angle) < turn_speed * Time.deltaTime ? Mathf.Abs(angle - target_angle) : turn_speed * Time.deltaTime;

        //Debug.Log("Angle: " + angle + " Target Angle: " + target_angle + " Left Angle: " + left + " Right Angle: "  + right);

        // Rotate left if player is closer to left side, otherwise rotate right
        
        if (left < right || (left == right && left == 180f))
        {
            gameObject.transform.Rotate(new Vector3(0f, -1f * rot, 0f));
        }

        else
        {
            gameObject.transform.Rotate(new Vector3(0f, rot, 0f));
        }

        // Lastly, move enemy towards target
        if ((turn_speed != IDLE_TURN_SPEED && Mathf.Abs(angle - target_angle) < 180) ||
            (turn_speed == IDLE_TURN_SPEED && Mathf.Abs(angle - target_angle) < 55 ))
        {
            gameObject.transform.Translate(new Vector3(0f, 0f, move_speed * Time.deltaTime));
            anim.SetFloat("BlendY", move_speed);
        }
    }

    public void stun()
    {
        if (state != 3)
        {
            state = 3;
            anim.SetInteger("EnemyState", 3);
            stun_timer = 0f;
        }
    }

    public void stopEnemy(bool won)
    {
        hasEnded = true;

        if (won)
        {
            anim.Play("Wins");
        }

        else
        {
            anim.Play("Loses");
        }
    }
}
