using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 该技能解锁所需的前置技能条件
/// </summary>
[Serializable]
public class SkillRequirement
{
	[Tooltip("前置技能")]
	public SkillSO requiredSkill;

	[Min(1)]
	[Tooltip("前置技能需要达到的等级")]
	public int requiredLevel = 1;
}

[CreateAssetMenu(fileName = "SkillDetails_", menuName = "Scriptable Objects/Skill Tree/Skill Details")]
public class SkillSO : ScriptableObject
{
	[Space(10)]
	[Header("技能基础详情")]

	[Tooltip("用于存档和跨项目引用的唯一标识，创建资产后不要随意修改")]
	public string skillID;

	[Tooltip("技能名称")]
	public string skillName;

	// 字符串显示为多行文本框
	[TextArea(2, 5)]
	[Tooltip("技能描述")]
	public string skillDescription;

	[Tooltip("技能图标")]
	public Sprite skillIcon;

	[Space(10)]
	[Header("技能升级规则")]

	[Min(1)]
	[Tooltip("技能最大等级")]
	public int maxLevel = 1;

	[Min(0)]
	[Tooltip("每次升级消耗的技能点")]
	public int pointCost = 1;

	[Tooltip("该技能的全部前置条件；没有前置条件时，技能默认可学习")]
	public List<SkillRequirement> skillRequirementList = new List<SkillRequirement>();

	/// <summary>
	/// 获取指定等级的升级消耗。子类可以重写此方法，实现逐级增加消耗等规则。
	/// </summary>
	public virtual int GetPointCost(int targetLevel)
	{
		return pointCost;
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		// 自动生成技能唯一 ID
		if (string.IsNullOrWhiteSpace(skillID))
		{
			skillID = Guid.NewGuid().ToString("N"); //将 GUID 转换为字符串，"N" 是格式参数
			EditorUtility.SetDirty(this);
		}

		// 修正非法的数值
		maxLevel = Mathf.Max(1, maxLevel);
		pointCost = Mathf.Max(0, pointCost);

		// 修正前置技能列表，防止出现技能自己作为前置技能的情况
		foreach (SkillRequirement skillRequirement in skillRequirementList)
		{
			if (skillRequirement == null)
				continue;

			skillRequirement.requiredLevel = Mathf.Max(1, skillRequirement.requiredLevel);

			if (skillRequirement.requiredSkill == this)
			{
				Debug.LogWarning($"技能 {name} 不能把自己设置为前置技能", this);
				skillRequirement.requiredSkill = null;
			}
		}
	}
#endif
}