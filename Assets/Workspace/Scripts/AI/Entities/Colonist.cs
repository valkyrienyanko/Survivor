using GameAPI.Tasks;
using UnityEngine;

public partial class Colonist : ResourceGatherer<Colonist>
{
    private Animator animator;

    public new void Start()
    {
        base.Start();

        Health = 10 + Random.Range(0, 5);
        MaxHealth = Health;

        transform.parent = World.CategoryColonists.Transform;

        Damage = 1 + Random.Range(0, 3);

        animator = GetComponentInChildren<Animator>();
    }

    public new void Update()
    {
        base.Update();

        float speedPercent = Mathf.Clamp01(body.velocity.magnitude * 10);
        animator.SetFloat("speedPercent", speedPercent, 0.1f, Time.deltaTime);
    }
}

public partial class Colonist
{
    private static GameObject _prefab = null;

    public static Colonist New(string name = "Colonist", Vector3? location = null, Faction faction = null)
    {
        if (_prefab == null)
            _prefab = Resources.Load("Prefabs/Colonist") as GameObject;

        Colonist colonist = (location == null ? Instantiate(_prefab) : Instantiate(_prefab, (Vector3)location, Quaternion.identity)).GetComponent<Colonist>();

        colonist.gameObject.name = name;
        colonist.Faction = faction;
        colonist.Queue(new GatherResourceTask<Colonist>(Material.Stone));

        return colonist;
    }
}