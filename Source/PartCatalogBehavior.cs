using System;
using System.Collections.Generic;
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
            return;
            PartCatalog.Instance.Dispose();
        }                                                

        public void OnGUI()
        {
            return;
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
            return;
            if (!launched && ((ResearchAndDevelopment.Instance != null && EditorPartList.Instance != null ) || HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX))          // Wait for some to fire up
            {
                launched = true;                

                Debug.Log("****Loading PartCatalog ****");

                ConfigHandlerHandler.Instance.LoadConfig();
                PartCatalog.Instance.LoadPartTags();

                GUIEditorControls.Instance.UpdateDisplayedTags();
                PartFilterManager.Instance.EnablePartFilter();
                if (ConfigHandlerHandler.Instance.FirstRun )
                {
                    ConfigHandlerHandler.Instance.FirstRun = false;
                    ConfigHandlerHandler.Instance.ButtonSize.x = 37;
                    ConfigHandlerHandler.Instance.ButtonSize.y = 26;
                    GUILayoutSettings.Instance.Open();
                    PartCatalog.Instance.AutoTagByMod();
                }
                EditorPartList.Instance.ShowTabs();
                EditorPartList.Instance.SelectTab(PartCategories.Pods);

                Debug.Log("**** Loaded PartCatalog ****");
            }
            else
            {
                GUIEditorControls.Instance.Update();
            }
        }
    }
}
