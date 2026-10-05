using System.Collections;
using UnityEngine;

namespace UnboundLib.GameModes
{
    public class SandboxHandler : GameModeHandler<GM_Test>
    {
        public override string Name => "Sandbox";
        public override bool AllowTeams => true;
        public override UISettings UISettings => new UISettings("A sandbox mode where you can play around, test out builds, and fight bots.");

        public override GameSettings Settings { get; protected set; }

        public SandboxHandler() : base(GameModeManager.SandBoxID) {
            Settings = new GameSettings();
        }

        public override void PlayerJoined(Player player)
        {
            // The old game's sandbox loaded its first map before letting players join; the 2025 GM_Test lets them join
            // first (OnEnable). A player added before the map was there threw in PlayerWasAdded (no map to spawn on) and
            // never spawned; a custom map, which takes longer to load, made that likely. Wait for the map.
            if (MapManager.instance.currentMap == null || MapManager.instance.currentMap.Map == null)
            {
                Unbound.Instance.StartCoroutine(AddWhenMapLoaded(player));
                return;
            }
            GameMode.InvokeMethod("PlayerWasAdded", player);
        }

        private IEnumerator AddWhenMapLoaded(Player player)
        {
            var giveUp = Time.realtimeSinceStartup + 30f;
            while ((MapManager.instance.currentMap == null || MapManager.instance.currentMap.Map == null) && Time.realtimeSinceStartup < giveUp)
            {
                yield return null;
            }
            if (player != null && GameMode != null) GameMode.InvokeMethod("PlayerWasAdded", player);
        }

        public override void PlayerDied(Player killedPlayer, int playersAlive)
        {
            GameMode.InvokeMethod("PlayerDied", killedPlayer, playersAlive);
        }

        public override TeamScore GetTeamScore(int teamID)
        {
            return new TeamScore(0, 0);
        }

        public override void SetTeamScore(int teamID, TeamScore score) { }

        public override void SetActive(bool active)
        {
            if (!active)
            {
                GameMode.gameObject.SetActive(active);
            }
        }

        public override void StartGame()
        {
            GameMode.gameObject.SetActive(true);
        }

        public override void ResetGame()
        {
            PlayerManager.instance.InvokeMethod("ResetCharacters");
        }
    }
}
