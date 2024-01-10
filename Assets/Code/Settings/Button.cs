using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    [SerializeField] private int numberChangeScean;

    public void ExitGame()
    {
        Application.Quit();
    }

    public void �hangeScene()
    {
        SceneManager.LoadScene(numberChangeScean);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(1);
    }

    public void Resume()
    {
        EntryPoint.Instance.Fsm.EnterIn<GameplayState>();
    }
}
