# 技能树模板使用说明

这个模板沿用 Dungeon Gunner 的组件协作方式：静态配置放在 ScriptableObject，运行时逻辑组件只维护规则和状态，表现组件通过事件订阅刷新。

## 分层

- `SkillSO`：单个技能的静态数据、升级消耗和前置条件。
- `SkillTreeSO`：一棵技能树包含的技能清单与初始技能点。
- `SkillTreeManager`：唯一可以修改等级与技能点的逻辑层。
- `SkillTreeEvent`：逻辑层向外发布变化和失败事件。
- `SkillTreeUI`：订阅变化事件，刷新技能点和全部槽位。
- `SkillSlot`：只显示一个技能并把按钮点击转发给逻辑层。
- `SkillTreeSaveData`：与具体存档框架无关的数据快照。

## 最小接入步骤

1. 通过 `Create > Scriptable Objects > Skill Tree > Skill Details` 创建技能资产。
2. 在后置技能的 `skillRequirementList` 中配置前置技能和所需等级。
3. 通过 `Create > Scriptable Objects > Skill Tree > Skill Tree Details` 创建技能树资产，并把全部技能加入 `skillList`。
4. 在场景逻辑对象上添加 `SkillTreeManager`。`SkillTreeEvent` 会由依赖特性自动添加。
5. 在 UI 根对象添加 `SkillTreeUI`，指定 `SkillTreeManager` 和技能点文本。
6. 每个按钮添加 `SkillSlot`，指定它显示的 `SkillSO` 和需要更新的 UI 引用。

## 复用约定

- 需要奖励技能点时，只调用 `SkillTreeManager.AddSkillPoints`。
- 需要查询技能效果时，调用 `GetSkillLevel`，不要读取 UI。
- 表现扩展订阅 `SkillTreeEvent`，不要在表现脚本中修改等级或技能点。
- 存档系统序列化 `CaptureSaveData()` 的返回值，读档后调用 `RestoreSaveData()`。
- 逐级消耗、职业限制等项目特有规则，可以继承 `SkillSO` 并重写 `GetPointCost`，或在 `SkillTreeManager` 的规则入口集中扩展。
