using System;
using System.Threading.Tasks;

using usb_i2c_board;

namespace Sample3
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

            if (!SC18IM700.AllPortModeWrite(0b00000000))
            {
                Console.WriteLine("Port Setting Failed");
                SC18IM700.Dispose();
                return;
            }

            var a = (byte)0b00000001;

            while(true)
            {
                for(var i = 0; i < 7; i++)
                {
                    SC18IM700.AllPortWrite(a);
                    Task.Delay(100).Wait();
                    a <<= 1;
                }

                for (var i = 0; i < 7; i++)
                {
                    SC18IM700.AllPortWrite(a);
                    Task.Delay(100).Wait();
                    a >>= 1;
                }
            }
        }
    }
}
