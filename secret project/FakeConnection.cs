﻿using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace secret_project
{
    public class FakeConnection : NetworkConnectionToClient
    {
        public FakeConnection(int connectionId) : base(connectionId)
        {

        }

        public override string address
        {
            get
            {
                return "localhost";
            }
        }

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }
        public override void Disconnect()
        {
        }
    }
}
