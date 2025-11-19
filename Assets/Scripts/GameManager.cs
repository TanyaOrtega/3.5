using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject gruntPrefab;

    // Vidas del jugador
    public int playerLives = 3;
    public float respawnDelay = 1.2f;

    private JohnMovement john;
    private Vector3 johnStartPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        john = FindFirstObjectByType<JohnMovement>();
        if (john != null)
        {
            johnStartPosition = john.transform.position;
        }
        RespawnGrunt();
    }

    void Update()
    {
        if (john == null)
            john = FindFirstObjectByType<JohnMovement>();

        if (ScoreManager.score >= 500)
        {
            LoadNextLevel();
        }
    }

    // --- GRUNT SPAWN (robusto: busca plataforma válida por raycast) ---
    public void RespawnGrunt()
    {
        if (john == null)
            john = FindFirstObjectByType<JohnMovement>();

        int groundMask = LayerMask.GetMask("Ground");

        Vector3 spawnPosition = Vector3.zero;
        bool foundGround = false;

        for (int i = 0; i < 15; i++)
        {
            float offsetX = Random.Range(1.2f, 3f);
            if (Random.value > 0.5f) offsetX *= -1;

            // Lanzamos el raycast desde arriba (para asegurar detectar tiles)
            Vector3 tryPos = (john != null ? john.transform.position : Vector3.zero) + new Vector3(offsetX, 3f, 0);

            RaycastHit2D hit = Physics2D.Raycast(tryPos, Vector2.down, 10f, groundMask);
            Debug.DrawRay(tryPos, Vector2.down * 10f, Color.red, 2f);

            if (hit.collider != null)
            {
                spawnPosition = new Vector3(hit.point.x, hit.point.y + 0.35f, -1f);
                foundGround = true;
                break;
            }
        }

        if (!foundGround)
        {
            // fallback seguro: a un lado de John, a su altura
            Vector3 basePos = (john != null ? john.transform.position : Vector3.zero);
            spawnPosition = basePos + new Vector3(1.5f, 0f, -1f);
            Debug.Log("⚠ No se encontró piso, usando spawn seguro.");
        }

        GameObject newGrunt = Instantiate(gruntPrefab, spawnPosition, Quaternion.identity);
        GruntScript gs = newGrunt.GetComponent<GruntScript>();
        if (gs != null) gs.John = john;

        Debug.Log("🐺 Grunt generado en " + spawnPosition);
    }

    // --- PLAYER RESPAWN & LIVES ---
    // Llamar desde John cuando muera (caída, recibir daño letal)
    public void OnPlayerDeath()
    {
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        // Reducir vidas
        playerLives = Mathf.Max(0, playerLives - 1);
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null) ui.UpdateLivesDisplay(playerLives);

        if (playerLives <= 0)
        {
            // Game Over: esperar un momento y volver al menu principal
            if (ui != null) ui.ShowGameOverPanel();
            yield return new WaitForSeconds(1.2f);
            // Cargar escena MainMenu (asegúrate de que esté en Build Settings)
            SceneManager.LoadScene("MainMenu");
            // Resetear score y vidas si quieres predeterminado
            ScoreManager.ResetScore();
            playerLives = 3; // o el valor por defecto que quieras
            yield break;
        }

        // Reaparecer al jugador después de un delay
        if (ui != null) ui.PlayRespawnAnimation();
        yield return new WaitForSeconds(respawnDelay);

        JohnMovement player = FindFirstObjectByType<JohnMovement>();
        if (player == null)
        {
            // Si el player no existe (por ejemplo escena recargada), instanciamos/posicionamos
            player = FindFirstObjectByType<JohnMovement>(); // intento extra
        }

        // Respawn en el último punto conocido (o inicio de la escena)
        Vector3 respawnPos = johnStartPosition;
        if (player != null)
        {
            player.transform.position = respawnPos;
            player.ResetStateAfterDeath(); // método en JohnMovement (ver script abajo)
        }
    }

    // Para cambiar manualmente el punto de respawn (por ejemplo checkpoints)
    public void SetPlayerRespawnPoint(Vector3 pos)
    {
        johnStartPosition = pos;
    }

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            ScoreManager.ResetScore();
        }
        else
        {
            Debug.Log("🎉 ¡Felicidades! No hay más niveles.");
        }
    }
}
