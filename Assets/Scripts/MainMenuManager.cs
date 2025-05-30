using UnityEngine;
using UnityEngine.SceneManagement; // Importante para gerenciar cenas

public class MainMenuManager : MonoBehaviour
{
    // Certifique-se de que o nome da sua cena de jogo esteja correto aqui!
    public string gameSceneName = "GameScene"; // Altere para o nome da sua cena de jogo

    public void StartGame()
    {
        // Carrega a cena do jogo
        SceneManager.LoadScene(gameSceneName);
    }

    // Você pode adicionar outras funções aqui, como um botão de "Sair"
    public void QuitGame()
    {
        Application.Quit(); // Fecha a aplicação
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Para o modo de jogo no Editor
        #endif
    }
}