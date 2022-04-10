using UnityEngine;
using Lean.Pool;
using TMPro;
using DATA;

public class EnemySpawner : MonoBehaviour
{
	[SerializeField] Transform[] enemy = new Transform[1];
	[SerializeField] Transform[] locations = new Transform[1];
	[SerializeField] UnitRace[] locRace = new UnitRace[1];
	[SerializeField] Vector3[] rotations = new Vector3[1];
	
	int spawningEnemyCount;
	
	float spawningClamp;
	
	const float constF_spawningClamp_Min = 7;
	const float constF_spawningClamp_Max = 12;
	
	void Awake()
	{
		spawningEnemyCount = enemy.Length;
		spawningClamp = 3f;
	}
	
	void Update()
	{
		spawningClamp -= Time.deltaTime;
		if (spawningClamp <= 0)
		{
			spawningClamp = Random.Range(constF_spawningClamp_Min, constF_spawningClamp_Max);
			
			for (int i = 0; i < locations.Length; i++)
			{
				Transform findSpawn = null;
				while (findSpawn == null)
				{
					int RNG = Random.Range( 0, spawningEnemyCount );
					if ( enemy[RNG].TryGetComponent(out Unit unit) )
					{
						int x = i;
						if (unit.Race != locRace[x])
						{
							findSpawn = unit.Prefab;
							locRace[x] = unit.Race;
						}
					}
				}
				int RNG2 = Random.Range( 0, 1 );
				Transform spawnedUnit = LeanPool.Spawn( findSpawn, locations[i].position, Quaternion.Euler(rotations[RNG2]) );
				spawnedUnit.GetComponent<Unit>().Init();
			}
		}
	}
}