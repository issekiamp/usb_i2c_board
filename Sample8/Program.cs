using System;
using System.Text;
using System.Threading.Tasks;

using usb_i2c_board;

namespace Sample8
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = "COM7";
            var temp1 = new ADT7410(port, 0x48);
            var lcd1 = new AQM1602(port, 0x3e);

            lcd1.LcdInit();

            while (true)
            {
                lcd1.LcdClear();
                var msg1 = DateTime.Now.ToString("HH:mm:ss");
                lcd1.MessageWrite(Encoding.ASCII.GetBytes(msg1));

                lcd1.LcdNewLine();
                var t = temp1.TempRead();
                var msg2 = t.ToString("F1");
                lcd1.MessageWrite(Encoding.ASCII.GetBytes(msg2));

                Task.Delay(3000).Wait();
            }
        }
    }
}
