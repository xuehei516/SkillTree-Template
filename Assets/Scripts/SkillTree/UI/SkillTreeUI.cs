using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class SkillTreeUI : MonoBehaviour
{
	[Header("逻辑层引用")]
	[Tooltip("要显示的技能树逻辑组件")]
	[SerializeField] private SkillTreeManager skillTreeManager;

	[Header("渲染引用")]
	[Tooltip("显示技能点的文本组件")]
	[SerializeField] private TMP_Text pointsText;
	[Tooltip("技能点文本格式，{0} 将被替换为当前可用技能点数量")]
	[SerializeField] private string pointsTextFormat = "技能点: {0}";
	[Tooltip("技能槽位数组，按技能树布局顺序排列")]
	[SerializeField] private SkillSlot[] skillSlotArray;

	private SkillTreeEvent skillTreeEvent;

	private void Awake()
	{
		// 防止在 Inspector 中未手动设置 SkillTreeManager
		if (skillTreeManager == null)
			skillTreeManager = GetComponentInParent<SkillTreeManager>();

		if (skillTreeManager != null)
			skillTreeEvent = skillTreeManager.GetComponent<SkillTreeEvent>();

		if (skillSlotArray == null || skillSlotArray.Length == 0)
			skillSlotArray = GetComponentsInChildren<SkillSlot>(true);
	}

	private void Start()
	{
		foreach (SkillSlot skillSlot in skillSlotArray)
		{
			if (skillSlot != null)
				skillSlot.Initialize(skillTreeManager);
		}

		RefreshAllSkillUI();
	}

	private void OnEnable()
	{
		if (skillTreeEvent != null)
			skillTreeEvent.OnSkillTreeChanged += SkillTreeEvent_OnSkillTreeChanged;
	}

	private void OnDisable()
	{
		if (skillTreeEvent != null)
			skillTreeEvent.OnSkillTreeChanged -= SkillTreeEvent_OnSkillTreeChanged;
	}

	
	private void SkillTreeEvent_OnSkillTreeChanged(SkillTreeEvent skillTreeEvent, SkillTreeEventArgs skillTreeEventArgs)
	{
		RefreshAllSkillUI();
	}

	/// <summary>
	/// 刷新技能点和全部槽位。技能升级可能同时解锁多个后续技能，因此统一刷新
	/// </summary>
	public void RefreshAllSkillUI()
	{
		// 更新技能点文本
		if (pointsText != null && skillTreeManager != null)
			pointsText.text = string.Format(pointsTextFormat, skillTreeManager.AvailablePoints);

		// 刷新所有技能槽位
		foreach (SkillSlot skillSlot in skillSlotArray)
		{
			if (skillSlot != null)
				skillSlot.Refresh();
		}
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (string.IsNullOrWhiteSpace(pointsTextFormat) || !pointsTextFormat.Contains("{0}"))
			pointsTextFormat = "技能点: {0}";
	}
#endif
}
