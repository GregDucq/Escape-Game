using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    // true = wall, false = no wall;
    // walls[0] = left, walls[1] = top, walls[2] = right, walls[3] = bottom
    public bool[] walls;
    // role = 0 means cell serves no role, role = 1 means start point, role = 2 means end point
    public Cell()
    {
        walls = new bool[4] { false, false, false, false };
    }
}

public class MazeGenerator : MonoBehaviour
{
    public GameObject wall;
    public GameObject player;
    public GameObject enemy;
    public GameObject ground;
    public GameObject portal;
    public GameObject parent;


    public int MAZE_HEIGHT = 15,
        MAZE_LENGTH = 15,
        MAZE_WIDTH = 7,
        start,
        end;
    public float WALL_WIDTH = 2f;
    private Cell[][] maze;


    // Start is called before the first frame update
    void Start()
    {
        MAZE_LENGTH = PlayerPrefs.GetInt("MAZE_LENGTH");
        MAZE_HEIGHT = PlayerPrefs.GetInt("MAZE_HEIGHT");
        MAZE_WIDTH = PlayerPrefs.GetInt("MAZE_WIDTH");
        WALL_WIDTH = PlayerPrefs.GetInt("WALL_WIDTH");

        maze = new Cell[MAZE_HEIGHT][];
        // Initialize maze with all walls
        for (int i = 0; i < MAZE_HEIGHT; i++)
        {  
            maze[i] = new Cell[MAZE_LENGTH];
            for(int j = 0; j < MAZE_LENGTH; j++){
                maze[i][j] = null;
            }
        }

        // Create random maze
        randomizeMaze();

        // Use matrix to create world
        generateMapFromMaze();

        // Spawn player and enemy into the world
        spawnCharacters();
    }

    void Update()
    {
        checkIfPlayerWon();
    }

