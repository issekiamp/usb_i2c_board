using usb_i2c_board;

namespace Sample6
{
    class Program
    {
        static void Main(string[] args)
        {
            var exp1 = new MCP23017("COM7", 0x20);
            exp1.RegWrite(0x00, 0xff);
            exp1.RegWrite(0x01, 0x00);

            while (true)
            {
                var a = exp1.RegRead(0x12);
                exp1.RegWrite(0x13, a[0]);
            }
        }
    }
}
