using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public class PointCloudLoaderController : MonoBehaviour {

    public string CloudPath;

	// Use this for initialization
	void Start () {
        Thread thread = new Thread(new ThreadStart(LoadFile));
        thread.Start();
    }

    private void LoadFile()
    {
        if (!CloudPath.EndsWith("\\"))
        {
            CloudPath = CloudPath + "\\";
        }
        string jsonfile;
        using (StreamReader reader = new StreamReader(CloudPath + "cloud.js", Encoding.Default))
        {
            jsonfile = reader.ReadToEnd();
            reader.Close();
        }
        PointCloudMetaData meta = PointCloudMetaData.ReadFromJson(jsonfile);
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
