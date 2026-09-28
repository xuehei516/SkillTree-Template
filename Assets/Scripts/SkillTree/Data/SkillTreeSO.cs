using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeDetails_", menuName = "Scriptable Objects/Skill Tree/Skill Tree Details")]
public class SkillTreeSO : ScriptableObject
{
	[Space(10)]
	[Header("技能树基础详情")]
	[Tooltip("技能树名称")]
	public string skillTreeName;

	[Tooltip("技能树唯一 ID")]
	public string skillTreeID;

	[Min(0)]
	[Tooltip("技能树初始技能点数量")]
	public int startingPoints;

	[Tooltip("技能树包含的全部技能")]
	public List<SkillSO> skillList = new List<SkillSO>();

	/// <summary>
	/// 通过技能 ID 获取技能详情对象
	/// </summary>
	/// <param name="skillID"></param>
	/// <returns></returns>
	public SkillSO GetSkillByID(string skillID)
	{
		if (string.IsNullOrWhiteSpace(skillID))
			return null;

		foreach (SkillSO skillSO in skillList)
		{
			if (skillSO != null && skillSO.skillID == skillID)
				return skillSO;
		}

		return null;
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (string.IsNullOrWhiteSpace(skillTreeID))
		{
			skillTreeID = Guid.NewGuid().ToString("N");
			EditorUtility.SetDirty(this);
		}

		startingPoints = Mathf.Max(0, startingPoints);
		HashSet<SkillSO> skillSet = new HashSet<SkillSO>();
		HashSet<string> skillIDSet = new HashSet<string>();

		foreach (SkillSO skillSO in skillList)
		{
			if (skillSO == null)
			{
				Debug.LogWarning($"技能树 {name} 中存在空技能引用", this);
				continue;
			}

			// 检查技能是否重复
			if (!skillSet.Add(skillSO))
				Debug.LogWarning($"技能树 {name} 重复包含技能 {skillSO.name}", this);

			// 检查技能 ID 是否为空或重复
			if (!string.IsNullOrWhiteSpace(skillSO.skillID) && !skillIDSet.Add(skillSO.skillID))
				Debug.LogWarning($"技能树 {name} 中存在重复技能 ID: {skillSO.skillID}", this);
		}
	}
#endif
}
