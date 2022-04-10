using UnityEngine;
using UnityEngine.UI;

public class Special : MonoBehaviour
{
    [SerializeField] Image cooldownFiller;
	const float _attackRateReturn = 0.5f;
	
	public void UpdateCD( Unit chara )
	{
		cooldownFiller.fillAmount = chara.attackRate / (_attackRateReturn / chara.AttackSpeed);
	}
}
