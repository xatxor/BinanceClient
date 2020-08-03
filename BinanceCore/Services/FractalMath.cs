using BinanceCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceCore.Services
{
    public class FractalMath
    {
        public static List<Tuple<int, FractalDefinition>> FindFractal(FractalDefinition config, string historyCode)
        {
            var symbolsAtStages = new List<Tuple<int, FractalDefinition>>();
            var graphParts = historyCode.Split(new char[] { ' ' });
            var fractalParts = config.Code.Split(new char[] { ';' });
            for (int i = 0; i < graphParts.Length - fractalParts.Length; i++)
            {
                bool fits = true;
                for (int p = 0; p < fractalParts.Length && fits; p++)
                {
                    var must = fractalParts[p].Trim();
                    var mustParts = must.Split(new char[] { '-' });
                    int mustMin = int.Parse(mustParts[1]);
                    int mustMax = int.Parse(mustParts[2]);
                    var have = graphParts[i + p].Trim();
                    var mustMode = mustParts[0];
                    var haveMode = have.Substring(0, 1);
                    var haveD = int.Parse(have.Substring(1));
                    switch (mustMode)
                    {
                        case "U":
                            if (haveMode == "S" && mustMin > 0) fits = false;
                            if (haveMode == "D") fits = false;
                            if (!(haveMode == "U" && haveD >= mustMin && haveD <= mustMax)) fits = false;
                            break;
                        case "D":
                            if (haveMode == "S" && mustMin > 0) fits = false;
                            if (haveMode == "U") fits = false;
                            if (!(haveMode == "D" && haveD >= mustMin && haveD <= mustMax)) fits = false;
                            break;
                        case "S":
                            if (haveMode == "U" && haveD > mustMax) fits = false;
                            if (haveMode == "D" && haveD > mustMin) fits = false;
                            break;
                    }
                }
                if (fits) symbolsAtStages.Add(new Tuple<int, FractalDefinition>(i, config));
            }
            return symbolsAtStages;
        }

    }
}
