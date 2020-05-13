﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class gameboard : MonoBehaviour {

    private static int boardWidth = 32;
    private static int boardHeight = 32;

    private bool didStartDeath = false;
    private bool didStartConsumed = false;

    private static int playerOneLevel = 1;
    private static int playerTwoLevel = 1;

    public int playerOnePelletsConsumed = 0;
    public int playerTwoPelletsConsumed = 0;

    public int totalPellets = 0;
    public int score = 0;
    public static int playerOneScore = 0;
    public static int playerTwoScore = 0;
    public int pacManLives = 3;

    public bool isPlayerOneUp = true;
    public bool shouldBlink = false;

    public float blinkIntervalTime = 0.1f;
    private float blinkIntervalTimer = 0;

    public AudioClip backgroundAudioNormal;
    public AudioClip backgroundAudioFrightened;
    public AudioClip backgroundAudioPacManDeath;
    public AudioClip consumedGhostAudioClip;

    public Sprite mazeBlue;
    public Sprite mazeWhite;

    public Text playerText;
    public Text ReadyText;

    public Text highScoreText;
    public Text playerOneUp;
    public Text playerTwoUp;
    public Text playerOneScoreText;
    public Text playerTwoScoreText;
    public Image playerLives2;
    public Image playerLives3;

    public Text consumedGhostScoreText;

    public GameObject[,] board = new GameObject[boardWidth, boardHeight];

	// Use this for initialization
	void Start () {

        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));

        foreach (GameObject o in objects)
        {
            Vector2 pos = o.transform.position;

            if (o.name != "pacman_1"&& o.name != "nodes" && o.name != "non nodes" && o.name != "maze" && o.name != "pellets" && o.tag != "Ghost"  && o.tag != "ghostHome" && o.name != "Canvas" && o.tag != "UIElements")
            {
                if (o.GetComponent<Tile>() != null)
                {
                    if (o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isSuperPellet)
                    {
                        totalPellets++;
                    }
                }
                board[(int)pos.x, (int)pos.y] = o;

            }
            else
            {
                Debug.Log("found pacman at: " + pos);
            }

        }

        StartGame();
	}

    void Update()
    {
        UpdateUI();
        CheckPelletsConsumed();
        CheckShouldBlink();
    }

    void UpdateUI()
    {
        playerOneScoreText.text = playerOneScore.ToString();
        playerTwoScoreText.text = playerTwoScore.ToString();

        if (pacManLives == 3)
        {
            playerLives3.enabled = true;
            playerLives2.enabled = true;
        }
        else if (pacManLives == 2)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = true;
        }
        else if (pacManLives == 1)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = false;

        }
    }

    void CheckPelletsConsumed()
    {
        if (isPlayerOneUp)
        {
            //-player one is playing
            if (totalPellets == playerOnePelletsConsumed) 
            {
                PlayerWin(1);
            }
        }
        else
        {
            //-player two is playing
            if (totalPellets == playerTwoPelletsConsumed)
            {
                PlayerWin(2);
            }
        }
    }

    void PlayerWin(int playerNum)
    {
        if (playerNum == 1)
        {
            playerOneLevel++;
        }
        else {
            playerTwoLevel++;
        }

        StartCoroutine(ProcessWin(2));
    }

    IEnumerator ProcessWin(float delay)
    {
        GameObject pacMan = GameObject.Find("pacman_1");
        pacMan.transform.GetComponent<pacman_1>().canMove = false;
        pacMan.transform.GetComponent<Animator>().enabled = false;

        transform.GetComponent<AudioSource>().Stop();

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
            ghost.transform.GetComponent<Animator>().enabled = false;
        }

        yield return new WaitForSeconds(delay);

        StartCoroutine(BlinkBoard(2));
    }

    IEnumerator BlinkBoard(float delay)
    {
        GameObject pacMan = GameObject.Find("pacman_1");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        //-blink board
        shouldBlink = true;
        yield return new WaitForSeconds (delay);
        //-restart the game at next level
        shouldBlink = false;
        StartNextLevel();
    }

    private void StartNextLevel()
    {
        SceneManager.LoadScene("pcuni");
    }

    private void CheckShouldBlink()
    {
        if (shouldBlink)
        {
            if (blinkIntervalTimer < blinkIntervalTime)
            {
                blinkIntervalTimer += Time.deltaTime;
            }
            else
            {
                blinkIntervalTimer = 0;

                if (GameObject.Find("maze").transform.GetComponent<SpriteRenderer>().sprite == mazeBlue)
                {
                    GameObject.Find("maze").transform.GetComponent<SpriteRenderer>().sprite = mazeWhite;
                }
                else
                {
                    GameObject.Find("maze").transform.GetComponent<SpriteRenderer>().sprite = mazeBlue;
                }
            }
        }
    }

    public void StartGame()
    {
        if (GameMenu.isOnePlayerGame)
        {
            playerTwoUp.GetComponent<Text>().enabled = false;
            playerTwoScoreText.GetComponent<Text>().enabled = false;
        }
        else
        {
            playerTwoUp.GetComponent<Text>().enabled = true;
            playerTwoScoreText.GetComponent<Text>().enabled = true;
        }
        if (isPlayerOneUp)
        {
            StartCoroutine(StartBlinking(playerOneUp));
        }
        else
        {
            StartCoroutine(StartBlinking(playerTwoUp));
        }
        //-hide all ghosts
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;

            ghost.transform.GetComponent<Ghost>().canMove = false;


        }

        GameObject pacMan = GameObject.Find("pacman_1");

        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

        pacMan.transform.GetComponent<pacman_1>().canMove = false;

        StartCoroutine(ShowObjectsAfter (2.25f));

    }

    public void StartConsumed(Ghost consumedGhost)
    {
        if (!didStartConsumed)
        {
            didStartConsumed = true;
            //-pause all ghosts
            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }
            //-pause pac-man
            GameObject pacMan = GameObject.Find("pacman_1");
            pacMan.transform.GetComponent<pacman_1>().canMove = false;

            //-hide pac-man
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            //-hide the consumed ghost 
            consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = false;

            //-stop background music
            transform.GetComponent<AudioSource>().Stop();

            Vector2 pos = consumedGhost.transform.position;

            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

            consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

            consumedGhostScoreText.GetComponent<Text>().enabled = true;

            //-play the consumed sound
            transform.GetComponent<AudioSource>().PlayOneShot(consumedGhostAudioClip);

            //-wait for the audio clip to finish
            StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }

    IEnumerator StartBlinking(Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);

        blinkText.GetComponent<Text>().enabled = !blinkText.GetComponent<Text>().enabled;
        StartCoroutine(StartBlinking(blinkText));
    }

    IEnumerator ProcessConsumedAfter(float delay, Ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);

        //-hide the score
        consumedGhostScoreText.GetComponent<Text>().enabled = false;

        //-show pac-man
        GameObject pacMan = GameObject.Find("pacman_1");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        //-show consumed ghost
        consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = true;

        //resumeall ghosts
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }

        //-resume pacman
        pacMan.transform.GetComponent<pacman_1>().canMove = true;

        //-start background music
        transform.GetComponent<AudioSource>().Play();

        didStartConsumed = false;

    }

    IEnumerator ShowObjectsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
        }

        GameObject pacMan = GameObject.Find("pacman_1");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
        playerText.transform.GetComponent<Text>().enabled = false;

        StartCoroutine(StartGameAfter (2));
    }

    IEnumerator StartGameAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }

        GameObject pacMan = GameObject.Find("pacman_1");
        pacMan.transform.GetComponent<pacman_1>().canMove = true;

        ReadyText.transform.GetComponent<Text>().enabled = false;

        transform.GetComponent<AudioSource>().clip = backgroundAudioNormal;
        transform.GetComponent<AudioSource>().Play();
    }

    public void StartDeath()
    {
        if (!didStartDeath)
        {
            StopAllCoroutines();

            if (GameMenu.isOnePlayerGame)
            {
                playerOneUp.GetComponent<Text>().enabled = true;
            }
            else
            {
                playerOneUp.GetComponent<Text>().enabled = true;
                playerTwoUp.GetComponent<Text>().enabled = true;
            }
            didStartDeath = true;

            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }

            GameObject pacMan = GameObject.Find("pacman_1");
            pacMan.transform.GetComponent<pacman_1>().canMove = false;

            pacMan.transform.GetComponent<Animator>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            StartCoroutine(ProcessDeathAfter(2));

        }
    }

    IEnumerator ProcessDeathAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        StartCoroutine(ProcessDeathAnimation(1.9f));
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        GameObject pacMan = GameObject.Find("pacman_1");

        pacMan.transform.localScale = new Vector3 (1, 1, 1);
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);

        pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<pacman_1>().deathAnimation;
        pacMan.transform.GetComponent<Animator>().enabled = true;

        transform.GetComponent<AudioSource>().clip = backgroundAudioPacManDeath;
        transform.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessRestart(1));
    }

    IEnumerator ProcessRestart(float delay)
    {
        pacManLives -= 1;

        if (pacManLives == 0)
        {
            playerText.transform.GetComponent<Text>().enabled = true;

            ReadyText.transform.GetComponent<Text>().text = "GAME OVER";
            ReadyText.transform.GetComponent<Text>().color = Color.red;

            ReadyText.transform.GetComponent<Text>().enabled = true;

            GameObject pacMan = GameObject.Find("pacman_1");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            StartCoroutine(ProcessGameOver(2));
        }
        else
        {
            playerText.transform.GetComponent<Text>().enabled = true;
            ReadyText.transform.GetComponent<Text>().enabled = true;

            GameObject pacMan = GameObject.Find("pacman_1");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            yield return new WaitForSeconds(delay);

            StartCoroutine(ProcessRestartShowObjects(1));
        }
    }

    IEnumerator ProcessGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("GameMenu");
        playerOneScore = 0;
    }
    IEnumerator ProcessRestartShowObjects(float delay)
    {
        playerText.transform.GetComponent<Text>().enabled = false;
        

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
            ghost.transform.GetComponent<Ghost>().MoveToStartingPosition();
        }

        GameObject pacMan = GameObject.Find("pacman_1");

        pacMan.transform.GetComponent<Animator>().enabled = false;
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
        pacMan.transform.GetComponent<pacman_1>().MoveToStartingPosition();

        yield return new WaitForSeconds(delay);
        Restart();
    }



    public void Restart()
    {
        ReadyText.transform.GetComponent<Text>().enabled = false;

       

        GameObject pacMan = GameObject.Find("pacman_1");
        pacMan.transform.GetComponent<pacman_1>().Restart();

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().Restart();
        }

        transform.GetComponent<AudioSource>().clip = backgroundAudioNormal;
        transform.GetComponent<AudioSource>().Play();

        didStartDeath = false;
    }
	
	// Update is called once per frame
	
}
