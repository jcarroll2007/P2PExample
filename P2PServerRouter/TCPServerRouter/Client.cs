﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerRouter
{
    public class Client
    {
        public Client()
        {

        }

        public double LastContactTime { get; set; }

        public void isAlive()
        {
            double t = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
            LastContactTime = t;
        }
    }
}