using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PokeDice : Dice
{
    public override string ToString()
    {
        return "[" + bonus + " ~ " + (bonus + sides) + "] * " + num;
    }
}
