using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace usb_i2c_board
{
    public class AQM1602
    {
        private byte address;

        public AQM1602(string port, byte adr)
        {
            if (!SC18IM700.Open(port))
            {
                Console.WriteLine("COM Port Open Failed");
                return;
            }
            address = adr;
        }

        ~AQM1602()
        {
            SC18IM700.Close();
        }

        public void DataWrite(byte data)
        {
            SC18IM700.I2cWrite(address, new Byte[] { 0x40, data });
            Task.Delay(10).Wait();
        }

        public void CommandWrite(byte command)
        {
            SC18IM700.I2cWrite(address, new Byte[] { 0x00, command });
            Task.Delay(10).Wait();
        }

        public void MessageWrite(IEnumerable<byte> message)
        {
            foreach(var msg in message)
            {
                DataWrite(msg);
            }
        }

        public void LcdClear()
        {
            CommandWrite(0x01);
        }
	
        public void LcdNewLine()
        {
            CommandWrite(0xC0);
        }

        public void LcdInit()
        {
            Task.Delay(100).Wait();
            var initcmd = new Byte[] { 0x38, 0x39, 0x14, 0x73, 0x56, 0x6c, 0x38, 0x01, 0x0c };
            foreach(var cmd in initcmd)
            {
                CommandWrite(cmd);
                Task.Delay(20).Wait();
            }
        }
    }
}
