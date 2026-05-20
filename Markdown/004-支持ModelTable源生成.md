# 简介

针对**MoTable特性**进行源生成支持

# 规则

- 在Delly.Modeling.Generator子项目修改ModelTableSourceGenerator.cs文件完善源生成支持代码
- 变更**IEntityModel实现类**的**Name属性**赋值逻辑
  - 来源为**MoTable特性**中的**Name属性**
- 添加**IEntityModelProperty实现类**过滤逻辑
  - 判断属性是否包含**MoColumn特性**定义，不包含则不参与源生成
- 添加**IEntityModel实现类**的**GetIndexes函数**取值逻辑
  - 根据**目标类属性**的**MoColumnIndex特性**生成
  - 当**目标类属性**的**MoColumnIndex特性**中的**Name属性**为空时，使用**目标类属性名称**填充**EntityModelIndex类**的name构造入参
  - 当**目标类属性**的**MoColumnIndex特性**中的**Name属性**不为空时，使用**MoColumnIndex特性**中的**Name属性**填充**EntityModelIndex类**的name构造入参
  - 当不同**目标类属性**的**MoColumnIndex特性**中的**Name属性**相同时，合并为一个**EntityModelIndex实例**
  - 兼容**IEntityModelProperty实现类**过滤逻辑，当属性未参与源生成时，**MoColumnIndex特性**不生效
- 变更**IEntityModelProperty实现类**的**Name属性**赋值逻辑
  - 当**MoColumn特性**的**Name属性**为空，使用属性名称
  - 当**MoColumn特性**的**Name属性**不为空，使用**ModelColumn特性**的**Name属性**
- 添加**IEntityModelProperty实现类**的其他属性取值逻辑
  - 与**MoColumn特性**同名属性可直接取值
  - **IsPrimaryKey属性**：来源为**MoColumn特性**中的**Key属性**
  - **IsAutoIncrement属性**：来源为**MoColumn特性**中的**AutoIncrement属性**