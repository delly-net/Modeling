# zl.1.18 需求补充

## 需求完善

### 与 zl.1.17 的职责划分

- zl.1.17 专注于 `IModel` 接口的泛型支持，适用于所有 `[Modelable]` 标记的类
- zl.1.18 专注于 `IEntityModel` 接口的泛型支持，适用于 `[MoTable]` 和 `[MoQuery]` 标记的实体类
- 两者关系：`IEntityModel` 继承自 `IBaseModel`，所以 `IsGenericModel` 等属性由 `IBaseModel` 提供，而泛型方法分别定义在各自的接口中

### 源生成器职责

- `ModelTableSourceGenerator` 需要为泛型实体类生成额外的泛型定义模型类
- 泛型定义模型类命名规范：`{ClassName}GenericModel`，如 `TableEntityGenericModel`
- 已构造泛型实体模型类使用统一的 `ConstructedGenericEntityModel` 类

## 规范要求

### 代码规范

- 遵循 [代码规范.md](../../../docs/开发指南/代码规范.md)
- 公共成员必须有 XML 文档注释
- 私有成员使用 `//` 注释
- 使用条件编译支持不同 .NET 版本

### 命名规范

- 泛型定义模型类：`{ClassName}GenericModel`
- 已构造泛型实体模型类：`ConstructedGenericEntityModel`
- 模型实例属性：`Instance`
- 预定义泛型实体模型类：`ListEntityModel`、`DictionaryEntityModel` 等

### AOT 友好性

- 不允许使用反射
- 泛型参数数量校验在编译时确定
- 使用 `Array.Empty<T>()` 而非 `new T[0]`
- Name 属性使用 `static readonly Type` 字段方式

## 已确认决策

| TODO 项 | 确认结果 | 说明 |
|:-------|:--------|:----|
| TODO 1 | **不支持** | 泛型实体模型不支持泛型属性的建模 |
| TODO 2 | **已实现** | 添加 `ListEntityModel`、`DictionaryEntityModel` 等常见泛型对象；Name 属性使用 `static readonly Type` 方式；`IEntityModel` 泛型支持在 `DefaultEntityModelFactory` 中通过 `GetModel<T>()` 获取 |
| TODO 3 | **使用 List 缓存** | 在泛型定义模型中使用 `List<IEntityModel>` 缓存已构造的泛型实体模型 |

## 工厂泛型支持实现细节

`DefaultEntityModelFactory.GetModel<T>()` 需要支持泛型类型：

```csharp
public IEntityModel GetModel<T>() where T : class
{
    var type = typeof(T);

    // 判断是否为泛型类型
    if (type.IsGenericType)
    {
        // 获取开放泛型定义类型
        var genericDefinition = type.GetGenericTypeDefinition();

        // 根据开放泛型定义类型名称获取对应的泛型定义模型
        var definitionModel = genericDefinition.Name switch
        {
            "List`1" => ListEntityModel.Instance,
            "Dictionary`2" => DictionaryEntityModel.Instance,
            _ => throw new NotSupportedException($"不支持的泛型类型: {genericDefinition.Name}")
        };

        // 获取泛型参数类型对应的模型
        var genericArguments = type.GetGenericArguments();
        var argumentModels = new IEntityModel[genericArguments.Length];

        for (int i = 0; i < genericArguments.Length; i++)
        {
            argumentModels[i] = GetModelByType(genericArguments[i]);
        }

        // 通过泛型定义模型创建已构造的泛型实体模型
        return definitionModel.MakeGenericModel(argumentModels);
    }

    // 非泛型类型，执行现有常规逻辑
    // ...
}
```

## 参考文件

- [zl.1.17-泛型建模支持.md](./zl.1.17-泛型建模支持.md) - 泛型建模支持参考文档
- [IEntityModel.cs](../../../Delly.Modeling/IEntityModel.cs) - 实体模型接口
- [DefaultEntityModelFactory.cs](../../../Delly.Modeling/DefaultEntityModelFactory.cs) - 实体模型工厂
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项

## 变更历史

| 日期 | 变更内容 |
|:----|:---------|
| 2026-05-29 | 初始版本，包含泛型实体建模支持的核心需求 |
| 2026-05-29 | 更新 TODO 2 实现细节：添加常见泛型对象、工厂支持逻辑 |
| 2026-05-29 | 确认所有 TODO 项，更新需求补充文档 |
