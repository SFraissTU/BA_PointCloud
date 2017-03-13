#if UNITY_STANDALONE
#define IMPORT_GLENABLE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

/* This should enable setting the pointsize in OpenGL. Unfortunately it doesn't seem to work.
 * Source: http://www.kamend.com/2014/05/rendering-a-point-cloud-inside-unity/
 */
public class PointInitializer : MonoBehaviour {

    const UInt32 GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642;
    const UInt32 GL_POINT_SMOOTH = 0x0B10;

    const string LibGLPath =
        #if UNITY_STANDALONE_WIN
            "opengl32.dll";
#elif UNITY_STANDALONE_OSX
            "/System/Library/Frameworks/OpenGL.framework/OpenGL";
#elif UNITY_STANDALONE_LINUX
            "libGL";  // Untested on Linux, this may not be correct
#else
            null;   // OpenGL ES platforms don't require this feature
#endif

#if IMPORT_GLENABLE
    [DllImport(LibGLPath)]
    public static extern void glEnable(UInt32 cap);

    private bool mIsOpenGL;

    void Start()
    {
        Debug.Log(SystemInfo.graphicsDeviceVersion);
        mIsOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
    }

    private void OnPreRender()
    {
        if (mIsOpenGL)
        {
            glEnable(GL_VERTEX_PROGRAM_POINT_SIZE);
            glEnable(GL_POINT_SMOOTH);
        }
    }
#endif
}
