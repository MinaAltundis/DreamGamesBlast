using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [Header("Moves")]
    [SerializeField] private TMP_Text movesText;

    [Header("Fail Popup")]
    [SerializeField] private GameObject failPopup;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button closeButton;

    [Header("Win Celebration")]
    [SerializeField] private GameObject celebrationRoot;
    [SerializeField] private RectTransform celebrationStar;
    [SerializeField] private ParticleSystem[] celebrationParticles;
    [SerializeField] private float celebrationDuration = 1.6f;
    [SerializeField] private float starAnimationDuration = 0.55f;

    private void Awake()
    {
        if (failPopup != null)
        {
            failPopup.SetActive(false);
        }

        if (celebrationRoot != null)
        {
            celebrationRoot.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryLevel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ReturnToMainScene);
        }
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RetryLevel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ReturnToMainScene);
        }
    }

    public void SetMoves(int remainingMoves)
    {
        if (movesText != null)
        {
            movesText.text =
                Mathf.Max(0, remainingMoves).ToString();
        }
    }

    public void ShowFail()
    {
        if (failPopup != null)
        {
            failPopup.SetActive(true);
            failPopup.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError(
                "Fail popup is not assigned.");
        }
    }

    public IEnumerator PlayWinCelebration()
    {
        if (celebrationRoot != null)
        {
            celebrationRoot.SetActive(true);
            celebrationRoot.transform.SetAsLastSibling();
        }

        foreach (ParticleSystem particles
                 in celebrationParticles)
        {
            if (particles != null)
            {
                particles.Play();
            }
        }

        if (celebrationStar != null)
        {
            celebrationStar.localScale =
                Vector3.zero;

            celebrationStar.localRotation =
                Quaternion.Euler(0f, 0f, -25f);

            float duration =
                Mathf.Max(
                    0.01f,
                    starAnimationDuration);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed / duration);

                float eased =
                    1f -
                    Mathf.Pow(
                        1f - progress,
                        3f);

                float bounce =
                    Mathf.Sin(
                        progress * Mathf.PI) *
                    0.2f;

                celebrationStar.localScale =
                    Vector3.one *
                    (eased + bounce);

                celebrationStar.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        Mathf.Lerp(
                            -25f,
                            0f,
                            eased));

                yield return null;
            }

            celebrationStar.localScale =
                Vector3.one;

            celebrationStar.localRotation =
                Quaternion.identity;
        }

        float remainingTime =
            Mathf.Max(
                0f,
                celebrationDuration -
                starAnimationDuration);

        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingTime);
        }
    }

    private void RetryLevel()
    {
        SceneManager.LoadScene(
            SceneNames.Level);
    }

    private void ReturnToMainScene()
    {
        SceneManager.LoadScene(
            SceneNames.Main);
    }
}