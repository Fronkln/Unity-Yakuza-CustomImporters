using UnityEngine;
using System.IO;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameIntegration2 : MonoBehaviour
{
    public GameObject Player;
    public Camera Camera;

    private GameObject m_modelResource;
    public Dictionary<int, GameObject> m_models = new Dictionary<int, GameObject>();
    private List<int> m_modelCreationQueue = new List<int>();

    private Dictionary<int, Vector3> pos = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> fpos = new Dictionary<int, Vector3>();

    private Vector3 cameraPos;
    private Vector3 cameraLookPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Thread t2;

    NamedPipeServerStream serverStream;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        m_modelResource = Resources.Load<GameObject>("pac_character");
        // Thread clientWriteThread = new Thread(WriteThread);
        // clientWriteThread.Start();

        Thread serverReadThread = new Thread(ReadThread);
        serverReadThread.Start();

        //t1 = clientWriteThread;
        t2 = serverReadThread;
    }

    public void Update()
    {
        //Player.transform.position = pos;
       // Player.transform.eulerAngles = euler;

        var list = new List<int>(m_modelCreationQueue);

        foreach(int uid in list)
        {
            m_models[uid] = Instantiate(m_modelResource);
        }

        m_modelCreationQueue.Clear();

        foreach(var kv in m_models)
        {
            if(pos.ContainsKey(kv.Key))
            {
                kv.Value.transform.position = pos[kv.Key];
                kv.Value.transform.forward = fpos[kv.Key];
            }
        }

        Camera.transform.position = cameraPos;
        Camera.transform.LookAt(cameraLookPos);

        var matrix= Camera.transform.localToWorldMatrix;
        Vector3 forward = new Vector3(matrix.m02, matrix.m12, matrix.m22);
        print(forward);
    }

    private void ReadThread()
    {
        serverStream = new NamedPipeServerStream("IT_ClientWrite", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None, 256, 256);
        serverStream.WaitForConnection();
        print("Connection established");

        BinaryReader reader = new BinaryReader(serverStream);

        while (true)
        {
            try
            {
                cameraPos = new PXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                cameraLookPos = new PXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                int fighterCount = reader.ReadInt32();

                for (int i = 0; i < fighterCount; i++)
                {
                    int uid = reader.ReadInt32();
                    uid = i;
                    Vector3 pos = new PXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 fpos = new PXDVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    if (!m_models.ContainsKey(uid))
                    {
                        if (!m_modelCreationQueue.Contains(uid))
                            m_modelCreationQueue.Add(uid);
                    }
                    else
                    {
                        this.pos[uid] = pos;
                        this.fpos[uid] = fpos;
                    }
                }
            }
            catch
            {
                print("fail");
            }
        }

        serverStream.Close();
    }

    private void OnDestroy()
    {
        serverStream.Close();
        // t1.Abort();
        t2.Abort();

        serverStream = null;
        t2 = null;
    }

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
