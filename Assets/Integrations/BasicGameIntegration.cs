using UnityEngine;
using System.IO;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System;
using System.Collections;

public class BasicGameIntegration : MonoBehaviour
{
    public GameObject Player;

    private Vector3 pos;
    private Vector3 euler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Thread t2;

    NamedPipeServerStream serverStream;

    IEnumerator Loop()
    {
        while(true)
        {
            yield return new WaitForSeconds(2);
            print("hello");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        StartCoroutine("Loop");
        Player = (GameObject)Instantiate(Resources.Load("pac_character"));
        // Thread clientWriteThread = new Thread(WriteThread);
        // clientWriteThread.Start();

        Thread serverReadThread = new Thread(ReadThread);
        serverReadThread.Start();

        //t1 = clientWriteThread;
        t2 = serverReadThread;
    }

    public void Update()
    {
        Player.transform.position = pos;
        Player.transform.eulerAngles = euler;
    }

    private void WriteThread()
    {
        NamedPipeClientStream clientStream = new NamedPipeClientStream(".", "IT_ClientWrite", PipeDirection.Out);
        clientStream.Connect();

        while (true)
        {
            System.Random rand = new System.Random();
            Vector3 rnd = new Vector3(rand.Next(0, 1000), rand.Next(0, 1000), rand.Next(0, 1000));


            BinaryWriter br = new BinaryWriter(clientStream);
            br.Write(rnd.x);
            br.Write(rnd.y);
            br.Write(rnd.z);
            br.Write(0);
            br.Flush();
            //new StreamString(clientStream).WriteString(new System.Random().Next(0, 1000).ToString());
        }

        clientStream.Close();
    }

    private void ReadThread()
    {
        serverStream = new NamedPipeServerStream("IT_ClientWrite", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None, 16, 16);
        serverStream.WaitForConnection();
        print("Connection established");

        BinaryReader reader = new BinaryReader(serverStream);

        while (true)
        {
            pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            uint yAngle = reader.ReadUInt32();

            euler = new Vector3(0, OERotationY.ToAngle((ushort)yAngle), 0);
            print("Got message " + pos);
        }

        serverStream.Close();
    }

    private void OnDestroy()
    {
        serverStream.Close();
        // t1.Abort();
        t2.Abort();
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
