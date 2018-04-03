using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace usb_i2c_board
{
    public enum SC18IM700Commmand : Byte
    {
        I2cStart = 0x53,
        I2cStop = 0x50,
        ReadRegister = 0x52,
        WriteRegister = 0x57,
        ReadPort = 0x49,
        WritePort = 0x4F,
        PowerOff = 0x5A
    }

    public enum SC18IM700Register : Byte
    {
        Brg0 = 0x00,
        Brg1 = 0x01,
        PortConf1 = 0x02,
        PortConf2 = 0x03,
        IOState = 0x04,
        I2cAddress = 0x06,
        I2cClockLow = 0x07,
        I2cClockHigh = 0x08,
        I2cTo = 0x09,
        I2cStatus = 0x0A
    }

    public enum SC18IM700GpioMode : Byte
    {
        Bidirectional = 0x00,
        Input = 0x01,
        Output = 0x02,
        OpenDrain = 0x03
    }

    public enum SC18IM700GpioState : Byte
    {
        High = 0x00,
        Low = 0x01
    }

    public static class SC18IM700
    {
        #region Field
        private static SerialPort serialPort = null;
        private static int deviceCount = 0;
        #endregion

        #region Method
        /// <summary>
        /// SC18IM700への接続をオープン
        /// </summary>
        /// <param name="port">ポート名</param>
        /// <returns>オープンの成否</returns>
        public static bool Open(String port)
        {
            if (IsValid())
            {
                if (!port.Equals(serialPort.PortName))
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    if (serialPort == null) { serialPort = new SerialPort(port, 9600) { ReadTimeout = 100, WriteTimeout = 100 }; }
                    serialPort.Open();
                    AllPortModeWrite();
                }
                catch (Exception)
                {
                    serialPort = null;
                    deviceCount = 0;
                    return false;
                }
            }

            deviceCount++;
            return true;
        }

        /// <summary>
        /// SC18IM700への接続をクローズ
        /// </summary>
        public static void Close()
        {
            deviceCount--;
            if (deviceCount <= 0) { Dispose(); }
        }

        /// <summary>
        /// SC18IM700への接続をすべてクローズ
        /// </summary>
        public static void Dispose()
        {
            deviceCount = 0;
            if (IsValid()) { serialPort.Close(); }
            serialPort?.Dispose();
            serialPort = null;
        }

        /// <summary>
        /// 指定レジスタへ値を書き込み
        /// </summary>
        /// <param name="addr">レジスタアドレス</param>
        /// <param name="data">書き込むバイトデータ</param>
        /// <returns>書き込みの成否</returns>
        public static bool RegWrite(SC18IM700Register addr, byte data)
        {
            if (!IsValid()) { return false; }

            var packet = new byte[] { (byte)SC18IM700Commmand.WriteRegister,
                                      (byte)addr,
                                      data,
                                      (byte)SC18IM700Commmand.I2cStop };
            try
            {
                serialPort.Write(packet, 0, packet.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 指定レジスタの値の読み出し
        /// </summary>
        /// <param name="addr">レジスタアドレス</param>
        /// <param name="data">読み出したバイトデータ</param>
        /// <returns>読み出しの成否</returns>
        public static bool TryRegRead(SC18IM700Register addr, out byte data)
        {
            data = 0;
            if (!IsValid()) { return false; }

            var packet = new byte[] { (byte)SC18IM700Commmand.ReadRegister,
                                      (byte)addr,
                                      (byte)SC18IM700Commmand.I2cStop };
            try
            {
                serialPort.Write(packet, 0, packet.Length);
            }
            catch (Exception)
            {
                return false;
            }

            var outdata = ReadSerialData(1);
            if (outdata.Length == 1) { data = outdata[0]; }
            return outdata.Length == 1;
        }

        public static bool AllPortModeWrite(Byte mode = 0xFF)
        {
            if (!IsValid()) { return false; }
 
            var status = new SC18IM700GpioMode[8];

            for (var i = 0; i < 8; i++)
            {
                status[i] = ((mode >> i) & 1) == 1 ? SC18IM700GpioMode.Input : SC18IM700GpioMode.Output;
            }

            return AllPortModeWrite(status);
        }

        public static bool AllPortModeWrite(SC18IM700GpioMode[] status)
        {
            if (!IsValid() || status.Count() != 8) { return false; }

            byte conf1 = 0;
            byte conf2 = 0;

            for (var i = 0; i < 4; i++)
            {
                conf1 |= (byte)(((byte)status[i] & 0x03) << (2 * i));
                conf2 |= (byte)(((byte)status[i+4] & 0x03) << (2 * i));
            }

            var packet = new byte[] { (byte)SC18IM700Commmand.WriteRegister,
                                      (byte)SC18IM700Register.PortConf1,
                                      conf1,
                                      (byte)SC18IM700Register.PortConf2,
                                      conf2,
                                      (byte)SC18IM700Commmand.I2cStop };
            try
            {
                serialPort.Write(packet, 0, packet.Length);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool AllPortWrite(byte data)
        {
            if (!IsValid()) { return false; }

            var packet = new byte[] { (byte)SC18IM700Commmand.WritePort,
                                      data,
                                      (byte)SC18IM700Commmand.I2cStop };
            try
            {
                serialPort.Write(packet, 0, packet.Length);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool AllPortWrite(SC18IM700GpioState[] data)
        {
            if (!IsValid() ||
                data.Count() != 8 ||
                data.Any(stat => stat != SC18IM700GpioState.High && stat != SC18IM700GpioState.Low))
            {
                return false;
            }

            byte iodata = 0;
            for (var i = 0; i < 8; i++)
            {
                if (data[i] == SC18IM700GpioState.High) { iodata |= (byte)(1 << i); }
            }

            return AllPortWrite(iodata);
        }

        public static bool TryAllPortRead(out byte data)
        {
            data = 0;
            if (!IsValid()) { return false; }

            var packet = new Byte[] { (Byte)SC18IM700Commmand.ReadPort,
                                      (Byte)SC18IM700Commmand.I2cStop };
            try
            {
                serialPort.Write(packet, 0, packet.Length);
            }
            catch (Exception)
            {
                return false;
            }

            var outdata = ReadSerialData(1);
            if (outdata.Length == 1) { data = outdata[0]; }
            return outdata.Length == 1;
        }

        public static bool TryAllPortRead(out SC18IM700GpioState[] data)
        {
            data = new SC18IM700GpioState[8];
            if (!TryAllPortRead(out byte tempdata)) { return false; }

            for (var i = 0; i < 8; i++)
            {
                data[i] = ((tempdata >> i) & 1) == 1 ? SC18IM700GpioState.High : SC18IM700GpioState.Low;
            }
            return true;
        }

        public static bool PortModeWrite(byte pin, SC18IM700GpioMode mode)
        {
            if (pin > 7) { return false; }
            if (!TryPortRegRead(out SC18IM700GpioMode[] status)) { return false; }

            status[pin] = mode;

            return AllPortModeWrite(status);
        }

        public static bool PortWrite(byte pin, SC18IM700GpioState value)
        {
            if (pin > 7) { return false; }
            if (!TryAllPortRead(out SC18IM700GpioState[] status)) { return false; }

            status[pin] = value;
            return AllPortWrite(status);
        }

        public static bool TryPortRead(byte pin, out SC18IM700GpioState value)
        {
            value = SC18IM700GpioState.Low;
            if (pin > 7) { return false; }
            if (!TryAllPortRead(out SC18IM700GpioState[] status)) { return false; }

            value = status[pin];
            return true;
        }

        public static bool I2cWrite(byte deviceaddr, IEnumerable<Byte> data)
        {
            if (!IsValid() || deviceaddr > 0x7F || data.Count() > Byte.MaxValue) { return false; }

            var writeaddr = (byte)((deviceaddr & 0x7F) << 1);
            var packet = new List<Byte>{(Byte)SC18IM700Commmand.I2cStart,
                                        writeaddr,
                                        (byte)data.Count()};
            packet.AddRange(data);
            packet.Add((Byte)SC18IM700Commmand.I2cStop);

            try
            {
                serialPort.Write(packet.ToArray(), 0, packet.Count());
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static byte[] I2cRead(byte deviceaddr, byte length = 1)
        {
            var readaddr = (byte)(((deviceaddr & 0x7F) << 1) + 1);
            var packet = new byte[] { (byte)SC18IM700Commmand.I2cStart,
                                      readaddr,
                                      length,
                                      (Byte)SC18IM700Commmand.I2cStop};
            try
            {
                serialPort.Write(packet, 0, packet.Length);
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }

            var readdata = ReadSerialData(length);
            return readdata.Count() == length ? readdata : Array.Empty<byte>();
        }


        /// <summary>
        /// IOポートの入出力設定の読み出し
        /// </summary>
        /// <param name="confdata">入出力設定の状態</param>
        /// <returns>入出力設定読み出しの成否</returns>
        private static bool TryPortRegRead(out SC18IM700GpioMode[] confdata)
        {
            confdata = new SC18IM700GpioMode[8];
            if (!TryRegRead(SC18IM700Register.PortConf1, out byte portconf1) ||
                !TryRegRead(SC18IM700Register.PortConf2, out byte portconf2))
            {
                return false;
            }

            for (var i = 0; i < 4; i++)
            {
                confdata[i]   = (SC18IM700GpioMode)((portconf1 >> (i * 2)) & 3);
                confdata[i+4] = (SC18IM700GpioMode)((portconf2 >> (i * 2)) & 3);
            }
            return true;
        }

        private static byte[] ReadSerialData(int length)
        {
            if (!IsValid() || length <= 0) { return Array.Empty<byte>(); }

            var outdata = new byte[length];
            int outdatalength;

            try
            {
                outdatalength = serialPort.Read(outdata, 0, length);
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }

            return outdatalength == length ? outdata : Array.Empty<byte>();
        }

        private static bool IsValid() => serialPort?.IsOpen ?? false;
        #endregion
    }
}










