using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

[RequireComponent(typeof(SkillTreeEvent))]
[DisallowMultipleComponent]
public class SkillTreeManager : MonoBehaviour
{
	[Tooltip("所有可用的技能树配置列表，用于在运行时切换技能树")]
	[SerializeField] private List<SkillTreeSO> skillTreeList = new List<SkillTreeSO>();

	[Tooltip("是否在 Start 时自动初始化")]
	[SerializeField] private bool initializeOnStart = true;

	/// <summary>
	/// 当前需要的技能树SO
	/// </summary>
	private SkillTreeSO currentSkillTreeSO;
	/// <summary>
	/// 存储当前技能树每个技能对应的当前技能等级，键为技能详情对象，值为当前技能等级
	/// </summary>
	private readonly Dictionary<SkillSO, int> skillLevelDictionary = new Dictionary<SkillSO, int>();
	/// <summary>
	/// 可用剩余技能点。升级技能时会消耗，重置技能时会返还
	/// </summary>
	private int availablePoints;
	/// <summary>
	/// 是否已初始化
	/// </summary>
	private bool isInitialized;

	/// <summary>
	/// 全部技能树的存档数据
	/// </summary>
	private AllSkillTreesSaveData allSkillTreesSaveData = new AllSkillTreesSaveData();

	private SkillTreeEvent skillTreeEvent;

	#region 成员属性
	public SkillTreeSO SkillTreeSO => currentSkillTreeSO;
	public int AvailablePoints => availablePoints;
	public bool IsInitialized => isInitialized;
	#endregion

	private void Awake()
	{
		skillTreeEvent = GetComponent<SkillTreeEvent>();
	}

	private void Start()
	{
		if (initializeOnStart && skillTreeList.Count > 0)
		{
			Initialize(skillTreeList[0], skillTreeList[0].startingPoints);
		}
	}

	/// <summary>
	/// 使用指定配置初始化技能树。重复调用会清空当前运行时进度
	/// </summary>
	public void Initialize(SkillTreeSO newSkillTreeSO, int startingPoints)
	{
		if (newSkillTreeSO == null)
		{
			Debug.LogError("无法使用空的 SkillTreeSO 初始化技能树", this);
			return;
		}

		// 一个管理器实例只能管理一个技能树，因此先清空当前技能树数据，重新初始化
		currentSkillTreeSO = newSkillTreeSO;
		skillLevelDictionary.Clear();

		// 遍历技能列表，初始化每个技能的等级为 0
		foreach (SkillSO skillSO in currentSkillTreeSO.skillList)
		{
			if (skillSO != null && !skillLevelDictionary.ContainsKey(skillSO))
				skillLevelDictionary.Add(skillSO, 0);
		}

		availablePoints = Mathf.Max(0, startingPoints);
		isInitialized = true;

		LoadFromDisk();

		skillTreeEvent.CallSkillTreeChangedEvent(null, 0, availablePoints, SkillTreeChangeType.initialized);
	}

	/// <summary>
	/// 尝试升级技能，返回升级结果。升级成功会触发技能树改变事件，失败会触发技能升级失败事件
	/// </summary>
	public SkillUpgradeResult TryUpgradeSkill(SkillSO skillSO)
	{
		SkillUpgradeResult skillUpgradeResult = GetSkillUpgradeResult(skillSO);

		// 如果升级条件不满足，触发技能升级失败事件并返回结果
		if (skillUpgradeResult != SkillUpgradeResult.success)
		{
			skillTreeEvent.CallSkillUpgradeFailedEvent(skillSO, skillUpgradeResult);
			return skillUpgradeResult;
		}

		// 可以升级技能

		// 升级技能，获取目标等级的技能点消耗
		int targetLevel = GetSkillLevel(skillSO) + 1;
		int pointCost = skillSO.GetPointCost(targetLevel);

		// 更新技能等级和剩余技能点
		skillLevelDictionary[skillSO] = targetLevel;
		availablePoints -= pointCost;

		skillTreeEvent.CallSkillTreeChangedEvent(skillSO, targetLevel, availablePoints, SkillTreeChangeType.skillUpgraded);

		SaveToDisk();

		//也可以 return skillUpgradeResult;
		return SkillUpgradeResult.success;
	}

