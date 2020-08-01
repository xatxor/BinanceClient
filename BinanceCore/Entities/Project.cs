using BinanceCore.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore.Entities
{
    class Project:AppSettings<Project>
    {
        public FractalDefinition[] fractals;
        public int interval;
        public string ticker;
    }
}
