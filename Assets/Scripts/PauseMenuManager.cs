using UnityEngine;
using UnityEngine.SceneManagement; // Para carregar cenas

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Referência ao painel do menu de pausa que criamos

    public string mainMenuSceneName = "MenuScene"; // Nome da sua cena de menu principal

    // Garante que o menu de pausa esteja desativado no início
    void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = 1f; // Garante que o jogo não esteja pausado ao iniciar
    }

    // Verifica a entrada do teclado para pausar/despausar
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (pauseMenuUI.activeSelf) // Se o menu de pausa já estiver ativo
            {
                ResumeGame(); // Despausa o jogo
            }
            else // Se o menu de pausa não estiver ativo
            {
                PauseGame(); // Pausa o jogo
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false); // Esconde o painel do menu de pausa
        }
        Time.timeScale = 1f; // Retorna o tempo do jogo ao normal
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true); // Mostra o painel do menu de pausa
        }
        Time.timeScale = 0f; // Pausa o tempo do jogo (faz com que tudo pare)
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Garante que o tempo do jogo volte ao normal antes de carregar outra cena
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitApplication()
    {
        Application.Quit(); // Fecha a aplicação
        #if UNITY_EDITOR // Linhas apenas para funcionar no editor
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}