# zl.1.18 Memory Backup

## 任务信息

- **任务号**: zl.1.18
- **任务名称**: 泛型实体建模支持
- **责任人**: 郑琳
- **任务类型**: 功能开发
- **创建时间**: 2026-05-29
- **最后更新**: 2026-05-29 15:25

## 核心内容备份

### 接口修改

需要在 `IEntityModel` 接口添加：

```csharp
IEntityModel GetGenericModelDefinition();
IReadOnlyList<IEntityModel> GetGenericModels();
IEntityModel MakeGenericModel(params IEntityModel[] models);
```

### 源生成器修改

`ModelTableSourceGenerator` 需要：
1. 检测泛型实体类
2. 生成泛型定义模型类
3. 生成已构造泛型实体模型类
4. 为生成的实体模型添加泛型方法

### 与 zl.1.17 的区别

- zl.1.17 针对 `IModel` 接口
- zl.1.18 针对 `IEntityModel` 接口
- 返回类型不同：`IModel` vs `IEntityModel`
- zl.1.18 新增预定义泛型实体模型：`ListEntityModel`、`DictionaryEntityModel`

## 已确认决策

| TODO 项 | 确认结果 | 说明 |
|:-------|:--------|:----|
| TODO 1 | **不支持** | 泛型实体模型不支持泛型属性的建模 |
| TODO 2 | **已实现** | 添加 `ListEntityModel`、`DictionaryEntityModel` 等；Name 属性使用 `static readonly Type` 方式；工厂支持 `GetModel<T>()` 获取泛型 |
| TODO 3 | **使用 List 缓存** | 在泛型定义模型中使用 `List<IEntityModel>` 缓存 |

## 关键实现细节

### Name 属性实现规范

```csharp
private static readonly Type _listType = typeof(List<>);
public string Name => _listType.Name;
```

### 工厂泛型支持逻辑

`DefaultEntityModelFactory.GetModel<T>()` 需要：
1. 判断类型是否为泛型
2. 如果为泛型：
   - 获取开放泛型定义类型
   - 根据名称获取对应的泛型定义模型（`ListEntityModel`、`DictionaryEntityModel`）
   - 获取泛型参数对应的模型
   - 调用 `MakeGenericModel` 返回最终泛型对象
3. 如果为非泛型：执行现有常规逻辑

## 预定义泛型实体模型

- `ListEntityModel`：`List<T>` 的泛型定义模型
- `DictionaryEntityModel`：`Dictionary<TKey, TValue>` 的泛型定义模型
- `ConstructedGenericEntityModel`：统一的已构造泛型实体模型类

## 变更历史

| 日期 | 变更内容 |
|:----|:---------|
| 2026-05-29 | 初始版本 |
| 2026-05-29 | 更新已确认决策和关键实现细节 |
