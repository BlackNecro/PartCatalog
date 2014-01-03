using System;
using UnityEngine;

namespace PartCatalog
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class PartCatalogBehavior : MonoBehaviour
    {
        public static GameObject GameObjectInstance;

        private bool launched = false;

        public void Awake()
        {                                          
        }

        public void OnDestroy()
        {
            PartCatalog.Instance.Dispose();
        }                                                

        public void OnGUI()
        {
            if (launched && HighLogic.LoadedScene == GameScenes.EDITOR || HighLogic.LoadedScene == GameScenes.SPH)
            {
                if (EditorLogic.fetch.editorScreen == EditorLogic.EditorScreen.Parts)
                {
                    GUILayoutSettings.Instance.Draw();
                    GUIEditorControls.Instance.Draw();
                    GUITagEditor.Instance.Draw();
                }
            }
        }
        public void Update()
        {
            if (!launched && ((ResearchAndDevelopment.Instance != null && EditorPartList.Instance != null ) || HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX))          // Wait for some to fire up
            {
                launched = true;
                Debug.Log("****Loading PartCatalog****");

                ConfigHandler.Instance.LoadConfig();
                PartCatalog.Instance.LoadPartTags();

                GUIEditorControls.Instance.UpdateDisplayedTags();
                PartFilterManager.Instance.EnablePartFilter();
                if (ConfigHandler.Instance.FirstRun )
                {
                    ConfigHandler.Instance.FirstRun = false;
                    ConfigHandler.Instance.ButtonSize.x = 37;
                    ConfigHandler.Instance.ButtonSize.y = 26;
                    GUILayoutSettings.Instance.Open();
                    PartCatalog.Instance.AutoTagByMod();
                }
                EditorPartList.Instance.ShowTabs();
                EditorPartList.Instance.SelectTab(PartCategories.Pods);
            }
            else
            {
                GUIEditorControls.Instance.Update();
            }
        }
    }
}
