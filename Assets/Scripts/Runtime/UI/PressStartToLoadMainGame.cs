using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Runtime.UI
{
    public class PressStartToLoadMainGame : MonoBehaviour
    {
        [SerializeField] private int sceneToLoadBuildIndex = 2;
        [SerializeField] private Image image;

        private Coroutine m_CoLoadScene;

        private void Update()
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if (m_CoLoadScene == null)
                {
                    // Finalize player selection before loading
                    if (PlayerSelectionManager.Instance != null)
                    {
                        PlayerSelectionManager.Instance.FinalizeSelection();
                    }
                    m_CoLoadScene = StartCoroutine(nameof(CoLoadScene));
                }
            }
        }

        private IEnumerator CoLoadScene()
        {
            Color color = image.color;
            color.a = 0.5f;
            image.color = color;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoadBuildIndex, LoadSceneMode.Single);
            operation.allowSceneActivation = false;
            while (operation.progress <= 0.8f)
            {
                yield return null;
            }

            operation.allowSceneActivation = true;
        }
    }
}
