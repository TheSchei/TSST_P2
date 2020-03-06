using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

public enum Type { HOST, ROUTER };

namespace Driver
{
    class Node
    {
        private int id;
        private IPAddress ip;
        private int port;
        private Type type;

        public int ID
        {
            get => id;
        }

        public IPAddress IP
        {
            get => ip;
        }

        public int Port
        {
            get => port;
        }

        public Type Type
        {
            get => type;
        }

        public Node(int id, IPAddress ip, int port, Type type)
        {
            this.id = id;
            this.ip = ip;
            this.type = type;
            this.port = port;
        }
    }
}
