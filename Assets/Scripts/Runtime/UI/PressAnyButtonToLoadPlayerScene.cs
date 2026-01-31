using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Runtime.UI
{
    class PressAnyButtonToLoadPlayerScene : MonoBehaviour
    {
        [SerializeField] private SceneAsset sceneToLoad;
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

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad.name, LoadSceneMode.Single);
            operation.allowSceneActivation = false;

            while (operation.progress <= 0.8)
            {
                yield return null;
            }

            operation.allowSceneActivation = true;

            yield return null;
        }
    }
}
