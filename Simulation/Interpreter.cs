using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Simulation
{
    public static class Interpreter
    {
        public static byte[] Path(IPAddress src, IPAddress dst, double band)
        {
            List<byte> temp = new List<byte>();
            temp.Add((byte)ControlParam.Path);
            temp.AddRange(src.GetAddressBytes());
            temp.AddRange(dst.GetAddressBytes());
            temp.AddRange(BitConverter.GetBytes(band));
            return temp.ToArray();
        }
        public static void Path(byte[] package, out IPAddress src, out IPAddress dst, out double band)
        {
            src = new IPAddress(new byte[] { package[1], package[2], package[3], package[4] });
            dst = new IPAddress(new byte[] { package[5], package[6], package[7], package[8] });
            band = BitConverter.ToDouble(package, 9);
        }
        public static byte[] PathOK(int sessionID)
        {
            List<byte> temp = new List<byte>();
            temp.AddRange(BitConverter.GetBytes(sessionID));
            return Protocol.CreateResponse(ControlResponse.OK, temp.ToArray());
        }
        public static void PathOK(byte[] package, out int sessionID)
        {
            sessionID = BitConverter.ToInt32(package, 1);
        }

        public static byte[] PathUpdate(int sessionID)
        {
            List<byte> temp = new List<byte>();
            temp.Add((byte)ControlParam.PathUpdate);
            temp.AddRange(BitConverter.GetBytes(sessionID));
            return temp.ToArray();
        }
        public static void PathUpdate(byte[] package, out int sessionID)
        {
            sessionID = BitConverter.ToInt32(package, 1);
        }
        public static byte[] PathReSet(int sessionID, double band)
        {
            List<byte> temp = new List<byte>();
            temp.Add((byte)ControlParam.PathReSet);
            temp.AddRange(BitConverter.GetBytes(sessionID));
            temp.AddRange(BitConverter.GetBytes(band));
            return temp.ToArray();
        }
        public static void PathReSet(byte[] package, out int sessionID, out double band)
        {
            sessionID = BitConverter.ToInt32(package, 1);
            band = BitConverter.ToDouble(package, 5);
        }
        public static byte[] PathTerminate(int sessionID)
        {
            List<byte> temp = new List<byte>();
            temp.Add((byte)ControlParam.PathTerminate);
            temp.AddRange(BitConverter.GetBytes(sessionID));
            return temp.ToArray();
        }
        public static void PathTerminate(byte[] package, out int sessionID)
        {
            sessionID = BitConverter.ToInt32(package, 1);
        }
        public static string TimeOut(int sessionID)
        {
            List<byte> temp = new List<byte>();
            temp.AddRange(BitConverter.GetBytes(sessionID));
            return Protocol.CreateMessage(ControlParam.TimedOut, temp.ToArray());
        }
        public static int Terminate(string message)
        {
            return Convert.ToInt32(message);
        }
        public static string Terminate(int sessionID)
        {
            List<byte> temp = new List<byte>();
            temp.AddRange(BitConverter.GetBytes(sessionID));
            return Protocol.CreateMessage(ControlParam.SessionTerminated, temp.ToArray());
        }
        public static int TimeOut(string message)
        {
            return Convert.ToInt32(message);
        }
        public static byte[] LinkDown(int FiberID)
        {
            List<byte> temp = new List<byte>();
            temp.Add((byte)ControlParam.LinkDown);
            temp.AddRange(BitConverter.GetBytes(FiberID));
            return temp.ToArray();
        }

        public static byte[] LinkUp(int FiberID)
        {
            List<byte> temp = new List<byte>();
            temp.Add((byte)ControlParam.LinkUp);
            temp.AddRange(BitConverter.GetBytes(FiberID));
            return temp.ToArray();
        }
    }
}
