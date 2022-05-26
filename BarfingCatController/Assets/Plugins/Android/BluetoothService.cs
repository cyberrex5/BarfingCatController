using System;
using UnityEngine;

namespace AndroidBluetooth
{
    public static class BluetoothService
    {
        private static AndroidJavaClass unityPlayer;
        private static AndroidJavaObject activity;
        private static AndroidJavaObject context;
        private static AndroidJavaClass unity3dbluetoothplugin;
        private static AndroidJavaObject BluetoothConnector;

        /// <summary>
        /// creating an instance of the bluetooth class from the plugin 
        /// </summary>
        public static void CreateBluetoothObject()
        {
            //if (Application.platform != RuntimePlatform.Android) return;

            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity.Call<AndroidJavaObject>("getApplicationContext");
            unity3dbluetoothplugin = new AndroidJavaClass("com.example.unity3dbluetoothplugin.BluetoothConnector");
            BluetoothConnector = unity3dbluetoothplugin.CallStatic<AndroidJavaObject>("getInstance");
        }

        /// <summary>
        /// starting bluetooth connection with device named "DeviceName"<br/>
        /// print the status on the screen using native android Toast
        /// </summary>
        public static bool StartBluetoothConnection(string DeviceName)
        {
            //if (Application.platform != RuntimePlatform.Android) return false;

            string connectionStatus = "";
            try
            {
                connectionStatus = BluetoothConnector.Call<string>("StartBluetoothConnection", DeviceName);
                BluetoothConnector.Call("PrintOnScreen", context, connectionStatus);
                return connectionStatus == "Connected";
            }
            catch (Exception)
            {
                BluetoothConnector.Call("PrintOnScreen", context, connectionStatus);
                return false;
            }
        }

        /// <summary>
        /// should be called inside OnApplicationQuit<br/>
        /// stop connection with the bluetooth device
        /// </summary>
        public static void StopBluetoothConnection()
        {
            //if (Application.platform != RuntimePlatform.Android) return;

            try
            {
                BluetoothConnector.Call("StopBluetoothConnection");
                BluetoothConnector.Call("PrintOnScreen", context, "connction stopped");
            }
            catch (Exception)
            {
                BluetoothConnector.Call("PrintOnScreen", context, "stop connection error");
            }
        }

        /// <summary>
        /// write data as a string to the bluetooth device
        /// </summary>
        public static void WritetoBluetooth(string data)
        {
            //if (Application.platform != RuntimePlatform.Android) return;

            try
            {
                BluetoothConnector.Call("WriteData", data);
            }
            catch (Exception)
            {
                BluetoothConnector.Call("PrintOnScreen", context, "write data error");
            }
        }

        /// <summary>
        /// read data from the bluetooth device<br/>
        /// if there is an error or there is no data coming, this method will return ""
        /// </summary>
        public static string ReadFromBluetooth()
        {
            //if (Application.platform != RuntimePlatform.Android) return "";

            try
            {
                return BluetoothConnector.Call<string>("ReadData");
            }
            catch (Exception)
            {
                BluetoothConnector.Call("PrintOnScreen", context, "read data error");
                return "";
            }
        }
    }
}