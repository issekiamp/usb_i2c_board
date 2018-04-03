using System;
using System.Threading.Tasks;

using usb_i2c_board;

namespace Sample4
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

            if (!SC18IM700.AllPortModeWrite(0b11111111))
            {
                Console.WriteLine("Port Setting Failed");
                SC18IM700.Dispose();
                return;
            }

            while (true)
            {
                if (SC18IM700.TryAllPortRead(out byte data))
                {
                    var a = (byte)~data;
                    Console.WriteLine(format: "PORT={0}", arg0: a.ToString());
                }

                Task.Delay(1000).Wait();
            }
        }
    }
}
