using System;
using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities;
using GridSystem;
using TriInspector;
using UnityEngine;

namespace GoalSystem
{
	public class GoalManager : Singleton<GoalManager>
	{
		public List<Goal> Goals { get; private set; }

		#region Setup

		[Title("Setup")]
		[SerializeField] [ListDrawerSettings(AlwaysExpanded = true)] private GoalSetup[] goalSetup;

		[Serializable]
		[DeclareHorizontalGroup("Goal")]
		private class GoalSetup
		{
			[GUIColor("$GetColor")]
			[Group("Goal")] public TileType TileColor;
			[Group("Goal")] public int Count;

			private Color GetColor
			{
				get
				{
					var color = TileColor switch
					{
						TileType._1Blue => Color.blue,
						TileType._2Green => Color.green,
						TileType._3Orange => new Color(1f, 0.5f, 0),
						TileType._4Pink => Color.magenta,
						TileType._5Purple => new Color(.7f, .25f, 1f),
						TileType._6Red => Color.red,
						TileType._7Yellow => Color.yellow,
						_ => throw new ArgumentOutOfRangeException()
					};

					return color;
				}
			}
		}

		#endregion

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
		}

		private void OnDestroy()
		{
			foreach (var goal in Goals)
				goal.OnGoalComplete -= OnGoalCompleted;

			Goals.Clear();
		}

		private void OnLevelLoaded()
		{
			Goals = new List<Goal>(goalSetup.Length);
			for (var i = 0; i < goalSetup.Length; i++)
			{
				var goal = new Goal(goalSetup[i].TileColor, goalSetup[i].Count);
				goal.OnGoalComplete += OnGoalCompleted;
				Goals.Add(goal);
			}
		}

		private void OnGoalCompleted(Goal goal)
		{
			goal.OnGoalComplete -= OnGoalCompleted;

			Goals.Remove(goal);

			if (Goals.Count.Equals(0))
			{
				LevelManager.Instance.Win();
			}
		}
	}
}