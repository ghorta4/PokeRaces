using Newtonsoft.Json;
public class ConDelayedMove : BaseDebuff
{

    [JsonProperty]
    public int moveToUse = -1;
    [JsonProperty]
    public string text;

    public override void OnRemoved()
    {
        base.OnRemoved();

        Element toUse = owner.elements.GetElement(moveToUse);

        if(toUse != null)
        {
            PokemonMove move = (PokemonMove)toUse;
            move.CallProc(power, text);
        }
    }
}