    private void randomizeMaze()
    {
        System.Random rand = new System.Random();
        List<int[]> cellsToVisit = new List<int[]>();
        HashSet<int> cellsFound = new HashSet<int>();
        //int endToPullFrom = 0; // 2 = pull from top of list,  0, 1= pull from bottom of list

        int start_pos = 0;

        switch (rand.Next(4))
        {
            // Start on left side
            case 0:
                start_pos = rand.Next(MAZE_HEIGHT - 1);
                cellsToVisit.Add(new int[] { (MAZE_LENGTH * (start_pos)), -1 });
                cellsFound.Add(MAZE_LENGTH * (start_pos));
                break;
            // Start on top
            case 1:
                start_pos = rand.Next(MAZE_LENGTH - 1);
                cellsToVisit.Add(new int[] { (MAZE_LENGTH - start_pos - 1), -1 });
                cellsFound.Add(MAZE_LENGTH - (start_pos));
                break;
            // start to the right side
            case 2:
                start_pos = rand.Next(MAZE_HEIGHT - 1);
                cellsToVisit.Add(new int[] { (MAZE_LENGTH * (MAZE_HEIGHT - start_pos - 1) + (MAZE_LENGTH - 1)), -1 });
                cellsFound.Add((MAZE_LENGTH * (MAZE_HEIGHT - start_pos) + MAZE_LENGTH));
                break;
            // Start on bottom
            case 3:
                start_pos = rand.Next(MAZE_LENGTH - 1);
                cellsToVisit.Add(new int[] { (MAZE_LENGTH * (MAZE_HEIGHT - 1) + start_pos), -1 });
                cellsFound.Add(MAZE_LENGTH * (MAZE_LENGTH * (MAZE_HEIGHT - 1) + start_pos));
                break;
        }

        // Start depth first search process
        while (!(cellsToVisit.Count == 0))
        {
            int index = cellsToVisit.Count - 1;

            // Get position info from list
            int[] info = cellsToVisit[index];
            cellsToVisit.RemoveAt(index);

            int pos = info[0],
                a = pos / MAZE_LENGTH,
                b = pos % MAZE_LENGTH,
                l_pos = info[1];

            // If position has not been visited yet
            if (maze[a][b] == null)
            {
                maze[a][b] = new Cell();

                // Mark outer walls of maze
                if (b == 0)
                {
                    maze[a][b].walls[0] = true;
                }

                if (a == 0)
                {
                    maze[a][b].walls[1] = true;
                }

                if (b == MAZE_LENGTH - 1)
                {
                    maze[a][b].walls[2] = true;
                }

                if (a == MAZE_HEIGHT - 1)
                {
                    maze[a][b].walls[3] = true;
                }

                //Debug.Log("pos = " + pos + " l_pos = " + l_pos);

                // Push valid sides to check to stack
                List<int> sidesToCheck = new List<int>(new int[] { 0, 1, 2, 3 }); // 0 - left, 1 - top, 2 - right, 3 - bottom

                // While there are still sides to check
                while (!(sidesToCheck.Count == 0))
                {   
                    int i = rand.Next(sidesToCheck.Count),
                        s = sidesToCheck[i];
                    sidesToCheck.RemoveAt(i);

                    // Check if left side is valid
                    if (s == 0 && b != 0)
                    {
                        if (maze[a][b - 1] == null)
                        {
                            cellsToVisit.Add(new int[] { pos - 1, pos });
                            cellsFound.Add(pos - 1);
              
                        }

                        else if (pos - 1 != l_pos)
                        {
                            //Debug.Log("pos " + pos + " wall left");
                            maze[a][b].walls[0] = true;
                            maze[a][b - 1].walls[2] = true;
                        }
                    }

                    // Check if top side is valid
                    else if (s == 1 && a != 0)
                    {
                        if (maze[a - 1][b] == null)
                        {                            
                            cellsToVisit.Add(new int[] { pos - MAZE_LENGTH, pos });
                            cellsFound.Add(pos - MAZE_LENGTH);
                        }

                        else if (pos - MAZE_LENGTH != l_pos)
                        {
                            //Debug.Log("pos " + pos + " wall top");
                            maze[a][b].walls[1] = true;
                            maze[a - 1][b].walls[3] = true;
                        }
                    }

                    // Check if right side is valid
                    else if (s == 2 && b != (MAZE_LENGTH - 1))
                    {
                        if (maze[a][b + 1] == null)
                        {

                            cellsToVisit.Add(new int[] { pos + 1, pos });
                            cellsFound.Add(pos + 1);
                            
                        }

                        else if (pos + 1 != l_pos)
                        {
                            //Debug.Log("pos " + pos + " wall right");
                            maze[a][b].walls[2] = true;
                            maze[a][b + 1].walls[0] = true;
                        }
                    }

                    // Check if bottom side is valid
                    else if (s == 3 && a != (MAZE_HEIGHT - 1))
                    {
                        if (maze[a + 1][b] == null)
                        {
                            cellsToVisit.Add(new int[] { pos + MAZE_LENGTH, pos });
                            cellsFound.Add(pos + MAZE_LENGTH);
                        }

                        else if (pos + MAZE_LENGTH != l_pos)
                        {
                            //Debug.Log("pos " + pos + " wall bottom");
                            maze[a][b].walls[3] = true;
                            maze[a + 1][b].walls[1] = true;
                        }
                    }
                }
            }
        }
    }

