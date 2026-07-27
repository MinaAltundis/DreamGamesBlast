using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneController : MonoBehaviour
{
    [SerializeField] private Button levelButton;
    [SerializeField] private TMP_Text levelButtonText;

    private void Awake()
    {
        if (levelButton != null)
        {
            levelButton.onClick.AddListener(
                OpenCurrentLevel);
        }
    }

    private void Start()
    {
        RefreshLevelButton();
    }

    private void OnDestroy()
    {
        if (levelButton != null)
        {
            levelButton.onClick.RemoveListener(
                OpenCurrentLevel);
        }
    }

    private void RefreshLevelButton()
    {
        bool finished =
            ProgressService.HasFinishedAllLevels();

        if (levelButtonText != null)
        {
            levelButtonText.text =
                finished
                    ? "Finished"
                    : $"Level {ProgressService.GetCurrentLevel()}";
        }

        if (levelButton != null)
        {
            levelButton.interactable =
                !finished;
        }
    }

    private void OpenCurrentLevel()
    {
        if (ProgressService.HasFinishedAllLevels())
        {
            return;
        }

        SceneManager.LoadScene(
            SceneNames.Level);
    }
}