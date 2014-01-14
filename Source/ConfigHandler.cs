using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace PartCatalog
{
    class ConfigHandler : IDisposable
    {
        static readonly ConfigHandler instance = new ConfigHandler();

        private ConfigHandler()
        {
            LoadConfig();            
        }

        
        public static ConfigHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public  void LoadConfig()
        {
            KSP.IO.PluginConfiguration config = KSP.IO.PluginConfiguration.CreateForType<PartCatalog>();
            config.load();

            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public))
            {
                foreach (Attribute at in field.GetCustomAttributes(false))
                {
                    if (at is SaveToConfig)
                    {
                        MethodInfo getValueMethod = null;
                        MethodInfo[] methods = typeof(KSP.IO.PluginConfiguration).GetMethods();
                        foreach(MethodInfo method in methods)
                        {
                            if(method.IsGenericMethodDefinition && method.ContainsGenericParameters && method.GetParameters().Length == 2)
                            {
                                getValueMethod = method.MakeGenericMethod(new Type[] {field.FieldType});
                                break;
                            }
                        }
                        if (getValueMethod != null)
                        {                            
                            object val = getValueMethod.Invoke(config, new object[] { field.Name, ((SaveToConfig)at).DefaultValue });                            
                            field.SetValue(this, val);
                        }
                    }
                }
            }
        }

        public void SaveConfig()
        {
            KSP.IO.PluginConfiguration config = KSP.IO.PluginConfiguration.CreateForType<PartCatalog>();

            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public))
            {
                foreach (Attribute at in field.GetCustomAttributes(false))
                {
                    if (at is SaveToConfig)
                    {
                        config.SetValue(field.Name, field.GetValue(this));
                    }
                }
            }
            config.save();
        }

        public void DefaultConfig()
        {

            TagButtonTextAlignment = TextAnchor.MiddleLeft;
            SortNumber = 3;
            DisplayedPage = 0;
            VerticalLayout = false;
            InvertFilter = false;
            SynchronizePartCategories = false;

        }

        [SaveToConfig((int)TextAnchor.MiddleLeft)]
        private int tagButtonTextAlignment;

        public TextAnchor TagButtonTextAlignment
        {
            get { return (TextAnchor)tagButtonTextAlignment; }
            set { tagButtonTextAlignment = (int)value; }
        }

        [SaveToConfig(3)]
        public int SortNumber;

        [SaveToConfig(0)]
        public int DisplayedPage;

        [SaveToConfig(false)]
        public bool VerticalLayout;

        [SaveToConfig(false)]
        public bool InvertFilter;

        [SaveToConfig(true)]
        public bool UnionFilter;

        [SaveToConfig(false)]
        public bool SynchronizePartCategories;

        [SaveToConfig(5)]
        public int TagMoveMultiplier;

        [SaveToConfig(1)]
        public int MouseWheelPrescaler;

        [SaveToConfig(true)]
        public bool EnableCategoryScrolling;

        [SaveToConfig(true)]
        public bool EnablePartListScrolling;

        [SaveToConfig(true)]
        public bool EnableShortcuts;

        [SaveToConfig((int)ToolBarPositions.VerticalLeft)]
        private int toolBarPreset;

        public ToolBarPositions ToolBarPreset
        {
            get { return (ToolBarPositions)toolBarPreset; }
            set { toolBarPreset = (int)value; }
        }

        /*
        [SaveToConfig(typeof(Vector2))]
        public Vector2 ToolBarStart;

        [SaveToConfig(typeof(Vector2))]
        public Vector2 ToolBarSize;
        */
        
        [SaveToConfig(true)]
        public bool FirstRun;

        [SaveToConfig(false)]
        public bool AutoHideToolBar;

        [SaveToConfig((int)ConfigButtonPositions.TopLeft)]
        private int configButtonPreset;

        public ConfigButtonPositions ConfigButtonPreset
        {
            get { return (ConfigButtonPositions)configButtonPreset; }
            set { configButtonPreset = (int)value; }
        }

        [SaveToConfig((int)ToolBarDirections.Down)]
        private int toolBarDirection;

        public ToolBarDirections ToolBarDirection
        {
            get { return (ToolBarDirections)toolBarDirection; }
            set { toolBarDirection = (int)value; }
        }

        [SaveToConfig(typeof(Vector2))]
        public Vector2 ConfigButtonPosition;

        [SaveToConfig(true)]
        public bool PageNumberOnToolbarEnd;

        [SaveToConfig(true)]
        public bool SearchPartInSubtags;

        [SaveToConfig(typeof(Vector2))]
        public Vector2 ButtonSize;

        [SaveToConfig(50)]
        public int ToolbarShiftSpeed;

        [SaveToConfig(0)]
        public int SmallModTagPartCount;

        [SaveToConfig(true)]
        public bool HelpActive;

        [SaveToConfig(false)]
        public bool DisplayAllOnEmptyFilter;

        [SaveToConfig(true)]
        public bool AutotagOnlyUntagged;

        [SaveToConfig(true)]
        public bool HideUnresearchedTags;

        [SaveToConfig(true)]
        public bool HideEmptyCategories;

        [SaveToConfig(408321)]
        public int WindowIndexOffset;

        [SaveToConfig(true)]
        public bool UseDynamicRules;

        [SaveToConfig(2048)]
        public int PartSerializationBufferSize;

        [SaveToConfig(50f)]
        public float MouseOverStopDelay;

        [SaveToConfig(10f)]
        public float MouseOverStartDelay;

        [SaveToConfig(true)]
        public bool SearchNames;
        [SaveToConfig(true)]
        public bool SearchTitles;
        [SaveToConfig(true)]
        public bool SearchDescription;
        [SaveToConfig(true)]
        public bool SearchTags;

        public int LayoutWindow
        {
            get
            {
                return WindowIndexOffset + 0;
            }
        }
        public int TagEditorWindow
        {
            get
            {
                return LayoutWindow + 1;
            }
        }
        public int MouseOverWindow
        {
            get
            {
                return TagEditorWindow + 1;
            }
        }
        public int SearchWindow
        {
            get
            {
                return MouseOverWindow + 1;
            }
        }

        public void Dispose()
        {
            this.SaveConfig();
        }
    }


    [AttributeUsage(AttributeTargets.Field)]
    class SaveToConfig : System.Attribute
    {
        public object DefaultValue;
        public SaveToConfig(object defaultVal = null)
        {
            if (defaultVal != null)
            {
                if (defaultVal is Type)
                {
                    DefaultValue = Activator.CreateInstance((Type)defaultVal);
                }
                else
                {
                    DefaultValue = defaultVal;
                }
            }
            
        }
    }
}


