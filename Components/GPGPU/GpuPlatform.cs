using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cloo;

namespace Components.GPGPU
{
    class GpuPlatform
    {
        #region StatusMessage
        private void StringAdd(object newline, int tabcount, ref string source)
        {
            while (tabcount > 0)
            {
                newline = "\t" + newline;
                tabcount--;
            }
            source += newline + "\n";
        }
        private void StringAdd(string tag, object value, int tabcount, ref string source)
        {
            string nl = string.Empty;
            while (tabcount > 0)
            {
                nl = "\t" + nl;
                tabcount--;
            }
            nl += tag + " : " + value;
            source += nl + "\n";
        }
        public string Status
        {
            get
            {
                string status = string.Empty;
                StringAdd(Platform.Name, 0, ref status);
                StringAdd(Platform.Version, 0, ref status);
                StringAdd(Platform.Profile, 0, ref status);
                for (int i = 0; i < CommandQueueList.Count; i++)
                {
                    ComputeDevice device = CommandQueueList[i].Device;
                    StringAdd("GPUDevice " + i + ":", 0, ref status);
                    StringAdd("Name", device.Name, 1, ref status);
                    StringAdd("Type", device.Type, 1, ref status);
                    StringAdd("DriverVersion", device.DriverVersion, 1, ref status);
                    StringAdd("Profile", device.Profile, 1, ref status);
                    StringAdd("MaxWorkGroupSize", device.MaxWorkGroupSize, 1, ref status);
                    StringAdd("MaxParameterSize", device.MaxParameterSize, 1, ref status);
                    StringAdd("MaxWorkItemDimensions", device.MaxWorkItemDimensions, 1, ref status);
                }
                return status;
            }
        }
        #endregion

        #region Property
        private bool HasGpu { get; set; }
        private int PrevCommandQueueIndex = 0;
        private ComputeCommandQueue CommandQueueSelector
        {
            get
            {
                PrevCommandQueueIndex++;
                int index;
                if (PrevCommandQueueIndex >= CommandQueueList.Count)
                { index = 0; PrevCommandQueueIndex = 0; }
                else { index = PrevCommandQueueIndex; }
                return CommandQueueList[index];
            }
        }

        public ComputePlatform Platform { get; private set; }
        public ComputeContextPropertyList Properties { get; private set; }
        public ComputeContext Context { get; private set; }
        public List<ComputeCommandQueue> CommandQueueList { get; private set; }
        #endregion

        #region InnerClass
        public class ProgramKernel
        {
            public string Name { get; private set; }
            public ComputeProgram Program { get; private set; }
            public ProgramKernel(string name, ComputeProgram program)
            {
                Name = name;
                Program = program;
            }

            public class OptionSet
            {
                public ComputeContext Context { get; private set; }
                public ComputeKernel Kernel { get; private set; }
                public ComputeCommandQueue Queue { get; private set; }

                public OptionSet(ComputeContext context, ComputeKernel kernel, ComputeCommandQueue queue)
                {
                    Context = context;
                    Kernel = kernel;
                    Queue = queue;
                }
            }

            public OptionSet CreateKernel(ComputeContext context, ComputeCommandQueue queue)
            {
                return new OptionSet(context, Program.CreateKernel(Name), queue);
            }
        }
        private List<ProgramKernel> Programs { get; set; }
        public int ProgramCount { get { return Programs.Count; } }
        #endregion

        private List<string> typeList = new List<string>()
            {
                "nvidia", "amd",
            };
        private bool isGpuProcessor(string versionString)
        {
            bool ret = false;
            versionString = versionString.ToLower();
            foreach (var item in typeList)
            {
                if (versionString.Contains(item))
                { ret = true; break; }
            }
            return ret;
        }

        private GpuPlatform(ComputePlatform platform)
        {
            Platform = platform;
            if (isGpuProcessor(Platform.Name))
            {
                foreach (var device in platform.Devices)
                {
                    if (device.Type == ComputeDeviceTypes.Gpu) { HasGpu = true; }
                }
                if (HasGpu)
                {
                    Properties = new ComputeContextPropertyList(platform);
                    Context = new ComputeContext(platform.Devices, Properties, null, IntPtr.Zero);
                    CommandQueueList = new List<ComputeCommandQueue>();
                    for (int i = 0; i < platform.Devices.Count; i++)
                    {
                        if (platform.Devices[i].Type == ComputeDeviceTypes.Gpu)
                        {
                            CommandQueueList.Add(new ComputeCommandQueue(Context, platform.Devices[i], ComputeCommandQueueFlags.None));
                        }
                    }
                    Programs = new List<ProgramKernel>();
                }
            }
        }

        public static GpuPlatform IsGpuProcesser(ComputePlatform platform)
        {
            GpuPlatform item = new GpuPlatform(platform);
            if (item.HasGpu)
            {
                return item;
            }
            return null;
        }

        public void Trush()
        {
            foreach (var item in Programs)
            {
                item.Program.Dispose();
            }
            foreach (var item in CommandQueueList)
            {
                item.Dispose();
            }
            Context.Dispose();
        }

        public bool Exists(string name)
        {
            if (Programs.FindAll(x => x.Name == name).Count > 0) { return true; }
            return false;
        }

        public bool CreateProgram(string name, string source)
        {
            try
            {
                ComputeProgram program = new ComputeProgram(Context, source);
                program.Build(Platform.Devices, null, null, IntPtr.Zero);
                Programs.Add(new ProgramKernel(name, program));
                Terminal.WriteLine(State.EventState.Log, "\"{0}\" ProgramKernel Created.", name);
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public ProgramKernel.OptionSet CreateKernel(string name)
        {
            return Programs.FindAll(x => x.Name == name)[0].CreateKernel(Context, CommandQueueSelector);
        }

    }
}
