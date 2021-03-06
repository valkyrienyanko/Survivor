public class Rock : StructureResource
{
    public static int ID = 0;

    public override void Awake()
    {
        base.Awake();

        SetName($"Rock {++ID}");
    }

    public override void Start() =>
        base.Start();

    public override string ToString() =>
        $"ID: {ID}";
}