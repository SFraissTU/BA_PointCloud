using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ObjectCreation {
    /* Stores unused GameObjects
     * 
     * The GameObjects given by this class consist of an empty MeshFilter and a MeshRenderer.
     * All Components of a GameObject are removed when recycling it
     */
    class GameObjectCache {
        
        private Queue<GameObject> objectQueue;

        public GameObjectCache() {
            objectQueue = new Queue<GameObject>();
        }

        //Returns true if an old object is reused
        public bool RequestGameObject(string name, out GameObject result) {
            GameObject gameObject;
            if (objectQueue.Count == 0) {
                gameObject = new GameObject(name);
                result = gameObject;
                return false;
            } else {
                gameObject = objectQueue.Dequeue();
                gameObject.name = name;
                gameObject.SetActive(true);
                result = gameObject;
                return true;
            }
        }

        public void RecycleGameObject(GameObject gameObject) {
            gameObject.SetActive(false);
            gameObject.name = "cached";
            objectQueue.Enqueue(gameObject);
        }
    }
}
