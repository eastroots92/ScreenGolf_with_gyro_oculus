using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;

public class EbmDataParser : MonoBehaviour
{
    public static EbmDataParser instance;

    public Quaternion QuatData;
    public Vector3 DistData;
    public Vector3 AccData;

    public String OriginalData;

    public Vector3 prevEuler;
    
    Quaternion quat;
    public Vector3 euler;
    public Vector3 dist;
    public Vector3 acc;

    public float differZAngle = 0.0f;

    public float shootPower = 0.0f;

    public bool connectCheck = false;



    Thread thread1;
    public SerialPort EbmPort = new SerialPort("COM8", 57600, Parity.None, 8, StopBits.One);

    public Boolean resetflag = false;
    public int frame = 0;
    //public List<Vector3> swingPosList = new List<Vector3>();
    //public LineRenderer lineRenderer = null;

    //public Transform swingPosDummy = null;
      
    void Awake()
    {
        if (EbmDataParser.instance == null)
        {
            EbmDataParser.instance = this;
        }
    }

    void Start()
    {
        //EbmPort.Open();
        //EbmPort.ReadTimeout = 3000;

        //thread1 = new Thread(SerialDataProc);
        //thread1.Start();

    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S) == true)
        //{
        //    lineRenderer.SetVertexCount(swingPosList.Count);
        //    for (int i = 0; i < swingPosList.Count; ++i)
        //    {
        //        lineRenderer.SetPosition(i, swingPosList[i]);
        //    }
        //}
    }

    public void OpenSerial(int portNmber)
    {
        String inputPortName = "COM" + portNmber;
        EbmPort.PortName = inputPortName;
        EbmPort.Open();
        EbmPort.ReadTimeout = 3000;

        thread1 = new Thread(SerialDataProc);
        thread1.Start();
    }

    public void CloseSerial()
    {
        EbmPort.Close();
        thread1.Abort();
    }

    void OnApplicationQuit()
    {
        EbmPort.Close();
        thread1.Abort();
    }


    void SerialDataProc()
    {

        string sdata = "";
        string[] item;
        int item_counter = 0;


        while (true)
        {
            Thread.Sleep(1);  // 1ms

            connectCheck = EbmPort.IsOpen;

            if (EbmPort.IsOpen != true) break;

            try { sdata = EbmPort.ReadLine(); }
            catch { continue; }

            OriginalData = sdata;

            item = sdata.Split(',');

            item[0] = item[0].Replace("*", "");

            item_counter = 0;

            if (item.Length >= item_counter + 3)  // z,x,y
            {
                euler.z = float.Parse(item[item_counter++]);
                euler.x = float.Parse(item[item_counter++]);
                euler.y = float.Parse(item[item_counter++]);
                quat.eulerAngles = euler;

                if (frame > 0)
                {
                    float tempDifferAngle = Mathf.Abs(prevEuler.z - euler.z);
                    if (tempDifferAngle > differZAngle)
                        differZAngle = tempDifferAngle;

                    //if (differZAngle > 5.0f)
                    //    swingPosList.Add(swingPosDummy.position);
                }
                //print(" euler.x : " + euler.x + " euler.y : " + euler.y + " euler.z : " + euler.z);
            }
            QuatData = quat;

            if (item.Length >= item_counter + 3)   // distance
            {
                acc.z = float.Parse(item[item_counter++]) ;
                acc.x = float.Parse(item[item_counter++]) ;
                acc.y = float.Parse(item[item_counter++]) ;
                //print(" dist.x : " + dist.x + " dist.y : " + dist.y + " dist.z : " + dist.z);
                if (acc.x > shootPower)
                    shootPower = acc.x;
            }
            AccData = acc;

            if (item.Length >= item_counter + 3)   // distance
            {
                dist.z = float.Parse(item[item_counter++]) * 0.1f;
                dist.x = float.Parse(item[item_counter++]) * 0.1f;
                dist.y = float.Parse(item[item_counter++]) * 0.1f;
                //print(" dist.x : " + dist.x + " dist.y : " + dist.y + " dist.z : " + dist.z);
            }
            DistData = dist;

            ++frame;

            prevEuler = euler;

            if (resetflag == true)
            {
                frame = 0;
                //differZAngle = 0.0f;
                shootPower = 0.0f;
                //swingPosList.Clear();
                //EbmPort.WriteLine("<posz>");
                EbmPort.WriteLine("/");
                resetflag = false;
                for (int n = 0; n < 5; n++)
                {
                    EbmPort.ReadLine();
                }

                //EbmPort.WriteLine("/");
                //EbmPort.WriteLine("/");
            }
        }
    }

    
}



