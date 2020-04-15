using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Components
{
    public static class State
    {
        #region EventMessage
        public enum EventState
        {
            State = 1,
            Log = 10,
            Option01 = 100,
        }

        public class DataEventArgs
        {
            public EventState Mode { get; set; }
            public int Step { get; set; }
            public object Object { get; set; }
        }

        public delegate void UpdateDataEvent(object sender, DataEventArgs e);
        private static UpdateDataEvent UpdateMessageDataEventHandler { get; set; }
        public static event UpdateDataEvent UpdateMessageData
        {
            add { UpdateMessageDataEventHandler += value; }
            remove { UpdateMessageDataEventHandler -= value; }
        }
        public static void SendMessage(EventState state, string message)
        {
            if (UpdateMessageDataEventHandler != null)
            {
                UpdateMessageDataEventHandler(null, new DataEventArgs() { Mode = state, Object = message });
            }
        }

        private static UpdateDataEvent UpdateObjectDataEventHandler { get; set; }
        public static event UpdateDataEvent UpdateObjectData
        {
            add { UpdateObjectDataEventHandler += value; }
            remove { UpdateObjectDataEventHandler -= value; }
        }
        public static void SendData(DataEventArgs e)
        {
            if (UpdateObjectDataEventHandler != null)
            {
                UpdateObjectDataEventHandler(null, e);
            }
        }
        #endregion

        #region GPGPU Define
        public enum MemoryModeSet
        {
            ReadOnly,
            WriteOnly,
            Parameter,
        }
        public enum ActionMode
        {
            Forward,
            Back,
        }
        #endregion

        #region Imaging Define
        public enum CaptureMode
        {
            Device_Camera,
        }
        #endregion

        #region Random
        private static Random random = new Random();
        private static object _____randlock = new object();
        public static Random RandomSource { get { lock (_____randlock) { return random; } } }
        public static double GetRandomValue(double ave = 0, double sigma = 1, double edging = 1)
        {
            double x, y;
            lock (_____randlock)
            {
                x = random.NextDouble();
                y = random.NextDouble();
            }
            return sigma * Math.Pow(Math.Sqrt(-2.0 * Math.Log(x)) * Math.Cos(2.0 * Math.PI * y), edging) + ave;
        }
        #endregion

        public delegate void CatchExceptionEventHandler(Exception ex);
        private static CatchExceptionEventHandler CatchExceptionHandler { get; set; }
        public static event CatchExceptionEventHandler ExceptionCatched
        {
            add { CatchExceptionHandler += value; }
            remove { CatchExceptionHandler -= value; }
        }

        public static void ExceptionState(Exception ex)
        {
            if (ex is Cloo.BuildProgramFailureComputeException)
            {

            }
            if (CatchExceptionHandler != null)
            {
                CatchExceptionHandler?.Invoke(ex);
            }
            else
            {
                throw ex;
            }
        }

        public static void SetAssembly(Assembly targetasm)
        {
            GPGPU.Core.SetAssembly(targetasm);
        }

        public static void AddSourceGroup(string nameSpace)
        {
            GPGPU.Core.AddNameSpace(nameSpace);
        }
        public static void AddSharedSourceGroup(string nameSpace)
        {
            GPGPU.Core.AddSharedNameSpace(nameSpace);
        }

        public static bool Initialize()
        {
            GPGPU_Startup();
            return GPGPU.Core.Instance.UseGPU;
        }

        private static void GPGPU_Startup()
        {
            GPGPU.Core.Instance.UseGPU = true;
            Terminal.WriteLine(EventState.State, GPGPU.Core.Instance.ProcesserStatus);

            GPGPU.Core.Instance.BuildAllMethod();
        }
    }
}
