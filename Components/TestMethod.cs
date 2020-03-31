using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    static class TestMethod
    {
        public static void TestStart_ImageConvolusion()
        {
            Components.Imaging.Core.Instance.Initialize(State.CaptureMode.Device_Camera);

            (new System.Threading.Tasks.Task(() =>
            {
                Random rand = new Random();
                RNdMatrix inmat = new RNdMatrix(1, 3, 340, 240);
                RNdMatrix outmat = new RNdMatrix(1, 3, 340, 240);

                var unit = new GPGPU.Layer.Unit.Convolution.Convolution()
                {
                    Activation = true,
                    KernelSize = 5,
                    OutputChannels = 3,
                    Rho = 0.001,
                };
                unit.Initialize(inmat.Clone());
                unit.Confirm();

                int counter = 0;
                double time = 0;
                while (true)
                {
                    time = 0;
                    var input = Imaging.Core.Instance.GetFrame(inmat.Shape);
                    unit.Input = input.Clone();
                    time += unit.Action(State.ActionMode.Forward);

                    var sigma = unit.Output - input;
                    unit.Sigma = sigma;
                    //time += unit.Action(ActionMode.Back);

                    unit.Output.Show("output", 0);
                    sigma.Show("sigma", 0);
                    Terminal.WriteLine(State.EventState.State, "Step:{0}, TimeSpan[ms]:{1}, FPS:{2}", counter++, (int)time, (int)(1000.0 / time));
                }
            })).Start();
        }

        public static void TestStart_SpikeNeuron()
        {

        }
    }
}
