using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PartCatalog
{
    class EditorLockManager
    {
        private EditorLockManager()
        {
            
        }
        private static EditorLockManager instance = new EditorLockManager();

        public static EditorLockManager Instance
        {
            get
            {
                return instance;
            }
        }

        private bool shouldLockGUI = false;
        private bool weLockedGUI = false;

        public void StartGUIDraw()
        {
            shouldLockGUI = false;
        }

        public void LockGUI()
        {
            shouldLockGUI = true;            
        }
        public void EndGUIDraw()
        {
            if(shouldLockGUI && !weLockedGUI)
            {
                EditorLogic.fetch.Lock(true, true, true, "PartCatalog_GUI");
                weLockedGUI = true;
            }
            if(weLockedGUI && !shouldLockGUI)
            {                
                EditorLogic.fetch.Unlock("PartCatalog_GUI");
            }
        }


        private bool shouldLockUpdate = false;
        private bool weLockedUpdate = false;

        public void StartUpdate()
        {
            shouldLockUpdate = false;
        }

        public void LockUpdate()
        {
            shouldLockUpdate = true;
        }
        public void EndUpdate()
        {
            if (shouldLockUpdate && !weLockedUpdate)
            {
                UnityEngine.Debug.Log("Lock");

                InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK,"PartCatalog_Update");
                weLockedUpdate = true;
            }
            if (!shouldLockUpdate && weLockedUpdate)
            {
                UnityEngine.Debug.Log("Unlock");
                InputLockManager.RemoveControlLock("PartCatalog_Update");
                weLockedUpdate = false;
            }
        }

    }
}
