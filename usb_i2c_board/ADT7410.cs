using System;
using System.Linq;

namespace usb_i2c_board
{
    public class ADT7410
    {
        private byte address;

        public ADT7410(string port, byte adr)
        {
            if (!SC18IM700.Open(port))
            {
                Console.WriteLine("COM Port Open Failed");
                return;
            }
            address = adr;
        }

        ~ADT7410()
        {
            SC18IM700.Close();
        }

        public double TempRead()
        {
            var data = SC18IM700.I2cRead(address, 2);
            if (data == Array.Empty<byte>()) { return 0.0; }

            if (!BitConverter.IsLittleEndian) { data.Reverse(); }
            var temp = ((data[0] << 8 | data[1]) >> 3);
            return (double)temp / 16;
        }
    }
}
