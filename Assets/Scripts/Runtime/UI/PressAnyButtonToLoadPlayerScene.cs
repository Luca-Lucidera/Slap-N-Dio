using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Runtime.UI
{
    class PressAnyButtonToLoadPlayerScene : MonoBehaviour
    {
        [SerializeField] private int sceneToLoadBuildIndex = 1;
        private Coroutine m_CoLoadScene;
        private TextMeshProUGUI m_text;

        private void Awake()
        {
            m_text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                if (m_CoLoadScene == null)
                {
                    m_CoLoadScene = StartCoroutine(nameof(LoadPlayerScene));
                }
            }
        }

        private IEnumerator LoadPlayerScene()
        {
            m_text.text = $"Caricamento in corso...";

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoadBuildIndex, LoadSceneMode.Single);
            operation.allowSceneActivation = false;

            while (operation.progress <= 0.8f)
            {
                yield return null;
            }

            operation.allowSceneActivation = true;

            yield return null;
        }
    }
}
