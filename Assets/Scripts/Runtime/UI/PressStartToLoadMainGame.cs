using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Runtime.UI
{
    public class PressStartToLoadMainGame : MonoBehaviour
    {
        [SerializeField] private SceneAsset sceneAsset;
        [SerializeField] private Image image;

        private Coroutine m_CoLoadScene;

        private void Update()
        {
            if(Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if (m_CoLoadScene == null)
                {
                    m_CoLoadScene = StartCoroutine(nameof(CoLoadScene));
                }
            }
        }

        private IEnumerator CoLoadScene()
        {
            Color color = image.color;
            color.a = 0.5f;
            image.color = color;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneAsset.name, LoadSceneMode.Single);
            operation.allowSceneActivation = false;
            while (operation.progress <= 0.8)
            {
                yield return null;
            }

            operation.allowSceneActivation = true;
        }
    }
}