using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BAPointCloudRenderer.Controllers
{
    /// <summary>
    /// Used internally for Previewing. Please don't attach.
    /// </summary>
    class PreviewObject : MonoBehaviour
    {
        public void Start()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
