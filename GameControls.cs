using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameControls : MonoBehaviour
{
    //prefabs and objects to move
    public Transform snake;
    public Transform snakeTailSegmentPrefab;
    public Transform foodPrefab;

    //movement values
    public float moveIntervalInSeconds = 0.3f;
    public float speedUpInSeconds = 0.1f;
    public float minIntervalInSeconds = 0.05f;

    //UI to update
    public Text scoreText;

    //Audio sources
    public AudioSource pickupSfx;
    public AudioSource dieSfx;
    public AudioSource moveSfx;

    //time keeping
    private float nextMoveTime = 0;
    private float currentIntervalInSeconds;

    //location of snake's head
    private Vector2Int headLocation = new Vector2Int(10, 10);

    //direction of movement
    private Vector2Int direction = new Vector2Int(0, 1);

    //tail objects and locations
    private List<Transform> tails = new List<Transform>();
    private List<Vector2Int> tailsPositions = new List<Vector2Int>();

    //food
    private Vector2Int currentFoodLocation;
    private Transform currentFood;

    //scores
    private int bestScore = 0;
    private int score = 0;



    // Start is called before the first frame update
    void Start()
    {
        //reset all positions, tails, timers and scores
        ResetGame();

        //Spawn some food
        SpawnFood();
        
    }

    // Update is called once per frame
    void Update()
    {
        //check key presses and change direction
        if (direction.x!=-1 && Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction.x = 1;
            direction.y = 0;
        }

        if (direction.x !=1 && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction.x = -1;
            direction.y = 0;
        }

        if (direction.y!=-1 && Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction.x = 0;
            direction.y = 1;
        }

        if (direction.y!=-1 && Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction.x = 0;
            direction.y = -1;
        }

        //move in current direction if it's time to move
        if (Time.time>nextMoveTime)
        {
            //update the score
            
            scoreText.text = $"BEST {bestScore}   ||  SCORE {score}";

            //add current head position at the start of tail positions list and
            //remove the last position. In effect, it moves all the tail segments
            //one position along

            tailsPositions.Insert(0, headLocation);
            tailsPositions.RemoveAt(tailsPositions.Count - 1);

            //move the head towards the direction the snake is moving
            headLocation += direction;

            //check if snake has gone off the edge and if so, move to the other side
            if (headLocation.x < 0)
                headLocation.x = 19;

            if (headLocation.x > 19)
                headLocation.x = 0;

            if (headLocation.y < 0)
                headLocation.y = 19;

            if (headLocation.y > 19)
                headLocation.y = 0;


            //check if snake has hit its own tail
            bool isDead = false;
            foreach (Vector2Int tailSegmentPosition in tailsPositions)
            {
                if (headLocation.Equals(tailSegmentPosition))
                {
                    isDead = true;
                    break;
                }
            }


            //did the snake hit its tail?
            if (isDead)
            {
                //yes, so game is over and reset
                ResetGame();

                //dieSfx.Play();

                //delay move by 3 seconds so player can see that game is reset
                nextMoveTime = Time.time + 2f;

                //update snake's position
                snake.position = new Vector3(headLocation.x + 0.5f, headLocation.y + 0.5f, snake.position.z);
            }
            else
            {
                //no, so continue and update tail
                nextMoveTime = Time.time + currentIntervalInSeconds;
                snake.position = new Vector3(headLocation.x + 0.5f, headLocation.y + 0.5f, snake.position.z);

                //loop through each tail object and set its current position
                for (int loop = 0; loop < tails.Count; loop++)
                    tails[loop].position = new Vector3(tailsPositions[loop].x + 0.5f, tailsPositions[loop].y + 0.5f, snake.position.z);

                //moveSfx.Play();
            }


            //check if we have hit food
            if (headLocation.Equals(currentFoodLocation))
            {
                //add a new tail segment at current head location
                AddTail(headLocation);

                //pickupSfx.Play();

                //destroy eaten food and spawn new food
                Destroy(currentFood.gameObject);
                SpawnFood();

                //shrink the movement interval to speed up snake
                if (tails.Count % 10 == 0)
                    currentIntervalInSeconds = Mathf.Max(minIntervalInSeconds, currentIntervalInSeconds - speedUpInSeconds);

                //change best score if current score is better
                if (++score > bestScore)
                    bestScore = score;                
            }
        }
    }



    //reset
    private void ResetGame()
    {
        headLocation = new Vector2Int(10, 10);
        direction = new Vector2Int(0, 1);
        currentIntervalInSeconds = moveIntervalInSeconds;
        score = 0;

        //destroy tail objects
        foreach (Transform tailSegment in tails)
        {
            Destroy(tailSegment.gameObject);
        }

        //clear the lists of tail objects and positions
        tails.Clear();
        tailsPositions.Clear();
    }
    

    //spawn food
    private void SpawnFood()
    {
        if (tails.Count==0)
            currentFoodLocation = new Vector2Int(Random.Range(0, 19), Random.Range(0, 19));                        
        else
        {
            bool foodOnTail = true;
            while (foodOnTail)
            {
                currentFoodLocation = new Vector2Int(Random.Range(0, 19), Random.Range(0, 19));
                foreach (Vector2Int tailSegmentPosition in tailsPositions)
                    foodOnTail = currentFoodLocation == tailSegmentPosition ? true : false;            
            }
        }
        
        currentFood = Instantiate<Transform>(foodPrefab, new Vector3(currentFoodLocation.x + 0.5f, currentFoodLocation.y + 0.5f), Quaternion.identity);
    }

    private void AddTail(Vector2Int location)
    {
        tails.Add(Instantiate<Transform>(snakeTailSegmentPrefab, new Vector3(location.x + 0.5f, location.y + 0.5f), Quaternion.identity));
        tailsPositions.Add(location);
    }

}
