using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SourcePokemon : SourceDataString<SourcePokemon.Row>
{
    [Serializable]
    public class Row : CardRow
    {
        public string affiliated_race_name => id;

        public string evolution;

        public string[] types;

        public int evolveLevel;

        public string[] levelUpMoves;

        public float spriteScale = 1.0f;
    }

    public override Row CreateRow()
    {
        return new Row
        {
            id = GetString(0),
            evolution = GetString(1),
            types = new string[] { GetString(2), GetString(3) },
            spriteScale = GetFloat(4),
            evolveLevel = GetInt(5),
            levelUpMoves = GetStringArray(6),
        };
    }

    public override void SetRow(Row r)
    {
        map[r.id] = r;
    }
}