    // Construct game world from the maze matrix that has been populated
    private void generateMapFromMaze()
    {
        // Randomly pick where player needs to start and end 
        // (end will always be on wall opposite of where player starts)
        System.Random rand = new System.Random();
        int start_pos,
            end_pos;
        float[] pos;
        GameObject newPortal;
        switch (rand.Next(4))
        {
            // Start on left side
            case 0:
                start_pos = rand.Next(MAZE_HEIGHT - 1);
                end_pos = rand.Next(MAZE_HEIGHT - 1);

                start = (MAZE_LENGTH * (start_pos));
                end = MAZE_LENGTH * (MAZE_HEIGHT - end_pos - 1) + (MAZE_LENGTH - 1);

                maze[end / MAZE_LENGTH][end % MAZE_LENGTH].walls[2] = false;

                pos = indicesToCoors(end / MAZE_LENGTH, end % MAZE_LENGTH);
                newPortal = Instantiate(portal, new Vector3(pos[0] + MAZE_WIDTH / 2f, (MAZE_WIDTH * 0.5f) + 1, pos[1]), Quaternion.identity);
                newPortal.transform.localScale = new Vector3(1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f));
                newPortal.transform.Rotate(new Vector3(0, 90f, 0));

                break;
            // Start on top
            case 1:
                start_pos = rand.Next(MAZE_LENGTH - 1);
                end_pos = rand.Next(MAZE_LENGTH - 1);

                start = MAZE_LENGTH - start_pos - 1;
                end = MAZE_LENGTH * (MAZE_HEIGHT - 1) + end_pos;

                maze[end / MAZE_LENGTH][end % MAZE_LENGTH].walls[3] = false;

                pos = indicesToCoors(end / MAZE_LENGTH, end % MAZE_LENGTH);
                newPortal = Instantiate(portal, new Vector3(pos[0], (MAZE_WIDTH * 0.5f) + 1, pos[1] - (MAZE_WIDTH) / 2f), Quaternion.identity);
                newPortal.transform.localScale = new Vector3(1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f));

                break;
            // start to the right side
            case 2:
                start_pos = rand.Next(MAZE_HEIGHT - 1);
                end_pos = rand.Next(MAZE_HEIGHT - 1);

                start = MAZE_LENGTH * (MAZE_HEIGHT - start_pos - 1) + (MAZE_LENGTH - 1);
                end = MAZE_LENGTH * end_pos;

                maze[end / MAZE_LENGTH][end % MAZE_LENGTH].walls[0] = false;

