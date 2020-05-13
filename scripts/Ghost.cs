using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

    public float moveSpeed = 5.9f;
    public float normalMoveSpeed = 5.9f;
    public float frightenedModeMoveSpeed = 2.9f;
    public float consumedMoveSpeed = 15f;

    public bool canMove = true;

    public int pinkyReleaseTimer = 5;
    public int inkyReleaseTimer = 14;
    public int clydeReleaseTimer = 21;
    public float ghostReleaseTimer = 0;

    public int frightenedModeDuration = 10;
    public int startBlinkingAt = 7;

    public bool isInGhostHouse = false;
    //public bool isGhostHouseEntrance = true;
    public nodes startingPosition;
    public nodes homeNode;
    public nodes ghostHouse;

    public int scatterModeTimer1 = 7;
    public int chaseModeTimer1 = 20;
    public int scatterModeTimer2 = 7;
    public int chaseModeTimer2 = 20;
    public int scatterModeTimer3 = 5;
    public int chaseModeTimer3 = 20;
    public int scatterModeTimer4 = 5;

    public Sprite eyesUp;
    public Sprite eyesDown;
    public Sprite eyesLeft;
    public Sprite eyesRight;

    public RuntimeAnimatorController ghostUp;
    public RuntimeAnimatorController ghostDown;
    public RuntimeAnimatorController ghostLeft;
    public RuntimeAnimatorController ghostRight;
    public RuntimeAnimatorController ghostWhite;
    public RuntimeAnimatorController ghostBlue;

   

    private int modeChangeIteration = 1;
    private float modeChangeTimer = 0;

    private float frightenedModeTimer = 0;
    private float blinkTimer = 0;

    private bool frightenedModeIsWhite = false;

    private float previousMoveSpeed;

    private AudioSource backgroundAudio;

    public enum Mode
    {
        Chase,
        Scatter,
        Frightened,
        Consumed
    }

    Mode currentMode = Mode.Scatter;
    Mode previousMode;

    public enum GhostType
    {
        Red,
        Pink,
        Blue,
        Orange
    }

    public GhostType ghostType = GhostType.Red;

    private GameObject pacMan;

    private nodes currentNode, targetNode, previousNode;
    private Vector2 direction, nextDirection;
	// Use this for initialization
    void Start()
    {

        backgroundAudio = GameObject.Find("Game").transform.GetComponent<AudioSource>();
        pacMan = GameObject.FindGameObjectWithTag("PacMan");

        nodes node = GetNodeAtPosition(transform.localPosition);



        if (node != null)
        {
            currentNode = node;
        }

        if (isInGhostHouse)
        {
            direction = Vector2.up;
            targetNode = currentNode.neighbours[0];
        }
        /*if (isGhostHouseEntrance)
        {
            direction = Vector2.down;
            targetNode = currentNode.neighbours[2];
        }*/


        else
        {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }

        previousNode = currentNode;

        UpdateAnimatorController();
    }

    public void MoveToStartingPosition()
    {
        if (transform.name != "Ghost_Blinky")
            isInGhostHouse = true;

        transform.position = startingPosition.transform.position;

        if (isInGhostHouse)
        {
            direction = Vector2.up;
        }
        else
        {
            direction = Vector2.left;
        }

        UpdateAnimatorController();
    }
	 public void Restart()
    {
        canMove = true;

        currentMode = Mode.Scatter;

        moveSpeed = normalMoveSpeed;

        previousMoveSpeed = 0;

       

         ghostReleaseTimer = 0;
         modeChangeIteration = 1;
         modeChangeTimer = 0;

         currentNode = startingPosition;

         if (isInGhostHouse) {
             direction = Vector2.up;
             targetNode = currentNode.neighbours[0];
         }
         else {
             direction = Vector2.left;
             targetNode = ChooseNextNode ();
         }

         previousNode = currentNode;
         UpdateAnimatorController ();

     }


	
	// Update is called once per frame
	void Update () {

        if (canMove)
        {

            ModeUpdate();
            Move();
            ReleaseGhosts();
            CheckCollision();
            CheckIsInGhostHouse();
        }
	}

    void CheckIsInGhostHouse()
    {
        if (currentMode == Mode.Consumed)
        {
            GameObject tile = GetTileAtPosition(transform.position);

            if (tile != null)
            {
                if (tile.transform.GetComponent<Tile>() != null)
                {
                    if (tile.transform.GetComponent<Tile>().isGhostHouse)
                    {
                        moveSpeed = normalMoveSpeed;

                        nodes node = GetNodeAtPosition(transform.position);

                        if (node != null)
                        {
                            currentNode = node;

                            direction = Vector2.up;
                            targetNode = currentNode.neighbours[0];

                            previousNode = currentNode;

                            currentMode = Mode.Chase;

                            UpdateAnimatorController();

                        }
                    }
                }
            }
        }
    }

    void CheckCollision()
    {
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);

        if (ghostRect.Overlaps(pacManRect))
        {
            if (currentMode == Mode.Frightened)
            {
                Consumed();
            }
            else
            {
                if (currentMode != Mode.Consumed)
                {
                    //-pacman should die
                    GameObject.Find("Game").transform.GetComponent<gameboard>().StartDeath();
                }
            }
            
        }
    }

    void Consumed()
    {
        if (GameMenu.isOnePlayerGame)
        {
            gameboard.playerOneScore += 200;
        }
        else
        {
            if (GameObject.Find("Game").GetComponent<gameboard>().isPlayerOneUp)
            {
                gameboard.playerOneScore += 200;
            }
            else
            {
                gameboard.playerTwoScore += 200;
            }
        }

        currentMode = Mode.Consumed;
        previousMoveSpeed = moveSpeed;
        moveSpeed = consumedMoveSpeed;

        UpdateAnimatorController ();

        GameObject.Find("Game").transform.GetComponent<gameboard>().StartConsumed(this.GetComponent<Ghost>());
    }
    void UpdateAnimatorController()
    {
        if (currentMode != Mode.Frightened && currentMode != Mode.Consumed)
        {
            if (direction == Vector2.up)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostUp;
            }

            else if (direction == Vector2.down)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostDown;
            }
            else if (direction == Vector2.left)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
            else if (direction == Vector2.right)
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostRight;
            }
            else
            {
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            }
        }
        else if ( currentMode == Mode.Frightened)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = ghostBlue;
        }
        else if (currentMode == Mode.Consumed)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = null;

            if (direction == Vector2.up)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesUp;
            }
            else if (direction == Vector2.down)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesDown;
            }
            else if (direction == Vector2.left)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesLeft;
            }
            else if (direction == Vector2.right)
            {
                transform.GetComponent<SpriteRenderer>().sprite = eyesRight;
            }

        }
      }
    void Move()
    {
        if (targetNode != currentNode && targetNode != null && !isInGhostHouse)
        {
            if (OverShotTarget())

            {
                
                currentNode = targetNode;

                transform.localPosition = currentNode.transform.position;

                targetNode = ChooseNextNode();
                previousNode = currentNode;
                currentNode = null;

                UpdateAnimatorController();

            }
            else
            {
                
               
                transform.localPosition += (Vector3)direction * moveSpeed * Time.deltaTime;
            }
        }
    }

    void ModeUpdate()
    {
        if (currentMode != Mode.Frightened)
        {
            modeChangeTimer += Time.deltaTime;

            if (modeChangeIteration == 1)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer1)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            
            if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer1)
            {
                modeChangeIteration = 2;
                ChangeMode(Mode.Scatter);
                modeChangeTimer = 0;
            }
        }
        else if (modeChangeIteration == 2)
        {
            if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer2)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            }
            if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer2)
            {
                modeChangeIteration = 3;
                ChangeMode(Mode.Scatter);
                modeChangeTimer = 0;
            }

        }
        else if (modeChangeIteration == 3)
        {
            if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer3)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            
            if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer3)
            {
                modeChangeIteration = 4;
                ChangeMode(Mode.Scatter);
                modeChangeTimer = 0;
            }
        }
        else if (modeChangeIteration == 4)
        {
            
            if(currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer4) 
            {
                ChangeMode (Mode.Chase);
                modeChangeTimer = 0;
            }

        }
      else if (currentMode == Mode.Frightened) {

          frightenedModeTimer += Time.deltaTime;

          if (frightenedModeTimer >= frightenedModeDuration)
          {
              backgroundAudio.clip = GameObject.Find("Game").transform.GetComponent<gameboard>().backgroundAudioNormal;
              backgroundAudio.Play();

              frightenedModeTimer = 0;
              ChangeMode(previousMode);
          }

          if (frightenedModeTimer >= startBlinkingAt)
          {
              blinkTimer += Time.deltaTime;
              if (blinkTimer >= 0.1f)
              {
                  blinkTimer = 0f;

                  if (frightenedModeIsWhite)
                  {
                      transform.GetComponent<Animator>().runtimeAnimatorController = ghostBlue;
                      frightenedModeIsWhite = false;
                  }

                  else
                  {
                      transform.GetComponent<Animator>().runtimeAnimatorController = ghostWhite;
                      frightenedModeIsWhite = true;
                  }
              }
          }

   }
}

    void ChangeMode(Mode m)
    {
        

        if (currentMode == Mode.Frightened)
        {
            moveSpeed = previousMoveSpeed;
        }

        if (m == Mode.Frightened)
        {
            previousMoveSpeed = moveSpeed;
            moveSpeed = frightenedModeMoveSpeed;

        }

        if (currentMode != m)
        {

            previousMode = currentMode;
            currentMode = m;
        }

        UpdateAnimatorController ();
    }

    public void StartFrightenedMode()
    {
        frightenedModeTimer = 0;
        backgroundAudio.clip = GameObject.Find("Game").transform.GetComponent<gameboard>().backgroundAudioFrightened;
        backgroundAudio.Play () ;
        ChangeMode(Mode.Frightened);
    }
    Vector2 GetRedGhostTargetTile()
    {
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));

        return targetTile;
    }
    Vector2 GetPinkGhostTargetTile()
    {
        //-four tiles ahead pac-man
        //-taking account position and orientation
        Vector2 pacManPosition = pacMan.transform.localPosition;
        Vector2 pacManOrientation = pacMan.GetComponent<pacman_1>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);
        Vector2 targetTile = pacManTile + (4 * pacManOrientation);

        return targetTile;

    }

    Vector2 GetBlueGhostTargetTile()
    {
        //-select the position two tiles in front of Pacman
        //-draw vector from blink to that position
        //-double the length of the vector
         Vector2 pacManPosition = pacMan.transform.localPosition;
         Vector2 pacManOrientation = pacMan.GetComponent<pacman_1>().orientation;

         int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
         int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

         Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);
         Vector2 targetTile = pacManTile + (2 * pacManOrientation);

         //-temporary blinky position
         Vector2 tempBlinkyPosition = GameObject.Find("Ghost_Blinky").transform.localPosition;

         int blinkyPositionX = Mathf.RoundToInt(tempBlinkyPosition.x);
         int blinkyPositionY = Mathf.RoundToInt(tempBlinkyPosition.y);

         tempBlinkyPosition = new Vector2(blinkyPositionX, blinkyPositionY);

         float distance = GetDistance(tempBlinkyPosition, targetTile);
         distance *= 2;
         targetTile = new Vector2(tempBlinkyPosition.x + distance, tempBlinkyPosition.y + distance);
         return targetTile;
       
    }

    Vector2 GetOrangeGhostTargetTile()
    {
        //-calculate the distance from pacman
        //-if the distance is greater than eight tiles targeting is the same as Blinky
        //-if the distance is less than eight tiles, then target is his home node, so same as scatter mode

         Vector2 pacManPosition = pacMan.transform.localPosition;

          float distance = GetDistance(transform.localPosition, pacManPosition);
          Vector2 targetTile = Vector2.zero;

          if (distance <= 8)
          {
              targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));
          }
          else if (distance >= 8)
          {
              targetTile = homeNode.transform.position;
          }

          return targetTile;
       
    }

    Vector2 GetTargetTile()
    {
        Vector2 targetTile = Vector2.zero;

        if (ghostType == GhostType.Red)
        {
            targetTile = GetRedGhostTargetTile() ;
        }
        else if (ghostType == GhostType.Pink)
        {
            targetTile = GetPinkGhostTargetTile();
        }
        else if (ghostType == GhostType.Blue)
        {
            targetTile = GetBlueGhostTargetTile();
        }
        else if (ghostType == GhostType.Orange)
        {
            targetTile = GetOrangeGhostTargetTile();
        }

        return targetTile;
    }

    Vector2 GetRandomTile()
    {
        int x = Random.Range(0, 32);
        int y = Random.Range(0, 32);

        return new Vector2(x, y);
    }
    void ReleasePinkGhost()
    {
        if (ghostType == GhostType.Pink && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseBlueGhost()
    {
        if (ghostType == GhostType.Blue && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }
    void ReleaseOrangeGhost()
    {
        if (ghostType == GhostType.Orange && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }


    void ReleaseGhosts()
    {
        ghostReleaseTimer += Time.deltaTime;

        if (ghostReleaseTimer > pinkyReleaseTimer)
            ReleasePinkGhost();
        if (ghostReleaseTimer > inkyReleaseTimer)
            ReleaseBlueGhost();
        if (ghostReleaseTimer > clydeReleaseTimer)
            ReleaseOrangeGhost();
    }


    nodes ChooseNextNode()
    {
        Vector2 targetTile = Vector2.zero;

        if (currentMode == Mode.Chase)
        {
            targetTile = GetTargetTile();
        }
        else if (currentMode == Mode.Scatter)
        {
            targetTile = homeNode.transform.position;
        }
        else if (currentMode == Mode.Frightened)
        {
            targetTile = GetRandomTile ();
        }
        else if (currentMode == Mode.Consumed)
        {
            targetTile = ghostHouse.transform.position;
        }
        

        nodes moveToNode = null;

        nodes[] foundNodes = new nodes[4];
        Vector2[] foundNodesDirection = new Vector2[4];

        int nodeCounter = 0;

        for (int i = 0; i < currentNode.neighbours.Length; i++)
        {
            if (currentNode.validDirections[i] != direction * -1)
            {
                if (currentMode != Mode.Consumed)

                {
                    //if (targetNode == currentNode)
                    //{
                    //if (currentNode != null)
                    // {
                   // previousNode = GetNodeAtPosition(transform.position);

                   // if (currentNode != null)
                  //  {
                       // currentNode = previousNode;
                        Debug.Log("here i reach-3");
                        
                        GameObject tile = GetTileAtPosition(ghostHouse.transform.position);
                        Debug.Log("here i reach-2");

                            //targetTile = homeNode.transform.position;
                            //targetTile = GetTargetTile();
                           
                            if (tile.transform.GetComponent<Tile>().isGhostHouseEntrance == true)
                            {
                                Debug.Log("here i reach-1");
                                //if (currentMode == Mode.Scatter) {
                                //-found a ghost house, don't want to allow movement
                                // targetTile = homeNode.transform.position;
                                //  targetTile = GetTargetTile();
                                //   targetTile = ghostHouse.transform.position;
                                if (currentNode.validDirections[i] != Vector2.down)

                                    Debug.Log("here i reach0");

                                {
                                    if (currentMode == Mode.Scatter)

                                    {
                                    Debug.Log("here i reach1");

                                     targetTile = homeNode.transform.position;
                                    foundNodes[nodeCounter] = currentNode.neighbours[i];
                                    foundNodesDirection[nodeCounter] = currentNode.validDirections[i];

                                    nodeCounter++;
                                     }


                                    //}


                                }

                           // }
                        //}
                    }
                    else
                    {

                        Debug.Log("here i reach2");
                        foundNodes[nodeCounter] = currentNode.neighbours[i];
                        foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    }
                        
                    

                
                }
                else
                {
                   // if (currentMode == Mode.Chase)
          
                        Debug.Log("here i reach3");
                        foundNodes[nodeCounter] = currentNode.neighbours[i];
                        foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    
                }
                
            }
        }
        if (foundNodes.Length == 1)
        {
            moveToNode = foundNodes[0];
            direction = foundNodesDirection[0];
        }

        if (foundNodes.Length > 1)
        {
            float leastDistance = 100000f;

            for (int i = 0; i < foundNodes.Length; i++)
            {
                if (foundNodesDirection[i] != Vector2.zero)
                {
                    float distance = GetDistance(foundNodes[i].transform.position, targetTile);

                    if (distance < leastDistance)
                    {
                        leastDistance = distance;
                        moveToNode = foundNodes[i];
                        direction = foundNodesDirection[i];
                    }
                }
            }
        }

        return moveToNode;
    }

    nodes GetNodeAtPosition (Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<gameboard> ().board[(int)pos.x, (int)pos.y];

       

        if (tile != null)
        {
            if (tile.GetComponent<nodes>() != null)
            {
                return tile.GetComponent<nodes>();
            }
            
        }

        return null;

    }

    GameObject GetTileAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject tile = GameObject.Find("Game").transform.GetComponent<gameboard>().board[tileX, tileY];

        if (tile != null)
            return tile;

        return null;
    }
    float LengthFromNode(Vector2 targetPosition)
    {
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }

    bool OverShotTarget()
    {
        float nodeToTarget = LengthFromNode(targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);

        return nodeToSelf > nodeToTarget;
    }

    float GetDistance(Vector2 posA, Vector2 posB)
    {
        float dx = posA.x - posB.x;
        float dy = posA.y - posB.y;

        float distance = Mathf.Sqrt (dx * dx + dy * dy);

        return distance;
    }
    
}
