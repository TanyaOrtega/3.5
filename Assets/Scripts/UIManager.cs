using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI livesText;
    public GameObject gameOverPanel;
    public GameObject respawnEffectObject; 

    void Start()
    {
        gameOverPanel.SetActive(false);
        if (GameManager.Instance != null) UpdateLivesDisplay(GameManager.Instance.playerLives);
    }

    public void UpdateLivesDisplay(int lives)
    {
        if (livesText != null) livesText.text = "Vidas: " + lives;
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void PlayRespawnAnimation()
    {
        if (respawnEffectObject != null)
        {
            respawnEffectObject.SetActive(true);
            StartCoroutine(DisableAfter(respawnEffectObject, 0.6f));
        }
    }

    private IEnumerator DisableAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go != null) go.SetActive(false);
    }
}
