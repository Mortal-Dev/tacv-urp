﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ServerSystemAttribute : NetworkSystemBaseAttribute
{
}