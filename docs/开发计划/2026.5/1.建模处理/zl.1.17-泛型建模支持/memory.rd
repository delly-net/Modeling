# zl.1.17 需求补充

本文档用于记录任务 zl.1.17 的需求补充信息。

## 需求完善

1. **泛型模型识别**
   - `IsGenericModel` 属性应返回当前模型是否为泛型模型
   - 对于开放泛型定义（如 `List<>`）
     - 必须返回 `true`
     - `IBaseModel` 中添加 `bool IsGenericDefinition`属性，开放定义时，`IsGenericDefinition`为`true`
     - `IBaseModel` 中添加 `int GenericDefinitionCount`属性，存储泛型定义数量，可在执行`MakeGenericModel`函数时校验
   - 对于已构造泛型（如 `List<int>`），应返回 `true`
   - 对于非泛型类型（如 `int`、`string`），应返回 `false`

2. **泛型定义模型获取**
   - `GetGenericModelDefinition()` 函数用于获取泛型的定义模型
   - 对于已构造泛型（如 `List<int>`），应返回其开放泛型定义（如 `List<>`）的模型
   - 对于非泛型类型，应返回自身
   - 对于开放泛型定义，应返回自身

3. **已构造泛型模型列表**
   - `GetGenericModels()` 函数用于获取所有已构造的泛型模型
   - 对于开放泛型定义（如 `List<>`），应返回所有已构造的具体泛型模型列表（如 `List<int>`、`List<string>` 等）
   - 对于已构造泛型（如 `List<int>`），应返回空列表
   - 对于非泛型类型，应返回空列表

4. **需求变更**
   - 使用`IBaseModel`定义会影响`IEntityModel`源生成，需要进行需求变更
     - `bool IsGenericModel`属性保持不变，依旧在`IBaseModel`种定义
       - 源生成代码中加入对`bool IsGenericModel`属性的支持
     - `IBaseModel GetGenericModelDefinition()`函数 变更为 `IModel GetGenericModelDefinition()`函数，定义在`IModel`中
     - `IReadOnlyList<IBaseModel> GetGenericModels()`函数 变更为 `IReadOnlyList<IModel> GetGenericModels()`函数，定义在`IModel`中
     - `IBaseModel MakeGenericModel(params IBaseModel[] models)`函数 变更为 `IModel MakeGenericModel(params IModel[] models)`函数，定义在`IModel`中

## 规范要求

1. **接口定义规范**
   - 所有接口定义使用非空类型，通过条件编译支持不同 .NET 版本
   - 参考 [代码规范.md](../../../docs/开发指南/代码规范.md)

2. **禁止反射约束**
   - 源生成器和运行时代码均禁止使用反射
   - 使用源生成时的 `typeof()` 获取类型信息
   - 使用静态数组维护已构造泛型模型列表

3. **AOT 友好性**
   - 避免运行时类型检查
   - 使用编译时确定的类型信息
   - 使用 `switch case` 替代动态分发

## 待确认清单

1. **泛型定义模型与已构造模型的关系管理方式**：
   - 当前方案：通过静态数组维护已构造模型列表
   - 是否需要支持动态注册？
     - **建议**：使用静态数组，源生成时生成所有已构造模型

2. **泛型模型在工厂中的注册方式**：
   - 当前方案：源生成器自动注册所有泛型模型
   - 是否需要支持手动注册？
     - **建议**：自动注册，减少用户手动操作
       - 加入`IModel MakeGenericModel(params IModel[] models)`函数支持泛型建模创建
       - 添加`ListModel`,`DictionaryModel`等常见的泛型对象
         - `ListModel`的Name属性，取值自`typeof(List<>).Name`，其他泛型参考此例实现
       - `IeModel` 不支持工厂注册，可以用以下方式进行泛型建模创建
       ```
       var listInt32Model = ListModel.Instance.MakeGenericModel(Int32Model.Instance);
       ```

3. **开放泛型类型的 ClassType 表示方式**：
   - 当前方案：使用 `typeof(List<>)`
   - 验证：`typeof(List<>)` 是否在 AOT 场景下可用？
     - **建议**：验证 AOT 兼容性
       - 可以兼容

4. **泛型定义模型的创建时机**：
   - 当前方案：在源生成时自动创建
   - 是否需要为每个泛型类型都创建定义模型？
     - **建议**：仅对项目中实际使用的泛型类型创建定义模型
       - 泛型建模本身不支持源生成

## 参考文件

- [IBaseModel.cs](../../../Delly.Modeling/IBaseModel.cs) - 基础模型接口
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项
- [zl.1.17-泛型建模支持.md](./zl.1.17-泛型建模支持.md) - 功能文档