                pos = indicesToCoors(end / MAZE_LENGTH, end % MAZE_LENGTH);
                newPortal = Instantiate(portal, new Vector3(pos[0] - (MAZE_WIDTH) / 2f, (MAZE_WIDTH * 0.5f) + 1, pos[1]), Quaternion.identity);
                newPortal.transform.localScale = new Vector3(1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f));
                newPortal.transform.Rotate(new Vector3(0, 90f, 0));

                break;

            // Start on bottom
            case 3:
                start_pos = rand.Next(MAZE_LENGTH - 1);
                end_pos = rand.Next(MAZE_LENGTH - 1);

                start = MAZE_LENGTH * (MAZE_HEIGHT - 1) + start_pos;
                end = MAZE_LENGTH - end_pos - 1;

                maze[end / MAZE_LENGTH][end % MAZE_LENGTH].walls[1] = false;

                // Create portal
                pos = indicesToCoors(end / MAZE_LENGTH, end % MAZE_LENGTH);
                newPortal = Instantiate(portal, new Vector3(pos[0], (MAZE_WIDTH * 0.5f) + 1, pos[1] + (MAZE_WIDTH) / 2f), Quaternion.identity);
                newPortal.transform.localScale = new Vector3(1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f), 1 / 40f * (MAZE_WIDTH*0.75f));
                break;
        }

        // Start placing walls in world
        float width = MAZE_WIDTH + WALL_WIDTH;
        float z = (((float)MAZE_HEIGHT)/2 - 1) * width, 
            x;

        ground.transform.localScale = new Vector3(width - WALL_WIDTH, 1f, width - WALL_WIDTH);
        for (int i = 0; i < MAZE_HEIGHT; i++)
        {
            x = -1 * (((float)MAZE_LENGTH)/2) * width;

            for (int j = 0; j < MAZE_LENGTH; j++)
            {
                pos = indicesToCoors(i,j);

                Instantiate(ground, new Vector3(pos[0],0,pos[1]), Quaternion.identity, parent.transform);

                Cell cell = maze[i][j];

                // Add wall to left side
                if (cell.walls[0] && j == 0)
                {
                    GameObject new_wall = Instantiate(wall, new Vector3(pos[0] - (MAZE_WIDTH + WALL_WIDTH) / 2f, 0, pos[1]), Quaternion.identity, parent.transform);
                    //new_wall.transform.localScale = new Vector3(2f, 1f, (WALL_WIDTH));
                    new_wall.transform.localScale = new Vector3(MAZE_WIDTH + 2 * WALL_WIDTH, 1, WALL_WIDTH); 
                    //Debug.Log(new_wall.transform.localScale);
                    new_wall.transform.Rotate(new Vector3(0, 90f, 0));
                }

                // Add wall above
                if (cell.walls[1] && i == 0)
                {
                    GameObject new_wall = Instantiate(wall, new Vector3(pos[0], 0, pos[1] + (MAZE_WIDTH + WALL_WIDTH) / 2f), Quaternion.identity, parent.transform);
                    new_wall.transform.localScale = new Vector3(MAZE_WIDTH + 2 * WALL_WIDTH, 1, WALL_WIDTH); 
                    //Debug.Log(new_wall.transform.localScale);
                    //new_wall.transform.localScale = new Vector3(2f, 1f, (WALL_WIDTH));
                }

                // Add wall to right side
                if (cell.walls[2])
                {
                    GameObject new_wall = Instantiate(wall, new Vector3(pos[0] + (MAZE_WIDTH + WALL_WIDTH) / 2f, 0, pos[1]), Quaternion.identity, parent.transform);
                    new_wall.transform.localScale = new Vector3(MAZE_WIDTH + 2 * WALL_WIDTH, 1, WALL_WIDTH); 
                    //Debug.Log(new_wall.transform.localScale);
                    new_wall.transform.Rotate(new Vector3(0, 90f, 0));
                }

                // Add path to the right
                else if(j != MAZE_LENGTH - 1)
                {
                    Instantiate(ground, new Vector3(pos[0] + width / 2f, 0, pos[1]), Quaternion.identity, parent.transform);
                }

                // Add wall below
                if (cell.walls[3])
                {
                    GameObject new_wall = Instantiate(wall, new Vector3(pos[0], 0, pos[1] - (MAZE_WIDTH + WALL_WIDTH) / 2f), Quaternion.identity, parent.transform);
                    new_wall.transform.localScale = new Vector3(MAZE_WIDTH +  2 * WALL_WIDTH, 1, WALL_WIDTH); 
                    //Debug.Log(new_wall.transform.localScale);
                }

                // Add path below
                else if (i != MAZE_HEIGHT - 1)
                {
                    Instantiate(ground, new Vector3(pos[0], 0, pos[1] - width / 2f), Quaternion.identity, parent.transform);
                }

                x += width;
            }

            z -= width;
        }
        
        // Place player in maze
        //player.GetComponent<PlayerController>().SpawnAtStart(start, side, MAZE_SIZE, MAZE_WIDTH);
        //enemy.GetComponent<EnemyLogic>().SpawnAtRandomPos();
    }

    private void spawnCharacters()
    {
        System.Random rand = new System.Random();
        float i = start / MAZE_LENGTH,
            j = start % MAZE_LENGTH,
            distance;

        // Spawn enemy a sufficient distance away
        do{
            float r_i = rand.Next(MAZE_HEIGHT),
                r_j = rand.Next(MAZE_LENGTH);
            distance = Vector3.Distance(new Vector3(i, 0, j), new Vector3(r_i, 0, r_j));
            
            // Spawn enemy a sufficient distance away

            if(distance <= 5){
                player = spawnEntity(player, (int)i, (int)j);
                enemy = spawnEntity(enemy, (int)(r_i), (int)(r_j));
                enemy.gameObject.GetComponent<EnemyLogic>().player = player;
            }

        }while(distance > 5);        
    }

    public float[] indicesToCoors(int a, int b)
    {
        int z = a,
        x = b;

        float x_ref,
            z_ref;

        // Position player
        if (MAZE_LENGTH % 2 == 0)
        {
            x_ref = (MAZE_WIDTH + WALL_WIDTH) / 2 + (MAZE_WIDTH + WALL_WIDTH) * (MAZE_LENGTH / 2 - 1);
        }

        else
        {
            x_ref = (MAZE_WIDTH + WALL_WIDTH) * (MAZE_LENGTH / 2);
        }

        if (MAZE_HEIGHT % 2 == 0)
        {
            z_ref = (MAZE_WIDTH + WALL_WIDTH) / 2 + (MAZE_WIDTH + WALL_WIDTH) * (MAZE_HEIGHT / 2 - 1);
        }

        else
        {
            z_ref = (MAZE_WIDTH + WALL_WIDTH) * (MAZE_HEIGHT / 2);
        }

        return new float[]{(MAZE_WIDTH + WALL_WIDTH) * x - x_ref, -1 * (MAZE_WIDTH + WALL_WIDTH) * z + z_ref};
    }

    private GameObject spawnEntity(GameObject entity, int a, int b)
    {

        //Debug.Log("pos = " + start + " s_a = " + a + " s_b " + b);
        // Get start position

        float[] coors = indicesToCoors(a, b);
        Vector3 pos = new Vector3(coors[0], 0.51f, coors[1]);

        float rotation = 0f;

        if (entity.CompareTag("player"))
        {
            if (b == 0 && a != 0)
            {
                rotation = 90f;
            }

            else if (a == 0 && b != MAZE_LENGTH - 1)
            {
                rotation = 180.0f;
            }

            else if (b == MAZE_LENGTH - 1 && a != MAZE_HEIGHT - 1)
            {
                rotation = -90f;
            }

            else
            {
                rotation = 0f;
            }
        }

        Vector3 rot = new Vector3(0.0f, rotation, 0.0f);
        return Instantiate(entity, pos, Quaternion.Euler(0, rotation, 0));
    }

    public float[] getDirection(Vector3 start, Vector3 target, int max_distance)
    {
        // Convert position of start and target in game world to indices in maze;
        int[] start_location = coorsToIndices(start),
            target_location = coorsToIndices(target);

        Stack<int> path = new Stack<int>();             // 0 = left, 1 = up, 2 = right, 3 = down
        Stack<int[]> cellsToVisit = new Stack<int[]>(); // [0] = position of next cell to visit, 
                                                        // [1] = direction taken to get their
                                                        // [2] = distance from start (used for backtracking)
        //Debug.Log("Enemy Coordinates: (" + start_location[0] + ", " + start_location[1] + ")");
        
        // Check if start and target are same location
        if (start_location[0] == target_location[0] && start_location[1] == target_location[1])
        {
            // If yes, move to their relative location
            float[] cell_coors = indicesToCoors(start_location[0], start_location[1]);
            return getTargetPosition(start, start_location[0], start_location[1], sideRelativeToCenter(cell_coors[0], cell_coors[1], target.x, target.z));
        }

        // Otherwise, get first set of cells to check
        Cell curr = maze[start_location[0]][start_location[1]];

        // Check left
        if (!curr.walls[0] && start_location[1] != 0)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] - 1, 0, 1 });
        }

        // Check top
        if (!curr.walls[1] && start_location[0] != 0)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] - MAZE_LENGTH, 1, 1 });
        }

        // Check right
        if (!curr.walls[2] && start_location[1] != MAZE_LENGTH - 1)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] + 1, 2, 1 });
        }

        // check bottom
        if (!curr.walls[3] && start_location[0] != MAZE_HEIGHT - 1)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] + MAZE_LENGTH, 3, 1 });
        }

        int dist = 1;

        while (cellsToVisit.Count != 0)
        {
            int[] info = cellsToVisit.Pop();
            // backtrack along path if dist parameter dictates
            while (dist > info[2])
            {
                path.Pop();
                dist--;
            }

            // Update current path
            path.Push(info[1]);

            // Get coordinates from position value
            int a = info[0] / MAZE_LENGTH,
                b = info[0] % MAZE_LENGTH,
                came_from = (info[1] + 2) % 4;

            dist += 1;

            if (a == target_location[0] && b == target_location[1])
            {
                string output = "";
                int[] finalPath = path.ToArray();
                for (int i = path.Count - 1; i > -1; i--)
                {
                    output += (" " + finalPath[i]);
                }

                //Debug.Log("PATH (" + max_distance + "):" + output);

                return getTargetPosition(start, start_location[0], start_location[1], finalPath[path.Count - 1]);//finalPath[path.Count - 1];
            }
            
            else if(dist <= max_distance)
            {
                curr = maze[a][b];

                if (!curr.walls[0] && b != 0 && came_from != 0)
                {
                    cellsToVisit.Push(new int[] { info[0] - 1, 0 , dist});
                }

                // Check top
                if (!curr.walls[1] && a != 0 && came_from != 1)
                {
                    cellsToVisit.Push(new int[] { info[0] - MAZE_LENGTH, 1, dist });
                }

                // Check right
                if (!curr.walls[2] && b != MAZE_LENGTH - 1 && came_from != 2)
                {
                    cellsToVisit.Push(new int[] { info[0] + 1, 2, dist });
                }

                // check bottom
                if (!curr.walls[3] && a != MAZE_HEIGHT - 1 && came_from != 3)
                {
                    cellsToVisit.Push(new int[] { info[0] + MAZE_LENGTH, 3, dist });
                }
            }
        }

        //Debug.Log("NO PATH (" + max_distance + ")");

        return null;
    }

    public float[] getDirection(Vector3 start, Vector3 target)
    {
        // Convert position of start and target in game world to indices in maze;
        int[] start_location = coorsToIndices(start),
            target_location = coorsToIndices(target);

        Stack<int> path = new Stack<int>();             // 0 = left, 1 = up, 2 = right, 3 = down
        Stack<int[]> cellsToVisit = new Stack<int[]>(); // [0] = position of next cell to visit, 
        // [1] = direction taken to get their
        // [2] = distance from start (used for backtracking)

        // Check if start and target are same location
        if (start_location[0] == target_location[0] && start_location[1] == target_location[1])
        {
            // If yes, move to their relative location
            float[] cell_coors = indicesToCoors(start_location[0], start_location[1]);
            return getTargetPosition(start, start_location[0], start_location[1], sideRelativeToCenter(cell_coors[0], cell_coors[1], target.x, target.z));
        }

        // Otherwise, get first set of cells to check
        Cell curr = maze[start_location[0]][start_location[1]];

        // Check left
        if (!curr.walls[0] && start_location[1] != 0)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] - 1, 0, 1 });
        }

        // Check top
        if (!curr.walls[1] && start_location[0] != 0)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] - MAZE_LENGTH, 1, 1 });
        }

        // Check right
        if (!curr.walls[2] && start_location[1] != MAZE_LENGTH - 1)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] + 1, 2, 1 });
        }

        // check bottom
        if (!curr.walls[3] && start_location[0] != MAZE_HEIGHT - 1)
        {
            cellsToVisit.Push(new int[] { start_location[0] * MAZE_LENGTH + start_location[1] + MAZE_LENGTH, 3, 1 });
        }

        int dist = 1;

        while (cellsToVisit.Count != 0)
        {
            int[] info = cellsToVisit.Pop();
            // backtrack along path if dist parameter dictates
            while (dist > info[2])
            {
                path.Pop();
                dist--;
            }

            // Update current path
            path.Push(info[1]);

            // Get coordinates from position value
            int a = info[0] / MAZE_LENGTH,
                b = info[0] % MAZE_LENGTH,
                came_from = (info[1] + 2) % 4;

            dist += 1;

            if (a == target_location[0] && b == target_location[1])
            {
                string output = "";
                int[] finalPath = path.ToArray();
                for (int i = path.Count - 1; i > -1; i--)
                {
                    output += (" " + finalPath[i]);
                }

                //Debug.Log("PATH (No limit): " + output);

                return getTargetPosition(start, start_location[0], start_location[1], finalPath[path.Count - 1]);//finalPath[path.Count - 1];
            }

            else
            {
                curr = maze[a][b];

                if (!curr.walls[0] && b != 0 && came_from != 0)
                {
                    cellsToVisit.Push(new int[] { info[0] - 1, 0, dist });
                }

                // Check top
                if (!curr.walls[1] && a != 0 && came_from != 1)
                {
                    cellsToVisit.Push(new int[] { info[0] - MAZE_LENGTH, 1, dist });
                }

                // Check right
                if (!curr.walls[2] && b != MAZE_LENGTH - 1 && came_from != 2)
                {
                    cellsToVisit.Push(new int[] { info[0] + 1, 2, dist });
                }

                // check bottom
                if (!curr.walls[3] && a != MAZE_HEIGHT - 1 && came_from != 3)
                {
                    cellsToVisit.Push(new int[] { info[0] + MAZE_LENGTH, 3, dist });
                }
            }
        }

        //Debug.Log("NO PATH (No limit)");

        return null;
    }

    public int[] coorsToIndices(Vector3 coors)
    {
        float x = coors.x,
            z = coors.z,
            tx = 0,
            tz = 0;
        if (MAZE_LENGTH % 2 == 0)
        {
            x += (MAZE_WIDTH + WALL_WIDTH) * MAZE_LENGTH / 2;   
            tx += (MAZE_WIDTH + WALL_WIDTH) * MAZE_LENGTH / 2;
        }

        else
        {
            x += (MAZE_WIDTH + WALL_WIDTH) * (MAZE_LENGTH / 2) + (MAZE_WIDTH + WALL_WIDTH) / 2f;
            tx += (MAZE_WIDTH + WALL_WIDTH) * (MAZE_LENGTH / 2) + (MAZE_WIDTH + WALL_WIDTH) / 2f;
        }

        if (MAZE_HEIGHT % 2 == 0)
        {
            z += (MAZE_WIDTH + WALL_WIDTH) * MAZE_HEIGHT / 2;
            tz += (MAZE_WIDTH + WALL_WIDTH) * MAZE_HEIGHT / 2;
        }

        else
        {
            z += (MAZE_WIDTH + WALL_WIDTH) * (MAZE_HEIGHT / 2) + (MAZE_WIDTH + WALL_WIDTH) / 2f;
            tz += (MAZE_WIDTH + WALL_WIDTH) * (MAZE_HEIGHT / 2) + (MAZE_WIDTH + WALL_WIDTH) / 2f;
        }

        //Debug.Log("Offset: (" + tx + ", " + tz + ")");

        int b = (int)(x / (MAZE_WIDTH + WALL_WIDTH)),
            a = MAZE_HEIGHT - (int)(z / (MAZE_WIDTH + WALL_WIDTH))-1;

        //Debug.Log("coorsToIndices Results: " + a + " " + b);

        return (new int[] { a, b });
    }

    private float[] getTargetPosition(Vector3 pos, int a, int b, int dir)
    {
        float[] cell_coors = indicesToCoors(a,b);
        float cell_x = cell_coors[0],
            cell_z = cell_coors[1],
            cell_offset = MAZE_WIDTH / 2.0f,
            wall_offset = WALL_WIDTH / 2.0f,
            target_x = 0,
            target_z = 0;

        int atSide = sideRelativeToCenter(cell_x, cell_z, pos.x, pos.z);

        string output = "" + dir + " ";

        // Find correct space to move to if you are in the cell
        if (atSide == -1)
        {
            target_x = cell_x;
            target_z = cell_z;
        }

        else if (atSide == 4)
        {
            output += "Moving to exit";
            switch (dir)
            {
                // Trying to move left
                case 0:
                    target_x = cell_x - cell_offset;
                    target_z = cell_z;
                    break;

                // Trying to move up
                case 1:
                    target_x = cell_x;
                    target_z = cell_z + cell_offset;
                    break;

                // Trying to move right
                case 2:
                    target_x = cell_x + cell_offset;
                    target_z = cell_z;
                    break;

                // Trying to move down
                case 3:
                    target_x = cell_x;
                    target_z = cell_z - cell_offset;
                    break;
            }
        }

        else if(atSide != dir){
            output += "Entering cell (" + a + ", " + b + ")";
            switch (atSide)
            {
                // Trying to move right
                case 0:
                    output += " from left";
                    target_x = cell_x - cell_offset;
                    target_z = pos.z;
                    break;

                // Trying to move down
                case 1:
                    output += " from top";
                    target_x = pos.x;
                    target_z = cell_z + cell_offset;
                    break;

                // Trying to move left
                case 2:
                    output += " from right";
                    target_x = cell_x + cell_offset;
                    target_z = pos.z;
                    break;

                // Trying to move up
                case 3:
                    output += " from bottom";
                    target_x = pos.x;
                    target_z = cell_z - cell_offset;
                    break;
            }

            output += " (" + atSide + " !=  " + dir + ")";
        }

        else{
            output += "Leaving cell (" + a + ", " + b + ")";
            switch (dir)
            {
                // Trying to move left
                case 0:
                    output += " at left";
                    target_x = cell_x - (cell_offset + wall_offset);
                    target_z = pos.z;
                    break;

                // Trying to move up
                case 1:
                    output += " at top";
                    target_x = pos.x;
                    target_z = cell_z + (cell_offset + wall_offset);
                    break;

                // Trying to move right
                case 2:
                    output += " at right";
                    target_x = cell_x + (cell_offset + wall_offset);
                    target_z = pos.z;
                    break;

                // Trying to move down
                case 3:
                    output += " at bottom";
                    target_x = pos.x;
                    target_z = cell_z - (cell_offset + wall_offset);
                    break;
            }
        }
        //output += " (" + target_x + ", " + target_z + ")";

        //Debug.Log(output);
        return (new float[] { target_x, target_z });
    }

    // Find the side of cell c that target t is on using their physical positions
    // 0 = left, 1 = top, 2 = right, 3 = bottom, 4 = in cell c, -1 = error.
    private int sideRelativeToCenter(float c_x, float c_z, float t_x, float t_z){
        // First, check if you are in the cell
        if (Mathf.Abs(c_x - t_x) <= MAZE_WIDTH / 2.0f && Mathf.Abs(c_z - t_z) <= MAZE_WIDTH / 2.0f)
        {
            return 4;
        }

        // Check if to left
        else if (c_x - t_x > MAZE_WIDTH / 2.0f && Mathf.Abs(c_z - t_z) < MAZE_WIDTH / 2.0f)
        {
            return 0;
        }

        // Check if to top
        else if (Mathf.Abs(c_x - t_x) < MAZE_WIDTH / 2.0f && c_z - t_z < MAZE_WIDTH / 2.0f)
        {
            return 1;
        }

        // Check if to right
        else if (c_x - t_x <= MAZE_WIDTH / 2.0f && Mathf.Abs(c_z - t_z) < MAZE_WIDTH / 2.0f)
        {
            return 2;
        }

        // Check if to bottom
        else if (Mathf.Abs(c_x - t_x) < MAZE_WIDTH / 2.0f && c_z - t_z >= MAZE_WIDTH / 2.0f)
        {
            return 3;
        }

        else
        {
            Debug.Log("Could not find side relative to center of cell (" + c_x + "," + c_z + ")");
            return -1;
        }
    }

    private void checkIfPlayerWon()
    {
        int[] player_pos = coorsToIndices(player.transform.position);

        if (end / MAZE_LENGTH == player_pos[0] && end % MAZE_LENGTH == player_pos[1])
        {
            player.GetComponent<PlayerController>().playerWins();
            enemy.GetComponent<EnemyLogic>().stopEnemy(false);
        }
    }
}
