using System.Collections;
using System;

namespace SlevaniNadoby;

class Program
{
    static void Main(string[] args)
    {
        int a = Ctecka.readInt();
        int b = Ctecka.readInt();
        int c = Ctecka.readInt();
        int x = Ctecka.readInt();
        int y = Ctecka.readInt();
        int z = Ctecka.readInt();
        Stav pocatecniStav = new Stav(a, b, c, x, y, z);
        Stavy stavy = new Stavy();
        stavy.VygenerujMozneStavy(pocatecniStav);
        stavy.VytiskniMozneObjemy();
        
    }
}
