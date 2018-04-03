using System;

namespace usb_i2c_board
{
    public class MCP23017
    {
        private byte address;

        public MCP23017(string port, byte adr)
        {
            if (!SC18IM700.Open(port))
            {
                Console.WriteLine("COM Port Open Failed");
                return;
            }
            address = adr;
        }

        public bool RegWrite(byte reg, byte val)
        {
            return SC18IM700.I2cWrite(address, new Byte[] { reg, val });
        }

        public byte[] RegRead(byte reg)
        {
            SC18IM700.I2cRead(address, reg);
            return SC18IM700.I2cRead(address);
        }
    }
}
