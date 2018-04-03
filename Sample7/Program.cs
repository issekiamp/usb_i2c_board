using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using usb_i2c_board;

namespace Sample7
{
    class Program
    {
        static void Main(string[] args)
        {
            var lcd1 = new AQM1602("COM7", 0x3e);

            lcd1.LcdInit();

            while (true)
            {
                lcd1.LcdClear();
                lcd1.MessageWrite(Encoding.ASCII.GetBytes("AQM1602XA-RN-GBW"));
                lcd1.LcdNewLine();

                var msg1 = new List<byte>();

                for (var i = 0; i < 16; i++)
                {
                    msg1.Add((byte)(i + 0xb1));
                }
                lcd1.MessageWrite(msg1);
                Task.Delay(3000).Wait();

                lcd1.LcdClear();
                var temp = 30.52;
                var msg2 = "[Room Temp.=" + temp.ToString("F2") + "]";
                lcd1.MessageWrite(Encoding.ASCII.GetBytes(msg2));

                lcd1.LcdNewLine();

                var vol = 3.3;
                var msg3 = "[Voltage=" + vol.ToString("F2") + "]";
                lcd1.MessageWrite(Encoding.ASCII.GetBytes(msg3));
                Task.Delay(3000).Wait();
            }
        }
    }
}
