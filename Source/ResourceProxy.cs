using System;
using System.Collections.Generic;
using System.Text;
using KSP.IO;
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
            public Texture2D GetTexture(bool on) { return on ? _enabled : _disabled; }
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
            Texture2D toReturn = new Texture2D((int)ConfigHandler.Instance.ButtonSize.x,(int)ConfigHandler.Instance.ButtonSize.y,TextureFormat.ARGB32,false);
            toReturn.LoadImage(KSP.IO.File.ReadAllBytes<PartCatalog>(name));
            return toReturn;
        }

        public bool BufferTexture(string name)
        {
            Texture2D Disabled = null;
            Texture2D Enabled = null;
            if (KSP.IO.File.Exists<PartCatalog>( name + "_Off.png" ))
            {
                Disabled = LoadTextureRaw(name + "_Off.png");
            }
            if (KSP.IO.File.Exists<PartCatalog>(name + "_On.png"))
            {
                Enabled = LoadTextureRaw(name + "_On.png");    
            }
            if (Enabled == null || Disabled == null)
            {                                         
                return false;
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

        public void SaveIconList()
        {
            /*
            TextWriter file = TextWriter.CreateForType<PartCatalog>("icons.txt");
            foreach (string iconName in LoadedTextures.Keys)
            {
                file.WriteLine(iconName);
            }
            file.Flush();
            file.Close();
             */
        }
                                        
        public void LoadIconList()
        {
            List<string> files = DirectoryLister.Instance.ListFiles(GUIConstants.CatalogDataPath);
            foreach (var file in files)
            {
                if (file.EndsWith("_On.png"))
                {
                    BufferTexture(file.Substring(0, file.Length - "_On.png".Length));
                }
            }
            /*
            if (File.Exists<PartCatalog>("icons.txt"))
            {
                TextReader file = TextReader.CreateForType<PartCatalog>("icons.txt");
                while (!file.EndOfStream)
                {
                    BufferTexture(file.ReadLine());
                }
            }       */
        }
    }

    internal class DirectoryLister : FileBrowser
    {

        static DirectoryLister instance = new DirectoryLister();
        public static DirectoryLister Instance
        {
            get
            {
                return instance;
            }
        }

        private DirectoryLister() : base(new Rect(),"",new FinishedCallback(Finished))
        {            
        }
        private static void Finished(string result)
        {
        }

        public List<string> ListFiles(string directory)
        {
            this.BrowserType = FileBrowserType.File;
            SetNewDirectory(directory);
            SwitchDirectoryNow();
            return new List<string>(this.m_files);
        }

        public List<string> ListDirectories(string directory)
        {
            this.BrowserType = FileBrowserType.Directory;
            SetNewDirectory(directory);
            SwitchDirectoryNow();
            return new List<string>(this.m_directories);
        }
    }
}
