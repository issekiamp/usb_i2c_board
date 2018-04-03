using System;
using usb_i2c_board;

namespace Sample2
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

            if (!SC18IM700.PortModeWrite(0, SC18IM700GpioMode.Output) ||
                !SC18IM700.PortModeWrite(7, SC18IM700GpioMode.Input))
            {
                Console.WriteLine("Port Setting Failed");
                SC18IM700.Dispose();
                return;
            }

            while (true)
            {
                if(!SC18IM700.TryPortRead(7, out SC18IM700GpioState value)) { continue; }

                if (value == SC18IM700GpioState.Low) {
                    SC18IM700.PortWrite(0, SC18IM700GpioState.High);
                } else {
                    SC18IM700.PortWrite(0, SC18IM700GpioState.Low);
                }
            }
        }
    }
}