	/// <summary>
	/// 返回传入技能是否能升级的结果，用于判断技能是否可以升级
	/// </summary>
	public SkillUpgradeResult GetSkillUpgradeResult(SkillSO skillSO)
	{
		// 先检查技能树是否已初始化
		if (!isInitialized)
			return SkillUpgradeResult.skillTreeNotInitialized;

		// 检查技能是否属于当前技能树
		if (skillSO == null || !skillLevelDictionary.ContainsKey(skillSO))
			return SkillUpgradeResult.skillNotFound;

		
		int currentLevel = GetSkillLevel(skillSO);

		// 检查是否达到最大等级
		if (currentLevel >= skillSO.maxLevel)
			return SkillUpgradeResult.maxLevelReached;

		// 检查前置条件是否满足
		if (!AreRequirementsMet(skillSO))
			return SkillUpgradeResult.requirementNotMet;

		// 检查是否有升级为目标技能等级足够的技能点
		if (availablePoints < skillSO.GetPointCost(currentLevel + 1))
			return SkillUpgradeResult.notEnoughPoints;

		return SkillUpgradeResult.success;
	}

	/// <summary>
	/// 获取技能当前显示状态
	/// </summary>
	public SkillState GetSkillState(SkillSO skillSO)
	{
		// 技能树未初始化、技能不存在于当前技能树中或传入了 null 时，返回锁定状态
		if (!isInitialized || skillSO == null || !skillLevelDictionary.ContainsKey(skillSO))
			return SkillState.locked;

		int currentLevel = GetSkillLevel(skillSO);

		// 如果技能已达到最大等级，返回达到最大等级状态
		if (currentLevel >= skillSO.maxLevel)
			return SkillState.maxed;

		// 如果技能已学习（等级大于 0），返回已学习状态
		if (currentLevel > 0)
			return SkillState.learned;
		
		// 如果技能前置条件满足且点数足够，返回可用状态，否则返回锁定状态
		return (AreRequirementsMet(skillSO) && availablePoints >= skillSO.GetPointCost(currentLevel + 1)) ? SkillState.available : SkillState.locked;
	}

	/// <summary>
	/// 在当前技能树中获取技能当前等级
	/// </summary>
	public int GetSkillLevel(SkillSO skillSO)
	{
		if (skillSO != null && skillLevelDictionary.TryGetValue(skillSO, out int currentLevel))
			return currentLevel;

		return 0;
	}

	/// <summary>
	/// 检查技能的全部前置条件是否满足
	/// </summary>
	public bool AreRequirementsMet(SkillSO skillSO)
	{
		if (skillSO == null)
			return false;

		// 如果技能没有前置条件，直接返回 true
		if (skillSO.skillRequirementList == null || skillSO.skillRequirementList.Count == 0)
			return true;

		// 遍历技能的前置条件列表，检查每个前置技能的等级是否满足要求
		foreach (SkillRequirement skillRequirement in skillSO.skillRequirementList)
		{
			if (skillRequirement == null || skillRequirement.requiredSkill == null)
				continue;

			if (GetSkillLevel(skillRequirement.requiredSkill) < skillRequirement.requiredLevel)
				return false;
		}

		return true;
	}

	/// <summary>
	/// 增加或扣除技能点，结果不会低于零
	/// </summary>
	public void AddSkillPoints(int amount)
	{
		if (!isInitialized)
			return;

		availablePoints = Mathf.Max(0, availablePoints + amount);

		SaveToDisk();
		skillTreeEvent.CallSkillTreeChangedEvent(null, 0, availablePoints, SkillTreeChangeType.pointsChanged);
	}

	/// <summary>
	/// 重置所有技能等级，并返还已消耗的技能点
	/// </summary>
	public void ResetSkills()
	{
		if (!isInitialized)
			return;

		int refundPoints = 0;
		// 根据字典的键创建一个技能列表，避免在遍历过程中修改字典
		List<SkillSO> skillList = new List<SkillSO>(skillLevelDictionary.Keys);

		// 遍历技能列表，计算返还的技能点，并将每个技能的等级重置为 0
		foreach (SkillSO skillSO in skillList)
		{
			int currentLevel = skillLevelDictionary[skillSO];

			for (int level = 1; level <= currentLevel; level++)
				refundPoints += skillSO.GetPointCost(level);

			skillLevelDictionary[skillSO] = 0;
		}

		availablePoints += refundPoints;

		SaveToDisk();
		skillTreeEvent.CallSkillTreeChangedEvent(null, 0, availablePoints, SkillTreeChangeType.reset);
	}

