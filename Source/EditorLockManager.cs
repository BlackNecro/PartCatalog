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
                InputLockManager.SetControlLock(ControlTypes.EDITOR_PAD_PICK_COPY | ControlTypes.EDITOR_PAD_PICK_PLACE | ControlTypes.CAMERACONTROLS, "PartCatalog_GUI");
                weLockedGUI = true;
            }
            if(weLockedGUI && !shouldLockGUI)
            {
                InputLockManager.RemoveControlLock("PartCatalog_GUI");
                weLockedGUI = false;
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
                InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "PartCatalog_Update");
                weLockedUpdate = true;
            }
            if (!shouldLockUpdate && weLockedUpdate)
            {
                InputLockManager.RemoveControlLock("PartCatalog_Update");
                weLockedUpdate = false;
            }
        }

    }
}
