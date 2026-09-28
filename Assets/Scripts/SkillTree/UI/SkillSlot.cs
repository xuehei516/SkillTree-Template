using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Header("技能数据")]
	[Tooltip("该槽位负责显示的技能")]
	[SerializeField] private SkillSO skillSO;

	[Header("渲染引用")]
	[SerializeField] private Image skillIcon;
	[SerializeField] private TMP_Text skillNameText;
	[SerializeField] private Image skillDescriptionImage;
	[SerializeField] private TMP_Text skillLevelText;
	[SerializeField] private TMP_Text skillCostText;

	[Header("状态颜色")]
	[SerializeField] private Color lockedColor = Color.gray;
	[SerializeField] private Color availableColor = Color.white;
	[SerializeField] private Color learnedColor = new Color(0.45f, 0.85f, 1f, 1f);
	[SerializeField] private Color maxedColor = new Color(1f, 0.82f, 0.25f, 1f);

	private Button skillButton;
	/// <summary>
	/// 负责管理技能树逻辑的 SkillTreeManager 引用
	/// </summary>
	private SkillTreeManager skillTreeManager;

	public SkillSO SkillSO => skillSO;

	private void Awake()
	{
		skillButton = GetComponent<Button>();
	}

	private void OnEnable()
	{
		skillButton.onClick.AddListener(TryUpgradeSkill);
	}

	private void OnDisable()
	{
		skillButton.onClick.RemoveListener(TryUpgradeSkill);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (skillSO != null && skillDescriptionImage != null)
		{
			skillDescriptionImage.gameObject.SetActive(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (skillSO != null && skillDescriptionImage != null)
		{
			skillDescriptionImage.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 由 SkillTreeUI 注入逻辑层引用，槽位自身不保存任何技能运行时状态。
	/// </summary>
	public void Initialize(SkillTreeManager newSkillTreeManager)
	{
		skillTreeManager = newSkillTreeManager;
		Refresh();
	}

	/// <summary>
	/// 将点击意图转发给逻辑层，由按钮点击时调用。槽位自身不保存任何技能运行时状态
	/// </summary>
	public void TryUpgradeSkill()
	{
		if (skillTreeManager != null && skillSO != null)
		{
			skillTreeManager.TryUpgradeSkill(skillSO);
		}
	}

	/// <summary>
	/// 从管理器读取信息并刷新显示，不在此处修改等级或技能点
	/// </summary>
	public void Refresh()
	{
		if (skillSO == null)
		{
			SetEmptyView();
			return;
		}

		// 更新技能图标、名称和描述
		if (skillIcon != null)
			skillIcon.sprite = skillSO.skillIcon;

		if (skillNameText != null)
			skillNameText.text = skillSO.skillName;

		if (skillDescriptionImage != null)
		{
			skillDescriptionImage.GetComponentInChildren<TMP_Text>().text = skillSO.skillDescription;
		}

		if (skillTreeManager == null || !skillTreeManager.IsInitialized)
		{
			SetWaitingView();
			return;
		}

		// 从管理器获取技能信息，包括当前等级、状态和升级结果
		int currentLevel = skillTreeManager.GetSkillLevel(skillSO);
		SkillState skillState = skillTreeManager.GetSkillState(skillSO);
		SkillUpgradeResult skillUpgradeResult = skillTreeManager.GetSkillUpgradeResult(skillSO);

		if (skillLevelText != null)
			skillLevelText.text = $"{currentLevel}/{skillSO.maxLevel}";

		if (skillCostText != null)
		{
			skillCostText.text = currentLevel >= skillSO.maxLevel
				? "MAX"
				: skillSO.GetPointCost(currentLevel + 1).ToString();
		}

		skillButton.interactable = skillUpgradeResult == SkillUpgradeResult.success;

		// 根据技能状态设置图标颜色
		SetStateColor(skillState);
	}

	/// <summary>
	/// 设置技能图标的颜色以反映技能状态
	/// </summary>
	/// <param name="skillState"></param>
	private void SetStateColor(SkillState skillState)
	{
		if (skillIcon == null)
			return;

		switch (skillState)
		{
			case SkillState.available:
				//print($"{this.gameObject.name} Skill is available");
				skillIcon.color = availableColor;
				break;
			case SkillState.learned:
				//print($"{this.gameObject.name} Skill is learned");
				skillIcon.color = learnedColor;
				break;
			case SkillState.maxed:
				//print($"{this.gameObject.name} Skill is maxed");
				skillIcon.color = maxedColor;
				break;
			default:
				//print($"{this.gameObject.name} Skill is locked");
				skillIcon.color = lockedColor;
				break;
		}
	}

	/// <summary>
	/// 设置槽位为缺失技能的显示状态，用于 SkillSO 为 null 的情况
	/// </summary>
	private void SetEmptyView()
	{
		skillButton.interactable = false;

		if (skillNameText != null)
			skillNameText.text = "Missing Skill";

		if (skillLevelText != null)
			skillLevelText.text = "-";
	}

	/// <summary>
	/// 设置槽位为等待技能树初始化的显示状态，用于 SkillTreeManager 未初始化的情况
	/// </summary>
	private void SetWaitingView()
	{
		skillButton.interactable = false;

		if (skillLevelText != null)
			skillLevelText.text = "-";

		if (skillCostText != null)
			skillCostText.text = string.Empty;

		if (skillIcon != null)
			skillIcon.color = lockedColor;
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (skillIcon != null && skillSO != null)
			skillIcon.sprite = skillSO.skillIcon;

		if (skillNameText != null && skillSO != null)
			skillNameText.text = skillSO.skillName;

		if (skillDescriptionImage != null && skillSO != null)
			skillDescriptionImage.GetComponentInChildren<TMP_Text>().text = skillSO.skillDescription;
	}
#endif
}
