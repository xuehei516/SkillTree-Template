using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SkillTreeEvent : MonoBehaviour
{
	/// <summary>
	/// 技能树改变事件，在技能升级、重置或其他操作后触发
	/// </summary>
	public event Action<SkillTreeEvent, SkillTreeEventArgs> OnSkillTreeChanged;
	/// <summary>
	/// 技能升级失败事件，在尝试升级技能但失败时触发
	/// </summary>
	public event Action<SkillTreeEvent, SkillUpgradeFailedEventArgs> OnSkillUpgradeFailed;

	/// <summary>
	/// 调用技能树改变事件，传入技能详情，当前等级，可用点数，以及技能树改变类型
	/// </summary>
	/// <param name="skillSO"></param>
	/// <param name="currentLevel"></param>
	/// <param name="availablePoints"></param>
	/// <param name="skillTreeChangeType"></param>
	public void CallSkillTreeChangedEvent(SkillSO skillSO, int currentLevel, int availablePoints,
		SkillTreeChangeType skillTreeChangeType)
	{
		OnSkillTreeChanged?.Invoke(this, new SkillTreeEventArgs
		{
			skillSO = skillSO,
			currentLevel = currentLevel,
			availablePoints = availablePoints,
			skillTreeChangeType = skillTreeChangeType
		});
	}

	/// <summary>
	/// 调用技能升级失败事件，传入技能详情和升级结果
	/// </summary>
	/// <param name="skillSO"></param>
	/// <param name="skillUpgradeResult"></param>
	public void CallSkillUpgradeFailedEvent(SkillSO skillSO, SkillUpgradeResult skillUpgradeResult)
	{
		OnSkillUpgradeFailed?.Invoke(this, new SkillUpgradeFailedEventArgs
		{
			skillSO = skillSO,
			skillUpgradeResult = skillUpgradeResult
		});
	}
}

public class SkillTreeEventArgs : EventArgs
{
	public SkillSO skillSO;
	public int currentLevel;
	public int availablePoints;
	public SkillTreeChangeType skillTreeChangeType;
}

public class SkillUpgradeFailedEventArgs : EventArgs
{
	public SkillSO skillSO;
	public SkillUpgradeResult skillUpgradeResult;
}
