using System;
using System.Collections.Generic;

[Serializable]
public class AllSkillTreesSaveData
{
	public List<SkillTreeSaveData> skillTreeSaveDataList = new List<SkillTreeSaveData>();
}



/// <summary>
/// 要保存的技能树数据
/// </summary>
[Serializable]
public class SkillTreeSaveData
{
	/// <summary>
	/// 技能树唯一 ID
	/// </summary>
	public string skillTreeID;
	/// <summary>
	/// 当前剩余技能点
	/// </summary>
	public int availablePoints;
	/// <summary>
	/// 所有技能等级记录
	/// </summary>
	public List<SkillLevelSaveData> skillLevelList = new List<SkillLevelSaveData>();
}

/// <summary>
/// 要保存的技能等级数据
/// </summary>
[Serializable]
public class SkillLevelSaveData
{
	/// <summary>
	/// 技能唯一 ID
	/// </summary>
	public string skillID;
	/// <summary>
	/// 当前等级
	/// </summary>
	public int currentLevel;
}
