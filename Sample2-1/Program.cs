using System;
using System.Threading.Tasks;

using usb_i2c_board;

namespace Sample1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!SC18IM700.Open("COM7"))
            {
                Console.WriteLine("COM Port Open Failed");
                return;
            }

            if (!SC18IM700.PortModeWrite(0, SC18IM700GpioMode.Output))
            {
                Console.WriteLine("Port Setting Failed");
                SC18IM700.Dispose();
                return;
            }

            while (true)
            {
                SC18IM700.PortWrite(0, SC18IM700GpioState.High);
                Task.Delay(500).Wait();
                SC18IM700.PortWrite(0, SC18IM700GpioState.Low);
                Task.Delay(500).Wait();
            }
        }
    }
}