	/// <summary>
	/// 保存当前技能树数据到 AllSkillTreesSaveData 对象中，并返回该对象
	/// </summary>
	public AllSkillTreesSaveData SaveCurrentSkillTreeData()
	{
		// 保存一棵技能树数据
		SkillTreeSaveData skillTreeSaveData = new SkillTreeSaveData()
		{
			skillTreeID = currentSkillTreeSO.skillTreeID,
			availablePoints = availablePoints,
		};

		skillTreeSaveData.skillLevelList.Clear();

		foreach (KeyValuePair<SkillSO, int> skillLevel in skillLevelDictionary)
		{
			skillTreeSaveData.skillLevelList.Add(new SkillLevelSaveData()
			{
				skillID = skillLevel.Key.skillID,
				currentLevel = skillLevel.Value
			});
		}

		string treeID = GetTreeID(currentSkillTreeSO);

		// 遍历所有技能树存档数据，查找当前技能树的存档数据，如果存在则更新，否则添加新的存档数据
		for (int i = 0; i < allSkillTreesSaveData.skillTreeSaveDataList.Count; i++)
		{
			// 如果存档数据的技能树 ID 与当前技能树的 ID 匹配，则更新该存档数据
			if (allSkillTreesSaveData.skillTreeSaveDataList[i].skillTreeID == treeID)
			{
				allSkillTreesSaveData.skillTreeSaveDataList[i] = skillTreeSaveData;
				return allSkillTreesSaveData;
			}
		}

		allSkillTreesSaveData.skillTreeSaveDataList.Add(skillTreeSaveData);

		return allSkillTreesSaveData;
	}

	/// <summary>
	/// 获取当前技能树的存档数据，并恢复技能等级和可用技能点
	/// </summary>
	public void RestoreSaveData(AllSkillTreesSaveData allSkillTreesSaveData)
	{
		if (!isInitialized || allSkillTreesSaveData == null)
			return;

		// 先将所有技能等级重置为 0
		foreach (SkillSO skillSO in currentSkillTreeSO.skillList)
		{
			if (skillSO != null)
				skillLevelDictionary[skillSO] = 0;
		}

		// 遍历所有技能树存档数据，查找当前技能树的存档数据，如果存在则恢复技能等级和可用技能点
		foreach (SkillTreeSaveData skillTreeSaveData in allSkillTreesSaveData.skillTreeSaveDataList)
		{
			// 如果存档数据的技能树 ID 与当前技能树的 ID 匹配，则恢复技能等级和可用技能点
			if (skillTreeSaveData.skillTreeID == GetTreeID(currentSkillTreeSO))
			{
				availablePoints = skillTreeSaveData.availablePoints;

				foreach (SkillLevelSaveData skillLevelSaveData in skillTreeSaveData.skillLevelList)
				{
					SkillSO skillSO = currentSkillTreeSO.GetSkillByID(skillLevelSaveData.skillID);

					if (skillSO != null)
						skillLevelDictionary[skillSO] = skillLevelSaveData.currentLevel;
				}
				break;
			}
		}

		skillTreeEvent.CallSkillTreeChangedEvent(null, 0, availablePoints, SkillTreeChangeType.restored);
	}

	private string GetSkillTreeSaveDataFilePath()
	{
		// 可以复制该路径来找到json存档位置
		//print(Path.Combine(Application.persistentDataPath, "skill_tree_save.json"));
		return Path.Combine(Application.persistentDataPath, "skill_tree_save.json");
	}

	private string GetTreeID(SkillTreeSO skillTreeSO)
	{
		if (skillTreeSO == null)
			return string.Empty;

		return skillTreeSO.skillTreeID;
	}

	private void SaveToDisk()
	{
		try
		{   // 保存技能树状态到 JSON 文件
			string skillTreeSaveDataJson = JsonUtility.ToJson(SaveCurrentSkillTreeData(), true);
			File.WriteAllText(GetSkillTreeSaveDataFilePath(), skillTreeSaveDataJson);
		}
		catch (Exception ex)
		{
			Debug.LogError($"保存技能树存档失败: {ex.Message}");
		}
	}

	private void LoadFromDisk()
	{
		try
		{
			string path = GetSkillTreeSaveDataFilePath();

			if (File.Exists(path))
			{
				string allSkillTreeSaveDataJson = File.ReadAllText(path);
				allSkillTreesSaveData = JsonUtility.FromJson<AllSkillTreesSaveData>(allSkillTreeSaveDataJson);

				if (allSkillTreesSaveData != null)
				{
					RestoreSaveData(allSkillTreesSaveData);
				}
				else
				{
					Debug.LogWarning($"读取到无效的存档 JSON：{path}");
				}

			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"读取技能树存档失败: {ex.Message}");
		}
	}
}
