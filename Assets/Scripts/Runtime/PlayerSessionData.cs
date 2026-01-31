using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Runtime
{
    /// <summary>
    /// Singleton that persists between scenes to store player join data.
    /// </summary>
    public class PlayerSessionData : MonoBehaviour
    {
        public static PlayerSessionData Instance { get; private set; }

        public List<PlayerJoinInfo> JoinedPlayers { get; private set; } = new List<PlayerJoinInfo>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetJoinedPlayers(List<PlayerJoinInfo> players)
        {
            JoinedPlayers = new List<PlayerJoinInfo>(players);
        }

        public void ClearPlayers()
        {
            JoinedPlayers.Clear();
        }
    }
}
