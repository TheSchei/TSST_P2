using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Simulation
{
    public enum ControlParam : byte {
        GetRouting,
        SetRouting,
        EditRouting,
        DeleteRouting,
        DeleteRoutingbyID,
        GetEdgeRouting,
        SetEdgeRouting,
        EditEdgeRouting,


        DeleteEdgeRouting,
        DeleteEdgeRoutingbyID,
        Stop,
        Start,

        Path,
        PathUpdate,
        PathReSet,
        PathTerminate,
        SessionTerminated,
        TimedOut,
        LinkDown,
        LinkUp,

        CallAccept
    };
    public enum ControlResponse : byte {
        OK,
        ERROR,
        OutOfIndex,
        NotFound,
        AlreadyExists,
        UnknownError
    };

    public static class Protocol
    {
        public static string CreateMessage(ControlParam param)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            return Encoding.ASCII.GetString(message.ToArray());
        }
        public static string CreateMessage(ControlParam param, byte[] payload)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            message.AddRange(payload);
            return Encoding.ASCII.GetString(message.ToArray());
        }
        public static string CreateMessage(ControlParam param, int payload)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            message.AddRange(BitConverter.GetBytes(payload));
            return Encoding.ASCII.GetString(message.ToArray());
        }
        public static byte[] CreateResponse(ControlResponse param, byte[] payload)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            message.AddRange(payload);
            return message.ToArray();
        }
        public static byte[] CreateResponse(ControlResponse param, string payload)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            message.AddRange(Encoding.ASCII.GetBytes(payload));
            return message.ToArray();
        }
        public static byte[] CreateResponse(ControlResponse param)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            return message.ToArray();
        }
        public static string CreateResponseStr(ControlResponse param, byte[] payload)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            message.AddRange(payload);
            return Encoding.ASCII.GetString(message.ToArray());
        }
        public static string CreateResponseStr(ControlResponse param, string payload)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            message.AddRange(Encoding.ASCII.GetBytes(payload));
            return Encoding.ASCII.GetString(message.ToArray());
        }
        public static string CreateResponseStr(ControlResponse param)
        {
            List<byte> message = new List<byte>();
            message.Add((byte)param);
            return Encoding.ASCII.GetString(message.ToArray());
        }
        public static byte[] DeleteControlParam(byte[] payload)
        {
            List<byte> temp = payload.ToList();
            temp.RemoveAt(0);
            return temp.ToArray();
        }
        public static string DeleteParam(string payload)
        {
            List<byte> temp = Encoding.ASCII.GetBytes(payload).ToList();
            temp.RemoveAt(0);
            return Encoding.ASCII.GetString(temp.ToArray());
        }
        public static ControlParam getControlParam(byte[] payload)
        {
            return (ControlParam)payload[0];
        }
        public static ControlParam getControlParam(string payload)
        {
            return (ControlParam)Encoding.ASCII.GetBytes(payload)[0];
        }
        public static ControlResponse getControlResponse(byte[] payload)
        {
            return (ControlResponse)payload[0];
        }
        public static ControlResponse getControlResponse(string payload)
        {
            return (ControlResponse)Encoding.ASCII.GetBytes(payload)[0];
        }
        public static ControlParam getControlParamAndDelete(ref byte[] payload)
        {
            ControlParam output = (ControlParam)payload[0];
            payload = DeleteControlParam(payload);
            return output;
        }
        public static ControlParam getControlParamAndDelete(ref string payload)
        {
            ControlParam output = (ControlParam)Encoding.ASCII.GetBytes(payload)[0];
            payload = DeleteParam(payload);
            return output;
        }
        public static ControlResponse getResponseParamAndDelete(ref string payload)
        {
            ControlResponse output = (ControlResponse)Encoding.ASCII.GetBytes(payload)[0];
            payload = DeleteParam(payload);
            return output;
        }
    }
}
