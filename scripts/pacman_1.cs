using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pacman_1 : MonoBehaviour {

    public AudioClip chomp1;

    public AudioClip chomp2;

    public RuntimeAnimatorController chompAnimation;
    public RuntimeAnimatorController deathAnimation;


    public Vector2 orientation;

    public float speed = 4.0f;

    public Sprite idleSprite;

    public bool canMove = true;

    private bool playedChomp1 = false;

    private AudioSource audio;

    private Vector2 direction = Vector2.zero;

    private Vector2 nextDirection;

    private nodes currentNode, previousNode, targetNode;

    private nodes startingPosition;

	// Use this for initialization
	void Start () {

        audio = transform.GetComponent<AudioSource>();

        nodes node = GetNodeAtPosition(transform.localPosition);

        startingPosition = node;

        if (node != null)
        {
            currentNode = node;
            // Debug.Log("currentNode: " + currentNode);
            Debug.Log(currentNode);
        }

        direction = Vector2.left;

        orientation = Vector2.left;
        ChangePosition(direction);
	}

    public void MoveToStartingPosition()
    {
        transform.position = startingPosition.transform.position;

        transform.GetComponent<SpriteRenderer>().sprite = idleSprite;

        direction = Vector2.left;
        orientation = Vector2.left;

        UpdateOrientation();
    }

    public void Restart()
    {
        canMove = true;

        currentNode = startingPosition;
        
        nextDirection = Vector2.left;

        transform.GetComponent<Animator>().runtimeAnimatorController = chompAnimation;
        transform.GetComponent<Animator>().enabled = true;

        ChangePosition(direction);
    }
	// Update is called once per frame
	void Update () {

        if (canMove)
        {

            CheckInput();

            Move();

            UpdateOrientation();

            UpdateAnimationState();

            ConsumePellet();
        }
	}

    void PlayChompSound()
    {
        if (playedChomp1)
        {
            //-play chomp 2, set playedchomp1 to false;
            audio.PlayOneShot(chomp2);
            playedChomp1 = false;
        }
        else
        {
            //-play chomp 1, set playedchomp1 to true;
            audio.PlayOneShot(chomp1);
            playedChomp1 = true;
        }
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {

            ChangePosition(Vector2.left);

        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            ChangePosition(Vector2.right);

        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            ChangePosition(Vector2.up);

        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {

            ChangePosition(Vector2.down);
        }
    }

    void ChangePosition(Vector2 d)
    {
        if (d != direction)

            nextDirection = d;

        if (currentNode != null)
        {
            nodes MoveToNode = CanMove(d);

            if (MoveToNode != null)
            {
                direction = d;
                targetNode = MoveToNode;
                previousNode = currentNode;
                currentNode = null;
            }
        }


    }
    void Move()
    {
        if (targetNode != currentNode && targetNode != null)
        {
            if (nextDirection == direction * -1)
            {
                direction *= -1;
                nodes tempNode = targetNode;
                targetNode = previousNode;
                previousNode = tempNode;
            }
            if (OverShortTarget())
            {
                currentNode = targetNode;
                transform.localPosition = currentNode.transform.position;

                nodes MoveToNode = CanMove(nextDirection);

                if (MoveToNode != null)
                    direction = nextDirection;
                if (MoveToNode == null)
                    MoveToNode = CanMove(direction);
                if (MoveToNode != null)
                {
                    targetNode = MoveToNode;
                    previousNode = currentNode;
                    currentNode = null;
                }
                else
                {
                    direction = Vector2.zero;
                }
            }
            else
            {
                transform.localPosition += (Vector3)((direction) * (speed)) * Time.deltaTime;
            }
        }
    }

    void MoveToNode(Vector2 d)
    {
        nodes moveToNode = CanMove(d);

        if (moveToNode != null)
        {
            transform.localPosition = moveToNode.transform.position;
            currentNode = moveToNode;
        }
    }

    void UpdateOrientation()
    {
        if (direction == Vector2.left)

        {
            orientation = Vector2.left;
            transform.localScale = new Vector3(-1, 1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, 0);

        }
        else if (direction == Vector2.right)
        {
            orientation = Vector2.right;
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == Vector2.up)
        {
            orientation = Vector2.up;
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (direction == Vector2.down)
        {
            orientation = Vector2.down;
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, 270);
        }
    }

    void UpdateAnimationState()
    {
        if (direction == Vector2.zero)
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = idleSprite;

        }
        else
        {
            GetComponent<Animator>().enabled = true;
        }
    }

    void ConsumePellet()
    {
        GameObject o = GetTileAtPosition(transform.position);
        if (o != null)
        {
            Tile tile = o.GetComponent<Tile>();
            if (tile != null)
            {
                if (!tile.didConsume && (tile.isPellet || tile.isSuperPellet))
                {
                    o.GetComponent<SpriteRenderer>().enabled = false;
                    tile.didConsume = true;

                    if (GameMenu.isOnePlayerGame)
                    {
                        gameboard.playerOneScore += 10;
                        GameObject.Find("Game").transform.GetComponent<gameboard>().playerOnePelletsConsumed++;
                    }
                    else
                    {
                        if (GameObject.Find("Game").transform.GetComponent<gameboard>().isPlayerOneUp)
                        {
                            gameboard.playerOneScore += 10;
                            GameObject.Find("Game").transform.GetComponent<gameboard>().playerOnePelletsConsumed++;
                        }
                        else
                        {
                            gameboard.playerOneScore += 10;
                            GameObject.Find("Game").transform.GetComponent<gameboard>().playerTwoPelletsConsumed++;
                        }
                    }
                    
                    PlayChompSound();

                    if (tile.isSuperPellet)
                    {
                        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

                        foreach (GameObject go in ghosts)
                        {
                            go.GetComponent<Ghost>().StartFrightenedMode();
                        }
                    }
                }
            }
        }
    }
    nodes CanMove(Vector2 d)
    {
        nodes moveToNode = null;
        for (int i = 0; i < currentNode.neighbours.Length; i++)
        {
            if (currentNode.validDirections[i] == d)
            {
                moveToNode = currentNode.neighbours[i];
                break;
            }
        }

        return moveToNode;
    }

    GameObject GetTileAtPosition(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt (pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject tile = GameObject.Find("Game").GetComponent<gameboard> ().board[tileX, tileY];

        if (tile != null)
        { 
            return tile;
        }

        return null;
    }

    nodes GetNodeAtPosition(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<gameboard> ().board[(int)pos.x, (int)pos.y];

        if (tile != null)
        {

            return tile.GetComponent<nodes>();
        }

        return null;

    }

    bool OverShortTarget()
    {
        float nodeToTarget = LengthFromNode (targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);

        return nodeToSelf > nodeToTarget;
    }

    float LengthFromNode(Vector2 targetPosition)
    {
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }
}
