﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace GobangGameServer
{
    class User
    {
        public TcpClient client { get; private set; }
        public StreamReader sr { get; private set; }
        public StreamWriter sw { get; private set; }
        public string userName { get; set; }
        public User(TcpClient client)
        {
            this.client = client;
            this.userName = "";
            NetworkStream netStream = client.GetStream();
            sr = new StreamReader(netStream, System.Text.Encoding.UTF8);
            sw = new StreamWriter(netStream, System.Text.Encoding.UTF8);
        }
    }
}
