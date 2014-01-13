using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace PartCatalog
{
    class ResourceProxy
    {
        public class BufferedTexture
        {
            public BufferedTexture(Texture2D enabled, Texture2D disabled) { _enabled = enabled; _disabled = disabled; }
            readonly Texture2D _enabled;
            readonly Texture2D _disabled;
            public bool IsToggle
            {
                get
                {
                    return _disabled != null;
                }
            }
            public Texture2D GetTexture(bool on) 
            {
                if (_disabled != null)
                {
                    return on ? _enabled : _disabled;
                }
                return _enabled;
            }
        }

        static readonly ResourceProxy instance = new ResourceProxy();

        public static ResourceProxy Instance
        {
            get { return ResourceProxy.instance; }
        }

        private ResourceProxy()
        {
            LoadIconList();
        }

        public SortedDictionary<string, BufferedTexture> LoadedTextures = new SortedDictionary<string, BufferedTexture>();

        private Texture2D LoadTextureRaw(string name)
        {
            Texture2D toReturn = new Texture2D((int)ConfigHandler.Instance.ButtonSize.x, (int)ConfigHandler.Instance.ButtonSize.y, TextureFormat.ARGB32, false);
            toReturn.LoadImage(System.IO.File.ReadAllBytes(name));
            return toReturn;
        }

        public bool BufferTexture(string name)
        {
            Texture2D Disabled = null;
            Texture2D Enabled = null;
            string path = GUIConstants.IconFolderPath + "/" + name;
            string offFile = path + "_off.png";
            string onFile = path + "_on.png";
            string genericFile = path + ".png";
            if (System.IO.File.Exists(offFile))
            {
                Disabled = LoadTextureRaw(offFile);
            }
            if (System.IO.File.Exists(onFile))
            {
                Enabled = LoadTextureRaw(onFile);
            }
            if (Enabled == null || Disabled == null)
            {

                if (System.IO.File.Exists(genericFile))
                {
                    Enabled = LoadTextureRaw(genericFile);
                    Disabled = null;
                }
                else
                {
                    return false;
                }
            }
            BufferedTexture newBuffered = new BufferedTexture(Enabled, Disabled);
            LoadedTextures[name] = newBuffered;
            return true;
        }

        public Texture2D GetIconTexture(string name, bool enabled, bool tryDefaultAfterwards = true)
        {
            if (LoadedTextures.ContainsKey(name) || BufferTexture(name))
            {
                return LoadedTextures[name].GetTexture(enabled);
            }
            if (tryDefaultAfterwards)
            {
                return GetIconTexture(GUIConstants.ErrorIconName, enabled, false);
            }
            Debug.LogError("Couldn't load error Icon");
            return null;
        }

        public Texture2D GetTagIcon(PartTag tag)
        {
            if (string.IsNullOrEmpty(tag.IconName))
            {
                return GetIconTexture(GUIConstants.DefaultIconName, tag.Enabled);
            }
            return GetIconTexture(tag.IconName, tag.Enabled);
        }

        public bool IconExists(string name)
        {
            return LoadedTextures.ContainsKey(name) || BufferTexture(name);
        }

        public void LoadIconList()
        {
            List<string> files = DirectoryLister.Instance.ListFiles(GUIConstants.IconFolderPath);
            foreach (var file in files)
            {                
                if (file.EndsWith(".png",StringComparison.OrdinalIgnoreCase))
                {
                    if (file.EndsWith("_on.png", StringComparison.OrdinalIgnoreCase))
                    {
                        BufferTexture(file.Substring(GUIConstants.IconFolderPath.Length + 1, file.Length - GUIConstants.IconFolderPath.Length - 1 -  "_on.png".Length));
                    }
                    else if (file.EndsWith("_off.png", StringComparison.OrdinalIgnoreCase))
                    {
                        BufferTexture(file.Substring(GUIConstants.IconFolderPath.Length + 1, file.Length - GUIConstants.IconFolderPath.Length - 1 - "_off.png".Length));
                    } else
                    {
                        BufferTexture(file.Substring(GUIConstants.IconFolderPath.Length + 1, file.Length - GUIConstants.IconFolderPath.Length - 1 - ".png".Length));
                    }
                }
            }
        }

        public void Reload()
        {
            LoadedTextures.Clear();
            LoadIconList();
        }
    }

    internal class DirectoryLister
    {

        static DirectoryLister instance = new DirectoryLister();
        public static DirectoryLister Instance
        {
            get
            {
                return instance;
            }
        }


        public List<string> ListFiles(string directory)
        {
            List<string> toReturn = new List<string>(Directory.GetFiles(directory));
            
            foreach (var dir in Directory.GetDirectories(directory))
            {
                toReturn.AddRange(ListFiles(dir));
            }
            return toReturn;
        }

        public List<string> ListDirectories(string directory)
        {
            List<string> toReturn = new List<string>();

            return toReturn;
        }
    }
}
