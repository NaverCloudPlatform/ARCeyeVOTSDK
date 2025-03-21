using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace ARCeye.VOT
{
    public enum LogLevel
    {
        DEBUG, INFO, WARNING, ERROR, FATAL
    }

    class LogElem
    {
        public string str { get; set; }
        public LogLevel level { get; set; }
        public LogElem(LogLevel l, string s)
        {
            str = s;
            level = l;
        }
    }

    public class LogViewer : MonoBehaviour
    {
        static private LogLevel s_LogLevel = LogLevel.DEBUG;
        public LogLevel logLevel
        {
            get => s_LogLevel;
            set => s_LogLevel = value;
        }


        private void Awake()
        {
            NativeBridge.VOTSDK_SetDebugLogFuncNative(DebugLog);
        }

        private void OnDestroy()
        {
            NativeBridge.VOTSDK_ReleaseLoggerNative();
        }


        [MonoPInvokeCallback(typeof(NativeBridge.DebugLogFuncDelegate))]
        static public void DebugLog(LogLevel logLevel, IntPtr raw)
        {
            string log = Marshal.PtrToStringAnsi(raw);
            DebugLog(logLevel, log);
        }

        static public void DebugLog(LogLevel logLevel, string log)
        {
            if (logLevel < s_LogLevel)
            {
                return;
            }

            string currTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string msg = string.Format("[{0}] {1}", currTime, log);
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    {
                        msg = "[Debug] " + msg;
                        Debug.Log(msg);
                        break;
                    }
                case LogLevel.INFO:
                    {
                        msg = "[Info] " + msg;
                        Debug.Log(msg);
                        break;
                    }
                case LogLevel.WARNING:
                    {
                        msg = "[Warning] " + msg;
                        Debug.LogWarning(msg);
                        break;
                    }
                case LogLevel.ERROR:
                    {
                        msg = "[Error] " + msg;
                        Debug.LogError(msg);
                        break;
                    }
                case LogLevel.FATAL:
                    {
                        msg = "[Fatal] " + msg;
                        Debug.LogError(msg);
                        break;
                    }
            }
        }
    }
}