using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using DATA;

public static class Utils
{
	const int race_OM = 1;
	const int race_HE = 2;
	const int race_SA = 3;
	const int race_EX = 4;
	const int race_PC = 5;
	
	public static bool CheckTeams( Unit source, Unit target )
	{
		bool[] sourceChecker = new bool[6];
		bool[] tC = new bool[6];
		
		sourceChecker[race_OM] = (source.Race == UnitRace.Offmoths);
		sourceChecker[race_HE] = (source.Race == UnitRace.Human_Expeditors);
		sourceChecker[race_SA] = (source.Race == UnitRace.Sentient_Automatons);
		sourceChecker[race_EX] = (source.Race == UnitRace.Extraterrestrials);
		sourceChecker[race_PC] = (source.Race == UnitRace.Planet_Cultists);
		
		tC[race_OM] = (target.Race == UnitRace.Offmoths);
		tC[race_HE] = (target.Race == UnitRace.Human_Expeditors);
		tC[race_SA] = (target.Race == UnitRace.Sentient_Automatons);
		tC[race_EX] = (target.Race == UnitRace.Extraterrestrials);
		tC[race_PC] = (target.Race == UnitRace.Planet_Cultists);
		
		bool[] comparer = new bool[6];
		
		comparer[1] = sourceChecker[race_OM] && ( tC[race_HE] || tC[race_SA] || tC[race_EX] || tC[race_PC] );
		comparer[2] = sourceChecker[race_HE] && ( tC[race_OM] || tC[race_SA] || tC[race_EX] || tC[race_PC] );
		comparer[3] = sourceChecker[race_SA] && ( tC[race_OM] || tC[race_HE] || tC[race_EX] || tC[race_PC] );
		comparer[4] = sourceChecker[race_EX] && ( tC[race_OM] || tC[race_HE] || tC[race_SA] || tC[race_PC] );
		comparer[5] = sourceChecker[race_PC] && ( tC[race_OM] || tC[race_HE] || tC[race_SA] || tC[race_EX] );
		
		bool finalOutput = comparer[1] || comparer[2] || comparer[3] || comparer[4] || comparer[5];
		
		if ( finalOutput ) return true;
		return false;
	}
}