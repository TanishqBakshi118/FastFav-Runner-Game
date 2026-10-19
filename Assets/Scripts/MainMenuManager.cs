using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject instructionsPanel;

    [Header("Text Elements")]
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        // Ensure the menu starts in the correct state
        ShowMenu();
        LoadHighScore();
    }

    void LoadHighScore()
    {
        int savedHighScore = 0;
        string path = Application.persistentDataPath + "/savefile.json";

        if (File.Exists(path))
        {
            string jsonLoad = File.ReadAllText(path);
            // Uses the SaveData class you already defined in your game script
            SaveData dataLoad = JsonUtility.FromJson<SaveData>(jsonLoad);
            savedHighScore = dataLoad.highScore;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + savedHighScore;
        }
    }

    public void PlayGame()
    {
        // Loads the next scene in your Build Settings (your actual game)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowInstructions()
    {
        menuPanel.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void ShowMenu()
    {
        instructionsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}