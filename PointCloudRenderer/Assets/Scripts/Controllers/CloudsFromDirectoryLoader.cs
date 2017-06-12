using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Controllers {
    public class CloudsFromDirectoryLoader : MonoBehaviour {

        public string path;
        public AbstractPointSetController pointset;

        
        void Start() {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (DirectoryInfo sub in dir.GetDirectories()) {
                GameObject go = new GameObject(sub.Name);
                DynamicLoaderController loader = go.AddComponent<DynamicLoaderController>();
                loader.cloudPath = sub.FullName;
                loader.setController = pointset;
                pointset.RegisterController(loader);
            }
        }
    }
}