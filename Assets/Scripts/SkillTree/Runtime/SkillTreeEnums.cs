/// <summary>
/// 技能在界面上的状态
/// </summary>
public enum SkillState
{
	/// <summary>
	/// 锁定状态。技能未学习，且前置条件未满
	/// </summary>
	locked,
	/// <summary>
	/// 可学习状态。技能未学习，但所有前置条件已满足，可以升级
	/// </summary>
	available,
	/// <summary>
	/// 已学习状态。技能当前等级大于 0，但还没有达到最大等级
	/// </summary>
	learned,
	/// <summary>
	/// 满级状态。技能当前等级已达到 maxLevel，不能继续升级
	/// </summary>
	maxed
}

/// <summary>
/// 技能升级请求结果
/// </summary>
public enum SkillUpgradeResult
{
	/// <summary>
	/// 升级成功
	/// </summary>
	success,
	/// <summary>
	/// 技能树还没有初始化，不能升级
	/// </summary>
	skillTreeNotInitialized,
	/// <summary>
	/// 技能不存在于当前技能树中，或者传入了 null
	/// </summary>
	skillNotFound,
	/// <summary>
	/// 技能已经达到最大等级
	/// </summary>
	maxLevelReached,
	/// <summary>
	/// 技能的前置条件没有满足
	/// </summary>
	requirementNotMet,
	/// <summary>
	/// 剩余技能点不足以支付本次升级消耗
	/// </summary>
	notEnoughPoints
}

/// <summary>
/// 技能树状态变化类型
/// </summary>
public enum SkillTreeChangeType
{
	/// <summary>
	/// 技能树完成初始化
	/// </summary>
	initialized,
	/// <summary>
	/// 某个技能升级成功
	/// </summary>
	skillUpgraded,
	/// <summary>
	/// 技能点数量发生变化，例如增加或扣除技能点
	/// </summary>
	pointsChanged,
	/// <summary>
	/// 所有技能被重置，并返还技能点
	/// </summary>
	reset,
	/// <summary>
	/// 从存档数据恢复了技能树状态
	/// </summary>
	restored
}
