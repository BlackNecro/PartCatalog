using System;
using System.Collections.Generic;
using UnityEngine;

namespace PartCatalog
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class PartCatalogBehavior : MonoBehaviour
    {
        //public static GameObject GameObjectInstance;

        private static PartCatalogBehavior instance = null;

        private bool launched = false;
        private DateTime launchTime = DateTime.MinValue;

        public void Awake()
        {
            instance = this;
            launched = false;
            launchTime = DateTime.MinValue;
        }

        private bool CheckInstance()
        {
            if(instance == this)
            {
                return true;
            }
            if(instance == null || instance == (UnityEngine.GameObject) null)
            {
                instance = this;
                return true;
            }
            return false;
        }

        public void OnDestroy()
        {
            if (CheckInstance())
            {
                ConfigHandler.Instance.SaveConfig();
                PartCatalog.Instance.SavePartTags();
            }
        }                                                

        public void OnGUI()
        {
            if (CheckInstance())
            {
                if (launched && HighLogic.LoadedScene == GameScenes.EDITOR || HighLogic.LoadedScene == GameScenes.SPH)
                {
                    if (EditorLogic.fetch.editorScreen == EditorLogic.EditorScreen.Parts)
                    {
                        EditorLockManager.Instance.StartGUIDraw();
                        GUILayoutSettings.Instance.Draw();
                        GUIEditorControls.Instance.Draw();
                        GUITagEditor.Instance.Draw();
                        SearchManager.Instance.Draw();
                        EditorLockManager.Instance.EndGUIDraw();
                    }
                }
            }
        }
        public void Update()
        {
            if (CheckInstance())
            {
                if (!launched)
                {


                    if (((ResearchAndDevelopment.Instance == null || EditorPartList.Instance == null) &&
                         HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX))
                    {
                        return;
                    }
                    
                    if (launchTime == DateTime.MinValue)
                    {
                        launchTime = DateTime.Now.AddSeconds(ConfigHandler.Instance.StartupDelay);
                    }

                    if (launchTime >= DateTime.Now)
                    {
                        return;
                    }


                    launched = true;

                    Debug.Log("****Loading PartCatalog ****");

                    ConfigHandler.Instance.LoadConfig();
                    PartCatalog.Instance.LoadPartTags();

                    GUIEditorControls.Instance.UpdateDisplayedTags();
                    PartFilterManager.Instance.EnablePartFilter();
                    if (ConfigHandler.Instance.FirstRun)
                    {
                        ConfigHandler.Instance.FirstRun = false;
                        ConfigHandler.Instance.ButtonSize.x = 37;
                        ConfigHandler.Instance.ButtonSize.y = 26;
                        GUILayoutSettings.Instance.Open();
                        LuaRuleHandler.Instance.ParseParts();
                        //PartCatalog.Instance.AutoTagByMod();
                    }
                    EditorPartList.Instance.ShowTabs();
                    EditorPartList.Instance.SelectTab(PartCategories.Pods);
                    SearchManager.Instance.UpdateSearchText("");

                    Debug.Log("**** Loaded PartCatalog ****");
                }
                else
                {
                    EditorLockManager.Instance.StartUpdate();
                    GUIEditorControls.Instance.Update();
                    SearchManager.Instance.OnUpdate();
                    GUITagEditor.Instance.Update();
                    PartFilterManager.Instance.Update();
                    EditorLockManager.Instance.EndUpdate();
                }
            }
        }
    }
}
