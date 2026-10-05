using HarmonyLib;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace UnboundLib.Patches
{
    [HarmonyPatch(typeof(EscapeMenuHandler))]
    public class EscapeMenuHandlerPath
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool Update(EscapeMenuHandler __instance)
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || ToggleCardsMenuHandler.disableEscapeButton) return true;
            
            if (ToggleLevelMenuHandler.instance.mapMenuCanvas.transform.Find("MapMenu/InfoMenu").gameObject.activeInHierarchy)
            {
                ToggleLevelMenuHandler.instance.mapMenuCanvas.transform.Find("MapMenu/InfoMenu").gameObject
                    .SetActive(false);
                return false;
            }
                
            if (ToggleCardsMenuHandler.cardMenuCanvas.transform.Find("CardMenu/InfoMenu").gameObject.activeInHierarchy)
            {
                ToggleCardsMenuHandler.cardMenuCanvas.transform.Find("CardMenu/InfoMenu").gameObject
                    .SetActive(false);
                if(ToggleCardsMenuHandler.menuOpenFromOutside) ToggleCardsMenuHandler.Close();
                return false;
            }

            if (ToggleLevelMenuHandler.instance.mapMenuCanvas.activeInHierarchy)
            {
                ToggleLevelMenuHandler.instance.mapMenuCanvas.SetActive (false);
                return false;
            }
                
            if (!ToggleCardsMenuHandler.disableEscapeButton && ToggleCardsMenuHandler.cardMenuCanvas.activeInHierarchy)
            {
                ToggleCardsMenuHandler.SetActive(ToggleCardsMenuHandler.cardMenuCanvas.transform, false);
                if(ToggleCardsMenuHandler.menuOpenFromOutside) ToggleCardsMenuHandler.Close();
                return false;
            }
            
            // The old game left a submenu's own GoBack to handle Escape, so this skipped the escape menu unless "Main" was
            // showing. Pause-menu pages made by MenuHandler have no GoBack any more (the 2025 GoBack doesn't go back while
            // a keyboard player is in the game), so Escape did nothing in MODS and its submenus. Escape now presses the
            // page's Back button: it closes the page and runs what leaving it needs (an old-UI mod menu unlocks input).
            // Everything else is the game's: ToggleEsc closes its own pages and the escape menu.
            foreach (Transform child in __instance.transform)
            {
                if (!child.GetComponent<MenuHandler.PauseMenuPage>()) continue;
                var group = child.Find("Group");
                if (!group || !group.gameObject.activeInHierarchy) continue;
                var back = group.Find("Back")?.GetComponent<UnityEngine.UI.Button>();
                if (!back) continue;
                back.onClick.Invoke();
                return false;
            }

            return true;
        }

        // Enter opens the game's console (in sandbox: the box to type a card name in). Opening the pause menu left it
        // open on top of the menu and everything opened from it; it now closes, without sending what was typed.
        [HarmonyPatch("ToggleEsc")]
        [HarmonyPostfix]
        private static void CloseConsole()
        {
            if (!EscapeMenuHandler.isEscMenu || !DevConsole.isTyping) return;
            var console = Object.FindObjectOfType<DevConsole>();
            if (!console || !console.inputField) return;
            console.inputField.text = "";
            console.inputField.gameObject.SetActive(false);
            DevConsole.isTyping = false;
            GameManager.lockInput = false;
        }
    }
}