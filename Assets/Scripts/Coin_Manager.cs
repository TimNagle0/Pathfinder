using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin_Manager : MonoBehaviour
{
    // Lists for storing the coins and their positions
    public List<GameObject> coins = new List<GameObject>(); // Coins in the scene
    public List<GameObject> scoreCoins = new List<GameObject>(); // Transparent coins at the top
    private List<Vector2> positions = new List<Vector2>();
    private List<Vector2> scoreCoinPositions = new List<Vector2>();
    public GameObject coinPrefab;
    public Vector2 position_1, position_2, position_3;
    public Color transparentCoin;
    public Color normalCoin = Color.white;

    // Research version of the game
    int rv;


    void Start()
    {
        UI_Button_Controller UIController = GameObject.Find("Canvas").GetComponent<UI_Button_Controller>();
        transparentCoin = new Color(1f, 1f, 1f, 0.3f);
        rv = UIController.researchVersion;//PlayerPrefs.GetInt("researchVersion");
    }

    // Instantiate the transparent coins and get the positions from the actual coins in the scene
    // Add them both to their respective lists
    public void SetupCoins()
    {
        for (int i = 0; i < 3; i++)
        {
            scoreCoinPositions.Add(new Vector2(-1 + i, 3.40f));
            GameObject placeholderCoin = Instantiate(coinPrefab, scoreCoinPositions[i], Quaternion.identity);
            placeholderCoin.GetComponent<SpriteRenderer>().color = transparentCoin;
            scoreCoins.Add(placeholderCoin);
            positions.Add(gameObject.transform.GetChild(i).transform.position);
            coins.Add(gameObject.transform.GetChild(i).gameObject);
        }
    }

    // For the experiment version without coins
    public void DisableCoins()
    {
        gameObject.SetActive(false);
        foreach (GameObject scoreCoin in scoreCoins)
        {
            scoreCoin.SetActive(false);
        }
    }

    // If a player hits a coin, destroy it and turn on the right coin a the top
    public void moveToScore(GameObject coin, int score)
    {
        scoreCoins[score].GetComponent<SpriteRenderer>().color = normalCoin;
        Destroy(coin);
    }

    // Spawn all the coins in the scene on their original positions
    public void SpawnCoins()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject coin = Instantiate(coinPrefab, positions[i], Quaternion.identity);
            coin.transform.parent = gameObject.transform;
            coins.Add(coin);
        }
    }

    //Reset the coins by destroying them all and then respawning them
    public void ResetCoins()
    {
        foreach (GameObject coin in coins)
        {
            Destroy(coin);
        }
        foreach (GameObject placholderCoin in scoreCoins)
        {
            placholderCoin.GetComponent<SpriteRenderer>().color = transparentCoin;
        }
        SpawnCoins();
    }
}
